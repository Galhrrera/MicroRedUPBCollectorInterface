using System;

namespace DataStructure
{
    public class Variable
    {
        /// <summary>
        /// Nombre de la variable
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Último valor de la variable
        /// </summary>
        public double LastValue { get; set; }

        /// <summary>
        /// Unidad de la variable
        /// </summary>
        public string Unit { get; set; }

    }
    
    public class SmartMeterVariable : Variable
    {
        public string Registry { get; set; }
    }

    public class FroniusVariable : Variable
    {
        /// <summary>
        /// String en el JSON que devuelve la API de Fronius
        /// </summary>
        public string JSONName { get; set; }
    }

    public class FroniusDataManagerVariable : Variable
    {
        /// <summary>
        /// Posición en el vector
        /// </summary>
        public int VectorPosition { get; set; }
    }

    public class EnphaseVariable : Variable
    {
        public string MicroInverterNumber { get; set; }
    }

    public class SolarLogVariable : Variable
    {
        public string Label { get; set; }
    }

    public class UbidotsVariable : Variable
    {
        /// <summary>
        /// Estampa de tiempo del último valor
        /// </summary>
        public DateTime TimeStamp { get; set; }

        public string ID_Ubidots { get; set; }

        public bool IsValid
        {
            get
            {
                bool isValid = false;
                if ((DateTime.Now - TimeStamp).TotalMinutes <= 5)
                {
                    isValid = true;
                }
                return isValid;
            }
        }

    }

    public class ModbusVariable : Variable
    {
        /// <summary>
        /// Dirección inicial del Holding Register
        /// </summary>
        public int StartRegister { get; set; }

        /// <summary>
        /// Número de bytes del Holding Register
        /// </summary>
        public int Bytes { get; set; }

        /// <summary>
        /// Factor de escala del Holding Register
        /// </summary>
        public double Scale { get; set; }

        /// <summary>
        /// Offset de la variable, no todas lo tienen
        /// </summary>
        public double Offset { get; set; } = 0;

        /// <summary>
        /// Tipo de registro
        /// Coil = 0,
        /// DiscreteInput = 1,
        /// HoldingRegister = 2,
        /// InputRegister = 3
        /// </summary>
        public RegisterType RegisterType { get; set; }
    }

    public enum RegisterType
    {
        Coil = 0,
        DiscreteInput = 1,
        HoldingRegister = 2,
        InputRegister = 3
    }


}
