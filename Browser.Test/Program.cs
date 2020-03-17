using System;

namespace FunkyBDD.SxS.Selenium.Browserstack.Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            var browser = new Browser("FirefoxLocal");
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
