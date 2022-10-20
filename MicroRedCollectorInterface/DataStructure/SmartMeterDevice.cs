using System;
using System.Collections.Concurrent;

namespace DataStructure
{
    public class SmartMeterDevice : Device
    {
        /// <summary>
        /// Puerto para la conexi[on del medidor
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Contrasenha para cierto nivel del medidor
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Tensión del lado de alta al cual está conectado el SM
        /// </summary>
        public double HighVoltage { get; set; }

        /// <summary>
        /// Tensión del lado de baja al cual está conectado el SM
        /// </summary>
        public double LowVoltage { get; set; }

        /// <summary>
        /// Estado del inversor
        /// </summary>
        public SmartMeterDeviceStatusTypes StatusType { get; set; } = SmartMeterDeviceStatusTypes.NotNormal;

        /// <summary>
        /// Si el estado del inversor es válido
        /// </summary>
        public bool IsValid
        {
            get
            {
                bool isValid = false;
                if (StatusType == SmartMeterDeviceStatusTypes.Normal && (DateTime.Now - TimeStamp).TotalMinutes <= 5)
                {
                    isValid = true;
                }
                return isValid;
            }
        }

        /// <summary>
        /// Estampa de tiempo del último pull
        /// </summary>
        public DateTime TimeStamp { get; set; }
        
        /// <summary>
        /// Variables del inversor
        /// </summary>
        public ConcurrentDictionary<string, SmartMeterVariable> Variables { get; set; }

        public SmartMeterDevice()
        {
            Variables = new ConcurrentDictionary<string, SmartMeterVariable>();
        }
    }
}
