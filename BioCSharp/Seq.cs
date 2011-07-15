using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using MathNet.Numerics.LinearAlgebra;



namespace BioCSharp
{
    namespace Seqs
    {

        public enum Alphabet
        {
            [Description("Unknown")]
            Unknown = 0,
            [Description("Protein")]
            Protein = 2,
            [Description("Nucleotide")]
            Nucleotide = 4,
            [Description("DNA")]
            DNA = 8,
            [Description("RNA")]
            RNA = 16
        };





        /// <summary>
        /// Immutable generic sequence. By convention, nucleotide sequence (DNA, RNA) are lower case
        /// and protein sequence are upper case.
        /// </summary>
        public partial class Seq
        {
            #region members
            protected readonly string data = null;
            //protected readonly Alphabet alphabet = Alphabet.Unknown;
            #endregion

         
            /// <summary>
            /// Generic Sequence. Use factory method to create sequence.
            /// </summary>
            /// <param name="data"></param>
            public Seq(string data)
            {
                if (data != null)
                {
                    this.data = data.Replace(" ", "").Replace("\t", "").Replace("\n", "");
                }
                else
                {
                    this.data = "";
                }
            }

            /// <summary>
            /// Create appriorate sequence
            /// </summary>
            /// <param name="sequence"></param>
            /// <returns></returns>
            public static Seq Create(string sequence)
            {
                Alphabet alpha = Seq.GuessAlphabet(sequence);
                if (alpha == Alphabet.DNA) return new DnaSeq(sequence);
                else if (alpha == Alphabet.Protein) return new ProteinSeq(sequence);
                else if (alpha == Alphabet.Nucleotide) return new NucleotideSeq(sequence);
                else if (alpha == Alphabet.RNA) throw new NotImplementedException();
                else return new Seq(sequence);
            }


            #region properties
            /// <summary>
            /// Length of sequence. 
            /// <seealso cref="SequenceLength"/>
            /// </summary>
            public int Length
            {
                get { return data == null ? 0 : data.Length; }
            }

            /// <summary>
            /// Get a character at a specific position
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            public char this[int i]
            {
                get { return data[i]; }
            }

            public IEnumerator<char> GetEnumerator()
            {
                for (int i = 0; i < Length; ++i)
                    yield return this[i];
            }

            /// <summary>
            /// Raw sequence data. By convention, nucleotide sequence (DNA, RNA) are lower case
            /// and protein sequence are upper case.
            /// </summary>
            public string Data
            {
                get { return data; }
            }

            /// <summary>
            /// Nice representation of sequence length, e.g: "123 bp", "123 AA"
            /// <seealso cref="Length"/>
            /// </summary>
            public string SequenceLength
            {
                get
                {
                    switch (Alphabet)
                    {
                        case Alphabet.Unknown:
                            return String.Format("{0}", data.Length);
                        case Alphabet.Protein:
                            return String.Format("{0} AA", data.Length);
                        case Alphabet.DNA:
                        case Alphabet.RNA:
                        case Alphabet.Nucleotide:
                            return String.Format("{0} bp", data.Length);
                        default:
                            return String.Format("{0}", data.Length);
                    }
                }
            }

            public Alphabet Alphabet
            {
                get
                {
                    if (this is DnaSeq)
                        return Alphabet.DNA;
                    else if (this is ProteinSeq)
                        return Alphabet.Protein;
                    else if (this is NucleotideSeq)
                        return Alphabet.Nucleotide;
                    else
                        return Alphabet.Unknown;
                }
            }




            public bool IsNucleotide
            {
                get
                {
                    if (Alphabet == Alphabet.Nucleotide || Alphabet == Alphabet.DNA || Alphabet == Alphabet.RNA)
                        return true;
                    else
                        return false;
                }
            }

            #endregion


            //public string BaseChars
            //{
            //    get
            //    {
            //        if (alphabet == Alphabet.Protein)
            //            return Seq.ProteinChars;
            //        else if (alphabet == Alphabet.DNA)
            //            return Seq.DNAChars;
            //        else if (alphabet == Alphabet.RNA)
            //            return Seq.RNAChars;
            //        else
            //            throw new InvalidOperationException("BaseChars is available only for DNA, RNA or protein");
            //    }
            //}



            public bool HasAmbiguous
            {
                get
                {
                    if (_hasAmbiguous == null)
                    {
                        _hasAmbiguous = false;
                        foreach (char c in this)
                        {
                            if (!Seq.DNAChars.Contains(c))
                            {
                                _hasAmbiguous = true;
                                break;
                            }
                        }
                    }
                    return (bool)_hasAmbiguous;
                }
            }

            


            #region sequence utilities

            /// <summary>
            /// (Preferred) gap character 
            /// </summary>
            public const char GapChar = '-';
            /// <summary>
            /// Gap characters "-."
            /// </summary>
            public const string GapChars = "-.";
            /// <summary>
            /// Orderred DNA characters "acgt"
            /// </summary>
            public const string DNAChars = "acgt";
            /// <summary>
            /// DNA characters with an ambiguous character "acgtn"
            /// </summary>
            public const string DNAnChars = "acgtn";
            /// <summary>
            /// Orderred RNA characters "acgu"
            /// </summary>
            public const string RNAChars = "acgu";
            /// <summary>
            /// RNA characters with an ambiguous character "acgun"
            /// </summary>
            public const string RNAnChars = "acgun";
            /// <summary>
            /// Nucleotide character with ambiguous characters "acgturyn"
            /// </summary>
            public const string NucleotidenChars = "acgturyn";
            /// <summary>
            /// Orderred protien characters "ARNDCQEGHILKMFPSTWYV"
            /// </summary>
            public const string ProteinChars = "ARNDCQEGHILKMFPSTWYV";
            /// <summary>
            /// Orderred protien characters "ARNDCQEGHILKMFPSTWYVBZX*"
            /// </summary>
            public const string ProteinNChars = "ARNDCQEGHILKMFPSTWYVBZX*";
            /// <summary>
            /// Molecular weight of A,C,G,T respectively
            /// </summary>
            public static readonly double[] DNAMolWeight = { 313.21, 289.18, 329.21, 304.2 };

            /// <summary>
            /// Guess type of alphabet from the given sequence
            /// </summary>
            /// <param name="seq"></param>
            /// <returns></returns>
            public static Alphabet GuessAlphabet(string seq)
            {
                // If at least MIN_NUCLEO_PCT of the first CHAR_COUNT non-gap
                // letters belong to the nucleotide alphabet, guess nucleo.
                // Otherwise amino.
                const int CHAR_COUNT = 100;
                const int MIN_NUCLEO_PCT = 95;

                // we have to define seperately
                // because seq could be upper or lower case
                const string DNAChars = "AGCTNagctn";
                const string RNAChars = "AGCUNagcun";
                const string NucleotideChars = "ACGTURYNacgturyn";

                if (seq == null)
                    return Alphabet.Unknown;

                int uSeqCount = 1;
                int uColCount = seq.Length;
                if (0 == uSeqCount)
                    return Alphabet.Unknown;

                int uDNACount = 0;
                int uRNACount = 0;
                int uTotal = 0;
                int i = 0;
                foreach (char c in seq)
                {
                    int uSeqIndex = i / uColCount;
                    if (uSeqIndex >= uSeqCount)
                        break;
                    int uColIndex = i % uColCount;
                    ++i;
                    if (GapChars.Contains(c))
                        continue;
                    if (DNAChars.Contains(c))
                        ++uDNACount;
                    if (RNAChars.Contains(c))
                        ++uRNACount;
                    ++uTotal;
                    if (uTotal >= CHAR_COUNT)
                        break;
                }
                if (uTotal != 0 && ((uRNACount * 100) / uTotal) >= MIN_NUCLEO_PCT)
                    return Alphabet.RNA;
                if (uTotal != 0 && ((uDNACount * 100) / uTotal) >= MIN_NUCLEO_PCT)
                    return Alphabet.DNA;
                return Alphabet.Protein;
            }

            /// <summary>
            /// Returns the reverse strand of a DNA or RNA sequence. Calculates 
            /// the reverse strand 3' --> 5' of sequence DNA. SEQ is returned in the same format as DNA, 
            /// so if DNA is an integer sequence then so is SEQ.
            /// </summary>
            /// <param name="seq"></param>
            /// <returns></returns>
            static public string reverse(string seq)
            {
                StringBuilder sb = new StringBuilder(seq.Length);
                foreach (char c in seq)
                    sb.Insert(0, c);
                return sb.ToString();
            }

            #endregion

            #region UI utitlities
            public override string ToString()
            {
                return data;
            }

            public static implicit operator string(Seq seq)
            {
                return seq.data;
            }

            public static implicit operator Seq(string data)
            {
                return new Seq(data);
            }

            





            #endregion

            #region lazy catch properties
            /// <summary>
            /// Count of respective base accouding to <code>DNAChars</code> or <code>ProteinChars</code>
            /// </summary>
            protected int[] _countBases = null;

            /// <summary>
            /// Count of respective base accouding to <code>DNAnChar</code> or <code>ProteinnChars</code>
            /// </summary>
            protected int[] _countBaseNs = null;



            protected double _molWeight = Double.NaN;
            protected double _molWeightwn = Double.NaN;
            protected bool? _hasAmbiguous = null;
            


            #endregion

            /// <summary>
            /// Retrive a <code>Seq</code> from this instance giving <paramref name="start"/> and 
            /// length of string. Use negative length to get reverse sequence.
            /// </summary>
            /// <param name="start"></param>
            /// <param name="length">length of resulting sequence. can be negative</param>
            /// <returns></returns>
            public DnaSeq SubSeq(int start, int length)
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

            /// <summary>
            /// Retrive as substring from the sequence.
            /// </summary>
            /// <param name="start"></param>
            /// <param name="length">length of desire string. throw exception if negative length</param>
            /// <returns></returns>
            public string Substring(int start, int length)
            { 
                return data.Substring(start, length);
                
            }

            public static bool IsComplement(char p, char p2)
            {
                if ((p == 'a' & p2 == 't') |
                    (p == 't' & p2 == 'a') |
                    (p == 'g' & p2 == 'c') |
                    (p == 'c' & p2 == 'g'))
                    return true;
                else
                    return false;
            }
        }

        namespace GeneticCode
        {
            /// <summary>
            /// The Genetic Codes. 
            /// 
            /// http://www.ncbi.nlm.nih.gov/Taxonomy/Utils/wprintgc.cgi?mode=c
            /// </summary>
            public enum Codes
            {
                Standard = 1,
                [Description("Vertebrate Mitochondrial")]
                VertebrateMitochondrial = 2,
                [Description("Yeast Mitochondrial")]
                YeastMitochondrial = 3,
                [Description("Mold, Protozoan, and Coelente rate Mitochondrial and Mycoplasma/Spiroplasma")]
                MoldProtozoanCoelente = 4,
                [Description("Invertebrate Mitochondrial")]
                InvertebrateMitochondrial = 5,
                [Description("Ciliate, Dasycladacean and Hexamita Nuclear")]
                CiliateDasycladaceanHexamitaNuclear = 6,
                [Description("Echinoderm Mitochondrial")]
                EchinodermMitochondrial = 9,
                [Description("Euplotid Nuclear")]
                EuplotidNuclear = 10,
                [Description("Bacterial and Plant Plastid")]
                BacterialPlantPlastid = 11,
                [Description("Alternative Yeast Nuclear")]
                AlternativeYeastNuclear = 12,
                [Description("Ascidian Mitochondrial")]
                AscidianMitochondrial = 13,
                [Description("Flatworm Mitochondrial")]
                FlatwormMitochondrial = 14,
                [Description("Blepharisma Nuclear")]
                BlepharismaNuclear = 15,
                [Description("Chlorophycean Mitochondrial")]
                ChlorophyceanMitochondrial = 16,
                [Description("Trematode Mitochondrial")]
                TrematodeMitochondrial = 21,
                [Description("Scenedesmus Obliquus Mitochondrial")]
                ScenedesmusObliquusMitochondrial = 22,
                [Description("Thraustochytrium Mitochondrial")]
                ThraustochytriumMitochondrial = 23
            }

            public enum Frames
            {
                [Description("Frame 1")]
                Frame1 = 1,
                [Description("Frame 2")]
                Frame2 = 2,
                [Description("Frame 3")]
                Frame3 = 3,
            }




            /// <summary>
            /// Genetic code for translating nucleotide codons to amino acids.
            /// </summary>
            static public class GeneticCode
            {
                public readonly static Dictionary<string, char> GeneticCode_Standard;
                public readonly static Dictionary<string, char> GeneticCode_VertebrateMitochondrial;
                public readonly static Dictionary<string, char> GeneticCode_YeastMitochondrial;
                public readonly static Dictionary<string, char> GeneticCode_MoldProtozoanCoelente;
                public readonly static Dictionary<string, char> GeneticCode_InvertebrateMitochondrial;
                public readonly static Dictionary<string, char> GeneticCode_CiliateDasycladaceanHexamitaNuclear;
                public readonly static Dictionary<string, char> GeneticCode_EchinodermMitochondrial;
                public readonly static Dictionary<string, char> GeneticCode_EuplotidNuclear;
                public readonly static Dictionary<string, char> GeneticCode_BacterialPlantPlastid;
                public readonly static Dictionary<string, char> GeneticCode_AlternativeYeastNuclear;
                public readonly static Dictionary<string, char> GeneticCode_AscidianMitochondrial;
                public readonly static Dictionary<string, char> GeneticCode_FlatwormMitochondrial;
                public readonly static Dictionary<string, char> GeneticCode_BlepharismaNuclear;
                public readonly static Dictionary<string, char> GeneticCode_ChlorophyceanMitochondrial;
                public readonly static Dictionary<string, char> GeneticCode_TrematodeMitochondrial;
                public readonly static Dictionary<string, char> GeneticCode_ScenedesmusObliquusMitochondrial;
                public readonly static Dictionary<string, char> GeneticCode_ThraustochytriumMitochondrial;

                public readonly static string[] GeneticCode_Starts_Standard = { "ATG", "CTG", "TTG" };
                public readonly static string[] GeneticCode_Starts_VertebrateMitochondrial = { "ATG", "ATA", "ATC", "ATT", "GTG" };
                public readonly static string[] GeneticCode_Starts_YeastMitochondrial = { "ATG", "ATA" };
                public readonly static string[] GeneticCode_Starts_MoldProtozoanCoelente = { "ATG", "ATA", "ATC", "ATT", "CTG", "GTG", "TTA", "TTG" };
                public readonly static string[] GeneticCode_Starts_InvertebrateMitochondrial = { "ATG", "ATA", "ATC", "ATT", "GTG", "TTG" };
                public readonly static string[] GeneticCode_Starts_CiliateDasycladaceanHexamitaNuclear = { "ATG" };
                public readonly static string[] GeneticCode_Starts_EchinodermMitochondrial = { "ATG", "GTG" };
                public readonly static string[] GeneticCode_Starts_EuplotidNuclear = { "ATG" };
                public readonly static string[] GeneticCode_Starts_BacterialPlantPlastid = { "ATG", "ATA", "ATC", "ATT", "CTG", "GTG", "TTG" };
                public readonly static string[] GeneticCode_Starts_AlternativeYeastNuclear = { "ATG", "CTG" };
                public readonly static string[] GeneticCode_Starts_AscidianMitochondrial = { "ATG" };
                public readonly static string[] GeneticCode_Starts_FlatwormMitochondrial = { "ATG" };
                public readonly static string[] GeneticCode_Starts_BlepharismaNuclear = { "ATG" };
                public readonly static string[] GeneticCode_Starts_ChlorophyceanMitochondrial = { "ATG" };
                public readonly static string[] GeneticCode_Starts_TrematodeMitochondrial = { "ATG", "GTG" };
                public readonly static string[] GeneticCode_Starts_ScenedesmusObliquusMitochondrial = { "ATG" };
                public readonly static string[] GeneticCode_Starts_ThraustochytriumMitochondrial = { "ATG", "ATT", "GTG" };

                /// <summary>
                /// Convert <code>Codes</code> enumeration to GeneticCode dictionary.
                /// </summary>
                public readonly static Dictionary<int, Dictionary<string, char>> Code2GeneticCode;
                /// <summary>
                /// Convert <code>Codes</code> enumeration to start codon dictionary.
                /// </summary>
                public readonly static Dictionary<int, string[]> Code2GeneticCode_Starts;

                public static string nt2aa(string nt) { return nt2aa(nt, Frames.Frame1, Codes.Standard, true); }

                public static string nt2aa(string nt, Frames frame) { return nt2aa(nt, frame, Codes.Standard, true); }

                public static string nt2aa(string nt, Frames frame, Codes code) { return nt2aa(nt, frame, code, true); }


                static GeneticCode()
                {
                    GeneticCode_Standard = new Dictionary<string, char>(65);
                    GeneticCode_Standard.Add("AAA", 'K');
                    GeneticCode_Standard.Add("AAC", 'N');
                    GeneticCode_Standard.Add("AAG", 'K');
                    GeneticCode_Standard.Add("AAT", 'N');
                    GeneticCode_Standard.Add("ACA", 'T');
                    GeneticCode_Standard.Add("ACC", 'T');
                    GeneticCode_Standard.Add("ACG", 'T');
                    GeneticCode_Standard.Add("ACT", 'T');
                    GeneticCode_Standard.Add("AGA", 'R');
                    GeneticCode_Standard.Add("AGC", 'S');
                    GeneticCode_Standard.Add("AGG", 'R');
                    GeneticCode_Standard.Add("AGT", 'S');
                    GeneticCode_Standard.Add("ATA", 'I');
                    GeneticCode_Standard.Add("ATC", 'I');
                    GeneticCode_Standard.Add("ATG", 'M');
                    GeneticCode_Standard.Add("ATT", 'I');
                    GeneticCode_Standard.Add("CAA", 'Q');
                    GeneticCode_Standard.Add("CAC", 'H');
                    GeneticCode_Standard.Add("CAG", 'Q');
                    GeneticCode_Standard.Add("CAT", 'H');
                    GeneticCode_Standard.Add("CCA", 'P');
                    GeneticCode_Standard.Add("CCC", 'P');
                    GeneticCode_Standard.Add("CCG", 'P');
                    GeneticCode_Standard.Add("CCT", 'P');
                    GeneticCode_Standard.Add("CGA", 'R');
                    GeneticCode_Standard.Add("CGC", 'R');
                    GeneticCode_Standard.Add("CGG", 'R');
                    GeneticCode_Standard.Add("CGT", 'R');
                    GeneticCode_Standard.Add("CTA", 'L');
                    GeneticCode_Standard.Add("CTC", 'L');
                    GeneticCode_Standard.Add("CTG", 'L');
                    GeneticCode_Standard.Add("CTT", 'L');
                    GeneticCode_Standard.Add("GAA", 'E');
                    GeneticCode_Standard.Add("GAC", 'D');
                    GeneticCode_Standard.Add("GAG", 'E');
                    GeneticCode_Standard.Add("GAT", 'D');
                    GeneticCode_Standard.Add("GCA", 'A');
                    GeneticCode_Standard.Add("GCC", 'A');
                    GeneticCode_Standard.Add("GCG", 'A');
                    GeneticCode_Standard.Add("GCT", 'A');
                    GeneticCode_Standard.Add("GGA", 'G');
                    GeneticCode_Standard.Add("GGC", 'G');
                    GeneticCode_Standard.Add("GGG", 'G');
                    GeneticCode_Standard.Add("GGT", 'G');
                    GeneticCode_Standard.Add("GTA", 'V');
                    GeneticCode_Standard.Add("GTC", 'V');
                    GeneticCode_Standard.Add("GTG", 'V');
                    GeneticCode_Standard.Add("GTT", 'V');
                    GeneticCode_Standard.Add("TAA", '*');
                    GeneticCode_Standard.Add("TAC", 'Y');
                    GeneticCode_Standard.Add("TAG", '*');
                    GeneticCode_Standard.Add("TAT", 'Y');
                    GeneticCode_Standard.Add("TCA", 'S');
                    GeneticCode_Standard.Add("TCC", 'S');
                    GeneticCode_Standard.Add("TCG", 'S');
                    GeneticCode_Standard.Add("TCT", 'S');
                    GeneticCode_Standard.Add("TGA", '*');
                    GeneticCode_Standard.Add("TGC", 'C');
                    GeneticCode_Standard.Add("TGG", 'W');
                    GeneticCode_Standard.Add("TGT", 'C');
                    GeneticCode_Standard.Add("TTA", 'L');
                    GeneticCode_Standard.Add("TTC", 'F');
                    GeneticCode_Standard.Add("TTG", 'L');
                    GeneticCode_Standard.Add("TTT", 'F');
                    GeneticCode_Standard.Add("---", '-');

                    GeneticCode_VertebrateMitochondrial = new Dictionary<string, char>(65);
                    GeneticCode_VertebrateMitochondrial.Add("AAA", 'K');
                    GeneticCode_VertebrateMitochondrial.Add("AAC", 'N');
                    GeneticCode_VertebrateMitochondrial.Add("AAG", 'K');
                    GeneticCode_VertebrateMitochondrial.Add("AAT", 'N');
                    GeneticCode_VertebrateMitochondrial.Add("ACA", 'T');
                    GeneticCode_VertebrateMitochondrial.Add("ACC", 'T');
                    GeneticCode_VertebrateMitochondrial.Add("ACG", 'T');
                    GeneticCode_VertebrateMitochondrial.Add("ACT", 'T');
                    GeneticCode_VertebrateMitochondrial.Add("AGA", '*');
                    GeneticCode_VertebrateMitochondrial.Add("AGC", 'S');
                    GeneticCode_VertebrateMitochondrial.Add("AGG", '*');
                    GeneticCode_VertebrateMitochondrial.Add("AGT", 'S');
                    GeneticCode_VertebrateMitochondrial.Add("ATA", 'M');
                    GeneticCode_VertebrateMitochondrial.Add("ATC", 'I');
                    GeneticCode_VertebrateMitochondrial.Add("ATG", 'M');
                    GeneticCode_VertebrateMitochondrial.Add("ATT", 'I');
                    GeneticCode_VertebrateMitochondrial.Add("CAA", 'Q');
                    GeneticCode_VertebrateMitochondrial.Add("CAC", 'H');
                    GeneticCode_VertebrateMitochondrial.Add("CAG", 'Q');
                    GeneticCode_VertebrateMitochondrial.Add("CAT", 'H');
                    GeneticCode_VertebrateMitochondrial.Add("CCA", 'P');
                    GeneticCode_VertebrateMitochondrial.Add("CCC", 'P');
                    GeneticCode_VertebrateMitochondrial.Add("CCG", 'P');
                    GeneticCode_VertebrateMitochondrial.Add("CCT", 'P');
                    GeneticCode_VertebrateMitochondrial.Add("CGA", 'R');
                    GeneticCode_VertebrateMitochondrial.Add("CGC", 'R');
                    GeneticCode_VertebrateMitochondrial.Add("CGG", 'R');
                    GeneticCode_VertebrateMitochondrial.Add("CGT", 'R');
                    GeneticCode_VertebrateMitochondrial.Add("CTA", 'L');
                    GeneticCode_VertebrateMitochondrial.Add("CTC", 'L');
                    GeneticCode_VertebrateMitochondrial.Add("CTG", 'L');
                    GeneticCode_VertebrateMitochondrial.Add("CTT", 'L');
                    GeneticCode_VertebrateMitochondrial.Add("GAA", 'E');
                    GeneticCode_VertebrateMitochondrial.Add("GAC", 'D');
                    GeneticCode_VertebrateMitochondrial.Add("GAG", 'E');
                    GeneticCode_VertebrateMitochondrial.Add("GAT", 'D');
                    GeneticCode_VertebrateMitochondrial.Add("GCA", 'A');
                    GeneticCode_VertebrateMitochondrial.Add("GCC", 'A');
                    GeneticCode_VertebrateMitochondrial.Add("GCG", 'A');
                    GeneticCode_VertebrateMitochondrial.Add("GCT", 'A');
                    GeneticCode_VertebrateMitochondrial.Add("GGA", 'G');
                    GeneticCode_VertebrateMitochondrial.Add("GGC", 'G');
                    GeneticCode_VertebrateMitochondrial.Add("GGG", 'G');
                    GeneticCode_VertebrateMitochondrial.Add("GGT", 'G');
                    GeneticCode_VertebrateMitochondrial.Add("GTA", 'V');
                    GeneticCode_VertebrateMitochondrial.Add("GTC", 'V');
                    GeneticCode_VertebrateMitochondrial.Add("GTG", 'V');
                    GeneticCode_VertebrateMitochondrial.Add("GTT", 'V');
                    GeneticCode_VertebrateMitochondrial.Add("TAA", '*');
                    GeneticCode_VertebrateMitochondrial.Add("TAC", 'Y');
                    GeneticCode_VertebrateMitochondrial.Add("TAG", '*');
                    GeneticCode_VertebrateMitochondrial.Add("TAT", 'Y');
                    GeneticCode_VertebrateMitochondrial.Add("TCA", 'S');
                    GeneticCode_VertebrateMitochondrial.Add("TCC", 'S');
                    GeneticCode_VertebrateMitochondrial.Add("TCG", 'S');
                    GeneticCode_VertebrateMitochondrial.Add("TCT", 'S');
                    GeneticCode_VertebrateMitochondrial.Add("TGA", 'W');
                    GeneticCode_VertebrateMitochondrial.Add("TGC", 'C');
                    GeneticCode_VertebrateMitochondrial.Add("TGG", 'W');
                    GeneticCode_VertebrateMitochondrial.Add("TGT", 'C');
                    GeneticCode_VertebrateMitochondrial.Add("TTA", 'L');
                    GeneticCode_VertebrateMitochondrial.Add("TTC", 'F');
                    GeneticCode_VertebrateMitochondrial.Add("TTG", 'L');
                    GeneticCode_VertebrateMitochondrial.Add("TTT", 'F');
                    GeneticCode_VertebrateMitochondrial.Add("---", '-');

                    GeneticCode_YeastMitochondrial = new Dictionary<string, char>(64);
                    GeneticCode_YeastMitochondrial.Add("AAA", 'K');
                    GeneticCode_YeastMitochondrial.Add("AAC", 'N');
                    GeneticCode_YeastMitochondrial.Add("AAG", 'K');
                    GeneticCode_YeastMitochondrial.Add("AAT", 'N');
                    GeneticCode_YeastMitochondrial.Add("ACA", 'T');
                    GeneticCode_YeastMitochondrial.Add("ACC", 'T');
                    GeneticCode_YeastMitochondrial.Add("ACG", 'T');
                    GeneticCode_YeastMitochondrial.Add("ACT", 'T');
                    GeneticCode_YeastMitochondrial.Add("AGA", 'R');
                    GeneticCode_YeastMitochondrial.Add("AGC", 'S');
                    GeneticCode_YeastMitochondrial.Add("AGG", 'R');
                    GeneticCode_YeastMitochondrial.Add("AGT", 'S');
                    GeneticCode_YeastMitochondrial.Add("ATA", 'M');
                    GeneticCode_YeastMitochondrial.Add("ATC", 'I');
                    GeneticCode_YeastMitochondrial.Add("ATG", 'M');
                    GeneticCode_YeastMitochondrial.Add("ATT", 'I');
                    GeneticCode_YeastMitochondrial.Add("CAA", 'Q');
                    GeneticCode_YeastMitochondrial.Add("CAC", 'H');
                    GeneticCode_YeastMitochondrial.Add("CAG", 'Q');
                    GeneticCode_YeastMitochondrial.Add("CAT", 'H');
                    GeneticCode_YeastMitochondrial.Add("CCA", 'P');
                    GeneticCode_YeastMitochondrial.Add("CCC", 'P');
                    GeneticCode_YeastMitochondrial.Add("CCG", 'P');
                    GeneticCode_YeastMitochondrial.Add("CCT", 'P');
                    GeneticCode_YeastMitochondrial.Add("CGA", 'R');
                    GeneticCode_YeastMitochondrial.Add("CGC", 'R');
                    GeneticCode_YeastMitochondrial.Add("CGG", 'R');
                    GeneticCode_YeastMitochondrial.Add("CGT", 'R');
                    GeneticCode_YeastMitochondrial.Add("CTA", 'T');
                    GeneticCode_YeastMitochondrial.Add("CTC", 'T');
                    GeneticCode_YeastMitochondrial.Add("CTG", 'T');
                    GeneticCode_YeastMitochondrial.Add("CTT", 'T');
                    GeneticCode_YeastMitochondrial.Add("GAA", 'E');
                    GeneticCode_YeastMitochondrial.Add("GAC", 'D');
                    GeneticCode_YeastMitochondrial.Add("GAG", 'E');
                    GeneticCode_YeastMitochondrial.Add("GAT", 'D');
                    GeneticCode_YeastMitochondrial.Add("GCA", 'A');
                    GeneticCode_YeastMitochondrial.Add("GCC", 'A');
                    GeneticCode_YeastMitochondrial.Add("GCG", 'A');
                    GeneticCode_YeastMitochondrial.Add("GCT", 'A');
                    GeneticCode_YeastMitochondrial.Add("GGA", 'G');
                    GeneticCode_YeastMitochondrial.Add("GGC", 'G');
                    GeneticCode_YeastMitochondrial.Add("GGG", 'G');
                    GeneticCode_YeastMitochondrial.Add("GGT", 'G');
                    GeneticCode_YeastMitochondrial.Add("GTA", 'V');
                    GeneticCode_YeastMitochondrial.Add("GTC", 'V');
                    GeneticCode_YeastMitochondrial.Add("GTG", 'V');
                    GeneticCode_YeastMitochondrial.Add("GTT", 'V');
                    GeneticCode_YeastMitochondrial.Add("TAA", '*');
                    GeneticCode_YeastMitochondrial.Add("TAC", 'Y');
                    GeneticCode_YeastMitochondrial.Add("TAG", '*');
                    GeneticCode_YeastMitochondrial.Add("TAT", 'Y');
                    GeneticCode_YeastMitochondrial.Add("TCA", 'S');
                    GeneticCode_YeastMitochondrial.Add("TCC", 'S');
                    GeneticCode_YeastMitochondrial.Add("TCG", 'S');
                    GeneticCode_YeastMitochondrial.Add("TCT", 'S');
                    GeneticCode_YeastMitochondrial.Add("TGA", 'W');
                    GeneticCode_YeastMitochondrial.Add("TGC", 'C');
                    GeneticCode_YeastMitochondrial.Add("TGG", 'W');
                    GeneticCode_YeastMitochondrial.Add("TGT", 'C');
                    GeneticCode_YeastMitochondrial.Add("TTA", 'L');
                    GeneticCode_YeastMitochondrial.Add("TTC", 'F');
                    GeneticCode_YeastMitochondrial.Add("TTG", 'L');
                    GeneticCode_YeastMitochondrial.Add("TTT", 'F');
                    GeneticCode_YeastMitochondrial.Add("---", '-');

                    GeneticCode_MoldProtozoanCoelente = new Dictionary<string, char>(65);
                    GeneticCode_MoldProtozoanCoelente.Add("AAA", 'K');
                    GeneticCode_MoldProtozoanCoelente.Add("AAC", 'N');
                    GeneticCode_MoldProtozoanCoelente.Add("AAG", 'K');
                    GeneticCode_MoldProtozoanCoelente.Add("AAT", 'N');
                    GeneticCode_MoldProtozoanCoelente.Add("ACA", 'T');
                    GeneticCode_MoldProtozoanCoelente.Add("ACC", 'T');
                    GeneticCode_MoldProtozoanCoelente.Add("ACG", 'T');
                    GeneticCode_MoldProtozoanCoelente.Add("ACT", 'T');
                    GeneticCode_MoldProtozoanCoelente.Add("AGA", 'R');
                    GeneticCode_MoldProtozoanCoelente.Add("AGC", 'S');
                    GeneticCode_MoldProtozoanCoelente.Add("AGG", 'R');
                    GeneticCode_MoldProtozoanCoelente.Add("AGT", 'S');
                    GeneticCode_MoldProtozoanCoelente.Add("ATA", 'I');
                    GeneticCode_MoldProtozoanCoelente.Add("ATC", 'I');
                    GeneticCode_MoldProtozoanCoelente.Add("ATG", 'M');
                    GeneticCode_MoldProtozoanCoelente.Add("ATT", 'I');
                    GeneticCode_MoldProtozoanCoelente.Add("CAA", 'Q');
                    GeneticCode_MoldProtozoanCoelente.Add("CAC", 'H');
                    GeneticCode_MoldProtozoanCoelente.Add("CAG", 'Q');
                    GeneticCode_MoldProtozoanCoelente.Add("CAT", 'H');
                    GeneticCode_MoldProtozoanCoelente.Add("CCA", 'P');
                    GeneticCode_MoldProtozoanCoelente.Add("CCC", 'P');
                    GeneticCode_MoldProtozoanCoelente.Add("CCG", 'P');
                    GeneticCode_MoldProtozoanCoelente.Add("CCT", 'P');
                    GeneticCode_MoldProtozoanCoelente.Add("CGA", 'R');
                    GeneticCode_MoldProtozoanCoelente.Add("CGC", 'R');
                    GeneticCode_MoldProtozoanCoelente.Add("CGG", 'R');
                    GeneticCode_MoldProtozoanCoelente.Add("CGT", 'R');
                    GeneticCode_MoldProtozoanCoelente.Add("CTA", 'L');
                    GeneticCode_MoldProtozoanCoelente.Add("CTC", 'L');
                    GeneticCode_MoldProtozoanCoelente.Add("CTG", 'L');
                    GeneticCode_MoldProtozoanCoelente.Add("CTT", 'L');
                    GeneticCode_MoldProtozoanCoelente.Add("GAA", 'E');
                    GeneticCode_MoldProtozoanCoelente.Add("GAC", 'D');
                    GeneticCode_MoldProtozoanCoelente.Add("GAG", 'E');
                    GeneticCode_MoldProtozoanCoelente.Add("GAT", 'D');
                    GeneticCode_MoldProtozoanCoelente.Add("GCA", 'A');
                    GeneticCode_MoldProtozoanCoelente.Add("GCC", 'A');
                    GeneticCode_MoldProtozoanCoelente.Add("GCG", 'A');
                    GeneticCode_MoldProtozoanCoelente.Add("GCT", 'A');
                    GeneticCode_MoldProtozoanCoelente.Add("GGA", 'G');
                    GeneticCode_MoldProtozoanCoelente.Add("GGC", 'G');
                    GeneticCode_MoldProtozoanCoelente.Add("GGG", 'G');
                    GeneticCode_MoldProtozoanCoelente.Add("GGT", 'G');
                    GeneticCode_MoldProtozoanCoelente.Add("GTA", 'V');
                    GeneticCode_MoldProtozoanCoelente.Add("GTC", 'V');
                    GeneticCode_MoldProtozoanCoelente.Add("GTG", 'V');
                    GeneticCode_MoldProtozoanCoelente.Add("GTT", 'V');
                    GeneticCode_MoldProtozoanCoelente.Add("TAA", '*');
                    GeneticCode_MoldProtozoanCoelente.Add("TAC", 'Y');
                    GeneticCode_MoldProtozoanCoelente.Add("TAG", '*');
                    GeneticCode_MoldProtozoanCoelente.Add("TAT", 'Y');
                    GeneticCode_MoldProtozoanCoelente.Add("TCA", 'S');
                    GeneticCode_MoldProtozoanCoelente.Add("TCC", 'S');
                    GeneticCode_MoldProtozoanCoelente.Add("TCG", 'S');
                    GeneticCode_MoldProtozoanCoelente.Add("TCT", 'S');
                    GeneticCode_MoldProtozoanCoelente.Add("TGA", 'W');
                    GeneticCode_MoldProtozoanCoelente.Add("TGC", 'C');
                    GeneticCode_MoldProtozoanCoelente.Add("TGG", 'W');
                    GeneticCode_MoldProtozoanCoelente.Add("TGT", 'C');
                    GeneticCode_MoldProtozoanCoelente.Add("TTA", 'L');
                    GeneticCode_MoldProtozoanCoelente.Add("TTC", 'F');
                    GeneticCode_MoldProtozoanCoelente.Add("TTG", 'L');
                    GeneticCode_MoldProtozoanCoelente.Add("TTT", 'F');
                    GeneticCode_MoldProtozoanCoelente.Add("---", '-');

                    GeneticCode_InvertebrateMitochondrial = new Dictionary<string, char>(65);
                    GeneticCode_InvertebrateMitochondrial.Add("AAA", 'K');
                    GeneticCode_InvertebrateMitochondrial.Add("AAC", 'N');
                    GeneticCode_InvertebrateMitochondrial.Add("AAG", 'K');
                    GeneticCode_InvertebrateMitochondrial.Add("AAT", 'N');
                    GeneticCode_InvertebrateMitochondrial.Add("ACA", 'T');
                    GeneticCode_InvertebrateMitochondrial.Add("ACC", 'T');
                    GeneticCode_InvertebrateMitochondrial.Add("ACG", 'T');
                    GeneticCode_InvertebrateMitochondrial.Add("ACT", 'T');
                    GeneticCode_InvertebrateMitochondrial.Add("AGA", 'S');
                    GeneticCode_InvertebrateMitochondrial.Add("AGC", 'S');
                    GeneticCode_InvertebrateMitochondrial.Add("AGG", 'S');
                    GeneticCode_InvertebrateMitochondrial.Add("AGT", 'S');
                    GeneticCode_InvertebrateMitochondrial.Add("ATA", 'M');
                    GeneticCode_InvertebrateMitochondrial.Add("ATC", 'I');
                    GeneticCode_InvertebrateMitochondrial.Add("ATG", 'M');
                    GeneticCode_InvertebrateMitochondrial.Add("ATT", 'I');
                    GeneticCode_InvertebrateMitochondrial.Add("CAA", 'Q');
                    GeneticCode_InvertebrateMitochondrial.Add("CAC", 'H');
                    GeneticCode_InvertebrateMitochondrial.Add("CAG", 'Q');
                    GeneticCode_InvertebrateMitochondrial.Add("CAT", 'H');
                    GeneticCode_InvertebrateMitochondrial.Add("CCA", 'P');
                    GeneticCode_InvertebrateMitochondrial.Add("CCC", 'P');
                    GeneticCode_InvertebrateMitochondrial.Add("CCG", 'P');
                    GeneticCode_InvertebrateMitochondrial.Add("CCT", 'P');
                    GeneticCode_InvertebrateMitochondrial.Add("CGA", 'R');
                    GeneticCode_InvertebrateMitochondrial.Add("CGC", 'R');
                    GeneticCode_InvertebrateMitochondrial.Add("CGG", 'R');
                    GeneticCode_InvertebrateMitochondrial.Add("CGT", 'R');
                    GeneticCode_InvertebrateMitochondrial.Add("CTA", 'L');
                    GeneticCode_InvertebrateMitochondrial.Add("CTC", 'L');
                    GeneticCode_InvertebrateMitochondrial.Add("CTG", 'L');
                    GeneticCode_InvertebrateMitochondrial.Add("CTT", 'L');
                    GeneticCode_InvertebrateMitochondrial.Add("GAA", 'E');
                    GeneticCode_InvertebrateMitochondrial.Add("GAC", 'D');
                    GeneticCode_InvertebrateMitochondrial.Add("GAG", 'E');
                    GeneticCode_InvertebrateMitochondrial.Add("GAT", 'D');
                    GeneticCode_InvertebrateMitochondrial.Add("GCA", 'A');
                    GeneticCode_InvertebrateMitochondrial.Add("GCC", 'A');
                    GeneticCode_InvertebrateMitochondrial.Add("GCG", 'A');
                    GeneticCode_InvertebrateMitochondrial.Add("GCT", 'A');
                    GeneticCode_InvertebrateMitochondrial.Add("GGA", 'G');
                    GeneticCode_InvertebrateMitochondrial.Add("GGC", 'G');
                    GeneticCode_InvertebrateMitochondrial.Add("GGG", 'G');
                    GeneticCode_InvertebrateMitochondrial.Add("GGT", 'G');
                    GeneticCode_InvertebrateMitochondrial.Add("GTA", 'V');
                    GeneticCode_InvertebrateMitochondrial.Add("GTC", 'V');
                    GeneticCode_InvertebrateMitochondrial.Add("GTG", 'V');
                    GeneticCode_InvertebrateMitochondrial.Add("GTT", 'V');
                    GeneticCode_InvertebrateMitochondrial.Add("TAA", '*');
                    GeneticCode_InvertebrateMitochondrial.Add("TAC", 'Y');
                    GeneticCode_InvertebrateMitochondrial.Add("TAG", '*');
                    GeneticCode_InvertebrateMitochondrial.Add("TAT", 'Y');
                    GeneticCode_InvertebrateMitochondrial.Add("TCA", 'S');
                    GeneticCode_InvertebrateMitochondrial.Add("TCC", 'S');
                    GeneticCode_InvertebrateMitochondrial.Add("TCG", 'S');
                    GeneticCode_InvertebrateMitochondrial.Add("TCT", 'S');
                    GeneticCode_InvertebrateMitochondrial.Add("TGA", 'W');
                    GeneticCode_InvertebrateMitochondrial.Add("TGC", 'C');
                    GeneticCode_InvertebrateMitochondrial.Add("TGG", 'W');
                    GeneticCode_InvertebrateMitochondrial.Add("TGT", 'C');
                    GeneticCode_InvertebrateMitochondrial.Add("TTA", 'L');
                    GeneticCode_InvertebrateMitochondrial.Add("TTC", 'F');
                    GeneticCode_InvertebrateMitochondrial.Add("TTG", 'L');
                    GeneticCode_InvertebrateMitochondrial.Add("TTT", 'F');
                    GeneticCode_InvertebrateMitochondrial.Add("---", '-');

                    GeneticCode_CiliateDasycladaceanHexamitaNuclear = new Dictionary<string, char>(65);
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("AAA", 'K');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("AAC", 'N');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("AAG", 'K');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("AAT", 'N');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("ACA", 'T');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("ACC", 'T');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("ACG", 'T');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("ACT", 'T');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("AGA", 'R');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("AGC", 'S');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("AGG", 'R');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("AGT", 'S');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("ATA", 'I');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("ATC", 'I');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("ATG", 'M');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("ATT", 'I');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CAA", 'Q');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CAC", 'H');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CAG", 'Q');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CAT", 'H');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CCA", 'P');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CCC", 'P');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CCG", 'P');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CCT", 'P');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CGA", 'R');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CGC", 'R');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CGG", 'R');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CGT", 'R');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CTA", 'L');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CTC", 'L');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CTG", 'L');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("CTT", 'L');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GAA", 'E');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GAC", 'D');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GAG", 'E');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GAT", 'D');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GCA", 'A');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GCC", 'A');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GCG", 'A');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GCT", 'A');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GGA", 'G');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GGC", 'G');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GGG", 'G');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GGT", 'G');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GTA", 'V');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GTC", 'V');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GTG", 'V');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("GTT", 'V');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TAA", 'Q');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TAC", 'Y');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TAG", 'Q');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TAT", 'Y');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TCA", 'S');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TCC", 'S');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TCG", 'S');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TCT", 'S');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TGA", '*');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TGC", 'C');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TGG", 'W');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TGT", 'C');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TTA", 'L');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TTC", 'F');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TTG", 'L');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("TTT", 'F');
                    GeneticCode_CiliateDasycladaceanHexamitaNuclear.Add("---", '-');

                    GeneticCode_EchinodermMitochondrial = new Dictionary<string, char>(65);
                    GeneticCode_EchinodermMitochondrial.Add("AAA", 'N');
                    GeneticCode_EchinodermMitochondrial.Add("AAC", 'N');
                    GeneticCode_EchinodermMitochondrial.Add("AAG", 'K');
                    GeneticCode_EchinodermMitochondrial.Add("AAT", 'N');
                    GeneticCode_EchinodermMitochondrial.Add("ACA", 'T');
                    GeneticCode_EchinodermMitochondrial.Add("ACC", 'T');
                    GeneticCode_EchinodermMitochondrial.Add("ACG", 'T');
                    GeneticCode_EchinodermMitochondrial.Add("ACT", 'T');
                    GeneticCode_EchinodermMitochondrial.Add("AGA", 'S');
                    GeneticCode_EchinodermMitochondrial.Add("AGC", 'S');
                    GeneticCode_EchinodermMitochondrial.Add("AGG", 'S');
                    GeneticCode_EchinodermMitochondrial.Add("AGT", 'S');
                    GeneticCode_EchinodermMitochondrial.Add("ATA", 'I');
                    GeneticCode_EchinodermMitochondrial.Add("ATC", 'I');
                    GeneticCode_EchinodermMitochondrial.Add("ATG", 'M');
                    GeneticCode_EchinodermMitochondrial.Add("ATT", 'I');
                    GeneticCode_EchinodermMitochondrial.Add("CAA", 'Q');
                    GeneticCode_EchinodermMitochondrial.Add("CAC", 'H');
                    GeneticCode_EchinodermMitochondrial.Add("CAG", 'Q');
                    GeneticCode_EchinodermMitochondrial.Add("CAT", 'H');
                    GeneticCode_EchinodermMitochondrial.Add("CCA", 'P');
                    GeneticCode_EchinodermMitochondrial.Add("CCC", 'P');
                    GeneticCode_EchinodermMitochondrial.Add("CCG", 'P');
                    GeneticCode_EchinodermMitochondrial.Add("CCT", 'P');
                    GeneticCode_EchinodermMitochondrial.Add("CGA", 'R');
                    GeneticCode_EchinodermMitochondrial.Add("CGC", 'R');
                    GeneticCode_EchinodermMitochondrial.Add("CGG", 'R');
                    GeneticCode_EchinodermMitochondrial.Add("CGT", 'R');
                    GeneticCode_EchinodermMitochondrial.Add("CTA", 'L');
                    GeneticCode_EchinodermMitochondrial.Add("CTC", 'L');
                    GeneticCode_EchinodermMitochondrial.Add("CTG", 'L');
                    GeneticCode_EchinodermMitochondrial.Add("CTT", 'L');
                    GeneticCode_EchinodermMitochondrial.Add("GAA", 'E');
                    GeneticCode_EchinodermMitochondrial.Add("GAC", 'D');
                    GeneticCode_EchinodermMitochondrial.Add("GAG", 'E');
                    GeneticCode_EchinodermMitochondrial.Add("GAT", 'D');
                    GeneticCode_EchinodermMitochondrial.Add("GCA", 'A');
                    GeneticCode_EchinodermMitochondrial.Add("GCC", 'A');
                    GeneticCode_EchinodermMitochondrial.Add("GCG", 'A');
                    GeneticCode_EchinodermMitochondrial.Add("GCT", 'A');
                    GeneticCode_EchinodermMitochondrial.Add("GGA", 'G');
                    GeneticCode_EchinodermMitochondrial.Add("GGC", 'G');
                    GeneticCode_EchinodermMitochondrial.Add("GGG", 'G');
                    GeneticCode_EchinodermMitochondrial.Add("GGT", 'G');
                    GeneticCode_EchinodermMitochondrial.Add("GTA", 'V');
                    GeneticCode_EchinodermMitochondrial.Add("GTC", 'V');
                    GeneticCode_EchinodermMitochondrial.Add("GTG", 'V');
                    GeneticCode_EchinodermMitochondrial.Add("GTT", 'V');
                    GeneticCode_EchinodermMitochondrial.Add("TAA", '*');
                    GeneticCode_EchinodermMitochondrial.Add("TAC", 'Y');
                    GeneticCode_EchinodermMitochondrial.Add("TAG", '*');
                    GeneticCode_EchinodermMitochondrial.Add("TAT", 'Y');
                    GeneticCode_EchinodermMitochondrial.Add("TCA", 'S');
                    GeneticCode_EchinodermMitochondrial.Add("TCC", 'S');
                    GeneticCode_EchinodermMitochondrial.Add("TCG", 'S');
                    GeneticCode_EchinodermMitochondrial.Add("TCT", 'S');
                    GeneticCode_EchinodermMitochondrial.Add("TGA", 'W');
                    GeneticCode_EchinodermMitochondrial.Add("TGC", 'C');
                    GeneticCode_EchinodermMitochondrial.Add("TGG", 'W');
                    GeneticCode_EchinodermMitochondrial.Add("TGT", 'C');
                    GeneticCode_EchinodermMitochondrial.Add("TTA", 'L');
                    GeneticCode_EchinodermMitochondrial.Add("TTC", 'F');
                    GeneticCode_EchinodermMitochondrial.Add("TTG", 'L');
                    GeneticCode_EchinodermMitochondrial.Add("TTT", 'F');
                    GeneticCode_EchinodermMitochondrial.Add("---", '-');

                    GeneticCode_EuplotidNuclear = new Dictionary<string, char>(65);
                    GeneticCode_EuplotidNuclear.Add("AAA", 'K');
                    GeneticCode_EuplotidNuclear.Add("AAC", 'N');
                    GeneticCode_EuplotidNuclear.Add("AAG", 'K');
                    GeneticCode_EuplotidNuclear.Add("AAT", 'N');
                    GeneticCode_EuplotidNuclear.Add("ACA", 'T');
                    GeneticCode_EuplotidNuclear.Add("ACC", 'T');
                    GeneticCode_EuplotidNuclear.Add("ACG", 'T');
                    GeneticCode_EuplotidNuclear.Add("ACT", 'T');
                    GeneticCode_EuplotidNuclear.Add("AGA", 'R');
                    GeneticCode_EuplotidNuclear.Add("AGC", 'S');
                    GeneticCode_EuplotidNuclear.Add("AGG", 'R');
                    GeneticCode_EuplotidNuclear.Add("AGT", 'S');
                    GeneticCode_EuplotidNuclear.Add("ATA", 'I');
                    GeneticCode_EuplotidNuclear.Add("ATC", 'I');
                    GeneticCode_EuplotidNuclear.Add("ATG", 'M');
                    GeneticCode_EuplotidNuclear.Add("ATT", 'I');
                    GeneticCode_EuplotidNuclear.Add("CAA", 'Q');
                    GeneticCode_EuplotidNuclear.Add("CAC", 'H');
                    GeneticCode_EuplotidNuclear.Add("CAG", 'Q');
                    GeneticCode_EuplotidNuclear.Add("CAT", 'H');
                    GeneticCode_EuplotidNuclear.Add("CCA", 'P');
                    GeneticCode_EuplotidNuclear.Add("CCC", 'P');
                    GeneticCode_EuplotidNuclear.Add("CCG", 'P');
                    GeneticCode_EuplotidNuclear.Add("CCT", 'P');
                    GeneticCode_EuplotidNuclear.Add("CGA", 'R');
                    GeneticCode_EuplotidNuclear.Add("CGC", 'R');
                    GeneticCode_EuplotidNuclear.Add("CGG", 'R');
                    GeneticCode_EuplotidNuclear.Add("CGT", 'R');
                    GeneticCode_EuplotidNuclear.Add("CTA", 'L');
                    GeneticCode_EuplotidNuclear.Add("CTC", 'L');
                    GeneticCode_EuplotidNuclear.Add("CTG", 'L');
                    GeneticCode_EuplotidNuclear.Add("CTT", 'L');
                    GeneticCode_EuplotidNuclear.Add("GAA", 'E');
                    GeneticCode_EuplotidNuclear.Add("GAC", 'D');
                    GeneticCode_EuplotidNuclear.Add("GAG", 'E');
                    GeneticCode_EuplotidNuclear.Add("GAT", 'D');
                    GeneticCode_EuplotidNuclear.Add("GCA", 'A');
                    GeneticCode_EuplotidNuclear.Add("GCC", 'A');
                    GeneticCode_EuplotidNuclear.Add("GCG", 'A');
                    GeneticCode_EuplotidNuclear.Add("GCT", 'A');
                    GeneticCode_EuplotidNuclear.Add("GGA", 'G');
                    GeneticCode_EuplotidNuclear.Add("GGC", 'G');
                    GeneticCode_EuplotidNuclear.Add("GGG", 'G');
                    GeneticCode_EuplotidNuclear.Add("GGT", 'G');
                    GeneticCode_EuplotidNuclear.Add("GTA", 'V');
                    GeneticCode_EuplotidNuclear.Add("GTC", 'V');
                    GeneticCode_EuplotidNuclear.Add("GTG", 'V');
                    GeneticCode_EuplotidNuclear.Add("GTT", 'V');
                    GeneticCode_EuplotidNuclear.Add("TAA", '*');
                    GeneticCode_EuplotidNuclear.Add("TAC", 'Y');
                    GeneticCode_EuplotidNuclear.Add("TAG", '*');
                    GeneticCode_EuplotidNuclear.Add("TAT", 'Y');
                    GeneticCode_EuplotidNuclear.Add("TCA", 'S');
                    GeneticCode_EuplotidNuclear.Add("TCC", 'S');
                    GeneticCode_EuplotidNuclear.Add("TCG", 'S');
                    GeneticCode_EuplotidNuclear.Add("TCT", 'S');
                    GeneticCode_EuplotidNuclear.Add("TGA", 'C');
                    GeneticCode_EuplotidNuclear.Add("TGC", 'C');
                    GeneticCode_EuplotidNuclear.Add("TGG", 'W');
                    GeneticCode_EuplotidNuclear.Add("TGT", 'C');
                    GeneticCode_EuplotidNuclear.Add("TTA", 'L');
                    GeneticCode_EuplotidNuclear.Add("TTC", 'F');
                    GeneticCode_EuplotidNuclear.Add("TTG", 'L');
                    GeneticCode_EuplotidNuclear.Add("TTT", 'F');
                    GeneticCode_EuplotidNuclear.Add("---", '-');

                    GeneticCode_BacterialPlantPlastid = new Dictionary<string, char>(65);
                    GeneticCode_BacterialPlantPlastid.Add("AAA", 'K');
                    GeneticCode_BacterialPlantPlastid.Add("AAC", 'N');
                    GeneticCode_BacterialPlantPlastid.Add("AAG", 'K');
                    GeneticCode_BacterialPlantPlastid.Add("AAT", 'N');
                    GeneticCode_BacterialPlantPlastid.Add("ACA", 'T');
                    GeneticCode_BacterialPlantPlastid.Add("ACC", 'T');
                    GeneticCode_BacterialPlantPlastid.Add("ACG", 'T');
                    GeneticCode_BacterialPlantPlastid.Add("ACT", 'T');
                    GeneticCode_BacterialPlantPlastid.Add("AGA", 'R');
                    GeneticCode_BacterialPlantPlastid.Add("AGC", 'S');
                    GeneticCode_BacterialPlantPlastid.Add("AGG", 'R');
                    GeneticCode_BacterialPlantPlastid.Add("AGT", 'S');
                    GeneticCode_BacterialPlantPlastid.Add("ATA", 'I');
                    GeneticCode_BacterialPlantPlastid.Add("ATC", 'I');
                    GeneticCode_BacterialPlantPlastid.Add("ATG", 'M');
                    GeneticCode_BacterialPlantPlastid.Add("ATT", 'I');
                    GeneticCode_BacterialPlantPlastid.Add("CAA", 'Q');
                    GeneticCode_BacterialPlantPlastid.Add("CAC", 'H');
                    GeneticCode_BacterialPlantPlastid.Add("CAG", 'Q');
                    GeneticCode_BacterialPlantPlastid.Add("CAT", 'H');
                    GeneticCode_BacterialPlantPlastid.Add("CCA", 'P');
                    GeneticCode_BacterialPlantPlastid.Add("CCC", 'P');
                    GeneticCode_BacterialPlantPlastid.Add("CCG", 'P');
                    GeneticCode_BacterialPlantPlastid.Add("CCT", 'P');
                    GeneticCode_BacterialPlantPlastid.Add("CGA", 'R');
                    GeneticCode_BacterialPlantPlastid.Add("CGC", 'R');
                    GeneticCode_BacterialPlantPlastid.Add("CGG", 'R');
                    GeneticCode_BacterialPlantPlastid.Add("CGT", 'R');
                    GeneticCode_BacterialPlantPlastid.Add("CTA", 'L');
                    GeneticCode_BacterialPlantPlastid.Add("CTC", 'L');
                    GeneticCode_BacterialPlantPlastid.Add("CTG", 'L');
                    GeneticCode_BacterialPlantPlastid.Add("CTT", 'L');
                    GeneticCode_BacterialPlantPlastid.Add("GAA", 'E');
                    GeneticCode_BacterialPlantPlastid.Add("GAC", 'D');
                    GeneticCode_BacterialPlantPlastid.Add("GAG", 'E');
                    GeneticCode_BacterialPlantPlastid.Add("GAT", 'D');
                    GeneticCode_BacterialPlantPlastid.Add("GCA", 'A');
                    GeneticCode_BacterialPlantPlastid.Add("GCC", 'A');
                    GeneticCode_BacterialPlantPlastid.Add("GCG", 'A');
                    GeneticCode_BacterialPlantPlastid.Add("GCT", 'A');
                    GeneticCode_BacterialPlantPlastid.Add("GGA", 'G');
                    GeneticCode_BacterialPlantPlastid.Add("GGC", 'G');
                    GeneticCode_BacterialPlantPlastid.Add("GGG", 'G');
                    GeneticCode_BacterialPlantPlastid.Add("GGT", 'G');
                    GeneticCode_BacterialPlantPlastid.Add("GTA", 'V');
                    GeneticCode_BacterialPlantPlastid.Add("GTC", 'V');
                    GeneticCode_BacterialPlantPlastid.Add("GTG", 'V');
                    GeneticCode_BacterialPlantPlastid.Add("GTT", 'V');
                    GeneticCode_BacterialPlantPlastid.Add("TAA", '*');
                    GeneticCode_BacterialPlantPlastid.Add("TAC", 'Y');
                    GeneticCode_BacterialPlantPlastid.Add("TAG", '*');
                    GeneticCode_BacterialPlantPlastid.Add("TAT", 'Y');
                    GeneticCode_BacterialPlantPlastid.Add("TCA", 'S');
                    GeneticCode_BacterialPlantPlastid.Add("TCC", 'S');
                    GeneticCode_BacterialPlantPlastid.Add("TCG", 'S');
                    GeneticCode_BacterialPlantPlastid.Add("TCT", 'S');
                    GeneticCode_BacterialPlantPlastid.Add("TGA", '*');
                    GeneticCode_BacterialPlantPlastid.Add("TGC", 'C');
                    GeneticCode_BacterialPlantPlastid.Add("TGG", 'W');
                    GeneticCode_BacterialPlantPlastid.Add("TGT", 'C');
                    GeneticCode_BacterialPlantPlastid.Add("TTA", 'L');
                    GeneticCode_BacterialPlantPlastid.Add("TTC", 'F');
                    GeneticCode_BacterialPlantPlastid.Add("TTG", 'L');
                    GeneticCode_BacterialPlantPlastid.Add("TTT", 'F');
                    GeneticCode_BacterialPlantPlastid.Add("---", '-');

                    GeneticCode_AlternativeYeastNuclear = new Dictionary<string, char>(65);
                    GeneticCode_AlternativeYeastNuclear.Add("AAA", 'K');
                    GeneticCode_AlternativeYeastNuclear.Add("AAC", 'N');
                    GeneticCode_AlternativeYeastNuclear.Add("AAG", 'K');
                    GeneticCode_AlternativeYeastNuclear.Add("AAT", 'N');
                    GeneticCode_AlternativeYeastNuclear.Add("ACA", 'T');
                    GeneticCode_AlternativeYeastNuclear.Add("ACC", 'T');
                    GeneticCode_AlternativeYeastNuclear.Add("ACG", 'T');
                    GeneticCode_AlternativeYeastNuclear.Add("ACT", 'T');
                    GeneticCode_AlternativeYeastNuclear.Add("AGA", 'R');
                    GeneticCode_AlternativeYeastNuclear.Add("AGC", 'S');
                    GeneticCode_AlternativeYeastNuclear.Add("AGG", 'R');
                    GeneticCode_AlternativeYeastNuclear.Add("AGT", 'S');
                    GeneticCode_AlternativeYeastNuclear.Add("ATA", 'I');
                    GeneticCode_AlternativeYeastNuclear.Add("ATC", 'I');
                    GeneticCode_AlternativeYeastNuclear.Add("ATG", 'M');
                    GeneticCode_AlternativeYeastNuclear.Add("ATT", 'I');
                    GeneticCode_AlternativeYeastNuclear.Add("CAA", 'Q');
                    GeneticCode_AlternativeYeastNuclear.Add("CAC", 'H');
                    GeneticCode_AlternativeYeastNuclear.Add("CAG", 'Q');
                    GeneticCode_AlternativeYeastNuclear.Add("CAT", 'H');
                    GeneticCode_AlternativeYeastNuclear.Add("CCA", 'P');
                    GeneticCode_AlternativeYeastNuclear.Add("CCC", 'P');
                    GeneticCode_AlternativeYeastNuclear.Add("CCG", 'P');
                    GeneticCode_AlternativeYeastNuclear.Add("CCT", 'P');
                    GeneticCode_AlternativeYeastNuclear.Add("CGA", 'R');
                    GeneticCode_AlternativeYeastNuclear.Add("CGC", 'R');
                    GeneticCode_AlternativeYeastNuclear.Add("CGG", 'R');
                    GeneticCode_AlternativeYeastNuclear.Add("CGT", 'R');
                    GeneticCode_AlternativeYeastNuclear.Add("CTA", 'L');
                    GeneticCode_AlternativeYeastNuclear.Add("CTC", 'L');
                    GeneticCode_AlternativeYeastNuclear.Add("CTG", 'S');
                    GeneticCode_AlternativeYeastNuclear.Add("CTT", 'L');
                    GeneticCode_AlternativeYeastNuclear.Add("GAA", 'E');
                    GeneticCode_AlternativeYeastNuclear.Add("GAC", 'D');
                    GeneticCode_AlternativeYeastNuclear.Add("GAG", 'E');
                    GeneticCode_AlternativeYeastNuclear.Add("GAT", 'D');
                    GeneticCode_AlternativeYeastNuclear.Add("GCA", 'A');
                    GeneticCode_AlternativeYeastNuclear.Add("GCC", 'A');
                    GeneticCode_AlternativeYeastNuclear.Add("GCG", 'A');
                    GeneticCode_AlternativeYeastNuclear.Add("GCT", 'A');
                    GeneticCode_AlternativeYeastNuclear.Add("GGA", 'G');
                    GeneticCode_AlternativeYeastNuclear.Add("GGC", 'G');
                    GeneticCode_AlternativeYeastNuclear.Add("GGG", 'G');
                    GeneticCode_AlternativeYeastNuclear.Add("GGT", 'G');
                    GeneticCode_AlternativeYeastNuclear.Add("GTA", 'V');
                    GeneticCode_AlternativeYeastNuclear.Add("GTC", 'V');
                    GeneticCode_AlternativeYeastNuclear.Add("GTG", 'V');
                    GeneticCode_AlternativeYeastNuclear.Add("GTT", 'V');
                    GeneticCode_AlternativeYeastNuclear.Add("TAA", '*');
                    GeneticCode_AlternativeYeastNuclear.Add("TAC", 'Y');
                    GeneticCode_AlternativeYeastNuclear.Add("TAG", '*');
                    GeneticCode_AlternativeYeastNuclear.Add("TAT", 'Y');
                    GeneticCode_AlternativeYeastNuclear.Add("TCA", 'S');
                    GeneticCode_AlternativeYeastNuclear.Add("TCC", 'S');
                    GeneticCode_AlternativeYeastNuclear.Add("TCG", 'S');
                    GeneticCode_AlternativeYeastNuclear.Add("TCT", 'S');
                    GeneticCode_AlternativeYeastNuclear.Add("TGA", '*');
                    GeneticCode_AlternativeYeastNuclear.Add("TGC", 'C');
                    GeneticCode_AlternativeYeastNuclear.Add("TGG", 'W');
                    GeneticCode_AlternativeYeastNuclear.Add("TGT", 'C');
                    GeneticCode_AlternativeYeastNuclear.Add("TTA", 'L');
                    GeneticCode_AlternativeYeastNuclear.Add("TTC", 'F');
                    GeneticCode_AlternativeYeastNuclear.Add("TTG", 'L');
                    GeneticCode_AlternativeYeastNuclear.Add("TTT", 'F');
                    GeneticCode_AlternativeYeastNuclear.Add("---", '-');

                    GeneticCode_AscidianMitochondrial = new Dictionary<string, char>(65);
                    GeneticCode_AscidianMitochondrial.Add("AAA", 'K');
                    GeneticCode_AscidianMitochondrial.Add("AAC", 'N');
                    GeneticCode_AscidianMitochondrial.Add("AAG", 'K');
                    GeneticCode_AscidianMitochondrial.Add("AAT", 'N');
                    GeneticCode_AscidianMitochondrial.Add("ACA", 'T');
                    GeneticCode_AscidianMitochondrial.Add("ACC", 'T');
                    GeneticCode_AscidianMitochondrial.Add("ACG", 'T');
                    GeneticCode_AscidianMitochondrial.Add("ACT", 'T');
                    GeneticCode_AscidianMitochondrial.Add("AGA", 'G');
                    GeneticCode_AscidianMitochondrial.Add("AGC", 'S');
                    GeneticCode_AscidianMitochondrial.Add("AGG", 'G');
                    GeneticCode_AscidianMitochondrial.Add("AGT", 'S');
                    GeneticCode_AscidianMitochondrial.Add("ATA", 'M');
                    GeneticCode_AscidianMitochondrial.Add("ATC", 'I');
                    GeneticCode_AscidianMitochondrial.Add("ATG", 'M');
                    GeneticCode_AscidianMitochondrial.Add("ATT", 'I');
                    GeneticCode_AscidianMitochondrial.Add("CAA", 'Q');
                    GeneticCode_AscidianMitochondrial.Add("CAC", 'H');
                    GeneticCode_AscidianMitochondrial.Add("CAG", 'Q');
                    GeneticCode_AscidianMitochondrial.Add("CAT", 'H');
                    GeneticCode_AscidianMitochondrial.Add("CCA", 'P');
                    GeneticCode_AscidianMitochondrial.Add("CCC", 'P');
                    GeneticCode_AscidianMitochondrial.Add("CCG", 'P');
                    GeneticCode_AscidianMitochondrial.Add("CCT", 'P');
                    GeneticCode_AscidianMitochondrial.Add("CGA", 'R');
                    GeneticCode_AscidianMitochondrial.Add("CGC", 'R');
                    GeneticCode_AscidianMitochondrial.Add("CGG", 'R');
                    GeneticCode_AscidianMitochondrial.Add("CGT", 'R');
                    GeneticCode_AscidianMitochondrial.Add("CTA", 'L');
                    GeneticCode_AscidianMitochondrial.Add("CTC", 'L');
                    GeneticCode_AscidianMitochondrial.Add("CTG", 'L');
                    GeneticCode_AscidianMitochondrial.Add("CTT", 'L');
                    GeneticCode_AscidianMitochondrial.Add("GAA", 'E');
                    GeneticCode_AscidianMitochondrial.Add("GAC", 'D');
                    GeneticCode_AscidianMitochondrial.Add("GAG", 'E');
                    GeneticCode_AscidianMitochondrial.Add("GAT", 'D');
                    GeneticCode_AscidianMitochondrial.Add("GCA", 'A');
                    GeneticCode_AscidianMitochondrial.Add("GCC", 'A');
                    GeneticCode_AscidianMitochondrial.Add("GCG", 'A');
                    GeneticCode_AscidianMitochondrial.Add("GCT", 'A');
                    GeneticCode_AscidianMitochondrial.Add("GGA", 'G');
                    GeneticCode_AscidianMitochondrial.Add("GGC", 'G');
                    GeneticCode_AscidianMitochondrial.Add("GGG", 'G');
                    GeneticCode_AscidianMitochondrial.Add("GGT", 'G');
                    GeneticCode_AscidianMitochondrial.Add("GTA", 'V');
                    GeneticCode_AscidianMitochondrial.Add("GTC", 'V');
                    GeneticCode_AscidianMitochondrial.Add("GTG", 'V');
                    GeneticCode_AscidianMitochondrial.Add("GTT", 'V');
                    GeneticCode_AscidianMitochondrial.Add("TAA", '*');
                    GeneticCode_AscidianMitochondrial.Add("TAC", 'Y');
                    GeneticCode_AscidianMitochondrial.Add("TAG", '*');
                    GeneticCode_AscidianMitochondrial.Add("TAT", 'Y');
                    GeneticCode_AscidianMitochondrial.Add("TCA", 'S');
                    GeneticCode_AscidianMitochondrial.Add("TCC", 'S');
                    GeneticCode_AscidianMitochondrial.Add("TCG", 'S');
                    GeneticCode_AscidianMitochondrial.Add("TCT", 'S');
                    GeneticCode_AscidianMitochondrial.Add("TGA", 'W');
                    GeneticCode_AscidianMitochondrial.Add("TGC", 'C');
                    GeneticCode_AscidianMitochondrial.Add("TGG", 'W');
                    GeneticCode_AscidianMitochondrial.Add("TGT", 'C');
                    GeneticCode_AscidianMitochondrial.Add("TTA", 'L');
                    GeneticCode_AscidianMitochondrial.Add("TTC", 'F');
                    GeneticCode_AscidianMitochondrial.Add("TTG", 'L');
                    GeneticCode_AscidianMitochondrial.Add("TTT", 'F');
                    GeneticCode_AscidianMitochondrial.Add("---", '-');

                    GeneticCode_FlatwormMitochondrial = new Dictionary<string, char>(65);
                    GeneticCode_FlatwormMitochondrial.Add("AAA", 'N');
                    GeneticCode_FlatwormMitochondrial.Add("AAC", 'N');
                    GeneticCode_FlatwormMitochondrial.Add("AAG", 'K');
                    GeneticCode_FlatwormMitochondrial.Add("AAT", 'N');
                    GeneticCode_FlatwormMitochondrial.Add("ACA", 'T');
                    GeneticCode_FlatwormMitochondrial.Add("ACC", 'T');
                    GeneticCode_FlatwormMitochondrial.Add("ACG", 'T');
                    GeneticCode_FlatwormMitochondrial.Add("ACT", 'T');
                    GeneticCode_FlatwormMitochondrial.Add("AGA", 'S');
                    GeneticCode_FlatwormMitochondrial.Add("AGC", 'S');
                    GeneticCode_FlatwormMitochondrial.Add("AGG", 'S');
                    GeneticCode_FlatwormMitochondrial.Add("AGT", 'S');
                    GeneticCode_FlatwormMitochondrial.Add("ATA", 'I');
                    GeneticCode_FlatwormMitochondrial.Add("ATC", 'I');
                    GeneticCode_FlatwormMitochondrial.Add("ATG", 'M');
                    GeneticCode_FlatwormMitochondrial.Add("ATT", 'I');
                    GeneticCode_FlatwormMitochondrial.Add("CAA", 'Q');
                    GeneticCode_FlatwormMitochondrial.Add("CAC", 'H');
                    GeneticCode_FlatwormMitochondrial.Add("CAG", 'Q');
                    GeneticCode_FlatwormMitochondrial.Add("CAT", 'H');
                    GeneticCode_FlatwormMitochondrial.Add("CCA", 'P');
                    GeneticCode_FlatwormMitochondrial.Add("CCC", 'P');
                    GeneticCode_FlatwormMitochondrial.Add("CCG", 'P');
                    GeneticCode_FlatwormMitochondrial.Add("CCT", 'P');
                    GeneticCode_FlatwormMitochondrial.Add("CGA", 'R');
                    GeneticCode_FlatwormMitochondrial.Add("CGC", 'R');
                    GeneticCode_FlatwormMitochondrial.Add("CGG", 'R');
                    GeneticCode_FlatwormMitochondrial.Add("CGT", 'R');
                    GeneticCode_FlatwormMitochondrial.Add("CTA", 'L');
                    GeneticCode_FlatwormMitochondrial.Add("CTC", 'L');
                    GeneticCode_FlatwormMitochondrial.Add("CTG", 'L');
                    GeneticCode_FlatwormMitochondrial.Add("CTT", 'L');
                    GeneticCode_FlatwormMitochondrial.Add("GAA", 'E');
                    GeneticCode_FlatwormMitochondrial.Add("GAC", 'D');
                    GeneticCode_FlatwormMitochondrial.Add("GAG", 'E');
                    GeneticCode_FlatwormMitochondrial.Add("GAT", 'D');
                    GeneticCode_FlatwormMitochondrial.Add("GCA", 'A');
                    GeneticCode_FlatwormMitochondrial.Add("GCC", 'A');
                    GeneticCode_FlatwormMitochondrial.Add("GCG", 'A');
                    GeneticCode_FlatwormMitochondrial.Add("GCT", 'A');
                    GeneticCode_FlatwormMitochondrial.Add("GGA", 'G');
                    GeneticCode_FlatwormMitochondrial.Add("GGC", 'G');
                    GeneticCode_FlatwormMitochondrial.Add("GGG", 'G');
                    GeneticCode_FlatwormMitochondrial.Add("GGT", 'G');
                    GeneticCode_FlatwormMitochondrial.Add("GTA", 'V');
                    GeneticCode_FlatwormMitochondrial.Add("GTC", 'V');
                    GeneticCode_FlatwormMitochondrial.Add("GTG", 'V');
                    GeneticCode_FlatwormMitochondrial.Add("GTT", 'V');
                    GeneticCode_FlatwormMitochondrial.Add("TAA", 'Y');
                    GeneticCode_FlatwormMitochondrial.Add("TAC", 'Y');
                    GeneticCode_FlatwormMitochondrial.Add("TAG", '*');
                    GeneticCode_FlatwormMitochondrial.Add("TAT", 'Y');
                    GeneticCode_FlatwormMitochondrial.Add("TCA", 'S');
                    GeneticCode_FlatwormMitochondrial.Add("TCC", 'S');
                    GeneticCode_FlatwormMitochondrial.Add("TCG", 'S');
                    GeneticCode_FlatwormMitochondrial.Add("TCT", 'S');
                    GeneticCode_FlatwormMitochondrial.Add("TGA", 'W');
                    GeneticCode_FlatwormMitochondrial.Add("TGC", 'C');
                    GeneticCode_FlatwormMitochondrial.Add("TGG", 'W');
                    GeneticCode_FlatwormMitochondrial.Add("TGT", 'C');
                    GeneticCode_FlatwormMitochondrial.Add("TTA", 'L');
                    GeneticCode_FlatwormMitochondrial.Add("TTC", 'F');
                    GeneticCode_FlatwormMitochondrial.Add("TTG", 'L');
                    GeneticCode_FlatwormMitochondrial.Add("TTT", 'F');
                    GeneticCode_FlatwormMitochondrial.Add("---", '-');

                    GeneticCode_BlepharismaNuclear = new Dictionary<string, char>(65);
                    GeneticCode_BlepharismaNuclear.Add("AAA", 'K');
                    GeneticCode_BlepharismaNuclear.Add("AAC", 'N');
                    GeneticCode_BlepharismaNuclear.Add("AAG", 'K');
                    GeneticCode_BlepharismaNuclear.Add("AAT", 'N');
                    GeneticCode_BlepharismaNuclear.Add("ACA", 'T');
                    GeneticCode_BlepharismaNuclear.Add("ACC", 'T');
                    GeneticCode_BlepharismaNuclear.Add("ACG", 'T');
                    GeneticCode_BlepharismaNuclear.Add("ACT", 'T');
                    GeneticCode_BlepharismaNuclear.Add("AGA", 'R');
                    GeneticCode_BlepharismaNuclear.Add("AGC", 'S');
                    GeneticCode_BlepharismaNuclear.Add("AGG", 'R');
                    GeneticCode_BlepharismaNuclear.Add("AGT", 'S');
                    GeneticCode_BlepharismaNuclear.Add("ATA", 'I');
                    GeneticCode_BlepharismaNuclear.Add("ATC", 'I');
                    GeneticCode_BlepharismaNuclear.Add("ATG", 'M');
                    GeneticCode_BlepharismaNuclear.Add("ATT", 'I');
                    GeneticCode_BlepharismaNuclear.Add("CAA", 'Q');
                    GeneticCode_BlepharismaNuclear.Add("CAC", 'H');
                    GeneticCode_BlepharismaNuclear.Add("CAG", 'Q');
                    GeneticCode_BlepharismaNuclear.Add("CAT", 'H');
                    GeneticCode_BlepharismaNuclear.Add("CCA", 'P');
                    GeneticCode_BlepharismaNuclear.Add("CCC", 'P');
                    GeneticCode_BlepharismaNuclear.Add("CCG", 'P');
                    GeneticCode_BlepharismaNuclear.Add("CCT", 'P');
                    GeneticCode_BlepharismaNuclear.Add("CGA", 'R');
                    GeneticCode_BlepharismaNuclear.Add("CGC", 'R');
                    GeneticCode_BlepharismaNuclear.Add("CGG", 'R');
                    GeneticCode_BlepharismaNuclear.Add("CGT", 'R');
                    GeneticCode_BlepharismaNuclear.Add("CTA", 'L');
                    GeneticCode_BlepharismaNuclear.Add("CTC", 'L');
                    GeneticCode_BlepharismaNuclear.Add("CTG", 'L');
                    GeneticCode_BlepharismaNuclear.Add("CTT", 'L');
                    GeneticCode_BlepharismaNuclear.Add("GAA", 'E');
                    GeneticCode_BlepharismaNuclear.Add("GAC", 'D');
                    GeneticCode_BlepharismaNuclear.Add("GAG", 'E');
                    GeneticCode_BlepharismaNuclear.Add("GAT", 'D');
                    GeneticCode_BlepharismaNuclear.Add("GCA", 'A');
                    GeneticCode_BlepharismaNuclear.Add("GCC", 'A');
                    GeneticCode_BlepharismaNuclear.Add("GCG", 'A');
                    GeneticCode_BlepharismaNuclear.Add("GCT", 'A');
                    GeneticCode_BlepharismaNuclear.Add("GGA", 'G');
                    GeneticCode_BlepharismaNuclear.Add("GGC", 'G');
                    GeneticCode_BlepharismaNuclear.Add("GGG", 'G');
                    GeneticCode_BlepharismaNuclear.Add("GGT", 'G');
                    GeneticCode_BlepharismaNuclear.Add("GTA", 'V');
                    GeneticCode_BlepharismaNuclear.Add("GTC", 'V');
                    GeneticCode_BlepharismaNuclear.Add("GTG", 'V');
                    GeneticCode_BlepharismaNuclear.Add("GTT", 'V');
                    GeneticCode_BlepharismaNuclear.Add("TAA", '*');
                    GeneticCode_BlepharismaNuclear.Add("TAC", 'Y');
                    GeneticCode_BlepharismaNuclear.Add("TAG", 'Q');
                    GeneticCode_BlepharismaNuclear.Add("TAT", 'Y');
                    GeneticCode_BlepharismaNuclear.Add("TCA", 'S');
                    GeneticCode_BlepharismaNuclear.Add("TCC", 'S');
                    GeneticCode_BlepharismaNuclear.Add("TCG", 'S');
                    GeneticCode_BlepharismaNuclear.Add("TCT", 'S');
                    GeneticCode_BlepharismaNuclear.Add("TGA", '*');
                    GeneticCode_BlepharismaNuclear.Add("TGC", 'C');
                    GeneticCode_BlepharismaNuclear.Add("TGG", 'W');
                    GeneticCode_BlepharismaNuclear.Add("TGT", 'C');
                    GeneticCode_BlepharismaNuclear.Add("TTA", 'L');
                    GeneticCode_BlepharismaNuclear.Add("TTC", 'F');
                    GeneticCode_BlepharismaNuclear.Add("TTG", 'L');
                    GeneticCode_BlepharismaNuclear.Add("TTT", 'F');
                    GeneticCode_BlepharismaNuclear.Add("---", '-');

                    GeneticCode_ChlorophyceanMitochondrial = new Dictionary<string, char>(65);
                    GeneticCode_ChlorophyceanMitochondrial.Add("AAA", 'K');
                    GeneticCode_ChlorophyceanMitochondrial.Add("AAC", 'N');
                    GeneticCode_ChlorophyceanMitochondrial.Add("AAG", 'K');
                    GeneticCode_ChlorophyceanMitochondrial.Add("AAT", 'N');
                    GeneticCode_ChlorophyceanMitochondrial.Add("ACA", 'T');
                    GeneticCode_ChlorophyceanMitochondrial.Add("ACC", 'T');
                    GeneticCode_ChlorophyceanMitochondrial.Add("ACG", 'T');
                    GeneticCode_ChlorophyceanMitochondrial.Add("ACT", 'T');
                    GeneticCode_ChlorophyceanMitochondrial.Add("AGA", 'R');
                    GeneticCode_ChlorophyceanMitochondrial.Add("AGC", 'S');
                    GeneticCode_ChlorophyceanMitochondrial.Add("AGG", 'R');
                    GeneticCode_ChlorophyceanMitochondrial.Add("AGT", 'S');
                    GeneticCode_ChlorophyceanMitochondrial.Add("ATA", 'I');
                    GeneticCode_ChlorophyceanMitochondrial.Add("ATC", 'I');
                    GeneticCode_ChlorophyceanMitochondrial.Add("ATG", 'M');
                    GeneticCode_ChlorophyceanMitochondrial.Add("ATT", 'I');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CAA", 'Q');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CAC", 'H');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CAG", 'Q');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CAT", 'H');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CCA", 'P');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CCC", 'P');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CCG", 'P');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CCT", 'P');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CGA", 'R');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CGC", 'R');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CGG", 'R');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CGT", 'R');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CTA", 'L');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CTC", 'L');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CTG", 'L');
                    GeneticCode_ChlorophyceanMitochondrial.Add("CTT", 'L');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GAA", 'E');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GAC", 'D');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GAG", 'E');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GAT", 'D');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GCA", 'A');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GCC", 'A');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GCG", 'A');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GCT", 'A');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GGA", 'G');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GGC", 'G');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GGG", 'G');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GGT", 'G');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GTA", 'V');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GTC", 'V');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GTG", 'V');
                    GeneticCode_ChlorophyceanMitochondrial.Add("GTT", 'V');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TAA", '*');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TAC", 'Y');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TAG", 'L');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TAT", 'Y');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TCA", 'S');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TCC", 'S');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TCG", 'S');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TCT", 'S');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TGA", '*');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TGC", 'C');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TGG", 'W');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TGT", 'C');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TTA", 'L');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TTC", 'F');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TTG", 'L');
                    GeneticCode_ChlorophyceanMitochondrial.Add("TTT", 'F');
                    GeneticCode_ChlorophyceanMitochondrial.Add("---", '-');

                    GeneticCode_TrematodeMitochondrial = new Dictionary<string, char>(65);
                    GeneticCode_TrematodeMitochondrial.Add("AAA", 'N');
                    GeneticCode_TrematodeMitochondrial.Add("AAC", 'N');
                    GeneticCode_TrematodeMitochondrial.Add("AAG", 'K');
                    GeneticCode_TrematodeMitochondrial.Add("AAT", 'N');
                    GeneticCode_TrematodeMitochondrial.Add("ACA", 'T');
                    GeneticCode_TrematodeMitochondrial.Add("ACC", 'T');
                    GeneticCode_TrematodeMitochondrial.Add("ACG", 'T');
                    GeneticCode_TrematodeMitochondrial.Add("ACT", 'T');
                    GeneticCode_TrematodeMitochondrial.Add("AGA", 'S');
                    GeneticCode_TrematodeMitochondrial.Add("AGC", 'S');
                    GeneticCode_TrematodeMitochondrial.Add("AGG", 'S');
                    GeneticCode_TrematodeMitochondrial.Add("AGT", 'S');
                    GeneticCode_TrematodeMitochondrial.Add("ATA", 'M');
                    GeneticCode_TrematodeMitochondrial.Add("ATC", 'I');
                    GeneticCode_TrematodeMitochondrial.Add("ATG", 'M');
                    GeneticCode_TrematodeMitochondrial.Add("ATT", 'I');
                    GeneticCode_TrematodeMitochondrial.Add("CAA", 'Q');
                    GeneticCode_TrematodeMitochondrial.Add("CAC", 'H');
                    GeneticCode_TrematodeMitochondrial.Add("CAG", 'Q');
                    GeneticCode_TrematodeMitochondrial.Add("CAT", 'H');
                    GeneticCode_TrematodeMitochondrial.Add("CCA", 'P');
                    GeneticCode_TrematodeMitochondrial.Add("CCC", 'P');
                    GeneticCode_TrematodeMitochondrial.Add("CCG", 'P');
                    GeneticCode_TrematodeMitochondrial.Add("CCT", 'P');
                    GeneticCode_TrematodeMitochondrial.Add("CGA", 'R');
                    GeneticCode_TrematodeMitochondrial.Add("CGC", 'R');
                    GeneticCode_TrematodeMitochondrial.Add("CGG", 'R');
                    GeneticCode_TrematodeMitochondrial.Add("CGT", 'R');
                    GeneticCode_TrematodeMitochondrial.Add("CTA", 'L');
                    GeneticCode_TrematodeMitochondrial.Add("CTC", 'L');
                    GeneticCode_TrematodeMitochondrial.Add("CTG", 'L');
                    GeneticCode_TrematodeMitochondrial.Add("CTT", 'L');
                    GeneticCode_TrematodeMitochondrial.Add("GAA", 'E');
                    GeneticCode_TrematodeMitochondrial.Add("GAC", 'D');
                    GeneticCode_TrematodeMitochondrial.Add("GAG", 'E');
                    GeneticCode_TrematodeMitochondrial.Add("GAT", 'D');
                    GeneticCode_TrematodeMitochondrial.Add("GCA", 'A');
                    GeneticCode_TrematodeMitochondrial.Add("GCC", 'A');
                    GeneticCode_TrematodeMitochondrial.Add("GCG", 'A');
                    GeneticCode_TrematodeMitochondrial.Add("GCT", 'A');
                    GeneticCode_TrematodeMitochondrial.Add("GGA", 'G');
                    GeneticCode_TrematodeMitochondrial.Add("GGC", 'G');
                    GeneticCode_TrematodeMitochondrial.Add("GGG", 'G');
                    GeneticCode_TrematodeMitochondrial.Add("GGT", 'G');
                    GeneticCode_TrematodeMitochondrial.Add("GTA", 'V');
                    GeneticCode_TrematodeMitochondrial.Add("GTC", 'V');
                    GeneticCode_TrematodeMitochondrial.Add("GTG", 'V');
                    GeneticCode_TrematodeMitochondrial.Add("GTT", 'V');
                    GeneticCode_TrematodeMitochondrial.Add("TAA", '*');
                    GeneticCode_TrematodeMitochondrial.Add("TAC", 'Y');
                    GeneticCode_TrematodeMitochondrial.Add("TAG", '*');
                    GeneticCode_TrematodeMitochondrial.Add("TAT", 'Y');
                    GeneticCode_TrematodeMitochondrial.Add("TCA", 'S');
                    GeneticCode_TrematodeMitochondrial.Add("TCC", 'S');
                    GeneticCode_TrematodeMitochondrial.Add("TCG", 'S');
                    GeneticCode_TrematodeMitochondrial.Add("TCT", 'S');
                    GeneticCode_TrematodeMitochondrial.Add("TGA", 'W');
                    GeneticCode_TrematodeMitochondrial.Add("TGC", 'C');
                    GeneticCode_TrematodeMitochondrial.Add("TGG", 'W');
                    GeneticCode_TrematodeMitochondrial.Add("TGT", 'C');
                    GeneticCode_TrematodeMitochondrial.Add("TTA", 'L');
                    GeneticCode_TrematodeMitochondrial.Add("TTC", 'F');
                    GeneticCode_TrematodeMitochondrial.Add("TTG", 'L');
                    GeneticCode_TrematodeMitochondrial.Add("TTT", 'F');
                    GeneticCode_TrematodeMitochondrial.Add("---", '-');

                    GeneticCode_ScenedesmusObliquusMitochondrial = new Dictionary<string, char>(65);
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("AAA", 'K');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("AAC", 'N');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("AAG", 'K');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("AAT", 'N');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("ACA", 'T');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("ACC", 'T');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("ACG", 'T');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("ACT", 'T');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("AGA", 'R');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("AGC", 'S');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("AGG", 'R');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("AGT", 'S');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("ATA", 'I');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("ATC", 'I');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("ATG", 'M');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("ATT", 'I');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CAA", 'Q');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CAC", 'H');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CAG", 'Q');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CAT", 'H');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CCA", 'P');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CCC", 'P');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CCG", 'P');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CCT", 'P');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CGA", 'R');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CGC", 'R');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CGG", 'R');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CGT", 'R');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CTA", 'L');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CTC", 'L');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CTG", 'L');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("CTT", 'L');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GAA", 'E');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GAC", 'D');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GAG", 'E');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GAT", 'D');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GCA", 'A');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GCC", 'A');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GCG", 'A');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GCT", 'A');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GGA", 'G');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GGC", 'G');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GGG", 'G');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GGT", 'G');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GTA", 'V');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GTC", 'V');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GTG", 'V');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("GTT", 'V');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TAA", '*');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TAC", 'Y');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TAG", 'L');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TAT", 'Y');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TCA", '*');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TCC", 'S');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TCG", 'S');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TCT", 'S');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TGA", '*');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TGC", 'C');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TGG", 'W');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TGT", 'C');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TTA", 'L');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TTC", 'F');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TTG", 'L');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("TTT", 'F');
                    GeneticCode_ScenedesmusObliquusMitochondrial.Add("---", '-');

                    GeneticCode_ThraustochytriumMitochondrial = new Dictionary<string, char>(65);
                    GeneticCode_ThraustochytriumMitochondrial.Add("AAA", 'K');
                    GeneticCode_ThraustochytriumMitochondrial.Add("AAC", 'N');
                    GeneticCode_ThraustochytriumMitochondrial.Add("AAG", 'K');
                    GeneticCode_ThraustochytriumMitochondrial.Add("AAT", 'N');
                    GeneticCode_ThraustochytriumMitochondrial.Add("ACA", 'T');
                    GeneticCode_ThraustochytriumMitochondrial.Add("ACC", 'T');
                    GeneticCode_ThraustochytriumMitochondrial.Add("ACG", 'T');
                    GeneticCode_ThraustochytriumMitochondrial.Add("ACT", 'T');
                    GeneticCode_ThraustochytriumMitochondrial.Add("AGA", 'R');
                    GeneticCode_ThraustochytriumMitochondrial.Add("AGC", 'S');
                    GeneticCode_ThraustochytriumMitochondrial.Add("AGG", 'R');
                    GeneticCode_ThraustochytriumMitochondrial.Add("AGT", 'S');
                    GeneticCode_ThraustochytriumMitochondrial.Add("ATA", 'I');
                    GeneticCode_ThraustochytriumMitochondrial.Add("ATC", 'I');
                    GeneticCode_ThraustochytriumMitochondrial.Add("ATG", 'M');
                    GeneticCode_ThraustochytriumMitochondrial.Add("ATT", 'I');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CAA", 'Q');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CAC", 'H');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CAG", 'Q');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CAT", 'H');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CCA", 'P');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CCC", 'P');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CCG", 'P');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CCT", 'P');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CGA", 'R');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CGC", 'R');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CGG", 'R');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CGT", 'R');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CTA", 'L');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CTC", 'L');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CTG", 'L');
                    GeneticCode_ThraustochytriumMitochondrial.Add("CTT", 'L');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GAA", 'E');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GAC", 'D');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GAG", 'E');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GAT", 'D');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GCA", 'A');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GCC", 'A');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GCG", 'A');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GCT", 'A');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GGA", 'G');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GGC", 'G');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GGG", 'G');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GGT", 'G');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GTA", 'V');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GTC", 'V');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GTG", 'V');
                    GeneticCode_ThraustochytriumMitochondrial.Add("GTT", 'V');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TAA", '*');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TAC", 'Y');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TAG", '*');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TAT", 'Y');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TCA", 'S');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TCC", 'S');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TCG", 'S');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TCT", 'S');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TGA", '*');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TGC", 'C');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TGG", 'W');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TGT", 'C');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TTA", '*');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TTC", 'F');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TTG", 'L');
                    GeneticCode_ThraustochytriumMitochondrial.Add("TTT", 'F');
                    GeneticCode_ThraustochytriumMitochondrial.Add("---", '-');

                    Code2GeneticCode = new Dictionary<int, Dictionary<string, char>>(17);
                    Code2GeneticCode.Add((int)Codes.Standard, GeneticCode_Standard);
                    Code2GeneticCode.Add((int)Codes.VertebrateMitochondrial, GeneticCode_VertebrateMitochondrial);
                    Code2GeneticCode.Add((int)Codes.YeastMitochondrial, GeneticCode_YeastMitochondrial);
                    Code2GeneticCode.Add((int)Codes.MoldProtozoanCoelente, GeneticCode_MoldProtozoanCoelente);
                    Code2GeneticCode.Add((int)Codes.InvertebrateMitochondrial, GeneticCode_InvertebrateMitochondrial);
                    Code2GeneticCode.Add((int)Codes.CiliateDasycladaceanHexamitaNuclear, GeneticCode_CiliateDasycladaceanHexamitaNuclear);
                    Code2GeneticCode.Add((int)Codes.EchinodermMitochondrial, GeneticCode_EchinodermMitochondrial);
                    Code2GeneticCode.Add((int)Codes.EuplotidNuclear, GeneticCode_EuplotidNuclear);
                    Code2GeneticCode.Add((int)Codes.BacterialPlantPlastid, GeneticCode_BacterialPlantPlastid);
                    Code2GeneticCode.Add((int)Codes.AlternativeYeastNuclear, GeneticCode_AlternativeYeastNuclear);
                    Code2GeneticCode.Add((int)Codes.AscidianMitochondrial, GeneticCode_AscidianMitochondrial);
                    Code2GeneticCode.Add((int)Codes.FlatwormMitochondrial, GeneticCode_FlatwormMitochondrial);
                    Code2GeneticCode.Add((int)Codes.BlepharismaNuclear, GeneticCode_BlepharismaNuclear);
                    Code2GeneticCode.Add((int)Codes.ChlorophyceanMitochondrial, GeneticCode_ChlorophyceanMitochondrial);
                    Code2GeneticCode.Add((int)Codes.TrematodeMitochondrial, GeneticCode_TrematodeMitochondrial);
                    Code2GeneticCode.Add((int)Codes.ScenedesmusObliquusMitochondrial, GeneticCode_ScenedesmusObliquusMitochondrial);
                    Code2GeneticCode.Add((int)Codes.ThraustochytriumMitochondrial, GeneticCode_ThraustochytriumMitochondrial);

                    Code2GeneticCode_Starts = new Dictionary<int, string[]>(17);
                    Code2GeneticCode_Starts.Add((int)Codes.Standard, GeneticCode_Starts_Standard);
                    Code2GeneticCode_Starts.Add((int)Codes.VertebrateMitochondrial, GeneticCode_Starts_VertebrateMitochondrial);
                    Code2GeneticCode_Starts.Add((int)Codes.YeastMitochondrial, GeneticCode_Starts_YeastMitochondrial);
                    Code2GeneticCode_Starts.Add((int)Codes.MoldProtozoanCoelente, GeneticCode_Starts_MoldProtozoanCoelente);
                    Code2GeneticCode_Starts.Add((int)Codes.InvertebrateMitochondrial, GeneticCode_Starts_InvertebrateMitochondrial);
                    Code2GeneticCode_Starts.Add((int)Codes.CiliateDasycladaceanHexamitaNuclear, GeneticCode_Starts_CiliateDasycladaceanHexamitaNuclear);
                    Code2GeneticCode_Starts.Add((int)Codes.EchinodermMitochondrial, GeneticCode_Starts_EchinodermMitochondrial);
                    Code2GeneticCode_Starts.Add((int)Codes.EuplotidNuclear, GeneticCode_Starts_EuplotidNuclear);
                    Code2GeneticCode_Starts.Add((int)Codes.BacterialPlantPlastid, GeneticCode_Starts_BacterialPlantPlastid);
                    Code2GeneticCode_Starts.Add((int)Codes.AlternativeYeastNuclear, GeneticCode_Starts_AlternativeYeastNuclear);
                    Code2GeneticCode_Starts.Add((int)Codes.AscidianMitochondrial, GeneticCode_Starts_AscidianMitochondrial);
                    Code2GeneticCode_Starts.Add((int)Codes.FlatwormMitochondrial, GeneticCode_Starts_FlatwormMitochondrial);
                    Code2GeneticCode_Starts.Add((int)Codes.BlepharismaNuclear, GeneticCode_Starts_BlepharismaNuclear);
                    Code2GeneticCode_Starts.Add((int)Codes.ChlorophyceanMitochondrial, GeneticCode_Starts_ChlorophyceanMitochondrial);
                    Code2GeneticCode_Starts.Add((int)Codes.TrematodeMitochondrial, GeneticCode_Starts_TrematodeMitochondrial);
                    Code2GeneticCode_Starts.Add((int)Codes.ScenedesmusObliquusMitochondrial, GeneticCode_Starts_ScenedesmusObliquusMitochondrial);
                    Code2GeneticCode_Starts.Add((int)Codes.ThraustochytriumMitochondrial, GeneticCode_Starts_ThraustochytriumMitochondrial);

                }

                /// <summary>
                /// Convert nucleotide sequence to amino acid sequence
                /// </summary>
                /// <param name="nt">String specifying a nucleotide sequence (Upper case). Valid characters include A, C, G, T, U, -. 
                /// Hyphens are valid only if the codon to which it belongs represents a gap, that is, the codon contains all hyphens.\n
                /// Example: ACT---TGA. ACT-TGA (Invalid).</param>
                /// <param name="frame">Property to specify a reading frame.</param>
                /// <param name="code">Property to specify a genetic code. </param>
                /// <param name="alternativeStartCodons">Property to control the translation of alternative codons. Default is <code>true</code>.</param>
                /// <returns></returns>
                public static string nt2aa(string nt, Frames frame, Codes code, bool alternativeStartCodons)
                {
                    // convert rna2dna
                    if (nt.Contains('U'))
                        nt = nt.Replace('U', 'T');

                    StringBuilder aa = new StringBuilder(nt.Length / 3);

                    int frameNum = (int)frame;
                    Dictionary<string, char> geneticCode = Code2GeneticCode[frameNum];


                    string triplet = "";
                    try
                    {
                        // loop through the codons looking up the AAs as we go
                        for (int i = frameNum; i < nt.Length; i += 3)
                        {
                            triplet = nt.Substring(i, 3);

                            aa.Append(geneticCode[triplet]);
                        }
                    }
                    catch
                    {
                        throw new InvalidOperationException("Invalid codon: " + triplet);
                    }

                    // deal with alternative start codons
                    if (alternativeStartCodons && nt.Length >= (frameNum) + 2)
                    {
                        string[] startCodons = Code2GeneticCode_Starts[frameNum];
                        if (startCodons.Contains<string>(nt.Substring(frameNum, 3)))
                        {
                            aa.Remove(0, 1);
                            aa.Insert(0, 'M');
                        }
                    }

                    return aa.ToString();
                }
            }


        }

        /// <summary>
        /// Clustered Scoring Matrix
        /// </summary>
        public class SubstitutionMatrix
        {
            // TODO: these member properties should be readonly
            public double[,] Matrix { get; set; }
            public string Name { get; set; }
            public double Scale { get; set; }
            public double Entropy { get; set; }
            public double ExpectedScore { get; set; }
            public double HighestScore { get; set; }
            public double LowestScore { get; set; }
            public string Order { get; set; }

            public readonly static SubstitutionMatrix Blosum50;
            public readonly static SubstitutionMatrix Blosum50n;
            public readonly static SubstitutionMatrix Blosum62n;
            public readonly static SubstitutionMatrix Pam50n;
            public readonly static SubstitutionMatrix Pam150n;
            public readonly static SubstitutionMatrix Nuc44;

            static SubstitutionMatrix()
            {
                Blosum50n = new SubstitutionMatrix();
                Blosum50n.Name = "BLOSUM50";
                Blosum50n.Scale = 1 / 3;
                Blosum50n.Entropy = 0.4808;
                Blosum50n.ExpectedScore = -0.3573;
                Blosum50n.HighestScore = 15;
                Blosum50n.LowestScore = -5;
                Blosum50n.Order = "ARNDCQEGHILKMFPSTWYVBZX*";
                Blosum50n.Matrix = new double[24, 24] {
                    {5, -2, -1, -2, -1, -1, -1, 0, -2, -1, -2, -1, -1, -3, -1, 1, 0, -3, -2, 0, -2, -1, -1, -5},
                    {-2, 7, -1, -2, -4, 1, 0, -3, 0, -4, -3, 3, -2, -3, -3, -1, -1, -3, -1, -3, -1, 0, -1, -5},
                    {-1, -1, 7, 2, -2, 0, 0, 0, 1, -3, -4, 0, -2, -4, -2, 1, 0, -4, -2, -3, 4, 0, -1, -5},
                    {-2, -2, 2, 8, -4, 0, 2, -1, -1, -4, -4, -1, -4, -5, -1, 0, -1, -5, -3, -4, 5, 1, -1, -5},
                    {-1, -4, -2, -4, 13, -3, -3, -3, -3, -2, -2, -3, -2, -2, -4, -1, -1, -5, -3, -1, -3, -3, -2, -5},
                    {-1, 1, 0, 0, -3, 7, 2, -2, 1, -3, -2, 2, 0, -4, -1, 0, -1, -1, -1, -3, 0, 4, -1, -5},
                    {-1, 0, 0, 2, -3, 2, 6, -3, 0, -4, -3, 1, -2, -3, -1, -1, -1, -3, -2, -3, 1, 5, -1, -5},
                    {0, -3, 0, -1, -3, -2, -3, 8, -2, -4, -4, -2, -3, -4, -2, 0, -2, -3, -3, -4, -1, -2, -2, -5},
                    {-2, 0, 1, -1, -3, 1, 0, -2, 10, -4, -3, 0, -1, -1, -2, -1, -2, -3, 2, -4, 0, 0, -1, -5},
                    {-1, -4, -3, -4, -2, -3, -4, -4, -4, 5, 2, -3, 2, 0, -3, -3, -1, -3, -1, 4, -4, -3, -1, -5},
                    {-2, -3, -4, -4, -2, -2, -3, -4, -3, 2, 5, -3, 3, 1, -4, -3, -1, -2, -1, 1, -4, -3, -1, -5},
                    {-1, 3, 0, -1, -3, 2, 1, -2, 0, -3, -3, 6, -2, -4, -1, 0, -1, -3, -2, -3, 0, 1, -1, -5},
                    {-1, -2, -2, -4, -2, 0, -2, -3, -1, 2, 3, -2, 7, 0, -3, -2, -1, -1, 0, 1, -3, -1, -1, -5},
                    {-3, -3, -4, -5, -2, -4, -3, -4, -1, 0, 1, -4, 0, 8, -4, -3, -2, 1, 4, -1, -4, -4, -2, -5},
                    {-1, -3, -2, -1, -4, -1, -1, -2, -2, -3, -4, -1, -3, -4, 10, -1, -1, -4, -3, -3, -2, -1, -2, -5},
                    {1, -1, 1, 0, -1, 0, -1, 0, -1, -3, -3, 0, -2, -3, -1, 5, 2, -4, -2, -2, 0, 0, -1, -5},
                    {0, -1, 0, -1, -1, -1, -1, -2, -2, -1, -1, -1, -1, -2, -1, 2, 5, -3, -2, 0, 0, -1, 0, -5},
                    {-3, -3, -4, -5, -5, -1, -3, -3, -3, -3, -2, -3, -1, 1, -4, -4, -3, 15, 2, -3, -5, -2, -3, -5},
                    {-2, -1, -2, -3, -3, -1, -2, -3, 2, -1, -1, -2, 0, 4, -3, -2, -2, 2, 8, -1, -3, -2, -1, -5},
                    {0, -3, -3, -4, -1, -3, -3, -4, -4, 4, 1, -3, 1, -1, -3, -2, 0, -3, -1, 5, -4, -3, -1, -5},
                    {-2, -1, 4, 5, -3, 0, 1, -1, 0, -4, -4, 0, -3, -4, -2, 0, 0, -5, -3, -4, 5, 2, -1, -5},
                    {-1, 0, 0, 1, -3, 4, 5, -2, 0, -3, -3, 1, -1, -4, -1, 0, -1, -2, -2, -3, 2, 5, -1, -5},
                    {-1, -1, -1, -1, -2, -1, -1, -2, -1, -1, -1, -1, -1, -2, -2, -1, 0, -3, -1, -1, -1, -1, -1, -5},
                    {-5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, 1}
                    };

                Blosum50 = new SubstitutionMatrix();
                Blosum50.Name = "BLOSUM50";
                Blosum50.Scale = 1 / 3;
                Blosum50.Entropy = 0.4808;
                Blosum50.ExpectedScore = -0.3573;
                Blosum50.HighestScore = 15;
                Blosum50.LowestScore = -5;
                Blosum50.Order = "ARNDCQEGHILKMFPSTWYV";
                Blosum50.Matrix = new double[20, 20] {
                    {5, -2, -1, -2, -1, -1, -1, 0, -2, -1, -2, -1, -1, -3, -1, 1, 0, -3, -2, 0},
                    {-2, 7, -1, -2, -4, 1, 0, -3, 0, -4, -3, 3, -2, -3, -3, -1, -1, -3, -1, -3},
                    {-1, -1, 7, 2, -2, 0, 0, 0, 1, -3, -4, 0, -2, -4, -2, 1, 0, -4, -2, -3},
                    {-2, -2, 2, 8, -4, 0, 2, -1, -1, -4, -4, -1, -4, -5, -1, 0, -1, -5, -3, -4},
                    {-1, -4, -2, -4, 13, -3, -3, -3, -3, -2, -2, -3, -2, -2, -4, -1, -1, -5, -3, -1},
                    {-1, 1, 0, 0, -3, 7, 2, -2, 1, -3, -2, 2, 0, -4, -1, 0, -1, -1, -1, -3},
                    {-1, 0, 0, 2, -3, 2, 6, -3, 0, -4, -3, 1, -2, -3, -1, -1, -1, -3, -2, -3},
                    {0, -3, 0, -1, -3, -2, -3, 8, -2, -4, -4, -2, -3, -4, -2, 0, -2, -3, -3, -4},
                    {-2, 0, 1, -1, -3, 1, 0, -2, 10, -4, -3, 0, -1, -1, -2, -1, -2, -3, 2, -4},
                    {-1, -4, -3, -4, -2, -3, -4, -4, -4, 5, 2, -3, 2, 0, -3, -3, -1, -3, -1, 4},
                    {-2, -3, -4, -4, -2, -2, -3, -4, -3, 2, 5, -3, 3, 1, -4, -3, -1, -2, -1, 1},
                    {-1, 3, 0, -1, -3, 2, 1, -2, 0, -3, -3, 6, -2, -4, -1, 0, -1, -3, -2, -3},
                    {-1, -2, -2, -4, -2, 0, -2, -3, -1, 2, 3, -2, 7, 0, -3, -2, -1, -1, 0, 1},
                    {-3, -3, -4, -5, -2, -4, -3, -4, -1, 0, 1, -4, 0, 8, -4, -3, -2, 1, 4, -1},
                    {-1, -3, -2, -1, -4, -1, -1, -2, -2, -3, -4, -1, -3, -4, 10, -1, -1, -4, -3, -3},
                    {1, -1, 1, 0, -1, 0, -1, 0, -1, -3, -3, 0, -2, -3, -1, 5, 2, -4, -2, -2},
                    {0, -1, 0, -1, -1, -1, -1, -2, -2, -1, -1, -1, -1, -2, -1, 2, 5, -3, -2, 0},
                    {-3, -3, -4, -5, -5, -1, -3, -3, -3, -3, -2, -3, -1, 1, -4, -4, -3, 15, 2, -3},
                    {-2, -1, -2, -3, -3, -1, -2, -3, 2, -1, -1, -2, 0, 4, -3, -2, -2, 2, 8, -1},
                    {0, -3, -3, -4, -1, -3, -3, -4, -4, 4, 1, -3, 1, -1, -3, -2, 0, -3, -1, 5}
                    };



                Blosum62n = new SubstitutionMatrix();
                Blosum62n.Name = "Blosum62";
                Blosum62n.Scale = 1 / 2;
                Blosum62n.Entropy = 0.6979;
                Blosum62n.ExpectedScore = -0.5209;
                Blosum62n.HighestScore = 11;
                Blosum62n.LowestScore = -4;
                Blosum62n.Order = "ARNDCQEGHILKMFPSTWYVBZX*";
                Blosum62n.Matrix = new double[24, 24] {
                    {5, -2, -1, -2, -1, -1, -1, 0, -2, -1, -2, -1, -1, -3, -1, 1, 0, -3, -2, 0, -2, -1, -1, -5},
                    {-2, 7, -1, -2, -4, 1, 0, -3, 0, -4, -3, 3, -2, -3, -3, -1, -1, -3, -1, -3, -1, 0, -1, -5},
                    {-1, -1, 7, 2, -2, 0, 0, 0, 1, -3, -4, 0, -2, -4, -2, 1, 0, -4, -2, -3, 4, 0, -1, -5},
                    {-2, -2, 2, 8, -4, 0, 2, -1, -1, -4, -4, -1, -4, -5, -1, 0, -1, -5, -3, -4, 5, 1, -1, -5},
                    {-1, -4, -2, -4, 13, -3, -3, -3, -3, -2, -2, -3, -2, -2, -4, -1, -1, -5, -3, -1, -3, -3, -2, -5},
                    {-1, 1, 0, 0, -3, 7, 2, -2, 1, -3, -2, 2, 0, -4, -1, 0, -1, -1, -1, -3, 0, 4, -1, -5},
                    {-1, 0, 0, 2, -3, 2, 6, -3, 0, -4, -3, 1, -2, -3, -1, -1, -1, -3, -2, -3, 1, 5, -1, -5},
                    {0, -3, 0, -1, -3, -2, -3, 8, -2, -4, -4, -2, -3, -4, -2, 0, -2, -3, -3, -4, -1, -2, -2, -5},
                    {-2, 0, 1, -1, -3, 1, 0, -2, 10, -4, -3, 0, -1, -1, -2, -1, -2, -3, 2, -4, 0, 0, -1, -5},
                    {-1, -4, -3, -4, -2, -3, -4, -4, -4, 5, 2, -3, 2, 0, -3, -3, -1, -3, -1, 4, -4, -3, -1, -5},
                    {-2, -3, -4, -4, -2, -2, -3, -4, -3, 2, 5, -3, 3, 1, -4, -3, -1, -2, -1, 1, -4, -3, -1, -5},
                    {-1, 3, 0, -1, -3, 2, 1, -2, 0, -3, -3, 6, -2, -4, -1, 0, -1, -3, -2, -3, 0, 1, -1, -5},
                    {-1, -2, -2, -4, -2, 0, -2, -3, -1, 2, 3, -2, 7, 0, -3, -2, -1, -1, 0, 1, -3, -1, -1, -5},
                    {-3, -3, -4, -5, -2, -4, -3, -4, -1, 0, 1, -4, 0, 8, -4, -3, -2, 1, 4, -1, -4, -4, -2, -5},
                    {-1, -3, -2, -1, -4, -1, -1, -2, -2, -3, -4, -1, -3, -4, 10, -1, -1, -4, -3, -3, -2, -1, -2, -5},
                    {1, -1, 1, 0, -1, 0, -1, 0, -1, -3, -3, 0, -2, -3, -1, 5, 2, -4, -2, -2, 0, 0, -1, -5},
                    {0, -1, 0, -1, -1, -1, -1, -2, -2, -1, -1, -1, -1, -2, -1, 2, 5, -3, -2, 0, 0, -1, 0, -5},
                    {-3, -3, -4, -5, -5, -1, -3, -3, -3, -3, -2, -3, -1, 1, -4, -4, -3, 15, 2, -3, -5, -2, -3, -5},
                    {-2, -1, -2, -3, -3, -1, -2, -3, 2, -1, -1, -2, 0, 4, -3, -2, -2, 2, 8, -1, -3, -2, -1, -5},
                    {0, -3, -3, -4, -1, -3, -3, -4, -4, 4, 1, -3, 1, -1, -3, -2, 0, -3, -1, 5, -4, -3, -1, -5},
                    {-2, -1, 4, 5, -3, 0, 1, -1, 0, -4, -4, 0, -3, -4, -2, 0, 0, -5, -3, -4, 5, 2, -1, -5},
                    {-1, 0, 0, 1, -3, 4, 5, -2, 0, -3, -3, 1, -1, -4, -1, 0, -1, -2, -2, -3, 2, 5, -1, -5},
                    {-1, -1, -1, -1, -2, -1, -1, -2, -1, -1, -1, -1, -1, -2, -2, -1, 0, -3, -1, -1, -1, -1, -1, -5},
                    {-5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, -5, 1}
                    };

                Pam50n = new SubstitutionMatrix();
                Pam50n.Name = "PAM50";
                Pam50n.Scale = 1 / 2;
                Pam50n.Entropy = 2.00;
                Pam50n.ExpectedScore = -3.70;
                Pam50n.LowestScore = -13;
                Pam50n.HighestScore = 13;
                Pam50n.Order = "ARNDCQEGHILKMFPSTWYVBZX*";
                Pam50n.Matrix = new double[24, 24] {
                    {5, -5, -2, -2, -5, -3, -1, -1, -5, -3, -5, -5, -4, -7, 0, 0, 0, -11, -6, -1, -2, -2, -2, -13},
                    {-5, 8, -4, -7, -6, 0, -7, -7, 0, -4, -7, 1, -3, -8, -3, -2, -5, -1, -8, -6, -5, -2, -4, -13},
                    {-2, -4, 7, 2, -8, -2, -1, -2, 1, -4, -6, 0, -6, -7, -4, 1, -1, -7, -3, -6, 5, -1, -2, -13},
                    {-2, -7, 2, 7, -11, -1, 3, -2, -2, -6, -10, -3, -8, -12, -6, -2, -3, -12, -9, -6, 6, 2, -4, -13},
                    {-5, -6, -8, -11, 9, -11, -11, -7, -6, -5, -12, -11, -11, -10, -6, -2, -6, -13, -3, -5, -9, -11, -7, -13},
                    {-3, 0, -2, -1, -11, 8, 2, -5, 2, -6, -4, -2, -3, -10, -2, -4, -4, -10, -9, -5, -2, 6, -3, -13},
                    {-1, -7, -1, 3, -11, 2, 7, -3, -3, -4, -7, -3, -5, -11, -4, -3, -4, -13, -7, -5, 2, 6, -3, -13},
                    {-1, -7, -2, -2, -7, -5, -3, 6, -7, -8, -9, -6, -7, -8, -4, -1, -4, -12, -11, -4, -2, -4, -4, -13},
                    {-5, 0, 1, -2, -6, 2, -3, -7, 9, -7, -5, -4, -8, -5, -3, -4, -5, -6, -2, -5, 0, 0, -4, -13},
                    {-3, -4, -4, -6, -5, -6, -4, -8, -7, 8, 0, -5, 0, -1, -7, -5, -1, -11, -5, 3, -5, -5, -3, -13},
                    {-5, -7, -6, -10, -12, -4, -7, -9, -5, 0, 6, -6, 2, -1, -6, -7, -5, -5, -5, -1, -7, -5, -5, -13},
                    {-5, 1, 0, -3, -11, -2, -3, -6, -4, -5, -6, 6, -1, -11, -5, -3, -2, -9, -8, -7, -1, -2, -4, -13},
                    {-4, -3, -6, -8, -11, -3, -5, -7, -8, 0, 2, -1, 10, -3, -6, -4, -3, -10, -8, 0, -7, -4, -4, -13},
                    {-7, -8, -7, -12, -10, -10, -11, -8, -5, -1, -1, -11, -3, 9, -8, -5, -7, -3, 3, -6, -9, -11, -6, -13},
                    {0, -3, -4, -6, -6, -2, -4, -4, -3, -7, -6, -5, -6, -8, 8, -1, -3, -11, -11, -4, -5, -3, -4, -13},
                    {0, -2, 1, -2, -2, -4, -3, -1, -4, -5, -7, -3, -4, -5, -1, 6, 1, -4, -5, -4, -1, -3, -2, -13},
                    {0, -5, -1, -3, -6, -4, -4, -4, -5, -1, -5, -2, -3, -7, -3, 1, 6, -10, -5, -2, -2, -4, -2, -13},
                    {-11, -1, -7, -12, -13, -10, -13, -12, -6, -11, -5, -9, -10, -3, -11, -4, -10, 13, -4, -12, -8, -11, -9, -13},
                    {-6, -8, -3, -9, -3, -9, -7, -11, -2, -5, -5, -8, -8, 3, -11, -5, -5, -4, 9, -6, -5, -8, -6, -13},
                    {-1, -6, -6, -6, -5, -5, -5, -4, -5, 3, -1, -7, 0, -6, -4, -4, -2, -12, -6, 7, -6, -5, -3, -13},
                    {-2, -5, 5, 6, -9, -2, 2, -2, 0, -5, -7, -1, -7, -9, -5, -1, -2, -8, -5, -6, 5, 1, -3, -13},
                    {-2, -2, -1, 2, -11, 6, 6, -4, 0, -5, -5, -2, -4, -11, -3, -3, -4, -11, -8, -5, 1, 6, -3, -13},
                    {-2, -4, -2, -4, -7, -3, -3, -4, -4, -3, -5, -4, -4, -6, -4, -2, -2, -9, -6, -3, -3, -3, -4, -13},
                    {-13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, -13, 1},
                    };

                Pam150n = new SubstitutionMatrix();
                Pam150n.Name = "PAM150";
                Pam150n.Scale = 1 / 2;
                Pam150n.Entropy = 0.754;
                Pam150n.ExpectedScore = -1.25;
                Pam150n.LowestScore = -7;
                Pam150n.HighestScore = 12;
                Pam150n.Order = "ARNDCQEGHILKMFPSTWYVBZX*";
                Pam150n.Matrix = new double[24, 24] {
                    {3, -2, 0, 0, -2, -1, 0, 1, -2, -1, -2, -2, -1, -4, 1, 1, 1, -6, -3, 0, 0, 0, -1, -7},
                    {-2, 6, -1, -2, -4, 1, -2, -3, 1, -2, -3, 3, -1, -4, -1, -1, -2, 1, -4, -3, -2, 0, -1, -7},
                    {0, -1, 3, 2, -4, 0, 1, 0, 2, -2, -3, 1, -2, -4, -1, 1, 0, -4, -2, -2, 3, 1, -1, -7},
                    {0, -2, 2, 4, -6, 1, 3, 0, 0, -3, -5, -1, -3, -6, -2, 0, -1, -7, -4, -3, 3, 2, -1, -7},
                    {-2, -4, -4, -6, 9, -6, -6, -4, -3, -2, -6, -6, -5, -5, -3, 0, -3, -7, 0, -2, -5, -6, -3, -7},
                    {-1, 1, 0, 1, -6, 5, 2, -2, 3, -3, -2, 0, -1, -5, 0, -1, -1, -5, -4, -2, 1, 4, -1, -7},
                    {0, -2, 1, 3, -6, 2, 4, -1, 0, -2, -4, -1, -2, -6, -1, -1, -1, -7, -4, -2, 2, 4, -1, -7},
                    {1, -3, 0, 0, -4, -2, -1, 4, -3, -3, -4, -2, -3, -5, -1, 1, -1, -7, -5, -2, 0, -1, -1, -7},
                    {-2, 1, 2, 0, -3, 3, 0, -3, 6, -3, -2, -1, -3, -2, -1, -1, -2, -3, 0, -3, 1, 1, -1, -7},
                    {-1, -2, -2, -3, -2, -3, -2, -3, -3, 5, 1, -2, 2, 0, -3, -2, 0, -5, -2, 3, -2, -2, -1, -7},
                    {-2, -3, -3, -5, -6, -2, -4, -4, -2, 1, 5, -3, 3, 1, -3, -3, -2, -2, -2, 1, -4, -3, -2, -7},
                    {-2, 3, 1, -1, -6, 0, -1, -2, -1, -2, -3, 4, 0, -6, -2, -1, 0, -4, -4, -3, 0, 0, -1, -7},
                    {-1, -1, -2, -3, -5, -1, -2, -3, -3, 2, 3, 0, 7, -1, -3, -2, -1, -5, -3, 1, -3, -2, -1, -7},
                    {-4, -4, -4, -6, -5, -5, -6, -5, -2, 0, 1, -6, -1, 7, -5, -3, -3, -1, 5, -2, -5, -5, -3, -7},
                    {1, -1, -1, -2, -3, 0, -1, -1, -1, -3, -3, -2, -3, -5, 6, 1, 0, -6, -5, -2, -2, -1, -1, -7},
                    {1, -1, 1, 0, 0, -1, -1, 1, -1, -2, -3, -1, -2, -3, 1, 2, 1, -2, -3, -1, 0, -1, 0, -7},
                    {1, -2, 0, -1, -3, -1, -1, -1, -2, 0, -2, 0, -1, -3, 0, 1, 4, -5, -3, 0, 0, -1, -1, -7},
                    {-6, 1, -4, -7, -7, -5, -7, -7, -3, -5, -2, -4, -5, -1, -6, -2, -5, 12, -1, -6, -5, -6, -4, -7},
                    {-3, -4, -2, -4, 0, -4, -4, -5, 0, -2, -2, -4, -3, 5, -5, -3, -3, -1, 8, -3, -3, -4, -3, -7},
                    {0, -3, -2, -3, -2, -2, -2, -2, -3, 3, 1, -3, 1, -2, -2, -1, 0, -6, -3, 4, -2, -2, -1, -7},
                    {0, -2, 3, 3, -5, 1, 2, 0, 1, -2, -4, 0, -3, -5, -2, 0, 0, -5, -3, -2, 3, 2, -1, -7},
                    {0, 0, 1, 2, -6, 4, 4, -1, 1, -2, -3, 0, -2, -5, -1, -1, -1, -6, -4, -2, 2, 4, -1, -7},
                    {-1, -1, -1, -1, -3, -1, -1, -1, -1, -1, -2, -1, -1, -3, -1, 0, -1, -4, -3, -1, -1, -1, -1, -7},
                    {-7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, -7, 1},
                                    };

                Nuc44 = new SubstitutionMatrix();
                Nuc44.Name = "NUC4.4";
                Nuc44.Scale = 0.277316;
                Nuc44.Entropy = 0.5164710;
                Nuc44.ExpectedScore = -1.7495024;
                Nuc44.LowestScore = -4;
                Nuc44.HighestScore = 5;
                Nuc44.Order = "ACGTRYKMSWBDHVN";
                Nuc44.Matrix = new double[15, 15] {
                    {5, -4, -4, -4, 1, -4, -4, 1, -4, 1, -4, -1, -1, -1, -2},
                    {-4, 5, -4, -4, -4, 1, -4, 1, 1, -4, -1, -4, -1, -1, -2},
                    {-4, -4, 5, -4, 1, -4, 1, -4, 1, -4, -1, -1, -4, -1, -2},
                    {-4, -4, -4, 5, -4, 1, 1, -4, -4, 1, -1, -1, -1, -4, -2},
                    {1, -4, 1, -4, -1, -4, -2, -2, -2, -2, -3, -1, -3, -1, -1},
                    {-4, 1, -4, 1, -4, -1, -2, -2, -2, -2, -1, -3, -1, -3, -1},
                    {-4, -4, 1, 1, -2, -2, -1, -4, -2, -2, -1, -1, -3, -3, -1},
                    {1, 1, -4, -4, -2, -2, -4, -1, -2, -2, -3, -3, -1, -1, -1},
                    {-4, 1, 1, -4, -2, -2, -2, -2, -1, -4, -1, -3, -3, -1, -1},
                    {1, -4, -4, 1, -2, -2, -2, -2, -4, -1, -3, -1, -1, -3, -1},
                    {-4, -1, -1, -1, -3, -1, -1, -3, -1, -3, -1, -2, -2, -2, -1},
                    {-1, -4, -1, -1, -1, -3, -1, -3, -3, -1, -2, -1, -2, -2, -1},
                    {-1, -1, -4, -1, -3, -1, -3, -1, -3, -1, -2, -2, -1, -2, -1},
                    {-1, -1, -1, -4, -1, -3, -3, -1, -1, -3, -2, -2, -2, -1, -1},
                    {-2, -2, -2, -2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                };
            }
        }

    }

}
