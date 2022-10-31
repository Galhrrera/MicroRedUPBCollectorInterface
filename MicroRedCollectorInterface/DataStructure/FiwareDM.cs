using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class FiwareDM: FiwareDevice
    {
        //Atributos para dispositivo DM
        /*
        public FiwareAtributo Radiation;
        public FiwareAtributo Temperature1;
        public FiwareAtributo Temperature2;

        public FiwareDM(double radiation, double temperature1, double temperature2)
        {
            this.Radiation = new FiwareAtributo(radiation);
            this.Temperature1 = new FiwareAtributo(temperature1);
            this.Temperature2 = new FiwareAtributo(temperature2);
        }
        */
        public Dictionary<string, FiwareAtributo> Atributos;

        public FiwareDM(Dictionary<string, double> atributos)
        {
            foreach (var atributo in atributos)
            {
                FiwareAtributo attr = new FiwareAtributo(atributo.Value);
                Atributos.Add(atributo.Key, attr);
            }
        }
    }
}
