using DataStructure;
using EasyModbus;
using MoreLinq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DataManager
{
    public static class ModbusManager
    {
        public static List<ModbusDevice> ModbusDevices { get; set; }

        static ModbusManager()
        {
            ModbusDevices = new List<ModbusDevice>();
            var modbusDevices = DataSource.DBContext.ModbusDevices;

            var _IPDevices = modbusDevices.DistinctBy(x => x.Device.IP);

            foreach (var _IPDevice in _IPDevices)
            {
                List<ModbusUnit> tempUnits = new List<ModbusUnit>();
                foreach (var modbusDevice in modbusDevices)
                {
                    if (_IPDevice.Device.IP.Equals(modbusDevice.Device.IP))
                    {
                        ConcurrentDictionary<string, ModbusVariable> dictionary = new ConcurrentDictionary<string, ModbusVariable>();
                        foreach (var variable in modbusDevice.Device.ModbusVariables)
                        {
                            double.TryParse(variable.Offset, out double offset);
                            offset = offset != double.NaN ? offset : 0;
                            double.TryParse(variable.Scale, out double scale);
                            scale = scale != double.NaN ? scale : 1;
                            var temp = new ModbusVariable
                            {
                                Name = variable.Name,
                                Offset = offset,
                                Bytes = int.Parse(variable.Bytes),
                                Scale = scale,
                                StartRegister = int.Parse(variable.StartRegister),
                                Unit = variable.Unit,
                                RegisterType = (RegisterType)variable.RegisterType
                            };
                            dictionary.TryAdd(variable.Name, temp);
                        }
                        var unit = new ModbusUnit
                        {
                            Identifier = (byte)modbusDevice.UnitID,
                            Name = modbusDevice.Device.Name,
                            Variables = dictionary,
                        };
                        tempUnits.Add(unit);
                    }
                }

                ScaleClass modbusDataType = ScaleClass.Mode1;
                if (_IPDevice.ScaleClass == 1)
                {
                    modbusDataType = ScaleClass.Mode1;
                }
                else if (_IPDevice.ScaleClass == 2)
                {
                    modbusDataType = ScaleClass.Mode2;
                }

                var tempModbusDevice = new ModbusDevice
                {
                    Area = _IPDevice.Device.Area,
                    DeviceType = DeviceType.Modbus,
                    IP = _IPDevice.Device.IP,
                    ScaleClass = modbusDataType,
                    Name = string.Format("Modbus_{0}", _IPDevice.Device.IP),
                    Port = _IPDevice.Port,
                    Location = _IPDevice.Device.Location,
                    Units = tempUnits,
                    Controllable = _IPDevice.Device.Controllable,
                    OrderClass = !_IPDevice.OrderClass ? OrderClass.BigEndian : OrderClass.LittleEndian
                };
                ModbusDevices.Add(tempModbusDevice);
            }
        }

        public static void ReadValues(object parameters)
        {
            Parallel.ForEach(ModbusDevices, modbusDevice =>
            {
                ReadValue(modbusDevice);
            });
        }

        private static void ReadValue(ModbusDevice modbus)
        {
            try
            {
                foreach (var unit in modbus.Units)
                {
                    ModbusClient modbusClient = new ModbusClient()
                    {
                        IPAddress = modbus.IP,
                        Port = modbus.Port,
                        UnitIdentifier = unit.Identifier,
                        ConnectionTimeout = 15000
                    };
                    modbusClient.Connect();

                    var epochTime = DataSource.ConvertDateTimeToUnixEpoch(DateTime.Now);
                    foreach (var variable in unit.Variables)
                    {
                        try
                        {
                            double value = double.NaN;
                            double scalingFactor = double.NaN;
                            bool[] registers = new bool[] { false};
                            int[] registers1 = new int[0];
                            switch (variable.Value.RegisterType)
                            {
                                case RegisterType.Coil:
                                    registers = modbusClient.ReadCoils(variable.Value.StartRegister, 1);
                                    break;
                                case RegisterType.DiscreteInput:
                                    registers = modbusClient.ReadDiscreteInputs(variable.Value.StartRegister, 1);
                                    break;
                                case RegisterType.HoldingRegister:
                                    registers1 = modbusClient.ReadHoldingRegisters(variable.Value.StartRegister, variable.Value.Bytes);
                                    break;
                                case RegisterType.InputRegister:
                                    registers1 = modbusClient.ReadInputRegisters(variable.Value.StartRegister, variable.Value.Bytes);
                                    break;
                            }

                            if (variable.Value.RegisterType == RegisterType.HoldingRegister || variable.Value.RegisterType == RegisterType.InputRegister)
                            {
                                ModbusClient.RegisterOrder registerOrder = modbus.OrderClass == OrderClass.BigEndian ? ModbusClient.RegisterOrder.LowHigh: ModbusClient.RegisterOrder.HighLow;

                                if (variable.Value.Bytes == 1)
                                {
                                    value = registers1[0];
                                }
                                else if (variable.Value.Bytes == 2)
                                {
                                    value = ModbusClient.ConvertRegistersToInt(registers1, registerOrder);
                                }
                                else if (variable.Value.Bytes == 4)
                                {
                                    value = ModbusClient.ConvertRegistersToLong(registers1, registerOrder);
                                }
                                switch (modbus.ScaleClass)
                                {
                                    case ScaleClass.Mode1:
                                        if (!double.IsNaN(variable.Value.Scale))
                                        {
                                            registers1 = modbusClient.ReadHoldingRegisters((int)variable.Value.Scale, 1);
                                            scalingFactor = Math.Pow(10, registers1[0]);
                                            value *= scalingFactor;
                                        }
                                        break;
                                    case ScaleClass.Mode2:
                                        value /= variable.Value.Scale;
                                        break;
                                }
                            }
                            else
                            {
                                value = registers[0] ? 1 : 0;
                            }

                            value += variable.Value.Offset;
                            variable.Value.LastValue = value;
                        }
                        catch (Exception ex)
                        {
                            modbus.StatusType = ModbusDeviceStatusTypes.NotNormal;
                            var message = string.Format("Unit not read {0}, {2}. {1}",
                                modbus.Name, ex.ToString(), unit.Name);
                            Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Modbus);
                        }
                        finally
                        {
                            var collection = string.Format(DataSource.MongoDBCollectionFormat, unit.Name, variable.Value.Name);
                            DataSource.InsertDocumentMongoDB(collection, variable.Value.LastValue, epochTime);
                        }
                    }

                    var values = new Dictionary<string, double>();
                    foreach (var variable in unit.Variables)
                    {
                        values.Add(variable.Value.Name, variable.Value.LastValue);
                    }
                    DataSource.InsertDocumentInfluxDBLocal(unit.Name, values, epochTime);
                    DataSource.InsertDocumentInfluxDB(unit.Name, values, epochTime);
                    DataSource.InsertTopicKafka(unit.Name, values, epochTime);
                    DataSource.PreparePatchToOrion(unit.Name, values);

                    modbusClient.Disconnect();
                }
                modbus.StatusType = ModbusDeviceStatusTypes.Normal;
                modbus.TimeStamp = DateTime.Now;
            }
            catch (Exception ex)
            {
                modbus.StatusType = ModbusDeviceStatusTypes.NotNormal;
                Console.WriteLine("Modbus NOT read " + modbus.Name);
                var message = string.Format("Modbus not read {0}. {1}", modbus.Name, ex.ToString());
                Logging.Logger.Logging(message, EventLogEntryType.Error, Logging.Logger.LoggerEventID.Modbus);
            }
        }

    }
}
