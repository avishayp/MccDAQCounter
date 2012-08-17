using System;
using System.Collections.Generic;

namespace Tester
{
    static class Program
    {
        [STAThread]
        static void Main(string[] argv)
        {
            Test1 test1 = new Test1();
            test1.RunTest();
        }
    }
}
