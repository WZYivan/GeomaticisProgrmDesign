using System.Globalization; // 提供全球化和本地化支持
using System.Numerics; // 提供大数运算支持
using System.Text; // 提供文本处理支持
using ToolBox.Utility; // 引入工具箱的实用工具
using static ToolBox.Format; // 静态导入Format类

namespace ToolBox
{
    /// <summary>
    /// 格式化操作工具类 - 提供对象包装、格式化和打印功能
    /// </summary>
    public static class FManip
    {
        /// <summary>
        /// 泛型对象包装器 - 将任意对象包装为IFormattable接口
        /// </summary>
        public class GenericObjectWrapper(object? obj) : IFormattable
        {
            private readonly object? wrapped = obj; // 被包装的对象

            /// <summary>
            /// 将包装的对象转换为字符串
            /// </summary>
            /// <param name="s">格式字符串</param>
            /// <param name="formatProvider">格式提供者</param>
            /// <returns>格式化后的字符串</returns>
            public string ToString(string? s, IFormatProvider? formatProvider)
            {
                string str;
                if (wrapped is IFormattable formattable)
                {
                    // 如果对象本身支持格式化，则使用其格式化方法
                    str = (formattable.ToString(s, formatProvider));
                }
                else
                {
                    // 如果对象不支持格式化，则调用ToString并进行包装
                    str = wrapped?.ToString().Wrap().ToString(s, formatProvider) ?? FConfig.WrapperNullString.Value;
                }
                return str;
            }
        }

        /// <summary>
        /// 字符串包装器 - 将字符串包装为IFormattable接口
        /// </summary>
        public class FormattableStringWrapper(string? s) : IFormattable
        {
            private readonly string? wrapped = s; // 被包装的字符串

            /// <summary>
            /// 将包装的字符串转换为字符串
            /// </summary>
            /// <param name="_">格式字符串（未使用）</param>
            /// <param name="__">格式提供者（未使用）</param>
            /// <returns>包装的字符串或空值字符串</returns>
            public string ToString(string? _, IFormatProvider? __)
            {
                return wrapped ?? FConfig.WrapperNullString.Value;
            }
        }

        /// <summary>
        /// 扩展方法 - 将字符串包装为IFormattable接口
        /// </summary>
        /// <param name="s">要包装的字符串</param>
        /// <returns>字符串包装器</returns>
        public static FormattableStringWrapper Wrap(this string? s)
        {
            return new FormattableStringWrapper(s);
        }

        /// <summary>
        /// 个体对象包装器 - 将对象包装并添加特殊标识
        /// </summary>
        public class IndividualObjectWrapper(object? obj) : GenericObjectWrapper(obj), IFormattable
        {
            /// <summary>
            /// 将包装的对象转换为字符串，添加特殊标识
            /// </summary>
            /// <param name="s">格式字符串</param>
            /// <param name="formatProvider">格式提供者</param>
            /// <returns>带有特殊标识的格式化字符串</returns>
            public new string ToString(string? s, IFormatProvider? formatProvider)
            {
                return Individualize(base.ToString(s, formatProvider));
            }

            /// <summary>
            /// 为字符串添加个体化标识
            /// </summary>
            /// <param name="s">输入字符串</param>
            /// <returns>添加标识后的字符串</returns>
            private static string Individualize(string s)
            {
                return "<:" + Environment.NewLine + s + ":>";
            }
        }
        /// <summary>
        /// 扩展方法 - 将对象包装为个体对象包装器
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要包装的对象</param>
        /// <returns>个体对象包装器</returns>
        public static IndividualObjectWrapper Individual(this object? obj)
        {
            return new IndividualObjectWrapper(obj);
        }

        /// <summary>
        /// 创建参数数组 - 将对象数组包装为GenericObjectWrapper数组
        /// </summary>
        /// <param name="args">输入对象数组</param>
        /// <returns>GenericObjectWrapper数组</returns>
        public static GenericObjectWrapper[] MakePPArgs(params object[] args)
        {
            return [.. args.Select(_ => new GenericObjectWrapper(_))];
        }

        /// <summary>
        /// 创建参数数组 - 使用指定函数将输入数组转换为包装器数组
        /// </summary>
        /// <typeparam name="TWrapper">包装器类型</typeparam>
        /// <param name="f">转换函数</param>
        /// <param name="args">输入数组</param>
        /// <returns>转换后的包装器数组</returns>
        public static TWrapper[] MakePPArgs<TWrapper>(Func<object, TWrapper> f, params object[] args) where TWrapper : IFormattable
        {
            return [.. args.Select(_ => f(_))];
        }
    }

    /// <summary>
    /// 格式化工具类 - 提供格式化器和模板功能
    /// </summary>
    public static class Format
    {
        /// <summary>
        /// 格式化器 - 用于格式化IFormattable对象数组
        /// </summary>
        /// <typeparam name="T">要格式化的对象类型，必须实现IFormattable</typeparam>
        public class Formatter<T>(IEnumerable<string> names, IEnumerable<int>? places = null) where T : IFormattable
        {
            /// <summary>
            /// 复制构造函数
            /// </summary>
            /// <param name="other">要复制的格式化器</param>
            public Formatter(Formatter<T> other) : this(other.names, other.places)
            {
                this.Prefix = other.Prefix;
                this.Suffix = other.Suffix;
            }

            private readonly IEnumerable<string> names = names; // 参数名称列表
            private readonly IEnumerable<int>? places = places; // 小数位数列表（可选）
            private string Prefix { get; set; } = String.Empty; // 前缀
            private string Suffix { get; set; } = String.Empty; // 后缀

            /// <summary>
            /// 格式化参数数组
            /// </summary>
            /// <param name="args">要格式化的参数数组</param>
            /// <returns>格式化后的字符串</returns>
            public string Format(params T[] args)
            {
                var sb = new StringBuilder();
                // 开始构建格式化字符串
                sb.Append($"<{Prefix + (String.IsNullOrEmpty(Prefix) ? "" : FConfig.FormatterSeperator.Value)}");

                if (places != null)
                {
                    // 如果指定了小数位数，使用指定的格式
                    string[] format = Array.ConvertAll(places.ToArray(), p => $"0.{new string('0', p)}");
                    foreach (var ((p, n), f) in args.Zip(this.names).Zip(format))
                    {
                        sb.Append($"{n}:{p.ToString(f, CultureInfo.CurrentCulture)}{FConfig.FormatterSeperator.Value}");
                    }
                }
                else
                {
                    // 如果没有指定小数位数，直接格式化
                    foreach (var (p, n) in args.Zip(this.names))
                    {
                        sb.Append($"{n}:{p}{FConfig.FormatterSeperator.Value}");
                    }
                }

                // 移除最后的分隔符
                if (sb.Length > 1)
                {
                    sb.Remove(sb.Length - 2, 2);
                }

                // 添加后缀并闭合格式化字符串
                sb.Append($"{(String.IsNullOrEmpty(Suffix) ? "" : FConfig.FormatterSeperator.Value) + Suffix}>");
                return sb.ToString();
            }

            /// <summary>
            /// 设置前缀
            /// </summary>
            /// <param name="p">前缀字符串</param>
            /// <returns>新的格式化器实例</returns>
            public Formatter<T> Pre(string p)
            {
                Formatter<T> nf = new(this)
                {
                    Prefix = p
                };
                return nf;
            }

            /// <summary>
            /// 设置后缀
            /// </summary>
            /// <param name="s">后缀字符串</param>
            /// <returns>新的格式化器实例</returns>
            public Formatter<T> Suf(string s)
            {
                Formatter<T> nf = new(this)
                {
                    Suffix = s
                };
                return nf;
            }
        }

        /// <summary>
        /// 命名格式化器模板 - 用于创建具有指定名称的格式化器
        /// </summary>
        public class FormatterNamedTemplate
        {
            private readonly IEnumerable<string> names; // 参数名称列表

            /// <summary>
            /// 构造函数 - 从字符串集合创建模板
            /// </summary>
            /// <param name="names">参数名称集合</param>
            public FormatterNamedTemplate(IEnumerable<string> names) { this.names = names; }

            /// <summary>
            /// 构造函数 - 从字符串数组创建模板
            /// </summary>
            /// <param name="names">参数名称数组</param>
            public FormatterNamedTemplate(params string[] names) { this.names = names; }

            /// <summary>
            /// 将名称转换后创建格式化器
            /// </summary>
            /// <typeparam name="U">格式化器类型</typeparam>
            /// <param name="transformer">名称转换函数</param>
            /// <returns>转换后的格式化器</returns>
            public Formatter<U> TransformNames<U>(Func<string, string> transformer) where U : IFormattable
            {
                return new Formatter<U>(names.Select(_ => transformer(_)));
            }

            /// <summary>
            /// 创建指定小数位数的浮点数格式化器
            /// </summary>
            /// <typeparam name="U">格式化器类型，必须是浮点数类型</typeparam>
            /// <param name="places">小数位数数组</param>
            /// <returns>指定小数位数的格式化器</returns>
            public Formatter<U> Placed<U>(params int[] places) where U : IFormattable, IFloatingPoint<U>
            {
                return new Formatter<U>(names, places);
            }

            /// <summary>
            /// 创建指定小数位数的浮点数格式化器
            /// </summary>
            /// <typeparam name="U">格式化器类型，必须是浮点数类型</typeparam>
            /// <param name="places">小数位数集合</param>
            /// <returns>指定小数位数的格式化器</returns>
            public Formatter<U> Placed<U>(IEnumerable<int> places) where U : IFormattable, IFloatingPoint<U>
            {
                return new Formatter<U>(names, places);
            }

            /// <summary>
            /// 创建所有参数具有相同小数位数的格式化器
            /// </summary>
            /// <typeparam name="U">格式化器类型</typeparam>
            /// <param name="place">小数位数</param>
            /// <returns>统一小数位数的格式化器</returns>
            public Formatter<U> PlacedAll<U>(int place) where U : IFormattable
            {
                return new Formatter<U>(names, Enumerable.Repeat(place, names.Count()));
            }

            /// <summary>
            /// 创建基本格式化器
            /// </summary>
            /// <typeparam name="U">格式化器类型</typeparam>
            /// <returns>基本格式化器</returns>
            public Formatter<U> Of<U>() where U : IFormattable
            {
                return new Formatter<U>(names);
            }
        }

        /// <summary>
        /// 自动命名格式化器模板 - 基于起始字符自动生成名称
        /// </summary>
        public class FormatterAutoNamedTemplate(char beg, Func<int, char, string>? trans = null)
        {
            private readonly char beg = beg; // 起始字符
            private readonly Func<int, char, string> trans = // 名称转换函数
                trans
                ??
                (
                    FConfig.IsFormatterAutoTemplateOrderSurrounded.Value ?
                    ((i, chr) => $"[{System.Convert.ToChar(chr + i)}]") : // 如果启用包围，则添加方括号
                    ((i, chr) => $"{System.Convert.ToChar(chr + i)}") // 否则直接使用字符
                )
            ;

            /// <summary>
            /// 根据指定长度生成命名模板
            /// </summary>
            /// <param name="len">模板长度</param>
            /// <returns>生成的命名模板</returns>
            public FormatterNamedTemplate Count(int len)
            {
                return new FormatterNamedTemplate(Enumerable.Range(0, len).Select(_ => trans(_, beg)));
            }
        }

        /// <summary>
        /// 预定义模板集合
        /// </summary>
        public static class Templates
        {
            /// <summary>
            /// 创建指定名称的命名模板
            /// </summary>
            /// <param name="names">名称数组</param>
            /// <returns>命名模板</returns>
            public static FormatterNamedTemplate Named(params string[] names)
            {
                return new FormatterNamedTemplate(names);
            }

            /// <summary>
            /// 创建方括号包围的命名模板
            /// </summary>
            /// <param name="names">名称数组</param>
            /// <returns>方括号包围的命名模板</returns>
            public static FormatterNamedTemplate SqQuoted(params string[] names)
            {
                return new FormatterNamedTemplate(names.Select(_ => $"[{_}]"));
            }

            /// <summary>
            /// 创建经过函数转换的命名模板
            /// </summary>
            /// <param name="f">名称转换函数</param>
            /// <param name="names">名称数组</param>
            /// <returns>转换后的命名模板</returns>
            public static FormatterNamedTemplate TransedNamed(Func<string, string> f, params string[] names)
            {
                return new FormatterNamedTemplate(names.Select(f));
            }

            /// <summary>
            /// 预定义的命名模板集合
            /// </summary>
            public static class Names
            {
                // 常用坐标系命名模板
                public static readonly FormatterNamedTemplate
                    XY = new("x", "y"), // 二维坐标
                    XYZ = new("x", "y", "z"), // 三维坐标
                    RPhi = new("r", "phi", "deg"), // 极坐标
                    BLH = new("b", "l", "h"); // 大地坐标

                /// <summary>
                /// 自动命名模板集合
                /// </summary>
                public static class Auto
                {
                    // 不同起始字符的自动命名模板
                    public static readonly FormatterAutoNamedTemplate
                        Zero = new('0'), // 数字0开始
                        One = new('1'), // 数字1开始
                        UpperA = new('A'), // 大写字母A开始
                        LowerA = new('a'); // 小写字母a开始
                }
            }

            /// <summary>
            /// 生成指定小数位数的格式化器
            /// </summary>
            /// <typeparam name="T">格式化器类型</typeparam>
            /// <param name="fnt">命名模板</param>
            /// <param name="p">小数位数</param>
            /// <returns>指定小数位数的格式化器</returns>
            private static Formatter<T> GenPlacedAll<T>(FormatterNamedTemplate fnt, int p) where T : IFormattable
            {
                return fnt.PlacedAll<T>(p);
            }

            /// <summary>
            /// 预定义的双精度浮点数格式化器集合
            /// </summary>
            public static class Double
            {
                // 生成指定小数位数的双精度浮点数格式化器
                private static Formatter<double> Gen(FormatterNamedTemplate fnt, int p) => GenPlacedAll<double>(fnt, p);
                public static readonly Formatter<double>
                    XY = Gen(Names.XY, 3), // 二维坐标，3位小数
                    XYZ = Gen(Names.XYZ, 3), // 三维坐标，3位小数
                    RPhi = Gen(Names.RPhi, 3), // 极坐标，3位小数
                    BLH = Gen(Names.BLH, 3); // 大地坐标，3位小数
            }
        }
    }

    /// <summary>
    /// 美化打印器 - 提供格式化打印功能
    /// </summary>
    public static class PPrinter
    {
        /// <summary>
        /// 直接打印字符串
        /// </summary>
        /// <param name="s">要打印的字符串</param>
        public static void PPrint(string s)
        {
            Console.WriteLine(Environment.NewLine + "# " + s + Environment.NewLine);
        }

        /// <summary>
        /// 打印对象数组（使用字符串包装）
        /// </summary>
        /// <param name="ps">对象数组</param>
        public static void PPrint(params object[] ps)
        {
            PPrint<FManip.FormattableStringWrapper>([.. ps.Select(_ => _.ToString().Wrap())]);
        }

        /// <summary>
        /// 打印IFormattable对象数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="ps">对象数组</param>
        public static void PPrint<T>(params T[] ps) where T : IFormattable
        {
            PPrint(Templates.Names.Auto.One, ps);
        }

        /// <summary>
        /// 使用指定格式化器打印对象数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="format">格式化器</param>
        /// <param name="ps">对象数组</param>
        public static void PPrint<T>(Formatter<T> format, params T[] ps) where T : IFormattable
        {
            PPrint(Console.Out, format, ps);
        }

        /// <summary>
        /// 使用自动命名格式化器打印对象数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="format">自动命名格式化器</param>
        /// <param name="ps">对象数组</param>
        public static void PPrint<T>(FormatterAutoNamedTemplate format, params T[] ps) where T : IFormattable
        {
            PPrint(Console.Out, format, ps);
        }

        /// <summary>
        /// 使用命名模板打印对象数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="format">命名模板</param>
        /// <param name="ps">对象数组</param>
        public static void PPrint<T>(FormatterNamedTemplate format, params T[] ps) where T : IFormattable
        {
            PPrint(Console.Out, format, ps);
        }

        /// <summary>
        /// 使用自动命名格式化器向指定输出流打印对象数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="os">输出流</param>
        /// <param name="format">自动命名格式化器</param>
        /// <param name="ps">对象数组</param>
        public static void PPrint<T>(TextWriter os, FormatterAutoNamedTemplate format, params T[] ps) where T : IFormattable
        {
            os.WriteLine(format.Count(ps.Length).Of<T>().Format(ps));
        }

        /// <summary>
        /// 使用命名模板向指定输出流打印对象数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="os">输出流</param>
        /// <param name="format">命名模板</param>
        /// <param name="ps">对象数组</param>
        public static void PPrint<T>(TextWriter os, FormatterNamedTemplate format, params T[] ps) where T : IFormattable
        {
            os.WriteLine(format.Of<T>().Format(ps));
        }

        /// <summary>
        /// 使用格式化器向指定输出流打印对象数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="os">输出流</param>
        /// <param name="format">格式化器</param>
        /// <param name="ps">对象数组</param>
        public static void PPrint<T>(TextWriter os, Formatter<T> format, params T[] ps) where T : IFormattable
        {
            os.WriteLine(format.Format(ps));
        }
    }

    /// <summary>
    /// 格式化配置结构 - 存储格式化相关的配置选项
    /// </summary>
    public struct FConfig
    {
        // 配置项定义
        public static AutoBackwardsRoller<string>
                WrapperNullString = new("<wrapper::null>"), // 包装器空值字符串
                FormatterSeperator = new(", "); // 格式化分隔符
        public static AutoBackwardsRoller<bool>
            IsFormatterAutoTemplateOrderSurrounded = new(true); // 格式化器自动模板是否包围
    }
}
