using DataStructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gurux.DLMS;
using Gurux.DLMS.Enums;
using Gurux.Net;
using Gurux.DLMS.Reader;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;

namespace DataManager
{
    public static class SmartMeterManager
    {
        public static List<SmartMeterDevice> SmartMeters { get; set; }

        static SmartMeterManager()
        {
            SmartMeters = new List<SmartMeterDevice>();
            var smartMeterDevices = DataSource.DBContext.SmartMeterDevices;

            foreach (var smartMeterDevice in smartMeterDevices)
            {
                ConcurrentDictionary<string, SmartMeterVariable> dictionary = new ConcurrentDictionary<string, SmartMeterVariable>();
                foreach (var variable in smartMeterDevice.Device.SmartMeterVariables)
                {
                    var tempVariable = new SmartMeterVariable()
                    {
                        Name = variable.Name,
                        Unit = variable.Unit,
                        Registry = variable.Registry
                    };
                    dictionary.TryAdd(variable.Name, tempVariable);
                }

                var tempSmartMeter = new SmartMeterDevice()
                {
                    Area = smartMeterDevice.Device.Area - 1,
                    DeviceType = DeviceType.SmartMeter,
                    HighVoltage = smartMeterDevice.HighVoltage,
                    IP = smartMeterDevice.Device.IP,
                    LowVoltage = smartMeterDevice.LowVoltage,
                    Name = smartMeterDevice.Device.Name,
                    Password = smartMeterDevice.Password,
                    Port = smartMeterDevice.Port,
                    Location = smartMeterDevice.Device.Location,
                    Variables = dictionary
                };

                SmartMeters.Add(tempSmartMeter);
            }
        }

        private class Settings
        {
            public GXNet media = new GXNet();
            public TraceLevel trace = TraceLevel.Info;
            public GXDLMSClient client = new GXDLMSClient(true);
            public GXDLMSReader reader = null;
        }

        public static void ReadValues(object parameters)
        {
            foreach (var smartMeter in SmartMeters)
            {
                ThreadPool.QueueUserWorkItem(ReadValue, smartMeter);
                Random random = new Random();
                Thread.Sleep(random.Next(10, 50));
            }
        }
        
        private static void ReadValue(object parameters)
        {
            SmartMeterDevice smartMeter = parameters as SmartMeterDevice;
            Settings settings = new Settings();
            lock (settings.media.SyncRoot)
            {
                try
                {
                    settings.media.Protocol = NetworkType.Tcp;
                    settings.media.Port = smartMeter.Port;
                    settings.media.HostName = smartMeter.IP;

                    settings.client.Password = Encoding.ASCII.GetBytes(smartMeter.Password);
                    settings.client.ServerAddress = 1;
                    settings.client.ClientAddress = 32;
                    settings.client.Authentication = Authentication.Low;
                    settings.client.InterfaceType = InterfaceType.HDLC;
                    settings.client.UseLogicalNameReferencing = false;

                    settings.reader = new GXDLMSReader(settings.client, settings.media, settings.trace);

                    settings.media.Open();

                    //Some meters need a break here.
                    Thread.Sleep(1000);
                    settings.reader.InitializeConnection();
                    settings.reader.GetAssociationView(false);
                    foreach (var variable in smartMeter.Variables)
                    {
                        if (!string.IsNullOrEmpty(variable.Value.Registry))
                        {
                            settings.reader.GetValueByObisCode(variable.Value.Registry, out double value);
                            variable.Value.LastValue = value;
                        }
                        else
                        {
                            variable.Value.LastValue = smartMeter.Variables[variable.Key.Substring(0, 2)].LastValue *
                                smartMeter.HighVoltage / smartMeter.LowVoltage;
                        }
                    }
                    smartMeter.TimeStamp = DateTime.Now;
                    smartMeter.StatusType = SmartMeterDeviceStatusTypes.Normal;
                    settings.media.Close();

                    var epochTime = DataSource.ConvertDateTimeToUnixEpoch(DateTime.Now);
                    Parallel.ForEach(smartMeter.Variables, variable =>
                    {
                        var collection = string.Format(DataSource.MongoDBCollectionFormat, smartMeter.Name, variable.Value.Name);
                        var value = variable.Value.LastValue;
                        DataSource.InsertDocumentMongoDB(collection, value, epochTime);
                    });

                    var values = new Dictionary<string, double>();
                    foreach (var variable in smartMeter.Variables)
                    {
                        values.Add(variable.Value.Name, variable.Value.LastValue);
                    }
                    DataSource.InsertDocumentInfluxDBLocal(smartMeter.Name, values, epochTime);
                    DataSource.InsertTopicKafka(smartMeter.Name, values, epochTime);
                    DataSource.PreparePatchToOrion(smartMeter.Name, values);

                    settings.reader.Close();
                }
                catch (Exception ex)
                {
                    smartMeter.StatusType = SmartMeterDeviceStatusTypes.NotNormal;
                    Console.WriteLine("Meter NOT read " + smartMeter.Name);
                    var message = string.Format("Meter not read {0}. {1}", smartMeter.Name, ex.ToString());
                    Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.SmartMeter);
                    try
                    {
                        using (PrimS.Telnet.TcpClient tcpClient = new PrimS.Telnet.TcpClient(smartMeter.IP, smartMeter.Port))
                        {
                            tcpClient.Close();
                            tcpClient.Dispose();
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                }
                finally
                {
                    if (settings.reader != null)
                    {
                        settings.reader.Close();
                    }
                }
            }
            return;
        }
    }
}
