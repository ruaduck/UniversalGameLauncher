using System;
using System.IO;

namespace VandVHashUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
        private static void OutputToCSV()
        {
            string newFileName = $@"{Application.StartupPath}\Hashes.csv";
            foreach (var tile in bannedtile)
            {
                string mytext = $"{tile.Color},{tile.Tile},{tile.X},{tile.Y},{tile.Z}{Environment.NewLine}";
                File.AppendAllText(newFileName, mytext);
            }
        }
        public struct MyHash
        {

            public string File;
            public HashCode X;
            public MD5CryptoServiceProvider Y;


        }

    }
}
