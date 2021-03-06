using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReverseDummy;
namespace ReverseDummyTest
{
    [TestClass]
    public class ReverseDummyTest1
    {
        private string RemoveWhiteSpace(string s) => s.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");

        [TestMethod]
        public void GenerateSignleEntitiyTest()
        {
            //Given
            var Person = new Person
            {
                Age = 17,
                BirthDate = new DateTime(1995, 11, 01),
                Name = "Lee"
            };

            //When
            var generatedCode  = Generator.ToCSharpCode("Person", Person);

            //Then
            string targetCode = $@"var Person =   new ReverseDummyTest.Person 
                {{
                                Age = 17 ,
                                Name = ""Lee"" ,
                                BirthDate = new DateTime({Person.BirthDate.Ticks})
                }}; ";

            Console.WriteLine(RemoveWhiteSpace(generatedCode));
            Console.WriteLine(RemoveWhiteSpace(targetCode));

            Assert.AreEqual(
                RemoveWhiteSpace(generatedCode), 
                RemoveWhiteSpace(targetCode));

        }

        [TestMethod]
        public void IgnoreXMLAttributeTest()
        {
            //Given
            var Person = new Person
            {
                Age = 17,
                BirthDate = new DateTime(1995, 11, 01),
                Name = "Lee"
            };

            var PersonWithIgnoreData = new Person
            {
                Age = 17,
                BirthDate = new DateTime(1995, 11, 01),
                Name = "Lee",
                MetaData = "meta"
            };

            //When
            var generatedCode = Generator.ToCSharpCode("Person", Person);
            var generatedWithIgnoreCode = Generator.ToCSharpCode("Person", PersonWithIgnoreData);

            //Then
            Assert.AreEqual(
                RemoveWhiteSpace(generatedCode),
                RemoveWhiteSpace(generatedWithIgnoreCode));
        }
    }
}
