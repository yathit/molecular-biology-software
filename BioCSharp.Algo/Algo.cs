using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioCSharp.Seqs;
using System.ComponentModel;
using BioCSharp.Algo.Phylo;

namespace BioCSharp.Algo
{
    public enum DistMethod
    {
        [Description("p-distance")]
        PDistance,
        [Description("Jukes-Cantor")]
        JukesCantor,
        [Description("Alignment-score")]
        AlignmentScore,
        [Description("Tajima-Nei")]
        TajimaNei,
        [Description("Kimura")]
        Kimura,
        [Description("Tamura")]
        Tamura,
        [Description("Hasegawa")]
        Hasegawa,
        [Description("Tamura-Nei")]
        TamuraNei,
        [Description("Poisson")]
        Poisson,
        [Description("Gamma")]
        Gamma
    }

    public enum LinkageMethod
    {
        [Description("nearest distance (single linkage method)")]
        Single,
        [Description("furthest distance (complete linkage method)")]
        Complete,
        [Description("unweighted average distance (UPGMA)")]
        Average,
        [Description("weighted average distance (WPGMA)")]
        Weighted,
        [Description("unweighted center of mass distance (UPGMC)")]
        Centroid,
        [Description("weighted center of mass distance (WPGMC)")]
        Median,
        [Description("Ward's linkage")]
        Ward
    }

    public static class Algo
    {


        /// <summary>
        /// computes pairwise distance between sequences.
        /// 
        /// returns a vector D containing biological distances
        ///  between each pair of sequences stored in the M elements of the 
        ///  <code>ReqRecord</code>. D is an [M, M] matrix, corresponding to respective sequence.
        /// </summary>
        /// <param name="seqs">Aligned sequence</param>
        /// <returns>distance matrix</returns>
        public static double[,] Pdist(SeqRecord[] seqs, DistMethod distMethod)
        {
            int M = seqs.Length;
            if (M < 3)
                throw new InvalidOperationException("There must be at least 3 sequence.");

            // make sure aligned sequence
            for (int i = 1; i < seqs.Length; ++i)
                if (seqs[0].Length != seqs[i].Length)
                    throw new InvalidOperationException("Input sequence must be aligned");

            //SubstitutionMatrix subtMat;
            //if (seqs[0].Seq.IsNucleotide)
            //    subtMat = SubstitutionMatrix.Nuc44;
            //else if (seqs[0].Seq.Alphabet == Alphabet.Protein)
            //    subtMat = SubstitutionMatrix.Blosum62n;
            //else
            //    throw new ArgumentException("Alphabet must be nucleotide or protein.");

            double a = 2; // Gamma parameter

            double[,] D = new double[M, M];

            bool indelMethod = true; // score gaps
            // h will contain the locs that need to be scored in the pairwise alignment
            bool[] h = new bool[seqs[0].Length];

            for (int i = 0; i < M - 1; ++i)
            {
                for (int j = i + 1; j < M; ++j)
                {
                    int sumH = 0;
                    for (int k = 0; k < seqs[i].Length; k++)
                    {
                        if (indelMethod)
                        {
                            // only avoid sites with double gaps, pairwise deleted gaps
                            h[k] = seqs[i][k] != Seq.GapChar | seqs[j][k] != Seq.GapChar;
                        }
                        else
                        {
                            // avoid any site with gaps
                            h[k] = seqs[i][k] != Seq.GapChar & seqs[j][k] != Seq.GapChar;
                        }
                        sumH += h[k] ? 1 : 0;
                    }

                    switch (distMethod)
                    {
                        case DistMethod.PDistance:
                            for (int k = 0; k < seqs[i].Length; ++k)
                            {
                                D[i, j] += (seqs[i][k] != seqs[j][k] & h[k]) ? 1 : 0;
                            }
                            D[i, j] /= sumH;
                            break;
                        case DistMethod.AlignmentScore:
                            throw new NotImplementedException();
                        case DistMethod.Gamma:
                            // Tajima and Nei (1984) Molecular Biology and Evolution
                            // % Let p be the proportion of substitutions and 'a' the Gamma parameter:
                            // d = a * ( (1-p)^(-1/a) - 1);
                            //
                            // default of 'a' is 2 unless another value is given by the user by setting 'OPTARGS'
                            for (int k = 0; k < seqs[i].Length; ++k)
                            {
                                D[i, j] += (seqs[i][k] != seqs[j][k] & h[k]) ? 1 : 0;
                            }
                            D[i, j] /= sumH;
                            D[i, j] = a * (Math.Pow((1 - D[i, j]), (-1 / a)) - 1);
                            break;
                        case DistMethod.Hasegawa:
                            throw new NotImplementedException();
                        case DistMethod.JukesCantor:
                            // Jukes, T. H. and C. R. Cantor. (1969) Mammalian Protein Metabolism Acad. Press
                            for (int k = 0; k < seqs[i].Length; ++k)
                            {
                                D[i, j] += (seqs[i][k] != seqs[j][k] & h[k]) ? 1 : 0;
                            }
                            D[i, j] /= sumH;
                            if (seqs[0].Alphabet == Alphabet.Protein)
                            {
                                // max prevents negative log
                                D[i, j] = -19.0 / 20.0 * Math.Log(Math.Max(Double.Epsilon, 1 - 20.0 * D[i, j] / 19.0));
                            }
                            else
                            {
                                D[i, j] = -3.0 / 4.0 * Math.Log(Math.Max(Double.Epsilon, 1 - 4.0 * D[i, j] / 3.0));
                            }
                            break;
                        case DistMethod.Kimura:
                            throw new NotImplementedException();
                        case DistMethod.Poisson:
                            // Let p be the proportion of substitutions:
                            // d = -log(1-p);
                            for (int k = 0; k < seqs[i].Length; ++k)
                            {
                                D[i, j] += (seqs[i][k] != seqs[j][k] & h[k]) ? 1 : 0;
                            }
                            D[i, j] /= sumH;
                            D[i, j] = -Math.Log(1 - D[i, j]);
                            break;
                        case DistMethod.TajimaNei:
                            throw new NotImplementedException();
                        case DistMethod.Tamura:
                            throw new NotImplementedException();
                        case DistMethod.TamuraNei:
                            throw new NotImplementedException();
                        default:
                            throw new NotSupportedException("Unknown distance method");
                    }
                }
            }

            return D;
        }

        /// <summary>
        /// constructs a B matrix for phylogenetic tree from pairwise distances.
        /// </summary>
        /// <param name="seqs"></param>
        /// <param name="dist"></param>
        /// <param name="distMethod"></param>
        /// <returns></returns>
        public static Tree Linkage(SeqRecord[] seqs, LinkageMethod method, DistMethod distMethod)
        {
            double[,] D = Algo.Pdist(seqs, distMethod);
            int m = seqs.Length;
            //int k3 = 0;
            List<double> Y = new List<double>(m * (m - 1) / 2);
            for (int k1 = 0; k1 < m; ++k1)
                for (int k2 = k1 + 1; k2 < m; ++k2)
                    Y.Add(D[k1, k2]);

            int numLeaves = m;
            int numBranches = m - 1;
            int[,] B = new int[numBranches, 2];
            double[] dist = new double[numBranches];

            // during updating clusters, cluster index is constantly changing, R is
            // a index vector mapping the original index to the current (row, column)
            // index in Y.  N denotes how many points are contained in each cluster.
            List<int> N = new List<int>();
            for (int k1 = 1; k1 <= 2 * m - 1; ++k1)
            {
                if (k1 <= m)
                    N.Add(1);
                else
                    N.Add(0);
            }
            int n = m; // since m is changing, we need to save m in n.
            List<int> R = new List<int>(m);
            for (int i = 1; i <= m; ++i)
                R.Add(i);


            for (int s = 1; s <= n - 1; ++s)
            {
                #region get min value v at k in Y
                int k = 1;
                double v = Y[k - 1];

                if (method == LinkageMethod.Average)
                {
                    List<int> p = new List<int>(m/2);
                    for (int k1 = m - 1; k1 >= 2; --k1)
                    {
                        p.Add(k1);
                    }
                    int[] AI = new int[(int)(m * (m - 1) / 2.0)];
                    int cumSum = 0;
                    foreach (int ip in p)
                    {
                        AI[cumSum] = 1;
                        cumSum += ip;
                    }
                    AI[AI.Length - 1] = 1;
                    cumSum = 0;
                    for (int k1 = 1; k1 < AI.Length; ++k1)
                        AI[k1] += AI[k1 - 1]; // cumsum

                    int[] AJ = new int[AI.Length];
                    for (int k1 = 0; k1 < AJ.Length; ++k1)
                        AJ[k1] = 1;
                    cumSum = 0;
                    foreach (int ip in p)
                    {
                        cumSum += ip;
                        AJ[cumSum] = 2 - ip;
                    }
                    AJ[0] = 2;
                    for (int k1 = 1; k1 < AJ.Length; ++k1)
                    {
                        AJ[k1] += AJ[k1 - 1];
                    }

                    int[] W = new int[AJ.Length];
                    for (int k1 = 0; k1 < AJ.Length; ++k1)
                        W[k1] = N[R[AI[k1] - 1] - 1] * N[R[AJ[k1] - 1] - 1];

                    for (int k1 = 1; k1 <= Y.Count; ++k1)
                    {
                        if ((Y[k1 - 1] / W[k1 - 1]) < v)
                        {
                            k = k1;
                            v = Y[k1 - 1] / W[k1 - 1];
                        }
                    }
                }
                else
                {
                    for (int k1 = 1; k1 <= Y.Count; ++k1)
                    {
                        if (Y[k1 - 1] < v)
                        {
                            k = k1;
                            v = Y[k1 - 1];
                        }
                    }
                }
                #endregion

                int i = (int)Math.Floor(m + 0.5 - Math.Sqrt(m * m - m + 0.25 - 2.0 * (k - 1)));
                int j = (int) (k - (i - 1.0) * (m - i / 2.0) + i);

                // update one more row to the output matrix
                B[s - 1, 0] = R[i - 1];
                B[s - 1, 1] = R[j - 1];
                dist[s - 1] = v;

                // these are temp variables for indexing
                int x;
                int[] I1 = new int[i - 1];
                int[] I2 = new int[(j - 1) - i];
                int[] I3 = new int[m - j];
                int[] U = new int[I1.Length + I2.Length + I3.Length];
                int[] I = new int[U.Length];
                int[] J = new int[U.Length];

                x = 0; int y = 0;
                for (int k1 = 1; k1 <= i - 1; ++k1)
                {
                    I1[x] = k1;
                    U[x] = I1[x];
                    I[x] = (int) (I1[x] * (m - (I1[x] + 1) / 2.0) - m + i);
                    J[x] = (int) (I1[x] * (m - (I1[x] + 1) / 2.0) - m + j);
                    x++;
                }

                x = 0; y = I1.Length;
                for (int k1 = i + 1; k1 <= j - 1; ++k1)
                {
                    I2[x] = k1;
                    U[y + x] = I2[x];
                    I[y + x] = (int) (i * (m - (i + 1) / 2.0) - m + I2[x]);
                    J[y + x] = (int) (I2[x] * (m - (I2[x] + 1) / 2.0) - m + j);
                    x++;
                }

                x = 0; y = I1.Length + I2.Length;
                for (int k1 = (j + 1); k1 <= m; ++k1)
                {
                    I3[x] = k1;
                    U[y + x] = I3[x];
                    I[y + x] = (int) (i * (m - (i + 1) / 2.0) - m + I3[x]);
                    J[y + x] = (int) (j * (m - (j + 1) / 2.0) - m + I3[x]);
                    x++;
                }


                for (int k1 = 0; k1 < I.Length; ++k1)
                {
                    switch (method)
                    {
                        case LinkageMethod.Average:
                            Y[I[k1] - 1] = Y[I[k1] - 1] + Y[J[k1] - 1];
                            break;
                        case LinkageMethod.Centroid:
                            throw new NotImplementedException();
                        case LinkageMethod.Complete:
                            Y[I[k1] - 1] = Math.Max(Y[I[k1] - 1], Y[J[k1] - 1]);
                            break;
                        case LinkageMethod.Median:
                            //Y[I[k1]-1] = (Y[I[k1]-1] + Y[J[k1]-1]) / 2 - v / 4;
                            throw new NotImplementedException();
                        case LinkageMethod.Single:
                            Y[I[k1] - 1] = Math.Min(Y[I[k1] - 1], Y[J[k1] - 1]);
                            break;
                        case LinkageMethod.Weighted:
                            Y[I[k1] - 1] = Y[I[k1] - 1] + Y[J[k1] - 1] / 2;
                            break;
                        case LinkageMethod.Ward:
                            throw new NotImplementedException(); 
                        default:
                            throw new InvalidOperationException();
                    }
                }

                // no need for the cluster information about j.
                List<int> J2 = new List<int>(J);
                J2.Add((int)(i * (m - (i + 1) / 2.0) - m + j));
                J2.Sort();
                //Console.Write("Y: ");
                //foreach (double xx in Y) { Console.Write("{0}, ", xx); }
                //Console.Write("J: ");
                //foreach (int xx in J2) { Console.Write("{0}, ", xx); }
                //Console.WriteLine(" ");
                for (int k1 = J2.Count-1; k1 >= 0; --k1)
                    Y.RemoveAt(J2[k1] - 1);

                // update m, N, R
                m = m-1;
                N[n + s - 1] = N[R[i - 1] - 1] + N[R[j - 1] - 1];
                R[i - 1] = n + s;
                for (int k1 = j + 1; k1 <= n; ++k1)
                    R[k1 - 1 - 1] = R[k1 - 1];
            }

            // Do squaring for 'ce', 'me', 'wa' methods

            // sort 
            for (int k1 = 0; k1 < B.GetLength(0); ++k1)
            {
                if (B[k1, 0] > B[k1, 1])
                {
                    int tmp = B[k1, 0];
                    B[k1, 0] = B[k1, 1];
                    B[k1, 1] = tmp;
                }
            }

            for (int k1 = 0; k1 < B.GetLength(0); ++k1)
            {
                --B[k1, 0];
                --B[k1, 1];
            }
            string[] names = new string[seqs.Length];
            for (int i = 0; i < seqs.Length; ++i)
                names[i] = seqs[i].Name;
            return new Tree(B, names, dist);
        }

        

        
    }
}
