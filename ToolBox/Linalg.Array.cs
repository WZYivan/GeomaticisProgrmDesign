using System.Text;

namespace ToolBox
{
    namespace Linalg.Array
    {
        /// <summary>
        /// 用于一维数组的线性代数扩展方法
        /// </summary>
        public static class Vector
        {
            public static double VecMul(this double[] v1, double[] v2) => v1.Zip(v2, (x1, x2) => x1 * x2).Sum();
            public static double Mean(this double[] v) => v.Average();
            public static double Std(this double[] vec) => Math.Sqrt(vec.Select(v => Math.Pow(v - vec.Mean(), 2)).Sum() / vec.Length);
            public static double[,] ToMatrix(this double[] vec, int row, int col)
            {
                var mat = new double[row, col];
                for (int r = 0; r != row; ++r)
                {
                    for (int c = 0; c != col; ++c)
                    {
                        try
                        {
                            double val = vec[r * col + c];
                            mat[r, c] = val;
                        }
                        catch
                        {
                            mat[r, c] = 0;
                        }
                    }
                }
                return mat;
            }
            public static string ToString(this double[] vec, char sep)
            {
                var sb = new StringBuilder();
                sb.Append(vec[0].ToString());
                foreach (var v in vec.Skip(1)) { sb.Append($"{sep} " + v.ToString()); }
                return sb.ToString();
            }
            public static double[] Standardize(this double[] vec)
            {
                double
                    mean = vec.Average(),
                    std = Math.Sqrt(vec.Select(v => Math.Pow(v - mean, 2)).Sum() / vec.Length);
                return vec.Select(v => (v - mean) / std).ToArray();
            }
        }
        /// <summary>
        /// 用于二位数组的线性代数扩展方法
        /// </summary>
        public static class Matrix
        {
            public static int RowLength(this double[,] matrix) => matrix.GetLength(0);
            public static int ColLength(this double[,] matrix) => matrix.GetLength(1);
            public static double[,] Eye(int x)
            {
                var I = new double[x, x];
                for (int i = 0; i != x; ++i)
                {
                    I[i, i] = 1;
                }
                return I;
            }
            public static double[] Row(this double[,] matrix, int r)
            {
                var cLen = matrix.ColLength();
                var row = new double[cLen];
                for (var c = 0; c != cLen; c++)
                {
                    row[c] = matrix[r, c];
                }
                return row;
            }
            public static double[] Col(this double[,] matrix, int c)
            {
                var rLen = matrix.RowLength();
                var col = new double[rLen];
                for (var r = 0; r != rLen; r++)
                {
                    col[r] = matrix[r, c];
                }
                return col;
            }
            public static double[,] MatMul(this double[,] m1, double[,] m2)
            {
                if (m1.ColLength() != m2.RowLength())
                {
                    throw new ArgumentException("Matrix Not Match");
                }

                var mat = new double[m1.RowLength(), m2.ColLength()];
                for (int r = 0; r != m1.RowLength(); ++r)
                {
                    for (int c = 0; c != m2.ColLength(); c++)
                    {
                        mat[r, c] = m1.Row(r).VecMul(m2.Col(c));
                    }
                }
                return mat;
            }
            public static double[,] MatAdd(this double[,] m1, double[,] m2, double scale = 1)
            {
                if (m1.ColLength() != m2.ColLength() || m1.RowLength() != m2.RowLength())
                {
                    throw new ArgumentException("Matrix Not Match");
                }

                var mat = new double[m1.RowLength(), m2.ColLength()];
                for (int r = 0; r != m1.RowLength(); ++r)
                {
                    for (int c = 0; c != m2.ColLength(); c++)
                    {
                        mat[r, c] = m1[r, c] + m2[r, c] * scale;
                    }
                }
                return mat;
            }
            public static double[,] T(this double[,] matrix)
            {
                var mat = new double[matrix.ColLength(), matrix.RowLength()];
                for (int r = 0; r != matrix.RowLength(); ++r)
                {
                    for (int c = 0; c != matrix.ColLength(); c++)
                    {
                        mat[c, r] = matrix[r, c];
                    }
                }
                return mat;
            }
            public static double[,] Apply(this double[,] matrix, Func<double, double> func)
            {
                var mat = new double[matrix.RowLength(), matrix.ColLength()];
                for (int r = 0; r != matrix.RowLength(); ++r)
                {
                    for (int c = 0; c != matrix.ColLength(); c++)
                    {
                        mat[r, c] = func(matrix[r, c]);
                    }
                }
                return mat;
            }
            public static string ToString(this double[,] matrix, char sep)
            {
                var sb = new StringBuilder();
                for (int r = 0; r != matrix.RowLength(); r++)
                {
                    sb.AppendLine(matrix.Row(r).ToString(sep));
                }
                return sb.ToString();
            }
            public static double[,] StandardizeByCol(this double[,] mat, int[]? exculde = null)
            {
                var res = new double[mat.RowLength(), mat.ColLength()];
                exculde = exculde ?? new int[] { };
                for (int c = 0; c != mat.ColLength(); ++c)
                {
                    if (exculde.Contains(c))
                    {
                        continue;
                    }
                    var vec = new double[mat.RowLength()];
                    for (int r = 0; r != mat.RowLength(); r++)
                    {
                        vec[r] = mat[r, c];
                    }
                    vec = vec.Standardize();
                    for (int r = 0; r != mat.RowLength(); r++)
                    {
                        res[r, c] = vec[r];
                    }
                }
                return res;
            }
            public static double Det(this double[,] mat)
            {
                int r = mat.GetLength(0), c = mat.GetLength(1);
                if (r != c)
                {
                    throw new ArgumentException("Matrix must be square.");
                }

                if (r == 1) return mat[0, 0];
                if (r == 2) return mat[0, 0] * mat[1, 1] - mat[1, 0] * mat[0, 1];

                double det = 0;
                for (int col = 0; col < r; col++)
                {
                    // 生成子矩阵（去掉第0行和第col列）
                    double[,] submatrix = GetSubmatrix(mat, 0, col);

                    // 递归计算子矩阵的行列式
                    double subDet = submatrix.Det();

                    // 符号因子：(-1)^(0 + col)
                    double sign = (col % 2 == 0) ? 1 : -1;

                    // 累加展开式
                    det += mat[0, col] * sign * subDet;
                }

                return det;
            }
            private static double[,] GetSubmatrix(double[,] mat, int removeRow, int removeCol)
            {
                int rows = mat.GetLength(0);
                int cols = mat.GetLength(1);
                double[,] result = new double[rows - 1, cols - 1];

                int targetRow = 0;
                for (int sourceRow = 0; sourceRow < rows; sourceRow++)
                {
                    if (sourceRow == removeRow) continue;

                    int targetCol = 0;
                    for (int sourceCol = 0; sourceCol < cols; sourceCol++)
                    {
                        if (sourceCol == removeCol) continue;

                        result[targetRow, targetCol] = mat[sourceRow, sourceCol];
                        targetCol++;
                    }

                    targetRow++;
                }

                return result;
            }
            public static int Rank(this double[,] _mat, double tolerance = 1e-8)
            {
                var matrix = (double[,])_mat.Clone();
                int rank = 0;

                if (matrix.ColLength() == 0 || matrix.RowLength() == 0)
                {
                    return rank;
                }

                for (int i = 0; i != matrix.ColLength(); ++i)
                {
                    int maxRow = Helper.FindMaxRowOf(matrix, i, i, tolerance);
                    if (maxRow == -1) continue;

                    if (maxRow != i)
                    {
                        Helper.SwapRow(matrix, i, maxRow);
                    }

                    double pivot = matrix[i, i];
                    for (int c = 0; c < matrix.ColLength(); c++)
                    {
                        matrix[i, c] /= pivot;
                    }

                    // 步骤4: 消去下方元素
                    for (int r = 0; r < matrix.RowLength(); r++)
                    {
                        double factor = matrix[r, i];
                        if (Math.Abs(factor) < tolerance || r == i) continue;

                        for (int c = 0; c < matrix.ColLength(); c++)
                        {
                            if (c == i)
                            {
                                matrix[r, c] = 0;
                                continue;
                            }
                            matrix[r, c] -= factor * matrix[i, c];
                        }
                    }

                    rank++; // 增加秩计数

                    // 如果达到最大可能秩则提前终止
                    if (rank == Math.Min(matrix.RowLength(), matrix.ColLength()))
                        break;
                }

                return rank;
            }
            public static bool IsFullRank(this double[,] mat, double tolerance = 1e-8) => mat.Rank(tolerance) == Math.Min(mat.ColLength(), mat.RowLength());
            public static double[,] Inv(this double[,] matrix)
            {
                int rows = matrix.RowLength(), cols = matrix.ColLength();
                if (rows != cols || !matrix.IsFullRank())
                {
                    throw new ArgumentException("Row != Col OR not FullRank");
                }

                // 构造增广矩阵
                var aug = new double[rows, cols * 2];
                for (int r = 0; r != rows; r++)
                {
                    for (int c = 0; c != cols; c++)
                    {
                        aug[r, c] = matrix[r, c];
                    }
                    aug[r, r + cols] = 1;
                }

                for (int i = 0; i != cols; i++)// i ：当前正在处理的行，对主元，行列相等，其索引为[i,i]
                {
                    int maxRow = Helper.FindMaxRowOf(aug, i, i);//搜索主元，从当前处理行开始，不搜索上方
                    if (i != maxRow)
                    {
                        Helper.SwapRow(aug, i, maxRow);//行交换
                    }

                    double pivot = aug[i, i];
                    for (int c = 0; c != cols * 2; ++c)
                    {
                        aug[i, c] /= pivot;//归一化主元行
                    }
                    // 构建单位矩阵，每一次都要处理所有行
                    for (int r = 0; r != rows; ++r)
                    {//      ^^^^^
                        if (r == i)
                        {
                            continue;
                        }
                        double factor = aug[r, i];//主元行以归一化，即main=1，所以系数就是主元列元素
                        for (int c = 0; c != cols * 2; ++c)
                        {
                            if (c == i)
                            {
                                aug[r, c] = 0;
                            }
                            else
                            {
                                aug[r, c] -= factor * aug[i, c];//系数乘以主元行元素
                            }
                        }
                    }
                }

                var inv = new double[rows, cols];
                for (int r = 0; r != rows; r++)
                {
                    for (int c = 0; c != cols; c++)
                    {
                        inv[r, c] = aug[r, c + cols];
                    }
                }
                return inv;
            }
            private static class Helper
            {
                public static int FindMaxRowOf(double[,] mat, int col, int rowBeg = 0, double tolerence = 1e-8)
                {
                    int maxRow = rowBeg;
                    for (int r = rowBeg; r != mat.RowLength(); ++r)
                    {
                        if (Math.Abs(mat[r, col]) > Math.Abs(mat[maxRow, col]))
                        {
                            maxRow = r;
                        }
                    }

                    if (Math.Abs(mat[maxRow, col]) < tolerence)
                    {
                        return -1;
                    }
                    return maxRow;
                }
                public static void SwapRow(double[,] mat, int r1, int r2)
                {
                    for (int c = 0; c != mat.ColLength(); ++c)
                    {
                        (mat[r1, c], mat[r2, c]) = (mat[r2, c], mat[r1, c]);
                    }
                }
            }
        }
    }
}
