using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Structures
{
    class HuffmanNode<T> where T : IProbability, new()
    {
        public T Value;
        public string Code;
        public HuffmanNode<T> Father;
        public HuffmanNode<T> Rightson;
        public HuffmanNode<T> Leftson;
 

        public HuffmanNode(T value)
        {
            Value = value;
        }
    }
}
