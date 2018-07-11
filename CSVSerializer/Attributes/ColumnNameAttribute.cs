using System;

namespace CommaSeparatedValuesSerializer.Attributes
{
    public class ColumnNameAttribute : Attribute
    {
        public string ColumnName { get; set; }

        public ColumnNameAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }
}
