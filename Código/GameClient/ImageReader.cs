using System;
using System.Collections.Generic;
using System.IO;

namespace GameClient
{
    public class ImageReader
    {
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static string GetBase64Image(string path)
        {
            FileInfo fileInfo = new FileInfo(path);

            byte[] data = new byte[fileInfo.Length];

            using (FileStream fs = fileInfo.OpenRead())
            {
                fs.Read(data, 0, data.Length);
            }
            return Convert.ToBase64String(data);
        }
    }
}
