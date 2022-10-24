using System;
using System.Collections.Concurrent;

namespace DataStructure
{
    public class SolarLogDevice : Device
    {
        /// <summary>
        /// Petición base para cada inversor, la IP se asocia al Manager
        /// </summary>
        private readonly string BaseRequest = "http://{0}/getjp";
        
        /// <summary>
        /// Petición para este inversor
        /// </summary>
        public string CompleteRequest
        {
            get
            {
                string request = string.Format(BaseRequest, IP);
                return request;
            }
        }

        /// <summary>
        /// String para el cuerpo de la petición 1
        /// </summary>
        public readonly string Body1 = "{\"801\":{\"170\":null}}";

        /// <summary>
        /// String para el cuerpo de la petición 2
        /// </summary>
        public readonly string Body2 = "{\"608\":null,\"609\":null,\"782\":null,\"858\":null,\"862\":null,\"863\":null}";

        /// <summary>
        /// String para el cuerpo de la petición 3
        /// </summary>
        public readonly string Body3 = "{\"141\":{\"32000\":{\"145\":null,\"148\":null,\"149\":null}}," +
            "\"480\":null,\"776\":{\"0\":null},\"801\":{\"100\":null}}";

        /// <summary>
        /// Usuario para realizar la conexipon en el equipo
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Contrasenha del usuario utilizado
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Capacidad del inversor [kWp]
        /// </summary>
        public double Capacity { get; set; }

        /// <summary>
        /// Numero de paneles, usado para calcular la eficiencia del arreglo
        /// </summary>
        public int NumberOfPanels { get; set; }

        /// <summary>
        /// Tensión del lado de alta al cual está conectado el SM
        /// </summary>
        public double HighVoltage { get; set; }

        /// <summary>
        /// Tensión del lado de baja al cual está conectado el SM
        /// </summary>
        public double LowVoltage { get; set; }

        /// <summary>
        /// Eficiencia del equipo
        /// </summary>
        public double Efficiency { get; set; }

        /// <summary>
        /// Estado del inversor
        /// </summary>
        public SolarLogDeviceStatusTypes StatusType { get; set; } = SolarLogDeviceStatusTypes.NotNormal;
        
        /// <summary>
        /// Si el estado del inversor es válido
        /// </summary>
        public bool IsValid
        {
            get
            {
                bool isValid = false;
                if (StatusType == SolarLogDeviceStatusTypes.Normal && (DateTime.Now - TimeStamp).TotalMinutes <= 5)
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
        public ConcurrentDictionary<string, SolarLogVariable> Variables { get; set; }
        
        public SolarLogDevice()
        {
            Variables = new ConcurrentDictionary<string, SolarLogVariable>();
        }

    }
}
