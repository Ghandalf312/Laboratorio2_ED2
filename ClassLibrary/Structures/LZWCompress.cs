using System;
using System.Collections.Generic;
using System.Linq;
using ClassLibrary.Utilities;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace ClassLibrary.Structures
{
    public class LZWCompress : ICompressor
    {
        Dictionary<string, int> LZWTable = new Dictionary<string, int>();
        Dictionary<int, List<byte>> DecompressLZWTable = new Dictionary<int, List<byte>>();
        List<byte> Differentchar = new List<byte>();
        List<byte> Characters = new List<byte>();
        List<int> NumbersToWrite = new List<int>();
        List<List<byte>> DecompressValues = new List<List<byte>>();
        int MaxValueLength = 0;
        int code = 1;
        string leftoverbits = "";


        private void ResetVariables()
        {
            LZWTable.Clear();
            DecompressLZWTable.Clear();
            Differentchar.Clear();
            Characters.Clear();
            NumbersToWrite.Clear();
            DecompressValues.Clear();
            leftoverbits = string.Empty;
            MaxValueLength = 0;
            code = 1;
        }

        private void FillDictionary(byte[] Text)
        {


            foreach (var character in Text)
            {
                if (!LZWTable.ContainsKey(character.ToString()))
                {
                    LZWTable.Add(character.ToString(), code);
                    code++;
                    Differentchar.Add(character);
                }
            }
        }

        public async Task CompressFile(string path, IFormFile file, string name)
        {
            if (System.IO.File.Exists($"{path}/Uploads/{file.FileName}"))
            {
                System.IO.File.Delete($"{path}/Uploads/{file.FileName}");
            }

            if (System.IO.File.Exists(($"{path}/Compressions/{name}.lzw")))
            {
                System.IO.File.Delete(($"{path}/Compressions/{name}.lzw"));
            }
            ResetVariables();

            using var saver = new FileStream($"{path}/Uploads/{file.FileName}", FileMode.OpenOrCreate);
            await file.CopyToAsync(saver);

            using var reader = new BinaryReader(saver);
            int bufferSize = 2000;
            var buffer = new byte[bufferSize];

            saver.Position = saver.Seek(0, SeekOrigin.Begin);
            while (saver.Position != saver.Length)
            {
                buffer = reader.ReadBytes(bufferSize);
                FillDictionary(buffer);
            }

            saver.Position = saver.Seek(0, SeekOrigin.Begin);
            while (saver.Position != saver.Length)
            {
                buffer = reader.ReadBytes(bufferSize);
                Characters = buffer.ToList();
                MaxValueLength = 0;
                while (Characters.Count != 0)
                {
                    int codeLength = 0;
                    string Subchain = Characters[codeLength].ToString();
                    codeLength++;
                    while (Subchain.Length != 0)
                    {
                        if (Characters.Count > codeLength)
                        {
                            if (!LZWTable.ContainsKey(Subchain + Characters[codeLength].ToString()))
                            {
                                NumbersToWrite.Add(LZWTable[Subchain]);
                                Subchain += Characters[codeLength].ToString();
                                AddValueToDictionary(Subchain);
                                Subchain = string.Empty;
                                for (int i = 0; i < codeLength; i++)
                                {
                                    Characters.RemoveAt(0);
                                }
                            }
                            else
                            {
                                Subchain += Characters[codeLength].ToString();
                                codeLength++;
                            }
                        }
                        else
                        {
                            if (saver.Position != saver.Length)
                            {
                                buffer = reader.ReadBytes(bufferSize);
                                List<byte> aux = buffer.ToList();
                                while (Characters.Count > 0)
                                {
                                    aux.Insert(0, Characters[0]);
                                    Characters.RemoveAt(0);
                                }
                                Characters = aux;
                                MaxValueLength = 0;
                            }
                            else
                            {
                                NumbersToWrite.Add(LZWTable[Subchain]);
                                AddValueToDictionary(Subchain);
                                for (int i = 0; i < codeLength; i++)
                                {
                                    Characters.RemoveAt(0);
                                }
                                Subchain = string.Empty;
                            }

                        }
                    }
                }
            }
            reader.Close();
            saver.Close();

            MaxValueLength = Convert.ToString(NumbersToWrite.Max(), 2).Length;

            if (!Directory.Exists($"{path}/Compressions"))
            {
                Directory.CreateDirectory($"{path}/Compressions");
            }
            using var fileToWrite = new FileStream($"{path}/Compressions/{name}.lzw", FileMode.OpenOrCreate);
            using var writer = new BinaryWriter(fileToWrite);
            string compressionCode = "";
            writer.Write(Convert.ToByte(MaxValueLength));
            writer.Write(Convert.ToByte(Differentchar.Count()));
            foreach (var item in Differentchar)
            {
                writer.Write(item);
            }
            string code = "";
            foreach (var number in NumbersToWrite)
            {
                compressionCode = Convert.ToString(number, 2);
                while (compressionCode.Length < MaxValueLength)
                {
                    compressionCode = "0" + compressionCode;
                }
                code += compressionCode;
                while (code.Length >= 8)
                {
                    writer.Write(Convert.ToByte(code.Substring(0, 8), 2));
                    code = code.Remove(0, 8);
                }
            }
            if (code.Length != 0)
            {
                while (code.Length != 8)
                {
                    code += "0";
                }
                writer.Write(Convert.ToByte(code, 2));
                code = string.Empty;
            }
            writer.Close();
            fileToWrite.Close();
            ResetVariables();
        }

        private void Compression(byte[] Text)
        {
            Characters = Text.ToList();
            MaxValueLength = 0;
            while (Characters.Count != 0)
            {
                int codeLength = 0;
                string Subchain = Characters[codeLength].ToString();
                codeLength++;
                while (Subchain.Length != 0)
                {
                    if (Characters.Count > codeLength)
                    {
                        if (!LZWTable.ContainsKey(Subchain + Characters[codeLength].ToString()))
                        {
                            NumbersToWrite.Add(LZWTable[Subchain]);
                            Subchain += Characters[codeLength].ToString();
                            AddValueToDictionary(Subchain);
                            Subchain = string.Empty;
                            for (int i = 0; i < codeLength; i++)
                            {
                                Characters.RemoveAt(0);
                            }
                        }
                        else
                        {
                            Subchain += Characters[codeLength].ToString();
                            codeLength++;
                        }
                    }
                    else
                    {
                        NumbersToWrite.Add(LZWTable[Subchain]);
                        AddValueToDictionary(Subchain);
                        for (int i = 0; i < codeLength; i++)
                        {
                            Characters.RemoveAt(0);
                        }
                        Subchain = string.Empty;
                    }
                }
            }
        }

        private void AddValueToDictionary(string key)
        {
            if (!LZWTable.ContainsKey(key))
            {
                LZWTable.Add(key, code);
                code++;
            }
        }

        public string CompressText(string text)
        {
            var buffer = ByteGenerator.ConvertToBytes(text);
            FillDictionary(buffer);
            Compression(buffer);
            MaxValueLength = Convert.ToString(NumbersToWrite.Max(), 2).Length;
            List<byte> returningList = new List<byte>
            {
                Convert.ToByte(MaxValueLength),
                Convert.ToByte(Differentchar.Count())
            };
            foreach (var item in Differentchar)
            {
                returningList.Add(item);
            }
            string code = string.Empty;
            foreach (var number in NumbersToWrite)
            {
                string subcode = Convert.ToString(number, 2);
                while (subcode.Length != MaxValueLength)
                {
                    subcode = "0" + subcode;
                }
                code += subcode;
                if (code.Length >= 8)
                {
                    returningList.Add(Convert.ToByte(code.Substring(0, 8), 2));
                    code = code.Remove(0, 8);
                }
            }
            if (code.Length != 0)
            {
                while (code.Length != 8)
                {
                    code += "0";
                }
                returningList.Add(Convert.ToByte(code, 2));
            }
            ResetVariables();
            return ByteGenerator.ConvertToString(returningList.ToArray());
        }

        

        private byte[] FillDecompressionDictionary(byte[] text)
        {
            for (int i = 0; i < text[1]; i++)
            {
                DecompressLZWTable.Add(code, new List<byte> { text[i + 2] });
                code++;
            }
            var CompressedText = new byte[text.Length - (2 + text[1])];
            for (int i = 0; i < CompressedText.Length; i++)
            {
                CompressedText[i] = text[2 + text[1] + i];
            }
            return CompressedText;
        }

        private List<int> Decompression(byte[] compressedText)
        {
            List<int> Codes = new List<int>();
            string binaryNum = leftoverbits;
            DecompressValues.Add(new List<byte>());
            DecompressValues.Add(new List<byte>());
            DecompressValues.Add(new List<byte>());
            foreach (var item in compressedText)
            {
                string subinaryNum = Convert.ToString(item, 2);
                while (subinaryNum.Length < 8)
                {
                    subinaryNum = "0" + subinaryNum;
                }
                binaryNum += subinaryNum;
                while (binaryNum.Length >= MaxValueLength)
                {
                    var index = Convert.ToInt32(binaryNum.Substring(0, MaxValueLength), 2);
                    binaryNum = binaryNum.Remove(0, MaxValueLength);
                    if (index != 0)
                    {
                        Codes.Add(index);
                        DecompressValues[0] = DecompressValues[1];
                        if (DecompressLZWTable.ContainsKey(index))
                        {
                            DecompressValues[1] = SetValuesForDecompress(DecompressLZWTable[index]);
                            DecompressValues[2].Clear();
                            foreach (var value in DecompressValues[0])
                            {
                                DecompressValues[2].Add(value);
                            }
                            DecompressValues[2].Add(DecompressValues[1][0]);
                        }
                        else
                        {
                            DecompressValues[1] = DecompressValues[0];
                            DecompressValues[2].Clear();
                            foreach (var value in DecompressValues[0])
                            {
                                DecompressValues[2].Add(value);
                            }
                            DecompressValues[2].Add(DecompressValues[1][0]);
                            DecompressValues[1] = SetValuesForDecompress(DecompressValues[2]);
                        }
                        if (!CheckIfExists(DecompressValues[2]))
                        {
                            DecompressLZWTable.Add(code, new List<byte>(DecompressValues[2]));
                            code++;
                        }
                    }
                }
            }
            DecompressValues.Clear();
            leftoverbits = binaryNum;
            return Codes;
        }

        private List<byte> SetValuesForDecompress(List<byte> values)
        {
            List<byte> newList = new List<byte>();
            foreach (var value in values)
            {
                newList.Add(value);
            }
            return newList;
        }

        private bool CheckIfExists(List<byte> actualString)
        {
            foreach (var item in DecompressLZWTable.Values)
            {
                if (actualString.Count == item.Count)
                {
                    if (CompareListofBytes(actualString, item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CompareListofBytes(List<byte> list1, List<byte> list2)
        {
            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public string DecompressText(string text)
        {
            var buffer = ByteGenerator.ConvertToBytes(text);
            MaxValueLength = buffer[0];
            buffer = FillDecompressionDictionary(buffer);
            var DecompressedIndexes = Decompression(buffer);
            var BytesToWrite = new List<byte>();
            foreach (var index in DecompressedIndexes)
            {
                foreach (var value in DecompressLZWTable[index])
                {
                    BytesToWrite.Add(value);
                }
            }
            return ByteGenerator.ConvertToString(BytesToWrite.ToArray());
        }

        public async Task DecompressFile(string path, IFormFile file, string name)
        {
            if (System.IO.File.Exists($"{path}/Uploads/{file.FileName}"))
            {
                System.IO.File.Delete($"{path}/Uploads/{file.FileName}");
            }

            if (System.IO.File.Exists($"{path}/Decompressions/{name}"))
            {
                System.IO.File.Delete($"{path}/Decompressions/{name}");
            }
            ResetVariables();
            using var saver = new FileStream($"{path}/Uploads/{file.FileName}", FileMode.OpenOrCreate);
            await file.CopyToAsync(saver);

            using var reader = new BinaryReader(saver);
            int bufferSize = 2000;
            var buffer = new byte[bufferSize];
            saver.Position = saver.Seek(0, SeekOrigin.Begin);
            buffer = reader.ReadBytes(bufferSize);
            MaxValueLength = buffer[0];
            saver.Position = 2 + buffer[1];
            buffer = FillDecompressionDictionary(buffer);
            List<int> Codes = new List<int>();
            string binaryNum = "";
            DecompressValues.Add(new List<byte>());
            DecompressValues.Add(new List<byte>());
            DecompressValues.Add(new List<byte>());
            while (saver.Position != saver.Length)
            {
                buffer = reader.ReadBytes(bufferSize);
                foreach (var item in buffer)
                {
                    string subinaryNum = Convert.ToString(item, 2);
                    while (subinaryNum.Length < 8)
                    {
                        subinaryNum = "0" + subinaryNum;
                    }
                    binaryNum += subinaryNum;
                    while (binaryNum.Length >= MaxValueLength)
                    {
                        var index = Convert.ToInt32(binaryNum.Substring(0, MaxValueLength), 2);
                        binaryNum = binaryNum.Remove(0, MaxValueLength);
                        if (index != 0)
                        {
                            Codes.Add(index);
                            DecompressValues[0] = DecompressValues[1];
                            if (DecompressLZWTable.ContainsKey(index))
                            {
                                DecompressValues[1] = SetValuesForDecompress(DecompressLZWTable[index]);
                                DecompressValues[2].Clear();
                                foreach (var value in DecompressValues[0])
                                {
                                    DecompressValues[2].Add(value);
                                }
                                DecompressValues[2].Add(DecompressValues[1][0]);
                            }
                            else
                            {
                                DecompressValues[1] = DecompressValues[0];
                                DecompressValues[2].Clear();
                                foreach (var value in DecompressValues[0])
                                {
                                    DecompressValues[2].Add(value);
                                }
                                DecompressValues[2].Add(DecompressValues[1][0]);
                                DecompressValues[1] = SetValuesForDecompress(DecompressValues[2]);
                            }
                            if (!CheckIfExists(DecompressValues[2]))
                            {
                                DecompressLZWTable.Add(code, new List<byte>(DecompressValues[2]));
                                code++;
                            }
                        }
                    }
                }
            }
            reader.Close();
            saver.Close();

            if (!Directory.Exists($"{path}/Decompressions"))
            {
                Directory.CreateDirectory($"{path}/Decompressions");
            }
            using var fileToWrite = new FileStream($"{path}/Decompressions/{name}", FileMode.OpenOrCreate);
            using var writer = new BinaryWriter(fileToWrite);
            foreach (var index in Codes)
            {
                foreach (var value in DecompressLZWTable[index])
                {
                    writer.Write(value);
                }
            }
            writer.Close();
            fileToWrite.Close();
        }
    }

}