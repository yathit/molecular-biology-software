using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Core;
using NUnit.Core.Tests;
using BioCSharp.Seqs;
using iMoBio.Primer;

namespace Tester
{
    [TestFixture]
    public class SeqTest
    {
        public void TestMan() 
        {
            PrimerTest.SelfDimer();
            PrimerTest.Primers();
            //Seq seq = "GTCAGCTAGTAGCTAGCGCACACCGGCTTGAGCTCGGAGAAATNNCAGTCTCAGTGACAAATTCTTNCCACGCGGTACGAGAGTCCAATT";

            //double[] tm = seq.MeltingTemp();

            //PrimerTest pt = new PrimerTest();
            //pt.SelfDimer();



            //PrimerRegion pr = new PrimerRegion(seq, 0, 50);
            //pr.MaxLength = 20;
            //pr.MinLength = 20;

            //PrimerRegion.Primers primers = pr.PrimersAt(20);

            //for (int i = 0; i < 20; ++i)
            //{
            //    string s = primers.SelfDimer(i, Form.Pretty);
            //    if (s != null)
            //        Console.WriteLine(s);
            //}

            SeqTest t = new SeqTest();
            t.MeltingTemp();
            //t.MolWeight();
        }

        public SeqTest()
        {
        }

        [Test]
        public void GC()
        {
            DnaSeq seq = "ATCGGATCTAGGCTA";
            Assert.AreEqual(46.666666666666664, seq.GC);

            seq = "CACCGGCTTGAGCTCGGAGAAATNNCAGTCTCAGTGACAAATTCTTNCCACGCGGTACGAGAGTCCAATT";
            Assert.AreEqual(50.714285714285708, seq.GC, 0.00000000000002);

            Console.WriteLine("GC");
        }

        [Test]
        public void MolWeight()
        {
            DnaSeq seq = "ATCGGATCTAGGCTA";
            Assert.AreEqual(4.592060000000000e+003, seq.MolWeight);

            seq = "CACTTGTTAG";
            Assert.AreEqual(3.018040000000000e+003, seq.MolWeight);

            seq = "CACCGGCTTGAGCTCGGAGAAATNNCAGTCTCAGTGACAAATTCTTNCCACGCGGTACGAGAGTCCAATT";
            Assert.AreEqual(2.153900500000000e+004, seq.MolWeight);


            Console.WriteLine("MolWeight");
        }

        [Test]
        public void MeltingTemp()
        {
            DnaSeq seq = "ATCGGATCTAGGCTA";

            double tm = seq.MeltingTemp();
            Assert.AreEqual(39.206666666666678, seq.BasicMeltingTemp()); // basic
            Assert.AreEqual(43.369568738644574, seq.SaltAdjustTm()); // saltadj
            Assert.AreEqual(46.081691703796992, seq.MeltingTemp()); // tm

            tm = seq.MeltingTemp(0.05, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Bres86));
            Assert.AreEqual(50.449809619399332, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sant96));
            Assert.AreEqual(47.069920021946075, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sant98));
            Assert.AreEqual(46.081691703796992, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sugi96));
            Assert.AreEqual(50.450781708627972, tm); // tm

            tm = seq.MeltingTemp(0.01, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Bres86));
            Assert.AreEqual(39.206666666666678, seq.BasicMeltingTemp()); // basic
            Assert.AreEqual(31.766666666666666, seq.SaltAdjustTm(0.01)); // saltadj
            Assert.AreEqual(38.846907547421438, tm); // tm

            tm = seq.MeltingTemp(0.01, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sant96));
            Assert.AreEqual(35.467017949968181, tm); // tm

            tm = seq.MeltingTemp(0.01, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sant98));
            Assert.AreEqual(34.478789631819097, tm); // tm

            tm = seq.MeltingTemp(0.01, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sugi96));
            Assert.AreEqual(38.847879636650077, tm); // tm

            seq = "GGCGCAC";
            tm = seq.MeltingTemp(0.05, 50e-5, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Bres86));
            Assert.AreEqual(26.000000000000000, seq.BasicMeltingTemp()); // basic
            Assert.AreEqual(26.000000000000004, seq.SaltAdjustTm(0.05)); // saltadj
            Assert.AreEqual(37.757180741255979, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-5, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sant96));
            Assert.AreEqual(40.701886693752783, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-5, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sant98));
            Assert.AreEqual(39.073979979627097, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-5, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sugi96));
            Assert.AreEqual(35.936125305195731, tm); // tm

            seq = "CACCGGCTTGAGCTCGGAGAAATNNCAGTCTCAGTGACAAATTCTTNCCACGCGGTACGAGAGTCCAATT";

            tm = seq.MeltingTemp();
            Assert.AreEqual(76.087142857142865, seq.BasicMeltingTemp()); // basic
            Assert.AreEqual(73.483743570893864, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Bres86));

            Assert.AreEqual(76.087142857142865, seq.BasicMeltingTemp()); // basic
            Assert.AreEqual(87.981473500549342, seq.SaltAdjustTm(0.05)); // saltadj
            Assert.AreEqual(95.055181849933120, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sant96));
            Assert.AreEqual(77.699563311072183, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sant98));
            Assert.AreEqual(73.483743570893864, tm); // tm

            tm = seq.MeltingTemp(0.05, 50e-6, seq.ThermoProp(DnaSeq.NearestNeighborMethod.Sugi96));
            Assert.AreEqual(80.921454751300217, tm); // tm

            // symmetry sequence
            Assert.AreEqual(22.227070222715383, (new DnaSeq("GCATATGC")).MeltingTemp());

            Console.WriteLine("MeltingTemp");
        }

        [Test]
        public void SelfDimer()
        {
        }
    }

    [TestFixture]
    public class PrimerTest
    {
        [Test]
        public static void SelfDimer()
        {
            DnaSeq seq = "GTCAGCTATAGCTAGCGCA";
            //Seq seq = "AGTCAGCTAGTAGCTAGCGCA";
            //Seq seq = "AGTCAGCTATAGCTAGCGCA";

            Primer p = new Primer(seq);
            //p.SearchDimers();
            p.DumpSelfDimers(Form.Pretty);
            p.DumpSelfDimers(Form.Numeric);

            Console.WriteLine(" ");
            //p.SearchHairPins();
            Console.WriteLine(seq);
            p.DumpHairPins(Form.Simple);
            p.DumpHairPins(Form.Numeric);

            //p.DumpHairPins(Form.Camel);
            p.DumpHairPins(Form.Pretty);

            //seq = "AGTCAGCTATAGCTAGCGCA";
            //p = new Primer(seq);

            //Console.WriteLine(" ");
            //p.SearchHairPins();
            //Console.WriteLine(seq);
            //p.DumpHairPins(Form.Simple);
            //p.DumpHairPins(Form.Numeric);
            //p.DumpHairPins(Form.Camel);
            //p.DumpHairPins(Form.Pretty);
        }

        [Test]
        public static void Primers()
        {
            Seq seq = "AGTCAGCTATAGACGAGTTATGACGTTATTCTACTTTGATTGTGCGAGCGCA";
            PrimerRegion pg = new PrimerRegion(seq, 0, seq.Length);
            int pLen = 19;
            pg.MaxLength = pLen;
            pg.MinLength = pLen;
            pg.MinDimerLength = 4;
            PrimerRegion.Primers ps = pg.PrimersAt(pLen);
            Console.WriteLine(seq);
            for (int i = 0; i <= seq.Length - pLen; ++i)
            {
                Console.WriteLine("Dimers at: {0}", i);

                string[] ss = ps.SelfDimers(i, Form.Pretty);
                string[] ns = ps.SelfDimers(i, Form.Numeric);
                string[] hs = ps.HairPins(i, Form.Pretty);
                for (int j = 0; j < ss.Length; ++j)
                {
                    Console.WriteLine(ns[j]);
                    Console.WriteLine(ss[j]);

                }
                if (hs.Length > 0)
                {
                    Console.WriteLine("hairpins at: {0}", i);
                    foreach (string s in hs)
                    {
                        Console.WriteLine(s);
                    }
                }
            }
        }
    }
}
