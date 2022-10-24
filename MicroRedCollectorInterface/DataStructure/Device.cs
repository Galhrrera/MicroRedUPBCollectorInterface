namespace DataStructure
{
    public class Device
    {
        /// <summary>
        /// IP del device
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Nombre asociado al inversor
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Si el dispositivo es controlable o no
        /// </summary>
        public bool Controllable { get; set; }

        /// <summary>
        /// Ubicación del inversor
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Tipo del dispositivo, heredan todos los dispositivos
        /// </summary>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// Area a la cual está asociado el dispositivo
        /// </summary>
        public int Area { get; set; }

    }
}
