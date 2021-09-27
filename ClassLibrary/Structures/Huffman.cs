using ClassLibrary.Utilities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClassLibrary.Structures
{
    public class Huffman<T> : ICompressor where T : IProbability, new()
    {
        HuffmanNode<T> Root;
        public int NextId = 1;
        string FilePath;
        Dictionary<byte, HuffmanNode<T>> BytesDictionary;
        PriorityQueue<HuffmanNode<T>> priorityQueue; 
        double BytesCount;

        public Huffman(string filePath)
        {
            FilePath = filePath;
        }
        public Huffman()
        {
        }

        public async Task CompressFile(string path, IFormFile file, string name)
        {
            if (System.IO.File.Exists($"{path}/{file.FileName}"))
            {
                System.IO.File.Delete($"{path}/{file.FileName}");
            }

            if (System.IO.File.Exists($"{path}/{name}"))
            {
                System.IO.File.Delete($"{path}/{name}");
            }

            using var saver = new FileStream($"{FilePath}/{file.FileName}", FileMode.OpenOrCreate);
            await file.CopyToAsync(saver);

            using var reader = new BinaryReader(saver);
            int bufferSize = 2000;
            var buffer = new byte[bufferSize];
            BytesDictionary = new Dictionary<byte, HuffmanNode<T>>();
            saver.Position = saver.Seek(0, SeekOrigin.Begin);
            BytesCount = 0.00;
            while (saver.Position != saver.Length)
            {
                buffer = reader.ReadBytes(bufferSize);
                T value = new T();
                HuffmanNode<T> Node;
                foreach (var byteData in buffer)
                {
                    if (!BytesDictionary.ContainsKey(byteData))
                    {
                        value.SetByte(byteData);
                        Node = new HuffmanNode<T>(value);
                        BytesDictionary.Add(byteData, Node);
                    }
                    BytesDictionary[byteData].Value.AddFrecuency(1);
                    BytesCount++;
                    value = new T();
                }
            }

            BuildHuffmanTree();              
            SetCode(Root, "");

          
            var newFile = new FileStream($"{FilePath}/{name}", FileMode.OpenOrCreate);
            var writer = new StreamWriter(newFile);
            string Metadata = "";
            Metadata += StringFromBytes((GetBytesFromInt(BytesDictionary.Values.Count, 1))); 
            int maxValue = BytesDictionary.Values.Max(x => x.Value.GetFrequency());
            var intBytes = ConvertToBinary(maxValue).Length / 8;
            Metadata += StringFromBytes((GetBytesFromInt(intBytes, 1)));
            writer.Write(Metadata);
            foreach (var byteObject in BytesDictionary.Values)
            {
                Metadata = (char)byteObject.Value.GetValue() + StringFromBytes((GetBytesFromInt(byteObject.Value.GetFrequency(), intBytes)));
                writer.Write(Metadata);
            }
    

            saver.Position = saver.Seek(0, SeekOrigin.Begin);
            string FinalText = "";
            while (saver.Position != saver.Length)
            {
                buffer = reader.ReadBytes(bufferSize);
                foreach (var byteData in buffer)
                {
                    FinalText += BytesDictionary[byteData].Code;
                    if (FinalText.Length >= 8)
                    {
                        writer.Write((char)Convert.ToByte(FinalText.Substring(0, 8), 2));
                        FinalText = FinalText.Remove(0, 8);
                    }
                }
            }
            if (FinalText.Length != 0)
            {
                while (FinalText.Length % 8 != 0)
                {
                    FinalText += "0";
                }
                writer.Write((char)Convert.ToByte(FinalText, 2));
            }
            saver.Close();
            writer.Close();
            newFile.Close();
        }

        public async Task DecompressFile(string path, IFormFile file, string name)
        {
            using var saver = new FileStream($"{FilePath}/{file.FileName}", FileMode.OpenOrCreate);
            await file.CopyToAsync(saver);

            using var reader = new StreamReader(saver);
            saver.Seek(0, SeekOrigin.Begin);
            var text = reader.ReadToEnd();
            BytesDictionary = new Dictionary<byte, HuffmanNode<T>>();
            saver.Position = saver.Seek(0, SeekOrigin.Begin);

            int differentByteQty = text.Substring(0, 1).ToCharArray()[0];
            text = text.Remove(0, 1);
            int frequencyLength = text.Substring(0, 1).ToCharArray()[0];
            text = text.Remove(0, 1);
            T value = new T();
            HuffmanNode<T> node;
            var textPortion = "";
            BytesCount = 0.00;
            for (int i = 0; i < differentByteQty; i++)
            {
                textPortion = text.Substring(0, frequencyLength + 1);
                value.SetByte((byte)textPortion.Substring(0,1).ToCharArray()[0]);
                value.AddFrecuency(GetIntFromText(textPortion.Remove(0,1)));
                BytesCount += value.GetFrequency();
                node = new HuffmanNode<T>(value);
                BytesDictionary.Add(value.GetValue(), node);
                value = new T();
                text = text.Remove(0, frequencyLength + 1);
            }

            saver.Close();
            BuildHuffmanTree();
            SetCode(Root, "");
            var binaryText = "";
            binaryText += ConvertFromBytesToBinary(ByteGenerator.ConvertToBytes(text));

            var code = "";
            var finalText = "";
            int codeLength = 0;
            if (System.IO.File.Exists($"{FilePath}/{name}"))
            {
                System.IO.File.Delete($"{FilePath}/{name}");
            }
            var newFile = new FileStream($"{FilePath}/{name}", FileMode.OpenOrCreate);
            var writer = new StreamWriter(newFile);
            while (binaryText.Length > 8)
            {
                foreach (var data in BytesDictionary.Values)
                {
                    if (data.Code == code)
                    {
                        finalText += (char)BytesDictionary.First(x => x.Value.Code == code).Key;
                        if (finalText.Length >= 8)
                        {
                            writer.Write(finalText.Substring(0, 8));
                            finalText = finalText.Remove(0, 8);
                        }
                        code = "";
                        binaryText = binaryText.Remove(0, codeLength);
                        codeLength = 0;
                    }
                }
                if (finalText.Length != BytesCount)
                {
                    codeLength++;
                    code = binaryText.Substring(0, codeLength);
                }
            }
            writer.Close();
            newFile.Close();
        }

        public string CompressText(string text)
        {
            int bufferSize = 2000000;
            var buffer = new byte[bufferSize];
            BytesDictionary = new Dictionary<byte, HuffmanNode<T>>();
            buffer = ByteGenerator.ConvertToBytes(text);
            T value = new T();
            HuffmanNode<T> Node;
            BytesCount = 0.00;
            foreach (var byteData in buffer)
            {
                if (!BytesDictionary.ContainsKey(byteData))
                {
                    value.SetByte(byteData);
                    Node = new HuffmanNode<T>(value);
                    BytesDictionary.Add(byteData, Node);
                }
                BytesDictionary[byteData].Value.AddFrecuency(1);
                BytesCount += 1;
                value = new T();
            }
            BuildHuffmanTree();
            SetCode(Root, "");
            string FinalText = "";
            string Metadata = "";
            Metadata += ByteGenerator.ConvertToString(GetBytesFromInt(BytesDictionary.Values.Count, 1)); 
            int maxValue = BytesDictionary.Values.Max(x => x.Value.GetFrequency());
            var intBytes = ConvertToBinary(maxValue).Length / 8;
            Metadata += ByteGenerator.ConvertToString(GetBytesFromInt(intBytes, 1));
            foreach (var byteObject in BytesDictionary.Values)
            {
                Metadata += ByteGenerator.ConvertToString(new byte[] { byteObject.Value.GetValue() }) + ByteGenerator.ConvertToString(GetBytesFromInt(byteObject.Value.GetFrequency(), intBytes));
            }
            foreach (var byteData in buffer)
            {
                FinalText += BytesDictionary[byteData].Code;
            }
            while (FinalText.Length % 8 != 0)
            {
                FinalText += "0";
            }

            var stringBytes = (from Match m in Regex.Matches(FinalText, @"\d{8}") select m.Value).ToList();
            FinalText = "";
            byte[] bytes = new byte[1];
            foreach (var byteData in stringBytes)
            {
                bytes[0] = Convert.ToByte(byteData, 2);
                FinalText += ByteGenerator.ConvertToString(bytes);
            }
            string savingText = Metadata + FinalText;
            return savingText;
        }

        public string DecompressText(string Text)
        {
            int bufferSize = 2000000;
            var buffer = new byte[bufferSize];
            BytesDictionary = new Dictionary<byte, HuffmanNode<T>>();

            
            buffer = ByteGenerator.ConvertToBytes(Text);
            int differentByteQty = buffer[0];
            int frequencyLength = buffer[1];
            T value = new T();
            HuffmanNode<T> node;
            BytesCount = 0.00;
            for (int i = 0; i < differentByteQty; i++)
            {
                buffer = ByteGenerator.ConvertToBytes(Text.Substring(2+(i*(frequencyLength + 1)),frequencyLength + 1));
                value.SetByte(buffer[0]);
                value.AddFrecuency(GetIntFromBytes(buffer));
                BytesCount += value.GetFrequency();
                node = new HuffmanNode<T>(value);
                BytesDictionary.Add(buffer[0], node);
                value = new T();
            }

            BuildHuffmanTree();

            SetCode(Root, "");
            var text = "";
            buffer = ByteGenerator.ConvertToBytes(Text.Substring(2+(differentByteQty*(frequencyLength + 1)),Text.Length- (2 + (differentByteQty * (frequencyLength + 1)))));
            text += ConvertFromBytesToBinary(buffer);

            var code = "";
            var finalText = "";
            int codeLength = 0;
            byte[] bytes = new byte[1];
            bool flag = true;
            while (finalText.Length != BytesCount)
            {
                while (flag)
                {
                    foreach (var character in BytesDictionary)
                    {
                        if (character.Value.Code == code)
                        {
                            bytes[0] = character.Key;
                            finalText += ByteGenerator.ConvertToString(bytes);
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        codeLength++;
                        code = text.Substring(0, codeLength);
                    }
                }
                flag = true;
                code = "";
                text = text.Remove(0, codeLength);
                codeLength = 0;
            }
            return finalText;
        }

        private string ConvertToBinary(int number)
        {
            string value = Convert.ToString(number, 2);
            while (value.Length % 8 != 0)
            {
                value = "0" + value;
            }
            return value;
        }

        private string ConvertToFixedBinary(int number, int byteQty)
        {
            string value = Convert.ToString(number, 2);
            while (value.Length % 8 != 0 || value.Length / 8 != byteQty)
            {
                value = "0" + value;
            }
            return value;
        }

        private string ConvertFromBytesToBinary(byte[] array)
        {
            var text = "";
            foreach (var data in array)
            {
                text += ConvertToFixedBinary(data, 1);
            }
            return text;
        }

        private byte[] GetBytesFromInt(int number, int byteQty)
        {
            string text = ConvertToFixedBinary(number, byteQty);
            var stringBytes = (from Match m in Regex.Matches(text, @"\d{8}") select m.Value).ToList();
            byte[] byteArray = new byte[stringBytes.Count];
            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] = Convert.ToByte(stringBytes[i], 2);
            }
            return byteArray;
        }

        private int GetIntFromBytes(byte[] array)
        {
            var result = "";
            for (int i = 1; i < array.Length; i++)
            {
                result += Convert.ToString(array[i], 2);
            }
            return Convert.ToInt32(result, 2);
        }

        private int GetIntFromText(string text)
        {
            List<byte> list = new List<byte>();
            foreach (var character in text)
            {
                list.Add((byte)character);
            }
            return GetIntFromBytesDecompression(list.ToArray());
        }

        private int GetIntFromBytesDecompression(byte[] array)
        {
            var result = "";
            var character = "";
            for (int i = 0; i < array.Length; i++)
            {
                character = Convert.ToString(array[i], 2);
                while (character.Length != 8)
                {
                    character = "0" + character;
                }
                result += character;
            }
            return Convert.ToInt32(result, 2);
        }

        private void SetCode(HuffmanNode<T> node, string prevCode)
        {
            if (node.Father != null)
            {
                if (node.Father.Leftson == node)
                {
                    node.Code = $"{prevCode}0";
                }
                else
                {
                    node.Code = $"{prevCode}1";
                }
                if (BytesDictionary.ContainsKey(node.Value.GetValue()))
                {
                    BytesDictionary[node.Value.GetValue()].Code = node.Code;
                }
            }
            else
            {
                node.Code = "";
            }

            if (node.Leftson != null)
            {
                SetCode(node.Leftson, node.Code);
            }

            if (node.Rightson != null)
            {
                SetCode(node.Rightson, node.Code);
            }
        }

        private void BuildHuffmanTree()
        {
            priorityQueue = new PriorityQueue<HuffmanNode<T>>();

            foreach (var Node in BytesDictionary.Values)
            {
                Node.Value.CalculateProbability(BytesCount);
                priorityQueue.AddValue(Node, Node.Value.GetProbability());
            }

            T NewNodeValue = new T();
            while (priorityQueue.DataNumber != 1)
            {
                var Node1 = priorityQueue.GetFirst();
                var Node2 = priorityQueue.GetFirst();
                NewNodeValue.SetProbability(Node1.Value.GetProbability() + Node2.Value.GetProbability());
                var NewNode = new HuffmanNode<T>(NewNodeValue);
                Node1.Father = NewNode;
                Node2.Father = NewNode;
                if (Node1.Value.GetProbability() < Node2.Value.GetProbability())
                {
                    NewNode.Leftson = Node2;
                    NewNode.Rightson = Node1;
                }
                else
                {
                    NewNode.Rightson = Node2;
                    NewNode.Leftson = Node1;
                }
                priorityQueue.AddValue(NewNode, NewNode.Value.GetProbability());
                Root = NewNode;
                NewNodeValue = new T();
            }
        }

        private string StringFromBytes(byte[] array)
        {
            var result = "";
            foreach (var item in array)
            {
                result += (char)item;
            }
            return result;
        }
    }
}
