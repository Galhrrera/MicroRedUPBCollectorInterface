using DataStructure;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DataManager
{
    public static class EnphaseManager
    {
        public static List<DeviceElements> Inverters { get; set; }
        
        public class DeviceElements
        {
            public DeviceElements(EnphaseDevice enphaseDevice)
            {
                EnphaseDevice = enphaseDevice;
            }

            public EnphaseDevice EnphaseDevice { get; set; }

            public ChromeDriver ChromeDriver { get; set; }

        }

        private static readonly string UrlAddressLogin = "https://enlighten.enphaseenergy.com/";

        private static readonly string UrlAddressDevices =
            "https://enlighten.enphaseenergy.com/systems/256274/devices?status=active";

        static EnphaseManager()
        {
            Inverters = new List<DeviceElements>();

            var EnphaseDevices = DataSource.DBContext.EnphaseDevices;

            foreach (var enphaseDevice in EnphaseDevices)
            {
                ConcurrentDictionary<string, EnphaseVariable> dictionary = new ConcurrentDictionary<string, EnphaseVariable>();
                foreach (var variable in enphaseDevice.Device.EnphaseVariables)
                {
                    var tempVariable = new EnphaseVariable()
                    {
                        MicroInverterNumber = variable.MicroInverterNumber,
                        Name = variable.Name,
                        Unit = variable.Unit
                    };
                    dictionary.TryAdd(variable.Name, tempVariable);
                }
                var tempInverter = new EnphaseDevice()
                {
                    Area = enphaseDevice.Device.Area - 1,
                    Capacity = enphaseDevice.Capacity,
                    DeviceType = DeviceType.Enphase,
                    IP = enphaseDevice.Device.IP,
                    Location = enphaseDevice.Device.Location,
                    Name = enphaseDevice.Device.Name,
                    MicroInverters = (int)enphaseDevice.MicroInverters,
                    NumberOfPanels = (int)enphaseDevice.NumberOfPanels,
                    UserName = enphaseDevice.UserName,
                    Variables = dictionary,
                    Password = enphaseDevice.Password
                };
                Inverters.Add(new DeviceElements(tempInverter));
            }
            LoadHTMLPage();
        }

        public static void ReadValues(object parameters)
        {
            foreach (var inverter in Inverters)
            {
                ReadValue(inverter);
                Random random = new Random();
                Thread.Sleep(random.Next(10, 50));
            }
        }

        private static void ReadValue(DeviceElements inverter)
        {
            try
            {
                if (inverter.ChromeDriver != null)
                {
                    inverter.ChromeDriver.Navigate().Refresh();
                    
                    IWebElement envoyStatusElement = inverter.ChromeDriver.FindElement(
                        By.XPath("//*[@id=\"t_emus\"]/tbody/tr/td[7]/a[2]"));
                    string envoyStatusString = envoyStatusElement.Text;
                    EnphaseInverterStatusType enphaseInverterStatusType;

                    if (envoyStatusString.Contains("Normal"))
                    {
                        enphaseInverterStatusType = EnphaseInverterStatusType.Normal;
                    }
                    else if (envoyStatusString.Contains("Comm"))
                    {
                        enphaseInverterStatusType = EnphaseInverterStatusType.Comm;
                    }
                    else if (envoyStatusString.Contains("Power"))
                    {
                        enphaseInverterStatusType = EnphaseInverterStatusType.Power;
                    }
                    else
                    {
                        enphaseInverterStatusType = EnphaseInverterStatusType.NoStatus;
                    }

                    inverter.EnphaseDevice.StatusType = enphaseInverterStatusType;

                    if (enphaseInverterStatusType == EnphaseInverterStatusType.Normal)
                    {
                        int timeCounter = 0;
                        IWebElement microInverters25 = inverter.ChromeDriver.FindElement(
                                By.XPath("//*[@id=\"user_pcu_devices_datatables_length\"]/label/select/option[2]"));
                        microInverters25.Click();
                        Thread.Sleep(1000);

                        IWebElement processingElement = inverter.ChromeDriver.FindElement(
                            By.XPath("//*[@id=\"user_pcu_devices_datatables_processing\"]"));
                        string pageStatus = processingElement.GetAttribute("style");
                        bool isReady = pageStatus.Contains("visibility: hidden;");

                        while (!isReady)
                        {
                            processingElement = inverter.ChromeDriver.FindElement(
                                By.XPath("//*[@id=\"user_pcu_devices_datatables_processing\"]"));
                            pageStatus = processingElement.GetAttribute("style");
                            isReady = pageStatus.Contains("visibility: hidden;");
                            Thread.Sleep(1000);
                            timeCounter++;
                            if (timeCounter >= 10)
                            {
                                return;
                            }
                        }

                        IWebElement totalEnergyElement = inverter.ChromeDriver.FindElement(
                            By.XPath("//*[@id=\"energy_lifetime\"]/div[2]/div/div/span[1]"));
                        var totalEnergyValue = double.Parse(totalEnergyElement.GetAttribute("data-value"));
                        var totalEnergyUnit = totalEnergyElement.GetAttribute("data-units");
                        IWebElement dayEnergyElement = inverter.ChromeDriver.FindElement
                            (By.XPath("//*[@id=\"energy_today\"]/div[2]/div[1]/div/span[1]"));
                        var dayEnergyValue = double.Parse(dayEnergyElement.GetAttribute("data-value"));
                        var dayEnergyUnit = dayEnergyElement.GetAttribute("data-units");
                        IWebElement microInvertersTable = inverter.ChromeDriver.FindElement(
                            By.XPath("//*[@id=\"user_pcu_devices_datatables\"]/tbody"));
                        string serialIDXPath = "//*[@id=\"user_pcu_devices_datatables\"]/tbody/tr[{0}]/td[2]/div/a";
                        string valueXPath = "//*[@id=\"user_pcu_devices_datatables\"]/tbody/tr[{0}]/td[4]";
                        double totalPower = 0;
                        for (int i = 0; i < inverter.EnphaseDevice.MicroInverters; i++)
                        {
                            IWebElement tempWebElementSerialID = inverter.ChromeDriver.FindElement(
                                By.XPath(string.Format(serialIDXPath, i + 1)));
                            var tempSerialID = tempWebElementSerialID.Text;
                            IWebElement tempWebElementValue = inverter.ChromeDriver.FindElement(
                                By.XPath(string.Format(valueXPath, i + 1)));
                            var tempValue = double.Parse(tempWebElementValue.Text);
                            (from d in inverter.EnphaseDevice.Variables
                             where d.Value.MicroInverterNumber.Equals(tempSerialID)
                             select d.Value).First().LastValue = tempValue;
                            totalPower += tempValue;
                        }

                        inverter.EnphaseDevice.Variables.TryGetValue("EnergyDay", out EnphaseVariable tempVariable);
                        tempVariable.LastValue = dayEnergyValue * PrefixManager.GetSufix(dayEnergyUnit, "Wh");

                        inverter.EnphaseDevice.Variables.TryGetValue("EnergyTotal", out tempVariable);
                        tempVariable.LastValue = totalEnergyValue * PrefixManager.GetSufix(totalEnergyUnit, "Wh");

                        inverter.EnphaseDevice.Variables.TryGetValue("PAC", out tempVariable);
                        tempVariable.LastValue = totalPower;

                        inverter.EnphaseDevice.TimeStamp = DateTime.Now;

                        var epochTime = DataSource.ConvertDateTimeToUnixEpoch(DateTime.Now);
                        Parallel.ForEach(inverter.EnphaseDevice.Variables, variable =>
                        {
                            var collection = string.Format(DataSource.MongoDBCollectionFormat, inverter.EnphaseDevice.Name, variable.Value.Name);
                            var value = variable.Value.LastValue;
                            DataSource.InsertDocumentMongoDB(collection, value, epochTime);
                        });

                        var values = new Dictionary<string, double>();
                        foreach (var variable in inverter.EnphaseDevice.Variables)
                        {
                            values.Add(variable.Value.Name, variable.Value.LastValue);
                        }
                        DataSource.InsertDocumentInfluxDBLocal(inverter.EnphaseDevice.Name, values, epochTime);
                        DataSource.InsertTopicKafka(inverter.EnphaseDevice.Name, values, epochTime);
                        DataSource.PreparePatchToOrion(inverter.EnphaseDevice.Name, values);
                    }
                }
            }
            catch (Exception ex)
            {
                CloseAndDisposeChromeDrivers();

                Console.WriteLine("Enphase inverter NOT read " + inverter.EnphaseDevice.Name);
                var message = string.Format("Enphase inverter not read {0}. {1}", inverter.EnphaseDevice.Name, ex.ToString());
                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Enphase);

                LoadHTMLPage();
            }
        }
        
        private static void LoadHTMLPage()
        {
            foreach (var inverter in Inverters)
            {
                try
                {
                    var options = new ChromeOptions();
                    options.AddArgument("headless");
                    ChromeDriverService service = ChromeDriverService.CreateDefaultService("ChromeDriver");
                    service.HideCommandPromptWindow = true;
                    inverter.ChromeDriver = new ChromeDriver(service, options);
                    inverter.ChromeDriver.Navigate().GoToUrl(UrlAddressLogin);
                    IWebElement userMail = inverter.ChromeDriver.FindElement(By.Id("user_email"));
                    userMail.SendKeys(inverter.EnphaseDevice.UserName);
                    IWebElement password = inverter.ChromeDriver.FindElement(By.Id("user_password"));
                    password.SendKeys(inverter.EnphaseDevice.Password);
                    IWebElement SignInButton = inverter.ChromeDriver.FindElement(By.Id("submit"));
                    SignInButton.Click();
                    inverter.ChromeDriver.Navigate().GoToUrl(UrlAddressDevices);
                }
                catch (Exception ex)
                {
                    CloseAndDisposeChromeDrivers();

                    inverter.EnphaseDevice.StatusType = EnphaseInverterStatusType.NoStatus;
                    var message = string.Format("Error loading Enphase Page {0}. {1}", inverter.EnphaseDevice.Name, ex.ToString());
                    Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Enphase);
                }
            }
        }

        public static void CloseAndDisposeChromeDrivers()
        {
            foreach (var inverter in Inverters)
            {
                if (inverter.ChromeDriver != null)
                {
                    try
                    {
                        inverter.ChromeDriver.Quit();
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format("Error quitting ChromeDriver {0}. {1}", inverter.EnphaseDevice.Name, ex.ToString());
                        Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Enphase);
                    }
                }
            }
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "taskkill /f /im \"chromedriver.exe\" /T && taskkill /f /im \"chrome.exe\" /T";
                process.StartInfo = startInfo;
                process.Start();
            }
            catch (Exception ex)
            {
                var message = string.Format("Error quitting ChromeDriver via CMD. {0}", ex.ToString());
                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Enphase);
            }
        }

    }
}
