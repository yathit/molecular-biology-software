using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace BioCSharp.Seqs
{
    #region color scheme class
    public enum ProteinColorScheme
    {
        None,
        Charge,
        Function,
        Hydrophybocity,
        RASMOL,
        Structure,
        Taylor
    }

    /// <summary>
    /// Color pallet for amino residue
    /// </summary>
    public class ProteinColorPallet
    {
        /// <summary>
        /// ordering of the amino residues
        /// </summary>
        /// public readonly static string Order = "ARNDCQEGHILKMFPSTWYV";

        private readonly Brush[] brushes;
        /// <summary>
        /// Color schemes
        /// </summary>
        public readonly ProteinColorScheme ColorScheme;

        public static ProteinColorPallet None;
        public static ProteinColorPallet Charge;
        public static ProteinColorPallet Function;
        public static ProteinColorPallet Hydrophybocity;
        public static ProteinColorPallet RASMOL;
        public static ProteinColorPallet Structure;
        public static ProteinColorPallet Taylor;

        static ProteinColorPallet()
        {
            None = new ProteinColorPallet(ProteinColorScheme.None);
            Charge = new ProteinColorPallet(ProteinColorScheme.Charge);
            Function = new ProteinColorPallet(ProteinColorScheme.Function);
            Hydrophybocity = new ProteinColorPallet(ProteinColorScheme.Hydrophybocity);
            RASMOL = new ProteinColorPallet(ProteinColorScheme.RASMOL);
            Structure = new ProteinColorPallet(ProteinColorScheme.Structure);
            Taylor = new ProteinColorPallet(ProteinColorScheme.Taylor);
        }


        private ProteinColorPallet(ProteinColorScheme scheme)
        {
            this.ColorScheme = scheme;
            brushes = new Brush[21];
            for (int i = 0; i < brushes.Length; i++)
            {
                brushes[i] = new SolidColorBrush(GetProteinColor(scheme, i));
                brushes[i].Freeze();
            }
        }

        public static ProteinColorPallet GetProteinColorPallet(ProteinColorScheme scheme)
        {
            if (scheme == ProteinColorScheme.Charge)
                return Charge;
            else if (scheme == ProteinColorScheme.Function)
                return Function;
            else if (scheme == ProteinColorScheme.Hydrophybocity)
                return Hydrophybocity;
            else if (scheme == ProteinColorScheme.RASMOL)
                return RASMOL;
            else if (scheme == ProteinColorScheme.Structure)
                return Structure;
            else if (scheme == ProteinColorScheme.Taylor)
                return Taylor;
            else
                return None;
        }

        /// <summary>
        /// Return freezed brush for a given amino residue.
        /// </summary>
        /// <param name="aa"></param>
        /// <returns>brush for a given amino residue. white color for invalid residue</returns>
        public Brush this[char aa]
        {
            get
            {
                int idx = Seq.ProteinChars.IndexOf(aa);
                if (idx >= 0)
                    return brushes[idx];
                else
                    return brushes[brushes.Length - 1];
            }
        }

        #region protein color schemes
        public static readonly byte[,] ProteinColorScheme_Charge = { 
                                                        {204, 255, 204},
                                                        {170, 170, 255},
                                                        {204, 255, 204},
                                                        {255, 150, 150},
                                                        {204, 255, 204},
                                                        {204, 255, 204},
                                                        {255, 150, 150},
                                                        {204, 255, 204},
                                                        {170, 170, 255},
                                                        {204, 255, 204},
                                                        {204, 255, 204},
                                                        {170, 170, 255},
                                                        {204, 255, 204},
                                                        {204, 255, 204},
                                                        {204, 255, 204},
                                                        {204, 255, 204},
                                                        {204, 255, 204},
                                                        {204, 255, 204},
                                                        {204, 255, 204},
                                                        {204, 255, 204},
                                                       };


        public static readonly byte[,] ProteinColorScheme_Function = {
                                                            {210, 210, 210},
                                                            {50, 150, 255},
                                                            {150, 255, 150},
                                                            {255, 150, 150},
                                                            {150, 255, 150},
                                                            {150, 255, 150},
                                                            {255, 150, 150},
                                                            {150, 255, 150},
                                                            {50, 150, 255},
                                                            {210, 210, 210},
                                                            {210, 210, 210},
                                                            {50, 150, 255},
                                                            {210, 210, 210},
                                                            {210, 210, 210},
                                                            {210, 210, 210},
                                                            {150, 255, 150},
                                                            {150, 255, 150},
                                                            {210, 210, 210},
                                                            {150, 255, 150},
                                                            {210, 210, 210}
                                                         };

        public static readonly byte[,] ProteinColorScheme_Hydrophybocity = {
                                                                   {255, 255, 150},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {255, 255, 150},
                                                            {255, 255, 150},
                                                            {50, 150, 255},
                                                            {255, 255, 150},
                                                            {255, 255, 150},
                                                            {255, 255, 150},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {255, 255, 150},
                                                            {50, 150, 255},
                                                            {255, 255, 150}
                                                               };

        public static readonly byte[,] ProteinColorScheme_RASMOL = {
                                                           {200, 200, 200},
                                                            {20, 90, 255},
                                                            {0, 220, 220},
                                                            {230, 10, 10},
                                                            {230, 230, 0},
                                                            {0, 220, 220},
                                                            {230, 10, 10},
                                                            {235, 235, 235},
                                                            {130, 130, 240},
                                                            {15, 130, 15},
                                                            {15, 130, 15},
                                                            {20, 90, 255},
                                                            {230, 230, 0},
                                                            {50, 90, 170},
                                                            {220, 150, 130},
                                                            {250, 150, 0},
                                                            {250, 150, 0},
                                                            {180, 90, 180},
                                                            {50, 50, 170},
                                                            {15, 130, 15}
                                                       };

        public static readonly byte[,] ProteinColorScheme_Structure = {
                                                              {204, 255, 204},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {204, 255, 204},
                                                            {50, 150, 255},
                                                            {50, 150, 255},
                                                            {204, 255, 204},
                                                            {50, 150, 255},
                                                            {255, 150, 50},
                                                            {255, 150, 50},
                                                            {50, 150, 255},
                                                            {255, 150, 50},
                                                            {255, 150, 50},
                                                            {204, 255, 204},
                                                            {204, 255, 204},
                                                            {204, 255, 204},
                                                            {204, 255, 204},
                                                            {204, 255, 204},
                                                            {255, 150, 50}
                                                          };

        public static readonly byte[,] ProteinColorScheme_Taylor = {
                                                           {204, 255, 0},
                                                            {0, 0, 255},
                                                            {204, 0, 255},
                                                            {255, 0, 0},
                                                            {250, 250, 0},
                                                            {255, 0, 204},
                                                            {255, 0, 102},
                                                            {255, 153, 0},
                                                            {0, 102, 255},
                                                            {102, 255, 0},
                                                            {51, 255, 0},
                                                            {102, 0, 255},
                                                            {0, 255, 0},
                                                            {0, 255, 102},
                                                            {255, 204, 0},
                                                            {255, 51, 0},
                                                            {255, 102, 0},
                                                            {0, 204, 255},
                                                            {0, 255, 204},
                                                            {153, 255, 0}
                                                       };

        public static Color GetProteinColor(ProteinColorScheme scheme, char aa)
        {
            int idx = Seq.ProteinChars.IndexOf(aa);
            return GetProteinColor(scheme, idx);
        }

        public static Color GetProteinColor(ProteinColorScheme scheme, int idx)
        {
            byte[,] colormap;
            if (scheme == ProteinColorScheme.Charge)
                colormap = ProteinColorScheme_Charge;
            else if (scheme == ProteinColorScheme.Function)
                colormap = ProteinColorScheme_Function;
            else if (scheme == ProteinColorScheme.Hydrophybocity)
                colormap = ProteinColorScheme_Hydrophybocity;
            else if (scheme == ProteinColorScheme.RASMOL)
                colormap = ProteinColorScheme_RASMOL;
            else if (scheme == ProteinColorScheme.Structure)
                colormap = ProteinColorScheme_Structure;
            else if (scheme == ProteinColorScheme.Taylor)
                colormap = ProteinColorScheme_Taylor;
            else
                return Colors.Transparent;


            if (idx < 0 || idx >= colormap.Length / 3)
                return Colors.White;
            else
                return Color.FromRgb(colormap[idx, 0], colormap[idx, 1], colormap[idx, 2]);

        }


        #endregion
    }

    #endregion

    public partial class Seq
    {
    }
}
