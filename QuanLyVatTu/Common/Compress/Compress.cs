using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Compress
{
    public class CompressData
    {
        public int CompressZip(string sPath, string strDest)
        {
            try
            {
                ZipOutputStream zipOut = new ZipOutputStream(File.Create(@strDest));
                foreach (string fName in Directory.GetFiles(sPath))
                {
                    FileInfo fi = new FileInfo(fName);
                    ZipEntry entry = new ZipEntry(fi.Name);
                    FileStream sReader = File.OpenRead(fName);
                    byte[] buff = new byte[Convert.ToInt32(sReader.Length)];
                    sReader.Read(buff, 0, (int)sReader.Length);
                    entry.DateTime = fi.LastWriteTime;
                    entry.Size = sReader.Length;
                    sReader.Close();
                    zipOut.PutNextEntry(entry);
                    zipOut.Write(buff, 0, buff.Length);
                }
                zipOut.Finish();
                zipOut.Close();
            }
            catch (Exception)
            {
                return -1;
            }
            return 0;

        }

        public int UncompressZip(string sFile, string strDest)
        {
            try
            {
                ZipInputStream zipIn = new ZipInputStream(File.OpenRead(sFile));
                ZipEntry entry;
                while ((entry = zipIn.GetNextEntry()) != null)
                {
                    FileStream streamWriter = File.Create(@strDest + "\\" + entry.Name);
                    long size = entry.Size;
                    byte[] data = new byte[size];
                    while (true)
                    {
                        size = zipIn.Read(data, 0, data.Length);
                        if (size > 0) streamWriter.Write(data, 0, (int)size);
                        else break;
                    }
                    streamWriter.Close();
                }
                Console.WriteLine("Done!!");
            }
            catch (Exception)
            {
                return -1;
            }
            return 0;
        }
    }
}
