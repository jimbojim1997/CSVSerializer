using CommaSeparatedValuesSerializer;
using System;
using System.Data;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteTestData();
            DataTable data = ReadDataTable();
            DisplayTable(data);
            Console.ReadLine();
        }

        private static void WriteTestData()
        {
            DataTable data = new DataTable();
            data.Columns.Add("Forename", typeof(string));
            data.Columns.Add("Surname", typeof(string));
            data.Columns.Add("Date Of Birth", typeof(DateTime));
            data.Columns.Add("Is Alive", typeof(bool));
            data.Columns.Add("Description", typeof(string));

            data.Rows.Add("Alice", "Alison", new DateTime(2001, 1, 1), true, "This is the description\nwith line breaks and \"quotes\"");
            data.Rows.Add("Bob", "Brown", new DateTime(2002, 2, 2), true, "blah blah blah");
            data.Rows.Add("Charlie", "Chaplin", new DateTime(2002, 2, 2), false, "Hmmm");

            CSVSerializer.Serialize("output.csv", data);
        }

        private static DataTable ReadDataTable()
        {
            using (FileStream stream = new FileStream("output.csv", FileMode.Open))
            {
                return CSVSerializer.Deserialize(stream);
            }
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
