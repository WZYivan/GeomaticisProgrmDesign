using System;
using ToolBox;
using ToolBox.PrattParsing;
using static ToolBox.FManip;
using static ToolBox.PPrinter;

namespace EmptyConsoleProject
{
    class Program
    {
        static void Main()
        {
            var expr = "sin(1) + cos(2)";
            Lexer lexer = new(expr);
            PPrint(expr);
            PPrint(lexer.Expr());
            var pexpr = Parse.FromStr(expr);
            PPrint(pexpr.Expr());
            PPrint(pexpr.Eval().Expr());
            PPrint(pexpr.Eval().Value());

            ToolBox.PrattParsing.InteractiveEnvironment.Connect();
        }
    }
}