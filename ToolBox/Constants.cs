using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
namespace ToolBox
{
    public static class Constants
    {
        public static readonly double
            deg2rad = Math.PI / 180;
        public static class GeodeticCoordinate
        {
            public static readonly double
                a = 6378137,
                f = 1 / 298.257222101,
                e2 = 2 * f - Pow(f, 2);
            public static double N(double B)
            {
                double sinB = Math.Sin(B);
                return a / Sqrt( 1 - e2 * Pow(sinB, 2) );
            }
        }
    }
}
