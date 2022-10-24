using DataStructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DataManager
{
    public static class UbidotsManager
    {
        public static int ValuesAccepted = 0;

        public static int ValuesRejected = 0;

        private static readonly int UbidotsTimerTime = 1000;

        private static readonly string httpPost = "https://industrial.api.ubidots.com/api/v1.6/variables/{0}/values?token={1}";

        public static List<UbidotsDevice> UbidotsDevices { get; set; }

        private static readonly HttpClient client = new HttpClient();

        private static ConcurrentDictionary<string, double> ConcurrentDictionary { get; set; }

        static UbidotsManager()
        {
            ConcurrentDictionary = new ConcurrentDictionary<string, double>();

            UbidotsDevices = new List<UbidotsDevice>();
            var ubidotsDevices = DataSource.DBContext.UbidotsDevices;

            foreach (var ubidotsDevice in ubidotsDevices)
            {
                Dictionary<string, UbidotsVariable> dictionary = new Dictionary<string, UbidotsVariable>();
                foreach (var variable in ubidotsDevice.Device.UbidotsVariables)
                {
                    var tempVariable = new UbidotsVariable()
                    {
                        ID_Ubidots = variable.ID_Ubidots,
                        Name = variable.Name,
                        Unit = variable.Unit
                    };
                    dictionary.Add(variable.Name, tempVariable);
                }
                UbidotsDevices.Add(new UbidotsDevice()
                {
                    Area = ubidotsDevice.Device.Area - 1,
                    IP = ubidotsDevice.Device.IP,
                    DeviceType = DeviceType.Ubidots,
                    Name = ubidotsDevice.Device.Name,
                    Location = ubidotsDevice.Device.Location,
                    Token = ubidotsDevice.Token,
                    Variables = dictionary
                });
            }
        }

        public static void ReadValues(object parameters)
        {
            foreach (var source in UbidotsDevices)
            {
                ReadValue(source);
            }
        }

        private static void ReadValue(UbidotsDevice ubidots)
        {
            try
            {
                foreach (var variable in ubidots.Variables)
                {
                    string url = string.Format(ubidots.BaseRequest, variable.Value.ID_Ubidots, ubidots.Token);
                    var responseString = client.GetStringAsync(url).Result;
                    JObject json = JObject.Parse(responseString);
                    if (json["last_value"]["value"] != null)
                    {
                        double.TryParse(json["last_value"]["value"].ToString(), out double value);
                        variable.Value.LastValue = value;
                        variable.Value.TimeStamp = DataSource.ConvertUnixEpochToDateTime(double.Parse(json["last_value"]["timestamp"].ToString()));
                    }
                }
                
                Parallel.ForEach(ubidots.Variables, variable =>
                {
                    var collection = string.Format(DataSource.MongoDBCollectionFormat, ubidots.Name, variable.Value.Name);
                    var value = variable.Value.LastValue;
                    var epochTime = DataSource.ConvertDateTimeToUnixEpoch(variable.Value.TimeStamp);
                    DataSource.InsertDocumentMongoDB(collection, value, epochTime);
                });

                var values = new Dictionary<string, double>();
                foreach (var variable in ubidots.Variables)
                {
                    values.Add(variable.Value.Name, variable.Value.LastValue);
                }
                DataSource.InsertDocumentInfluxDBLocal(ubidots.Name, values, DataSource.ConvertDateTimeToUnixEpoch(DateTime.Now));
                DataSource.InsertTopicKafka(ubidots.Name, values, DataSource.ConvertDateTimeToUnixEpoch(DateTime.Now));
            }
            catch (Exception)
            {

            }
        }

    }
}
