using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace CustomGenerics.Structures
{
    interface ICompressor
    {
        Task CompressFile(string path, IFormFile file, string name);
        Task DecompressFile(IFormFile file, string name);
        string CompressText(string text);
        string DecompressText(string text);
    }
}
