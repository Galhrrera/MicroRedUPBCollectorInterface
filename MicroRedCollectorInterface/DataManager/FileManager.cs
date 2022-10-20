using DataStructure;
using MimeKit;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataManager
{
    public static class FileManager
    {

        public static void StartSchedulers()
        {
            SaveMonthlyFileScheduler();
        }

        public static async Task SaveMonthlyFile(DateTime dateTime)
        {
            var valueDifferenceType = ValueDifferenceType.OneMonth;

            double energyB10 = new GetDifferenceMongo().GetValueDifference("MI_B10_5___EnergyTotal", dateTime, valueDifferenceType);

            double energyB18 = new GetDifferenceMongo().GetValueDifference("FR1_B18_12.5___EnergyTotal", dateTime, valueDifferenceType) +
                new GetDifferenceMongo().GetValueDifference("FR2_B18_12.5___EnergyTotal", dateTime, valueDifferenceType) +
                new GetDifferenceMongo().GetValueDifference("FR1_B18_10___EnergyTotal", dateTime, valueDifferenceType) +
                new GetDifferenceMongo().GetValueDifference("FR2_B18_10___EnergyTotal", dateTime, valueDifferenceType);

            double energyB11C = new GetDifferenceMongo().GetValueDifference("FR1_B11_20___EnergyTotal", dateTime, valueDifferenceType) +
                new GetDifferenceMongo().GetValueDifference("FR2_B11_20___EnergyTotal", dateTime, valueDifferenceType);

            double energyB11Labs = new GetDifferenceMongo().GetValueDifference("SL_B11_28___EnergyTotal", dateTime, valueDifferenceType);

            double energyHabitat = new GetDifferenceMongo().GetValueDifference("VICTRON_HABITAT___EnergyTotal", dateTime, valueDifferenceType) * 1000;

            double totalEnergy = energyB10 + energyB18 + energyB11C + energyB11Labs + energyHabitat;

            double totalEnergyUniversity = 0;

            List<Tuple<string, double>> energyPerblock = new List<Tuple<string, double>>();

            foreach (var smartMeter in SmartMeterManager.SmartMeters)
            {
                double tempValue = new GetDifferenceMongo().GetValueDifference(
                    string.Format(DataSource.MongoDBCollectionFormat, smartMeter.Name, "ActiveEnergyImport"), dateTime, valueDifferenceType);
                totalEnergyUniversity += tempValue;
                energyPerblock.Add(new Tuple<string, double>(smartMeter.Name, tempValue));
                Thread.Sleep(500);
            }

            double energyB11 = new GetDifferenceMongo().GetValueDifference("SL_B11_28___ConsumptionEnergyTotal", dateTime, valueDifferenceType);

            totalEnergyUniversity = totalEnergyUniversity + totalEnergy + energyB11;

            energyPerblock.Add(new Tuple<string, double>("SolarLog_B11", energyB11));

            var fileText = new List<string>
            {
                string.Format("Hola, en el siguiente correo se envía la información de la Micro-Red UPB para {0}", dateTime.AddMonths(-1).ToString("MMMM/yyyy")),
                string.Format("Energía generada 5 kWp (Bloque 10): {0} kWh/mes", (energyB10 / 1000.0).ToString("N2")),
                string.Format("Energía generada 45 kWp (Bloque 18): {0} kWh/mes", (energyB18 / 1000.0).ToString("N2")),
                string.Format("Energía generada 28 kWp (Bloque 11 Labs): {0} kWh/mes", (energyB11Labs / 1000.0).ToString("N2")),
                string.Format("Energía generada 2.4 kWp (HABITAT): {0} kWh/mes", (energyHabitat / 1000.0).ToString("N2")),
                string.Format("Energía generada 52.3 kWp (Bloque 11C): {0} kWh/mes", (energyB11C / 1000.0).ToString("N2")),
                string.Format("Energía generada total: {0} kWh/mes", (totalEnergy / 1000.0).ToString("N2")),
                string.Format("Energía consumida Universidad (AMI's + B11 + Energía_Generada): {0} kWh/mes", (totalEnergyUniversity / 1000).ToString("N2"))
            };

            string stringSMFormat = "Equipo: {0}\tEnergía: {1} kWh/mes";

            foreach (var value in energyPerblock)
            {
                fileText.Add(string.Format(stringSMFormat, value.Item1, (value.Item2 / 1000).ToString("N2")));
            }

            using (StreamWriter file = File.CreateText(string.Format("C:\\Users\\Administrator\\Desktop\\Informe_{0}.txt", dateTime.AddMonths(-1).ToString("MMMM/yyyy"))))
            {
                foreach (var line in fileText)
                {
                    file.WriteLine(line);
                }
                file.Close();
            }

            Logging.Logger.Logging("Se guardó el archivo mensual", EventLogEntryType.Information, Logging.Logger.LoggerEventID.Mail);
        }

        private class SaveMonthlyFileJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                await SaveMonthlyFile(DateTime.Now);
            }
        }

        private static async void SaveMonthlyFileScheduler()
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };

            StdSchedulerFactory factory = new StdSchedulerFactory(props);

            IScheduler scheduler = await factory.GetScheduler();

            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<SaveMonthlyFileJob>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("SendMonthlyMailJob", "SendMonthlyMailJob")
                .WithCronSchedule("0 0 8 1 1/1 ? *")
                .StartAt(DateTime.Now)
                .WithPriority(2)
                .Build();

            await scheduler.ScheduleJob(job, trigger);

        }
    }
}
