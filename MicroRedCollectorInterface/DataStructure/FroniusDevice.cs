using System;
using System.Collections.Concurrent;

namespace DataStructure
{
    public class FroniusDevice : Device
    {
        /// <summary>
        /// Petición base para cada inversor
        /// </summary>
        private readonly string BaseRequest = "http://{0}/solar_api/v1/GetInverterRealtimeData.cgi?Scope=Device&DeviceId={1}&DataCollection=CommonInverterData";

        /// <summary>
        /// Petición para este inversor
        /// </summary>
        public string CompleteRequest
        {
            get
            {
                string request = string.Format(BaseRequest, IP, InverterNumber);
                return request;
            }
        }
        
        /// <summary>
        /// Número del inversor, necesario para las consultas
        /// </summary>
        public int InverterNumber { get; set; }

        /// <summary>
        /// Capacidad del inversor [kWp]
        /// </summary>
        public double Capacity { get; set; }

        /// <summary>
        /// Numero de paneles, usado para calcular la eficiencia del arreglo
        /// </summary>
        public int NumberOfPanels { get; set; }

        /// <summary>
        /// Eficiencia del equipo
        /// </summary>
        public double Efficiency { get; set; }

        /// <summary>
        /// Estado del inversor
        /// </summary>
        public FroniusDeviceStatusType StatusType { get; set; }

        /// <summary>
        /// Si el estado del inversor es válido
        /// </summary>
        public bool IsValid
        {
            get
            {
                bool isValid = false;
                if (StatusType == FroniusDeviceStatusType.Running && (DateTime.Now - TimeStamp).TotalMinutes <= 5)
                {
                    isValid = true;
                }
                return isValid;
            }
        }

        /// <summary>
        /// Estampa de tiempo del último valor
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Variables del inversor
        /// </summary>
        public ConcurrentDictionary<string, FroniusVariable> Variables { get; set; }

        public FroniusDevice()
        {
            Variables = new ConcurrentDictionary<string, FroniusVariable>();
        }
    }
}
