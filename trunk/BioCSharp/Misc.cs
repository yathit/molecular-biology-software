using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BioCSharp;
using System.Text.RegularExpressions;
using BioCSharp.Seqs;

namespace BioCSharp
{
    namespace Misc
    {
        public enum SneakAnswer { UnknownFormat, OK, Error, Ambigious }
        public enum FileType { Unknown, GenBank, Fasta, PlainSequence, MutliAlign, Trace }
        public enum LocusTopology { Unknown, Linear, Circular }
        public enum Quantity { Single, Multiple }

        public struct SneakResult
        {
            public FileType FileType;
            public SneakAnswer Result;
            public Alphabet Alphabet;
            public string Name;
            public int BaseCount;
            public LocusTopology Topology;
            public Quantity Quantity;
            public string FileName;

            public SneakResult(int BaseCount, string Name)
            {
                this.Topology = LocusTopology.Unknown;
                this.BaseCount = BaseCount;
                this.Name = Name;
                FileType = FileType.GenBank;
                Result = SneakAnswer.OK;
                Alphabet = Alphabet.DNA;
                Quantity = Quantity.Single;
                FileName = Name;
            }
        }

        public static class Misc
        {
            public const string UNKNOWN = "unknown";

            static private Regex reg = new Regex(@"\d+");

            /// <summary>
            /// Quickly examine given text file for known file format.
            /// 
            /// Known file format: GenBank, Fasta
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static SneakResult Sneak(string fileName)
            {
                SneakResult result = new SneakResult();
                result.Result = SneakAnswer.Error;
                result.FileName = fileName;

                if (fileName.EndsWith(".scf")) // sequence trace file
                {
                    result.FileType = FileType.Trace;
                    result.Result = SneakAnswer.OK;
                    result.Alphabet = Alphabet.DNA;
                    result.Quantity = Quantity.Single;
                    return result;
                }

                StreamReader reader = new StreamReader(fileName);

                try
                {
                    // skip blank files
                    string line = "";
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        if (line.Trim().Length > 0)
                            break;
                    }

                    
                    // see if GenBank format
                    if (line.StartsWith("LOCUS"))
                    {
                        result.FileType = FileType.GenBank;

                        string[] tokens = line.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        List<string> content = new List<string>(tokens);

                        if (tokens.Length > 5)
                        {
                            result.Result = SneakAnswer.OK;
                            result.Name = tokens[1];
                            string basePair = tokens[2];
                            try
                            {
                                Match m = reg.Match(tokens[2]);
                                int x = 0;
                                if (m.Success)
                                    if (Int32.TryParse(m.Value, out x))
                                    result.BaseCount =  x;
                            }
                            catch
                            {
                                result.Result = SneakAnswer.Ambigious;
                            }
                        }
                        else
                        {
                            result.Result = SneakAnswer.Ambigious;
                        }

                        if (content.Contains("DNA"))
                            result.Alphabet = Alphabet.DNA;
                        else if (content.Contains("RNA"))
                            result.Alphabet = Alphabet.RNA;
                        else if (content.Contains("Protein") || content.Contains("protein"))
                            result.Alphabet = Alphabet.Protein;
                        else
                            result.Alphabet = Alphabet.Unknown;

                        if (content.Contains("circular"))
                            result.Topology = LocusTopology.Circular;
                        else if (content.Contains("linear"))
                            result.Topology = LocusTopology.Linear;
                        else
                            result.Topology = LocusTopology.Unknown;

                        

                        result.Quantity = Quantity.Single;
                        while (!reader.EndOfStream)
                        {
                            line = reader.ReadLine();
                            if (line.StartsWith("LOCUS"))
                            {
                                result.Quantity = Quantity.Multiple;
                                break;
                            }
                        }

                    }
                    else if (line.StartsWith(">"))
                    {
                        result.FileType = FileType.Fasta;
                        result.Result = SneakAnswer.OK;

                        result.Quantity = Quantity.Single;

                        // check for multiple sequence
                        while (!reader.EndOfStream)
                        {
                            line = reader.ReadLine();
                            if (line.StartsWith(">"))
                            {
                                result.Quantity = Quantity.Multiple;
                                break;
                            }
                        }
                    }
                    else if (line.Trim().StartsWith("CLUSTAL"))
                    {
                        result.FileType = FileType.MutliAlign;
                        result.Quantity = Quantity.Multiple;
                        result.Result = SneakAnswer.OK;
                    }
                    else
                    {
                        result.FileType = FileType.Unknown;
                    }
                }
                finally
                {
                    reader.Close();
                }

                return result;
            }

        }

    }
}
