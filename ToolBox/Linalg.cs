using System.Collections;
using System.Globalization;


namespace ToolBox
{
    namespace Linalg
    {
        public class Vector : IEnumerable<double>
        {
            private readonly double[] _data;
            public int Length { get; }

            public Vector(int l)
            {
                Length = l;
                _data = new double[Length];
            }

            public Vector(double[] data) : this(data.Length)
            {
                System.Array.Copy(data, _data, Length);
            }

            public double this[int i]
            {
                get { return _data[i]; }
                set { _data[i] = value; }
            }

            public static implicit operator double[](Vector v) => v._data;
            public string ToString(char sep)
            {
                string s = "";
                foreach (var v in _data)
                {
                    s += $"{v}" + sep;
                }
                s = s.Remove(s.LastIndexOf(sep));
                return s;
            }
            public override string ToString() => ToString(',');

            public IEnumerator<double> GetEnumerator() => ((IEnumerable<double>)_data).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public static double operator *(Vector v1, Vector v2) => v1.Zip(v2, (x, y) => x * y).Sum();
            //public static Vector operator *(Vector v, double s) => new Vector(v.Select(x => x * s).ToArray());
            //public static Vector operator *(double s, Vector v) => v * s;
        }
        public class Matrix : IEnumerable<double>
        {
            #region Data
            private readonly double[,] _data;
            public int Rows { get; }
            public int Cols { get; }
            #endregion

            #region Indexer
            public double this[int r, int c]
            {
                get
                {
                    if (r < 0 || r > Rows || c < 0 || c > Cols)
                    {
                        throw new IndexOutOfRangeException($"[{r},{c}] out of range [{Rows}, {Cols}]");
                    }
                    return _data[r, c];
                }

                set
                {
                    if (r < 0 || r > Rows || c < 0 || c > Cols)
                    {
                        throw new IndexOutOfRangeException($"[{r},{c}] out of range [{Rows}, {Cols}]");
                    }
                    _data[r, c] = value;
                }
            }
            public double this[int i]
            {
                get
                {
                    if (i < 0 || i > Rows * Cols)
                    {
                        throw new IndexOutOfRangeException($"[{i}] out of range [{Rows * Cols}]");
                    }

                    return _data[(i - i % Cols) / Cols, i % Cols];
                }
                set
                {
                    if (i < 0 || i > Rows * Cols)
                    {
                        throw new IndexOutOfRangeException($"[{i}] out of range [{Rows * Cols}]");
                    }

                    _data[(i - i % Cols) / Cols, i % Cols] = value;
                }
            }
            #endregion

            #region Assigner
            public class Assigner
            {
                private Matrix _m;
                private int _i = 0;

                public Assigner(Matrix m) => _m = m;

                public Assigner Assign(double v)
                {
                    _m[_i++] = v;
                    return this;
                }

                public static Assigner operator |(Assigner a, double v)
                {
                    return a.Assign(v);
                }
            }

            public static Assigner operator |(Matrix m, double v)
            {
                var ass = new Assigner(m);
                return ass.Assign(v);
            }

            public static void Block(Assigner a) { }
            public static void Block(RefBase.Assigner a) { }
            #endregion

            #region Ref
            public abstract class RefBase : IEnumerable<double>
            {
                protected readonly Matrix _m;
                protected readonly int _i;

                public RefBase(Matrix m, int i)
                {
                    _m = m;
                    _i = i;
                }

                public abstract int Length { get; }
                public abstract double this[int i] { get; set; }
                public abstract Vector ToVector();
                public Vector Vec { get => ToVector(); }
                public static implicit operator Vector(RefBase r) => r.Vec;

                public IEnumerator<double> GetEnumerator()
                {
                    for (int i = 0; i != Length; ++i)
                    {
                        yield return this[i];
                    }
                }
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

                public class Assigner
                {
                    int _i = 0;
                    RefBase _r;

                    public Assigner(RefBase r)
                    {
                        _r = r;
                    }

                    public Assigner Assign(double v)
                    {
                        _r[_i++] = v;
                        return this;
                    }
                    public static Assigner operator |(Assigner a, double v) => a.Assign(v);
                }

                public static Assigner operator |(RefBase r, double v)
                {
                    var a = new Assigner(r);
                    return a.Assign(v);
                }

                public double Mean() => this.Sum() / this.Count();
            }

            public class RowRef : RefBase
            {
                private readonly int r;
                public RowRef(Matrix m, int r) : base(m, 0) => this.r = r;
                public override int Length => _m.Cols;
                public override double this[int col]
                {
                    get => _m[r, col];
                    set => _m[r, col] = value;
                }
                public override Vector ToVector()
                {
                    var v = new Vector(this.ToArray());
                    return v;
                }
            }

            public class ColRef : RefBase
            {
                private readonly int c;
                public ColRef(Matrix m, int c) : base(m, 0) => this.c = c;
                public override int Length => _m.Rows;
                public override double this[int row]
                {
                    get => _m[row, c];
                    set => _m[row, c] = value;
                }
                public override Vector ToVector() => new Vector(this.ToArray());
            }

            public RowRef Row(int i) => new RowRef(this, i);
            public ColRef Col(int i) => new ColRef(this, i);
            #endregion

            #region Operator
            public static Matrix operator *(Matrix m1, Matrix m2)
            {
                if (m1.Cols != m2.Rows)
                    throw new ArgumentException("Matrix shape doesn't match");
                var m = new Matrix(m1.Rows, m2.Cols);
                for (int r = 0; r < m.Rows; ++r)
                {
                    for (int c = 0; c != m.Cols; ++c)
                    {
                        m[r, c] = m1.Row(r).Vec * m2.Col(c).Vec;
                    }
                }

                return m;
            }
            public static Matrix operator *(Matrix m, double v) => new Matrix(m.Select(x => x * v).ToArray(), m.Rows, m.Cols);
            public static Matrix operator *(double v, Matrix m) => m * v;
            public static Matrix operator +(Matrix m1, Matrix m2)
            {
                if ((m1.Rows != m2.Rows) || (m1.Cols != m2.Cols))
                {
                    throw new ArgumentException("Matrix shape doesn't match");
                }
                return new Matrix(m1.Zip(m2, (x, y) => x + y).ToArray(), m1.Rows, m1.Cols);
            }
            public static Matrix operator +(Matrix m, double v) => new Matrix(m.Select(x => x + v).ToArray(), m.Rows, m.Cols);
            public static Matrix operator +(double v, Matrix m) => m + v;
            public static Matrix operator -(Matrix m1, Matrix m2)
            {
                if ((m1.Rows != m2.Rows) || (m1.Cols != m2.Cols))
                {
                    throw new ArgumentException("Matrix shape doesn't match");
                }
                return new Matrix(m1.Zip(m2, (x, y) => x - y).ToArray(), m1.Rows, m1.Cols);
            }
            public static Matrix operator -(Matrix m, double v) => new Matrix(m.Select(x => x - v).ToArray(), m.Rows, m.Cols);
            public static Matrix operator -(double v, Matrix m) => m - v;
            public static Matrix operator /(Matrix m, double v) => new Matrix(m.Select(x => x / v).ToArray(), m.Rows, m.Cols);
            public static Matrix operator /(double v, Matrix m) => m / v;

            #endregion

            #region Init
            public Matrix(double[,] values)
            {
                Rows = values.GetLength(0);
                Cols = values.GetLength(1);
                this._data = (double[,])values.Clone();
            }
            public Matrix(double[] values, int rows, int cols) : this(rows, cols)
            {
                var ma = new Assigner(this);
                foreach (var v in values)
                {
                    Block(ma | v);
                }
            }
            public Matrix(int rows, int cols) : this(new double[rows, cols]) { }
            #endregion

            #region IO
            public override string ToString()
            {
                string s = "";
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Cols; j++)
                    {
                        s += $"{_data[i, j]}, ";
                    }
                    s = s[..^2];
                    s += "\n";
                }
                return s;
            }

            private static string[] SplitRow(string s, IList<char> sep)
            {
                return s.Split(sep.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            public static Matrix FromStringList(IList<string> rows, IEnumerable<char>? additionalSep = null)
            {
                if (rows == null || rows.Count == 0)
                    throw new ArgumentNullException(nameof(rows));

                var sep = new List<char> { ' ', '\t', ';', ',' };
                if (additionalSep != null)
                {
                    sep.AddRange(additionalSep.Where(x => !sep.Contains(x)));
                }

                int rC = rows.Count,
                    cC = SplitRow(rows[0], sep).Length;
                var mat = new Matrix(rC, cC);

                for (int r = 0; r != rC; ++r)
                {
                    string[] values = SplitRow(rows[r], sep);
                    if (values.Length != cC)
                    {
                        throw new ArgumentException($"Row {r + 1} has {values.Length} elements, but expected {cC}");
                    }

                    for (int c = 0; c != cC; ++c)
                    {
                        if (!double.TryParse(values[c], NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                            throw new ArgumentException($"Invalid number format at row {r + 1}, column {c + 1}: '{values[c]}'");

                        mat[r, c] = value;
                    }
                }

                return mat;
            }
            #endregion

            #region Linear
            public static Matrix Eye(int size)
            {
                var m = new Matrix(size, size);
                for (int i = 0; i < size; i++)
                    m[i, i] = 1;
                return m;
            }
            public Matrix T
            {
                get
                {
                    var result = new Matrix(Cols, Rows);
                    for (int i = 0; i < Rows; i++)
                        for (int j = 0; j < Cols; j++)
                            result[j, i] = _data[i, j];
                    return result;
                }
            }
            public double Det()
            {
                {
                    if (Rows != Cols)
                        throw new InvalidOperationException("Matrix must be square");

                    int n = Rows;
                    if (n == 1) return _data[0, 0];
                    if (n == 2) return _data[0, 0] * _data[1, 1] - _data[0, 1] * _data[1, 0];

                    double det = 0;
                    for (int j = 0; j < n; j++)
                    {
                        det += (j % 2 == 0 ? 1 : -1) * _data[0, j] * Sub(0, j).Det();
                    }
                    return det;
                }
            }

            public Matrix InvSym()
            {
                var a = this.ToArray();
                int n = Rows;
                double[] b = new double[n];

                for (int k = 0; k <= n - 1; k++)
                {
                    double w = a[0] + 1.0e-25;
                    int m = n - k - 1;

                    for (int i = 1; i < n; i++)
                    {
                        double g = a[i * n];
                        b[i] = g / w;
                        if (i <= m) b[i] = -b[i];

                        int tmpOffset1 = (i - 1) * n - 1;
                        int tmpOffset2 = i * n;

                        for (int j = 1; j <= i; j++)
                        {
                            a[tmpOffset1 + j] = a[tmpOffset2 + j] + g * b[j];
                        }
                    }

                    a[n * n - 1] = 1.0 / w;
                    for (int i = 1; i <= n - 1; i++)
                    {
                        a[(n - 1) * n + i - 1] = b[i];
                    }
                }

                for (int i = 0; i <= n - 2; i++)
                {
                    for (int j = i + 1; j <= n - 1; j++)
                    {
                        a[i * n + j] = a[j * n + i];
                    }
                }

                return new Matrix(a, Rows, Rows);
            }
            // 矩阵求逆（高斯-约旦消元法）
            public Matrix Inverse()
            {
                var matrix = new double[Rows, Cols];
                for (int r = 0; r != Rows; r++)
                {
                    for (int c = 0; c != Cols; c++)
                    {
                        matrix[r, c] = this[r, c];
                    }
                }


                int n = matrix.GetLength(0);
                double[,] augmented = new double[n, 2 * n];

                // 构造增广矩阵 [matrix | I]
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        augmented[i, j] = matrix[i, j];
                    }
                    augmented[i, i + n] = 1;
                }
                // 高斯-约旦消元
                for (int i = 0; i < n; i++)
                {
                    double diag = augmented[i, i];

                    // 归一化当前行
                    for (int j = 0; j < 2 * n; j++)
                    {
                        augmented[i, j] /= diag;
                    }
                    // 消去其他行
                    for (int k = 0; k < n; k++)
                    {
                        if (k != i)
                        {
                            double factor = augmented[k, i];
                            for (int j = 0; j < 2 * n; j++)
                            {
                                augmented[k, j] -= factor * augmented[i, j];
                            }
                        }
                    }
                }
                // 提取逆矩阵
                double[,] inverse = new double[n, n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        inverse[i, j] = augmented[i, j + n];
                    }
                }
                return new Matrix(inverse);
            }

            #region helper
            private Matrix Sub(int exR, int exC)
            {
                var sub = new Matrix(Rows - 1, Cols - 1);
                int r = 0;
                for (int i = 0; i < Rows; i++)
                {
                    if (i == exR) continue;
                    int c = 0;
                    for (int j = 0; j < Cols; j++)
                    {
                        if (j == exC) continue;
                        sub[r, c] = _data[i, j];
                        c++;
                    }
                    r++;
                }
                return sub;
            }
            //private void 
            #endregion
            #endregion

            #region IEnumerable
            public IEnumerator<double> GetEnumerator()
            {
                for (int r = 0; r != Rows; ++r)
                {
                    for (int c = 0; c != Cols; ++c)
                    {
                        yield return this[r, c];
                    }
                }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            #endregion

            #region Helper
            public Matrix Apply(Func<double, double> f) => new (this.Select(x => f(x)).ToArray(), Rows, Cols);

            public Matrix SubMatrix(int rFrom, int rTo, int cFrom, int cTo)
            {
                var m = new Matrix(rTo - rFrom, cTo - cFrom);
                var ma = new Assigner(m);
                for (int r = rFrom; r != rTo; ++r)
                {
                    for (int c = cFrom; c != cTo; ++c)
                    {
                        Block(ma | this[r, c]);
                    }
                }
                return m;
            }

            public static Matrix Random(int rows, int cols, double scale = 10) 
            {
                Matrix m = new(rows, cols);
                var ass = new Assigner(m);
                var rng = new System.Random();

                for(int s=rows*cols; s!=0; --s)
                {
                    Matrix.Block(ass | rng.NextDouble() * scale);
                }

                return m;
            }
            #endregion
        }
    }
}
