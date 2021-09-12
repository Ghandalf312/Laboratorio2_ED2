using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Structures
{
    public interface IProbability
    {
        double GetProbability();
        int GetFrequency();
        byte GetValue();
        void AddFrecuency(int number);
        void SetByte(byte value);
        void CalculateProbability(double totalBytes);
        void SetProbability(double number);
    }
}
