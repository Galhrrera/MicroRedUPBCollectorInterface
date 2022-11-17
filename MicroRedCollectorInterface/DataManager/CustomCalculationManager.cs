using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager
{
    public static class CustomCalculationManager
    {
        
        static CustomCalculationManager()
        {

        }

        public static void Calculate(object parameters)
        {
            CalculateEfficiency();

            Dictionary<string, double> CalculatedVariables = new Dictionary<string, double>();

            CalculatedVariables = new Dictionary<string, double>()
            {
                { "TotalGeneratedEnergy", 0},           ///Wh
                { "TotalGeneratedEnergyDay", 0},        ///Wh
                { "TotalGeneratedPower", 0},            ///W
                { "TotalActiveEnergyConsumedDay", 0},   ///Wh
                { "TotalActivePowerConsumed", 0},       ///W
                { "TotalEmissions", 0},                 ///kgCO2
                { "EquivalentTrees", 0},                ///Trees
                { "TotalPowerShare", 0},                ///Percentage
                { "TotalEnergyShare", 0},               ///Percentage
                { "GeneratedPowerA1", 0},               ///W
                { "ActivePowerConsumedA1", 0},          ///W
                { "PowerShareA1", 0},                   ///Percentage
                { "EnergyShareA1", 0},                  ///Percentage
                { "GeneratedPowerA2", 0},               ///W
                { "ActivePowerConsumedA2", 0},          ///W
                { "PowerShareA2", 0},                   ///Percentage
                { "EnergyShareA2", 0},                  ///Percentage
                { "GeneratedPowerA3", 0},               ///W
                { "ActivePowerConsumedA3", 0},          ///W
                { "PowerShareA3", 0},                   ///Percentage
                { "EnergyShareA3", 0},                  ///Percentage
            };

            double TotalGeneratedEnergy = CalculateTotalGeneratedEnergy();
            double TotalGeneratedEnergyDay = CalculateGeneratedEnergyDay();
            double TotalGeneratedPower = CalculateGeneratedPower();
            double TotalActiveEnergyConsumedDay = CalculateActiveEnergyConsumedDay() + TotalGeneratedEnergyDay;
            double TotalActivePowerConsumed = CalculateActivePowerConsumed() + TotalGeneratedPower;

            CalculatedVariables["TotalGeneratedEnergy"] = TotalGeneratedEnergy;
            CalculatedVariables["TotalGeneratedEnergyDay"] = TotalGeneratedEnergyDay;
            CalculatedVariables["TotalActiveEnergyConsumedDay"] = TotalActiveEnergyConsumedDay;
            CalculatedVariables["TotalEmissions"] = TotalGeneratedEnergy * 0.222 / 1000;
            CalculatedVariables["EquivalentTrees"] = (double)CalculatedVariables["TotalEmissions"] / 15.0;

            CalculatedVariables["TotalGeneratedPower"] = TotalGeneratedPower;
            CalculatedVariables["TotalActivePowerConsumed"] = TotalActivePowerConsumed;
            CalculatedVariables["TotalPowerShare"] = 100 * TotalGeneratedPower / (TotalActivePowerConsumed);
            CalculatedVariables["TotalEnergyShare"] = 100 * TotalGeneratedEnergyDay / (TotalActiveEnergyConsumedDay);

            for (int i = 1; i <= 3; i++)
            {
                var genPower = CalculateGeneratedPower(i);
                var conPower = CalculateActivePowerConsumed(i) + genPower;
                var genEnergy = CalculateGeneratedEnergyDay(i);
                var conEnergy = CalculateActiveEnergyConsumedDay(i) + genEnergy;

                CalculatedVariables["GeneratedPowerA" + i] = genPower;
                CalculatedVariables["GeneratedEnergyDayA" + i] = genEnergy;
                CalculatedVariables["ActivePowerConsumedA" + i] = conPower;
                CalculatedVariables["PowerShareA" + i] = 100 * genPower / conPower;
                CalculatedVariables["EnergyShareA" + i] = 100 * genEnergy / conEnergy;
            }

            var epochTime = DataSource.ConvertDateTimeToUnixEpoch(DateTime.Now);
            Parallel.ForEach(CalculatedVariables, variable =>
            {
                var collection = string.Format(DataSource.MongoDBCollectionFormat, "Calculation", variable.Key);
                var value = variable.Value;
                DataSource.InsertDocumentMongoDB(collection, (double)value, epochTime);
            });
            DataSource.InsertDocumentInfluxDBLocal("Calculation", CalculatedVariables, epochTime);
            DataSource.InsertTopicKafka("Calculation", CalculatedVariables, epochTime);
        }

        private static double GetRadiation()
        {
            double radiation;

            var source = (from d in UbidotsManager.UbidotsDevices
                          where d.Name.Equals("WS_UPB")
                          select d).FirstOrDefault();

            var listVariables = (from d in source.Variables
                                 where d.Value.Name == "Solar_radiation" && d.Value.IsValid
                                 select d.Value.LastValue).ToList();
            if (listVariables.Count == 0)
            {
                return 0;
            }
            radiation = listVariables.Max();

            return radiation;
        }

        private static void CalculateEfficiency()
        {
            var epochTime = DataSource.ConvertDateTimeToUnixEpoch(DateTime.Now);
            double radiation = GetRadiation();

            var efficiency = new Dictionary<Tuple<string, string>, double>();

            foreach (var inverter in FroniusManager.FroniusDevices)
            {
                inverter.Efficiency = radiation == 0 ? 0 :
                    inverter.Variables["PAC"].LastValue * 100 / (1.63 * radiation * inverter.NumberOfPanels);

                var tuple = new Tuple<string, string>("Calculation", inverter.Name + "Efficiency");
                var value = inverter.Efficiency;
                efficiency.Add(tuple, value);
            }

            foreach (var inverter in EnphaseManager.Inverters)
            {
                inverter.EnphaseDevice.Efficiency = radiation == 0 ? 0 :
                    inverter.EnphaseDevice.Variables["PAC"].LastValue * 100 / (1.63 * radiation * inverter.EnphaseDevice.NumberOfPanels);

                var tuple = new Tuple<string, string>("Calculation", inverter.EnphaseDevice.Name + "Efficiency");
                var value = inverter.EnphaseDevice.Efficiency;
                efficiency.Add(tuple, value);
            }

            var efficiencyInfluxDB = new Dictionary<string, double>();
            foreach (var value in efficiency)
            {
                DataSource.InsertDocumentMongoDB(string.Format(DataSource.MongoDBCollectionFormat, value.Key.Item1, value.Key.Item2), value.Value, epochTime);
                efficiencyInfluxDB.Add(value.Key.Item2, value.Value);
            }
            DataSource.InsertDocumentInfluxDBLocal("Calculation", efficiencyInfluxDB, epochTime);
            DataSource.InsertTopicKafka("Calculation", efficiencyInfluxDB, epochTime);

        }

        private static double CalculateTotalGeneratedEnergy()
        {
            var froniusValue = (from d in FroniusManager.FroniusDevices
                                select d.Variables["EnergyTotal"].LastValue).Sum();
            var enphaseValue = (from d in EnphaseManager.Inverters
                                select d.EnphaseDevice.Variables["EnergyTotal"].LastValue).Sum();
            double totalValue = froniusValue + enphaseValue;
            return totalValue;
        }

        private static double CalculateGeneratedEnergyDay(int area = 0)
        {
            double totalValue;
            if (area == 0)
            {
                totalValue = (from d in FroniusManager.FroniusDevices
                              select d.Variables["EnergyDay"].LastValue).Sum() +
                             (from d in EnphaseManager.Inverters
                              select d.EnphaseDevice.Variables["EnergyDay"].LastValue).Sum();
            }
            else
            {
                totalValue = (from d in FroniusManager.FroniusDevices
                              where area == d.Area
                              select d.Variables["EnergyDay"].LastValue).Sum() +
                             (from d in EnphaseManager.Inverters
                              where area == d.EnphaseDevice.Area
                              select d.EnphaseDevice.Variables["EnergyDay"].LastValue).Sum();
            }
            return totalValue;
        }

        private static double CalculateGeneratedPower(int area = 0)
        {
            double totalValue;
            if (area == 0)
            {
                totalValue = (from d in FroniusManager.FroniusDevices
                              where d.IsValid == true
                              select d.Variables["PAC"].LastValue).Sum() +
                             (from d in EnphaseManager.Inverters
                              where d.EnphaseDevice.IsValid == true
                              select d.EnphaseDevice.Variables["PAC"].LastValue).Sum();
            }
            else
            {
                totalValue = (from d in FroniusManager.FroniusDevices
                              where d.IsValid == true && area == d.Area
                              select d.Variables["PAC"].LastValue).Sum() +
                             (from d in EnphaseManager.Inverters
                              where d.EnphaseDevice.IsValid == true && area == d.EnphaseDevice.Area
                              select d.EnphaseDevice.Variables["PAC"].LastValue).Sum();
            }
            return totalValue;
        }

        private static double CalculateActiveEnergyConsumedDay(int area = 0)
        {
            double totalValue;
            if (area == 0)
            {
                totalValue = (from d in SmartMeterManager.SmartMeters
                              select d.Variables["ActiveEnergyImportDay"].LastValue).Sum();
            }
            else
            {
                totalValue = (from d in SmartMeterManager.SmartMeters
                              where area == d.Area
                              select d.Variables["ActiveEnergyImportDay"].LastValue).Sum();
            }
            return totalValue;
        }

        private static double CalculateActivePowerConsumed(int area = 0)
        {
            double totalValue;
            if (area == 0)
            {
                totalValue = (from d in SmartMeterManager.SmartMeters
                              where d.IsValid == true
                              select d.Variables["ActivePower"].LastValue).Sum();
            }
            else
            {
                totalValue = (from d in SmartMeterManager.SmartMeters
                              where d.IsValid == true && area == d.Area
                              select d.Variables["ActivePower"].LastValue).Sum();
            }
            return totalValue;
        }
    }
}
