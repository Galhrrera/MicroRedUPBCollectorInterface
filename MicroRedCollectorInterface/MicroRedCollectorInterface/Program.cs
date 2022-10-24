using DataManager;
using System;

namespace MicroRedCollectorInterface
{
    class Program
    {

        static void Main()
        {
            //FileManager.SaveMonthlyFile(new DateTime(2022, 7, 1, 8, 0, 0));

            StartCollector();

            Console.ReadLine();
        }

        private static void StartCollector()
        {
            CollectorInterfaceManager.Start();
        }

    }
}
