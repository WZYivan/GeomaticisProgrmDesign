using ToolBox;
using ToolBox.CoordinateSystem;
using ToolBox.Linalg;
using static ToolBox.FManip;
using static ToolBox.PPrinter;
using static ToolBox.Format.Templates;

namespace Lab2
{
    class Program
    {
        public static void Main(string[] args)
        {

            var (r, p) = GeoConvert.XYToPolar(1, 1);
            var (x, y) = GeoConvert.PolarToXY(1, 45, unit: "deg");
            PPrint("GeoConvert");
            PPrint(Names.RPhi, r, p);
            PPrint(Names.XY, x, y);

            var (l, phi) = CoordinateCalculation.Inverse(0, 0, 1, -1);
            PPrint("Inverse Calculation");
            PPrint(Named("L", "Phi"), l, phi);

            Matrix
                m1 = Matrix.Random(3, 3),
                m2 = Matrix.Random(3, 3);

            PPrint("Random build matrix");
            PPrint(m1.Individual(), m2.Individual());

            Matrix
                mul = m1 * m2,
                pls = m1 + m2,
                sub = m1 - m2,
                div = m1 * m2.Inverse();
            PPrint("Linalg Calaulation");

            using (var _ = FConfig.FormatterSeperator.Using(",\n"))
            {
                //PPrint(SqQuoted("+", "-", "*", "/"), pls.Indi(), sub.Indi(), mul.Indi(), div.Indi());
                PPrint(SqQuoted("+", "-", "*", "/"), MakePPArgs(FManip.Individual, pls, sub, mul, div, 3));
            }

        }
    }
}