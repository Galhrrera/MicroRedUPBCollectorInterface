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
    
    public partial class VoltageControllerDevice
    {
        public short ID { get; set; }
        public short Device_ID { get; set; }
        public double Gvq { get; set; }
        public double Gvg { get; set; }
        public double Gvp { get; set; }
        public double PFmin { get; set; }
        public double Vnom { get; set; }
        public double VoltageReference { get; set; }
        public double Vmax { get; set; }
        public double Vmin { get; set; }
        public string VoltageVariable_ID { get; set; }
        public string QVariable_ID { get; set; }
        public string PFVariable_ID { get; set; }
        public string EnableVariable_ID { get; set; }
        public short LoadVariable_ID { get; set; }
    
        public virtual Device Device { get; set; }
    }
}