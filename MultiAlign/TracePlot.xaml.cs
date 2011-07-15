using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Swordfish.WPF.Charts;
using BioCSharp.Assembly;
using MathNet.Numerics;
using System.Globalization;

namespace iMoBio.Controls
{
    /// <summary>
    /// Interaction logic for TracePlot.xaml
    /// </summary>
    public partial class TracePlot : UserControl
    {
        class SequenceCanvas : Canvas
        {
            private TracePlot parent;
            double xBase;
            double pointWidth;

            public SequenceCanvas(TracePlot parent)
            {
                this.parent = parent;

                Rect plotArea = ChartUtilities.GetPlotRectangle(parent.tracePlot.Primitives, 0.01f);

                double x = plotArea.Left;
                pointWidth = (parent.tracePlot.Width - Math.Abs(plotArea.Left) * 2) / parent.lineA.Points.Count;
                //pointWidth = plotArea.Width / parent.lineA.Points.Count;

                xBase = x;
            }


            protected override void OnRender(DrawingContext dc)
            {
                Size size = MeasureOverride(new Size(0, 0));

                int nPoints = parent.lineA.Points.Count;


                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, size.Width, size.Height));


                Brush brushA = new SolidColorBrush(parent.lineA.Color);
                brushA.Freeze();
                Brush brushC = new SolidColorBrush(parent.lineC.Color);
                brushC.Freeze();
                Brush brushG = new SolidColorBrush(parent.lineG.Color);
                brushG.Freeze();
                Brush brushT = new SolidColorBrush(parent.lineT.Color);
                brushT.Freeze();

                CultureInfo cf = CultureInfo.GetCultureInfo("en-us");
                Typeface tf = new Typeface("Lucida Console");

                for (int i = 0; i < parent.sequence.Length; ++i)
                {
                    Brush b = Brushes.Gray;
                    if (parent.sequence[i] == 'A')
                        b = brushA;
                    else if (parent.sequence[i] == 'C')
                        b = brushC;
                    else if (parent.sequence[i] == 'G')
                        b = brushG;
                    else if (parent.sequence[i] == 'T')
                        b = brushT;

                    FormattedText text = new FormattedText(parent.sequence.Substring(i, 1),
                    cf,
                    FlowDirection.LeftToRight,
                    tf,
                    12,    // Font size in pixels             
                    b);

                    dc.DrawText(text, new Point(pointWidth * (parent.peakIndex[i] - xBase) - text.Width / 2, 0));
                }


            }

            protected override Size MeasureOverride(Size constraint)
            {
                double w = parent.tracePlot.Width;
                if (Double.IsNaN(w))
                    w = 20;
                return new Size(w, 20);
            }
        }

        ChartPrimitive lineA;
        ChartPrimitive lineC;
        ChartPrimitive lineG;
        ChartPrimitive lineT;
        SequenceCanvas sequencePanel;
        string sequence;
        int[] peakIndex;

        public TracePlot()
        {
            InitializeComponent();



            lineA = new ChartPrimitive();
            lineC = new ChartPrimitive();
            lineG = new ChartPrimitive();
            lineT = new ChartPrimitive();
            lineA.Color = Colors.Green;
            lineC.Color = Colors.Blue;
            lineG.Color = Colors.Black;
            lineT.Color = Colors.Red;


            tracePlot.Primitives.Add(lineA);
            tracePlot.Primitives.Add(lineC);
            tracePlot.Primitives.Add(lineG);
            tracePlot.Primitives.Add(lineT);
            //tracePlot.RedrawPlotLines();

            lineA.HitTest = false;
            lineC.HitTest = false;
            lineG.HitTest = false;
            lineT.HitTest = false;

            lineA.ShowInLegend = false;
            lineC.ShowInLegend = false;
            lineG.ShowInLegend = false;
            lineT.ShowInLegend = false;


            tracePlot.IsDrawXGridAndTick = false;
            tracePlot.IsEnableClosestPointPicker = false;
        }

        public void Load(string fileName)
        {
            int[] A; int[] C; int[] G; int[] T;
            int[] probA; int[] probC; int[] probG; int[] probT;
            char[] bases;
            TraceIO.ScfRead(fileName, out A, out C, out G, out T,
                out probA, out probC, out probG, out probT, out peakIndex, out bases);
            sequence = new string(bases);

            for (int i = 0; i < A.Length; ++i)
            {

                lineA.AddPoint(i, A[i]);
                lineC.AddPoint(i, C[i]);
                lineG.AddPoint(i, G[i]);
                lineT.AddPoint(i, T[i]);

            }

            tracePlot.Width = probA.Length * 10;

            sequencePanel = new SequenceCanvas(this);
            tracePlot.AddLegendPanel(sequencePanel);

            tracePlot.RedrawPlotLines();
        }

    }
}
