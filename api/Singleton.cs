using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using ClassLibrary.Structures;

namespace api
{
    public class Singleton
    {
        private static Singleton _instance = null;

        public static Singleton Instance
        {
            get
            {
                if (_instance == null) _instance = new Singleton();
                return _instance;
            }
        }
        public Huffman<HuffmanCharacter> HuffmanTree;
        public List<HuffmanCompressions> HistoryList = new List<HuffmanCompressions>();
    }
}
