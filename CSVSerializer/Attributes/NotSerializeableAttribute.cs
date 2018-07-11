using System;

namespace CommaSeparatedValuesSerializer.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DoNotSerializeAttribute : Attribute
    {
        public DoNotSerializeAttribute()
        {
        }
    }
}
