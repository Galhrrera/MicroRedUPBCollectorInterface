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
            BatteryChargeActive = new FiwareAtributo(batteryChargeActive);
            BatteryChargeActiveDay = new FiwareAtributo(batteryChargeActiveDay);
            BatteryChargeActiveMonth = new FiwareAtributo(batteryChargeActiveMonth);
            BatteryCurrent = new FiwareAtributo(batteryCurrent);
            BatteryDischargeActive = new FiwareAtributo(batteryDischargeActive);
            BatteryDischargeActiveDay = new FiwareAtributo(batteryDischargeActiveDay);
            BatteryDischargeActiveMonth = new FiwareAtributo(batteryDischargeActiveMonth);
            BatteryPower = new FiwareAtributo(batteryPower);
            BatteryVoltage = new FiwareAtributo(batteryVoltage);
            ChargeDCCurrent = new FiwareAtributo(chargeDCCurrent);
            ChargeDCPower = new FiwareAtributo(chargeDCPower);
            ChargeDCPowerPercentage = new FiwareAtributo(chargeDCPowerPercentage);
            ChargerEnabled = new FiwareAtributo(chargerEnabled);
            ChargerStatus = new FiwareAtributo(chargerStatus);
            DeviceState = new FiwareAtributo(deviceState);
            EnergyFromBattery = new FiwareAtributo(energyFromBattery);
            EnergyFromBatteryDay = new FiwareAtributo(energyFromBatteryDay);
            EnergyFromBatteryMonth = new FiwareAtributo(energyFromBatteryMonth);
            EnergyToBattery = new FiwareAtributo(energyToBattery);
            EnergyToBatteryDay = new FiwareAtributo(energyToBatteryDay);
            EnergyToBatteryMonth = new FiwareAtributo(energyToBatteryMonth);
            ForcedSell = new FiwareAtributo(forcedSell);
            GridACCurrent = new FiwareAtributo(gridACCurrent);
            GridACFrequency = new FiwareAtributo(gridACFrequency);
            GridACInputCurrent = new FiwareAtributo(gridACInputCurrent);
            GridACInputPowerApparent = new FiwareAtributo(gridACInputPowerApparent);
            GridACInputVoltage = new FiwareAtributo(gridACInputVoltage);
            GridACL1Current = new FiwareAtributo(gridACL1Current);
            GridACL1Voltage = new FiwareAtributo(gridACL1Voltage);
            GridACPower = new FiwareAtributo(gridACPower);
            GridACVoltage = new FiwareAtributo(gridACVoltage);
            GridInputActive = new FiwareAtributo(gridInputActive);
            GridInputActiveDay = new FiwareAtributo(gridInputActiveDay);
            GridInputActiveMonth = new FiwareAtributo(gridInputActiveMonth);
            GridInputEnergy = new FiwareAtributo(gridInputEnergy);
            GridInputEnergyDay = new FiwareAtributo(gridInputEnergyDay);
            GridInputEnergyMonth = new FiwareAtributo(gridInputEnergyMonth);
            GridOutputActive = new FiwareAtributo(gridOutputActive);
            GridOutputActiveDay = new FiwareAtributo(gridOutputActiveDay);
            GridOutputActiveMonth = new FiwareAtributo(gridOutputActiveMonth);
            GridOutputCurrent = new FiwareAtributo(gridOutputCurrent);
            GridOutputEnergy = new FiwareAtributo(gridOutputEnergy);
            GridOutputEnergyDay = new FiwareAtributo(gridOutputEnergyDay);
            GridOutputEnergyMonth = new FiwareAtributo(gridOutputEnergyMonth);
            GridOutputFrequency = new FiwareAtributo(gridOutputFrequency);
            GridOutputPower = new FiwareAtributo(gridOutputPower);
            GridOutputPowerApparent = new FiwareAtributo(gridOutputPowerApparent);
            GridOutputVoltage = new FiwareAtributo(gridOutputVoltage);
            InverterDCCurrent = new FiwareAtributo(inverterDCCurrent);
            InverterDCPower = new FiwareAtributo(inverterDCPower);
            InverterEnabled = new FiwareAtributo(inverterEnabled);
            InverterStatus = new FiwareAtributo(inverterStatus);
            LoadACCurrent = new FiwareAtributo(loadACCurrent);
            LoadACFrequency = new FiwareAtributo(loadACFrequency);
            LoadACL1Current = new FiwareAtributo(loadACL1Current);
            LoadACL1Voltage = new FiwareAtributo(loadACL1Voltage);
            LoadACPower = new FiwareAtributo(loadACPower);
            LoadACPowerApparent = new FiwareAtributo(loadACPowerApparent);
            LoadACVoltage = new FiwareAtributo(loadACVoltage);
            LoadOutputActive = new FiwareAtributo(loadOutputActive);
            LoadOutputActiveDay = new FiwareAtributo(loadOutputActiveDay);
            LoadOutputActiveMonth = new FiwareAtributo(loadOutputActiveMonth);
            LoadOutputEnergy = new FiwareAtributo(loadOutputEnergy);
            LoadOutputEnergyDay = new FiwareAtributo(loadOutputEnergyDay);
            LoadOutputEnergyMonth = new FiwareAtributo(loadOutputEnergyMonth);
            SellEnabled = new FiwareAtributo(sellEnabled);
        }
    }
}
