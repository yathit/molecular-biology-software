using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace BioCSharp.Db
{

    [Table(Name = "Seq")]
    public class Seq
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
        public long Id;

        [Column(CanBeNull = true)]
        public int Alphabet;

        public string Data;
    }

    [Table (Name="SeqRecord")]
    public class SeqRecord
    {
        [Column(IsPrimaryKey = true)]
        public long Id;

        public string Name;

        private EntityRef<string> _Description;
        [Association(Storage="_Description")]
        public string Description
        {
            get { return _Description.Entity; }
            set { _Description.Entity = value; }
        }


        private EntityRef<Seq> _Seq;
        [Association(Storage = "_Seq", ThisKey = "Id", IsForeignKey = true)]
        [Column(Name="Seq", CanBeNull=false)]
        public Seq Seq
        {
            get { return _Seq.Entity; }
            set { _Seq.Entity = value; }
        }

        
    }

    [Table(Name = "Qualifier")]
    public class Qualifier
    {
        [Column(IsPrimaryKey=true)]
        public long Id;

        public string Key;

        public string Value;
    }

    [Table (Name="Feature")]
    public class Feature
    {
        [Column(IsPrimaryKey = true, IsDbGenerated=true)]
        public long Id;
        public string Key;

        public string Location;

        private EntityRef<ICollection<Qualifier>> _Qualifiers;
        [Association(Storage = "_Qualifiers", ThisKey = "Id")]
        public ICollection<Qualifier> Qualifiers
        {
            get { return _Qualifiers.Entity; }
            set { _Qualifiers.Entity = value; }
        }
    }

    [Table(Name="KeyValue")]
    public class KeyValue
    {
        [Column(IsPrimaryKey=true)]
        public long Id;

        public string Key;

        public string Value;
    }

    /// <summary>
    /// Entity Classes class for Insdc record <see cref="BioCSharp.Seqs.InsdcRecord"/>
    /// </summary>
    [Table (Name="Insdcs")]
    public class Insdc : SeqRecord
    {
        private EntityRef<ICollection<Feature>> _Features;
        [Association(Storage = "_Features", ThisKey = "Id")]
        public ICollection<Feature> Features
        {
            get { return _Features.Entity; }
            set { _Features.Entity = value; }
        }

        private EntityRef<ICollection<KeyValue>> _KeyValues;
        [Association(Storage = "_KeyValues", ThisKey = "Id")]
        public ICollection<KeyValue> KeyValues
        {
            get { return _KeyValues.Entity; }
            set { _KeyValues.Entity = value; }
        }
    }
}
