//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DBModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class Device
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Device()
        {
            this.EnphaseDevices = new HashSet<EnphaseDevice>();
            this.EnphaseVariables = new HashSet<EnphaseVariable>();
            this.FroniusDataManagerDevices = new HashSet<FroniusDataManagerDevice>();
            this.FroniusDataManagerVariables = new HashSet<FroniusDataManagerVariable>();
            this.FroniusDevices = new HashSet<FroniusDevice>();
            this.FroniusVariables = new HashSet<FroniusVariable>();
            this.ModbusDevices = new HashSet<ModbusDevice>();
            this.ModbusVariables = new HashSet<ModbusVariable>();
            this.SmartMeterDevices = new HashSet<SmartMeterDevice>();
            this.SmartMeterVariables = new HashSet<SmartMeterVariable>();
            this.SolarLogDevices = new HashSet<SolarLogDevice>();
            this.SolarLogVariables = new HashSet<SolarLogVariable>();
            this.UbidotsDevices = new HashSet<UbidotsDevice>();
            this.UbidotsVariables = new HashSet<UbidotsVariable>();
            this.VoltageControllerDevices = new HashSet<VoltageControllerDevice>();
        }
    
        public short ID { get; set; }
        public string IP { get; set; }
        public string Name { get; set; }
        public bool Controllable { get; set; }
        public string Location { get; set; }
        public short DeviceType { get; set; }
        public short Area { get; set; }
    
        public virtual Area Area1 { get; set; }
        public virtual DevicesType DevicesType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EnphaseDevice> EnphaseDevices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EnphaseVariable> EnphaseVariables { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FroniusDataManagerDevice> FroniusDataManagerDevices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FroniusDataManagerVariable> FroniusDataManagerVariables { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FroniusDevice> FroniusDevices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FroniusVariable> FroniusVariables { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ModbusDevice> ModbusDevices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ModbusVariable> ModbusVariables { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SmartMeterDevice> SmartMeterDevices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SmartMeterVariable> SmartMeterVariables { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SolarLogDevice> SolarLogDevices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SolarLogVariable> SolarLogVariables { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UbidotsDevice> UbidotsDevices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UbidotsVariable> UbidotsVariables { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VoltageControllerDevice> VoltageControllerDevices { get; set; }
    }
}
