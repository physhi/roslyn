using Microsoft.CodeAnalysis.CSharp.TestGround;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            TestGroundFile1.Main(
                args,
                Console.WriteLine);
        }
    }
}
