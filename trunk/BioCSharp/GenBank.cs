using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BioCSharp.Seqs;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace BioCSharp
{

    /// <summary>
    /// Annotated sequence data. All GeneBank data and additional data are allocated for use in <code>SeqCanvas</code>
    /// Code to work with GenBank formatted files.
    /// </summary>
    namespace Seqs
    {

        /// <summary>
        /// GenBank record
        /// </summary>
        public class GenBank : InsdcRecord
        {

            const int HEADER_WIDTH = 12;
            const int FEATURE_QUALIFIER_INDENT = 16;
            const int FEATURE_INDENT = 6;
            const int SEQUENCE_INDENT = 10;

            readonly static Regex regDefination = new Regex("DEFINITION\\s+(\\w|\\W)+");

            private GenBank() : base() { }


            /// <summary>
            /// Parse GenBank record file
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static GenBank[] parse(string fileName) 
            {
                List<GenBank> records = new List<GenBank>(1); // generally only one record

                GenBank record = null;

                StreamReader reader = new StreamReader(fileName);
                try
                {
                    string line = null;
                    while (reader.Peek() >= 0)
                    {
                        if (line == null)
                        {
                            line = reader.ReadLine();
                        }
                        if (line.Trim().Length == 0) { line = null; continue; } // skip blank line
                        string head = line.Length >= HEADER_WIDTH ? line.Substring(0, HEADER_WIDTH - 1).Trim() : line.Trim();
                        string body = line.Length > HEADER_WIDTH ? line.Substring(HEADER_WIDTH) : "";

                        switch (head)
                        {
                            case "LOCUS": // LOCUS - Mandatory 
                                if (record != null && record.Name != null)
                                {
                                    records.Add(record);
                                }
                                record = new GenBank();
                                record.Name = line.Substring(12, 15); //  %#ok
                                record.Keys.Add(InsdcRecord.INSDSeq_length, line.Substring(29, 10).Trim());
                                record.Keys.Add(InsdcRecord.INSDSeq_strandedness, line.Substring(44, 2).Trim());
                                record.Keys.Add(InsdcRecord.INSDSeq_moltype, line.Substring(55, 7).Trim());
                                record.Keys.Add(InsdcRecord.INSDSeq_topology, line.Substring(47, 5).Trim());
                                if (line.Length > 66) record.Keys.Add(InsdcRecord.INSDSeq_division, line.Substring(64, 2).Trim());
                                if (line.Length > 98) record.Keys.Add(InsdcRecord.INSDSeq_update_date, line.Substring(68, 10).Trim());
                                line = null;
                                break;
                            case "DEFINITION":
                                record.description = body;
                                record.Keys.Add(InsdcRecord.INSDSeq_definition, body);
                                line = null;
                                break;
                            case "FEATURES":
                                List<string> fsLines = new List<string>();
                                while (!reader.EndOfStream)
                                {
                                    line = reader.ReadLine();
                                    if (line.Length == 0) { continue; }
                                    if (line.Length < FEATURE_INDENT || line.StartsWith("ORIGIN"))
                                    {
                                        break;
                                    }
                                    fsLines.Add(line.Substring(FEATURE_INDENT - 1));
                                }
                                record.SetFeatures(extract_feature(fsLines));
                                break;
                            case "ORIGIN": // SEQUENCE
                                StringBuilder sb = new StringBuilder();
                                while (reader.Peek() >= 0)
                                {
                                    line = reader.ReadLine();
                                    if (line.StartsWith("L") || line.StartsWith("/")) // L for LOCUS and / for //
                                    {
                                        break;
                                    }
                                    sb.Append(line.Substring(SEQUENCE_INDENT));
                                }
                                string molType = record.Keys[InsdcRecord.INSDSeq_topology];
                                if (molType.Equals("DNA"))
                                {
                                    record.seq = new DnaSeq(sb.ToString());
                                }
                                else if (molType.Equals("AA"))
                                {
                                    record.seq = new ProteinSeq(sb.ToString());
                                }
                                else
                                {
                                    // request Seq to guess the alphabet
                                    record.seq = Seq.Create(sb.ToString());
                                }
                                break;
                            default:
                                line = null; // just consume
                                break;
                        }
                    }

                    if (record != null && record.Name != null)
                    {
                        records.Add(record);
                    }
                }
                finally
                {
                    reader.Close();
                }

                return records.ToArray();
            }

            /// <summary>
            /// Extract the feature information
            /// typically CDS, gene, mRNA
            /// </summary>
            /// <param name="lines">List of feature string without initial blank (but do not Trim)</param>
            /// <returns></returns>
            static public List<Feature> extract_feature(List<string> lines)
            {
                List<Feature> features = new List<Feature>();

                string key = null;
                string location = "";
                List<string> qualifiers = new List<string>();
                foreach (string line in lines)
                {
                    if (line.Length < FEATURE_QUALIFIER_INDENT) { continue; }
                    string head = line.Substring(0, FEATURE_QUALIFIER_INDENT).Trim();
                    if (head.Length == 0) // qualifiers
                    {
                        qualifiers.Add(line.Substring(FEATURE_QUALIFIER_INDENT));
                    }
                    else  // feature key
                    {
                        if (key != null)
                        {
                            features.Add(new Feature(key, location, qualifiers));
                            key = null;
                            location = "";
                            qualifiers = new List<string>();
                        }
                        if (Feature.KEYS.Contains(head))
                        {
                            // BioApp.Window1.log(ft, head);
                            key = head;
                        }
                        else
                        {
                            // this is not suppose to be. New feature?
                            // BioApp.Window1.log(ft, "unknown: " + head);
                            key = Feature.KEY_unknown;
                        }
                        // read location
                        location = line.Substring(FEATURE_QUALIFIER_INDENT);
                    }
                }

                return features;
            }
        }


    }

}
