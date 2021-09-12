using System;
using System.Collections.Generic;
using System.Text;

namespace CustomGenerics.Structures
{
    internal class PQNode<T> : ICloneable
    {
        public PQNode<T> Father;
        public PQNode<T> RightSon;
        public PQNode<T> LeftSon;
        public T Value;
        public double Priority; 

        public PQNode() { }

        public PQNode(T value, double priority)
        {
            Value = value;
            Priority = priority;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
