using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioCSharp.Seqs
{
    public struct HalfLife
    {
        public double Mammalian;
        public double Yeast;
        public double Ecoli;

        public HalfLife(double m, double y, double e)
        {
            Mammalian = m;
            Yeast = y;
            Ecoli = e;
        }
    }



    public class AminoAcidInfo
    {
        /// <summary>
        /// pK table (EMBOSS)
        /// </summary>
        public static class pKSet
        {
            public static double N_term = 8.6;
            public static double K = 10.8;
            public static double R = 12.5;
            public static double H = 6.5;
            public static double D = 3.9;
            public static double E = 4.1;
            public static double C = 8.5;
            public static double Y = 10.1;
            public static double C_term = 3.6;
        }

        private char code;
        private string triplet;
        private string name;
        private string molecularFormula;
        private double molecularWeight;
        private double aminoAcidComposition;
        private double bulkiness;
        private double polarity;
        private double recognitionFactor;
        private double hydrophobicity;
        private double hydropathicityKD;
        private double refractivity;
        private double numberOfCodons;
        private double percentBuriedResidues;
        private double percentAccessibleResidues;
        private double averageAreaBuried;
        private double averageFlexibility;
        private HalfLife halfLife;

        public char Code { get { return code; } }
        public string Triplet { get { return triplet; } }
        public string Name { get { return name; } }
        public string MolecularFormula { get { return molecularFormula; } }
        public double MolecularWeight { get { return molecularWeight; } }
        public double AminoAcidComposition { get { return aminoAcidComposition; } }
        public double Bulkiness { get { return bulkiness; } }
        public double Polarity { get { return polarity; } }
        public double RecognitionFactor { get { return recognitionFactor; } }
        public double Hydrophobicity { get { return hydrophobicity; } }
        public double HydropathicityKD { get { return hydropathicityKD; } }
        public double Refractivity { get { return refractivity; } }
        public double NumberOfCodons { get { return numberOfCodons; } }
        public double PercentBuriedResidues { get { return percentBuriedResidues; } }
        public double PercentAccessibleResidues { get { return percentAccessibleResidues; } }
        public double AverageAreaBuried { get { return averageAreaBuried; } }
        public double AverageFlexibility { get { return averageFlexibility; } }
        public HalfLife HalfLife { get { return halfLife; } }

        private AminoAcidInfo(char code, string triplet,
            string name, string molecularFormula, double molecularWeight,
            double aminoAcidComposition, double bulkiness, double polarity, double recognitionFactor,
            double hydrophobicity, double hydropathicityKD, double refractivity,
            double numberOfCodons, double percentBuriedResidues, double percentAccessibleResidues,
            double averageAreaBuried, double averageFlexibility, HalfLife halfLife)
        {
            this.triplet = triplet;
            this.code = code;
            this.name = name;
            this.molecularFormula = molecularFormula;
            this.molecularWeight = molecularWeight;
            this.aminoAcidComposition = aminoAcidComposition;
            this.bulkiness = bulkiness;
            this.polarity = polarity;
            this.recognitionFactor = recognitionFactor;
            this.hydrophobicity = hydrophobicity;
            this.hydropathicityKD = hydropathicityKD;
            this.refractivity = refractivity;
            this.numberOfCodons = numberOfCodons;
            this.percentBuriedResidues = percentBuriedResidues;
            this.percentAccessibleResidues = percentAccessibleResidues;
            this.averageAreaBuried = averageAreaBuried;
            this.averageFlexibility = averageFlexibility;
            this.halfLife = halfLife;
        }

        public static AminoAcidInfo AminoAcid(char code)
        {
            if (code == 'A')
                return AminoAcidInfo.A;
            else if (code == 'C')
                return AminoAcidInfo.C;
            else if (code == 'D')
                return AminoAcidInfo.D;
            else if (code == 'E')
                return AminoAcidInfo.E;
            else if (code == 'F')
                return AminoAcidInfo.F;
            else if (code == 'G')
                return AminoAcidInfo.G;
            else if (code == 'H')
                return AminoAcidInfo.H;
            else if (code == 'I')
                return AminoAcidInfo.I;
            else if (code == 'K')
                return AminoAcidInfo.K;
            else if (code == 'L')
                return AminoAcidInfo.L;
            else if (code == 'M')
                return AminoAcidInfo.M;
            else if (code == 'N')
                return AminoAcidInfo.N;
            else if (code == 'P')
                return AminoAcidInfo.P;
            else if (code == 'Q')
                return AminoAcidInfo.Q;
            else if (code == 'R')
                return AminoAcidInfo.R;
            else if (code == 'S')
                return AminoAcidInfo.S;
            else if (code == 'T')
                return AminoAcidInfo.T;
            else if (code == 'V')
                return AminoAcidInfo.V;
            else if (code == 'W')
                return AminoAcidInfo.W;
            else if (code == 'Y')
                return AminoAcidInfo.Y;
            else
                return AminoAcidInfo.X;
        }

        /// <summary>
        /// calulate the estimated pI
        /// </summary>
        /// <param name="pepcounts"></param>
        /// <returns></returns>
        public double pIest(int[] pepcounts)
        {
            double pIest = 7;
            double pH_diff = 3.5;
            double pp_charge = calc_charge(pepcounts, pIest);
            while (Math.Abs(pp_charge) >= 10e-4)
            {
                pIest = pIest + (Math.Sign(pp_charge) * pH_diff);
                pH_diff = pH_diff / 2;
                pp_charge = calc_charge(pepcounts, pIest);
            }
            return pIest;
        }

        /// <summary>
        /// calculates the charge for a polypeptide at default pH value of 7.2
        /// </summary>
        /// <param name="pepcounts"></param>
        /// <returns></returns>
        private double calc_charge(int[] pepcounts)
        {
            return calc_charge(pepcounts, 7.2);
        }

        /// <summary>
        /// calculates the charge for a polypeptide at a specified pH.
        /// </summary>
        /// <param name="pepcounts"></param>
        /// <param name="pH"></param>
        /// <returns></returns>
        private double calc_charge(int[] pepcounts, double pH)
        {
            //double net_charge = p_chrg(AminoAcidInfo.pKSet.N_term, pH)
            //    + pepcounts.K * p_chrg(AminoAcidInfo.pKSet.K, pH)
            //    + pepcounts.R * p_chrg(AminoAcidInfo.pKSet.R, pH)
            //    + pepcounts.H * p_chrg(AminoAcidInfo.pKSet.H, pH)
            //    - pepcounts.D * p_chrg(pH, AminoAcidInfo.pKSet.D)
            //    - pepcounts.E * p_chrg(pH, AminoAcidInfo.pKSet.E)
            //    - pepcounts.C * p_chrg(pH, AminoAcidInfo.pKSet.C)
            //    - pepcounts.Y * p_chrg(pH, AminoAcidInfo.pKSet.Y)
            //    - p_chrg(pH, AminoAcidInfo.pKSet.C_term);
            return 0.0; /* net_charge; */
        }

        /// <summary>
        /// calculates the partial charge for a specific amino acid
        /// </summary>
        /// <param name="pA"></param>
        /// <param name="pB"></param>
        /// <returns></returns>
        private double p_chrg(double pA, double pB)
        {
            double cr = Math.Pow(10, (pA - pB));
            return cr / (cr + 1);
        }

        #region class initializer
        public static readonly AminoAcidInfo A = new AminoAcidInfo('A', "ala", "Alanine",
                "C3H7NO2",
                89.090000,
                8.300000,
                11.500000,
                8.100000,
                78.000000,
                0.620000,
                1.800000,
                4.340000,
                4.000000,
                11.200000,
                6.600000,
                86.600000,
                0.360000,
                new HalfLife(4.4, 20, 10)
                );

        public static readonly AminoAcidInfo C = new AminoAcidInfo('C', "cys",
            "Cysteine", "C3H7NO2S",
            121.150000,
            1.700000,
            13.460000,
            15.500000,
            89.000000,
            0.290000,
            2.500000,
            35.770000,
            1.000000,
            4.100000,
            0.900000,
            132.300000,
            0.350000,
            new HalfLife(1.2, 20, 10)
            );

        public static readonly AminoAcidInfo D = new AminoAcidInfo('D', "asp",
            "AsparticAcid", "C4H7NO4",
            133.100000,
            5.300000,
            11.680000,
            13.000000,
            81.000000,
            -0.900000,
            -3.500000,
            13.280000,
            2.000000,
            2.900000,
            7.700000,
            97.800000,
            0.510000,
            new HalfLife(1.1, 2, 10)
            );

        public static readonly AminoAcidInfo E = new AminoAcidInfo('E', "glu",
            "GlutamicAcid", "C5H9NO4",
            147.130000,
            6.200000,
            13.570000,
            12.300000,
            78.000000,
            -0.740000,
            -3.500000,
            17.560000,
            2.000000,
            1.800000,
            5.700000,
            113.900000,
            0.500000,
            new HalfLife(1, 30, 10)
            );

        public static readonly AminoAcidInfo F = new AminoAcidInfo('F', "phe",
            "Phenylalanine", "C9H11NO2",
            165.190000,
            3.900000,
            19.800000,
            5.200000,
            81.000000,
            1.190000,
            2.800000,
            29.400000,
            2.000000,
            5.100000,
            2.400000,
            194.100000,
            0.310000,
            new HalfLife(1.1, 3, 2)
            );

        public static readonly AminoAcidInfo G = new AminoAcidInfo('G', "gly",
            "Glycine", "C2H5NO2",
            75.070000,
            7.200000,
            3.400000,
            9.000000,
            84.000000,
            0.480000,
            -0.400000,
            0.000000,
            4.000000,
            11.800000,
            6.700000,
            62.900000,
            0.540000,
            new HalfLife(30, 20, 10)
            );

        public static readonly AminoAcidInfo H = new AminoAcidInfo('H', "his",
            "Histidine", "C6H9N3O2",
            155.160000,
            2.200000,
            13.690000,
            10.400000,
            84.000000,
            -0.400000,
            -3.200000,
            21.810000,
            2.000000,
            2.000000,
            2.500000,
            155.800000,
            0.320000,
            new HalfLife(3.5, 10, 10)
            );

        public static readonly AminoAcidInfo I = new AminoAcidInfo('I', "ile",
            "Isoleucine", "C6H13NO2",
            131.170000,
            5.200000,
            21.400000,
            5.200000,
            88.000000,
            1.380000,
            4.500000,
            18.780000,
            3.000000,
            8.600000,
            2.800000,
            158.000000,
            0.460000,
            new HalfLife(20, 30, 10)
            );

        public static readonly AminoAcidInfo K = new AminoAcidInfo('K', "lys",
            "Lysine", "C6H14N2O2",
            146.190000,
            5.700000,
            15.710000,
            11.300000,
            87.000000,
            -1.500000,
            -3.900000,
            21.290000,
            2.000000,
            0.500000,
            10.300000,
            115.500000,
            0.470000,
            new HalfLife(1.3, 3.0 / 60, 2.0 / 60)
            );

        public static readonly AminoAcidInfo L = new AminoAcidInfo('L', "leu",
            "Leucine", "C6H13NO2",
            131.170000,
            9.000000,
            21.400000,
            4.900000,
            85.000000,
            1.060000,
            3.800000,
            19.060000,
            6.000000,
            11.700000,
            4.800000,
            164.100000,
            0.370000,
            new HalfLife(5.5, 3.0 / 60, 2.0 / 60)
            );

        public static readonly AminoAcidInfo M = new AminoAcidInfo('M', "met",
            "Methionine", "C5H11NO2S",
            149.210000,
            2.400000,
            16.250000,
            5.700000,
            80.000000,
            0.640000,
            1.900000,
            21.640000,
            1.000000,
            1.900000,
            1.000000,
            172.900000,
            0.300000,
            new HalfLife(30, 20, 10)
            );

        public static readonly AminoAcidInfo N = new AminoAcidInfo('N', "asn",
            "Asparagine", "C4H8N2O3",
            132.120000,
            4.400000,
            12.820000,
            11.600000,
            94.000000,
            -0.780000,
            -3.500000,
            12.000000,
            2.000000,
            2.900000,
            6.700000,
            103.300000,
            0.460000,
            new HalfLife(1.4, 3, 10)
            );

        public static readonly AminoAcidInfo P = new AminoAcidInfo('P', "pro",
            "Proline", "C5H9NO2",
            115.130000,
            5.100000,
            17.430000,
            8.000000,
            91.000000,
            0.120000,
            -1.600000,
            10.930000,
            4.000000,
            2.700000,
            4.800000,
            92.900000,
            0.510000,
            new HalfLife(20, 20, Double.NaN)
            );

        public static readonly AminoAcidInfo Q = new AminoAcidInfo('Q', "gln",
            "Glutamine", "C5H10N2O3",
            146.150000,
            4.000000,
            14.450000,
            10.500000,
            87.000000,
            -0.850000,
            -3.500000,
            17.260000,
            2.000000,
            1.600000,
            5.200000,
            119.200000,
            0.490000,
            new HalfLife(0.8, 10, 10)
            );

        public static readonly AminoAcidInfo R = new AminoAcidInfo('R', "arg",
            "Arginine", "C6H14N4O2",
            174.200000,
            5.700000,
            14.280000,
            10.500000,
            95.000000,
            -2.530000,
            -4.500000,
            26.660000,
            6.000000,
            0.500000,
            4.500000,
            162.200000,
            0.530000,
            new HalfLife(1.0, 2.0 / 60, 2.0 / 60)
            );

        public static readonly AminoAcidInfo S = new AminoAcidInfo('S', "ser",
            "Serine", "C3H7NO3",
            105.090000,
            6.900000,
            9.470000,
            9.200000,
            107.000000,
            -0.180000,
            -0.800000,
            6.350000,
            6.000000,
            8.000000,
            9.400000,
            85.600000,
            0.510000,
            new HalfLife(1.9, 20.0, 10.0)
            );

        public static readonly AminoAcidInfo T = new AminoAcidInfo('T', "thr",
            "Threonine", "C4H9NO3",
            119.120000,
            5.800000,
            15.770000,
            8.600000,
            93.000000,
            -0.050000,
            -0.700000,
            11.010000,
            4.000000,
            4.900000,
            7.000000,
            106.500000,
            0.440000,
            new HalfLife(7.2, 20.0, 10.0)
            );

        public static readonly AminoAcidInfo V = new AminoAcidInfo('V', "val",
            "Valine", "C5H11NO2",
            117.150000,
            6.600000,
            21.570000,
            5.900000,
            89.000000,
            1.080000,
            4.200000,
            13.920000,
            4.000000,
            12.900000,
            4.500000,
            141.000000,
            0.390000,
            new HalfLife(100.0, 20.0, 10.0)
            );

        public static readonly AminoAcidInfo W = new AminoAcidInfo('W', "trp",
            "Tryptophan", "C11H12N2O2",
            204.230000,
            1.300000,
            21.670000,
            5.400000,
            104.000000,
            0.810000,
            -0.900000,
            42.530000,
            1.000000,
            2.200000,
            1.400000,
            224.600000,
            0.310000,
            new HalfLife(2.8, 3.0 / 60, 2.0 / 60)
            );

        public static readonly AminoAcidInfo Y = new AminoAcidInfo('Y', "tyr",
            "Tyrosine", "C9H11NO3",
                181.190000,
                3.200000,
                18.030000,
                6.200000,
                84.000000,
                0.260000,
                -1.300000,
                31.530000,
                2.000000,
                2.600000,
                5.100000,
                177.700000,
                0.420000,
                new HalfLife(2.8, 10, 2)
                );

        public static readonly AminoAcidInfo X = new AminoAcidInfo('X', "xxx",
            "", "",
                119.7863, // average of 20 amino acid residue
                Double.NaN,
                Double.NaN,
                Double.NaN,
                Double.NaN,
                Double.NaN,
                Double.NaN,
                Double.NaN,
                Double.NaN,
                Double.NaN,
                Double.NaN,
                Double.NaN,
                Double.NaN,
                new HalfLife(Double.NaN, Double.NaN, Double.NaN)
                );

        #endregion

    }

    public class ProteinSeq : Seq
    {
        public ProteinSeq(string data)
            : base(data.ToUpper())
        {
           
        }
    }
}
