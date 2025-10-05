using System;
using ToolBox;
using ToolBox.PrattParsing;
using ToolBox.PrattParsing.Expression;
using ToolBox.PrattParsing.Token;
using static ToolBox.FManip;
using static ToolBox.PPrinter;

namespace EmptyConsoleProject
{
    class Program
    {
        static void Main()
        {
            var expr = "a ^ b - sin (x0!)";
            Lexer lexer = new(expr);
            PPrint(expr);
            PPrint(lexer.Expr());

            var pexpr = (Operation)Parse.FromStr(expr);
            PPrint(pexpr.Expr());

            bool algoAss = pexpr.AssignAlgebra(Parse.MakeAlgebraAssignmentMap("a", 3, "b", 2, "x0", 5));         
            PPrint($"Algebra assignment success: {algoAss}");
            PPrint(pexpr.Eval().Value());

            //PPrint("Now entering interactive environment.");
            //ToolBox.PrattParsing.InteractiveEnvironment.Connect();
        }
    }
}