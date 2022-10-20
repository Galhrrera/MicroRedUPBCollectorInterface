using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace DataManager
{
    public static class CollectorInterfaceManager
    {
        private static readonly System.Timers.Timer ThreadsTimer;

        private static readonly System.Timers.Timer ThreadsTimerModbus;

        static CollectorInterfaceManager()
        {
            int devices;

            devices = EnphaseManager.Inverters.Count;
            devices = FroniusDataManagerManager.FroniusDataManagerDevices.Count;
            devices = FroniusManager.FroniusDevices.Count;
            devices = SmartMeterManager.SmartMeters.Count;
            devices = SolarLogManager.SolarLogDevices.Count;
            devices = UbidotsManager.UbidotsDevices.Count;
            devices = ModbusManager.ModbusDevices.Count;

            FileManager.StartSchedulers();

            RestartEnergyVariablesScheduler();

            ThreadsTimer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = 60000,
            };
            ThreadsTimer.Elapsed += new System.Timers.ElapsedEventHandler(ThreadsTimerElapsed);

            ThreadsTimerModbus = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = 30000,
            };
            ThreadsTimerModbus.Elapsed += new System.Timers.ElapsedEventHandler(ThreadsTimerElapsedModbus);
        }

        public static void Start()
        {
            ThreadsTimer.Start();
            ThreadsTimerElapsed(null, null);
            ThreadsTimerModbus.Start();
            ThreadsTimerElapsedModbus(null, null);
        }

        public static void Stop()
        {
            ThreadsTimer.Stop();
            ThreadsTimerModbus.Stop();
        }

        private static void ThreadsTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StartThreads();
        }

        private static void ThreadsTimerElapsedModbus(object sender, System.Timers.ElapsedEventArgs e)
        {
            StartThreadsModbus();
        }

        private static void StartThreads()
        {
            ThreadPool.QueueUserWorkItem(FroniusManager.ReadValues);
            ThreadPool.QueueUserWorkItem(EnphaseManager.ReadValues);
            ThreadPool.QueueUserWorkItem(UbidotsManager.ReadValues);
            ThreadPool.QueueUserWorkItem(FroniusDataManagerManager.ReadValues);
            ThreadPool.QueueUserWorkItem(SolarLogManager.ReadValues);
            ThreadPool.QueueUserWorkItem(CustomCalculationManager.Calculate);

            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine();
        }

        private static void StartThreadsModbus()
        {
            ThreadPool.QueueUserWorkItem(ModbusManager.ReadValues);
            ThreadPool.QueueUserWorkItem(SmartMeterManager.ReadValues);
        }

        public static void RestartEnergyVariables()
        {
            Parallel.ForEach(EnphaseManager.Inverters, inverter =>
            {
                inverter.EnphaseDevice.Variables["EnergyDay"].LastValue = 0;
            });

            Parallel.ForEach(FroniusManager.FroniusDevices, inverter =>
            {
                inverter.Variables["EnergyDay"].LastValue = 0;
            });

            Parallel.ForEach(SolarLogManager.SolarLogDevices, inverter =>
            {
                inverter.Variables["EnergyDay"].LastValue = 0;
            });

        }

        private class RestartEnergyVariablesJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                RestartEnergyVariables();
            }
        }

        private static async void RestartEnergyVariablesScheduler()
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };

            StdSchedulerFactory factory = new StdSchedulerFactory(props);

            IScheduler scheduler = await factory.GetScheduler();

            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<RestartEnergyVariablesJob>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("RestartEnergyVariablesJob", "RestartEnergyVariablesJob")
                .WithCronSchedule("0 0 0 1/1 * ? *")
                .StartAt(DateTime.Now)
                .WithPriority(2)
                .Build();

            await scheduler.ScheduleJob(job, trigger);

        }

    }
}