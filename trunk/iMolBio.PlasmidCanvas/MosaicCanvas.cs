using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using BioCSharp.Seqs;
using BaseLibrary;

namespace iMoBio.PlasmidCanvas
{

    

    public class MosaicCanvas : PlasmidCanvas
    {
        static public string CanvasType = "Mosaic";

        OverviewPanel overviewPanel;
        DetailPanel mainCanvas;
        DockPanel dock;
        int CurrentBlockNo = 0;
        int NumberOfSequencePerLine = -1;
        bool IsNumberOfSequencePerLineSet = false;
        int WordSize = 10;

        public MosaicCanvas()
            : base()
        {
            layerManager.LayerWidth = 20;
            MinHeight = 20.0;
            MinWidth = 60.0;

            MainStrand = new Layer(layerManager, Layer.LAYER_MAINSTRAND, false);
            //Children.Add(MainStrandLayer.Shape);
            ReverseStrand = new Layer(layerManager, Layer.LAYER_REVERSESTRAND, false);
            //Children.Add(ReverseStrandLayer.Shape);
            RestrictSite = new Layer(layerManager, Layer.LAYER_RESTRICT, false);
            //Children.Add(CuttingSiteLayer.Shape);

            mainCanvas = new DetailPanel(this);
            overviewPanel = new OverviewPanel(this);

            dock = new DockPanel();
            DockPanel.SetDock(overviewPanel, Dock.Right);
            dock.Children.Add(overviewPanel);
            dock.Children.Add(mainCanvas);
            
            Children.Add(dock);
        }

        public override string Zoom(string zoomValue)
        {
            throw new NotImplementedException();
        }

        public override void OnSelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public override void Redraw()
        {
            throw new NotImplementedException();
        }

        public override void Init()
        {
            #region Add layers
            // Each feature is a layer by our convention
            foreach (string key in plasmid.FeatureKeys)
            {
                Layer layer = layerManager[key];
                // for those don't have a layer
                if (layer == null)
                {
                    layer = new Layer(layerManager, key, true);
                }

                // draw shape
                // no shape 
            }
            #endregion


            // Remove redundent annotation
            // TODO:

            #region Add annotation
            int bpTotal = PlasmidRecord.BaseCount;
            annotations = new FeatureUI[plasmid.FeatureCount];
            for (int i = 0; i < plasmid.FeatureCount; i++)
            {
                Layer layer = layerManager[plasmid.Features[i].Key];
                FeatureUI annotation = new FeatureUI(this, layer, plasmid.Features[i]);
                annotations[i] = annotation;
            }

            #endregion
        }

        class DetailPanel : Panel
        {
            MosaicCanvas Owner;
            double gap = 5.0;
            double BlockHeight = 20.0;

            public DetailPanel(MosaicCanvas Owner)
                : base()
            {
                this.Owner = Owner;
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                Console.WriteLine("M: {0}: {1}", this.ToString(), availableSize);
                if (Owner.IsNumberOfSequencePerLineSet || Owner.NumberOfSequencePerLine <= 0)
                {
                    double charWidth = MediaExt.CharWidth(UserSetting.INSTANT.SeqText_FontSize);
                    int NumberOfCharPerLine = (int) Math.Floor((availableSize.Width - 2 * gap) / charWidth);
                    if (Owner.WordSize > 0 & Owner.WordSize < NumberOfCharPerLine)
                    {
                        int NumOfWords = (int)Math.Floor((1.0 * NumberOfCharPerLine) / (Owner.WordSize + 1));
                        Owner.NumberOfSequencePerLine = NumOfWords * Owner.WordSize;
                    }
                    else
                    {
                        Owner.NumberOfSequencePerLine = NumberOfCharPerLine;
                    }
                }
                return availableSize; // take all
            }

            //protected override Size ArrangeOverride(Size finalSize)
            //{
            //    Console.WriteLine("A: {0}: {1}", this.ToString(), finalSize);
                
            //    return finalSize;
            //}

            protected override void OnRender(DrawingContext dc)
            {
                
                int seqLen = Owner.PlasmidRecord.Length;
                int mantissa = (int)Math.Ceiling(Math.Log10(seqLen));
                double charWidth = MediaExt.CharWidth(UserSetting.INSTANT.SeqText_FontSize);
                double left = (mantissa + 2) * charWidth;

                for (int i = Owner.CurrentBlockNo; i*Owner.NumberOfSequencePerLine < Owner.PlasmidRecord.BaseCount; ++i)
                {
                    if ((i + 1) * BlockHeight > Height)
                        return;

                    int nb = i * Owner.NumberOfSequencePerLine;
                    int len = Math.Min(Owner.NumberOfSequencePerLine, Math.Abs(seqLen - nb));
                    string text = (nb + 1).ToString().PadLeft(mantissa + 1) + " ";
                    if (Owner.WordSize > 0 & Owner.WordSize < Owner.NumberOfSequencePerLine)
                    {
                        StringBuilder sb = new StringBuilder(text);
                        for (int j = 0; j < Owner.NumberOfSequencePerLine; j += Owner.WordSize)
                        {
                            if (nb + j > seqLen) break;
                            int sbLen = Math.Min(Owner.WordSize, Math.Abs(seqLen - (nb + j)));
                            sb.Append(Owner.PlasmidRecord.Seq.Substring(nb + j, sbLen) + " ");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        text = sb.ToString();
                    }
                    else
                    {
                        text += Owner.PlasmidRecord.Seq.Substring(nb, len);
                    }

                    FormattedText seqText = new FormattedText(text,
                        MediaExt.CultureInfoEnUS,
                        FlowDirection.LeftToRight, UserSetting.INSTANT.SeqText_Typeface, 
                        UserSetting.INSTANT.SeqText_FontSize, Brushes.Black);

                    dc.DrawText(seqText, new Point(left, i * BlockHeight));
                }

            }
        }

        class OverviewPanel : Panel
        {
            MosaicCanvas Owner;

            public OverviewPanel(MosaicCanvas Owner)
                : base()
            {
                this.Owner = Owner;
            }

            //protected override Size MeasureOverride(Size availableSize)
            //{
            //    Console.WriteLine("M: {0}: {1}", this.ToString(), availableSize);
            //    return new Size(80.0, availableSize.Height);
            //}

            //protected override Size ArrangeOverride(Size finalSize)
            //{
            //    Console.WriteLine("A: {0}: {1}", this.ToString(), finalSize);
            //    Width = finalSize.Width;
            //    Height = finalSize.Height;

            //    return new Size(Width, Height);
            //}

            protected override void OnRender(DrawingContext dc)
            {
                #region Draw annotation
                int bp = Owner.PlasmidRecord.BaseCount;
                double numOfBlockd = (1.0 * bp) / Owner.NumberOfSequencePerLine;
                int numOfBlock = (int)Math.Ceiling(numOfBlockd);
                double charWidth = Width / Owner.NumberOfSequencePerLine;
                double blockHeight = Height / numOfBlockd;
                double maxWidth = charWidth * Owner.NumberOfSequencePerLine;

                foreach (FeatureUI annotation in Owner.annotations)
                {
                    Layer layer = Owner.layerManager[annotation.Key];
                    int start = annotation.Start;
                    int end = annotation.End;
                    
                    PathFigure figure = new PathFigure();
                    

                    // first line (left transcated)
                    long startBlockNo = start / Owner.NumberOfSequencePerLine;
                    long offsetX = start % Owner.NumberOfSequencePerLine;
                    double x = charWidth * offsetX;
                    double y = startBlockNo * blockHeight;
                    double startX = x;
                    double startY = y + blockHeight;
                    long numChar = end - start;
                    double w = Math.Min(charWidth * numChar, maxWidth);
                    //dc.DrawLine(annotation.Pen, new Point(x, y), new Point(w, y));
                    figure.StartPoint = new Point(x, startY);
                    figure.Segments.Add(new LineSegment(new Point(x, y), true));
                    figure.Segments.Add(new LineSegment(new Point(w, y), true));
                    
                    long endBlockNo = end / Owner.NumberOfSequencePerLine;

                    // middle lines
                    //for (long i = startBlockNo + 1; i < endBlockNo; i++)
                    //{
                    //    y = i * blockHeight;
                    //    dc.DrawLine(annotation.Pen, new Point(0.0, y), new Point(maxWidth, y));
                    //}
                    

                    // last line (right transcated)
                    if (endBlockNo > startBlockNo)
                    {
                        numChar = end % Owner.NumberOfSequencePerLine;
                        w = charWidth * numChar;
                        y = endBlockNo * blockHeight;
                        // dc.DrawLine(annotation.Pen, new Point(0.0, y), new Point(w, y));
                        figure.Segments.Add(new LineSegment(new Point(maxWidth, y - blockHeight), true));
                        figure.Segments.Add(new LineSegment(new Point(w, y - blockHeight), true));
                        figure.Segments.Add(new LineSegment(new Point(w, y), true));
                        figure.Segments.Add(new LineSegment(new Point(0.0, y), true));
                        figure.Segments.Add(new LineSegment(new Point(0.0, startY), true));
                    }
                    else
                    {
                        figure.Segments.Add(new LineSegment(new Point(w, startY), true));
                    }

                    figure.IsClosed = true;
                    PathGeometry geo = new PathGeometry(new PathFigure[]{figure});
                    dc.DrawGeometry(annotation.Fill, null, geo);
                }
                #endregion
            }
        }
    }
}
