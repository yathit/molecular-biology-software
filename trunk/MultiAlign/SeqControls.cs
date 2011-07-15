using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using BioCSharp.Seqs;
using System.Globalization;
using System.Windows.Controls;
using System.Windows;

namespace iMoBio.Controls
{
    public class SeqLabelCanvas : Canvas
    {
        private static CultureInfo _usCultureInfo = CultureInfo.GetCultureInfo("en-us");
        private FormattedText ftext;
        ProteinColorScheme scheme;
        private static Typeface typeface = new Typeface("Lucida Console");

        public SeqLabelCanvas(Seq seq) : this(seq, ProteinColorScheme.Charge) { }

        public SeqLabelCanvas(Seq seq, ProteinColorScheme scheme) : this(seq, scheme, 12.0, typeface) { }
        public SeqLabelCanvas(Seq seq, ProteinColorScheme scheme, double fontSize, Typeface tf)
            : base()
        {
            this.scheme = scheme;
            ftext = new FormattedText(seq.Data,
                _usCultureInfo,
               System.Windows.FlowDirection.LeftToRight,
                tf,
                fontSize,
                Brushes.Black);

            Height = ftext.Height;
            Width = ftext.Width;
        }

        public ProteinColorScheme ColorScheme
        {
            get { return scheme; }
            set { scheme = value; }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double charWidth = ftext.Width / ftext.Text.Length;
            ProteinColorPallet pallet = ProteinColorPallet.GetProteinColorPallet(scheme);
            int i = 0;
            double h = ftext.Height;
            foreach (char ch in ftext.Text)
            {
                drawingContext.DrawRectangle(pallet[ch], null,
                    new Rect(i * charWidth, 0, charWidth, h));
                ++i;
            }
            drawingContext.DrawText(ftext, new System.Windows.Point(0, 0));
        }
    }
}
