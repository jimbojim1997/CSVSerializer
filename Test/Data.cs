using System;
using CommaSeparatedValuesSerializer.Attributes;

namespace Test
{
    class Data
    {
        public string Forename { get; set; }
        public string Surname { get; set; }

        [ColumnName("Date Of Birth")]
        public DateTime DateOfBirth { get; set; }
        public bool IsAlive { get; set; }
        public string Description { get; set; }


        public Data()
        {
        }

        public Data(string forename, string surname, DateTime dateOfBirth, bool isAlive, string description)
        {
            Forename = forename;
            Surname = surname;
            DateOfBirth = dateOfBirth;
            IsAlive = isAlive;
            Description = description;
        }
    }
}
