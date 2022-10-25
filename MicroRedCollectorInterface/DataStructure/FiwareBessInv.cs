using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class FiwareBessInv : FiwareDevice
    {
        public FiwareAtributo BatteryChargeActive;
        public FiwareAtributo BatteryChargeActiveDay;
        public FiwareAtributo BatteryChargeActiveMonth;
        public FiwareAtributo BatteryCurrent;
        public FiwareAtributo BatteryDischargeActive;
        public FiwareAtributo BatteryDischargeActiveDay;
        public FiwareAtributo BatteryDischargeActiveMonth;
        public FiwareAtributo BatteryPower;
        public FiwareAtributo BatteryVoltage;
        public FiwareAtributo ChargeDCCurrent;
        public FiwareAtributo ChargeDCPower;
        public FiwareAtributo ChargeDCPowerPercentage;
        public FiwareAtributo ChargerEnabled;
        public FiwareAtributo ChargerStatus;
        public FiwareAtributo DeviceState;
        public FiwareAtributo EnergyFromBattery;
        public FiwareAtributo EnergyFromBatteryDay;
        public FiwareAtributo EnergyFromBatteryMonth;
        public FiwareAtributo EnergyToBattery;
        public FiwareAtributo EnergyToBatteryDay;
        public FiwareAtributo EnergyToBatteryMonth;
        public FiwareAtributo ForcedSell;
        public FiwareAtributo GridACCurrent;
        public FiwareAtributo GridACFrequency;
        public FiwareAtributo GridACInputCurrent;
        public FiwareAtributo GridACInputPowerApparent;
        public FiwareAtributo GridACInputVoltage;
        public FiwareAtributo GridACL1Current;
        public FiwareAtributo GridACL1Voltage;
        public FiwareAtributo GridACPower;
        public FiwareAtributo GridACVoltage;
        public FiwareAtributo GridInputActive;
        public FiwareAtributo GridInputActiveDay;
        public FiwareAtributo GridInputActiveMonth;
        public FiwareAtributo GridInputEnergy;
        public FiwareAtributo GridInputEnergyDay;
        public FiwareAtributo GridInputEnergyMonth;
        public FiwareAtributo GridOutputActive;
        public FiwareAtributo GridOutputActiveDay;
        public FiwareAtributo GridOutputActiveMonth;
        public FiwareAtributo GridOutputCurrent;
        public FiwareAtributo GridOutputEnergy;
        public FiwareAtributo GridOutputEnergyDay;
        public FiwareAtributo GridOutputEnergyMonth;
        public FiwareAtributo GridOutputFrequency;
        public FiwareAtributo GridOutputPower;
        public FiwareAtributo GridOutputPowerApparent;
        public FiwareAtributo GridOutputVoltage;
        public FiwareAtributo InverterDCCurrent;
        public FiwareAtributo InverterDCPower;
        public FiwareAtributo InverterEnabled;
        public FiwareAtributo InverterStatus;
        public FiwareAtributo LoadACCurrent;
        public FiwareAtributo LoadACFrequency;
        public FiwareAtributo LoadACL1Current;
        public FiwareAtributo LoadACL1Voltage;
        public FiwareAtributo LoadACPower;
        public FiwareAtributo LoadACPowerApparent;
        public FiwareAtributo LoadACVoltage;
        public FiwareAtributo LoadOutputActive;
        public FiwareAtributo LoadOutputActiveDay;
        public FiwareAtributo LoadOutputActiveMonth;
        public FiwareAtributo LoadOutputEnergy;
        public FiwareAtributo LoadOutputEnergyDay;
        public FiwareAtributo LoadOutputEnergyMonth;
        public FiwareAtributo SellEnabled;

        public FiwareBessInv(double batteryChargeActive, double batteryChargeActiveDay, double batteryChargeActiveMonth, double batteryCurrent, double batteryDischargeActive, 
            double batteryDischargeActiveDay, double batteryDischargeActiveMonth, double batteryPower, double batteryVoltage, double chargeDCCurrent, double chargeDCPower, 
            double chargeDCPowerPercentage, double chargerEnabled, double chargerStatus, double deviceState, double energyFromBattery, double energyFromBatteryDay, 
            double energyFromBatteryMonth, double energyToBattery, double energyToBatteryDay, double energyToBatteryMonth, double forcedSell, double gridACCurrent, 
            double gridACFrequency, double gridACInputCurrent, double gridACInputPowerApparent, double gridACInputVoltage, double gridACL1Current, double gridACL1Voltage,
            double gridACPower, double gridACVoltage, double gridInputActive, double gridInputActiveDay, double gridInputActiveMonth, double gridInputEnergy, 
            double gridInputEnergyDay, double gridInputEnergyMonth, double gridOutputActive, double gridOutputActiveDay, double gridOutputActiveMonth, 
            double gridOutputCurrent, double gridOutputEnergy, double gridOutputEnergyDay, double gridOutputEnergyMonth, double gridOutputFrequency, 
            double gridOutputPower, double gridOutputPowerApparent, double gridOutputVoltage, double inverterDCCurrent, double inverterDCPower, double inverterEnabled, 
            double inverterStatus, double loadACCurrent, double loadACFrequency, double loadACL1Current, double loadACL1Voltage, double loadACPower, 
            double loadACPowerApparent, double loadACVoltage, double loadOutputActive, double loadOutputActiveDay, double loadOutputActiveMonth, double loadOutputEnergy, 
            double loadOutputEnergyDay, double loadOutputEnergyMonth, double sellEnabled)
        {
            BatteryChargeActive = batteryChargeActive;
            BatteryChargeActiveDay = batteryChargeActiveDay;
            BatteryChargeActiveMonth = batteryChargeActiveMonth;
            BatteryCurrent = batteryCurrent;
            BatteryDischargeActive = batteryDischargeActive;
            BatteryDischargeActiveDay = batteryDischargeActiveDay;
            BatteryDischargeActiveMonth = batteryDischargeActiveMonth;
            BatteryPower = batteryPower;
            BatteryVoltage = batteryVoltage;
            ChargeDCCurrent = chargeDCCurrent;
            ChargeDCPower = chargeDCPower;
            ChargeDCPowerPercentage = chargeDCPowerPercentage;
            ChargerEnabled = chargerEnabled;
            ChargerStatus = chargerStatus;
            DeviceState = deviceState;
            EnergyFromBattery = energyFromBattery;
            EnergyFromBatteryDay = energyFromBatteryDay;
            EnergyFromBatteryMonth = energyFromBatteryMonth;
            EnergyToBattery = energyToBattery;
            EnergyToBatteryDay = energyToBatteryDay;
            EnergyToBatteryMonth = energyToBatteryMonth;
            ForcedSell = forcedSell;
            GridACCurrent = gridACCurrent;
            GridACFrequency = gridACFrequency;
            GridACInputCurrent = gridACInputCurrent;
            GridACInputPowerApparent = gridACInputPowerApparent;
            GridACInputVoltage = gridACInputVoltage;
            GridACL1Current = gridACL1Current;
            GridACL1Voltage = gridACL1Voltage;
            GridACPower = gridACPower;
            GridACVoltage = gridACVoltage;
            GridInputActive = gridInputActive;
            GridInputActiveDay = gridInputActiveDay;
            GridInputActiveMonth = gridInputActiveMonth;
            GridInputEnergy = gridInputEnergy;
            GridInputEnergyDay = gridInputEnergyDay;
            GridInputEnergyMonth = gridInputEnergyMonth;
            GridOutputActive = gridOutputActive;
            GridOutputActiveDay = gridOutputActiveDay;
            GridOutputActiveMonth = gridOutputActiveMonth;
            GridOutputCurrent = gridOutputCurrent;
            GridOutputEnergy = gridOutputEnergy;
            GridOutputEnergyDay = gridOutputEnergyDay;
            GridOutputEnergyMonth = gridOutputEnergyMonth;
            GridOutputFrequency = gridOutputFrequency;
            GridOutputPower = gridOutputPower;
            GridOutputPowerApparent = gridOutputPowerApparent;
            GridOutputVoltage = gridOutputVoltage;
            InverterDCCurrent = inverterDCCurrent;
            InverterDCPower = inverterDCPower;
            InverterEnabled = inverterEnabled;
            InverterStatus = inverterStatus;
            LoadACCurrent = loadACCurrent;
            LoadACFrequency = loadACFrequency;
            LoadACL1Current = loadACL1Current;
            LoadACL1Voltage = loadACL1Voltage;
            LoadACPower = loadACPower;
            LoadACPowerApparent = loadACPowerApparent;
            LoadACVoltage = loadACVoltage;
            LoadOutputActive = loadOutputActive;
            LoadOutputActiveDay = loadOutputActiveDay;
            LoadOutputActiveMonth = loadOutputActiveMonth;
            LoadOutputEnergy = loadOutputEnergy;
            LoadOutputEnergyDay = loadOutputEnergyDay;
            LoadOutputEnergyMonth = loadOutputEnergyMonth;
            SellEnabled = sellEnabled;
        }
    }
}
