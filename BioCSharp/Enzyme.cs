using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using BioCSharp.Seqs;
using BaseLibrary;
using System.Windows.Media;


namespace BioCSharp
{
    namespace Bio
    {
        /// <summary>
        /// cutting site
        /// </summary>
        public struct CuttingSite
        {
            /// <summary>
            /// Start point of the site in 5' strand. Negative indicate 3' strand
            /// </summary>
            public long Index;
            /// <summary>
            /// Length of the site. It can be positive, 0 or negative depending on 3' hang, blunt
            /// end or 5' hang.
            /// </summary>
            public long Length;

            /// <summary>
            /// Name of enzyme.
            /// 
            /// Use <code>Enzyme.EnzymeNamePattern</code> to get detail information about the enzyme.
            /// </summary>
            public string Enzyme;
        }


        /// <summary>
        /// </summary>
        public static class Enzyme
        {
            static Dictionary<string, Pattern> EnzymeNamePattern;

            /// <summary>
            /// Cuts a SEQ sequence into fragments at
            /// the restriction sites of restriction enzyme ENZYME where ENZYME is the
            /// name of a restriction enzyme from REBASE Version 510.
            ///  
            /// </summary>
            /// <param name="seq"></param>
            /// <param name="enzymeName"></param>
            /// <returns>cutting sites</returns>
            static public CuttingSite[] Restrict(BioCSharp.Seqs.Seq seq, string enzymeName)
            {
                // TODO: check whether it is neuclotide
                Pattern enzyme = EnzymeNamePattern[enzymeName];
                MatchCollection mc = enzyme.regexp.Matches(seq.Data.ToUpper());

                CuttingSite[] sites = new CuttingSite[mc.Count];
                int i = 0;
                foreach (Match m in mc)
                {
                    if (m.Success)
                    {
                        sites[i].Index = m.Index + enzyme.c1;
                        sites[i].Length = enzyme.c2 - enzyme.c1;
                        sites[i].Enzyme = enzymeName;
                    }
                    i++;
                }

                return sites;
            }

            static Enzyme()
            {
                if (!File.Exists(UserSetting.INSTANT.FileName_Rebase_Enzyme))
                {
                    throw new Exception("Unable to read the Rebase file.\n" + UserSetting.INSTANT.FileName_Rebase_Enzyme);
                }

                EnzymeNamePattern = new Dictionary<string, Pattern>(717);
                StreamReader reader = new StreamReader(UserSetting.INSTANT.FileName_Rebase_Enzyme);


                // skip header
                while (true)
                {
                    int firstChar = reader.Peek();
                    if (firstChar != 35) // 35 is '#', the header symbol in rebase file.
                        break;
                    reader.ReadLine(); // skip a line
                }

                char[] delimiters = new char[1] { '\t' };
                while (reader.Peek() > 0)
                {
                    string line = reader.ReadLine();
                    string[] tokens = line.Split();
                    if (tokens.Length == 1) continue; // skip empty line
                    if (tokens.Length < 9)
                    {
                        throw new Exception("Unable to read the Rebase file.\n" + UserSetting.INSTANT.FileName_Rebase_Enzyme);
                    }
                    Pattern pattern = new Pattern();
                    pattern.pattern = seq2regexp(tokens[1], true); // we work only with nucleotide
                    pattern.regexp = new Regex(pattern.pattern);
                    pattern.c1 = Int32.Parse(tokens[5]);
                    pattern.c2 = Int32.Parse(tokens[6]);
                    pattern.c3 = Int32.Parse(tokens[7]);
                    pattern.c4 = Int32.Parse(tokens[8]);

                    // FIXME: there are situation where one enzyme has more than one pattern
                    // Example: TaqII
                    if (!EnzymeNamePattern.ContainsKey(tokens[0]))
                        EnzymeNamePattern.Add(tokens[0], pattern);
                }
            }

            /// <summary>
            /// %SEQ2REGEXP converts extended NT or AA symbols into a regular expression.
            /// 
            ///    SEQ2REGEXP(SEQUENCE) converts extended nucleotide or amino acid symbols
            ///    in SEQUENCE into regular expression format.
            /// 
            ///    SEQ2REGEXP(...,'ALPHABET',type) specifies whether the sequence is amino
            ///    acids ('AA') or nucleotides ('NT'). The default is NT.
            /// 
            ///    SEQ2REGEXP(...,'AMBIGUOUS',false) removes the ambiguous characters from
            ///    the output regular expressions. This was the default behavior in older
            ///    versions of the Bioinformatics Toolbox.
            /// 
            ///    IUB/IUPAC nucleic acid code conversions:
            /// 
            ///    A --> A                   M --> [AC] (amino)
            ///    C --> C                   S --> [GC] (strong)
            ///    G --> G                   W --> [AT] (weak)
            ///    T --> T                   B --> [GTC]
            ///    U --> U                   D --> [GAT]
            ///    R --> [GA] (purine)       H --> [ACT]
            ///    Y --> [TC] (pyrimidine)   V --> [GCA]
            ///    K --> [GT] (keto)         N --> [AGCT] (any)
            /// 
            ///    Amino acid conversions:
            /// 
            ///    B --> [DN] 	aspartic acid or asparagine
            ///    Z --> [EQ]	glutamic acid or glutamine
            ///    X --> [ARNDCQEGHILKMFPSTWYV]
            /// 
            ///    Example:
            /// 
            ///       r = seq2regexp('ACWTMAN')
            /// 
            /// </summary>
            /// <param name="isNucleotide">true for nucleotide sequence</param>
            /// <param name="req">pattern sequence</param>
            /// <returns>regular expression</returns>
            private static string seq2regexp(string seq, bool isNucleotide)
            {
                string oSeq = seq.ToUpper();

                // Some databases use parentheses to demark quantifiers. This regexprep
                // corrects those statements to the standard curly braces without making a
                // global replacement of all parentheses.
                // uSeq = regexprep(out,'\((\d+,?\d*)\)','{$1}');
                oSeq = reg.Replace(oSeq, "{$1}");

                if (!isNucleotide)
                {
                    if (oSeq.Contains('R')) oSeq = oSeq.Replace("R", "[AG]");
                    if (oSeq.Contains('Y')) oSeq = oSeq.Replace("Y", "[CT]");
                    if (oSeq.Contains('M')) oSeq = oSeq.Replace("M", "[AC]");
                    if (oSeq.Contains('K')) oSeq = oSeq.Replace("K", "[GT]");
                    if (oSeq.Contains('S')) oSeq = oSeq.Replace("S", "[CG]");
                    if (oSeq.Contains('W')) oSeq = oSeq.Replace("W", "[AT]");
                    if (oSeq.Contains('B')) oSeq = oSeq.Replace("B", "[CGT]");
                    if (oSeq.Contains('D')) oSeq = oSeq.Replace("D", "[AGT]");
                    if (oSeq.Contains('H')) oSeq = oSeq.Replace("H", "[ACT]");
                    if (oSeq.Contains('V')) oSeq = oSeq.Replace("V", "[ACG]");
                    if (oSeq.Contains('N')) oSeq = oSeq.Replace("N", "[ACGT]");
                }
                else
                {
                    if (oSeq.Contains('B')) oSeq = oSeq.Replace("B", "[ND]");
                    if (oSeq.Contains('Z')) oSeq = oSeq.Replace("Z", "[QE]");
                    if (oSeq.Contains('X')) oSeq = oSeq.Replace("X", "[ARNDCQEGHILKMFPSTWYV]");
                }
                return oSeq;
            }
            static Regex reg = new Regex(@"\((\d+,?\d*)\)");

            /// <summary>
            /// Get user preferred color for enzyme  
            /// </summary>
            /// <param name="enzymeName"></param>
            /// <returns></returns>
            public static Color GetColor(string enzymeName)
            {
                int code = Math.Abs(enzymeName.GetHashCode());
                double r = ((int)((code / 100.0 - (int)(code / 100.0)) * 100.0)) / 100.0;
                double g = ((int)((code / 10000.0 - (int)(code / 10000.0)) * 10000.0)) / 10000.0;
                double b = ((int)((code / 1000000.0 - (int)(code / 1000000.0)) * 1000000.0)) / 1000000.0;

                return Color.FromRgb((byte)(r * 244), (byte)(g * 244), (byte)(b * 244));
            }

        }

        struct Pattern
        {
            public string pattern;
            public Regex regexp;
            public int c1;
            public int c2;
            public int c3;
            public int c4;
        }
    }
}
