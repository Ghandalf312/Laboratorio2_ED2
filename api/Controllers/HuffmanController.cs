using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mime;
using api.Models;
using ClassLibrary.Structures;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace api.Controllers
{
    [Route("api")]
    [ApiController]
    public class HuffmanController : ControllerBase
    {
        private IWebHostEnvironment Environment;

        public HuffmanController(IWebHostEnvironment env)
        {
            Environment = env;
        }

        // GET: api/<HuffmanController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/<HuffmanController>
        [HttpGet]
        [Route("compressions")]
        public List<HuffmanCompressions> GetListCompress()
        {
            HuffmanCompressions.LoadHistList(Environment.ContentRootPath);
            return Singleton.Instance.HistoryList;
        }

        // GET api/<HuffmanController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<HuffmanController>
        [Route("compress/lzw/{name}")]
        [HttpPost]
        public async Task<IActionResult> PostLzwAsync([FromForm] IFormFile file, string name)
        {
            try
            {
                int i = 1;
                var originalname = name;
                if (!Directory.Exists($"{Environment.ContentRootPath}/Uploads/"))
                {
                    Directory.CreateDirectory($"{Environment.ContentRootPath}/Uploads/");
                }
                while (System.IO.File.Exists($"{Environment.ContentRootPath}/Uploads/{name}"))
                {
                    name = originalname + "(" + i.ToString() + ")";
                    i++;
                }
                await Singleton.Instance._Compressions.CompressFile(Environment.ContentRootPath, file, name);
                var LZWInfo = new HuffmanCompressions();
                LZWInfo.SetAttributes(Environment.ContentRootPath, file.FileName, name);
                Singleton.Instance.HistoryList.Add(LZWInfo);

                return PhysicalFile($"{Environment.ContentRootPath}/Compressions/{name}.lzw", MediaTypeNames.Text.Plain, $"{name}.lzw");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        // POST api/<HuffmanController>
        [HttpPost]
        [Route("compress/huffman/{name}")]
        public async Task<IActionResult> PostHuffmanAsync([FromForm] IFormFile file, string name) // Key: file | Value: [nombre archivo].txt | Description: file
        {
            Singleton.Instance.HuffmanTree = new Huffman<HuffmanCharacter>($"{Environment.ContentRootPath}");
            int i = 1;
            var originalname = name;
            while (System.IO.File.Exists($"{Environment.ContentRootPath}/{name}"))
            {
                name = originalname + "(" + i.ToString() + ")";
                i++;
            }
                await Singleton.Instance.HuffmanTree.CompressFile(Environment.ContentRootPath, file, name);
            var HuffmanInfo = new HuffmanCompressions();
            HuffmanInfo.SetAttributes(Environment.ContentRootPath, file.FileName, name);
            Singleton.Instance.HistoryList.Add(HuffmanInfo);

            return PhysicalFile($"{Environment.ContentRootPath}/{name}", MediaTypeNames.Text.Plain, $"{name}.huff");
          
        }

        // POST api/<HuffmanController>
        [HttpPost]
        [Route("decompress/huffman")]
        public async Task<IActionResult> Decompress([FromForm] IFormFile file) // Key: file | Value: [nombre archivo].huff | Description: file
        {
                Singleton.Instance.HuffmanTree = new Huffman<HuffmanCharacter>($"{Environment.ContentRootPath}");
                HuffmanCompressions.LoadHistList(Environment.ContentRootPath);
                var name = "";
                foreach (var item in Singleton.Instance.HistoryList)
                {
                    if (item.CompressedName == file.FileName)
                    {
                        name = item.OriginalName;
                    }
                }
                await Singleton.Instance.HuffmanTree.DecompressFile(Environment.ContentRootPath, file, name);
                return PhysicalFile($"{Environment.ContentRootPath}/{name}", MediaTypeNames.Text.Plain, ".txt"); 
        }
    }
}
