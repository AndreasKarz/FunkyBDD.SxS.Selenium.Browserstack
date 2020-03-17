using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace FunkyBDD.SxS.Selenium.Browserstack
{
    public class Browser
    {
        private static string BrowserstackUser = Environment.GetEnvironmentVariable("BROWSERSTACK_USERNAME") ?? "";
        private static string BrowserstackKey = Environment.GetEnvironmentVariable("BROWSERSTACK_ACCESS_KEY") ?? "";
        private static string Headless = Environment.GetEnvironmentVariable("HEADLESS") ?? "false";

        /// <summary>
        ///     Reference to the WebDriver
        /// </summary>
        public IWebDriver Driver { get; set; }
        
        /// <summary>
        ///     Accuracy in promille for image comparison
        /// </summary>
        public int AccuracyInPromille { get; set; } = 1000;

        /// <summary>
        ///     Default timeout in seconds used for explicite waiting
        /// </summary>
        public int DefaultTimeout { get; set; } = 120;
        
        /// <summary>
        ///     Indicates if the browser is a Desktop browser or not.
        ///     Set in appsettings.browserstack.json
        /// </summary>
        public bool IsDesktop { get; set; } = false;

        /// <summary>
        ///     Indicates if a browser config for the given browser name is present in the 
        ///     appsettings.browserstack.json
        /// </summary>
        public string Status { get; set; } = "NOK";
        
        /// <summary>
        ///     Indicates if the tests run over BrowserStack local or not
        /// </summary>
        public bool BrowserstackLocal { get; set; } = false;
        
        /// <summary>
        ///     Name of the test project
        /// </summary>
        public string BrowserstackProject { get; set; } = Environment.GetEnvironmentVariable("BROWSERSTACK_PROJECT") ?? "";
        
        /// <summary>
        ///     Name of the test environment
        /// </summary>
        public string BrowserstackEnvironment { get; set; } = Environment.GetEnvironmentVariable("BROWSERSTACK_ENVIRONMENT") ?? "";
        
        /// <summary>
        ///     LocalIdentifier for BrowserStack. This value will only set over the env
        ///     inside the build pipeline.
        /// </summary>
        public string BrowserstackLocalIdentifier { get; set; } = Environment.GetEnvironmentVariable("BROWSERSTACK_LOCAL_IDENTIFIER");

        /// <summary>
        ///     Create a instance of the selected browser if configured 
        ///     appsettings.browserstack.json
        /// </summary>
        /// <param name="browserName"></param>
        public Browser(string browserName)
        {
            var configFile = "appsettings.browserstack.json";

            if (!File.Exists(configFile))
            {
                var exampleConfig = @"
{
  ""_comment"": ""https://www.browserstack.com/automate/capabilities"",
  ""DefaultTimeout"": 30,
  ""BrowserstackLocal"": true,
  ""Project"": ""BaseHooks Test"",
  ""Environment"": ""LocalHorst"",
  ""Project"": ""Enter your project"",
  ""Environment"": ""Enter your Environment"",
  ""BROWSERSTACK_USERNAME"": ""yourUserName"",
  ""BROWSERSTACK_ACCESS_KEY"": ""**********"",
  ""Browsers"": [
        {
          ""Name"": ""Chrome"",
          ""AccuracyInPromille"": 930,
          ""isDesktop"":  true,
          ""Capabilities"":{
              ""browser"": ""Chrome"",
              ""os"": ""Windows"",
              ""os_version"": ""10"",
              ""resolution"": ""1920x1080""
            }
        }
    ]
}";
                File.WriteAllText(configFile, exampleConfig);
                Status = $"The configuration file '{configFile}' did not exist. It was now created with an example. Please complete the configuration.";
            }

            var config = JObject.Parse(File.ReadAllText(configFile));

            #region Read the base configuration
                if (config.ContainsKey("DefaultTimeout"))
                {
                    DefaultTimeout = (int)config["DefaultTimeout"];
                }
                if (config.ContainsKey("BrowserstackLocal"))
                {
                    BrowserstackLocal = (bool)config["BrowserstackLocal"];
                }
                if (config.ContainsKey("Project") && BrowserstackProject == "")
                {
                    BrowserstackProject = (string)config["BrowserstackProject"];
                }
                if (config.ContainsKey("Environment") && BrowserstackEnvironment == "")
                {
                    BrowserstackEnvironment = (string)config["Environment"];
                }
                if (config.ContainsKey("BROWSERSTACK_USERNAME") && BrowserstackUser == "")
                {
                    BrowserstackUser = (string)config["BROWSERSTACK_USERNAME"];
                }
                if (config.ContainsKey("BROWSERSTACK_ACCESS_KEY") && BrowserstackKey == "")
                {
                    BrowserstackKey = (string)config["BROWSERSTACK_ACCESS_KEY"];
                }
            #endregion

            IEnumerable<JToken> result = from browser in config["Browsers"].Children()
                                         where (string)browser["Name"] == browserName
                                         select browser;

            if (result.Any())
            {
                JToken browserConfig = result.FirstOrDefault();

                AccuracyInPromille = (int)(browserConfig["AccuracyInPromille"] ?? 1000);
                IsDesktop = (bool)(browserConfig["isDesktop"] ?? false);

                var capabilities = new DesiredCapabilities();
                capabilities.SetCapability("browserstack.user", BrowserstackUser);
                capabilities.SetCapability("browserstack.key", BrowserstackKey);
                capabilities.SetCapability("project", BrowserstackProject);
                capabilities.SetCapability("name", BrowserstackEnvironment);
                capabilities.SetCapability("browserstack.console", "errors");
                capabilities.SetCapability("browserstack.networkLogs", "false");
                if (BrowserstackLocalIdentifier != null)
                {
                    capabilities.SetCapability("browserstack.localIdentifier", BrowserstackLocalIdentifier);
                }
                IEnumerable<JToken> capabilityConfig = from capability in browserConfig["Capabilities"]
                                                       select capability;
                foreach (JProperty capability in capabilityConfig)
                {
                    capabilities.SetCapability(capability.Name, (string)capability.Value);
                }

                Driver = new RemoteWebDriver(
                    new Uri("http://hub-cloud.browserstack.com/wd/hub/"), capabilities, TimeSpan.FromSeconds(DefaultTimeout)
                );
                Status = $"OK - {browserName}";
            }
            else
            {
                var firefoxOptions = new FirefoxOptions();
                firefoxOptions.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                firefoxOptions.SetLoggingPreference(LogType.Server, LogLevel.Off);
                firefoxOptions.SetLoggingPreference(LogType.Client, LogLevel.Off);
                firefoxOptions.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                firefoxOptions.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                firefoxOptions.LogLevel = FirefoxDriverLogLevel.Error;
                firefoxOptions.AcceptInsecureCertificates = true;
                firefoxOptions.PageLoadStrategy = PageLoadStrategy.Eager;
                firefoxOptions.AddArguments("-purgecaches", "-private", "--disable-gpu", "--disable-direct-write", "--disable-display-color-calibration", "--allow-http-screen-capture", "--disable-accelerated-2d-canvas");
                if (Headless != "false")
                {
                    firefoxOptions.AddArguments("-headless");
                }

                IsDesktop = true;
                Driver = new FirefoxDriver("./", firefoxOptions, TimeSpan.FromSeconds(DefaultTimeout));
                Status = $"Firefox - '{browserName}' not found in the config '{configFile}'";
            }

            if (IsDesktop)
            {
                try
                {
                    Driver.Manage().Window.Maximize();
                }
                catch (Exception)
                {
                    // if not supported by the device
                }
            }
        }

        /// <summary>
        ///     Dispose the WebDriver, NOT the browser object
        /// </summary>
        public void DisposeDriver()
        {
            try
            {
                Driver.Close();
                Driver.Dispose();
                Driver.Quit();
            }
            catch (Exception)
            {
                // do nothing, there are some issues at Browserstack with timeouts on TearDown mobile devices
            }
        }
    }
}
