using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure
{
    public class FiwareFronius : FiwareDevice
    {
        //Variables para dispositivo Fronius
        public FiwareAtributo EnergyDay;
        public FiwareAtributo EnergyTotal;
        public FiwareAtributo EnergyYear;
        public FiwareAtributo Frequency;
        public FiwareAtributo IAC;
        public FiwareAtributo IDC;
        public FiwareAtributo PAC;
        public FiwareAtributo VAC;
        public FiwareAtributo VDC;

        public FiwareFronius(double energyDay, double energyTotal, double energyYear, double frequency, double iAC, double iDC, double pAC, 
            double vAC, double vDC)
        {
            this.EnergyDay.value = energyDay;
            this.EnergyTotal.value = energyTotal;
            this.EnergyYear.value = energyYear;
            this.Frequency.value = frequency;
            this.IAC.value = iAC;
            this.IDC.value = iDC;
            this.PAC.value = pAC;
            this.VAC.value = vAC;
            this.VDC.value = vDC;
        }

        public FiwareFronius(double energyTotal)
        {
            this.EnergyTotal = new FiwareAtributo(energyTotal);
            this.EnergyDay = new FiwareAtributo();
            this.EnergyYear = new FiwareAtributo();
            this.Frequency = new FiwareAtributo();
            this.IAC = new FiwareAtributo();
            this.IDC = new FiwareAtributo();
            this.PAC = new FiwareAtributo();
            this.VAC = new FiwareAtributo();
            this.VDC = new FiwareAtributo();
        }
    }
}
