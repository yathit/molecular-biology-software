using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioCSharp.Seqs;
using MathLib;


namespace iMoBio.Primer
{
    

    public class PrimerPair
    {
        readonly private Primer primer1;
        readonly private Primer primer2;

        public PrimerPair(Primer primer1, Primer primer2)
        {
            this.primer1 = primer1;
            this.primer2 = primer2;
        }
    }

    public class PrimerRegion
    {
        readonly DnaSeq seq;
        int start;
        int end;
        int minLength = 15;
        int maxLength = 24;
        int targetLength = 20;
        static int maxLengthMax = 30;
        bool isForward;
        int minHpinBases = 3;
        int minHpinLoop = 2;
        int minDimerLength = 2;
        //int minDimerCount = 4;
        List<Primers> primers = new List<Primers>();

        /// <summary>
        /// Region of sequence in int format
        /// </summary>
        int[] seqNum;

        bool isInvalid = true;


        public PrimerRegion(Seq seq, int start, int end)
        {
            this.seq = seq.SubSeq(start, end);
            this.start = start;
            this.end = end;
            this.isForward = start <= end;
            seqNum = (int[])this.seq;

            int len = Math.Abs(start - end);
            if (minLength > len)
                minLength = len;
            if (maxLength > len)
                maxLength = len;
            if (targetLength > len)
                targetLength = len;
        }

        public Primers PrimersAt(int primerLength)
        {
            if (isInvalid) Validate();
            if (primerLength >= minLength & primerLength <= maxLength)
            {
                var query = from q in primers
                            where q.PrimerLength == primerLength
                            select q;
                return query.First<Primers>();
                            
            }
            else
            {
                throw new IndexOutOfRangeException("primer length: " + primerLength);
            }
        }

        

        #region properties
        public int Length { get { return seqNum.Length; } }
        public int MaxLength
        {
            set 
            {
                if (value > Length)
                    throw new ArgumentException("value must be less than sequence length");
                maxLength = value;
                if (minLength > maxLength)
                    minLength = maxLength;
                isInvalid = true; 
            }
            get { return maxLength; }
        }

        public int MinLength
        {
            set 
            {
                if (value > Length)
                    throw new ArgumentException("value must be less than sequence length");
                minLength = value;
                if (minLength > maxLength)
                    maxLength = minLength;
                isInvalid = true; 
            }
            get { return minLength; }
        }

        public int MinDimerLength
        {
            set
            {
                if (value < 2 | value > maxLengthMax)
                {
                    throw new ArgumentException("value must be larger than 2 and less than primer length");
                }
                minDimerLength = value;
            }
            get
            {
                return minDimerLength;
            }
        }

        public int MinHpinBases
        {
            set 
            { 
                minHpinBases = value;
                if (minHpinLoop > MinHpinBases)
                    minHpinLoop = MinHpinBases;
                isInvalid = true; 
            }
            get { return minHpinBases; }
        }

        public int MinHpinLoop
        {
            set 
            { 
                minHpinLoop = value;
                if (minHpinBases < minHpinLoop)
                    minHpinBases = minHpinLoop;
                isInvalid = true; 
            }
            get { return minHpinLoop; }
        }
        #endregion

        public void Invalidate() { isInvalid = true; }

        public void Validate()
        {
            int increasingLength = targetLength;
            int decreasingLength = targetLength;
            bool direction = true;  // direction of increasing, we go alternative
            do
            {
                if (direction) // go increase
                {
                    Primers p = new Primers(this, increasingLength);
                    p.Validate();
                    primers.Add(p);
                }
                else
                {
                    Primers p = new Primers(this, decreasingLength);
                    p.Validate();
                    primers.Add(p);
                }
                #region change primer length back and forth
                if (direction) // go increase
                {
                    if (increasingLength + 1 > maxLength)
                    {
                        if (decreasingLength - 1 < minLength)
                        {
                            break;
                        }
                        else
                        {
                            direction = !direction;
                            decreasingLength--;
                        }
                    }
                    increasingLength++;
                }
                else
                {
                    if (decreasingLength - 1 < minLength)
                    {
                        if (increasingLength + 1 > maxLength)
                        {
                            break;
                        }
                        else
                        {
                            direction = !direction;
                            increasingLength++;
                        }
                    }
                    increasingLength--;
                }
                #endregion
            } while (true);

            isInvalid = false;
        }

        public class Primers
        {
            // primer length
            readonly int primerLength;
            readonly PrimerRegion parent;

            /// <summary>
            /// Offset length for reverse side at a location specified by first dimesion
            /// </summary>
            int[][] selfDimers;
            /// <summary>
            /// Turning point for hair pin at a location specified by first dimesion
            /// </summary>
            float[][] hairPins;

            public Primers(PrimerRegion parent, int primerLength)
            {
                this.parent = parent;
                this.primerLength = primerLength;
                
            }

            internal void Validate()
            {
                int n = parent.seq.Length - primerLength + 1;
                selfDimers = new int[n][];
                hairPins = new float[n][];

                for (int i = 0; i < n; ++i)
                {
                    Primer primer = new Primer(parent.seq.SubSeq(i, primerLength));
                    //primer.SearchDimers();
                    selfDimers[i] = primer.SelfDimers;
                    //primer.SearchHairPins();
                    hairPins[i] = primer.HairPins;
                }
            }

            /// <summary>
            /// Return string of most stable self-dimer at given location 
            /// </summary>
            /// <param name="location"></param>
            /// <returns></returns>
            public string SelfDimer(int location, Form form)
            {
                if (selfDimers[location] == null)
                    return null;

                return Primer.PrintDimer(parent.seq, selfDimers[location][0], form);
            }

            /// <summary>
            /// Return pretty string of all dimers at given location
            /// </summary>
            /// <param name="location"></param>
            /// <returns></returns>
            public string[] SelfDimers(int location, Form form)
            {
                if (location > (parent.seq.Length - primerLength))
                {
                    throw new IndexOutOfRangeException("location must be less than (sequence length - primer length)");
                }
                if (selfDimers[location] == null)
                {
                    return new string[0];
                }
                else
                {
                    string[] ss = new string[selfDimers[location].Length];
                    for (int i = 0; i < ss.Length; ++i)
                    {
                        ss[i] = Primer.PrintDimer(parent.seq.SubSeq(parent.start + location, primerLength),
                            selfDimers[location][i], form);
                    }
                    return ss;
                }
            }


            public string[] HairPins(int location, Form form)
            {
                if (location > (parent.seq.Length - primerLength))
                {
                    throw new IndexOutOfRangeException("location must be less than (sequence length - primer length)");
                }
                if (hairPins[location] == null)
                {
                    return new string[0];
                }
                else
                {
                    string[] ss = new string[hairPins[location].Length];
                    for (int i = 0; i < ss.Length; ++i)
                    {
                        ss[i] = Primer.PrintHairPin(parent.seq.SubSeq(parent.start + location, primerLength),
                            hairPins[location][i], form);
                    }
                    return ss;
                }
            }

            /// <summary>
            /// Primer length
            /// </summary>
            public int PrimerLength { get { return primerLength; } }

            public int RegionLength { get { return parent.seq.Length; } }
        }


    }

    /// <summary>
    /// Form of string for dimer and hairpin
    /// </summary>
    public enum Form
    {
        Numeric,
        Simple,
        Camel,
        Pretty
    }

    public class Primer
    {
        readonly private DnaSeq seq;
        private int[] selfDimers;
        private float[] hairPins;

        public Primer(DnaSeq seq)
        {
            this.seq = seq;

            // should we use byte? think about possible overflow in filter

            bool[,] W = ComplementMatrix(seq);
           
            selfDimers = searchSelfDimers(W, 3, -1);
        
            hairPins = searchHairPins(W,  4, 2);


        }

        #region properties
        public Seq Seq { get { return seq; } }
        public int[] SelfDimers { get { return selfDimers; } }
        public float[] HairPins { get { return hairPins; } }
        #endregion 

        #region methods

        public void DumpSelfDimers(Form form)
        {
            if (selfDimers == null)
            {
                Console.WriteLine("No self dimmer");
                return;
            }
            for (int i = 0; i < selfDimers.Length; ++i)
            {
                Console.WriteLine(PrintDimer(seq, selfDimers[i], form));
            }
        }

        
        public void DumpHairPins(Form form)
        {
            if (hairPins == null) return;

            for (int i = 0; i < hairPins.Length; ++i)
            {
                Console.WriteLine(PrintHairPin(seq, hairPins[i], form));
            }
        }
        #endregion

        #region static helper functions

        public static string PrintDimer(DnaSeq seq, int revOffset, Form form)
        {
            if (form == Form.Pretty)
            {
                string revSeq = (string)seq.Reverse();
                StringBuilder sb2 = new StringBuilder(seq.Length);
                for (int j = 0; j < seq.Length; ++j)
                {
                    int rj = seq.Length - revOffset + j - 1;
                    /* selfDimers[i].Locations[j] */
                    if (rj >= 0 && rj < revSeq.Length && Seq.IsComplement(seq[j], revSeq[rj]) /* selfDimers[i].Locations[j]*/)
                    {
                        sb2.Append("|");
                    }
                    else
                    {
                        sb2.Append(" ");

                    }
                }
                return ((string)seq).PadLeft(seq.Length * 2 - 1) + "\n" + sb2.ToString().PadLeft(seq.Length * 2 - 1) + "\n" +
                    revSeq.PadLeft(revOffset + seq.Length);
            }
            else // numeric format assumed
            {
                return String.Format("{0} in {1}", revOffset, seq.Length);
            }
        }

        public static string PrintHairPin(DnaSeq seq, float pin, Form form)
        {
            int L = seq.Length;
            int iPin = (int)pin;
            string s = seq.ToString();
            int half = iPin == pin ? 1 : 2;
            //iPin -= half;

            if (form == Form.Pretty)
            {
                StringBuilder sb = new StringBuilder();
                if (half == 1) sb.Append(s[iPin]);
                string s1 = s.Substring(iPin+half);
                if (half == 1) s1 = " " + s1;
                StringBuilder sb2 = new StringBuilder();
                if (half == 1) sb2.Append(" ");

                for (int i = 0; i < iPin; ++i)
                {
                    sb2.Append(s[iPin - i - 1]);
                    if ((iPin + half + i) < L &&
                        Seq.IsComplement(s[iPin - i - 1], s[iPin + i + half]))
                    {
                        sb.Append('|');
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }

                return s1 + "\n" + sb.ToString() + "\n" + sb2.ToString();
            }
            else if (form == Form.Camel)
            {
                StringBuilder sb = new StringBuilder();
                half = half == 1 ? 0 : 1;
                for (int i = 0; i < seq.Length; ++i)
                {
                    bool pair = false;
                    if ((i <= iPin && ((iPin + 1) * 2 - i) < seq.Length) &&
                        Seq.IsComplement(seq[i], seq[(iPin + 1) * 2 - i - 1]))
                    {
                        pair = true;
                    }
                    if (pair)
                    {
                        sb.Append(Char.ToUpper(seq[i]));
                    }
                    else
                    {
                        sb.Append(seq[i]);
                    }
                }
                return sb.ToString();
            }
            else if (form == Form.Simple)
            {
                if (half == 1)
                {
                    return s.Substring(0, iPin) + "]" + s.Substring(iPin, 1) + "[" + s.Substring(iPin + 1);
                }
                else
                {
                    return s.Substring(0, iPin) + "|" + s.Substring(iPin);
                }
            }
            else
            {
                return String.Format("{0} of {1}", pin, seq.Length);
            }
        }

        /// <summary>
        /// holds positions that complement with the reverse, or Ns
        /// </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static bool[,] ComplementMatrix(DnaSeq seq)
        {
            int[] B = (int[])seq;
            int n = seq.Length - 1;
            int[] c = new int[seq.Length];
            c[0] = B[0];
            int[] r = new int[2 * seq.Length - 1];
            for (int i = 0; i < seq.Length; ++i)
                r[i] = B[i];
            // ([B(1) zeros(1,n)],[B zeros(1,n)]);
            int[,] T = MathLib.General.Toeplitz(c, r);
            // 5-B' is the complement of the sequence B
            int[,] R = new int[B.Length, n + n + 1];
            for (int i = 0; i < R.GetLength(0); ++i)
                for (int j = 0; j < R.GetLength(1); ++j)
                    R[i, j] = 5 - B[i];
            // W holds positions that complement with the reverse, or Ns
            bool[,] W = new bool[T.GetLength(0), T.GetLength(1)];
            // W = (T==R) | (R~=0 & T==15) | (R==-10 & T~=0); 
            for (int i = 0; i < T.GetLength(0); ++i)
                for (int j = 0; j < T.GetLength(1); ++j)
                    W[i, j] = T[i, j] == R[i, j] |
                        (R[i, j] != 0 & T[i, j] == 15) |
                        (R[i, j] == -10 & T[i, j] != 0);

            return W;
        }

        /// <summary>
        /// Search hair pin
        /// </summary>
        /// <param name="W">complement matrix</param>
        /// <param name="minHpinBases"></param>
        /// <param name="minHpinLoop"></param>
        /// <returns></returns>
        public static float[] searchHairPins(bool[,] W, /*Seq seq,*/ int minHpinBases, int minHpinLoop)
        {
            int[,] filter = General.Filter(W, minHpinBases);

            //m = new MathNet.Numerics.LinearAlgebra.Matrix(filter);
            //Console.WriteLine("filter2: \n{0}", m);

            double[,] index = new double[W.GetLength(0), W.GetLength(1)];
            for (int i = 0; i < W.GetLength(0); ++i)
            {
                for (int j = 0; j < W.GetLength(1); ++j)
                    index[i, j] = i + 1;
            }
            //m = new MathNet.Numerics.LinearAlgebra.Matrix(index);
            //Console.WriteLine("index: \n{0}", m);

            for (int i = 0; i < filter.GetLength(0); ++i)
                for (int j = 0; j < filter.GetLength(1); ++j)
                    if (filter[i, j] != minHpinBases)
                        index[i, j] = Double.NaN;

            //m = new MathNet.Numerics.LinearAlgebra.Matrix(index);
            //Console.WriteLine("index2: \n{0}", m);

            List<int> hpins = new List<int>();
            for (int i = 0; i < index.GetLength(1); ++i)
            {
                // Console.WriteLine("{0} => Max: {1}, Min: {2}", i, index[i].Max(), index[i].Min()); // doesn't work
                double max = 0.0;
                double min = Int32.MaxValue;
                for (int j = 0; j < index.GetLength(0); ++j)
                {
                    if (!Double.IsNaN(index[j, i]))
                    {
                        if (index[j, i] > max)
                            max = index[j, i];
                        if (index[j, i] < min)
                            min = index[j, i];
                    }
                }
                //Console.WriteLine("{0} => Max: {1}, Min: {2}", i, max, min);
                if (max - min >= minHpinBases + minHpinLoop)
                    hpins.Add(i);

            }
            //int[] hairPins = hpins.ToArray();
            //Console.WriteLine(hairPins[0]);

            float[] hairPins = new float[hpins.Count];
            for (int i = 0; i < hpins.Count; ++i)
            {
                int s = W.GetLength(1);
                int e = 0;
                for (int j = 0; j < W.GetLength(0); ++j)
                {
                    if (W[j, hpins[i]])
                    {
                        if (j < s)
                            s = j;
                        if (j > e)
                            e = j;
                    }
                    //if (!W[j-1, hpins[i]])
                    //{
                    //    Console.Write(seq[j-1]);
                    //}
                    //else
                    //{
                    //    Console.Write(Char.ToUpper(seq[j-1]));
                    //}
                }
                float pin = (s + e) / 2.0f;
                //Console.WriteLine(" {0:F}", pin);
                hairPins[i] = pin;
            }
            return hairPins;
        }

        /// <summary>
        /// find positions that forms self dimer and possible hairpins
        /// </summary>
        /// <param name="W">Complement matrix</param>
        /// <param name="minDimerCount">minumn number of total complementary base count</param>
        /// <param name="minDimerLength">minimum number of bases for dimers</param>
        public static int[] searchSelfDimers(bool[,] W, int minDimerLength, int minDimerCount)
        {
            // column indeces with at least dBases matches and/or Ns
            int[,] filter = MathLib.General.Filter(W, minDimerLength);
            List<int> selfComp = new List<int>();
            for (int k1 = 0; k1 < W.GetLength(1); ++k1)
            {
                for (int k2 = 0; k2 < W.GetLength(0); ++k2)
                {
                    if (filter[k2, k1] == minDimerLength)
                    {
                        if (minDimerCount > 0)
                        {
                            int dimerCount = 0;
                            for (int k3 = 0; k3 < W.GetLength(0); ++k3)
                                if (W[k3, k1]) ++dimerCount;
                            if (dimerCount < minDimerCount) continue;
                        }
                        if (!selfComp.Contains(k1))
                            selfComp.Add(k1);
                        break;
                    }
                }
            }

            //MathNet.Numerics.LinearAlgebra.Matrix m = new MathNet.Numerics.LinearAlgebra.Matrix(W);
            //Console.WriteLine("W: \n{0}", m);
            //m = new MathNet.Numerics.LinearAlgebra.Matrix(filter);
            //Console.WriteLine("filter: \n{0}", m);

            if (selfComp.Count == 0)
                return null;
            else 
                return selfComp.ToArray();


        }

        #endregion
    }



}
