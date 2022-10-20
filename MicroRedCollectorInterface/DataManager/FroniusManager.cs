using DataStructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace DataManager
{
    /// <summary>
    /// Debe existir uno por cada DataLogger, y cada uno tener inversores asociados
    /// </summary>
    public static class FroniusManager
    {
        public static List<FroniusDevice> FroniusDevices { get; set; }

        private static readonly HttpClient client = new HttpClient();

        static FroniusManager()
        {
            FroniusDevices = new List<FroniusDevice>();
            var froniusDevices = DataSource.DBContext.FroniusDevices;
            
            foreach (var froniusDevice in froniusDevices)
            {
                ConcurrentDictionary<string, FroniusVariable> dictionary = new ConcurrentDictionary<string, FroniusVariable>();
                foreach (var variable in froniusDevice.Device.FroniusVariables)
                {
                    var tempVariable = new FroniusVariable()
                    {
                        Name = variable.Name,
                        Unit = variable.Unit,
                        JSONName = variable.JsonName
                    };
                    dictionary.TryAdd(variable.Name, tempVariable);
                }
                var tempInverter = new FroniusDevice()
                {
                    Area = froniusDevice.Device.Area - 1,
                    Capacity = froniusDevice.Capacity,
                    DeviceType = DeviceType.Fronius,
                    InverterNumber = froniusDevice.InverterNumber,
                    IP = froniusDevice.Device.IP,
                    Location = froniusDevice.Device.Location,
                    Name = froniusDevice.Device.Name,
                    NumberOfPanels = (int)froniusDevice.NumberOfPanels,
                    Variables = dictionary
                };
                FroniusDevices.Add(tempInverter);
            }
        }

        public static void ReadValues(object parameters)
        {
            foreach (var inverter in FroniusDevices)
            {
                ReadValue(inverter);
            }
        }

        private static void ReadValue(FroniusDevice inverter)
        {
            try
            {
                var responseString = client.GetStringAsync(inverter.CompleteRequest).Result;
                JObject json = JObject.Parse(responseString);

                double statusTypeValue = 10;
                var statusCode = json["Body"]["Data"]["DeviceStatus"];
                if (statusCode != null)
                {
                    double.TryParse(json["Body"]["Data"]["DeviceStatus"]["StatusCode"].ToString(), out statusTypeValue);
                }

                if (statusTypeValue >= 0 && statusTypeValue <= 6)
                {
                    inverter.StatusType = FroniusDeviceStatusType.StartUp;
                }
                else if (statusTypeValue == 7)
                {
                    inverter.StatusType = FroniusDeviceStatusType.Running;
                }
                else if (statusTypeValue == 8)
                {
                    inverter.StatusType = FroniusDeviceStatusType.StandBy;
                }
                else if (statusTypeValue == 9)
                {
                    inverter.StatusType = FroniusDeviceStatusType.BootLoading;
                }
                else if (statusTypeValue == 10)
                {
                    inverter.StatusType = FroniusDeviceStatusType.Error;
                }
                if (inverter.StatusType == FroniusDeviceStatusType.Running)
                {
                    inverter.TimeStamp = DateTime.Now;
                    foreach (var variable in inverter.Variables)
                    {
                        if (json["Body"]["Data"][variable.Value.JSONName] != null)
                        {
                            variable.Value.LastValue = double.Parse(json["Body"]["Data"][variable.Value.JSONName]["Value"].ToString());
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
            }
            catch (Exception)
            {
                inverter.StatusType = FroniusDeviceStatusType.Error;
            }
        }

    }
}
