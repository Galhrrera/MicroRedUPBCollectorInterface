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
    
    public partial class EnphaseDevice
    {
        public short ID { get; set; }
        public short DeviceID { get; set; }
        public double Capacity { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Nullable<short> MicroInverters { get; set; }
        public Nullable<short> NumberOfPanels { get; set; }
    
        public virtual Device Device { get; set; }
    }
}