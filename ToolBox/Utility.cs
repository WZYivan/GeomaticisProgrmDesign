using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBox.Container;

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
                if(VStack.TryPop(out T val))
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

            public T[] InernalView() => VStack.ToFlattenArray();

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
    
        
    }
}
