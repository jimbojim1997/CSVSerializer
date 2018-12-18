﻿using CommaSeparatedValuesSerializer.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
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
                        Type columnType = data.Columns[i].DataType;
                        if (!IsValidType(columnType)) throw new InvalidTypeException($"Type \"{columnType.FullName}\" is not a supported type. See documentation for supported types.");
                        object convertedColumn = null;
                        ConvertFromString(column, columnType, ref convertedColumn);
                        row[i] = convertedColumn;
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
        public static void Serialize<T>(Stream stream, IEnumerable<T> data) where T : new()
        {
            if (stream == null) throw new ArgumentNullException("stream", "stream cannot be null.");
            if (data == null) throw new ArgumentNullException("data", "data cannot be null.");

            //get properties to be included and for column names
            List<KeyValuePair<string, string>> properties = new List<KeyValuePair<string, string>>(); //key is the property, value is the column name
            foreach(PropertyInfo property in typeof(T).GetRuntimeProperties())
            {
                bool doNotSerialize = false;
                string columnName = null;
                foreach(Attribute attribute in property.GetCustomAttributes())
                {
                    if (attribute.GetType() == typeof(DoNotSerializeAttribute)) doNotSerialize = true;
                    else if (attribute.GetType() == typeof(ColumnNameAttribute))
                    {
                        columnName = (attribute as ColumnNameAttribute).ColumnName;
                    }
                }

                if (doNotSerialize) continue;

                if (columnName == null) properties.Add(new KeyValuePair<string, string>(property.Name, property.Name));
                else properties.Add(new KeyValuePair<string, string>(property.Name, columnName));
            }

            //tabulate data
            DataTable table = new DataTable();

            //add column names
            foreach(KeyValuePair<string, string> property in properties)
            {
                table.Columns.Add(new DataColumn(property.Value));
            }

            //add data
            foreach(T item in data)
            {
                DataRow row = table.NewRow();
                foreach (KeyValuePair<string, string> property in properties)
                {
                    string value = typeof(T).GetRuntimeProperty(property.Key).GetValue(item).ToString();
                    row[property.Value] = value;
                }
                table.Rows.Add(row);
            }

            Serialize(stream, table);
        }

        public static void Serialize<T>(string path, IEnumerable<T> data) where T : new()
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
        public static IEnumerable<T> Deserialize<T>(Stream stream) where T : new()
        {
            if (stream == null) throw new ArgumentNullException("stream", "stream cannot be null.");

            //get properties to be deserialised and for column names
            List<KeyValuePair<string, string>> properties = new List<KeyValuePair<string, string>>(); //key is the property, value is the column name
            foreach (PropertyInfo property in typeof(T).GetRuntimeProperties())
            {
                bool doNotSerialize = false;
                string columnName = null;
                foreach (Attribute attribute in property.GetCustomAttributes())
                {
                    if (attribute.GetType() == typeof(DoNotSerializeAttribute)) doNotSerialize = true;
                    else if (attribute.GetType() == typeof(ColumnNameAttribute))
                    {
                        columnName = (attribute as ColumnNameAttribute).ColumnName;
                    }
                }

                if (doNotSerialize) continue;

                if (columnName == null) properties.Add(new KeyValuePair<string, string>(property.Name, property.Name));
                else properties.Add(new KeyValuePair<string, string>(property.Name, columnName));
            }

            DataTable table = Deserialize(stream);
            List<T> data = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                T item = new T();
                foreach (KeyValuePair<string, string> property in properties)
                {
                    PropertyInfo propertyInfo = typeof(T).GetProperty(property.Key);
                    Type valueType = propertyInfo.PropertyType;
                    if(!IsValidType(valueType)) throw new InvalidTypeException($"Type \"{valueType.FullName}\" is not a supported type. See documentation for supported types.");

                    string value = row[property.Value].ToString();
                    object convertedValue = null;
                    ConvertFromString(value, valueType, ref convertedValue);
                    propertyInfo.SetValue(item, convertedValue);
                }
                data.Add(item);
            }

            return data;
        }

        public static IEnumerable<T> Deserialize<T>(string path) where T : new()
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
            if (value.Length > 0 && value[0] == CSV_STRING_CHAR && value[value.Length - 1] == CSV_STRING_CHAR)
            {
                value = value.Substring(1, value.Length - 2);
            }
            value = value.Replace("\"\"", "\"");

            return value;
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
                    row.Add(FromCSVString(csvText.Substring(start, end - start)));
                    start = end + 1;
                    continue;
                }
                else if(csvText[end] == CSV_LINE_SEPARATOR[0] && csvText.Substring(end, CSV_LINE_SEPARATOR.Length) == CSV_LINE_SEPARATOR)
                {
                    row.Add(FromCSVString(csvText.Substring(start, end - start)));
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
            ConvertFromString(value, typeof(T), ref item);
        }

        private static void ConvertFromString(string value, Type type, ref object item)
        {
            if (type == typeof(string)) item = value;
            else if (type == typeof(bool)) item = Convert.ToBoolean(value);
            else if (type == typeof(sbyte)) item = Convert.ToSByte(value);
            else if (type == typeof(byte)) item = Convert.ToByte(value);
            else if (type == typeof(char)) item = Convert.ToChar(value);
            else if (type == typeof(Int16)) item = Convert.ToInt16(value);
            else if (type == typeof(Int32)) item = Convert.ToInt32(value);
            else if (type == typeof(Int64)) item = Convert.ToInt64(value);
            else if (type == typeof(UInt16)) item = Convert.ToUInt16(value);
            else if (type == typeof(UInt32)) item = Convert.ToUInt32(value);
            else if (type == typeof(UInt64)) item = Convert.ToUInt64(value);
            else if (type == typeof(float)) item = Convert.ToSingle(value);
            else if (type == typeof(double)) item = Convert.ToDouble(value);
            else if (type == typeof(Decimal)) item = Convert.ToDecimal(value);
            else if (type == typeof(DateTime)) item = Convert.ToDateTime(value);
            else throw new InvalidTypeException($"Type \"{type.FullName}\" is not a supported type. See documentation for supported types.");
        }
        #endregion
    }
}
