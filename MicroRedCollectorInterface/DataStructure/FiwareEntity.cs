using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class FiwareEntity
    {
        public Dictionary<string, FiwareAtributo> Atributos = new Dictionary<string, FiwareAtributo>();

        public FiwareEntity(Dictionary<string, double> atributos)
        {
            foreach (var atributo in atributos)
            {
                bool hasvalue = atributos.TryGetValue(atributo.Key, out double value);
                if (hasvalue)
                {
                    try
                    {
                        FiwareAtributo Attr = new FiwareAtributo(value);

                        if (atributo.Key == "FR1_B18_12.5Efficiency")
                            this.Atributos.Add("FR1_B18_12_5Efficiency", Attr);
                        else if (atributo.Key == "FR2_B18_12.5Efficiency")
                            this.Atributos.Add("FR2_B18_12_5Efficiency", Attr);
                        else
                            this.Atributos.Add(atributo.Key, Attr);

                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error al inicializar el sensor: " + e.Message);
                    }

                }
            }
        }
    }
}
