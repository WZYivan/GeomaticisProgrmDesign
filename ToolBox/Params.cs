using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBox
{
    static public class Params
    {
        public static class Config
        {
            public static class Hint
            {
                public static bool EchoStdIn { get; set; } = true;
                public static readonly string
                    Args = "> {}",
                    Fetch = "  |-> {}";
            }
        }

        public static Dictionary<string, T> FetchFromStdIO<T>(params string[] args) where T : IParsable<T>, new()
        {
            var dict = new Dictionary<string, T>();
            string? line;
            bool fetchOk;
            foreach (var arg in args)
            {
                do
                {
                    Console.Write($"> {arg}:");
                    fetchOk = false;
                    line = Console.ReadLine();
                    if (T.TryParse(line, CultureInfo.CurrentCulture, out T? val))
                    {
                        fetchOk = true;
                        dict[arg] = val ?? new T();
                        if (Config.Hint.EchoStdIn)
                        {
                            Console.WriteLine($"  |-> {val}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  |-> [{line}] can't be parsed as {typeof(T)}");
                    }
                } while (!fetchOk);
            }
            return dict;
        }
    }
}
