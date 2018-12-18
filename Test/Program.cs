using CommaSeparatedValuesSerializer;
using System;
using System.Data;
using System.Collections.Generic;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = CSVSerializer.Deserialize<Data>("postcodes.csv");

            Console.ReadLine();
        }
    }
}
