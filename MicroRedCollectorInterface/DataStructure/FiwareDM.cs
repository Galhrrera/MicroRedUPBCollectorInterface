using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class FiwareDM: FiwareDevice
    {
        public FiwareAtributo Radiation;
        public FiwareAtributo Temperature1;
        public FiwareAtributo Temperature2;

        public FiwareDM(double radiation, double temperature1, double temperature2)
        {
            this.Radiation.value = radiation;
            this.Temperature1.value = temperature1;
            this.Temperature2.value = temperature2;
        }
    }
}
