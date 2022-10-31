using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class FiwareBessBM : FiwareDevice
    {
        public FiwareAtributo BatteryCapacityRemaining;
        public FiwareAtributo BatteryCapacityRemoved;
        public FiwareAtributo BatteryCapacityReturned;
        public FiwareAtributo BatteryCurrent;
        public FiwareAtributo BatteryNumberOfChargeCycles;
        public FiwareAtributo BatteryNumberOfDischarges;
        public FiwareAtributo BatteryStateOfCharge;
        public FiwareAtributo BatteryTemperature;
        public FiwareAtributo BatteryTimeToDischarge;
        public FiwareAtributo BatteryTimeToFull;
        public FiwareAtributo BatteryVoltage;
        public FiwareAtributo DeviceState;

        public FiwareBessBM(double batteryCapacityRemaining, double batteryCapacityRemoved, double batteryCapacityReturned, double batteryCurrent, 
            double batteryNumberOfChargeCycles, double batteryNumberOfDischarges, double batteryStateOfCharge, double batteryTemperature, double batteryTimeToDischarge, 
            double batteryTimeToFull, double batteryVoltage, double deviceState)
        {
            this.BatteryCapacityRemaining.value = batteryCapacityRemaining;
            this.BatteryCapacityRemoved.value = batteryCapacityRemoved;
            this.BatteryCapacityReturned.value = batteryCapacityReturned;
            this.BatteryCurrent.value = batteryCurrent;
            this.BatteryNumberOfChargeCycles.value = batteryNumberOfChargeCycles;
            this.BatteryNumberOfDischarges.value = batteryNumberOfDischarges;
            this.BatteryStateOfCharge.value = batteryStateOfCharge;
            this.BatteryTemperature.value = batteryTemperature;
            this.BatteryTimeToDischarge.value = batteryTimeToDischarge;
            this.BatteryTimeToFull.value = batteryTimeToFull;
            this.BatteryVoltage.value = batteryVoltage;
            this.DeviceState.value = deviceState;
        }
    }
}
