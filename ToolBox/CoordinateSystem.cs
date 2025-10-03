using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace ToolBox
{
    namespace CoordinateSystem
    {
        public static class GeoUtility
        {
            public enum Quadrant
            {
                First, Second, Third, Forth
            }
            public static Quadrant QuadrantOf(double x, double y)
            {
                return (x > 0, y > 0) switch
                {
                    (true, true) => Quadrant.First,
                    (false, true) => Quadrant.Second,
                    (false, false) => Quadrant.Third,
                    (true, false) => Quadrant.Forth
                };
            }
            /// <summary>
            /// Azumith of Normmal Coordinate System (N[y], E[x])
            /// </summary>
            /// <param name="x1"></param>
            /// <param name="y1"></param>
            /// <param name="x2"></param>
            /// <param name="y2"></param>
            /// <returns></returns>
            public static double Azumith(double x1, double y1, double x2, double y2)
            {
                double dx = x2 - x1,
                       dy = y2 - y1;

                if (dx == 0)
                {
                    return (dy) switch
                    {
                        > 0 => 0,        // 正Y方向
                        < 0 => Math.PI,  // 负Y方向
                        _ => 0
                    };
                }
                if (dy == 0)
                {
                    return (dx) switch
                    {
                        > 0 => Math.PI / 2,     // 正X方向  
                        < 0 => Math.PI * 3 / 2, // 负X方向
                        _ => 0
                    };
                }

                double atan = Math.Atan(Math.Abs(dy / dx));

                //if (dx > 0 && dy > 0)
                //{
                //    return atan;
                //}
                //else if (dx > 0 && dy < 0)
                //{
                //    return Math.PI - atan;
                //}
                //else if (dx < 0 && dy > 0)
                //{
                //    return Math.PI * 2 - atan;
                //}
                //else
                //{
                //    return Math.PI + atan;
                //}

                return QuadrantOf(dx, dy) switch
                {
                    Quadrant.First => atan,
                    Quadrant.Second => Math.PI * 2 - atan,
                    Quadrant.Third => Math.PI + atan,
                    Quadrant.Forth => Math.PI - atan,
                    _ => -1
                };
            }
        }

        public static class GeoConvert
        {
            public static double DegToRad(double deg)
            {
                return deg * Constants.deg2rad;
            }
            public static (double x, double y) PolarToXY(double r, double phi, string unit = "rad")
            {
                if (unit != "rad")
                {
                    if (unit == "deg")
                    {
                        phi = DegToRad(phi);
                    }
                    else
                    {
                        throw new ArgumentException(message: $"{unit} is not supported", paramName: nameof(unit));
                    }
                }

                return (x: r * Cos(phi), y: r * Sin(phi));
            }
            public static (double r, double phi) XYToPolar(double x, double y)
            {
                return (r: Sqrt(Pow(x, 2) + Pow(y, 2)), phi: Atan2(y, x));
            }

            public static (double x, double y, double z) GeoCoordToXYZ(double b, double l, double h, string unit = "rad")
            {
                if (unit != "rad")
                {
                    if (unit == "deg")
                    {
                        b = DegToRad(b);
                        l = DegToRad(l);
                    }
                    else
                    {
                        throw new ArgumentException(message: $"{unit} is not supported", paramName: nameof(unit));
                    }
                }
                double
                    N = Constants.GeodeticCoordinate.N(b),
                    e2 = Constants.GeodeticCoordinate.e2;
                return (
                    x: (N + h) * Cos(b) * Cos(l),
                    y: (N + h) * Cos(b) * Sin(l),
                    z: (N * (1 - e2) + h) * Sin(b)
                    );
            }
        }

        public static class CoordinateCalculation
        {
            public static (double l, double phi) Inverse(double x1, double y1, double x2, double y2)
            {
                return (
                    l: Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)),
                    phi: GeoUtility.Azumith(y1, x1, y2, x2)
                    );
            }

            public static (double x, double y) Forward(double x, double y, double l, double phi)
            {
                double dx = l * Cos(phi),
                    dy = l * Sin(phi);
                return (
                    x: x + dx,
                    y: y + dy
                    );
            }
        }
    }
}
