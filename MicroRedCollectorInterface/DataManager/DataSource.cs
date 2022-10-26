using Confluent.Kafka;
using DataStructure;
using DBModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using MoreLinq;
using System.Linq;
using System.Net.Http;
using System.Text;
using Gurux.DLMS.Enums;

namespace DataManager
{
    public static class DataSource
    {
        private static MongoClient MongoDBClient { get; set; }

        private static InfluxDBClient InfluxDBLocalClient { get; set; }

        private static InfluxDBClient InfluxDBClient { get; set; }

        private static IProducer<Null, string> KafkaProducer { get; set; }

        public static IMongoDatabase MongoDataBase { get; set; }

        public static CollectorInterfaceDBEntities DBContext { get { return new CollectorInterfaceDBEntities(); } }

        public static readonly string MongoDBCollectionFormat = "{0}___{1}";

        public static readonly string SmartGridCollectionFormat = "{0}_{1}";

        public static readonly string SmartGrid = ConfigurationManager.AppSettings["MicroGrid"];

        //private static List<string> entity_ids = new List<string>() { "DM_B11_ING" };

        static DataSource()
        {
            InitializeMongoDB();
            InitializeInfluxDB();
            InitializeInfluxDBLocal();
            InitializeKafkaProducer();

            MonitoringMongoDBConnection();
            MonitoringInfluxDBConnection();
            MonitoringInfluxDBLocalConnection();
            MonitoringKafkaConnection();
        }

        private static void InitializeMongoDB()
        {
            Logging.Logger.Logging("Connecting to MongoDB DataBase", EventLogEntryType.Information, Logging.Logger.LoggerEventID.Mongo);
            try
            {
                MongoDBClient = new MongoClient(ConfigurationManager.AppSettings["MongoDBHost"]);
                MongoDataBase = MongoDBClient.GetDatabase(ConfigurationManager.AppSettings["MongoDBDataBase"]);
                Logging.Logger.Logging("Connected to MongoDB DataBase", EventLogEntryType.Information, Logging.Logger.LoggerEventID.Mongo);
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to connect to MongoDB DataBase. {0}", ex.ToString());
                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Mongo);
            }
        }

        private static void InitializeInfluxDBLocal()
        {
            Logging.Logger.Logging("Connecting to local InfluxDB DataBase", EventLogEntryType.Information, Logging.Logger.LoggerEventID.InfluxDB);
            try
            {
                InfluxDBLocalClient = InfluxDBClientFactory.Create(ConfigurationManager.AppSettings["InfluxDBLocalHost"], ConfigurationManager.AppSettings["InfluxDBLocalToken"]);
                Logging.Logger.Logging("Connected to local InfluxDB DataBase", EventLogEntryType.Information, Logging.Logger.LoggerEventID.InfluxDB);
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to connect to local InfluxDB DataBase. {0}", ex.ToString());
                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
            }

        }

        private static void InitializeInfluxDB()
        {
            Logging.Logger.Logging("Connecting to InfluxDB DataBase", EventLogEntryType.Information, Logging.Logger.LoggerEventID.InfluxDB);
            try
            {
                InfluxDBClient = InfluxDBClientFactory.Create(ConfigurationManager.AppSettings["InfluxDBHost"], ConfigurationManager.AppSettings["InfluxDBToken"]);
                Logging.Logger.Logging("Connected to InfluxDB DataBase", EventLogEntryType.Information, Logging.Logger.LoggerEventID.InfluxDB);
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to connect to InfluxDB DataBase. {0}", ex.ToString());
                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
            }

        }

        private static void InitializeKafkaProducer()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = ConfigurationManager.AppSettings["kafkaHost"],
                ClientId = "CollectorInterface"
            };
            Logging.Logger.Logging("Connecting to Kafka", EventLogEntryType.Information, Logging.Logger.LoggerEventID.Kafka);
            try
            {
                KafkaProducer = new ProducerBuilder<Null, string>(config).Build();
                Logging.Logger.Logging("Connected to Kafka", EventLogEntryType.Information, Logging.Logger.LoggerEventID.Kafka);
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to connect to Kafka. {0}", ex.ToString());
                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Kafka);
            }
        }

        //Inicialización

        private static void MonitoringMongoDBConnection()
        {
            new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(5000);
                        if (MongoDBClient == null ||
                            (MongoDBClient != null && MongoDBClient.Cluster.Description.State == MongoDB.Driver.Core.Clusters.ClusterState.Disconnected))
                        {
                            try
                            {

                                Logging.Logger.Logging("Error MongoDB DataBase disconnected. Trying to reconnect.", EventLogEntryType.Error, Logging.Logger.LoggerEventID.Mongo);
                                InitializeMongoDB();
                            }
                            catch (Exception ex)
                            {
                                var message = string.Format("Error reconnecting MongoDB DataBase Monitor. {0}", ex.ToString());
                                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Mongo);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("Error MongoDB DataBase Monitor. {0}", ex.ToString());
                    Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Mongo);
                }
            }).Start();
        }

        private static void MonitoringInfluxDBLocalConnection()
        {
            new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(5000);
                        if (InfluxDBLocalClient != null)
                        {
                            using (var influxDBClientHealth = InfluxDBLocalClient.HealthAsync())
                            {
                                var status = influxDBClientHealth.Result.Status;
                                if (status == HealthCheck.StatusEnum.Fail)
                                {
                                    try
                                    {
                                        Logging.Logger.Logging("Error local InfluxDB DataBase disconnected. Trying to reconnect.", EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                                        InitializeInfluxDBLocal();
                                    }
                                    catch (Exception ex)
                                    {
                                        var message = string.Format("Error reconnecting local InfluxDB DataBase Monitor. {0}", ex.ToString());
                                        Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                                    }
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                Logging.Logger.Logging("Error InfluxDB DataBase disconnected. Trying to reconnect.", EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                                InitializeInfluxDBLocal();
                            }
                            catch (Exception ex)
                            {
                                var message = string.Format("Error reconnecting InfluxDB DataBase Monitor. {0}", ex.ToString());
                                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("Error InfluxDB DataBase Monitor. {0}", ex.ToString());
                    Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                }
            }).Start();
        }

        private static void MonitoringInfluxDBConnection()
        {
            new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(5000);
                        if (InfluxDBClient != null)
                        {
                            using (var influxDBClientHealth = InfluxDBClient.HealthAsync())
                            {
                                var status = influxDBClientHealth.Result.Status;
                                if (status == HealthCheck.StatusEnum.Fail)
                                {
                                    try
                                    {
                                        Logging.Logger.Logging("Error InfluxDB DataBase disconnected. Trying to reconnect.", EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                                        InitializeInfluxDB();
                                    }
                                    catch (Exception ex)
                                    {
                                        var message = string.Format("Error reconnecting InfluxDB DataBase Monitor. {0}", ex.ToString());
                                        Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                                    }
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                Logging.Logger.Logging("Error InfluxDB DataBase disconnected. Trying to reconnect.", EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                                InitializeInfluxDB();
                            }
                            catch (Exception ex)
                            {
                                var message = string.Format("Error reconnecting InfluxDB DataBase Monitor. {0}", ex.ToString());
                                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("Error InfluxDB DataBase Monitor. {0}", ex.ToString());
                    Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                }
            }).Start();
        }

        private static void MonitoringKafkaConnection()
        {
            new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(5000);
                        if (KafkaProducer != null)
                        {
                            var statusValue = KafkaProducer.ProduceAsync("Status", new Message<Null, string> { Value = "true" });
                            if (statusValue != null)
                            {
                                if (KafkaProducer != null && statusValue.Result.Status != PersistenceStatus.Persisted)
                                {
                                    try
                                    {
                                        Logging.Logger.Logging("Error Kafka disconnected. Trying to reconnect.", EventLogEntryType.Error, Logging.Logger.LoggerEventID.Kafka);
                                        InitializeKafkaProducer();
                                    }
                                    catch (Exception ex)
                                    {
                                        var message = string.Format("Error reconnecting Kafka Monitor. {0}", ex.ToString());
                                        Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Kafka);
                                    }
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                Logging.Logger.Logging("Error Kafka disconnected. Trying to reconnect.", EventLogEntryType.Error, Logging.Logger.LoggerEventID.Kafka);
                                InitializeKafkaProducer();
                            }
                            catch (Exception ex)
                            {
                                var message = string.Format("Error reconnecting Kafka Monitor. {0}", ex.ToString());
                                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Kafka);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("Error InfluxDB DataBase Monitor. {0}", ex.ToString());
                    Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.InfluxDB);
                }
            }).Start();
        }

        //Monitoreo de la conexión

        public static void InsertDocumentMongoDB(string collection, double value, long epochTime)
        {
            var MongoDBCollection = MongoDataBase.GetCollection<MongoDBVariable>(collection);
            MongoDBCollection.InsertOneAsync(new MongoDBVariable
            {
                timestamp = epochTime,
                value = value
            });
        }

        public static void InsertDocumentInfluxDBLocal(string collection, Dictionary<string, double> values, long epochTime)
        {
            var dateTime = ConvertUnixEpochToDateTime(epochTime);
            var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            utcDateTime = utcDateTime.AddHours(10);

            //Validar los valores, y la entidad
            Console.WriteLine("DATOS QUE SE ENVIAN A INFLUXDDB:");
            Console.WriteLine("El string collection es: " + collection);

            foreach (var dic in values)
            {
                Console.WriteLine("la clave del diccionario es: {0}, y el valor es {1}", dic.Key, dic.Value);
            }

            //FIN DE LA VALIDACIÓN

            using (var writeApi = InfluxDBLocalClient.GetWriteApi())
            {
                var points = new List<PointData>();
                foreach (var value in values)
                {
                    var point = PointData.Measurement(collection)
                       .Field(value.Key, value.Value)
                       .Timestamp(utcDateTime, WritePrecision.Ns);
                    points.Add(point);
                }

                writeApi.WritePoints(ConfigurationManager.AppSettings["InfluxDBLocalDataBase"], ConfigurationManager.AppSettings["InfluxDBLocalOrg"], points);
            }
        }

        public static void InsertDocumentInfluxDB(string collection, Dictionary<string, double> values, long epochTime)
        {
            var dateTime = ConvertUnixEpochToDateTime(epochTime);
            var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            utcDateTime = utcDateTime.AddHours(10);

            using (var writeApi = InfluxDBClient.GetWriteApi())
            {
                var points = new List<PointData>();
                foreach (var value in values)
                {
                    var point = PointData.Measurement(string.Format(SmartGridCollectionFormat, SmartGrid, collection))
                       .Field(value.Key, value.Value)
                       .Timestamp(utcDateTime, WritePrecision.Ns);
                    points.Add(point);
                }

                writeApi.WritePoints(ConfigurationManager.AppSettings["InfluxDBDataBase"], ConfigurationManager.AppSettings["InfluxDBOrg"], points);
            }
        }

        //Preparar datos para Orion
        public static void PreparePatchToOrion(string collection, Dictionary<string, double> values){
            //Colocar la lógica
            
            if (collection == "DM_B11_ING")
            {
                FiwareDM DMAttr;

                bool hasvalueRad = values.TryGetValue("Radiation",out double valueRad);
                bool hasvalueTemp1 = values.TryGetValue("Temperature1", out double valueTemp1);
                bool hasvalueTemp2 = values.TryGetValue("Temperature2", out double valueTemp2);

                if (hasvalueRad && hasvalueTemp1 && hasvalueTemp2)
                {
                    DMAttr = new FiwareDM(valueRad, valueTemp1, valueTemp2);
                    PatchToOrion(DMAttr, collection);
                }
                else
                {
                    Console.WriteLine("Alguno de los valores de variables para FiwareDM están vacíos");
                    throw new Exception("Alguno de los valores de variables para FiwareDM están vacíos");
                }
            }

            else if(collection == "MODBUS_FR1_B11_20" || collection == "MODBUS_FR1_B18_10" || collection == "MODBUS_FR1_B18_12.5" || collection == "MODBUS_FR2_B11_20" ||
                collection == "MODBUS_FR2_B18_10" || collection == "MODBUS_FR2_B18_12.5")
            {

                FiwareModbus ModbusAttr;

                bool hasApparentPower = values.TryGetValue("ApparentPower", out double valueApparentPower);
                bool hasEnergyTotal = values.TryGetValue("EnergyTotal", out double valueEnergyTotal);
                bool hasOutPFSet = values.TryGetValue("OutPFSet", out double valueOutPFSet);
                bool hasOutPFSet_Ena = values.TryGetValue("OutPFSet_Ena", out double valueOutPFSet_Ena);
                bool hasOutPFSet_RmpTms = values.TryGetValue("OutPFSet_RmpTms", out double valueOutPFSet_RmpTms);
                bool hasOutPFSet_RvrtTms = values.TryGetValue("OutPFSet_RvrtTms", out double valueOutPFSet_RvrtTms);
                bool hasOutPFSet_WinTms = values.TryGetValue("OutPFSet_WinTms", out double valueOutPFSet_WinTms);
                bool hasPAC = values.TryGetValue("PAC", out double valuePAC);
                bool hasReactivePower = values.TryGetValue("ReactivePower", out double valueReactivePower);
                bool hasTotalPowerFactor = values.TryGetValue("TotalPowerFactor", out double valueTotalPowerFactor);
                bool hasV1 = values.TryGetValue("V1", out double valueV1);
                bool hasV2 = values.TryGetValue("V2", out double valueV2);
                bool hasV3 = values.TryGetValue("V3", out double valueV3);
                bool hasvWMaxLim_Ena = values.TryGetValue("WMaxLim_Ena", out double valueWMaxLim_Ena);
                bool hasWMaxLimPct = values.TryGetValue("WMaxLimPct", out double valueWMaxLimPct);
                bool hasWMaxLimPct_RmpTms = values.TryGetValue("WMaxLimPct_RmpTms", out double valueWMaxLimPct_RmpTms);
                bool hasWMaxLimPct_RvrtTms = values.TryGetValue("WMaxLimPct_RvrtTms", out double valueWMaxLimPct_RvrtTms);
                bool hsaWMaxLimPct_WinTms = values.TryGetValue("WMaxLimPct_WinTms", out double valueWMaxLimPct_WinTms);

                if (hasApparentPower && hasEnergyTotal && hasOutPFSet && hasOutPFSet_Ena && hasOutPFSet_RmpTms && hasOutPFSet_RvrtTms && hasOutPFSet_WinTms &&
                    hasPAC && hasReactivePower && hasTotalPowerFactor && hasV1 && hasV2 && hasV3 && hasvWMaxLim_Ena && hasWMaxLimPct && hasWMaxLimPct_RmpTms &&
                    hasWMaxLimPct_RvrtTms && hsaWMaxLimPct_WinTms)
                {
                    ModbusAttr = new FiwareModbus(valueApparentPower, valueEnergyTotal, valueOutPFSet, valueOutPFSet_Ena, valueOutPFSet_RmpTms, valueOutPFSet_RvrtTms,
                        valueOutPFSet_WinTms, valuePAC, valueReactivePower, valueTotalPowerFactor, valueV1, valueV2, valueV3, valueWMaxLim_Ena, valueWMaxLimPct, valueWMaxLimPct_RmpTms,
                        valueWMaxLimPct_RvrtTms, valueWMaxLimPct_WinTms);

                    PatchToOrion(ModbusAttr, collection);
                }
                else
                {
                    Console.WriteLine("Alguno de las variables de MODBUS está nula o incorrecta");
                    throw new Exception("Alguno de los valores para las variables de MODBUS no es correcto o es null");
                }


                string entity_id = collection.Replace("MODBUS_", ""); //Dispositivos FR / solo reciben, hasta ahora, energía total
                FiwareFronius FroniusAttr;
                /*
                bool hasvalueEnergyDay = values.TryGetValue("EnergyDay", out double valueEnergyDay);
                bool hasvalueEnergyTotal = values.TryGetValue("EnergyTotal", out double valueEnergyTotal);
                bool hasvalueEnergyYear = values.TryGetValue("EnergyYear", out double valueEnergyYear);
                bool hasvalueFrequency = values.TryGetValue("Frequency", out double valueFrequency);
                bool hasvalueIAC = values.TryGetValue("IAC", out double valueIAC);
                bool hasvalueIDC = values.TryGetValue("IDC", out double valueIDC);
                bool hasvaluePAC = values.TryGetValue("PAC", out double valuePAC);
                bool hasvalueVAC = values.TryGetValue("VAC", out double valueVAC);
                bool hasvalueVDC = values.TryGetValue("VDC", out double valueVDC);

                if (hasvalueEnergyDay && hasvalueEnergyTotal && hasvalueEnergyYear && hasvalueFrequency && hasvalueIAC && 
                    hasvalueIDC && hasvaluePAC && hasvalueVAC && hasvalueVDC)
                {
                    FroniusAttr = new FiwareFronius(valueEnergyDay, valueEnergyTotal, valueEnergyYear, valueFrequency, valueIAC, valueIDC, valuePAC, valueVAC, valueVDC);
                    PatchToOrion(FroniusAttr, entity_id);
                }
                else
                {
                    Console.WriteLine("Alguno de los valores de variables para FiwareFronius están vacíos");
                    throw new Exception("Alguno de los valores de variables para Fronius están vacíos");
                }*/
                bool hasvalueEnergyTotalFR = values.TryGetValue("EnergyTotal", out double valueEnergyTotalFR);

                if (hasvalueEnergyTotalFR)
                {
                    FroniusAttr = new FiwareFronius(valueEnergyTotalFR);
                    PatchToOrion(FroniusAttr, entity_id);
                }
                else
                {
                    Console.WriteLine("Valor de la variable para Fronius no está completo o es null");
                }


            }
            else if (collection == "BESS_BIBL_Inverter1_Phase1" || collection == " BESS_BIBL_Inverter2_Phase2" || collection == "BESS_BIBL_Inverter3_Phase3")
            {
                FiwareBessInv BessInvAttr;

                bool hasvalueBatteryChargeActive = values.TryGetValue("BatteryChargeActive", out double valueBatteryChargeActive);
                bool hasvalueBatteryChargeActiveDay = values.TryGetValue("BatteryChargeActiveDay", out double valueBatteryChargeActiveDay);
                bool hasvalueBatteryChargeActiveMonth = values.TryGetValue("BatteryChargeActiveMonth", out double valueBatteryChargeActiveMonth);
                bool hasvalueBatteryCurrent = values.TryGetValue("BatteryCurrent", out double valueBatteryCurrent);
                bool hasvalueBatteryDischargeActive = values.TryGetValue("BatteryDischargeActive", out double valueBatteryDischargeActive);
                bool hasvalueBatteryDischargeActiveDay = values.TryGetValue("BatteryDischargeActiveDay", out double valueBatteryDischargeActiveDay);
                bool hasvalueBatteryDischargeActiveMonth = values.TryGetValue("BatteryDischargeActiveMonth", out double valueBatteryDischargeActiveMonth);
                bool hasvalueBatteryPower = values.TryGetValue("BatteryPower", out double valueBatteryPower);
                bool hasvalueBatteryVoltage = values.TryGetValue("BatteryVoltage", out double valueBatteryVoltage);
                bool hasvalueChargeDCCurrent = values.TryGetValue("ChargeDCCurrent", out double valueChargeDCCurrent);
                bool hasvalueChargeDCPower = values.TryGetValue("ChargeDCPower", out double valueChargeDCPower);
                bool hasvalueChargeDCPowerPercentage = values.TryGetValue("ChargeDCPowerPercentage", out double valueChargeDCPowerPercentage);
                bool hasvalueChargerEnabled = values.TryGetValue("ChargerEnabled", out double valueChargerEnabled);
                bool hasvalueChargerStatus = values.TryGetValue("ChargerStatus", out double valueChargerStatus);
                bool hasvalueDeviceState = values.TryGetValue("DeviceState", out double valueDeviceState);
                bool hasvalueEnergyFromBattery = values.TryGetValue("EnergyFromBattery", out double valueEnergyFromBattery);
                bool hasvalueEnergyFromBatteryDay = values.TryGetValue("EnergyFromBatteryDay", out double valueEnergyFromBatteryDay);
                bool hasvalueEnergyFromBatteryMonth = values.TryGetValue("EnergyFromBatteryMonth", out double valueEnergyFromBatteryMonth);
                bool hasvalueEnergyToBattery = values.TryGetValue("EnergyToBattery", out double valueEnergyToBattery);
                bool hasvaueEnergyToBatteryDay = values.TryGetValue("EnergyToBatteryDay", out double valueEnergyToBatteryDay);
                bool hasvalueEnergyToBatteryMonth = values.TryGetValue("EnergyToBatteryMonth", out double valueEnergyToBatteryMonth);
                bool hasvalueForcedSell = values.TryGetValue("ForcedSell", out double valueForcedSell);
                bool hasvalueGridACCurrent = values.TryGetValue("GridACCurrent", out double valueGridACCurrent);
                bool hasvalueGridACFrequency = values.TryGetValue("GridACFrequency", out double valueGridACFrequency);
                bool hasvalueGridACInputCurrent = values.TryGetValue("GridACInputCurrent", out double valueGridACInputCurrent);
                bool hasvalueGridACInputPowerApparent = values.TryGetValue("GridACInputPowerApparent", out double valueGridACInputPowerApparent);
                bool hasvalueGridACInputVoltage = values.TryGetValue("GridACInputVoltage", out double valueGridACInputVoltage);
                bool hasvalueGridACL1Current = values.TryGetValue("GridACL1Current", out double valueGridACL1Current);
                bool hasvalueGridACL1Voltage = values.TryGetValue("GridACL1Voltage", out double valueGridACL1Voltage);
                bool hasvalueGridACPower = values.TryGetValue("GridACPower", out double valueGridACPower);
                bool hasvalueGridACVoltage = values.TryGetValue("GridACVoltage", out double valueGridACVoltage);
                bool hasvalueGridInputActive = values.TryGetValue("GridInputActive", out double valueGridInputActive);
                bool hasvalueGridInputActiveDay = values.TryGetValue("GridInputActiveDay", out double valueGridInputActiveDay);
                bool hasvalueGridInputActiveMonth = values.TryGetValue("GridInputActiveMonth", out double valueGridInputActiveMonth);
                bool hasvalueGridInputEnergy = values.TryGetValue("GridInputEnergy", out double valueGridInputEnergy);
                bool hasvalueGridInputEnergyDay = values.TryGetValue("GridInputEnergyDay", out double valueGridInputEnergyDay);
                bool hasvalueGridInputEnergyMonth = values.TryGetValue("GridInputEnergyMonth", out double valueGridInputEnergyMonth);
                bool hasvalueGridOutputActive = values.TryGetValue("GridOutputActive", out double valueGridOutputActive);
                bool hasvalueGridOutputActiveDay = values.TryGetValue("GridOutputActiveDay", out double valueGridOutputActiveDay);
                bool hasvalueGridOutputActiveMonth = values.TryGetValue("GridOutputActiveMonth", out double valueGridOutputActiveMonth);
                bool hasvaueGridOutputCurrent = values.TryGetValue("GridOutputCurrent", out double valueGridOutputCurrent);
                bool hasvalueGridOutputEnergy = values.TryGetValue("GridOutputEnergy", out double valueGridOutputEnergy);
                bool hasvalueGridOutputEnergyDay = values.TryGetValue("GridOutputEnergyDay", out double valueGridOutputEnergyDay);
                bool hasvalueGridOutputEnergyMonth = values.TryGetValue("GridOutputEnergyMonth", out double valueGridOutputEnergyMonth);
                bool hasvalueGridOutputFrequency = values.TryGetValue("GridOutputFrequency", out double valueGridOutputFrequency);
                bool hasvalueGridOutputPower = values.TryGetValue("GridOutputPower", out double valueGridOutputPower);
                bool hasvalueGridOutputPowerApparent = values.TryGetValue("GridOutputPowerApparent", out double valueGridOutputPowerApparent);
                bool hasvalueGridOutputVoltage = values.TryGetValue("GridOutputVoltage", out double valueGridOutputVoltage);
                bool hasvalueInverterDCCurrent = values.TryGetValue("InverterDCCurrent", out double valueInverterDCCurrent);
                bool hasvalueInverterDCPower = values.TryGetValue("InverterDCPower", out double valueInverterDCPower);
                bool hasvalueInverterEnabled = values.TryGetValue("InverterEnabled", out double valueInverterEnabled);
                bool hasvaluesInverterStatus = values.TryGetValue("InverterStatus", out double valueInverterStatus);
                bool hasvalueLoadACCurrent = values.TryGetValue("LoadACCurrent", out double valueLoadACCurrent);
                bool hasvalueLoadACFrequency = values.TryGetValue("LoadACFrequency", out double valueLoadACFrequency);
                bool hasvalueLoadACL1Current = values.TryGetValue("LoadACL1Current", out double valueLoadACL1Current);
                bool hasvalueLoadACL1Voltage = values.TryGetValue("LoadACL1Voltage", out double valueLoadACL1Voltage);
                bool hasvalueLoadACPower = values.TryGetValue("LoadACPower", out double valueLoadACPower);
                bool hasvalueLoadACPowerApparent = values.TryGetValue("LoadACPowerApparent", out double valueLoadACPowerApparent);
                bool hasvalueLoadACVoltage = values.TryGetValue("LoadACVoltage", out double valueLoadACVoltage);
                bool hasvalueLoadOutputActive = values.TryGetValue("LoadOutputActive", out double valueLoadOutputActive);
                bool hasvalueLoadOutputActiveDay = values.TryGetValue("LoadOutputActiveDay", out double valueLoadOutputActiveDay);
                bool hasvalueLoadOutputActiveMonth = values.TryGetValue("LoadOutputActiveMonth", out double valueLoadOutputActiveMonth);
                bool hasvalueLoadOutputEnergy = values.TryGetValue("LoadOutputEnergy", out double valueLoadOutputEnergy);
                bool hasvalueLoadOutputEnergyDay = values.TryGetValue("LoadOutputEnergyDay", out double valueLoadOutputEnergyDay);
                bool hasvalueLoadOutputEnergyMonth = values.TryGetValue("LoadOutputEnergyMonth", out double valueLoadOutputEnergyMonth);
                bool hasvalueSellEnabled = values.TryGetValue("SellEnabled", out double valueSellEnabled);

                if(hasvalueBatteryChargeActive && hasvalueBatteryChargeActiveDay && hasvalueBatteryChargeActiveMonth && hasvalueBatteryCurrent && hasvalueBatteryDischargeActive
                    && hasvalueBatteryDischargeActiveDay && hasvalueBatteryDischargeActiveMonth && hasvalueBatteryPower && hasvalueBatteryVoltage && hasvalueChargeDCCurrent
                    && hasvalueChargeDCPower && hasvalueChargeDCPowerPercentage && hasvalueChargerEnabled && hasvalueChargerStatus && hasvalueDeviceState && hasvalueEnergyFromBattery
                    && hasvalueEnergyFromBatteryDay && hasvalueEnergyFromBatteryMonth && hasvalueEnergyToBattery && hasvaueEnergyToBatteryDay && hasvalueEnergyToBatteryMonth
                    && hasvalueForcedSell && hasvalueGridACCurrent && hasvalueGridACFrequency && hasvalueGridACInputCurrent && hasvalueGridACInputPowerApparent && hasvalueGridACInputVoltage
                    && hasvalueGridACL1Current && hasvalueGridACL1Voltage && hasvalueGridACPower && hasvalueGridACVoltage && hasvalueGridInputActive && hasvalueGridInputActiveDay && hasvalueGridInputActiveMonth
                    && hasvalueGridInputEnergy && hasvalueGridInputEnergyDay && hasvalueGridInputEnergyMonth && hasvalueGridOutputActive && hasvalueGridOutputActiveDay && hasvalueGridOutputActiveMonth
                    && hasvaueGridOutputCurrent && hasvalueGridOutputEnergy && hasvalueGridOutputEnergyDay && hasvalueGridOutputEnergyMonth && hasvalueGridOutputFrequency && hasvalueGridOutputPower
                    && hasvalueGridOutputPowerApparent && hasvalueGridOutputVoltage && hasvalueInverterDCCurrent && hasvalueInverterDCPower && hasvalueInverterEnabled && hasvaluesInverterStatus
                    && hasvalueLoadACCurrent && hasvalueLoadACFrequency && hasvalueLoadACL1Current && hasvalueLoadACL1Voltage && hasvalueLoadACPower && hasvalueLoadACPowerApparent && hasvalueLoadACVoltage
                    && hasvalueLoadOutputActive && hasvalueLoadOutputActiveDay && hasvalueLoadOutputActiveMonth && hasvalueLoadOutputEnergy && hasvalueLoadOutputEnergyDay && hasvalueLoadOutputEnergyMonth
                    && hasvalueSellEnabled)
                {
                    BessInvAttr = new FiwareBessInv(valueBatteryChargeActive, valueBatteryChargeActiveDay, valueBatteryChargeActiveMonth, valueBatteryCurrent, valueBatteryDischargeActive,
                        valueBatteryDischargeActiveDay, valueBatteryDischargeActiveMonth, valueBatteryPower, valueBatteryVoltage, valueChargeDCCurrent, valueChargeDCPower, valueChargeDCPowerPercentage,
                        valueChargerEnabled, valueChargerStatus, valueDeviceState, valueEnergyFromBattery, valueEnergyFromBatteryDay, valueEnergyFromBatteryMonth, valueEnergyToBattery, valueEnergyToBatteryDay,
                        valueEnergyToBatteryMonth, valueForcedSell, valueGridACCurrent, valueGridACFrequency, valueGridACInputCurrent, valueGridACInputPowerApparent, valueGridACInputVoltage, valueGridACL1Current,
                        valueGridACL1Voltage, valueGridACPower, valueGridACVoltage, valueGridInputActive, valueGridInputActiveDay, valueGridInputActiveMonth, valueGridInputEnergy, valueGridInputEnergyDay,
                        valueGridInputEnergyMonth, valueGridOutputActive, valueGridOutputActiveDay, valueGridOutputActiveMonth, valueGridOutputCurrent, valueGridOutputEnergy, valueGridOutputEnergyDay,
                        valueGridOutputEnergyMonth, valueGridOutputFrequency, valueGridOutputPower, valueGridOutputPowerApparent, valueGridOutputVoltage, valueInverterDCCurrent, valueInverterDCPower,
                        valueInverterEnabled, valueInverterStatus, valueLoadACCurrent, valueLoadACFrequency, valueLoadACL1Current, valueLoadACL1Voltage, valueLoadACPower, valueLoadACPowerApparent,
                        valueLoadACVoltage, valueLoadOutputActive, valueLoadOutputActiveDay, valueLoadOutputActiveMonth, valueLoadOutputEnergy, valueLoadOutputEnergyDay, valueLoadOutputEnergyMonth, valueSellEnabled);

                    PatchToOrion(BessInvAttr, collection);
                }
                else
                {
                    Console.WriteLine("Alguno de las variables de BESS INV está nula o incorrecta");
                    throw new Exception("Alguno de los valores para las variables de BESS_INV no es correcto o es null");
                }
            }

        }

        //Realizar PATCH en Orion
        public static async void PatchToOrion(FiwareDevice ObjetToSend, string id)
        {
            HttpClient cliente = new HttpClient()
            {
                BaseAddress = new Uri("http://10.61.3.135:1026") //Colocar en app.config una vez todo esté listo
            };

            var json = JsonConvert.SerializeObject(ObjetToSend);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage respuesta = await cliente.PostAsync(
                "v2/entities/" + id + "/attrs", content);

                respuesta.EnsureSuccessStatusCode();
                Console.WriteLine(respuesta.Content + " Para el entity_id: "+id);
                var jsonResponse = await respuesta.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Error al actualizar la entidad: " + id+" con error: "+ e.Message);
            }
        }



        public static void InsertTopicKafka(string collection, Dictionary<string, double> values, long epochTime)
        {
            if (KafkaProducer != null)
            {
                var fields = new List<string>();
                values.Add("timestamp", epochTime);

                foreach (var value in values)
                {
                    fields.Add(value.Key);
                }

                var schema = new Schema
                {
                    Fields = fields,
                    Name = collection,
                    Type = "struct"
                };

                var kafkaStructure = new KafkaStructure
                {
                    Payload = values,
                    Schema = schema
                };

                string topic = string.Format(SmartGridCollectionFormat, SmartGrid, collection);
                KafkaProducer.ProduceAsync(topic, new Message<Null, string> { Value = JsonConvert.SerializeObject(kafkaStructure) });
            }
        }

        public static DateTime ConvertUnixEpochToDateTime(double ms)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dateTime.ToLocalTime().AddMilliseconds(ms);
        }

        public static long ConvertDateTimeToUnixEpoch(DateTime dateTime)
        {
            long epoch = (long)(dateTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return epoch;
        }

    }

    public class GetDifferenceMongo
    {
        public double GetValueDifference(string collection, DateTime dateTime, ValueDifferenceType valueDifferenceType)
        {
            var MongoDBCollection = DataSource.MongoDataBase.GetCollection<MongoDBVariable>(collection);

            DateTime dateTimePast = dateTime;

            switch (valueDifferenceType)
            {
                case ValueDifferenceType.OneWeek:
                    dateTimePast = dateTime.AddDays(-7);
                    break;
                case ValueDifferenceType.TwoWeeks:
                    dateTimePast = dateTime.AddDays(-15);
                    break;
                case ValueDifferenceType.ThreeWeeks:
                    dateTimePast = dateTime.AddDays(-21);
                    break;
                case ValueDifferenceType.OneMonth:
                    dateTimePast = dateTime.AddMonths(-1).AddHours(-8);
                    break;
            }
            long dateTimeEpochPast = DataSource.ConvertDateTimeToUnixEpoch(dateTimePast);
            long dateTimeEpochPresent = DataSource.ConvertDateTimeToUnixEpoch(dateTime.AddHours(-8));

            double presentValue, pastValue;

            try
            {
                var queryPast = from d in MongoDBCollection.AsQueryable()
                                where d.timestamp >= dateTimeEpochPast
                                select d;

                var queryPresent = from d in MongoDBCollection.AsQueryable()
                                   where d.timestamp >= dateTimeEpochPresent
                                   select d;

                if (queryPast.Count() > 0 && queryPresent.Count() > 0)
                {
                    var pastMongoDBVariable = queryPast.OrderBy(n => n.timestamp, OrderByDirection.Ascending).FirstOrDefault();
                    var presentMongoDBVariable = queryPresent.OrderBy(n => n.timestamp, OrderByDirection.Ascending).FirstOrDefault();

                    var pastDate = DataSource.ConvertUnixEpochToDateTime(pastMongoDBVariable.timestamp);
                    var presentDate = DataSource.ConvertUnixEpochToDateTime(presentMongoDBVariable.timestamp);

                    pastValue = pastMongoDBVariable.value;
                    presentValue = presentMongoDBVariable.value;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }

            if (presentValue >= pastValue)
            {
                return presentValue - pastValue;
            }
            else
            {
                int i = 0;
                while (true)
                {
                    if (pastValue / Math.Pow(10, i) < 1)
                    {
                        break;
                    }
                    i++;
                }
                return Math.Pow(10, i) - pastValue + presentValue;
            }
        }

    }

}
