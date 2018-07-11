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
            List<Data> data = new List<Data>();
            data.Add(new Data("Alice", "Alison", new DateTime(2001, 1, 1), true, "This is the description\n\nwith line breaks and \"quotes\""));
            data.Add(new Data("Bob", "Brown", new DateTime(2002, 2, 2), true, "blah blah blah"));
            data.Add(new Data("Charlie", "Chaplin", new DateTime(1989, 4, 16), true, "Hmmm"));

            CSVSerializer.Serialize<Data>("output.csv", data);
        }

        private static void DisplayTable(DataTable table)
        {
            bool isFirstColumn = true;
            foreach(DataColumn column in table.Columns)
            {
                if (!isFirstColumn) Console.Write(",");
                else isFirstColumn = false;
                Console.Write(column.ColumnName);
            }
            Console.WriteLine();
            foreach (DataRow row in table.Rows)
            {
                bool isFirstItem = true;
                foreach(object item in row.ItemArray)
                {
                    if (!isFirstItem) Console.Write(",");
                    else isFirstItem = false;
                    Console.Write(item.ToString());
                }
                Console.WriteLine();
            }
        }
    }
}
