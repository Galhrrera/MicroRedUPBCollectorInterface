using System;

namespace DataManager
{
    public static class PrefixManager
    {
        private static readonly string Units = "kMGTPE";

        public static void GetPrefix(double value, out double scaledValue, out string prefix)
        {
            int exp = (int)Math.Floor(Math.Log10(value) / 3);
            if (exp == 0)
            {
                prefix = "";
                scaledValue = value;
            }
            else
            {
                prefix = Units.ToCharArray()[exp - 1].ToString();
                double den = Math.Pow(1000, exp);
                scaledValue = value / den;
            }
        }

        public static double GetSufix(string unit, string baseUnit)
        {
            int exp = 0;
            if (unit.Equals(baseUnit))
            {
                return 1;
            }
            for (int i = 0; i < Units.ToCharArray().Length; i++)
            {
                string temp = Units.ToCharArray()[i] + baseUnit;
                if (temp.Equals(unit))
                {
                    exp = i + 1;
                    break;
                }
            }
            return Math.Pow(1000, exp);
        }

    }
}
