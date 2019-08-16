# FunkyBDD.SxS.Selenium.Browserstack
Selenium helpers to optimize the work with BrowserStack.

```c#
using System;
using FunkyBDD.SxS.Selenium.Browserstack;

namespace MainApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var browser = new Browser("iPhoneXS");
            var driver = browser.Driver;
            driver.Navigate().GoToUrl("https://www.swisslife.ch/");
            Console.WriteLine($"The titel is '{driver.Title}'");
            Console.WriteLine($"The accuracy is {browser.AccuracyInPromille} promille");

            Console.WriteLine(" ");
            Console.WriteLine("Press enter to terminate...");
            Console.ReadLine();

            browser.DisposeDriver();
        }
    }
}
```

With this class you can easily work with [BrowserStack](https://www.browserstack.com/). The configuration is controlled by the file `appsettings.browserstack.json` , which is stored in the project. This should be copied into the bin directory. If it does not exist there, the file will be created with an example.

The following values can also be set as environment variables, where they override the values from the configuration file. Thus, the values in the test pipeline can be set as environment variables so that there are no credentials in the configuration file.

- BROWSERSTACK_PROJECT
- BROWSERSTACK_ENVIRONMENT
- BROWSERSTACK_USERNAME
- BROWSERSTACK_ACCESS_KEY

If no configured browser is found, a local Firefox is used.

You will find a learning project with examples based on this package on [GitHub](https://github.com/AndreasKarz/AutomatedTestingWorkshop). 