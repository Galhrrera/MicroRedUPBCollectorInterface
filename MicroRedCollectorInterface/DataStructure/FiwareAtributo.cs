using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class FiwareAtributo
    {
        public string type { get; set; }
        public double value { get; set; }

        public FiwareAtributo(double value)
        {
            this.type = "Number";
            this.value = value;
        }
    }
}
