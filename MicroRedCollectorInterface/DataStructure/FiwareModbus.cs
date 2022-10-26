using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class FiwareModbus : FiwareDevice
    {
        public FiwareAtributo ApparentPower;
        public FiwareAtributo EnergyTotal;
        public FiwareAtributo OutPFSet;
        public FiwareAtributo OutPFSet_Ena;
        public FiwareAtributo OutPFSet_RmpTms;
        public FiwareAtributo OutPFSet_RvrtTms;
        public FiwareAtributo OutPFSet_WinTms;
        public FiwareAtributo PAC;
        public FiwareAtributo ReactivePower;
        public FiwareAtributo TotalPowerFactor;
        public FiwareAtributo V1;
        public FiwareAtributo V2;
        public FiwareAtributo V3;
        public FiwareAtributo WMaxLim_Ena;
        public FiwareAtributo WMaxLimPct;
        public FiwareAtributo WMaxLimPct_RmpTms;
        public FiwareAtributo WMaxLimPct_RvrtTms;
        public FiwareAtributo WMaxLimPct_WinTms;

        public FiwareModbus(double apparentPower, double energyTotal, double outPFSet, double outPFSet_Ena, double outPFSet_RmpTms, 
            double outPFSet_RvrtTms, double outPFSet_WinTms, double pAC, double reactivePower, double totalPowerFactor, double v1, double v2, 
            double v3, double wMaxLim_Ena, double wMaxLimPct, double wMaxLimPct_RmpTms, double wMaxLimPct_RvrtTms, double wMaxLimPct_WinTms)
        {
            this.ApparentPower.value = apparentPower;
            this.EnergyTotal.value = energyTotal;
            this.OutPFSet.value = outPFSet;
            this.OutPFSet_Ena.value = outPFSet_Ena;
            this.OutPFSet_RmpTms.value = outPFSet_RmpTms;
            this.OutPFSet_RvrtTms.value = outPFSet_RvrtTms;
            this.OutPFSet_WinTms.value = outPFSet_WinTms;
            this.PAC.value = pAC;
            this.ReactivePower.value = reactivePower;
            this.TotalPowerFactor.value = totalPowerFactor;
            this.V1.value = v1;
            this.V2.value = v2;
            this.V3.value = v3;
            this.WMaxLim_Ena.value = wMaxLim_Ena;
            this.WMaxLimPct.value = wMaxLimPct;
            this.WMaxLimPct_RmpTms.value = wMaxLimPct_RmpTms;
            this.WMaxLimPct_RvrtTms.value = wMaxLimPct_RvrtTms;
            this.WMaxLimPct_WinTms.value = wMaxLimPct_WinTms;
        }
    }
}
