using System.Collections.Generic;
using System.IO;
using System.Net;

namespace UniversalGameLauncher
{
    class CSV
    {
        public static List<HashFiles> hashFiles = new List<HashFiles>();
        private static readonly string MyCSV = Constants.GAME_CSV_PATH;
        public static void LoadCSV()
        {
            if (File.Exists(MyCSV))
            {
                using (StreamReader sr = new StreamReader(MyCSV))
                {
                    while (!sr.EndOfStream)
                    {
                        HashFiles file;

                        string[] rows = sr.ReadLine().Split(',');
                        file.filename = rows[0];
                        file.sha256 = rows[1];
                        hashFiles.Add(file);
                    }

                }

            }
        }
        public struct HashFiles
        {
            public string filename;
            public string sha256;
        }
        
    }
}
