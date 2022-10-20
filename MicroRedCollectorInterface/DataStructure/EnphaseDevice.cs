using System;
using System.Collections.Concurrent;

namespace DataStructure
{
    public class EnphaseDevice : Device
    {
       
        /// <summary>
        /// Usuario del enphase
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Capacidad del inversor [kWp]
        /// </summary>
        public double Capacity { get; set; }
        
        /// <summary>
        /// Numero de microinversores del arreglo
        /// </summary>
        public int MicroInverters { get; set; }

        /// <summary>
        /// Estado del inversor
        /// </summary>
        public EnphaseInverterStatusType StatusType { get; set; }

        /// <summary>
        /// Numero de paneles, usado para calcular la eficiencia del arreglo
        /// </summary>
        public int NumberOfPanels { get; set; }

        /// <summary>
        /// Eficiencia del equipo
        /// </summary>
        public double Efficiency { get; set; }

        /// <summary>
        /// Si el estado del inversor es válido
        /// </summary>
        public bool IsValid
        {
            get
            {
                bool isValid = false;
                if (StatusType == EnphaseInverterStatusType.Normal && (DateTime.Now - TimeStamp).TotalMinutes <= 30)
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
        public ConcurrentDictionary<string, EnphaseVariable> Variables { get; set; }

        public EnphaseDevice()
        {
            Variables = new ConcurrentDictionary<string, EnphaseVariable>();
        }
    }
}
