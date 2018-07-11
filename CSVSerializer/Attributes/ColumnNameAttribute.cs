using System;

namespace CommaSeparatedValuesSerializer.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnNameAttribute : Attribute
    {
        public string ColumnName { get; set; }

        public ColumnNameAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }
}
