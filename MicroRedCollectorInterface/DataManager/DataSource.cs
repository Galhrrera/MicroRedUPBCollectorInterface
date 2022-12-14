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

        public static readonly string fiware_ip = ConfigurationManager.AppSettings["fiware_host"];


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
        public static void PreparePatchToOrion(string collection, Dictionary<string, double> values)
        {
            FiwareEntity entity = new FiwareEntity(values);
            PatchToOrion(entity, collection);
        }

        //Realizar PATCH en Orion
        public static async void PatchToOrion(FiwareEntity ObjectToSend, string id)
        {

            //Console.WriteLine("-- Validar nuevos sensores: ");
            //Console.WriteLine("-- ID: " + id);
            //Console.WriteLine("-- Data: ");


            HttpClient cliente = new HttpClient()
            {
                BaseAddress = new Uri(fiware_ip)
            };

            var json = JsonConvert.SerializeObject(ObjectToSend.Atributos, formatting: Formatting.Indented);

            /*
            if (id != "BESS_BIBL_BatteryMonitor" && id != "BESS_BIBL_Inverter1_Phase1" && id != "BESS_BIBL_Inverter2_Phase2" && id != "BESS_BIBL_Inverter3_Phase3" && 
                id != "DM_B11_ING" && id != "FR1_B11_20" && id != "FR1_B18_10" && id != "FR1_B18_12.5" && id != "FR2_B11_20" && id != "FR2_B18_10" && id != "FR2_B18_12.5" && 
                id != "calculation" && id != "MODBUS_FR1_B11_20" && id != "MODBUS_FR1_B18_10" && id != "MODBUS_FR1_B18_12.5" && id != "MODBUS_FR2_B11_20" && 
                id != "MODBUS_FR2_B18_10" && id != "MODBUS_FR2_B18_12.5" && id != "ECA01" && id != "ECA02" && id != "EC03" && id != "ECA04" && id != "ECA05" && 
                id != "ECA06" && id != "WS_CUC" && id != "WS_EAFIT_LLANOGRANDE" && id != "WS_ISA" && id != "WS_UNAL_LAPAZ" && id != "WS_UNIGUAJIRA" && id != "WS_UNISUCRE" && 
                id != "WS_UPB" && id != "WS_UPB_BUCARAMANGA" && id != "WS_UPB_PALMIRA" && id != "SM_B10_ARQ" && id != "SM_B12_DERE" && id != "SM_B15_BIBL" && id != "SM_B7_POLI" && 
                id != "SM_B18_PARQ" && id != "SM_B3_RECT" && id != "SM_B4_PRIM" && id != "SM_B5_BACH" && id != "SM_7_CTIC" && id != "SM_B7_TAC" && id != "SM_B8_AA" && 
                id != "SM_B8_CPA" && id != "SM_B8_LABS" && id != "SM_B9_SFA1" && id != "SM_B9_SFA2" && id != "SM_HABITAT" && id != "SL_B11_28")
            {
                
                
            }
            */

            if (id.Contains("SENSOR"))
            {
                Console.WriteLine("-- Validar nuevos sensores: \n");
                Console.WriteLine("-- ID: " + id);
                Console.WriteLine(json);
                Console.WriteLine();
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage respuesta = await cliente.PostAsync(
                "v2/entities/" + id + "/attrs", content);

                respuesta.EnsureSuccessStatusCode();
                var jsonResponse = await respuesta.Content.ReadAsStringAsync();
                if (id.Contains("SENSOR"))
                {
                    Console.WriteLine(respuesta);
                    Console.WriteLine();
                    Console.WriteLine(jsonResponse);
                }

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //throw new Exception("Error al actualizar la entidad: " + id + " con error: " + e.Message);
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
