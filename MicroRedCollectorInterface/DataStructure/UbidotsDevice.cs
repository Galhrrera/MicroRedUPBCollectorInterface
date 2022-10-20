using System.Collections.Generic;

namespace DataStructure
{
    public class UbidotsDevice : Device
    {
        /// <summary>
        /// Petición base
        /// </summary>
        public readonly string BaseRequest = "https://industrial.api.ubidots.com/api/v1.6/variables/{0}?token={1}";
        
        /// <summary>
        /// Token para las consultas a ubidots
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Variables del inversor
        /// </summary>
        public Dictionary<string, UbidotsVariable> Variables { get; set; }

        public UbidotsDevice()
        {
            Variables = new Dictionary<string, UbidotsVariable>();
        }

    }
}
