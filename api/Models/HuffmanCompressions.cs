﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.Json;


namespace api.Models
{
    public class HuffmanCompressions
    {
        public string OriginalName { get; set; }
        public string CompressedName { get; set; }
        public string CompressedFilePath { get; set; }
        public double CompressionRatio { get; set; }
        public double CompressionFactor { get; set; }
        public double ReductionPercentage { get; set; }

        public HuffmanCompressions() { }

        public void SetAttributes(string path, string prevName, string newName)
        {
            var CountBytesO = System.IO.File.ReadAllBytes($"{path}/{prevName}");
            var FileP = $"{path}/{newName}";
            double BNO = CountBytesO.Count();
            var CountBytesC = System.IO.File.ReadAllBytes($"{path}/{newName}");
            double BNC = CountBytesC.Count();

            OriginalName = prevName;
            CompressedName = newName;
            CompressedFilePath = FileP;
            GetRatio(BNC, BNO);
            GetFactor(BNC, BNO);
            RPercentage();

            LoadHistList(path);
            Singleton.Instance.HistoryList.Add(this);
            var file = new FileStream($"{path}/CompressionHist", FileMode.OpenOrCreate);
            var BytesToWrite = JsonSerializer.Serialize(Singleton.Instance.HistoryList, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var writer = new StreamWriter(file);
            writer.WriteLine(BytesToWrite);
            writer.Close();
            file.Close();
        }

        public static void LoadHistList(string path)
        {
            var file = new FileStream($"{path}/CompressionHist", FileMode.OpenOrCreate);
            if (file.Length != 0)
            {
                Singleton.Instance.HistoryList.Clear();
                using var reader = new StreamReader(file);
                var content = reader.ReadToEnd();
                var list = JsonSerializer.Deserialize<List<HuffmanCompressions>>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                foreach (var item in list)
                {
                    Singleton.Instance.HistoryList.Add(item);
                }
            }
            file.Close();
        }

        public void GetRatio(double bNC, double bNO)
        {
            CompressionRatio = Math.Round(bNC / bNO, 4);
        }

        public void GetFactor(double NumBC, double NumBO)
        {
            CompressionFactor = Math.Round(NumBO / NumBC, 3);
        }

        public void RPercentage()
        {
            ReductionPercentage = Math.Round(CompressionRatio * 100, 5);
        }
    }
}
