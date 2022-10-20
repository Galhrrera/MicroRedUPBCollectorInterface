using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DataStructure
{
    public class ModbusDevice : Device
    {
        /// <summary>
        /// Puerto para la conexión Modbus/TCP
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Unidades asociadas al dispositivo Modbus
        /// </summary>
        public List<ModbusUnit> Units { get; set; } = new List<ModbusUnit>();

        /// <summary>
        /// Si el estado del equipo es válido
        /// </summary>
        public bool IsValid
        {
            get
            {
                bool isValid = false;
                if (StatusType == ModbusDeviceStatusTypes.Normal && (DateTime.Now - TimeStamp).TotalMinutes <= 5)
                {
                    isValid = true;
                }
                return isValid;
            }
        }

        /// <summary>
        /// Estampa de tiempo de la última consulta
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Estado del equipo
        /// </summary>
        public ModbusDeviceStatusTypes StatusType { get; set; }

        /// <summary>
        /// Tipo de escala
        /// </summary>
        public ScaleClass ScaleClass { get; set; }

        /// <summary>
        /// Si es BigEndian o LittleEndian
        /// </summary>
        public OrderClass OrderClass { get; set; }

    }

    public enum ScaleClass
    {
        Mode1, //Mode xbits plus scalingFactor
        Mode2 //Mode xbits constant scalingFactor
    }

    public enum OrderClass
    {
        BigEndian = 0,
        LittleEndian = 1
    }

    public class ModbusUnit
    {
        /// <summary>
        /// Nombre del equipo para la conexión Modbus/TCP
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identificador del equipo para la conexión Modbus/TCP
        /// </summary>
        public byte Identifier { get; set; }

        /// <summary>
        /// Variables del inversor
        /// </summary>
        public ConcurrentDictionary<string, ModbusVariable> Variables { get; set; } = new ConcurrentDictionary<string, ModbusVariable>();


    }
}
