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
using System.Windows.Controls.Primitives;
using BioCSharp.Seqs;
using BaseLibrary;
using System.Globalization;

namespace iMoBio.PlasmidCanvas
{
    /// <summary>
    /// Display <code>PlasmidRecord</code> in the form of wrapped sequence with linear stacked 
    /// annotation.
    /// </summary>
    public partial class SequenceCanvas : UserControl, ISeqCanvas
    {
        static public string CanvasType = "Sequence";

        #region variables initialized in SeqTextCanvas
        private double blockHeight;
        private int numberOfRows;
        private int numberOfSequencePerLine = UserSetting.INSTANT.SeqText_LineWidth;
        private double leftNumberingWidth;
        private double charWidth;
        private int wordSize = UserSetting.INSTANT.SeqText_WordSize;
        private int numberOfCharPerLine;
        #endregion

        LayerManager layerManager;

        private PlasmidRecord plasmid = null;

        protected Dictionary<Feature, FeatureUI> annotations = new Dictionary<Feature, FeatureUI>();

        /// <summary>
        /// Layer representing 5' strand
        /// </summary>
        Layer MainStrandLayer { get; set; }

        /// <summary>
        /// Layer representing 3' strand
        /// </summary>
        Layer ReverseStrandLayer { get; set; }

        Layer CuttingSiteLayer { get; set; }

        SeqTextBlock seqTextblock;

        public SequenceCanvas()
        {
            InitializeComponent();

            HorizontalAlignment = HorizontalAlignment.Stretch;

            layerManager = new LayerManager(this);
            layerManager.Offset = layerManager.LayerWidth;

           
            MainStrandLayer = new Layer(layerManager, Layer.LAYER_MAINSTRAND, false);
            ReverseStrandLayer = new Layer(layerManager, Layer.LAYER_REVERSESTRAND, false);
            ReverseStrandLayer.Visibility = Visibility.Hidden;
            CuttingSiteLayer = new Layer(layerManager, Layer.LAYER_RESTRICT, false);
            CuttingSiteLayer.Visibility = Visibility.Hidden;
            mainPanel.Children.Add(CuttingSiteLayer.Shape);

            seqTextblock = new SeqTextBlock(this);
            Canvas.SetZIndex(seqTextblock, Layer.ZINDEX_ANNOTATION - 1);

            //TickBar tb = new TickBar();

            mainPanel.Children.Add(seqTextblock);
        }


        #region ISeqCanvas Members

        public void AddChild(UIElement child) { throw new InvalidOperationException("Sequence Canvas draw itself"); }

        public void Redraw() { }

        public PlasmidRecord Plasmid
        {
            get
            {
                return plasmid;
            }
            set
            {
                plasmid = value;
                if (plasmid != null)
                    plasmid.SelectionToEvent += new RoutedEventHandler(OnSelectionChanged);
                Init();
            }
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {

            seqTextblock.OnSelectionChanged(sender, e);
        }

        public LayerManager LayerManager
        {
            get
            {
                return layerManager;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Init()
        {
            if (plasmid == null)
                return;

            #region Add layers
            // Each feature is a layer by our convention
            foreach (string key in plasmid.FeatureKeys)
            {
                Layer layer = layerManager[key];
                // for those don't have a layer
                if (layer == null)
                {
                    layer = new Layer(layerManager, key, true);
                    mainPanel.Children.Add(layer.Shape);
                }

                // draw shape
                // no shape 
            }
            #endregion

            // we call background text sequence first 
            // it will set width for respective layers
            seqTextblock.Sequence = plasmid.Sequence;
            
            //layerManager.Update();

            Height = seqTextblock.Height;
            Width = seqTextblock.Width;


            // Remove redundent annotation
            // TODO:

            #region Add annotation
            int bpTotal = Plasmid.BaseCount;
            foreach (Feature ft in plasmid.Features)
            {
                Layer layer = layerManager[ft.Key];
                FeatureUI annotation = new FeatureUI(this, layer, ft);
            
                annotations.Add(ft, annotation);

                int stackNoFirst = (int)Math.Floor(1.0 * annotation.Start / numberOfSequencePerLine);
                int stackNoLast = (int)Math.Floor(1.0 * annotation.End / numberOfSequencePerLine);

                double height = layer.Width;

                PathSegmentCollection segments = new PathSegmentCollection();
                PathGeometry shape = new PathGeometry();

                if (annotation.AnnotationShape == AnnotationShape.Rectangle || annotation.AnnotationShape == AnnotationShape.Arrow)
                {
                    GeometryGroup gg = new GeometryGroup();

                    Rect[] rects = DrawRects(annotation.Start, annotation.End, height, layer.Inner);
                    foreach (Rect rect in rects)
                    {
                        RectangleGeometry r = new RectangleGeometry(rect);
                        r.Freeze();
                        gg.Children.Add(r);
                    }

                    gg.Freeze();
                    annotation.Geometry = gg;
                }
            }

            #endregion

            OnSelectionChanged(this, null);
        }

        /// <summary>
        /// Provide rectangles for region of given sequence
        /// </summary>
        /// <param name="h">height of rectangle</param>
        /// <param name="start">start sequence no</param>
        /// <param name="end">end sequence no</param>
        /// <param name="inner">stack height</param>
        /// <returns></returns>
        private Rect[] DrawRects(long start, long end, double h, double inner)
        {
            List<Rect> rects = new List<Rect>();
            if (start > end)
            {
                long temp = end;
                end = start;
                start = temp;
            }

            // first rectangle (left transcated)
            long startBlockNo = start / numberOfSequencePerLine;
            long offsetBp = start % numberOfSequencePerLine;
            // Note: we put a space between each word E.g: 'atc aga tga' two spaces in three words
            long offsetX = offsetBp + offsetBp / wordSize;
            double x = leftNumberingWidth + charWidth * offsetX;
            double y = LayerManager.Ascent + inner + startBlockNo * blockHeight;
            long numChar = end - start;
            numChar = numChar + numChar / wordSize;
            double w = charWidth * numChar;
            double maxX = leftNumberingWidth + charWidth * numberOfCharPerLine;
            if (w + x > maxX)
            {
                w = maxX - x;
            }
            //Console.WriteLine("s: {6}, e: {7}, x: {0}, y: {1}, w: {2}, ox:{3}, bn: {4}, bh: {5}", x, y, w, offsetX, blockNo, blockHeight,
            //    range.Start, range.End);
            if (x >= 0 && y >= 0 && w >= 0 && h >= 0)
            {
                rects.Add(new Rect(x, y, w, h));
            }

            long endBlockNo = end / numberOfSequencePerLine;

            // middle rectangles
            w = charWidth * (numberOfCharPerLine - 1);
            x = leftNumberingWidth;
            for (long i = startBlockNo + 1; i < endBlockNo; i++)
            {
                y = i * blockHeight + LayerManager.Ascent + inner;
                rects.Add(new Rect(x, y, w, h));
            }

            // last rectangle (right transcated)
            if (endBlockNo > startBlockNo)
            {
                offsetBp = end % numberOfSequencePerLine;
                numChar = offsetBp + offsetBp / wordSize;
                w = charWidth * numChar;
                y = endBlockNo * blockHeight + LayerManager.Ascent + inner;
                if (x >= 0 && y >= 0 && w >= 0 && h >= 0)
                {
                    rects.Add(new Rect(x, y, w, h));
                }
            }

            return rects.ToArray();
        }

        #endregion


        /// <summary>
        /// Display large quantity of text and provide selection model suitable for sequence
        /// </summary>
        private class SeqTextBlock : Canvas
        {
            #region members
            private double textDescent = 5;
            private double fontSize = UserSetting.INSTANT.SeqText_FontSize;
            private bool? capatalized = UserSetting.INSTANT.SeqText_Capatalized;
            private Typeface typeface = new Typeface(UserSetting.INSTANT.SeqText_FontFamily);
            
            private List<Visual> visuals = new List<Visual>();
            private DrawingVisual selectionVisual = new DrawingVisual();

            private string sequence;

            readonly private SequenceCanvas parent;
            #endregion

            public SeqTextBlock(SequenceCanvas parent)
                : base()
            {
                this.parent = parent;

                AddVisual(selectionVisual);
            }

            /// <summary>
            /// Set sequence
            /// </summary>
            public string Sequence
            {
                set
                {
                    // clean the sequence
                    sequence = value;
                    sequence = sequence.Replace(" ", "").Replace("\t", "").Replace("\n", "");
                    if (capatalized != null)
                    {
                        if (capatalized.Value)
                        {
                            this.sequence = sequence.ToUpper();
                        }
                        else
                        {
                            this.sequence = sequence.ToLower();
                        }
                    }

                    // Sample extend of one line
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < parent.numberOfSequencePerLine; i++)
                        sb.Append("A");
                    // Create the initial formatted text string.
                    FormattedText seqText = new FormattedText(
                        sb.ToString(),
                        MediaExt.CultureInfoEnUS,
                        FlowDirection.LeftToRight,
                        typeface,
                        fontSize,
                        Brushes.Black);

                    // set layer width accordingly
                    parent.MainStrandLayer.Width = seqText.Height;
                    parent.MainStrandLayer.Descent = textDescent;
                    parent.MainStrandLayer.Offset = parent.MainStrandLayer.Width + parent.MainStrandLayer.Descent;
                    
                    parent.ReverseStrandLayer.Width = seqText.Height;
                    parent.ReverseStrandLayer.Descent = textDescent;
                    parent.ReverseStrandLayer.Offset = parent.ReverseStrandLayer.Width + parent.ReverseStrandLayer.Descent;

                    // draw in visual to speed up the rendering
                    int mantissa = (int)Math.Ceiling(Math.Log10(sequence.Length));
                    parent.blockHeight = parent.LayerManager.Width;
                    parent.charWidth = seqText.Width / parent.numberOfSequencePerLine;
                    parent.leftNumberingWidth = (mantissa + 2) * parent.charWidth;
                    parent.numberOfCharPerLine = parent.numberOfSequencePerLine + (parent.numberOfSequencePerLine / parent.wordSize);

                    // make sure that wordSize is positive
                    if (parent.wordSize <= 0)
                    {
                        parent.wordSize = parent.numberOfSequencePerLine;
                    }

                    DrawingVisual visual = new DrawingVisual();
                    DrawingContext dc = visual.RenderOpen();

                    int nb = 0;
                    double top = parent.LayerManager.Ascent;
                    double left = 0.0;


                    for (int i = 0; i * parent.numberOfSequencePerLine < sequence.Length; i++)
                    {
                        nb = i * parent.numberOfSequencePerLine;
                        int len = Math.Min(parent.numberOfSequencePerLine, Math.Abs(sequence.Length - nb));
                        string text = (nb + 1).ToString().PadLeft(mantissa + 1) + " ";
                        if (parent.wordSize > 0)
                        {
                            sb = new StringBuilder(text);
                            for (int j = 0; j < parent.numberOfSequencePerLine; j += parent.wordSize)
                            {
                                if (nb + j > sequence.Length) break;
                                int sbLen = Math.Min(parent.wordSize, Math.Abs(sequence.Length - (nb + j)));
                                sb.Append(sequence.Substring(nb + j, sbLen) + " ");
                            }
                            sb.Remove(sb.Length - 1, 1);
                            text = sb.ToString();
                        }
                        else
                        {
                            text += sequence.Substring(nb, len);
                        }

                        seqText = new FormattedText(text,
                            MediaExt.CultureInfoEnUS,
                            FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);

                        dc.DrawText(seqText, new Point(left, i * parent.blockHeight + top));

                        parent.numberOfRows = i + 1;
                        if (i == 0)
                        {
                            // this assume?
                            Width = seqText.Width;
                        }
                    }

                    parent.LayerManager.PlasmidLength = Width - parent.leftNumberingWidth;
                    parent.LayerManager.Left = parent.leftNumberingWidth;

                    dc.Close();
                    AddVisual(visual);

                    // set height and width
                    Height = top + parent.blockHeight * parent.numberOfRows;
                }
                get
                {
                    return sequence;
                }

            }



            #region event handling

            public void OnSelectionChanged(object sender, RoutedEventArgs e)
            {
                DrawingContext dc = selectionVisual.RenderOpen();
                if (parent.Plasmid.Selection == null)
                {
                    dc.Close();
                    return;
                }

                List<Range> selection = parent.Plasmid.Selection;

                double h = parent.LayerManager[Layer.LAYER_MAINSTRAND].Width;
                if (h < 0)
                    return;
                //Console.WriteLine(this.ToString() + ": no: " + parent.PlasmidRecord.Selection.Count + 
                //    ", Start: " + parent.PlasmidRecord.Selection[0].Start + ", End: " + parent.PlasmidRecord.Selection[0].End);
                
                foreach (Range range in parent.Plasmid.Selection)
                {
                    Rect[] rects = parent.DrawRects(range.Start, range.End, h, 0);
                    foreach (Rect r in rects)
                    {
                        dc.DrawRectangle(PlasmidCanvas.SelectionBrush, null, r);
                    }
                }
                dc.Close();
            }

            /// <summary>
            /// Calculate location of sequence at the point <code>location</code>. 
            /// </summary>
            /// <param name="location">Mouse pointer location</param>
            /// <returns>Number of base (or sequence) at the point. If <code>location</code> is not on the sequence text, -1 is return.</returns>
            private long GetSelectionBase(Point location)
            {
                double locY = location.Y - parent.LayerManager.Ascent - parent.blockHeight * Math.Floor(location.Y / parent.blockHeight);
                double locX = location.X - parent.leftNumberingWidth;
                if (locX > -parent.charWidth && locX <= (parent.numberOfCharPerLine + 1) * parent.charWidth &&
                    locY >= 0 && locY <= parent.LayerManager[Layer.LAYER_MAINSTRAND].Width)
                {
                    long bp = (long)Math.Round(locX / parent.charWidth);
                    //Console.WriteLine("bp: {0}, {1}", bp, bp - (long)Math.Floor(1.0 * bp / (wordSize + 1)));
                    bp = bp - (long)Math.Floor(1.0 * bp / (parent.wordSize + 1)); // minus empty character
                    int blockNo = (int)Math.Floor((location.Y - parent.LayerManager.Ascent) / parent.blockHeight);
                    bp = bp + blockNo * parent.numberOfSequencePerLine;
                    return bp;
                }
                else
                {
                    return -1;
                }
            }

            //Random r = new Random();
            protected override void OnMouseMove(MouseEventArgs e)
            {
                // We show Ibeam cusor on text, otherwise normal arrow

                Point loc = e.GetPosition(this);
                long bp = GetSelectionBase(loc);
                if (bp >= 0L)
                {
                    Cursor = Cursors.IBeam;
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        // Do a selection
                        parent.Plasmid.SelectTo(bp);
                    }
                }
                else
                {
                    Cursor = Cursors.Arrow;
                }

                e.Handled = true;
            }

            protected override void OnMouseDown(MouseButtonEventArgs e)
            {
                Point loc = e.GetPosition(this);
                long bp = GetSelectionBase(loc);
                if (bp >= 0)
                {
                    Cursor = Cursors.IBeam;
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        // TODO: multiple selection feature
                        // Start a selection
                        parent.Plasmid.SelectStart(bp);
                        //Console.WriteLine("To: " + bp + blockNo * numberOfCharPerLine);
                    }
                }
                else
                {
                    Cursor = Cursors.Arrow;
                }
                base.OnMouseDown(e);
            }


           
            #endregion


            #region Visual layer programming
            protected override int VisualChildrenCount
            {
                get
                {
                    return visuals.Count;
                }
            }

            protected override Visual GetVisualChild(int index)
            {
                return visuals[index];
            }

            public void AddVisual(Visual visual)
            {
                visuals.Add(visual);

                base.AddVisualChild(visual);
                base.AddLogicalChild(visual);
            }

            public void DeleteVisual(Visual visual)
            {
                visuals.Remove(visual);

                base.RemoveVisualChild(visual);
                base.RemoveLogicalChild(visual);
            }
            #endregion

        }

        #region ISeqCanvas Members


        public string Zoom(string zoomValue)
        {
            // zoom is yet to be implemented
            return zoomValue;
        }

        #endregion
    }
}
