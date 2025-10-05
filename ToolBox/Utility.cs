using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBox.Container;
using static ToolBox.PPrinter;
using static ToolBox.Utility.TerminalMessageMaker;
namespace ToolBox
{
    namespace Utility
    {
        public class RepeatableValue<T>(T val) where T : IEquatable<T>
        {
            public T Value = val;
            private int RepeatCount = 0;
            public int Count => RepeatCount;

            public bool IsNotRepeat()
            {
                return RepeatCount == 0;
            }
            public bool IsRepeat()
            {
                return RepeatCount != 0;
            }
            public bool IsRepeatValue(T val)
            {
                return Value.Equals(val);
            }
            public void Repeat()
            {
                RepeatCount++;
            }
            public bool TryDeRepeat()
            {
                if (IsNotRepeat())
                {
                    return false;
                }
                else
                {
                    RepeatCount--;
                    return true;
                }
            }
            public IEnumerable<T> Flatten()
            {
                return Enumerable.Repeat(Value, Count);
            }
            public T[] FlattenArray()
            {
                return [.. Flatten()];
            }
        }
        public class AutoBackwardsRoller<T>(T val) where T : IEquatable<T>
        {
            private readonly RepeatCounterStack<T> VStack = new();
            public T Value = val;

            public void Set(T val)
            {
                VStack.Push(Value);
                Value = val;
            }

            public bool RollBack()
            {
                if (VStack.TryPop(out T val))
                {
                    Value = val;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public AutoBackwardsRollerToken Using(T val)
            {
                Set(val);
                return new(this);
            }

            public T[] InternalView() => VStack.ToFlattenArray();

            public class AutoBackwardsRollerToken(AutoBackwardsRoller<T> abr) : IDisposable
            {
                public AutoBackwardsRoller<T> InUsingABR = abr;
                private bool Done = false;
                public void Dispose()
                {
                    // 重要：防止重复释放
                    if (!Done)
                    {
                        InUsingABR.RollBack();
                        Done = true;
                    }
                    GC.SuppressFinalize(this);
                }
            }
        }


        public static class TerminalMessageMaker
        {
            public static readonly string SystemPrefix = "[System]";
            public static string Concat(params string[] msgs)
            {
                return string.Join(" ", msgs);
            }
            public static string FromSys(string msg)
            {
                return Concat(SystemPrefix, msg);
            }

        }
        public interface IAcceptableAndGivableCore<T>
        {
            public void Accept(string line);
            public string Give();
            public bool NowQuit();
            public string HowToQuit();
            public void Terminate();
        }
        public class AcceptAndGiveTerminal<T>(IAcceptableAndGivableCore<T> core, TextReader tr, TextWriter tw)
        {
            private readonly IAcceptableAndGivableCore<T> _core = core;
            private readonly TextReader ins = tr;
            private readonly TextWriter outs = tw;
            public void Initialize()
            {
                outs.WriteLine(FromSys(_core.HowToQuit()));
                outs.WriteLine(FromSys("This is a simple (core) accept and give, (usr) give and accept interactive environment in terminal for scenes such as demo etc. ."));
            }

            public void DoBeforeQuit()
            {
                outs.WriteLine(FromSys("Bye."));
            }

            public void Start()
            {
                Initialize();

                while (true)
                {
                    outs.Write(">> ");
                    string? line = ins.ReadLine();

                    _core.Accept(line ?? string.Empty);

                    outs.WriteLine(_core.Give());

                    if (_core.NowQuit())
                    {
                        break;
                    }
                }

                DoBeforeQuit();
            }

            public void End()
            {
                _core.Terminate();
            }
        }

        public static class TryConvert
        {

            public static bool ToDouble(object? obj, out double val)
            {
                return Invoker.Try(Convert.ToDouble, obj, out val);
            }

            public static bool To<T>(object? obj, out T val) where T : IConvertible
            {
                return Invoker.Try(new Func<object, T>(static  _obj => (T)Convert.ChangeType(_obj, typeof(T))), obj, out val);
            }
        }

        public static class Invoker
        {
            public static bool Try<TSrc, TRes>(Func<TSrc, TRes> f, TSrc? input, out TRes output)
            {
                try
                {
                    output = input switch
                    {
                        null => default!,
                        _ => f(input)
                    };
                }
                catch
                {
                    output = default!;
                    return false;
                }

                return true;
            }
        }
    }
}
