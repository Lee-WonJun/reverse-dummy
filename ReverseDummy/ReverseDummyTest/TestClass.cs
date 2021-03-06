using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace ReverseDummyTest
{
    internal class Famlly
    {
        public List<Person> People { get; set; }
        public string Address { get; set; }
    }

    internal class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }

        [XmlIgnore]
        public string MetaData { get; set; }
    }
}
