using Common.Compress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Database
{
    public class ExportData
    {
        #region Static variable
        private static String[,] DBTypeConversionKey = new String[,] 
        {
             {"BigInt","System.Int64"},
             {"Binary","System.Byte[]"},
             {"Bit","System.Boolean"},
             //{"Char","System.String"},
             {"DateTime","System.DateTime"},
             {"Decimal","System.Decimal"},
             {"Float","System.Double"},
             {"Image","System.Byte[]"},
             {"Int","System.Int32"},
             //{"Money","System.Decimal"},
             //{"NChar","System.String"},
             //{"NText","System.String"},
             {"NVarChar","System.String"},
             {"Real","System.Single"},
             //{"SmallDateTime","System.DateTime"},
             {"SmallInt","System.Int16"},
             //{"SmallMoney","System.Decimal"},
             //{"Text","System.String"},
             //{"Timestamp","System.DateTime"},
             {"TinyInt","System.Byte"},
             //{"UniqueIdentifer","System.Guid"},
             //{"VarBinary","System.Byte[]"},
             //{"VarChar","System.String"},
             {"Variant","System.Object"},
             {"UniqueIdentifier","System.Guid"} 
        };
        #endregion

        public int Export(string strTable, string strTkXMLFile, string strTkXSDFile)
        {
            FileInfo fileInfoTkXMLFile = new FileInfo(strTkXMLFile);
            FileInfo fileInfoTkXSDFile = new FileInfo(strTkXSDFile);
            if ((fileInfoTkXMLFile.Exists) || (fileInfoTkXSDFile.Exists))
            {
                return -1;
            }
            Common.Database.DBManager dbmanager = new Common.Database.DBManager();
            try
            {                
                SqlDataAdapter adapter = new SqlDataAdapter("select * from " + strTable, dbmanager.Sqlconnection);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                if (ds.Tables[0].Rows.Count <= 0)
                {
                    dbmanager.Close();
                    return 1;
                }
                ds.WriteXml(strTkXMLFile);
                ds.WriteXmlSchema(strTkXSDFile);
                dbmanager.Close();
            }
            catch (Exception)
            {
                dbmanager.Close();
                return -2;
            }
            return 0;
        }

        public int Import(string _TableName, string _key_table, string strXMLFile, string strXSDFile)
        {
            ExportData objExport = new ExportData();
            CompressData objCompress = new CompressData();

            FileInfo fileInfoXMLFile = new FileInfo(strXMLFile);
            FileInfo fileInfoXSDFile = new FileInfo(strXSDFile);
            if ((!fileInfoXMLFile.Exists) || (!fileInfoXSDFile.Exists))
            {
                return -1;
            }
            Common.Database.DBManager dbmanager = new Common.Database.DBManager();
            try
            {
                if (ImportFromXML(_TableName, _key_table, strXMLFile, strXSDFile, dbmanager.Sqlconnection) < 0)
                {
                    dbmanager.Close();
                    return -2;
                }                
                    SqlCommand cmd = new SqlCommand(_TableName + "_Convert", dbmanager.Sqlconnection);                    
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                    dbmanager.Close();
                
            }
            catch (Exception ex)
            {
                dbmanager.Close();
                return -2;
            }
            return 0;
        }

        public int ImportFromXML(string strTable, string _key_table, string strXMLFile, string strXSDFile, SqlConnection con)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXmlSchema(strXSDFile);
                ds.ReadXml(strXMLFile);
                SqlCommand cmdInsertCommand = new SqlCommand();
                cmdInsertCommand.CommandText = "(";
                for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                {
                    if (i == ds.Tables[0].Columns.Count - 1)
                        cmdInsertCommand.CommandText += ds.Tables[0].Columns[i].ColumnName;
                    else
                        cmdInsertCommand.CommandText += ds.Tables[0].Columns[i].ColumnName + ",";
                }
                cmdInsertCommand.CommandText += ") values(";
                for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                {
                    if (i == ds.Tables[0].Columns.Count - 1)
                        cmdInsertCommand.CommandText += "@" + ds.Tables[0].Columns[i].ColumnName;
                    else
                        cmdInsertCommand.CommandText += "@" + ds.Tables[0].Columns[i].ColumnName + ",";
                }
                cmdInsertCommand.CommandText += ")";
                cmdInsertCommand.CommandText = "insert into " + strTable + cmdInsertCommand.CommandText;
                cmdInsertCommand.CommandType = CommandType.Text;
                SqlDbType dbType;
                for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                {
                    dbType = SystemTypeToDbType(ds.Tables[0].Columns[i].DataType);
                    cmdInsertCommand.Parameters.Add("@" + ds.Tables[0].Columns[i].ColumnName, dbType, ds.Tables[0].Columns[i].MaxLength);
                }

                cmdInsertCommand.Connection = con;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    for (int j = 0; j < cmdInsertCommand.Parameters.Count; j++)
                    {
                        cmdInsertCommand.Parameters[j].Value = ds.Tables[0].Rows[i][j];
                    }

                    cmdInsertCommand.Parameters["@" + _key_table].Value = Guid.NewGuid();                 
                    cmdInsertCommand.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }

        public SqlDbType SystemTypeToDbType(System.Type sourceType)
        {
            SqlDbType result;
            String SystemType = sourceType.ToString();
            String DBType = String.Empty;
            int keyCount = DBTypeConversionKey.GetLength(0);

            for (int i = 0; i < keyCount; i++)
            {
                if (DBTypeConversionKey[i, 1].Equals(SystemType))
                    DBType = DBTypeConversionKey[i, 0];
            }

            if (DBType == String.Empty) DBType = "Variant";

            result = (SqlDbType)Enum.Parse(typeof(SqlDbType), DBType);

            return result;
        }

        public int DeleteTableData(string _TableName, Guid _donvi_id, SqlConnection con)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "delete from " + _TableName + " where donvi_id= '" + _donvi_id.ToString() + "'";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return -1;
            }
            return 0;


        }

        /// <summary>
        /// Check table exist data 
        /// </summary>
        /// <param name="_TableName"></param>
        /// <param name="_donviID"></param>
        /// <returns></returns>
        public Boolean CheckExistData(String _TableName, Guid _donviID, SqlConnection con)
        {
            //check whether dmtk had data of bqlda_id or not
            SqlDataAdapter adapter = new SqlDataAdapter("select * from " + _TableName + " where donvi_id= '" +
                _donviID.ToString() + "'",
                 con);
            DataSet ds = new DataSet();
            adapter.Fill(ds);

            if (ds.Tables[0].Rows.Count > 0) return true;
            else return false;
        }
    }
}
