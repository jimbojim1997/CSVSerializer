using System;
using CommaSeparatedValuesSerializer.Attributes;

namespace Test
{
    class Data
    {
        [ColumnName("Postcode")]
        public string Postcode { get; set; }
        [ColumnName("Positional_quality_indicator")]
        public string PositionalQualityIndicator { get; set; }
        [ColumnName("Eastings")]
        public string Eastings { get; set; }
        [ColumnName("Northings")]
        public string Northings { get; set; }
        [ColumnName("Country_code")]
        public string CountryCode { get; set; }
        [ColumnName("NHS_regional_HA_code")]
        public string NHSRegionalHACode { get; set; }
        [ColumnName("NHS_HA_code")]
        public string NHSHACode { get; set; }
        [ColumnName("Admin_county_code")]
        public string AdminCountyCode { get; set; }
        [ColumnName("Admin_district_code")]
        public string AdminDistrictCode { get; set; }
        [ColumnName("Admin_ward_code")]
        public string AdminWardCode { get; set; }
    }
}
