using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;


namespace MathLib
{

    static public class MatExtension
    {
        
        /// <summary>
        /// Plus
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 + m2</returns>
        public static double[] Plus(this double[] m1, double[] m2)
        {
            if (m1.Length != m2.Length) throw new InvalidOperationException("Input array size must be same or a scalar");
            double[] x = new double[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] + m2[i];

            return x;
        }

        /// <summary>
        /// Minus
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 - m2</returns>
        public static double[] Minus(this double[] m1, double[] m2)
        {
            if (m1.Length != m2.Length) throw new InvalidOperationException("Input array size must be same or a scalar");
            double[] x = new double[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] - m2[i];

            return x;
        }

        /// <summary>
        /// Plus
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 + m2</returns>
        public static double[] Plus(this double[] m1, double m2)
        {
            double[] x = new double[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] + m2;

            return x;
        }

        /// <summary>
        /// Plus
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 + m2</returns>
        public static double[] Plus(this int[] m1, double m2)
        {
            double[] x = new double[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] + m2;

            return x;
        }

        /// <summary>
        /// Plus
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 + m2</returns>
        public static int[] Plus(this int[] m1, int[] m2)
        {
            if (m1.Length != m2.Length) throw new InvalidOperationException("Input array size must be same or a scalar");
            int[] x = new int[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] + m2[i];

            return x;
        }

        /// <summary>
        /// Plus
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 + m2</returns>
        public static int[] Plus(this int[] m1, int m2)
        {
            int[] x = new int[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] + m2;

            return x;
        }

        /// <summary>
        /// Times, elemental multiplication
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 * m2</returns>
        public static int[] Times(this int[] m1, int[] m2)
        {
            if (m1.Length != m2.Length) throw new InvalidOperationException("Input array size must be same or a scalar");
            int[] x = new int[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] * m2[i];

            return x;
        }

        /// <summary>
        /// Times, elemental multiplication
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 * m2</returns>
        public static int[] Times(this int[] m1, int m2)
        {
            int[] x = new int[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] * m2;

            return x;
        }

        /// <summary>
        /// Times, elemental multiplication
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 * m2</returns>
        public static double[] Times(this double[] m1, double[] m2)
        {
            if (m1.Length != m2.Length) throw new InvalidOperationException("Input array size must be same or a scalar");
            double[] x = new double[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] * m2[i];

            return x;
        }

        /// <summary>
        /// Times, elemental multiplication
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>m1 * m2</returns>
        public static double[] Times(this double[] m1, double m2)
        {
            double[] x = new double[m1.Length];
            for (int i = 0; i < m1.Length; i++)
                x[i] = m1[i] * m2;

            return x;
        }

        /// <summary>
        /// Sort array x
        /// </summary>
        /// <param name="x">Input array to be sorted</param>
        /// <returns>sort index</returns>         
        public static int[] Sort(this double[] x)
        {
            int[] sortOrder = x.SortIndex();
            x = x.Select((y, i) => x[sortOrder[i]]).ToArray();
            return sortOrder;
        }

        /// <summary>
        /// Sort array x
        /// </summary>
        /// <param name="x">Input array to be sorted</param>
        /// <returns>sort index</returns>
        public static int[] Sort(this int[] x)
        {
            int[] sortOrder = x.SortIndex();
            x = (int[])x.Select((y, i) => x[sortOrder[i]]);
            return sortOrder;
        }

        /// <summary>
        /// Get index of sort order by using bubble sort
        /// </summary>
        /// <param name="vals">The input. (not sorted)</param>
        /// <returns>index of sort order</returns>
        public static int[] SortIndex(this double[] x)
        {
            int[] permIdx = new int[x.Length];
            for (int i = 0; i < permIdx.Length; i++) permIdx[i] = i;

            double temp = 0;
            int tempIdx = 0;
            bool doMore = true;
            while (doMore)
            {
                doMore = false;  // assume this is last pass over array
                for (int i = 0; i < x.Length - 1; i++)
                {
                    if (x[i] > x[i + 1])
                    {
                        // exchange elements
                        temp = x[i]; tempIdx = permIdx[i];
                        x[i] = x[i + 1]; permIdx[i] = permIdx[i + 1];
                        x[i + 1] = temp; permIdx[i + 1] = tempIdx;
                        doMore = true;  // after an exchange, must look again
                    }
                }
            }
            return permIdx;
        }

        [Flags]
        public enum SortOption { Ascending, Descending };

        /// <summary>
        /// Get index of sort ascending order by using bubble sort
        /// </summary>
        /// <param name="vals">The input. (not sorted)</param>
        /// <returns>index of sort order</returns>
        public static int[] SortIndex(this int[] x)
        {
            return SortIndex(x, SortOption.Ascending);
        }

        /// <summary>
        /// Get index of sort order by using bubble sort
        /// </summary>
        /// <param name="vals">The input. (not sorted)</param>
        /// <returns>index of sort order</returns>
        public static int[] SortIndex(this int[] x, SortOption option)
        {
            int[] permIdx = new int[x.Length];
            for (int i = 0; i < permIdx.Length; i++) permIdx[i] = i;

            bool isAscending = (option & SortOption.Ascending) == SortOption.Ascending;

            int temp = 0;
            int tempIdx = 0;
            bool doMore = true;
            while (doMore)
            {
                doMore = false;  // assume this is last pass over array
                for (int i = 0; i < x.Length - 1; i++)
                {
                    if (isAscending && x[i] > x[i + 1] ||
                        !isAscending && x[i] < x[i + 1])
                    {
                        // exchange elements
                        temp = x[i]; tempIdx = permIdx[i];
                        x[i] = x[i + 1]; permIdx[i] = permIdx[i + 1];
                        x[i + 1] = temp; permIdx[i + 1] = tempIdx;
                        doMore = true;  // after an exchange, must look again
                    }
                }
            }
            return permIdx;
        }


        /// <summary>
        /// Get index of sort order by using bubble sort
        /// </summary>
        /// <param name="x">The input is not sorted however</param>
        /// <returns>index of sort order</returns>
        public static int[] SortIndex(this List<double> x)
        {
            int[] permIdx = new int[x.Count];
            for (int i = 0; i < permIdx.Length; i++) permIdx[i] = i;

            double temp = 0;
            int tempIdx = 0;
            bool doMore = true;
            while (doMore)
            {
                doMore = false;  // assume this is last pass over array
                for (int i = 0; i < x.Count - 1; i++)
                {
                    if (x[i] > x[i + 1])
                    {
                        // exchange elements
                        temp = x[i]; tempIdx = permIdx[i];
                        x[i] = x[i + 1]; permIdx[i] = permIdx[i + 1];
                        x[i + 1] = temp; permIdx[i + 1] = tempIdx;
                        doMore = true;  // after an exchange, must look again
                    }
                }
            }
            return permIdx;
        }

        /// <summary>
        /// Get index of sort order by using bubble sort
        /// </summary>
        /// <param name="x">The input is not sorted however</param>
        /// <returns>index of sort order</returns>
        public static int[] SortIndex(this List<int> x)
        {
            int[] permIdx = new int[x.Count];
            for (int i = 0; i < permIdx.Length; i++) permIdx[i] = i;

            int temp = 0;
            int tempIdx = 0;
            bool doMore = true;
            while (doMore)
            {
                doMore = false;  // assume this is last pass over array
                for (int i = 0; i < x.Count - 1; i++)
                {
                    if (x[i] > x[i + 1])
                    {
                        // exchange elements
                        temp = x[i]; tempIdx = permIdx[i];
                        x[i] = x[i + 1]; permIdx[i] = permIdx[i + 1];
                        x[i + 1] = temp; permIdx[i + 1] = tempIdx;
                        doMore = true;  // after an exchange, must look again
                    }
                }
            }
            return permIdx;
        }

    }

    public static class General
    {
        /// <summary>
        /// Spacing of floating point numbers.
        /// </summary>
        public const double Eps = 2.220446049250313e-016;


        /// <summary>
        /// Cummulative sum
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int[] CumSum(int[] x)
        {
            for (int i = 0; i < x.Length - 1; ++i)
                x[i + 1] += x[i];
            return x;
        }


        /// <summary>
        /// Difference and approximate derivative
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double[] Diff(double[] x)
        {
            if (x == null || x.Length < 2) return new double[0];
            double[] y = new double[x.Length - 1];
            for (int i = 1; i < x.Length; i++)
            {
                y[i - 1] = x[i] - x[i - 1];
            }
            return y;
        }

        /// <summary>
        /// Absolute value
        /// </summary>
        /// <param name="x">ABS(X) is the absolute value of the elements of X.</param>
        /// <returns></returns>
        public static double[] Abs(double[] x)
        {
            return (double[])x.Select(y => Math.Abs(y)).ToArray();
        }


        /// <summary>
        /// Apply filter to W with length of <code>window</code>
        /// </summary>
        /// <param name="W"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static int[,] Filter(bool[,] W, int window)
        {
            int[,] filter = new int[W.GetLength(0), W.GetLength(1)];
            for (int i = 0; i < W.GetLength(0); ++i)
            {
                for (int j = 0; j < W.GetLength(1); ++j)
                {
                    if (W[i, j])
                    {
                        for (int k = 0; k < window; ++k)
                        {
                            int idx = i + k;
                            if (idx < filter.GetLength(0))
                                filter[idx, j] += 1;
                            else
                                break;

                        }
                    }
                }
            }
            return filter;
        }




        /// <summary>
        /// /Toeplitz matrix, a non-symmetric Toeplitz matrix having C as its
        ///   first column and R as its first row.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static double[,] Toeplitz(double[] c, double[] r)
        {

            if (r == null)
            {
                // set up for Hermitian Toeplitz
                // c[0] = conj[c[0]];
                r = c;
                // c[0] = conj[c[0]];
            }
            int p = r.Length;
            int m = c.Length;
            double[] x = new double[p - 1 + m];
            for (int i = p; i >= 2; --i)
                x[i] = r[i];
            for (int i = 0; i < c.Length; ++i)
                x[p + i] = c[i];
            int[] cidx = new int[m];
            for (int i = 0; i < m; ++i)
                cidx[i] = i;
            int[] ridx = new int[p];
            for (int i = p; i > 0; --i)
                ridx[i] = i;

            double[,] y = new double[m, p];
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < p; ++i)
                    y[i, j] = x[cidx[i] + ridx[j] * m];

            return y;
        }

        /// <summary>
        /// Toeplitz matrix, a non-symmetric Toeplitz matrix having C as its
        ///   first column and R as its first row.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static int[,] Toeplitz(int[] c, int[] r)
        {

            if (r == null)
            {
                // set up for Hermitian Toeplitz
                // c[0] = conj[c[0]];
                r = c;
                // c[0] = conj[c[0]];
            }
            int p = r.Length;
            int m = c.Length;
            int[] x = new int[p - 1 + m];
            int k = 0;
            for (int i = p - 1; i >= 1; --i)
                x[k++] = r[i];
            for (int i = 0; i < c.Length; ++i)
                x[p - 1 + i] = c[i];
            int[] cidx = new int[m];
            for (int i = 0; i < m; ++i)
                cidx[i] = i;
            int[] ridx = new int[p];
            k = 0;
            for (int i = p; i > 0; --i)
                ridx[k++] = i;

            //for (int i = 0; i < x.Length; ++i)
            //    Console.Write("{0} ", x[i]);
            //Console.WriteLine(" ");

            int[,] y = new int[m, p];
            for (int i = 0; i < m; ++i)
            {
                for (int j = 0; j < p; ++j)
                {
                    y[i, j] = x[cidx[i] + ridx[j] - 1];
                    //Console.Write("{0}, ", y[i, j]);
                }
                //Console.WriteLine(" ");
            }

            return y;
        }

        /// <summary>
        /// /Toeplitz matrix, a non-symmetric Toeplitz matrix having C as its
        ///   first column and R as its first row.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static byte[,] Toeplitz(byte[] c, byte[] r)
        {

            if (r == null)
            {
                // set up for Hermitian Toeplitz
                // c[0] = conj[c[0]];
                r = c;
                // c[0] = conj[c[0]];
            }
            int p = r.Length;
            int m = c.Length;
            byte[] x = new byte[p - 1 + m];
            for (int i = p; i >= 2; --i)
                x[i] = r[i];
            for (int i = 0; i < c.Length; ++i)
                x[p + i] = c[i];
            int[] cidx = new int[m];
            for (int i = 0; i < m; ++i)
                cidx[i] = i;
            int[] ridx = new int[p];
            for (int i = p; i > 0; --i)
                ridx[i] = i;

            byte[,] y = new byte[m, p];
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < p; ++i)
                    y[i, j] = x[cidx[i] + ridx[j] * m];

            return y;
        }
    }
}
