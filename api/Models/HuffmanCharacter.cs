using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibrary.Structures;

namespace api.Models
{
    public class HuffmanCharacter : IProbability
    {
        int Frequency;
        double Probability;
        byte Value;

        public HuffmanCharacter() { }

        public void AddFrecuency(int num)
        {
            Frequency += num;
        }

        public void CalculateProbability(double totalBytes)
        {
            Probability = Convert.ToDouble(Frequency) / totalBytes;
        }

        public int GetFrequency()
        {
            return Frequency;
        }

        public double GetProbability()
        {
            return Probability;
        }

        public byte GetValue()
        {
            return Value;
        }

        public void SetByte(byte value)
        {
            Value = value;
        }

        public void SetProbability(double number)
        {
            Probability = number;
        }
    }
}
