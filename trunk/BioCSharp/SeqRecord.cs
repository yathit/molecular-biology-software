using System;
using System.Collections.Generic;
using System.Text;
using BioCSharp.Misc;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace BioCSharp
{
    namespace Seqs
    {
        /// <summary>
        /// A SeqRecord object consist sequence, its identifier (name) and optionally description.
        /// 
        ///     Main attributes:
        /// id          - Identifier such as a locus tag (string)
        /// seq         - The sequence itself (Seq object)
        /// 
        /// Additional attributes:
        /// name        - Sequence name, e.g. gene name (string)
        /// description - Additional text (string)
        /// </summary>
        public class SeqRecord : System.Windows.DependencyObject, ICloneable
        {
            protected Seq seq;
            protected string name;
            protected string description;


            /// <summary>
            /// Sequence
            /// </summary>
            public Seq Seq
            {
                get { return seq; }
                set { seq = value; }
            }

            public Alphabet Alphabet
            {
                get { return seq.Alphabet; }
            }

            /// <summary>
            /// Short name of sequence
            /// </summary>
            public string Name
            {
                get
                {
                    if (name != null && name.Length > 0) { return name; }
                    else if (description != null && description.Length > 0)
                    {
                        return description.Substring(0, Math.Max(255, description.Length));
                    }
                    else { return ""; }
                }
                set
                {
                    name = value;
                }
            }

            /// <summary>
            /// Get a character at a specific position
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            public char this[int i] { get { return seq[i]; } }


            /// <summary>
            /// Sequence description
            /// </summary>
            public string Description
            {
                get { return description == null ? "" : description; }
                set { description = value; }
            }

            /// <summary>
            /// Sequence. Same as <code>Data.Data</code>
            /// </summary>
            public string Sequence
            {
                get { return seq.Data; }
            }



            /// <summary>
            /// Number of character in the sequence.
            /// </summary>
            public int BaseCount { get { return this.Seq.Length; } }

            /// <summary>
            /// Length of sequence. Same as <code>Data.Length</code>. <seealso cref="SequenceLength"/>
            /// </summary>
            public int Length { get { return seq.Length; } }

            /// <summary>
            /// A string with the counts of bases for the sequence. <seealso cref="Length"/>
            /// </summary>
            virtual public string SequenceLength
            {
                get
                {
                    if (seq != null)
                    {
                        return seq.SequenceLength;
                    }
                    else { return ""; }
                }
            }

            public SeqRecord(string data, Alphabet alphabet, string name, string description)
            {
                if (alphabet == Alphabet.DNA)
                    this.seq = new DnaSeq(data);
                else if (alphabet == Alphabet.Protein)
                    this.seq = new ProteinSeq(data);
                else
                    this.seq = new Seq(data);

                this.name = name;
                this.description = description;
            }

            public SeqRecord(string data, string name, string description)
                : this(data, Seq.GuessAlphabet(data), name, description) { }

            /// <summary>
            /// A SeqRecord object holds a sequence and information about it
            /// </summary>
            /// <param name="seq">The sequence itself</param>
            /// <param name="name">Sequence name or identifier, e.g. gene name</param>
            /// <param name="description">Additional text</param>
            public SeqRecord(Seq seq, string name, string description)
            {
                this.seq = seq;
                this.description = description;
                this.name = name;
            }


            public SeqRecord(Seq seq, string name) : this(seq, name, null) { }

            public SeqRecord(string data, string name) :
                this(data, Seq.GuessAlphabet(data), name, null) { }

            public SeqRecord(Seq seq) : this(seq, null) { }

            public SeqRecord(string data) : this(data, Seq.GuessAlphabet(data), null, null) { }

            public SeqRecord()  { }

            /// <summary>
            /// Parse fasta and multiple alignment files.
            /// </summary>
            /// <param name="result"></param>
            /// <returns></returns>
            public static SeqRecord[] parse(SneakResult result)
            {
                if (result.FileType == FileType.Fasta)
                {
                    return FastaSeqRecord.parse(result.FileName);
                }
                else if (result.FileType == FileType.MutliAlign)
                {
                    return parseMultiAlign(result.FileName);
                }
                else
                {
                    throw new InvalidOperationException("Unknown file format");
                }
            }

            private static SeqRecord[] parseMultiAlign(string fileName)
            {
                StreamReader reader = File.OpenText(fileName);


                Regex tokens = new Regex(@"\S+");

                List<string> headers = new List<string>();
                List<string> sequences = new List<string>();
                List<int> count = new List<int>();

                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();
                    // skip empty line
                    if (line.Length == 0)
                        continue;

                    MatchCollection mc = tokens.Matches(line);
                    if ( mc.Count == 2)
                    {
                        if (!headers.Contains(mc[0].Value))
                        {
                            headers.Add(mc[0].Value);
                            sequences.Add(mc[1].Value);
                            count.Add(1);
                        }
                        else
                        {
                            int idx = headers.IndexOf(mc[0].Value);
                            sequences[idx] += mc[1].Value;
                            count[idx]++;
                        }

                    }
                    else
                    {
                        // skip
                    }


                }


                reader.Close();


                #region keep only multi align sequence in block
                // get mode
                Dictionary<int, int> mode = new Dictionary<int, int>();
                foreach (int i in count)
                {
                    if (mode.ContainsKey(i))
                        mode[i]++;
                    else
                        mode.Add(i, 1);
                }
                
                //foreach (int k in mode.Keys)
                //    Console.WriteLine("key: {0}, value: {1}", k, mode[k]);
                // get maximun mode
                int max = 0;
                foreach (int i in mode.Keys)
                    if (mode[i] > max)
                        max = i;
                

                // take those of maximun mode
                List<SeqRecord> seqs = new List<SeqRecord>();
                for (int i = 0; i < count.Count; ++i)
                {
                    if (count[i] == max)
                    {
                        seqs.Add(new SeqRecord(sequences[i], headers[i]));
                    }
                }


                #endregion



                return seqs.ToArray();
            }

            public static implicit operator SeqRecord(Seq seq)
            {
                return new SeqRecord(seq);
            }

            public static explicit operator Seq(SeqRecord record)
            {
                return record.Seq;
            }

            public object Clone()
            {
                // Seq is immutable
                SeqRecord r = new SeqRecord((Seq)seq, name, description);
                return r;
            }


            public void RemoveGap()
            {
                StringBuilder sb = new StringBuilder(seq.Length);
                foreach (char c in seq)
                {
                    if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                    {
                        sb.Append(c);
                    }
                }
            }
        }

        /// <summary>
        /// The class is used full to pase and write fasta file. <code>SeqRecord</code> also did the same
        /// but using this class ensure maximal consistency.
        /// </summary>
        public class FastaSeqRecord : SeqRecord
        {
            /// <summary>
            /// Title line ('>' character not included).
            /// </summary>
            protected string title;
            public static int COLWIDTH = 60;

            public readonly static Regex regIdSimple = new Regex(@"(\S+)", RegexOptions.IgnoreCase);

            /// <summary>
            /// Create a new Record.  colwidth specifies the number of residues
            /// to put on each line when generating FASTA format
            /// </summary>
            public FastaSeqRecord(Seq seq, string title)
                : base(seq, null, null)
            {
                this.title = title;
                this.seq = seq;
                Match m = regIdSimple.Match(title);
                if (m.Success && m.Groups[0].Success)
                {
                    base.name = m.Groups[0].Value;
                    description = title.Substring(m.Groups[0].Length);
                }
            }

            public FastaSeqRecord(string sequence, string title)  
            {
                this.title = title;
                Alphabet alpha = Seq.GuessAlphabet(sequence);
                if (alpha == Alphabet.DNA)
                    seq = new DnaSeq(sequence);
                else if (alpha == Alphabet.Protein)
                    seq = new ProteinSeq(sequence);
                else
                    seq = new Seq(sequence);
                        
            }

            public override string ToString()
            {
                StringBuilder s = new StringBuilder(String.Format(">{0}\n", title));
                for (int i = 0; i < seq.Length; i += COLWIDTH)
                {
                    s.Append(seq.Data.Substring(i, COLWIDTH));
                    s.Append("\n");
                }

                return s.ToString();
            }

            public static FastaSeqRecord[] parse(string fileName)
            {
                StreamReader reader = new StreamReader(fileName);

                List<FastaSeqRecord> records = new List<FastaSeqRecord>();

                StringBuilder sb = new StringBuilder();
                string title = null;
                while (true)
                {
                    int peek = reader.Peek();
                    if (peek < 0) { break; }
                    string line = reader.ReadLine();
                    if (peek == 62) // 62 = '>'
                    {
                        if (title != null)
                        {
                            sb.Replace("\t", "").Replace(" ", "");
                            records.Add(new FastaSeqRecord(sb.ToString(), title));
                        }
                        title = line.Substring(1);
                        sb = new StringBuilder();
                    }
                    else
                    {
                        sb.Append(line);
                    }
                }
                if (title != null)
                {
                    sb.Replace("\t", "").Replace(" ", "");
                    records.Add(new FastaSeqRecord(sb.ToString(), title));
                }
                return records.ToArray();

            }
        }

        /// <summary>
        /// A Biosequence is a single continuous feature annotated biological sequence. 
        /// It can be nucleic acid or protein. 
        /// 
        /// Additional attributes:
        /// name        - Sequence name, e.g. gene name (string)
        /// description - Additional text (string)
        ///  dbxrefs     - List of database cross references (list of strings)
        /// features    - Any (sub)features defined (list of SeqFeature objects)
        ///  annotations - Further information about the whole sequence (dictionary)
        /// </summary>
        public class BioSequence : SeqRecord
        {
            protected internal KeyValuePair<string, string>[] dbxrefs;
            protected internal Feature[] features;

            /// <summary>
            /// A listing of Features making up the feature table.
            /// </summary>
            public Feature[] Features
            {
                get { return features == null ? new Feature[0] : features; }
            }



            /// <summary>
            /// Database reference accession
            /// </summary>
            public KeyValuePair<string, string>[] Xrefs
            {
                get { return dbxrefs == null ? new KeyValuePair<string, string>[0] : dbxrefs; }
            }


            /// <summary>
            /// A string with the counts of bases for the sequence.
            /// </summary>
            override public string SequenceLength
            {
                get
                {
                    if (seq != null)
                    {
                        return seq.SequenceLength;
                    }
                    else { return ""; }
                }
            }

            /// <summary>
            /// A SeqRecord object holds a sequence and information about it
            /// </summary>
            /// <param name="seq">The sequence itself</param>
            /// <param name="name">Sequence name, e.g. gene name</param>
            /// <param name="description">Additional text</param>
            /// <param name="dbxrefs">List of database cross references</param>
            /// <param name="features">Any (sub)features defined</param>
            public BioSequence(Seq seq, string name, string description, KeyValuePair<string, string>[] dbxrefs, Feature[] features)
                : base(seq, name, description)
            {
                this.dbxrefs = dbxrefs;
                this.features = features;
            }

            public BioSequence(Seq seq) : this(seq, "", "", null, null) { }

        }


        /// <summary>
        /// Represent Feature element  including feature keys, qualifiers, accession numbers, 
        /// database name abbreviations, feature labels, and location operators, are all named 
        /// following the same conventions.
        /// </summary>
        public class Feature
        {
            #region constants for feature keys and qualifiers
            // 1. misc_difference
            public const string KEY_misc_difference = "misc_difference";
            public const string KEY_conflict = "conflict";
            public const string KEY_unsure = "unsure";
            public const string KEY_old_sequence = "old_sequence";
            public const string KEY_variation = "variation";
            public const string KEY_modified_base = "modified_base";
            // 2. gene
            public const string KEY_gene = "gene";
            // 3. misc_signal
            // a) promoter
            public const string KEY_promoter = "promoter";
            public const string KEY_CAAT_signal = "CAAT_signal";
            public const string KEY_TATA_signal = "TATA_signal";
            public const string KEY_GC_minus35 = "-35";
            public const string KEY_GC_minus10 = "-10";
            public const string KEY_GC_signal = "GC_signal";
            // b) RBS
            public const string KEY_RBS = "RBS";
            public const string KEY_polyA_signal = "polyA_signal";
            public const string KEY_enhancer = "enhancer";
            public const string KEY_attenuator = "attenuator";
            public const string KEY_terminator = "terminator";
            public const string KEY_rep_origin = "rep_origin";
            public const string KEY_oriT = "oriT";
            public const string KEY_prim_transcript = "prim_transcript";
            //  4. misc_RNA 
            public const string KEY_misc_RNA = "misc_RNA";
            // a) prim_transcript
            public const string KEY_precursor_RNA = "precursor_RNA";
            public const string KEY_mRNA = "mRNA";
            public const string KEY_5clip = "5'clip ";
            public const string KEY_3clip = "3'clip ";
            public const string KEY_5UTR = "5'UTR";
            public const string KEY_3UTR = "3'UTR";
            public const string KEY_exon = "exon";
            public const string KEY_CDS = "CDS";
            public const string KEY_sig_peptide = "sig_peptide";
            public const string KEY_transit_peptide = "transit_peptide";
            public const string KEY_mat_peptide = "mat_peptide";
            public const string KEY_intron = "intron";
            public const string KEY_polyA_site = "polyA_site";
            public const string KEY_rRNA = "rRNA";
            public const string KEY_tRNA = "tRNA";
            public const string KEY_scRNA = "scRNA";
            public const string KEY_snRNA = "snRNA";
            public const string KEY_snoRNA = "snoRNA";
            // 5. Immunogobulin related 
            public const string KEY_C_region = "C_region";
            public const string KEY_D_segment = "D_segment";
            public const string KEY_J_segment = "J_segment";
            public const string KEY_N_region = "N_region";
            public const string KEY_S_region = "S_region";
            public const string KEY_V_region = "V_region";
            public const string KEY_V_segment = "V_segment";
            // 6. repeat_region
            public const string KEY_repeat_unit = "repeat_unit";
            public const string KEY_LTR = "LTR";
            public const string KEY_satellite = "satellite";
            //  7. misc_binding 
            public const string KEY_misc_binding = "misc_binding";
            public const string KEY_primer_bind = "primer_bind";
            public const string KEY_protein_bind = "protein_bind";
            //  8. misc_recomb 
            public const string KEY_iDNA = "iDNA";
            // 9. misc_structure
            public const string KEY_stem_loop = "stem_loop";
            public const string KEY_Dloop = "D-loop";
            // 10. gap
            public const string KEY_gap = "gap";
            // 11. operon
            public const string KEY_operon = "operon";

            public const string KEY_misc_feature = "misc_feature";
            public const string KEY_source = "source";
            public const string KEY_Promoter = "promoter";
            public const string KEY_Rep_Origin = "rep_origin";
            public const string KEY_unknown = BioCSharp.Misc.Misc.UNKNOWN;


            /// <summary>
            /// Restriction site, this is non standard qualifier 
            /// </summary>
            public const string KEY_restriction_site = "restric_site";

            /// <summary>
            /// List of "feature key" 
            /// </summary>
            public readonly static string[] KEYS = new string[] {
                KEY_misc_difference,
                KEY_conflict,
                KEY_unsure,
                KEY_old_sequence,
                KEY_variation,
                KEY_modified_base,
                // 2. gene
                KEY_gene,
                // 3. misc_signal
                // a) promoter
                KEY_promoter,
                KEY_CAAT_signal,
                KEY_TATA_signal,
                KEY_GC_minus35,
                KEY_GC_minus10,
                KEY_GC_signal,
                // b) RBS
                KEY_RBS,
                KEY_polyA_signal,
                KEY_enhancer,
                KEY_attenuator,
                KEY_terminator,
                KEY_rep_origin,
                KEY_oriT,
                KEY_prim_transcript,
                //  4. misc_RNA 
                KEY_misc_RNA,
                // a) prim_transcript
                KEY_precursor_RNA,
                KEY_mRNA,
                KEY_5clip,
                KEY_3clip,
                KEY_5UTR,
                KEY_3UTR,
                KEY_exon,
                KEY_CDS,
                KEY_sig_peptide,
                KEY_transit_peptide,
                KEY_mat_peptide,
                KEY_intron,
                KEY_polyA_site,
                KEY_rRNA,
                KEY_tRNA,
                KEY_scRNA,
                KEY_snRNA,
                KEY_snoRNA,
                // 5. Immunogobulin related 
                KEY_C_region,
                KEY_D_segment,
                KEY_J_segment,
                KEY_N_region,
                KEY_S_region,
                KEY_V_region,
                KEY_V_segment,
                // 6. repeat_region
                KEY_repeat_unit,
                KEY_LTR,
                KEY_satellite,
                //  7. misc_binding 
                KEY_misc_binding,
                KEY_primer_bind,
                KEY_protein_bind,
                //  8. misc_recomb 
                KEY_iDNA,
                // 9. misc_structure
                KEY_stem_loop,
                KEY_Dloop,
                // 10. gap
                KEY_gap,
                // 11. operon
                KEY_operon,

                KEY_misc_feature,
                KEY_source,
                KEY_Promoter,
                KEY_Rep_Origin,
                KEY_unknown
                };

            public const string QUALIFIER_label = "label";
            public const string QUALIFIER_note = "note";
            public const string QUALIFIER_allele = "allele";
            public const string QUALIFIER_citation = "citation";
            public const string QUALIFIER_codon = "codon";
            public const string QUALIFIER_codon_start = "codon_start";
            public const string QUALIFIER_db_xref = "db_xref";
            public const string QUALIFIER_EC_number = "EC_number";
            public const string QUALIFIER_exception = "exception";
            public const string QUALIFIER_experiment = "experiment";
            public const string QUALIFIER_function = "function";
            public const string QUALIFIER_gene = "gene";
            public const string QUALIFIER_inference = "inference";
            public const string QUALIFIER_locus_tag = "locus_tag";
            public const string QUALIFIER_map = "map";
            public const string QUALIFIER_number = "number";
            public const string QUALIFIER_old_locus_tag = "old_locus_tag";
            public const string QUALIFIER_operon = "operon";
            public const string QUALIFIER_product = "product";
            public const string QUALIFIER_protein_id = "protein_id";
            public const string QUALIFIER_pseudo = "pseudo";
            public const string QUALIFIER_ribosomal_slippage = "ribosomal_slippage";
            public const string QUALIFIER_standard_name = "standard_name";
            public const string QUALIFIER_translation = "translation";
            public const string QUALIFIER_transl_except = "transl_except";
            public const string QUALIFIER_transl_table = "transl_table";
            public const string QUALIFIER_trans_splicing = "trans_splicing";
            public const string QUALIFIER_phenotype = "phenotype";
            public const string QUALIFIER_PCR_conditions = "PCR_conditions";
            public const string QUALIFIER_bound_moiety = "bound_moiety";


            #endregion

            private readonly static string[] QualifierLabelList = {QUALIFIER_label, QUALIFIER_gene, QUALIFIER_product, QUALIFIER_locus_tag, 
                                                                QUALIFIER_note, QUALIFIER_db_xref, QUALIFIER_protein_id};


            //public static readonly List<string> KEYS;


            protected internal string key;
            protected internal string location;
            protected internal Dictionary<string, string> qualifiers;
            protected internal string label = null;
           

            public Feature() { }


            public Feature(string key, string location, Dictionary<string, string> qualifiers)
            {
                this.key = key;
                this.location = location;
                this.qualifiers = qualifiers;
            }

            public Feature(string key, string location, List<KeyValuePair<string, string>> qualifiers)
            {
                this.key = key;
                this.location = location;
                this.qualifiers = new Dictionary<string, string>(qualifiers.Count);
                foreach (KeyValuePair<string, string> kvp in qualifiers)
                {
                    if (this.qualifiers.ContainsKey(kvp.Key))
                    {
                        string value = this.qualifiers[kvp.Key] + ", " + kvp.Value;
                        this.qualifiers.Remove(kvp.Key);
                        this.qualifiers.Add(kvp.Key, value);
                    }
                    else
                    {
                        this.qualifiers.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            public Feature(string key, string location, List<string> qualifier)
            {
                this.key = key;
                this.location = location;

                // reading buffer
                string qKey = null;
                string qValue = "";
                this.qualifiers = new Dictionary<string, string>(qualifier.Count);

                foreach (string line in qualifier)
                {
                    if (line.StartsWith("/")) // qualifier start
                    {
                        // save reading buffer
                        if (qKey != null)
                        {
                            if (qualifiers.ContainsKey(qKey))
                            {
                                qValue = qualifiers[qKey] + ", " + qValue;
                                qualifiers.Remove(qKey);
                                qualifiers.Add(qKey, qValue);
                            }
                            else
                            {
                                qualifiers.Add(qKey, qValue);
                            }
                        }
                        int equal = line.IndexOf('=');
                        if (equal >= 1)
                        {
                            qKey = line.Substring(1, equal - 1);
                            if (line.Length > equal)
                            {
                                qValue = line.Substring(equal + 1);
                            }
                            int firstQuote = line.IndexOf('"');
                            if (firstQuote >= 0) // check multi-line feature
                            {
                                if (line.IndexOf('"', firstQuote + 1) < 0)
                                {
                                    // no second quote, must be multiple line qualifiers
                                    continue;
                                }
                            }
                        }
                        else // may be only key
                        {
                            qKey = line.Substring(1);
                            qValue = "";
                        }
                        if (qualifiers.ContainsKey(qKey))
                        {
                            qValue = qualifiers[qKey] + ", " + qValue;
                            qualifiers.Remove(qKey);
                            qualifiers.Add(qKey, qValue);
                        }
                        else
                        {
                            qualifiers.Add(qKey, qValue);
                        }
                        qKey = null;
                        qValue = "";
                    }
                    else // continuation
                    {
                        qValue += line.Trim();
                    }
                }

                if (qKey != null)
                {
                    if (qualifiers.ContainsKey(qKey))
                    {
                        qValue = qualifiers[qKey] + ", " + qValue;
                        qualifiers.Remove(qKey);
                        qualifiers.Add(qKey, qValue);
                    }
                    else
                    {
                        qualifiers.Add(qKey, qValue);
                    }
                }
            }


            virtual public string Location
            {
                get { return location; }
                set { location = value; }
            }
            public string Key
            {
                get { return key; }
                set { key = value; }
            }
            public Dictionary<string, string> Qualifiers
            {
                get { return qualifiers; }
                set { qualifiers = value; }
            }

            public string ToolTip
            {
                get
                {
                    // StringBuilder sb = new StringBuilder(Key);
                    //foreach (string qf in qualifiers.Keys)
                    //{
                    //    string s = qualifiers[qf];
                    //    if (s.Length > 60)
                    //        s = s.Substring(0, 59) + "...";
                    //    sb.Append(String.Format("\n{0:G}: {1:G}", qf, s));
                    //}
                    //return sb.ToString();
                    return Label;
                }
            }

            /// <summary>
            /// A qualifier to use it as label for this feature
            /// </summary>
            public string Label
            {
                get
                {
                    if (label == null)
                    {
                        label = Key;
                        foreach (string qualifier in QualifierLabelList)
                        {
                            if (qualifiers.ContainsKey(qualifier))
                            {
                                label = qualifiers[qualifier].Trim();
                                // remove quotes
                                if (label.StartsWith("\"")) label = label.Substring(1);
                                if (label.EndsWith("\"")) label = label.Substring(0, label.Length - 1);
                                break;
                            }
                        }
                    }
                    return label;
                }
            }

            /// <summary>
            /// Deep copy constructor
            /// </summary>
            /// <returns></returns>
            internal Feature Clone()
            {
                Dictionary<string, string> dict = new Dictionary<string, string>(this.qualifiers.Count);
                foreach (string key in this.qualifiers.Keys)
                {
                    dict.Add(key, this.qualifiers[key]);
                }
                return new Feature(this.key, this.location, dict);
            }
        }



    }

}
