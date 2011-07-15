using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace BioCSharp
{

    namespace Seqs
    {
        /// <summary>
        /// # The "brains" for parsing GenBank and EMBL files (and any
        /// # other flat file variants from the INSDC in future) is in
        /// # Bio.GenBank.Scanner (plus the _FeatureConsumer in Bio.GenBank)
        /// #
        /// # See also
        /// # ========
        /// # International Nucleotide Sequence Database Collaboration
        /// # http://www.insdc.org/
        /// </summary>
        public class InsdcRecord : SeqRecord, ICloneable
        {
            // protected internal string locusName; // id
            protected internal Dictionary<string, string> keys;
            /// <summary>
            /// Main key entery
            /// <seealso cref="ListKeys"/>
            /// </summary>
            public Dictionary<string, string> Keys { get { return keys; } }
            protected internal Dictionary<string, List<string>> listKeys;
            /// <summary>
            /// Main key entry with multiple values
            /// 
            /// <seealso cref="Keys"/>
            /// </summary>
            public Dictionary<string, List<string>> ListKeys { get { return listKeys; } }

            // these key are as defined in the XML version of INSD-Seq Version 1.4
            // INSDSeq_locus was occupied by Seq.id
            public const string INSDSeq_length = "locusSequenceLength";
            public const string INSDSeq_strandedness = "locusNumberofStrands";
            public const string INSDSeq_moltype = "locusTopology";
            public const string INSDSeq_topology = "locusMoleculeType";
            public const string INSDSeq_update_date = "locusModificationDate";
            public const string INSDSeq_division = "locusGenBankDivision";

            public const string INSDSeq_definition = "DEFINITION"; // also occupy by SeqRecord.defination

            public const string LISTKEY_ACCESSION = "ACCESSION";

            public const string KEY_VERSION = "VERSION";
            public const string KEY_VERSION_GI = "VERSION_GI"; // think about storing as long

            public const string KEY_PROJECT = "PROJECT";

            public const string LISTKEY_KEYWORDS = "KEYWORDS";

            public const string KEY_SEGMENT = "SEGMENT";

            public const string KEY_SOURCE = "SOURCE";

            public const string KEY_ORGANISM = "ORGANISM";

            public const string KEY_REFERENCE = "REFERENCE";

            public const string KEY_COMMENT = "COMMENT";

            public readonly static string[] KEYS = new string[] { 
                INSDSeq_length, 
                INSDSeq_strandedness,
                INSDSeq_moltype,
                INSDSeq_topology,
                INSDSeq_update_date,
                INSDSeq_division,
                INSDSeq_definition,
                KEY_VERSION,
                KEY_VERSION_GI,
                KEY_PROJECT,
                LISTKEY_KEYWORDS,
                KEY_SEGMENT,
                KEY_SOURCE,
                KEY_ORGANISM,
                KEY_REFERENCE,
                KEY_COMMENT
            };

            private Feature[] features;

            /// <summary>
            /// Number of features
            /// </summary>
            virtual public int FeatureCount
            {
                get { return features == null ? 0 : features.Length; }
            }

            /// <summary>
            /// Get list of features.
            /// <seealso cref="FeatureKeys"/>
            /// </summary>
            virtual public Feature[] Features
            {
                get { return features; }
            }

            protected void SetFeatures(List<Feature> value)
            {
                features = value.ToArray();
                _featureKeys_catch = null;
            }

            protected void AddFeatures(List<Feature> fts)
            {
                if (fts.Count == 0) return;
                Feature[] old = features;
                features = new Feature[old.Length + fts.Count];
                old.CopyTo(features, 0);
                for (int i = 0; i < fts.Count; i++)
                {
                    features[i + old.Length] = fts[i];
                }
                _featureKeys_catch = null;
            }


            /// <summary>
            /// Get list of Keys in Features (<code>Feature.Key</code>)
            /// </summary>
            virtual public string[] FeatureKeys
            {
                get
                {
                    // check catch is valid
                    if (_featureKeys_catch == null)
                    {
                        _featureKeys_catch = features.Select(f => f.Key).ToArray();
                    }

                    return _featureKeys_catch;
                }
                
            }
            private string[] _featureKeys_catch;


            /// <summary>
            /// The "brains" for parsing GenBank and EMBL files (and any other flat file variants from the INSDC).
            /// 
            /// This is primary data in <code>SeqCanvas</code>.
            /// </summary>
            /// <param name="data"></param>
            /// <param name="name"></param>
            public InsdcRecord(Seq data, string name)
                : base(data, name)
            {
                keys = new Dictionary<string, string>(20);
                listKeys = new Dictionary<string, List<string>>(2);
            }

            protected internal InsdcRecord()
                : this(null, null) { }


            /// <summary>
            /// Complete constructure
            /// </summary>
            /// <param name="data"></param>
            /// <param name="name"></param>
            /// <param name="description"></param>
            /// <param name="features"></param>
            /// <param name="keys"></param>
            /// <param name="listkeys"></param>
            public InsdcRecord(Seq data, string name, string description, Feature[] features,
                Dictionary<string, string> keys, Dictionary<string, List<string>> listkeys)
            {
                this.seq = data;
                this.name = name;
                this.description = description;
                this.features = features;
                this.keys = keys;
                this.listKeys = listkeys;
            }

            /// <summary>
            /// Deep copy constructor
            /// </summary>
            /// <param name="record"></param>
            public object Clone()
            {
                InsdcRecord record = new InsdcRecord();
                record.description = description;
                record.name = name;
                record.seq = seq;
                record.features = new Feature[FeatureCount];
                features.CopyTo(record.features, 0);               
                record.keys = new Dictionary<string, string>(keys.Count);
                foreach (string key in keys.Keys)
                {
                    record.keys.Add(key, keys[key]);
                }
                record.listKeys = new Dictionary<string, List<string>>();
                foreach (string key in listKeys.Keys)
                {
                    record.listKeys.Add(key, (List<string>) listKeys[key].ToArray().Clone());
                }
                return record;
            }


        }
    }
}
