using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AutoStuff
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now);
            for (int i = 0; i < 15; i++) {
                var processor = new Processor();
                processor.Process();
                processor.Dispose();
                Console.WriteLine("========================================\n");
            }
        }
    }
}
