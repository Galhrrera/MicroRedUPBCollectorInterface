using System;
using System.Collections.Concurrent;

namespace DataStructure
{
    public class FroniusDataManagerDevice : Device
    {
        /// <summary>
        /// Petición base para cada inversor
        /// </summary>
        private readonly string BaseRequest = "http://{0}/solar_api/v1/GetSensorRealtimeData.cgi?DataCollection=NowSensorData&Scope=System";

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
        /// Estado del inversor
        /// </summary>
        public FroniusDeviceStatusType StatusType { get; set; } = FroniusDeviceStatusType.Error;

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
        public ConcurrentDictionary<string, FroniusDataManagerVariable> Variables { get; set; }

        public FroniusDataManagerDevice()
        {
            Variables = new ConcurrentDictionary<string, FroniusDataManagerVariable>();
        }

    }
}
