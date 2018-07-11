using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace CommaSeparatedValuesSerializer
{
    public static class CSVSerializer
    {
        private const char CSV_STRING_CHAR = '"';
        private const char CSV_VALUE_SEPARATOR = ',';
        private static readonly string CSV_LINE_SEPARATOR = Environment.NewLine;

        #region Serialize DataTable
        public static void Serialize(Stream stream, DataTable data)
        {
            if (stream == null) throw new ArgumentNullException("stream", "stream cannot be null.");
            if (data == null) throw new ArgumentNullException("data", "data cannot be null.");

            StringBuilder csv = new StringBuilder();

            //Write headers
            bool isFirstColumn = true;
            foreach(DataColumn column in data.Columns)
            {
                if (!isFirstColumn) csv.Append(CSV_VALUE_SEPARATOR);
                else isFirstColumn = false;
                csv.Append(ToCSVString(column.ColumnName));
            }
            csv.Append(CSV_LINE_SEPARATOR);

            //Write data
            foreach (DataRow row in data.Rows)
            {
                bool isFirstItem = true;
                foreach(object item in row.ItemArray)
                {
                    if(!IsValidType(item.GetType())) throw new InvalidTypeException($"Type \"{item.GetType().FullName}\" is not a supported type. See documentation for supported types.");

                    if (!isFirstItem) csv.Append(CSV_VALUE_SEPARATOR);
                    else isFirstItem = false;
                    csv.Append(ToCSVString(item.ToString()));
                }
                csv.Append(CSV_LINE_SEPARATOR);
            }

            //Write to stream
            using(StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(csv);
            }
        }

        public static void Serialize(string path, DataTable data)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path", "path cannot be null, empty or whitespace.");
            if (data == null) throw new ArgumentNullException("data", "data cannot be null.");

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                Serialize(stream, data);
            }
        }
        #endregion

        #region Deserialize DataTable
        public static DataTable Deserialize(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream", "stream cannot be null.");

            //Read data
            string csvText = "";
            using (StreamReader reader = new StreamReader(stream))
            {
                csvText = reader.ReadToEnd();
            }

            //Split CSV text
            if (csvText.Length == 0) throw new NoDataException("No valid CSV data found.");
            List<List<string>> csvItems = SplitCSVToItems(csvText);
            if (csvItems.Count == 0) throw new NoDataException("No valid CSV data found.");

            //Create DataTable
            DataTable data = new DataTable();
            bool isHeaderRow = true;
            foreach(List<string> itemRow in csvItems)
            {
                DataRow row = data.NewRow();
                for (int i = 0; i < itemRow.Count; i++)
                {
                    string column = itemRow[i];
                    if (isHeaderRow)
                    {
                        data.Columns.Add(column);
                    }
                    else
                    {
                        row[i] = column;
                    }
                }
                if (isHeaderRow) isHeaderRow = false;
                else data.Rows.Add(row);
            }
            return data;
        }

        public static DataTable Deserialize(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path", "path cannot be null, empty or whitespace.");

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                return Deserialize(stream);
            }
        }
        #endregion

        #region Serialize Generic
        public static void Serialize<T>(Stream stream, T data) where T : new()
        {
            if (stream == null) throw new ArgumentNullException("stream", "stream cannot be null.");
            if (data == null) throw new ArgumentNullException("data", "data cannot be null.");

        }

        public static void Serialize<T>(string path, T data) where T : new()
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path", "path cannot be null, empty or whitespace.");
            if (data == null) throw new ArgumentNullException("data", "data cannot be null.");

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                Serialize<T>(stream, data);
            }
        }
        #endregion

        #region Deserialize Generic
        public static T Deserialize<T>(Stream stream) where T : new()
        {
            if (stream == null) throw new ArgumentNullException("stream", "stream cannot be null.");

            throw new NotImplementedException();
        }

        public static T Deserialize<T>(string path) where T : new()
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path", "path cannot be null, empty or whitespace.");

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                return Deserialize<T>(stream);
            }
        }
        #endregion

        #region Utilities
        private static string ToCSVString(string value)
        {
            string[] valueParts = value.Split(CSV_STRING_CHAR);
            StringBuilder result = new StringBuilder();

            result.Append(CSV_STRING_CHAR);
            bool isFirstPart = true;
            foreach(string part in valueParts)
            {
                if (!isFirstPart)
                {
                    result.Append(CSV_STRING_CHAR);
                    result.Append(CSV_STRING_CHAR);
                }
                else
                {
                    isFirstPart = false;
                }

                result.Append(part);
            }
            result.Append(CSV_STRING_CHAR);

            return result.ToString();
        }

        private static string FromCSVString(string value)
        {
            string[] valueParts = value.Split(CSV_STRING_CHAR);
            StringBuilder result = new StringBuilder();

            bool isFirstPart = true;
            bool wasLastPartEmpty = false;
            foreach (string part in valueParts)
            {
                if (!isFirstPart && !wasLastPartEmpty) result.Append(CSV_STRING_CHAR);
                else isFirstPart = false;

                wasLastPartEmpty = part == "" && !wasLastPartEmpty;
                if(part != "") result.Append(part);
            }

            return result.ToString();
        }

        private static List<List<string>> SplitCSVToItems(string csvText)
        {
            List<List<string>> table = new List<List<string>>();
            List<string> row = new List<string>();

            int start = 0;
            bool isInCSVString = false;
            for (int end = 0; end < csvText.Length; end++)
            {
                if (csvText[end] == CSV_STRING_CHAR)
                {
                    isInCSVString = !isInCSVString;
                }
                else if(csvText[end] == CSV_VALUE_SEPARATOR && !isInCSVString)
                {
                    row.Add(FromCSVString(csvText.Substring(start, end - start - 1)));
                    start = end + 1;
                    continue;
                }
                else if(csvText[end] == CSV_LINE_SEPARATOR[0] && csvText.Substring(end, CSV_LINE_SEPARATOR.Length) == CSV_LINE_SEPARATOR)
                {
                    row.Add(FromCSVString(csvText.Substring(start, end - start - 1)));
                    table.Add(row);
                    row = new List<string>();
                    start = end = end + CSV_LINE_SEPARATOR.Length;
                    end--;
                }
            }
            return table;
        }

        private static bool IsValidType(Type type)
        {
            if (type == typeof(string) ||
                type == typeof(bool) ||
                type == typeof(sbyte) ||
                type == typeof(byte) ||
                type == typeof(char) ||
                type == typeof(Int16) ||
                type == typeof(Int32) ||
                type == typeof(Int64) ||
                type == typeof(UInt16) ||
                type == typeof(UInt32) ||
                type == typeof(UInt64) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(Decimal) ||
                type == typeof(DateTime)) return true;
            else return false;
        }

        private static void ConvertFromString<T>(string value, ref object item)
        {
            if (typeof(T) == typeof(string)) item = value;
            if (typeof(T) == typeof(bool)) item = Convert.ToBoolean(value);
            if (typeof(T) == typeof(sbyte)) item = Convert.ToSByte(value);
            if (typeof(T) == typeof(byte)) item = Convert.ToByte(value);
            if (typeof(T) == typeof(char)) item = Convert.ToChar(value);
            if (typeof(T) == typeof(Int16)) item = Convert.ToInt16(value);
            if (typeof(T) == typeof(Int32)) item = Convert.ToInt32(value);
            if (typeof(T) == typeof(Int64)) item = Convert.ToInt64(value);
            if (typeof(T) == typeof(UInt16)) item = Convert.ToUInt16(value);
            if (typeof(T) == typeof(UInt32)) item = Convert.ToUInt32(value);
            if (typeof(T) == typeof(UInt64)) item = Convert.ToUInt64(value);
            if (typeof(T) == typeof(float)) item = Convert.ToSingle(value);
            if (typeof(T) == typeof(double)) item = Convert.ToDouble(value);
            if (typeof(T) == typeof(Decimal)) item = Convert.ToDecimal(value);
            if (typeof(T) == typeof(DateTime)) item = Convert.ToDateTime(value);
        }
        #endregion
    }
}
