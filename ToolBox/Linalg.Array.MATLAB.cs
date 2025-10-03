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
        /// MATLAB的部分接口
        /// </summary>
        public static class MATLAB
        {
            /// <summary>
            /// 多项式求值，系数顺序为：c0 + c1 x^1 + c2 x^2 + ...
            /// </summary>
            /// <param name="coff"></param>
            /// <param name="xs"></param>
            /// <returns></returns>
            public static double[] PolyVal(double[] coff, double[] xs)
            {
                var degrees = Enumerable.Range(0, coff.Length);

                return xs.Select(x => coff.Zip(degrees, (ci, di) => ci * Math.Pow(x, di)).Sum()).ToArray();
            }
            /// <summary>
            /// 多项式拟合，系数顺序为：c0 + c1 x^1 + c2 x^2 + ...
            /// </summary>
            /// <param name="xs"></param>
            /// <param name="ys"></param>
            /// <param name="degrees"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static double[] PolyFit(double[] xs, double[] ys/*l*/, int degrees)
            {
                int len = degrees + 1;
                if (xs.Length != ys.Length || xs.Length < degrees)
                {
                    throw new ArgumentException("Data not enough for polyfit");
                }

                var A = new double[len, len]; // : A' = A.T * A
                                              // x1^0 x1^1 ... x1^deg
                                              // x2^0 x2^1 ... x2^deg  <==== A = 范德蒙(xs)
                                              // ...
                                              // xr^0 xr^1 ... xr^deg
                                              // |
                                              // V
                                              // sum(xi^0) sum(xi^1) ... sum(xi^deg)
                                              // sum(xi^1) sum(xi^2) ... sum(xi^deg+r)    <====== A'
                                              // ...
                                              // sum(xi^r) sum(xi^r+1) ... sum(xi^deg+c)
                var b = new double[len]; // : A.T * l <== l = ys
                                         // sum(x1^0) * y1
                                         // ...                <===== b
                                         // sum(xi^r) * yi
                // 构造系数与截距矩阵
                for (int r = 0; r != len; ++r)
                {
                    for (int c = 0; c != len; ++c)
                    {
                        A[r, c] = xs.Select(x => Math.Pow(x, r + c)).Sum();
                    }

                    b[r] = xs.Select(x => Math.Pow(x, r)).Zip(ys, (xi, yi) => xi * yi).Sum();
                }

                return SolveLinearSystem(A, b);
            }
            /// <summary>
            /// 解线性方程组
            /// </summary>
            /// <param name="A"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            private static double[] SolveLinearSystem(double[,] A, double[] b)
            {
                int n = b.Length;
                var aug = new double[n, n + 1];//增广矩阵[A|b]
                for (int r = 0; r != n; ++r)
                {
                    for (int c = 0; c != n; ++c)
                    {
                        aug[r, c] = A[r, c];
                    }
                    aug[r, n] = b[r];
                }

                for (int i = 0; i != n; ++i)
                {
                    int maxRow = i;
                    // 搜索主元
                    for (int r = i + 1; r != n; ++r)
                    {
                        if (Math.Abs(aug[r, i]) > Math.Abs(aug[maxRow, i]))
                        {
                            maxRow = r;
                        }
                    }
                    // 交换主元
                    for (int c = i; c != n + 1; ++c)
                    {
                        (aug[i, c], aug[maxRow, c]) = (aug[maxRow, c], aug[i, c]);
                    }
                    // 构建上三角矩阵，
                    for (int r = i + 1; r != n; ++r)
                    {//          ^^^^^
                        double factor = aug[r, i] / aug[i, i];
                        for (int c = i; c != n + 1; ++c)
                        {
                            if (c == i)
                            {
                                aug[r, c] = 0; // 理论值
                            }
                            else
                            {
                                aug[r, c] -= factor * aug[i, c];
                            }
                        }
                    }
                }

                var x = new double[n];
                // A已经是上三角矩阵，从下自上求解，将上个方程解代入下一个
                for (int r = n - 1; r != -1; --r)
                {//          ^^^^^
                    x[r] = aug[r, n] / aug[r, r];
                    for (int i = r - 1; i != -1; --i)
                    {
                        aug[i, n] -= aug[i, r] * x[r];
                        // b[i]       A[i, r]
                    }
                }

                return x;
            }
            public static class Ref
            {
                public static double[] PolyVal(double[] coefficients, double[] x)
                {
                    int n = x.Length;
                    int degree = coefficients.Length - 1;
                    double[] result = new double[n];

                    for (int i = 0; i < n; i++)
                    {
                        double xi = x[i];
                        double value = 0;

                        // 计算多项式值: value = c0 + c1*xi + c2*xi^2 + ... + cn*xi^n
                        for (int j = 0; j <= degree; j++)
                        {
                            value += coefficients[j] * Math.Pow(xi, j);
                        }

                        result[i] = value;
                    }

                    return result;
                }
                public static double[] PolyFit(double[] xs, double[] ys, int degree)
                {
                    int n = xs.Length;
                    if (n <= degree)
                    {
                        throw new ArgumentException("Data Num can't eval polyfit");
                    }

                    var A = new double[degree + 1, degree + 1];
                    var b = new double[degree + 1];

                    for (int r = 0; r <= degree; ++r)
                    {
                        for (int c = 0; c <= degree; ++c)
                        {
                            A[r, c] = xs.Select(x => Math.Pow(x, r + c)).Sum();
                        }
                        b[r] = xs.Select(x => Math.Pow(x, r)).Zip(ys, (xi, yi) => xi * yi).Sum();
                    }

                    return SolveLinearSystem(A, b);
                }

                public static double[] SolveLinearSystem(double[,] A, double[] b)
                {
                    int n = b.Length;
                    double[,] augmented = new double[n, n + 1];

                    // 构建增广矩阵 [A|b]
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            augmented[i, j] = A[i, j];
                        }
                        augmented[i, n] = b[i];
                    }

                    // 前向消元
                    for (int i = 0; i < n; i++)
                    {
                        // 寻找主元(r,r)
                        // main Value has same Row and Column
                        int maxRow = i;
                        for (int k = i + 1; k < n; k++) // i->r
                        {
                            if (Math.Abs(augmented[k, i]) > Math.Abs(augmented[maxRow, i]))
                            {
                                maxRow = k;
                            }
                        }

                        // 交换行
                        // 交换行
                        // r <-> maxRow
                        // begin from <c=r>
                        // 仅保留上三角部分，下三角将归零
                        for (int j = i; j <= n; j++)// i->r; j->c
                        {
                            double temp = augmented[i, j];
                            augmented[i, j] = augmented[maxRow, j];
                            augmented[maxRow, j] = temp;
                        }

                        // 消元
                        // r 已经是MaxRow
                        // 处理r之后的每一行
                        for (int k = i + 1; k < n; k++)
                        {
                            double factor = augmented[k, i] / augmented[i, i];
                            for (int j = i; j <= n; j++)
                            {
                                if (i == j)
                                {
                                    augmented[k, j] = 0;
                                }
                                else
                                {
                                    augmented[k, j] -= factor * augmented[i, j];
                                }
                            }
                        }
                    }

                    // 回代求解
                    // 此时为上三角阵
                    // 自下而上求解，将每个求解的未知量代入上一个方程
                    double[] x = new double[n];
                    for (int i = n - 1; i >= 0; i--)// i->r 
                    {
                        x[i] = augmented[i, n] / augmented[i, i];
                        for (int k = i - 1; k >= 0; k--)
                        {
                            augmented[k, n] -= augmented[k, i] * x[i];
                        }
                    }

                    return x;
                }
            }
        }
    }
}
