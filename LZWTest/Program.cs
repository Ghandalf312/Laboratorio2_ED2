﻿using System;
using ClassLibrary.Structures;

namespace LZWTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                LZWCompress lZW = new LZWCompress();
                Console.WriteLine("Escribe el string a comprimir");
                string text = Console.ReadLine();
                Console.WriteLine("Se ha guardado el string con éxito para comprimir");
                string CompressedText = lZW.CompressText(text);
                Console.WriteLine("El resultado de la compresión es el siguiente:");
                Console.WriteLine(CompressedText);
                Console.WriteLine("¿Desea descomprimirlo? | Presione 'Y'. De lo contrario, presione cualquier otra tecla.");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.Clear();
                    Console.WriteLine("El resultado de la descompresión es el siguiente:");
                    Console.WriteLine(lZW.DecompressText(CompressedText));
                    Console.ReadLine();
                }
                Console.WriteLine("Feliz día! :D");
                Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("Inserte un string válido");
                Console.ReadLine();
                Console.Clear();
            }
        }
    }
}
