/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BioCSharp.Seqs
{
    public class DnaSeq : NucleotideSeq
    {
        public DnaSeq(string data)
            : base(data)
        {

        }

        #region operators
        /// <summary>
        /// convert a string of nucleotides from letters to numbers.
        /// A => 1 , C => 2, G => 3,  T(U) => 4.
        /// </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static implicit operator byte[](DnaSeq seq)
        {
            byte[] data = new byte[seq.data.Length];
            for (int i = 0; i < seq.data.Length; ++i)
            {
                data[i] = DnaSeq.Nt2Byte(seq.data[i]);
            }
            return data;
        }


        /// <summary>
        /// convert a string of nucleotides from letters to numbers.
        /// A => 1 , C => 2, G => 3,  T(U) => 4.
        /// </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static implicit operator int[](DnaSeq seq)
        {
            int[] data = new int[seq.data.Length];
            for (int i = 0; i < seq.data.Length; ++i)
            {
                data[i] = DnaSeq.Nt2Byte(seq.data[i]);
            }
            return data;
        }


        public static implicit operator DnaSeq(string data)
        {
            return new DnaSeq(data);
        }
        #endregion


        #region methods

        /// <summary>
        /// Retrive a <code>Seq</code> from this instance giving <paramref name="start"/> and 
        /// length of string. Use negative length to get reverse sequence.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length">length of resulting sequence. can be negative</param>
        /// <returns></returns>
        new public DnaSeq SubSeq(int start, int length)
        {
            if (length >= 0)
            {
                return new DnaSeq(data.Substring(start, length));
            }
            else
            {
                return SubSeq(start, -length).Reverse();
            }
        }

        public double GC
        {
            get
            {
                if (Double.IsNaN(_gc))
                {
                    _gc = (100.0 * (CountG + CountC + CountN / 2.0)) / Length;
                }
                return _gc;
            }
        }

        public double GCWithN
        {
            get
            {
                if (Double.IsNaN(_gcwn))
                {
                    _gcwn = (100.0 * (CountG + CountC)) / CountBasesWithN.Sum();
                }
                return _gcwn;
            }
        }

        public double MolWeight
        {
            get
            {
                if (Double.IsNaN(_molWeight))
                {
                    _molWeight = CountA * DNAMolWeight[0] +
                                CountC * DNAMolWeight[1] +
                                CountG * DNAMolWeight[2] +
                                CountT * DNAMolWeight[3] - 61.96;
                    if (HasAmbiguous)
                    {
                        _molWeight += CountN * (DNAMolWeight[2] + DNAMolWeight[1]) / 2.0;
                    }


                }
                return _molWeight;
            }
        }

        public double MolWeightWithN
        {
            get
            {
                if (Double.IsNaN(_molWeightwn))
                {
                    _molWeightwn = CountA * DNAMolWeight[0] +
                        CountG * DNAMolWeight[1] +
                        CountC * DNAMolWeight[2] +
                        CountT * DNAMolWeight[3] - 61.96 +
                        CountN * (DNAMolWeight[2] + DNAMolWeight[3]) / 2.0;


                }
                return _molWeightwn;
            }
        }

        /// <summary>
        /// codon counts for a sequence for a given frame
        /// </summary>
        /// <param name="seq">sequence</param>
        /// <param name="frame">1, 2, 3, -1, -2, -3</param>
        /// <returns></returns>
        public static int[, ,] CodonCount(string seq, int frame)
        {
            int[] validFrames = { 1, 2, 3, -1, -2, -3, 0 };
            if (validFrames.Contains<int>(frame))
                throw new ArgumentException("Invalid frame number: " + frame);

            int[, ,] count = new int[5, 5, 5];

           
            if (frame < 0)
            {
                seq = revcomplement(seq);
                frame = -frame;
            }

            for (int i = frame; i < seq.Length - 2; ++i)
            {
                int k1 = DNAnChars.IndexOf(seq[i]);
                int k2 = DNAnChars.IndexOf(seq[i + 1]);
                int k3 = DNAnChars.IndexOf(seq[i + 2]);
                if (k1 < 0 | k2 < 0 | k3 < 0)
                {
                    continue;
                }

                ++count[k1, k2, k3];
            }

            return count;
        }

        public int[] CountBases
        {
            get
            {
                if (_countBases == null)
                {
                    _countBases = new int[DnaSeq.DNAChars.Length];
                    foreach (char c in this)
                    {
                        int idx = DnaSeq.DNAChars.IndexOf(c);
                        if (idx >= 0)
                            ++_countBases[idx];
                    }
                }
                return _countBases;
            }
        }

        public int[] CountBasesWithN
        {
            get
            {
                if (_countBaseNs == null)
                {
                    _countBaseNs = new int[DNAnChars.Length];
                    foreach (char c in this)
                    {
                        int idx = DNAnChars.IndexOf(c);
                        if (idx >= 0)
                            ++_countBaseNs[idx];
                    }
                }
                return _countBaseNs;
            }
        }

        public int CountA { get { return CountBases[0]; } }
        public int CountC { get { return CountBases[1]; } }
        public int CountG { get { return CountBases[2]; } }
        public int CountT { get { return CountBases[3]; } }
        public int CountN
        {
            get
            {
                return CountBasesWithN[4];
            }
        }



        /// <summary>
        /// Return true if the sequence contains only A or T
        /// <seealso cref="IsOnlyGC"/> <seealso cref="IsSymmetry"/>
        /// </summary>
        public bool IsOnlyAT
        {
            get
            {
                if (_isOnlyAT == null)
                {
                    _isOnlyAT = true;
                    for (int i = 0; i < Length; ++i)
                    {
                        if (data[i] != 'a' | data[i] != 't' | data[i] != 'n')
                        {
                            _isOnlyAT = false;
                            break;
                        }
                    }
                }
                return (bool)_isOnlyAT;
            }
        }

        /// <summary>
        /// Return true if the sequence contains only G or C
        /// <seealso cref="IsOnlyAT"/> <seealso cref="IsSymmetry"/>
        /// </summary>
        public bool IsOnlyGC
        {
            get
            {
                if (_isOnlyGC == null)
                {
                    _isOnlyGC = true;
                    for (int i = 0; i < Length; ++i)
                    {
                        if (data[i] != 'a' | data[i] != 't' | data[i] != 'n')
                        {
                            _isOnlyGC = false;
                            break;
                        }
                    }
                }
                return (bool)_isOnlyGC;
            }
        }

        /// <summary>
        /// Returns the complementary strand of a DNA sequence.
        /// </summary>
        public string Complement { get { return DnaSeq.complement(data); } }

        /// <summary>
        /// Returns the reverse strand of a DNA or RNA sequence.
        /// </summary>
        public DnaSeq Reverse() { return new DnaSeq(DnaSeq.reverse(data)); }

        /// <summary>
        /// Returns the reverse complementary strand of a DNA sequence.
        /// </summary>
        public DnaSeq Revcomplement() { return new DnaSeq(DnaSeq.revcomplement(data)); }

        /// <summary>
        /// True if the sequence is reverse complement to itself.
        /// <seealso cref="IsOnlyAT"/> <seealso cref="IsOnlyAT"/>
        /// </summary>
        public bool IsSymmetry
        {
            get
            {
                if (_isSelfRevComplement == null)
                {
                    _isSelfRevComplement = true;
                    for (int i = 0; i < Length; ++i)
                    {
                        if (!DnaSeq.IsComplement(data[i], data[Length - i - 1]))
                        {
                            _isSelfRevComplement = false;
                            break;
                        }
                    }
                }
                return (bool)_isSelfRevComplement;
            }
        }

        /// <summary>
        /// Returns the complementary strand of a DNA sequence. Calculates the complementary strand 
        /// (A-->T, C-->G, G-->C, T-->A) of sequence DNA.
        /// </summary>
        /// <param name="dna"></param>
        /// <returns></returns>
        static public string complement(string dna)
        {
            //% Full mapping of all nucleotide symbols
            //% * A C G T(U) R Y K M S W B D H V N - 
            //% * T G C A    Y R M K S W V H D B N - 
            StringBuilder sb = new StringBuilder(dna.Length);
            foreach (char c in dna)
            {
                switch (c)
                {
                    case '*': sb.Append('*'); break;
                    case 'a': sb.Append('t'); break;
                    case 'c': sb.Append('g'); break;
                    case 'g': sb.Append('c'); break;
                    case 't': sb.Append('a'); break;
                    case 'u': sb.Append('a'); break;
                    case 'r': sb.Append('y'); break;
                    case 'y': sb.Append('r'); break;
                    case 'k': sb.Append('m'); break;
                    case 'm': sb.Append('k'); break;
                    case 's': sb.Append('s'); break;
                    case 'w': sb.Append('w'); break;
                    case 'b': sb.Append('v'); break;
                    case 'd': sb.Append('h'); break;
                    case 'h': sb.Append('d'); break;
                    case 'v': sb.Append('b'); break;
                    case 'n': sb.Append('n'); break;
                    default: sb.Append('-'); break;
                }
            }
            return sb.ToString();
        }



        /// <summary>
        /// Returns the reverse complementary strand of a DNA sequence. calculates 
        /// the reverse complementary strand  3' --> 5' (A-->T, C-->G, G-->C, T-->A) of sequence DNA.
        /// </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        static public string revcomplement(string seq)
        {
            return DnaSeq.reverse(DnaSeq.complement(seq));
        }


        /// <summary>
        /// convert a string of nucleotides from character to byte.
        /// </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static byte Nt2Byte(char c)
        {
            if (c == 'a')
                return 1;
            else if (c == 'b')
                return 11;
            else if (c == 'c')
                return 2;
            else if (c == 'd')
                return 12;
            else if (c == 'e')
                return 0;
            else if (c == 'f')
                return 0;
            else if (c == 'g')
                return 3;
            else if (c == 'h')
                return 13;
            else if (c == 'i')
                return 0;
            else if (c == 'j')
                return 0;
            else if (c == 'k')
                return 7;
            else if (c == 'l')
                return 7;
            else if (c == 'm')
                return 0;
            else if (c == 'n')
                return 15;
            else if (c == 'o')
                return 0;
            else if (c == 'p')
                return 0;
            else if (c == 'q')
                return 0;
            else if (c == 'r')
                return 5;
            else if (c == 's')
                return 9;
            else if (c == 't')
                return 4;
            else if (c == 'u')
                return 4;
            else if (c == 'v')
                return 14;
            else if (c == 'w')
                return 10;
            else if (c == 'x')
                return 15;
            else if (c == 'y')
                return 6;
            else if (c == 'z')
                return 0;
            else
                return 16;

        }

        /// <summary>
        /// Get melting temperature using basic formula. 4 * (#AT) + 2 * (#GC)
        /// </summary>
        /// <returns></returns>
        public double BasicMeltingTemp()
        {
            double basic;

            if (!HasAmbiguous)
            {

                if (Length < 14)
                {
                    basic = 2.0 * (CountA + CountT) + 4.0 * (CountG + CountC);
                }
                else
                {
                    basic = 64.9 + (41.0 * ((CountC + CountG - 16.4) / Length));
                }
            }
            else
            {
                if (Length < 14)
                {
                    basic = 2.0 * (CountA + CountT) + 4 * (CountG + CountC) + 3 * CountN;
                }
                else
                {
                    basic = 64.9 + (41.0 * (CountC + CountG - 16.4 + CountN / 2.0) / Length);
                }
            }
            return basic;
        }

        /// <summary>
        /// Adjustment melting temperature at default salt concentration of 0.05 M
        /// </summary>
        /// <returns></returns>
        public double SaltAdjustTm()
        {
            return SaltAdjustTm(0.05);
        }

        /// <summary>
        /// Adjustment melting temperature for a salt concentration (M)
        /// </summary>
        /// <param name="salt">salt concentration</param>
        /// <returns></returns>
        public double SaltAdjustTm(double salt)
        {
            double saltadj;
            double basic = BasicMeltingTemp();

            if (!HasAmbiguous)
            {

                if (Length < 14)
                {
                    saltadj = basic - 16.6 * Math.Log10(0.05) + 16.6 * Math.Log10(salt);
                }
                else
                {
                    saltadj = 100.5 + ((41.0 * (CountC + CountG)) / Length) -
                        (820.0 / Length) + (16.6 * Math.Log10(salt)); // TM SALT ADJUSTED [9]
                }
            }
            else
            {
                if (Length < 14)
                {

                    saltadj = basic - 16.6 * Math.Log10(0.05) + 16.6 * Math.Log10(salt);

                }
                else
                {
                    saltadj = 100.5 - (820.0 / Length) + (16.6 * Math.Log10(salt))
                        + 41.0 * ((CountG + CountC + CountN / 2.0) / Length);
                }
            }
            return saltadj;
        }

        /// <summary>
        /// calculate melting temperature
        /// </summary>
        /// <param name="salt">Value that specifies a salt concentration in moles/liter for melting 
        /// temperature calculations. </param>
        /// <param name="primerConc">Value that specifies the primer concentration in moles/liter for melting
        /// temperature calculations.</param>
        /// <param name="NN">thermo values as returned by <code>NearNeigh</code></param>
        /// <returns>melting temperature and +/- delta level</returns>
        public double MeltingTemp(double salt, double primerConc, double[] NN)
        {
            double b = 4.0;
            if (IsSymmetry)
                b = 1.0; // correction value for nearest neighbor melting temperature calculation

            //double basic = BasicMeltingTemp();
            double saltadj = SaltAdjustTm(salt);
            double tmdeltaH;
            double tmdeltaS;
            double tm;

            if (NN == null) NN = ThermoProp(NearestNeighborMethod.Sant98);

            if (!HasAmbiguous)
            {

                tm = (NN[0] * 1000.0 / (NN[1] + (1.9872 * Math.Log(primerConc / b)))) + (16.6 * Math.Log10(salt)) - 273.15; // TM NEAREST NEIGHBOR
            }
            else
            {
                if (Length < 14)
                {
                    tmdeltaH = CountN;
                    tmdeltaS = 3.0 * CountN;
                }
                else
                {
                    tmdeltaH = 1.0 / 2.0 * 41.0 * (CountN / Length);
                    tmdeltaS = 41.0 * 1.0 / 2.0 * (CountN / Length);
                }

                tm = (((NN[0] + NN[2]) * 1000.0 / ((NN[1] + NN[3]) + (1.9872 * Math.Log(primerConc / b)))) + (16.6 * Math.Log10(salt)) - 273.15 +
                    ((NN[0] - NN[2]) * 1000.0 / ((NN[1] - NN[3]) + (1.9872 * Math.Log(primerConc / b)))) + (16.6 * Math.Log10(salt)) - 273.15) * 1.0 / 2.0; // NEAREST NEIGHBOR

            }



            return tm;
        }

        static readonly double[,] Bres86_H = { { -9.1, -6.5, -7.8, -8.6 }, { -5.8, -11, -11.9, -7.8 }, { -5.6, -11.1, -11, -6.5 }, { -6, -5.6, -5.8, -9.1 } };
        static readonly double[,] Bres86_S = { { -24, -17.3, -20.8, -23.9 }, { -12.9, -26.6, -27.8, -20.8 }, { -13.5, -26.7, -26.6, -17.3 }, { -16.9, -13.5, -12.9, -24 } };
        static readonly double[,] Sant96_H = { { -8.4, -8.6, -6.1, -6.5 }, { -7.4, -6.7, -10.1, -6.1 }, { -7.7, -11.1, -6.7, -8.6 }, { -6.3, -7.7, -7.4, -8.4 } };
        static readonly double[,] Sant96_S = { { -23.6, -23, -16.1, -18.8 }, { -19.3, -15.6, -25.5, -16.1 }, { -20.3, -28.4, -15.6, -23 }, { -18.5, -20.3, -19.3, -23.6 } };
        static readonly double[,] Sant98_H = { { -7.9, -8.4, -7.8, -7.2 }, { -8.5, -8, -10.6, -7.8 }, { -8.2, -9.8, -8, -8.4 }, { -7.2, -8.2, -8.5, -7.9 } };
        static readonly double[,] Sant98_S = { { -22.2, -22.4, -21, -20.4 }, { -22.7, -19.9, -27.2, -21 }, { -22.2, -24.4, -19.9, -22.4 }, { -21.3, -22.2, -22.7, -22.2 } };
        static readonly double[,] Sugi96_H = { { -8, -9.4, -6.6, -5.6 }, { -8.2, -10.9, -11.8, -6.6 }, { -8.8, -10.5, -10.9, -9.4 }, { -6.6, -8.8, -8.2, -8 } };
        static readonly double[,] Sugi96_S = { { -21.9, -25.5, -16.4, -15.2 }, { -21, -28.4, -29, -16.4 }, { -23.5, -26.4, -28.4, -25.5 }, { -18.4, -23.5, -21, -21.9 } };

        static readonly double[,] Bres86_H_WithN = { { -9.1, -6.5, -7.8, -8.6, -8 }, { -5.8, -11, -11.9, -7.8, -9.125 }, { -5.6, -11.1, -11, -6.5, -8.55 }, { -6, -5.6, -5.8, -9.1, -6.625 }, { -6.625, -8.55, -9.125, -8, -8.075 } };
        static readonly double[,] Bres86_S_WithN = { { -24, -17.3, -20.8, -23.9, -21.5 }, { -12.9, -26.6, -27.8, -20.8, -22.025 }, { -13.5, -26.7, -26.6, -17.3, -21.025 }, { -16.9, -13.5, -12.9, -24, -16.825 }, { -16.825, -21.025, -22.025, -21.5, -20.3438 } };
        static readonly double[,] Sant96_H_WithN = { { -8.4, -8.6, -6.1, -6.5, -7.4 }, { -7.4, -6.7, -10.1, -6.1, -7.575 }, { -7.7, -11.1, -6.7, -8.6, -8.525 }, { -6.3, -7.7, -7.4, -8.4, -7.45 }, { -7.45, -8.525, -7.575, -7.4, -7.7375 } };
        static readonly double[,] Sant96_S_WithN = { { -23.6, -23, -16.1, -18.8, -20.375 }, { -19.3, -15.6, -25.5, -16.1, -19.125 }, { -20.3, -28.4, -15.6, -23, -21.825 }, { -18.5, -20.3, -19.3, -23.6, -20.425 }, { -20.425, -21.825, -19.125, -20.375, -20.4375 } };
        static readonly double[,] Sant98_H_WithN = { { -7.9, -8.4, -7.8, -7.2, -7.825 }, { -8.5, -8, -10.6, -7.8, -8.725 }, { -8.2, -9.8, -8, -8.4, -8.6 }, { -7.2, -8.2, -8.5, -7.9, -7.95 }, { -7.95, -8.6, -8.725, -7.825, -8.275 } };
        static readonly double[,] Sant98_S_WithN = { { -22.2, -22.4, -21, -20.4, -21.5 }, { -22.7, -19.9, -27.2, -21, -22.7 }, { -22.2, -24.4, -19.9, -22.4, -22.225 }, { -21.3, -22.2, -22.7, -22.2, -22.1 }, { -22.1, -22.225, -22.7, -21.5, -22.1313 } };
        static readonly double[,] Sugi96_H_WithN = { { -8, -9.4, -6.6, -5.6, -7.4 }, { -8.2, -10.9, -11.8, -6.6, -9.375 }, { -8.8, -10.5, -10.9, -9.4, -9.9 }, { -6.6, -8.8, -8.2, -8, -7.9 }, { -7.9, -9.9, -9.375, -7.4, -8.6437 } };
        static readonly double[,] Sugi96_S_WithN = { { -21.9, -25.5, -16.4, -15.2, -19.75 }, { -21, -28.4, -29, -16.4, -23.7 }, { -23.5, -26.4, -28.4, -25.5, -25.95 }, { -18.4, -23.5, -21, -21.9, -21.2 }, { -21.2, -25.95, -23.7, -19.75, -22.65 } };

        public enum NearestNeighborMethod
        {
            [Description("Breslauer et. al. 1986")]
            Bres86 = 0,
            [Description("SantaLucia et. al. 1996")]
            Sant96 = 1,
            [Description("SantaLucia et. al. 1998")]
            Sant98 = 2,
            [Description("Sugimoto et. al. 1996")]
            Sugi96 = 3
        }

        /// <summary>
        /// compute thermo values using Nearest Neighbor methods using default method 
        /// <code>NearestNeighborMethod.Sant98</code>
        /// </summary>
        /// <returns>Columns are H, S, DeltaH and DeltaS</code>.</returns>
        public double[] ThermoProp() { return ThermoProp(NearestNeighborMethod.Sant98); }

        /// <summary>
        /// compute thermo values using Nearest Neighbor methods
        /// </summary>
        /// <returns>Columns are H, S, DeltaH and DeltaS</code>.</returns>
        public double[] ThermoProp(NearestNeighborMethod method)
        {
            double[] NN = new double[4];
            if (!HasAmbiguous)
            {
                for (int k = 0; k < Length - 1; ++k)
                {
                    int i = DNAChars.IndexOf(this[k]);
                    int j = DNAChars.IndexOf(this[k + 1]);
                    if (method == NearestNeighborMethod.Bres86)
                    {
                        NN[0] += Bres86_H[i, j];
                        NN[1] += Bres86_S[i, j];
                    }
                    else if (method == NearestNeighborMethod.Sant96)
                    {
                        NN[0] += Sant96_H[i, j];
                        NN[1] += Sant96_S[i, j];
                    }
                    else if (method == NearestNeighborMethod.Sant98)
                    {
                        NN[0] += Sant98_H[i, j];
                        NN[1] += Sant98_S[i, j];
                        //Console.WriteLine("{0}, {1}: {2}, {3}", i, j, Sant98_H[i, j], Sant98_S[i, j]);
                    }
                    else
                    {
                        NN[0] += Sugi96_H[i, j];
                        NN[1] += Sugi96_S[i, j];
                    }
                }

                if (IsOnlyAT)
                {
                    if (method == NearestNeighborMethod.Bres86)
                        NN[1] += -20.13;
                    else if (method == NearestNeighborMethod.Sant96)
                        NN[1] += -9.0;
                    else if (method == NearestNeighborMethod.Sugi96)
                    {
                        NN[0] += 0.6;
                        NN[1] += -9.0;
                    }
                }
                else
                {
                    if (method == NearestNeighborMethod.Bres86)
                        NN[1] += -16.77;
                    else if (method == NearestNeighborMethod.Sant96)
                        NN[1] += -5.9;
                    else if (method == NearestNeighborMethod.Sugi96)
                    {
                        NN[0] += 0.6;
                        NN[1] += -9.0;
                    }
                }

                if (IsSymmetry)
                {
                    if (method == NearestNeighborMethod.Bres86)
                        NN[1] += -1.34;
                    else if (method == NearestNeighborMethod.Sant96)
                        NN[1] += -1.4;
                    else if (method == NearestNeighborMethod.Sant98)
                        NN[1] += -1.4;
                    else if (method == NearestNeighborMethod.Sugi96)
                        NN[1] += -1.4;
                }

                // initiation with terminal  5'
                if (this[0] == 'c' | this[0] == 'g')
                {
                    if (method == NearestNeighborMethod.Sant98)
                    {
                        NN[0] += 0.1;
                        NN[1] += -2.8;
                    }
                }
                else if (this[Length - 1] == 'a' | this[Length - 1] == 't')
                {
                    if (method == NearestNeighborMethod.Sant98)
                    {
                        NN[0] += 2.3;
                        NN[1] += 4.1;
                    }
                }

                // initiation with terminal  3'
                if (this[Length - 1] == 'g' | this[Length - 1] == 'c')
                {
                    if (method == NearestNeighborMethod.Sant98)
                    {
                        NN[0] += 0.1;
                        NN[1] += -2.8;
                    }
                }
                else if (this[Length - 1] == 'a' | this[Length - 1] == 't')
                {
                    if (method == NearestNeighborMethod.Sant98)
                    {
                        NN[0] += 2.3;
                        NN[1] += 4.1;
                    }
                }
            }
            else
            {
                for (int k = 0; k < Length - 1; ++k)
                {
                    int i = DNAnChars.IndexOf(this[k]);
                    int j = DNAnChars.IndexOf(this[k + 1]);
                    if (method == NearestNeighborMethod.Bres86)
                    {
                        NN[0] += Bres86_H_WithN[i, j];
                        NN[1] += Bres86_S_WithN[i, j];
                    }
                    else if (method == NearestNeighborMethod.Sant96)
                    {
                        NN[0] += Sant96_H_WithN[i, j];
                        NN[1] += Sant96_S_WithN[i, j];
                    }
                    else if (method == NearestNeighborMethod.Sant98)
                    {
                        NN[0] += Sant98_H_WithN[i, j];
                        NN[1] += Sant98_S_WithN[i, j];
                    }
                    else
                    {
                        NN[0] += Sugi96_H_WithN[i, j];
                        NN[1] += Sugi96_S_WithN[i, j];
                    }
                }

                if (IsSymmetry)
                {
                    if (method == NearestNeighborMethod.Bres86)
                        NN[1] += -16.77;
                    else if (method == NearestNeighborMethod.Sant96)
                        NN[1] += -5.9;
                    else if (method == NearestNeighborMethod.Sant98)
                        NN[1] += -1.4;
                    else if (method == NearestNeighborMethod.Sugi96)
                        NN[1] += -1.4;
                }

                double[] NN1 = new double[4]; // case when all Ns are G/C
                for (int i = 0; i < NN.Length; ++i) NN1[i] = NN[i];
                double[] NN2 = new double[4]; // case when all Ns are A/T
                for (int i = 0; i < NN.Length; ++i) NN2[i] = NN[i];

                if (IsOnlyAT)
                {
                    if (method == NearestNeighborMethod.Bres86)
                        NN1[1] += -20.13;
                    else if (method == NearestNeighborMethod.Sant96)
                        NN1[1] += -9.0;
                    else if (method == NearestNeighborMethod.Sugi96)
                    {
                        NN1[0] += 0.6;
                        NN1[1] += -9.0;
                    }
                }
                else
                {
                    if (method == NearestNeighborMethod.Bres86)
                        NN2[1] += -16.77;
                    else if (method == NearestNeighborMethod.Sant96)
                        NN2[1] += -5.9;
                    else if (method == NearestNeighborMethod.Sugi96)
                    {
                        NN2[0] += 0.6;
                        NN2[1] += -9.0;
                    }
                }


                // initiation with terminal 5'(only Sant98)
                if (method == NearestNeighborMethod.Sant98)
                {
                    if (this[0] == 'g' | this[0] == 'c' | this[0] == 'n')
                    {
                        NN2[0] += 0.1;
                        NN2[1] += -2.8;
                    }
                    else if (this[0] == 'a' | this[0] == 't' | this[0] == 'n')
                    {
                        NN1[0] += 2.3;
                        NN1[1] += 4.1;
                        //NN[NN_Sant96, 1] += 0.4;
                    }


                    // initiation with terminal  3'
                    if (this[Length - 1] == 'g' | this[Length - 1] == 'c')
                    {
                        NN2[0] += 0.1;
                        NN2[1] += -2.8;
                    }
                    else if (this[Length - 1] == 'a' | this[Length - 1] == 't' | this[Length - 1] == 'n')
                    {
                        NN1[0] += 2.3;
                        NN1[1] += 4.1;
                        //NN[NN_Sant96, 1] += 0.4;
                    }
                }

                // average
                NN[0] = (NN1[0] + NN2[0]) * 0.5;
                NN[1] = (NN1[1] + NN2[1]) * 0.5;
                NN[2] = (Math.Max(NN1[0], NN2[0]) - Math.Min(NN1[0], NN2[0])) / 2.0; // delta level
                NN[3] = (Math.Max(NN1[1], NN2[1]) - Math.Min(NN1[1], NN2[1])) / 2.0; // delta level
            }

            return NN;
        }

        /// <summary>
        /// calculate melting temperature using default salt concentration 0.05 and
        /// default primer concentration of 50e-6 and default method for themo property calcualtion.
        /// </summary>
        /// <returns></returns>
        public double MeltingTemp()
        {
            return MeltingTemp(0.05, 50e-6, null);
        }



        #endregion

        #region catch properties
        /// <summary>
        /// GC percent
        /// </summary>
        double _gc = Double.NaN;
        bool? _isOnlyAT = null;
        bool? _isOnlyGC = null;
        double _gcwn = Double.NaN;
        bool? _isSelfRevComplement = null;
        #endregion
    }
}
