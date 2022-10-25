using DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using System.IO;

namespace DataManager
{
    public static class SolarLogManager
    {
        public static List<SolarLogDevice> SolarLogDevices { get; set; }

        private static readonly WinHttpHandler handler = new WinHttpHandler();

        private static readonly HttpClient client = new HttpClient(handler);

        static SolarLogManager()
        {
            SolarLogDevices = new List<SolarLogDevice>();
            var solarLogDevices = DataSource.DBContext.SolarLogDevices;

            foreach (var solarLogDevice in solarLogDevices)
            {
                ConcurrentDictionary<string, SolarLogVariable> dictionary = new ConcurrentDictionary<string, SolarLogVariable>();
                foreach (var variable in solarLogDevice.Device.SolarLogVariables)
                {
                    var tempVariable = new SolarLogVariable()
                    {
                        Name = variable.Name,
                        Unit = variable.Unit,
                        Label = variable.Label
                    };
                    dictionary.TryAdd(variable.Name, tempVariable);
                }
                var tempInverter = new SolarLogDevice()
                {
                    Area = solarLogDevice.Device.Area - 1,
                    Capacity = solarLogDevice.Capacity,
                    DeviceType = DeviceType.SolarLog,
                    HighVoltage = solarLogDevice.HighVoltage,
                    IP = solarLogDevice.Device.IP,
                    LowVoltage = solarLogDevice.LowVoltage,
                    Name = solarLogDevice.Device.Name,
                    NumberOfPanels = (int)solarLogDevice.NumberOfPanels,
                    Password = solarLogDevice.Password,
                    Location = solarLogDevice.Device.Location,
                    UserName = solarLogDevice.UserName,
                    Variables = dictionary
                };
                SolarLogDevices.Add(tempInverter);
            }
        }

        public static void ReadValues(object parameters)
        {
            foreach (var inverter in SolarLogDevices)
            {
                _ = ReadValue(inverter);
            }
        }

        private static async Task ReadValue(SolarLogDevice inverter)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(inverter.CompleteRequest),
                    Content = new StringContent("", Encoding.UTF8),
                };
                var response = await client.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    inverter.StatusType = SolarLogDeviceStatusTypes.Normal;
                }
                if (inverter.StatusType == SolarLogDeviceStatusTypes.Normal)
                {
                    JObject jsonBody1 = JObject.Parse(GetBodyString(inverter.CompleteRequest, inverter.Body1));
                    JObject jsonBody2 = JObject.Parse(GetBodyString(inverter.CompleteRequest, inverter.Body2));
                    JObject jsonBody3 = JObject.Parse(GetBodyString(inverter.CompleteRequest, inverter.Body3));

                    inverter.TimeStamp = DateTime.Now;
                    foreach (var variable in inverter.Variables)
                    {
                        if (!string.IsNullOrEmpty(variable.Value.Label))
                        {
                            var array = variable.Value.Label.Split(',');
                            if (array[0].Equals("Body1"))
                            {
                                variable.Value.LastValue = double.Parse(jsonBody1["801"]["170"][array[1]].ToString());
                            }
                            else if (array[0].Equals("Body2"))
                            {
                                variable.Value.LastValue = double.Parse(jsonBody2["782"][array[1]].ToString());
                            }
                            else if (array[0].Equals("Body3"))
                            {
                                var leng = jsonBody3["776"]["0"].Count() - 1;
                                variable.Value.LastValue = double.Parse(jsonBody3["776"]["0"][leng][1][0][1].ToString());
                            }
                        }
                        else
                        {
                            variable.Value.LastValue = inverter.Variables[variable.Key.Substring(0, 3)].LastValue *
                                inverter.HighVoltage / inverter.LowVoltage;
                        }
                    }
                }

                var epochTime = DataSource.ConvertDateTimeToUnixEpoch(DateTime.Now);

                Parallel.ForEach(inverter.Variables, variable =>
                {
                    var collection = string.Format(DataSource.MongoDBCollectionFormat, inverter.Name, variable.Value.Name);
                    var value = variable.Value.LastValue;
                    DataSource.InsertDocumentMongoDB(collection, value, epochTime);
                });

                var values = new Dictionary<string, double>();
                foreach (var variable in inverter.Variables)
                {
                    values.Add(variable.Value.Name, variable.Value.LastValue);
                }
                DataSource.InsertDocumentInfluxDBLocal(inverter.Name, values, epochTime);
                DataSource.InsertTopicKafka(inverter.Name, values, epochTime);
                DataSource.PreparePatchToOrion(inverter.Name, values);
            }
            catch (Exception)
            {
                inverter.StatusType = SolarLogDeviceStatusTypes.NotNormal;
                Console.WriteLine("SolarLog NOT read " + inverter.IP + " " + inverter.Location);
            }
        }

        private static string GetBodyString(string url, string body)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = body;
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }

        }
    }
}
