using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Creating a dynamic dictionary.
            dynamic person = new DynamicDictionary();

            // Adding new dynamic properties. 
            // The TrySetMember method is called.
            person.FirstName = "Ellen";
            person.LastName = "Adams";
            person.MiddleName = "Alice";

            // Getting values of the dynamic properties.
            // The TryGetMember method is called.
            // Note that property names are case-insensitive.
            Console.WriteLine(person.firstname + " " + person.lastname);

            Console.WriteLine("person.MiddleName" + " " + person.middlename);

            // Getting the value of the Count property.
            // The TryGetMember is not called, 
            // because the property is defined in the class.
            Console.WriteLine(
                "Number of dynamic properties:" + person.Count);

            // The following statement throws an exception at run time.
            // There is no "address" property,
            // so the TryGetMember method returns false and this causes a
            // RuntimeBinderException.
            // Console.WriteLine(person.address);
            //GetMemberBinder binder = new GetMemberBinder();
            //person.TryGetMember()
            dynamic dynEO = new ExpandoObject();//初始化一个不包含任何成员的ExpandoObject
            dynEO.number = 20;
            dynEO.MeThod = new Func<int,int, string>((i,j) => { return (i +j+ 20).ToString(); });
            Console.WriteLine(dynEO.number);
            Console.WriteLine(dynEO.MeThod(dynEO.number,dynEO.number + 5));
            var functio1n = new funtion((i, j) => { return (i + j).ToString(); });
            dynEO.MeThod = functio1n;
            Console.WriteLine(dynEO.MeThod(5, 6));
            functio1n = GetResult;
            Console.WriteLine(dynEO.MeThod(5, 6));
            Console.ReadKey();

        }
        private static string GetResult(int i, int j)
        {
            return "123";
        }
    }
}
