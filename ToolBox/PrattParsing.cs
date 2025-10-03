using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBox.PrattParsing.Expression;
using ToolBox.PrattParsing.Token;
using ToolBox.Utility;

namespace ToolBox
{
    namespace PrattParsing
    {
        namespace Token
        {
            /// <summary>
            /// 令牌类型枚举，定义了不同类型的令牌
            /// </summary>
            public enum TokenType
            {
                Atom,       // 原子类型（数字）
                Operator,   // 操作符类型
                Eof,        // 文件结束符
                Prefix,     // 前缀操作符
                Suffix,     // 后缀操作符
                AlgebraAtom // 代数原子（变量）
            }

            /// <summary>
            /// 令牌接口，定义了令牌的基本行为
            /// </summary>
            public interface IToken : IFormattable
            {
                /// <summary>
                /// 检查令牌是否有效
                /// </summary>
                public void CheckValid();

                /// <summary>
                /// 获取令牌的表达式字符串
                /// </summary>
                public string Expr();

                /// <summary>
                /// 计算操作符的值（默认未实现）
                /// </summary>
                public double Eval(params Atom[] atoms) => throw new NotImplementedException();

                /// <summary>
                /// 获取原子的值（默认未实现）
                /// </summary>
                public double Value() => throw new NotImplementedException();

                /// <summary>
                /// 获取令牌类型（内部方法）
                /// </summary>
                internal TokenType TypeOf();

                /// <summary>
                /// 执行令牌操作（根据令牌类型调用相应方法）
                /// </summary>
                public double Invoke(params Atom[] atoms)
                {
                    return (TypeOf()) switch
                    {
                        TokenType.Atom => Value(),      // 原子类型返回值
                        TokenType.Operator => Eval(atoms), // 操作符类型执行计算
                        _ => throw new InvalidOperationException("this type not register") // 未注册类型抛出异常
                    };
                }

                /// <summary>
                /// 实现IFormattable接口的ToString方法
                /// </summary>
                string IFormattable.ToString(string? _, IFormatProvider? __) => Expr();
            }

            /// <summary>
            /// 原子类，表示数字原子
            /// </summary>
            public class Atom : IToken
            {
                private string _str = String.Empty; // 存储原子的字符串表示

                /// <summary>
                /// 获取原子的数值
                /// </summary>
                public double Value() => Convert.ToDouble(_str);

                /// <summary>
                /// 获取原子的表达式字符串
                /// </summary>
                public string Expr() => _str;

                /// <summary>
                /// 获取令牌类型
                /// </summary>
                public TokenType TypeOf() => TokenType.Atom;

                /// <summary>
                /// 检查原子是否有效（是否为数字）
                /// </summary>
                public void CheckValid()
                {
                    try
                    {
                        Double.Parse(_str); // 尝试解析为数字
                    }
                    catch
                    {
                        throw new ArgumentException(_str); // 解析失败抛出异常
                    }
                }

                /// <summary>
                /// 与另一个原子合并
                /// </summary>
                public Atom MergeWith(Atom other)
                {
                    _str += other.Expr(); // 连接字符串
                    CheckValid(); // 验证合并后的字符串
                    return this;
                }

                /// <summary>
                /// 与另一个原子连接为小数点格式
                /// </summary>
                public Atom DotPoint(Atom other)
                {
                    _str = $"{_str}.{other.Expr()}"; // 格式化为小数
                    return this;
                }

                /// <summary>
                /// 用字符串构造原子
                /// </summary>
                public Atom(string str)
                {
                    _str = str;
                    CheckValid();
                }

                /// <summary>
                /// 用字符构造原子
                /// </summary>
                public Atom(char chr)
                {
                    _str = $"{chr}";
                    CheckValid();
                }

                /// <summary>
                /// 用双精度数构造原子
                /// </summary>
                public Atom(double d)
                {
                    _str = $"{d}";
                    CheckValid();
                }
            }

            /// <summary>
            /// 操作符类，表示各种操作符
            /// </summary>
            public class Operator : IToken
            {
                private readonly string _str = String.Empty; // 操作符字符串

                /// <summary>
                /// 用字符串构造操作符
                /// </summary>
                public Operator(string str)
                {
                    _str = str;
                    CheckValid();
                }

                /// <summary>
                /// 用字符构造操作符
                /// </summary>
                public Operator(char chr)
                {
                    _str = $"{chr}";
                    CheckValid();
                }

                /// <summary>
                /// 检查操作符是否有效
                /// </summary>
                public void CheckValid()
                {
                    if (!OperatorInfo.IsOp(_str)) // 检查是否为有效操作符
                    {
                        throw new ArgumentException(_str); // 无效操作符抛出异常
                    }
                }

                /// <summary>
                /// 获取操作符的表达式字符串
                /// </summary>
                public string Expr() => _str;

                /// <summary>
                /// 执行操作符计算
                /// </summary>
                public double Eval(params Atom[] atoms)
                {
                    return OperatorInfo.EvalOp(this, atoms); // 使用操作符信息执行计算
                }

                /// <summary>
                /// 获取令牌类型
                /// </summary>
                public TokenType TypeOf() => TokenType.Operator;
            }

            /// <summary>
            /// 代数原子类，表示变量
            /// </summary>
            public class AlgebraAtom : IToken
            {
                private string _str = String.Empty; // 变量名
                private double? _value = default;   // 变量值（可空）

                /// <summary>
                /// 获取变量的值
                /// </summary>
                public double Value() => _value ?? 0.0;

                /// <summary>
                /// 获取变量的表达式字符串
                /// </summary>
                public string Expr() => _str;

                /// <summary>
                /// 获取令牌类型
                /// </summary>
                public TokenType TypeOf() => TokenType.AlgebraAtom;

                /// <summary>
                /// 检查变量是否有效（此实现中总是有效）
                /// </summary>
                public void CheckValid()
                {
                }

                /// <summary>
                /// 与另一个代数原子合并
                /// </summary>
                public AlgebraAtom MergeWith(AlgebraAtom other)
                {
                    _str += other.Expr(); // 连接变量名
                    CheckValid();
                    return this;
                }

                /// <summary>
                /// 用字符串构造代数原子
                /// </summary>
                public AlgebraAtom(string str)
                {
                    _str = str;
                }

                /// <summary>
                /// 用字符构造代数原子
                /// </summary>
                public AlgebraAtom(char chr)
                {
                    _str = $"{chr}";
                }

                /// <summary>
                /// 为变量赋值
                /// </summary>
                public void Assign(double v) => _value = v;
            }

            /// <summary>
            /// 操作符信息类，包含所有操作符的定义和实现
            /// </summary>
            public static class OperatorInfo
            {
                /// <summary>
                /// 有效的操作符列表
                /// </summary>
                public static readonly List<string> ValidOp = ["+", "-", "*", "/", "(", ")", "sin", "cos", "tan", "^", "sqrt", ".", "!"];

                /// <summary>
                /// 前缀操作符列表
                /// </summary>
                public static readonly List<string> PrefixOp = ["-", "sin", "cos", "tan", "sqrt"];

                /// <summary>
                /// 后缀操作符列表
                /// </summary>
                public static readonly List<string> SuffixOp = ["!"];

                /// <summary>
                /// 算法库，包含各种操作符的实现
                /// </summary>
                public static class AlgoLib
                {
                    /// <summary>
                    /// 加法操作
                    /// </summary>
                    public static double Plus(params Atom[] atoms)
                    {
                        return atoms[0].Value() + atoms[1].Value();
                    }

                    /// <summary>
                    /// 减法操作
                    /// </summary>
                    public static double Minus(params Atom[] atoms)
                    {
                        return atoms[0].Value() - atoms[1].Value();
                    }

                    /// <summary>
                    /// 乘法操作
                    /// </summary>
                    public static double Multiply(params Atom[] atoms)
                    {
                        return atoms[0].Value() * atoms[1].Value();
                    }

                    /// <summary>
                    /// 除法操作
                    /// </summary>
                    public static double Divide(params Atom[] atoms)
                    {
                        return atoms[0].Value() / atoms[1].Value();
                    }

                    /// <summary>
                    /// 负号操作
                    /// </summary>
                    public static double Negative(params Atom[] atoms)
                    {
                        return -atoms[0].Value();
                    }

                    /// <summary>
                    /// 正弦操作
                    /// </summary>
                    public static double Sin(params Atom[] atoms)
                    {
                        return Math.Sin(atoms[0].Value());
                    }

                    /// <summary>
                    /// 余弦操作
                    /// </summary>
                    public static double Cos(params Atom[] atoms)
                    {
                        return Math.Cos(atoms[0].Value());
                    }

                    /// <summary>
                    /// 正切操作
                    /// </summary>
                    public static double Tan(params Atom[] atoms)
                    {
                        return Math.Tan(atoms[0].Value());
                    }

                    /// <summary>
                    /// 幂操作
                    /// </summary>
                    public static double Power(params Atom[] atoms)
                    {
                        return Math.Pow(atoms[0].Value(), atoms[1].Value());
                    }

                    /// <summary>
                    /// 开方操作
                    /// </summary>
                    public static double SqrtBy(params Atom[] atoms)
                    {
                        return Math.Pow(atoms[0].Value(), 1.0 / atoms[1].Value());
                    }

                    /// <summary>
                    /// 平方根操作
                    /// </summary>
                    public static double Sqrt(params Atom[] atoms)
                    {
                        return Math.Sqrt(atoms[0].Value());
                    }

                    /// <summary>
                    /// 阶乘操作
                    /// </summary>
                    public static double Fraction(params Atom[] atoms)
                    {
                        return FractionImpl(atoms[0].Value());
                    }

                    /// <summary>
                    /// 阶乘实现（递归）
                    /// </summary>
                    public static double FractionImpl(double v)
                    {
                        return v switch
                        {
                            0 => 0,
                            1 => 1,
                            _ => FractionImpl(v - 1) * v // 递归计算阶乘
                        };
                    }
                }

                /// <summary>
                /// 获取操作符对应的函数
                /// </summary>
                public static Func<Atom[], double> OpFunc(Operator op, int argc)
                {
                    return (op.Expr()) switch
                    {
                        "+" => AlgoLib.Plus, // 加法
                        "-" => (argc) switch // 减法或负号（根据参数个数）
                        {
                            1 => AlgoLib.Negative, // 一元负号
                            2 => AlgoLib.Minus,     // 二元减法
                            _ => default!
                        },
                        "*" => AlgoLib.Multiply, // 乘法
                        "/" => AlgoLib.Divide,   // 除法
                        "sin" => AlgoLib.Sin,    // 正弦
                        "cos" => AlgoLib.Cos,    // 余弦
                        "tan" => AlgoLib.Tan,    // 正切
                        "^" => AlgoLib.Power,    // 幂
                        "sqrt" => (argc) switch // 开方（根据参数个数）
                        {
                            1 => AlgoLib.Sqrt,    // 平方根
                            2 => AlgoLib.SqrtBy,  // n次方根
                            _ => default!
                        },
                        "!" => AlgoLib.Fraction, // 阶乘
                        _ => default!
                    };
                }

                /// <summary>
                /// 获取操作符的左右优先级
                /// </summary>
                internal static (double left, double right) Power(Operator op)
                {
                    return (op.Expr()) switch
                    {
                        "+" or "-" => (1.0, 1.1),    // 加减法优先级
                        "*" or "/" => (2.0, 2.1),    // 乘除法优先级
                        "^" or "sqrt" => (3.1, 3.0), // 幂和开方优先级
                        _ when op.IsSufOp() => SufPower(op), // 后缀操作符
                        _ when op.IsPreOp() => PrePower(op), // 前缀操作符
                        _ => default!
                    };
                }

                /// <summary>
                /// 获取前缀操作符的优先级
                /// </summary>
                internal static (double left, double right) PrePower(Operator op)
                {
                    var rPower = op.Expr() switch
                    {
                        _ when op.IsPreOp() => 100, // 前缀操作符右优先级
                        _ => default!
                    };
                    return (0.0, rPower);
                }

                /// <summary>
                /// 获取后缀操作符的优先级
                /// </summary>
                internal static (double left, double right) SufPower(Operator op)
                {
                    var lPower = op.Expr() switch
                    {
                        _ when op.IsSufOp() => 100, // 后缀操作符左优先级
                        _ => default!
                    };
                    return (lPower, 0.0);
                }

                /// <summary>
                /// 执行操作符计算
                /// </summary>
                public static double EvalOp(Operator op, params Atom[] atoms)
                {
                    return OpFunc(op, atoms.Length)(atoms); // 获取函数并执行
                }

                /// <summary>
                /// 检查字符串是否为有效操作符
                /// </summary>
                public static bool IsOp(string s)
                {
                    return ValidOp.Contains(s);
                }

                /// <summary>
                /// 检查字符是否为有效操作符
                /// </summary>
                public static bool IsOp(char s)
                {
                    return ValidOp.Any(_ => _ == $"{s}");
                }

                /// <summary>
                /// 检查操作符是否为前缀操作符
                /// </summary>
                public static bool IsPreOp(this Operator op)
                {
                    return PrefixOp.Contains(op.Expr());
                }

                /// <summary>
                /// 检查操作符是否为后缀操作符
                /// </summary>
                public static bool IsSufOp(this Operator op)
                {
                    return SuffixOp.Contains(op.Expr());
                }

                /// <summary>
                /// 获取操作符的位置类型
                /// </summary>
                public static TokenType OpPosition(this Operator op)
                {
                    return op switch
                    {
                        _ when op.IsPreOp() => TokenType.Prefix,  // 前缀操作符
                        _ when op.IsSufOp() => TokenType.Suffix,  // 后缀操作符（这里可能是错误）
                        _ => TokenType.Operator,                  // 普通操作符
                    };
                }
            }

            /// <summary>
            /// 文件结束符类
            /// </summary>
            public class Eof : IToken
            {
                public Eof() { }

                /// <summary>
                /// 检查EOF是否有效（总是有效）
                /// </summary>
                public void CheckValid() { }

                /// <summary>
                /// 获取EOF的表达式字符串
                /// </summary>
                public string Expr() => "<Eof>";

                /// <summary>
                /// 获取EOF的令牌类型
                /// </summary>
                public TokenType TypeOf() => TokenType.Eof;
            }
        }

        namespace Expression
        {
            /// <summary>
            /// 表达式类型枚举
            /// </summary>
            public enum ExpressionType
            {
                Atom,      // 原子表达式
                Operation  // 操作表达式
            }

            /// <summary>
            /// 表达式接口，定义表达式的基本行为
            /// </summary>
            public interface IExpression
            {
                /// <summary>
                /// 获取表达式类型
                /// </summary>
                public ExpressionType TypeOf();

                /// <summary>
                /// 获取表达式的字符串表示
                /// </summary>
                public string Expr();

                /// <summary>
                /// 计算表达式
                /// </summary>
                public IExpression Eval();

                /// <summary>
                /// 获取表达式的值
                /// </summary>
                public double Value();
            }

            /// <summary>
            /// 原子表达式类
            /// </summary>
            public class AtomExpr(Atom atom) : IExpression
            {
                private readonly Atom _atom = atom; // 原子对象

                /// <summary>
                /// 获取表达式类型
                /// </summary>
                public ExpressionType TypeOf() => ExpressionType.Atom;

                /// <summary>
                /// 获取表达式的字符串表示
                /// </summary>
                public string Expr() => _atom.Expr();

                /// <summary>
                /// 获取表达式的值
                /// </summary>
                public double Value() => _atom.Value();

                /// <summary>
                /// 计算表达式（返回自身）
                /// </summary>
                public IExpression Eval() => this;

                /// <summary>
                /// 获取原子令牌
                /// </summary>
                public Atom Token() => _atom;

                /// <summary>
                /// 用字符串构造原子表达式
                /// </summary>
                public AtomExpr(string s) : this(new Atom(s))
                {
                }

                /// <summary>
                /// 用令牌构造原子表达式
                /// </summary>
                public AtomExpr(IToken tok) : this((Atom)(tok)) { }
            }

            /// <summary>
            /// 操作表达式类
            /// </summary>
            public class Operation(Operator oprt, List<IExpression> exprs) : IExpression
            {
                private readonly Operator op = oprt;          // 操作符
                private readonly List<IExpression> _subExpr = exprs; // 子表达式列表

                /// <summary>
                /// 获取表达式类型
                /// </summary>
                public ExpressionType TypeOf() => ExpressionType.Operation;

                /// <summary>
                /// 获取表达式的字符串表示
                /// </summary>
                public string Expr()
                {
                    if (AllAtom()) // 如果所有子表达式都是原子
                    {
                        return $"({op.Expr()} {String.Join(" ", _subExpr.Select(_ => _.Expr()))})";
                    }
                    StringBuilder sb = new();
                    sb.Append($"({op.Expr()} ");
                    sb.Append(String.Join(" ", _subExpr.Select(_ => _.Expr())));
                    sb.Append(')');
                    return sb.ToString();
                }

                /// <summary>
                /// 计算表达式
                /// </summary>
                public IExpression Eval()
                {
                    if (AllAtom()) // 如果所有子表达式都是原子
                    {
                        return new AtomExpr(new Atom(op.Eval([.. _subExpr.Select(_ => ((AtomExpr)_).Token())]))); // 执行计算
                    }

                    return new Operation(op, [.. _subExpr.Select(_ => _.Eval())]); // 递归计算子表达式
                }

                /// <summary>
                /// 获取表达式的值
                /// </summary>
                public double Value()
                {
                    if (AllAtom()) // 如果所有子表达式都是原子
                    {
                        return op.Eval([.. _subExpr.Select(_ => ((AtomExpr)_).Token())]); // 直接计算
                    }
                    else
                    {
                        return Eval().Value(); // 先计算再取值
                    }
                }

                /// <summary>
                /// 检查是否所有子表达式都是原子
                /// </summary>
                internal bool AllAtom()
                {
                    return _subExpr.All(_ => _.TypeOf() == ExpressionType.Atom);
                }
            }
        }

        /// <summary>
        /// 词法分析器类，将表达式字符串转换为令牌序列
        /// </summary>
        public class Lexer
        {
            private readonly Stack<IToken> _tokens = []; // 令牌栈

            /// <summary>
            /// 用表达式字符串构造词法分析器
            /// </summary>
            public Lexer(string expr)
            {
                List<IToken> _items = new(expr.Length); // 创建令牌列表
                var ca = expr.ToCharArray(); // 转换为字符数组
                foreach (var item in ca)
                {
                    if (item == ' ') // 跳过空格
                    {
                        continue;
                    }
                    if (OperatorInfo.IsOp(item)) // 如果是操作符
                    {
                        _items.Add(new Operator(item));
                    }
                    else if (item >= '0' && item <= '9') // 如果是数字
                    {
                        _items.Add(new Atom(item));
                    }
                    else // 否则是代数原子（变量）
                    {
                        _items.Add(new AlgebraAtom(item));
                    }
                }

                MergeAtoms(_items); // 合并相邻的原子
                ConvertAlgebraToOperator(_items); // 转换代数原子为操作符
                _items.Add(new Eof()); // 添加结束符
                _items.Reverse(); // 反转列表

                _tokens = new(_items); // 初始化令牌栈
            }

            /// <summary>
            /// 合并相邻的原子
            /// </summary>
            internal static void MergeAtoms(List<IToken> _items)
            {
                MergeAdjacentAtom(_items);      // 合并相邻数字原子
                MergeDotPoint(_items);          // 处理小数点
                MergeAdjacentAlgebraAtom(_items); // 合并相邻代数原子
            }

            /// <summary>
            /// 合并相邻的数字原子
            /// </summary>
            internal static void MergeAdjacentAtom(List<IToken> _items)
            {
                for (int i = _items.Count - 1; i > 0; --i)
                {
                    if (_items[i - 1].TypeOf() == TokenType.Atom &&
                       _items[i].TypeOf() == TokenType.Atom)
                    {
                        var lhs = (Atom)_items[i - 1];
                        var rhs = (Atom)_items[i];

                        lhs.MergeWith(rhs); // 合并原子

                        _items.RemoveAt(i); // 移除已合并的原子
                    }
                }
            }

            /// <summary>
            /// 处理小数点
            /// </summary>
            internal static void MergeDotPoint(List<IToken> _items)
            {
                for (int i = _items.Count - 3; i >= 0; --i)
                {
                    if (_items[i + 2].TypeOf() == TokenType.Atom &&
                       _items[i].TypeOf() == TokenType.Atom &&
                       _items[i + 1].Expr() == ".")
                    {
                        var lhs = (Atom)_items[i];
                        var rhs = (Atom)_items[i + 2];

                        lhs.DotPoint(rhs); // 连接为小数

                        _items.RemoveAt(i + 2); // 移除右侧原子
                        _items.RemoveAt(i + 1); // 移除小数点
                        i -= 3; // 调整索引
                    }
                }
            }

            /// <summary>
            /// 合并相邻的代数原子
            /// </summary>
            internal static void MergeAdjacentAlgebraAtom(List<IToken> _items)
            {
                for (int i = _items.Count - 1; i > 0; --i)
                {
                    if (_items[i - 1].TypeOf() == TokenType.AlgebraAtom &&
                       _items[i].TypeOf() == TokenType.AlgebraAtom)
                    {
                        var lhs = (AlgebraAtom)_items[i - 1];
                        var rhs = (AlgebraAtom)_items[i];

                        lhs.MergeWith(rhs); // 合并代数原子

                        _items.RemoveAt(i); // 移除已合并的原子
                    }
                }
            }

            /// <summary>
            /// 将代数原子转换为操作符（如果它们是有效的操作符）
            /// </summary>
            internal static void ConvertAlgebraToOperator(List<IToken> _items)
            {
                for (int i = 0; i != _items.Count; ++i)
                {
                    var item = _items[i];
                    var expr = item.Expr();
                    if (item.TypeOf() == TokenType.AlgebraAtom &&
                        OperatorInfo.IsOp(expr)) // 如果是代数原子且是有效操作符
                    {
                        _items[i] = new Operator(expr); // 转换为操作符
                    }
                }
            }

            /// <summary>
            /// 获取词法分析器的表达式字符串
            /// </summary>
            public string Expr()
            {
                return String.Join(Environment.NewLine, _tokens.Select(_ => _.Expr()));
            }

            /// <summary>
            /// 将令牌栈转换为列表
            /// </summary>
            public List<IToken> ToList() => [.. _tokens.Reverse()];

            /// <summary>
            /// 获取下一个令牌（并从栈中弹出）
            /// </summary>
            public IToken Next()
            {
                return _tokens.Pop();
            }

            /// <summary>
            /// 查看下一个令牌（不从栈中弹出）
            /// </summary>
            public IToken Peek()
            {
                return _tokens.Peek();
            }

            /// <summary>
            /// 回滚令牌（将令牌压回栈）
            /// </summary>
            public void RollBack(IToken tk)
            {
                _tokens.Push(tk);
            }
        }

        /// <summary>
        /// 解析器类，使用Pratt解析算法解析表达式
        /// </summary>
        public static class Parse
        {
            /// <summary>
            /// 从字符串解析表达式
            /// </summary>
            public static Expression.IExpression FromStr(string str)
            {
                Lexer lexer = new(str); // 创建词法分析器
                return ParseExpr(lexer, 0.0); // 开始解析
            }

            /// <summary>
            /// 解析表达式（Pratt解析算法的核心）
            /// </summary>
            internal static IExpression ParseExpr(Lexer lexer, double power)
            {
                var lhsTk = lexer.Next(); // 获取左侧令牌
                IExpression lhs; // 左侧表达式

                if (lhsTk.TypeOf() == TokenType.Atom) // 如果是原子
                {
                    lhs = new AtomExpr(lhsTk);
                }
                else if (lhsTk.Expr() == "(") // 如果是左括号
                {
                    lhs = ParseExpr(lexer, 0.0); // 递归解析括号内表达式
                    if (lexer.Next().Expr() != ")") // 检查是否有匹配的右括号
                    {
                        throw new ArgumentException("Unbraced \")\"");
                    }
                    if (lexer.Peek().TypeOf() == TokenType.Eof) // 如果是文件结尾
                    {
                        return lhs;
                    }
                }
                else if (((Operator)lhsTk).IsPreOp()) // 如果是前缀操作符
                {
                    var op = (Operator)lhsTk;
                    lhs = new Operation(op, [ParseExpr(lexer, OperatorInfo.PrePower(op).right)]); // 解析操作数
                }
                else
                {
                    throw new ArgumentException($"bad token {lhsTk.Expr()}"); // 无效令牌
                }

                while (true) // 处理中缀操作符
                {
                __read_mid_optk__: // 标签，用于处理后缀操作符
                    var opTk = lexer.Peek(); // 查看操作符令牌
                    if (opTk.TypeOf() == TokenType.Eof || opTk.Expr() == ")") // 如果是文件结尾或右括号
                    {
                        break;
                    }
                    var op = (Operator)opTk;
                    bool suf = op.IsSufOp(); // 检查是否为后缀操作符

                    if (suf) // 如果是后缀操作符
                    {
                        lhs = new Operation(op, [lhs]); // 创建后缀操作
                        lexer.Next(); // 消费操作符
                        goto __read_mid_optk__; // 继续读取
                    }

                    var (lPower, rPower) = OperatorInfo.Power(op); // 获取操作符优先级
                    if (lPower < power) // 如果左优先级小于当前优先级
                    {
                        break; // 停止解析
                    }

                    if (opTk.TypeOf() == TokenType.Operator) // 如果是普通操作符
                    {
                        lexer.Next(); // 消费操作符
                    }
                    else
                    {
                        throw new ArgumentException($"bad token {opTk.Expr()}"); // 无效令牌
                    }

                    var rhs = ParseExpr(lexer, rPower); // 解析右侧表达式
                    lhs = new Operation(op, [lhs, rhs]); // 创建操作表达式
                }

                return lhs; // 返回解析结果
            }
        }

        internal class PrattParsingInteractiveCore : IAcceptableAndGivableCore<PrattParsingInteractiveCore>
        {
            private string _inline = String.Empty, _outline = String.Empty;
            private IExpression _expr = default!;
            private bool _exit = false;
            public void Accept(string line)
            {
                _inline = line;
                _exit = line switch
                {
                    "q" or "quit" or "exit" => true,
                    _ => false
                };
                try
                {
                    _expr = PrattParsing.Parse.FromStr(_inline);
                }
                catch (Exception ex)
                {
                    _outline = $"[Error] {ex.Message}";
                    return;
                }

                _outline = $"[Echo] {_inline}\n" +
                    $"[Parse] {_expr.Expr()}\n" +
                    $"[Eval] {_expr.Value()}";
            }

            public string Give()
            {
                return _outline;
            }
            public string HowToQuit() => "Type \"q|quit|exit\" line to quit.";
            public bool NowQuit() => _exit;
            public void Terminate() => _exit = true;
        }

        public static class InteractiveEnvironment
        {
            internal static AcceptAndGiveTerminal<PrattParsingInteractiveCore> _shell = new(new PrattParsingInteractiveCore(), Console.In, Console.Out);
            public static void Connect()
            {
                _shell.Start();
            }

            public static void Disconnect()
            {
                _shell.End();
            }
        }
    }
}