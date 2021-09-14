﻿using Microsoft.AspNetCore.Mvc;
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
    [Route("api/[controller]")]
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

        // GET api/<HuffmanController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
     
        // POST api/<HuffmanController>
        [HttpPost]
        [Route("compress/{name}")]
        public async Task<IActionResult> PostHuffmanAsync([FromForm] IFormFile file, string name)
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
    }
}