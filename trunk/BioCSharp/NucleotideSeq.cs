using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioCSharp.Seqs
{
    public class NucleotideSeq : Seq
    {
        public NucleotideSeq(string data)
            : base(data.ToLower())
        {
            
        }
    }
}
