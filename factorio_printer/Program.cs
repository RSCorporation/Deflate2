using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
namespace FactorioPrinter
{
    class Program
    {
        public static void Main()
        {
            string tree = File.ReadAllText("tree.txt");
            Deflate2Tree tr = new Deflate2Tree(tree);
            Deflate2.CompressAPI(File.ReadAllText("json.txt"), tr);
        }
    }
}