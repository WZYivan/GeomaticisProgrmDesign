using System;
using System.ComponentModel;
using static System.Math;

namespace Lab1
{
    public static class Lib
    {
        public class Circle(double __radii)
        {
            private double radii = __radii;
            public double Area()
            {
                return Pow(radii, 2) * 2 * Math.PI;
            }
        }

        public static void PrintLn()
        {
            Console.WriteLine($"{"Wang ZiYou",-10}, {Math.PI:.000}");
        }

        public enum GradeLevel : int
        {
            [Description("A")] A,
            [Description("B")] B,
            [Description("C")] C,
            [Description("D")] D,
            [Description("E")] E
        }

        public static GradeLevel GetGradeLevel_switch(double grade)
        {
            return (grade) switch
            {                >= 90 => GradeLevel.A,
                >= 80 => GradeLevel.B,
                >= 70 => GradeLevel.C,
                >= 60 => GradeLevel.D,
                _ => GradeLevel.E
            };
        }

        public static GradeLevel GetGradeLevel_if(double grade)
        {
            GradeLevel l;
            if (grade >= 90 && grade <= 100)
            {
                l = GradeLevel.A;
            }
            else if (grade >= 80 && grade <= 89)
            {
                l = GradeLevel.B;
            }
            else if (grade >= 70 && grade <= 79)
            {
                l = GradeLevel.C;
            }
            else if (grade >= 60 && grade <= 69)
            {
                l = GradeLevel.D;
            }
            else
            {
                l = GradeLevel.E;
            }
            return l;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Lib.PrintLn();
            var c = new Lib.Circle(2);
            Console.WriteLine(c.Area());

            Lib.GradeLevel
                l1 = Lib.GetGradeLevel_switch(90),
                l2 = Lib.GetGradeLevel_if(90);
            Console.WriteLine($"Level of 90 from switch: {l1}");
            Console.WriteLine($"Level of 90 from if: {l2}");
        }
    }
}