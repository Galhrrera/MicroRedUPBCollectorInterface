using DataStructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace DataManager
{
    public static class FroniusDataManagerManager
    {
        public static List<FroniusDataManagerDevice> FroniusDataManagerDevices { get; set; }

        private static readonly HttpClient client = new HttpClient();

        static FroniusDataManagerManager()
        {
            FroniusDataManagerDevices = new List<FroniusDataManagerDevice>();
            var froniusDataManagersDevices = DataSource.DBContext.FroniusDataManagerDevices;

            foreach (var froniusDataManagerDevice in froniusDataManagersDevices)
            {
                ConcurrentDictionary<string, FroniusDataManagerVariable> dictionary = new ConcurrentDictionary<string, FroniusDataManagerVariable>();
                foreach (var variable in froniusDataManagerDevice.Device.FroniusDataManagerVariables)
                {
                    var tempVariable = new FroniusDataManagerVariable()
                    {
                        Name = variable.Name,
                        Unit = variable.Unit,
                        VectorPosition = variable.VectorPosition
                    };
                    dictionary.TryAdd(variable.Name, tempVariable);
                }
                var tempInverter = new FroniusDataManagerDevice()
                {
                    Area = froniusDataManagerDevice.Device.Area - 1,
                    DeviceType = DeviceType.FroniusDataManager,
                    IP = froniusDataManagerDevice.Device.IP,
                    Location = froniusDataManagerDevice.Device.Location,
                    Name = froniusDataManagerDevice.Device.Name,
                    Variables = dictionary
                };
                FroniusDataManagerDevices.Add(tempInverter);
            }
        }

        public static void ReadValues(object parameters)
        {
            foreach (var dataManager in FroniusDataManagerDevices)
            {
                ReadValue(dataManager);
            }
        }

        private static void ReadValue(FroniusDataManagerDevice dataManager)
        {
            try
            {
                var responseString = client.GetStringAsync(dataManager.CompleteRequest).Result;
                JObject json = JObject.Parse(responseString);
                double.TryParse(json["Head"]["Status"]["Code"].ToString(), out double statusTypeValue);
                if (statusTypeValue == 0)
                {
                    dataManager.StatusType = FroniusDeviceStatusType.Running;
                }
                if (dataManager.StatusType == FroniusDeviceStatusType.Running)
                {
                    dataManager.TimeStamp = DateTime.Now;
                    foreach (var variable in dataManager.Variables)
                    {
                        variable.Value.LastValue = double.Parse(json["Body"]["Data"]["1"][variable.Value.VectorPosition.ToString()]["Value"].ToString());
                    }
                }

                var epochTime = DataSource.ConvertDateTimeToUnixEpoch(DateTime.Now);

                Parallel.ForEach(dataManager.Variables, variable =>
                {
                    var collection = string.Format(DataSource.MongoDBCollectionFormat, dataManager.Name, variable.Value.Name);
                    var value = variable.Value.LastValue;
                    DataSource.InsertDocumentMongoDB(collection, value, epochTime);
                });

                var values = new Dictionary<string, double>();
                foreach (var variable in dataManager.Variables)
                {
                    values.Add(variable.Value.Name, variable.Value.LastValue);
                }
                DataSource.InsertDocumentInfluxDBLocal(dataManager.Name, values, epochTime);
                DataSource.InsertTopicKafka(dataManager.Name, values, epochTime);
            }
            catch (Exception)
            {
                dataManager.StatusType = FroniusDeviceStatusType.Error;
            }
        }

    }
}
