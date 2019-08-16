using System;
using FunkyBDD.SxS.Selenium.Browserstack;

namespace FunkyBDD.SxS.Selenium.Browserstack.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var browser = new Browser("iPhoneXS");
            var driver = browser.Driver;
            driver.Navigate().GoToUrl("https://www.swisslife.ch/");
            Console.WriteLine($"The titel is '{driver.Title}'");
            Console.WriteLine($"The accuracy for image comparison is {browser.AccuracyInPromille} promille");

            #region console handling
                Console.WriteLine(" ");
                Console.WriteLine("Press enter to terminate...");
                Console.ReadLine();
            #endregion

            browser.DisposeDriver();
        }
    }
}
