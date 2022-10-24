using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Logging
{
    public class Logger
    {
        private static ConcurrentDictionary<EventLogEntryType, List<string>> storedMessages = new ConcurrentDictionary<EventLogEntryType, List<string>>();

        public static EventLog eventLog = new EventLog();

        public enum LoggerEventID
        {
            General = 1,
            SmartMeter = 2,
            Fronius = 3,
            SolarLog = 4,
            Victron = 5,
            Enphase = 6,
            Mongo = 7,
            Mail = 8,
            Modbus = 9,
            InfluxDB = 10,
            Kafka = 11
        }

        static Logger()
        {
            eventLog.Source = "uRedCollectorInterfaceService"; 
            eventLog.Log = "uRedCollectorInterface";

            foreach (EventLogEntryType eventLog in Enum.GetValues(typeof(EventLogEntryType)))
            {
                storedMessages.TryAdd(eventLog, new List<string>());
            }
        }

        public static void CleanStoredMessages()
        {
            foreach (var storedMessage in storedMessages)
            {
                storedMessage.Value.Clear();
            }
        }

        public static void Logging(string message, EventLogEntryType eventLogEntryType, LoggerEventID loggerEventID)
        {
            new Thread(() =>
            {
                if (storedMessages.TryGetValue(eventLogEntryType, out List<string> storedMessagesList))
                {
                    if (!storedMessages[eventLogEntryType].Contains(message) || eventLogEntryType == EventLogEntryType.Information)
                    {
                        eventLog.WriteEntry(message, eventLogEntryType, (int)loggerEventID);
                        storedMessages[eventLogEntryType].Add(message);
                    }
                }
            }).Start();
        }

    }
}
