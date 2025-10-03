using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBox
{
    namespace Linalg.Array
    {
        /// <summary>
        /// NumPy的部分接口
        /// </summary>
        public static class NumPy
        {
            /// <summary>
            /// 在线性空间中均匀地生成一系列点
            /// </summary>
            /// <param name="beg"></param>
            /// <param name="end"></param>
            /// <param name="count"></param>
            /// <param name="random">随机生成替代均匀生成</param>
            /// <returns>值序列</returns>
            public static double[] LinaSpace(double beg, double end, int count, bool random = false)
            {
                double[] ret = new double[count];
                double range = end - beg;

                if (random) 
                { 
                    var rng = new Random();
                    for (int i = 0; i < count; ++i)
                    {
                        ret[i] = beg + range * rng.NextDouble();
                    }
                    return ret;
                }
                
                for (int i = 0; i < count; ++i)
                {
                    ret[i] = beg + range * ((double)i / count);
                }
                return ret;
            }
            /// <summary>
            /// 在行方向上堆叠矩阵，m1在上，m2在下
            /// </summary>
            /// <param name="m1"></param>
            /// <param name="m2"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static double[,] StackInRow(double[,] m1, double[,] m2)
            {
                if(m1.ColLength() != m2.ColLength())
                {
                    throw new ArgumentException("m1, m2 Col Size Not Match");
                }

                int r1 = m1.RowLength(), r2 = m2.RowLength(), c = m1.ColLength();
                var res = new double[r1 + r2, c];

                for(int r=0; r!=r1; r++)
                {
                    for(int i=0; i!=c; ++i)
                    {
                        res[r, i] = m1[r, i];
                    }
                }
                for (int r = 0; r != r2; r++)
                {
                    for (int i = 0; i != c; ++i)
                    {
                        res[r+r1, i] = m1[r, i];
                    }
                }

                return res;
            }
            /// <summary>
            /// 连接两向量，a前b后
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static double[] Contact(double[] a, double[] b) 
            {
                int aL = a.Length, bL = b.Length;
                var c = new double[aL + bL];
                for(int i=0; i!=aL; ++i)
                {
                    c[i] = a[i];
                }
                for (int i = 0; i != bL; ++i)
                {
                    c[i+aL] = b[i];
                }
                return c;
            }
            /// <summary>
            /// 将源数据按比例缩放至新范围
            /// </summary>
            /// <param name="sou"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="offsetPercet">新范围的偏移比例</param>
            /// <returns></returns>
            public static double[] ShrinkToFit(double[] sou, double min, double max, double offsetPercet=0.0)
            {
                double
                    sMin = sou.Min(),
                    sMax = sou.Max(),
                    sRange = sMax - sMin,
                    range = max - min;
                sMin -= sRange * offsetPercet / 2;
                sMax += sRange * offsetPercet / 2;
                sRange = sMax - sMin;
                return sou.Select(x => (x - sMin) / sRange * range).ToArray();
            }
            /// <summary>
            /// 计算数据的多次差分
            /// </summary>
            /// <param name="sou">原始数据数组</param>
            /// <param name="degree">差分阶数（正整数）</param>
            /// <returns>差分结果数组</returns>
            public static double[] Difference(double[] sou, int degree)
            {
                // 验证输入参数
                if (sou == null) throw new ArgumentNullException(nameof(sou));
                if (degree <= 0) throw new ArgumentException("差分阶数必须为正整数", nameof(degree));

                int n = sou.Length;

                // 边界情况处理
                if (n == 0) return System.Array.Empty<double>();
                if (degree >= n) return System.Array.Empty<double>();

                // 复制原始数据以避免修改原数组
                double[] result = (double[])sou.Clone();

                // 进行多阶差分
                for (int d = 0; d < degree; d++)
                {
                    // 当前差分阶的有效数据长度
                    int currentLength = n - (d + 1);

                    // 计算当前阶的差分
                    for (int i = 0; i < currentLength; i++)
                    {
                        result[i] = result[i + 1] - result[i];
                    }
                }

                // 返回差分结果（截取有效部分）
                return result.Take(n - degree).ToArray();
            }
            /// <summary>
            /// 标准化
            /// </summary>
            /// <param name="arr"></param>
            /// <returns></returns>
            public static double[] Standard(double[] arr)
            {
                double
                    mean = arr.Average(),
                    count = arr.Count(),
                    std = Math.Sqrt(arr.Select(_ => Math.Pow(_ - mean, 2)).Sum() / count);
                return arr.Select(_ => (_ - mean) / std).ToArray();
            }
            /// <summary>
            /// 标准差
            /// </summary>
            /// <param name="arr"></param>
            /// <returns></returns>
            public static double StandradError(double[] arr)
            {
                double
                    mean = arr.Average(),
                    count = arr.Count(),
                    std = Math.Sqrt(arr.Select(_ => Math.Pow(_ - mean, 2)).Sum() / count);
                return std;
            }
            /// <summary>
            /// 斐波那契数
            /// </summary>
            /// <param name="n"></param>
            /// <returns></returns>
            public static int Fractial(int n)
            {
                int result = 1;
                for (int i = 2; i <= n; ++i)
                {
                    result *= n;
                }
                return result;
            }
            /// <summary>
            /// C(n, k) = n! / (k! * (n-k)!)
            /// </summary>
            /// <param name="n"></param>
            /// <param name="k"></param>
            /// <returns></returns>
            public static int Combination(int n, int k)
            {
                if (k < 0 || k > n) return 0;
                if (k == 0 || k == n) return 1;

                // 优化计算：使用乘法公式避免大数阶乘
                int result = 1;
                for (int i = 1; i <= k; i++)
                    result *= (n - k + i);
                return result / Fractial(k);
            }
            /// <summary>
            /// 计算排列数 A(n, k) = n! / (n - k)!
            /// </summary>
            /// <param name="n"></param>
            /// <param name="k"></param>
            /// <returns></returns>
            public static int Arrangement(int n, int k)
            {
                if (k < 0 || k > n) return 0;

                int result = 1;
                for (int i = 0; i < k; i++)
                    result *= n - i;
                return result;
            }
        }
    }
}
