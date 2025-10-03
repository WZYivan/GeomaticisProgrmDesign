using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBox.Utility;

namespace ToolBox
{
    namespace Container
    {
        public class RepeatCounterStack<T> : Stack<RepeatableValue<T>> where T : IEquatable<T>
        {
            public RepeatCounterStack() : base() { }

            public RepeatCounterStack(int capacity) : base(capacity) { }

            public RepeatCounterStack(IEnumerable<RepeatableValue<T>> collection) : base(collection) { }

            [Obsolete("Use direct push <internal value> instead.", true)]
            public new void Push(RepeatableValue<T> _) { }

            [Obsolete("Use direct TryPop <internal value> instead.", true)]
            public new bool TryPop(out RepeatableValue<T> _) { _ = default!; return false; }
            [Obsolete("Use direct TryPeek <internal value> instead.", true)]
            public new bool TryPeek(out RepeatableValue<T> _) { _ = default!; return false; }

            public void Push(T val)
            {
                if (base.Count == 0)
                {
                    base.Push(new(val));
                    return;
                }

                var top = base.Peek();
                if (top.Value.Equals(val))
                {
                    top.Repeat();
                    return;
                }
                else
                {
                    base.Push(new(val));
                }
            }

            public new T Pop()
            {
                var top = base.Peek();
                if (top.TryDeRepeat())
                {
                    return top.Value;
                }
                else
                {
                    return base.Pop().Value;
                }
            }

            public bool TryPop(out T val)
            {
                try
                {
                    val = Pop();
                    return true;
                }
                catch
                {
                    val = default!;
                    return false;
                }
            }

            public new T Peek()
            {
                return base.Peek().Value;
            }
            public bool TryPeek(out T result)
            {
                var _ = base.TryPeek(out var res);
                if (res == null)
                {
                    result = default!;
                }
                else
                {
                    result = res.Value;
                }
                return _;
            }

            public T[] ToFlattenArray()
            {
                List<T> list = [];
                foreach (var item in base.ToArray())
                {
                    list.AddRange(item.Flatten());
                }
                return [.. list];
            }

            public bool Contains(T val)
            {
                return base.ToArray().Select(_ => _.Value).Contains(val);
            }
        }
    }
}
