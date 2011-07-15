using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioCSharp.Seqs;
using BioCSharp.Algo;
using System.Diagnostics;
using Muscle;
using MathNet.Numerics.LinearAlgebra;
using System.Collections;
using BioCSharp.Algo.Phylo;

namespace BioCSharp.Algo
{
    /// <summary>
    /// Multiple alignment.
    /// 
    /// Use <code>DoMuscle</code> method for multiple alignment.
    /// </summary>
    public class MultiAlign : IEnumerable
    {
        protected readonly SeqRecord[] records;
        /// <summary>
        /// Each element in <code>alignments</code> correspond to <code>records</code>
        /// </summary>
        protected SeqRecord[] alignments;

        protected Tree tree;

        string _concensus = null;

        /// <summary>
        /// Construct an multiple alignment with given input sequence to be align. Input sequence
        /// will be deep copied.
        /// 
        /// 
        /// </summary>
        /// <param name="records"></param>
        public MultiAlign(SeqRecord[] records)
        {
            // assume given input is alignment
            // do a deep copy
            this.records = new SeqRecord[records.Length];
            this.alignments = new SeqRecord[records.Length];
            for (int i = 0; i < records.Length; ++i)
            {
                SeqRecord r = records[i];
                this.alignments[i] = (SeqRecord)r.Clone();
                this.records[i] = (SeqRecord)r.Clone();
                this.records[i].RemoveGap();
                if (this.records[0].Alphabet != this.records[i].Alphabet)
                {
                    throw new InvalidOperationException("All sequence must be same Alphabet type.");
                }
            }
            if (this.records[0].Alphabet == Alphabet.DNA | this.records[0].Alphabet == Alphabet.RNA |
                (this.records[0].Alphabet != Alphabet.Protein))
            {
                throw new InvalidOperationException("Alphabet must be DNA, RNA or protein.");
            }
        }

        public SeqRecord[] Input
        {
            get
            {
                return records;
            }
        }

        /// <summary>
        /// Do multiple sequence alignment using MUSCLE algorithm.
        /// </summary>
        public void DoMuscle()
        {
            List<string> ids = new List<string>(records.Length);
            List<string> seqs = new List<string>(records.Length);
            List<string> alns = new List<string>(records.Length);
            foreach (SeqRecord r in records)
            {
                ids.Add(r.Name);
                seqs.Add(r.Sequence);
            }

            Muscle.Multialign.DoMuscle(seqs, ids, alns);
            Debug.Assert(seqs.Count == alns.Count);
            alignments = new SeqRecord[alns.Count];
            for (int i = 0; i < alns.Count; ++i)
            {
                alignments[i] = new SeqRecord(alns[i], 
                    records[i].Alphabet, records[i].Name, records[i].Description);
            }

            _concensus = null;
        }

        /// <summary>
        /// Build phylogenetic using default method
        /// </summary>
        public void BuildTree()
        {
            BuildTree(LinkageMethod.Average, DistMethod.JukesCantor);
        }

        /// <summary>
        /// Build phylogenetic
        /// </summary>
        /// <param name="linkage"></param>
        /// <param name="distMethod"></param>
        public void BuildTree(LinkageMethod linkage, DistMethod distMethod)
        {
            tree = Algo.Linkage(Aligment, linkage, distMethod);
        }

        /// <summary>
        /// Get the tree
        /// </summary>
        public Tree Tree
        {
            get
            {
                if (tree == null)
                {
                    BuildTree();
                }
                return tree;
            }
        }

        /// <summary>
        /// Get the alignment. Alignment algorithm must be run before calling this method.
        /// </summary>
        public SeqRecord[] Aligment { get { return alignments; } }
        

        public string Concensus
        {
            get
            {
                if (_concensus != null)
                    return _concensus;

                Matrix profile = new Matrix(Profile);
                Matrix scoringMatrix = new Matrix(Seqs.SubstitutionMatrix.Blosum50.Matrix);
                Matrix consensusValues = scoringMatrix * profile;

                //Console.WriteLine(profile);
                //Console.WriteLine(scoringMatrix);
                //Console.WriteLine(consensusValues);

                // take consensus symbols by the maximun
                StringBuilder sb = new StringBuilder(alignments[0].Length);
                for (int i = 0; i < alignments[0].Length; i++)
                {
                    sb.Append(Seq.ProteinChars[consensusValues.MaxIndexOfRow(i)]);
                }
                _concensus = sb.ToString();

                return _concensus;
            }
        }

        public double[] ConservationScore
        {
            get
            {
                Matrix profile = new Matrix(Profile);
                Matrix scoringMatrix = new Matrix(Seqs.SubstitutionMatrix.Blosum50.Matrix);
                Matrix consensusValues = scoringMatrix * profile;
                //Console.WriteLine(profile);
                //Console.WriteLine(consensusValues);
                double[] scores = new double[alignments[0].Length];
                for (int i = 0; i < alignments[0].Length; i++)
                {
                    double[] sqrSum = new double[scoringMatrix.ColumnCount];
                    for (int k2 = 0; k2 < scoringMatrix.ColumnCount; ++k2)
                    {
                        for (int k1 = 0; k1 < scoringMatrix.RowCount; ++k1)
                        {
                            double dist = (consensusValues[k1, i] - scoringMatrix[k1, k2]);
                            sqrSum[k2] += dist * dist;
                        }
                    }
                    for (int k1 = 0; k1 < scoringMatrix.RowCount; ++k1)
                    {
                        scores[i] += Math.Sqrt(sqrSum[k1]) * profile[k1, i];
                    }
                }
                return scores;
            }
        }

        /// <summary>
        /// Computes the sequence profile of a multiple alignment. 
        /// returns a matrix P of size [20 x seq Length]
        /// with the count of amino acids (or nucleotides) for every column in
        /// the multiple alignment.
        /// 
        /// Ambiguous symbols are ignored.
        /// </summary>
        public int[,] ProfileCount
        {
            get
            {
                int[,] p = null;

                if (records[0].Alphabet == Alphabet.Protein)
                {
                    p = new int[Seq.ProteinChars.Length, alignments[0].Length];
                    for (int i = 0; i < alignments.Length; i++)
                    {
                        for (int j = 0; j < alignments[i].Length; j++)
                        {
                            int idx = Seq.ProteinChars.IndexOf(alignments[i][j]);
                            if (idx >= 0)
                            {
                                ++p[idx, j];
                            }
                        }
                    }
                }
                else if (records[0].Alphabet == Alphabet.DNA)
                {
                    p = new int[Seq.DNAChars.Length, alignments[0].Length];
                    for (int i = 0; i < alignments.Length; i++)
                    {
                        for (int j = 0; j < alignments[i].Length; j++)
                        {
                            int idx = Seq.DNAChars.IndexOf(alignments[i][j]);
                            if (idx >= 0)
                            {
                                ++p[idx, j];
                            }
                        }
                    }
                }
                else if (records[0].Alphabet == Alphabet.RNA)
                {
                    p = new int[Seq.RNAChars.Length, alignments[0].Length];
                    for (int i = 0; i < alignments.Length; i++)
                    {
                        for (int j = 0; j < alignments[i].Length; j++)
                        {
                            int idx = Seq.RNAChars.IndexOf(alignments[i][j]);
                            if (idx >= 0)
                            {
                                ++p[idx, j];
                            }
                        }
                    }
                }
                else
                {
                    // this is not possible since we check in the constructor
                    throw new Exception("Internal error.");
                }

                return p;
            }

        }
            /// <summary>
        /// Computes the sequence profile of a multiple alignment. 
        /// returns a matrix P of size [20 x seq Length]
        /// with the frequency of amino acids (or nucleotides) for every column in
        /// the multiple alignment.
        /// 
        /// Ambiguous symbols are ignored.
        /// </summary>
        public double[,] Profile
        {
            get
            {
                int[,] iP = ProfileCount;
                double[,] p = new double[iP.Length / alignments[0].Length, alignments[0].Length];

                
                // calculate frequency
                for (int i = 0; i < alignments[0].Length; i++)
                {
                    double sum = 0.0;
                    for (int j = 0; j < (p.Length / alignments[0].Length); j++)
                    {
                        sum += iP[j, i];
                    }
                    for (int j = 0; j < (p.Length / alignments[0].Length); j++)
                    {
                        p[j, i] = iP[j, i] / sum;
                    }
                }
                return p;
            }
        }

        #region IEnumerable Members

        /// <summary>
        /// Return alignment sequence as <code>SeqRecord</code>
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            foreach (SeqRecord r in alignments)
            {
                yield return r;
            }
        }

        #endregion
    }
}
