using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using BaseLibrary;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace iMolBio
{
    namespace PlasmidCanvas
    {

        
        /// <summary>
        /// Display large quantity of text and provide selection model suitable for sequence
        /// </summary>
        public class SeqTextBlock : Canvas
        {
            private int wordSize = UserSetting.INSTANT.SeqText_WordSize;
            private int numberOfSequencePerLine = UserSetting.INSTANT.SeqText_LineWidth;
            private double fontSize = UserSetting.INSTANT.SeqText_FontSize;
            private bool? capatalized = UserSetting.INSTANT.SeqText_Capatalized;
            Typeface typeface = new Typeface(UserSetting.INSTANT.SeqText_FontFamily);

            private List<Visual> visuals = new List<Visual>();
            private DrawingVisual selectionVisual = new DrawingVisual();

            private string sequence;

            private double blockHeight ;
            private int numberOfRows;
            private double leftNumberingWidth;
            private double charWidth;
            private int numberOfCharPerLine;

            readonly private SequenceCanvas parent;

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
                    for (int i = 0; i < numberOfSequencePerLine; i++)
                        sb.Append("A");
                    // Create the initial formatted text string.
                    FormattedText seqText = new FormattedText(
                        sb.ToString(),
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        typeface,
                        fontSize,
                        Brushes.Black);

                    // set layer width accordingly
                    parent.LayerManager.GetLayer(Layers.Layer.LAYER_MAINSTRAND).Width = seqText.Height;
                    parent.LayerManager.GetLayer(Layers.Layer.LAYER_REVERSESTRAND).Width = seqText.Height;

                    // draw in visual to speed up the rendering
                    int mantissa = (int)Math.Ceiling(Math.Log10(sequence.Length));
                    blockHeight = parent.LayerManager.Width;
                    charWidth = seqText.Width / numberOfSequencePerLine;
                    leftNumberingWidth = (mantissa + 2) * charWidth;
                    numberOfCharPerLine = numberOfSequencePerLine + (numberOfSequencePerLine / wordSize);

                    // make sure that wordSize is positive
                    if (wordSize <= 0)
                    {
                        wordSize = numberOfSequencePerLine;
                    }

                    DrawingVisual visual = new DrawingVisual();
                    DrawingContext dc = visual.RenderOpen();

                    int nb = 0;
                    double top = parent.LayerManager.Ascent;
                    double left = 0.0;


                    for (int i = 0; i * numberOfSequencePerLine < sequence.Length; i++)
                    {
                        nb = i * numberOfSequencePerLine;
                        int len = Math.Min(numberOfSequencePerLine, Math.Abs(sequence.Length - nb));
                        string text = (nb + 1).ToString().PadLeft(mantissa + 1) + " ";
                        if (wordSize > 0)
                        {
                            sb = new StringBuilder(text);
                            for (int j = 0; j < numberOfSequencePerLine; j += wordSize)
                            {
                                if (nb + j > sequence.Length) break;
                                int sbLen = Math.Min(wordSize, Math.Abs(sequence.Length - (nb+j)));
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
                            CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);

                        dc.DrawText(seqText, new Point(left, i * blockHeight + top));

                        numberOfRows = i + 1;
                        if (i == 0)
                        {
                            // this assume?
                            Width = seqText.Width;
                        }
                    }

                    parent.LayerManager.PlasmidLength = Width - leftNumberingWidth;
                    parent.LayerManager.Left = leftNumberingWidth;

                    dc.Close();
                    AddVisual(visual);

                    // set height and width
                    Height = top + blockHeight * numberOfRows;
                }
                get
                {
                    return sequence;
                }

            }

            

            #region event handling

            public void OnSelectionChanged(object sender, RoutedEventArgs e)
            {
                //if (!parent.IsVisible)
                //    return;

                List<Range> selection = parent.PlasmidRecord.Selection;
                
                DrawingContext dc = selectionVisual.RenderOpen();
                double h = parent.LayerManager.GetLayer(Layers.Layer.LAYER_MAINSTRAND).Width;
                if (h < 0)
                    return;
                //Console.WriteLine(this.ToString() + ": no: " + parent.PlasmidRecord.Selection.Count + 
                //    ", Start: " + parent.PlasmidRecord.Selection[0].Start + ", End: " + parent.PlasmidRecord.Selection[0].End);
                foreach (Range range in parent.PlasmidRecord.Selection)
                {
                    long start = range.Start;
                    long end = range.End;
                    if (start > end)
                    {
                        long temp = end;
                        end = start;
                        start = temp;
                    }

                    // first rectangle (left transcated)
                    long startBlockNo = start / numberOfSequencePerLine;
                    long offsetBp =  start % numberOfSequencePerLine;
                    long offsetX = offsetBp + offsetBp / wordSize;
                    double x = leftNumberingWidth + charWidth * offsetX;
                    double y = parent.LayerManager.Ascent + startBlockNo * blockHeight;
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
                        dc.DrawRectangle(Brushes.LightSkyBlue, null,
                            new Rect(x, y, w, h));
                    }

                    long endBlockNo = end / numberOfSequencePerLine;

                    // middle rectangles
                    w = charWidth * (numberOfCharPerLine - 1);
                    x = leftNumberingWidth;
                    for (long i = startBlockNo+1; i < endBlockNo; i++)
                    {
                        y = i * blockHeight + parent.LayerManager.Ascent;
                        dc.DrawRectangle(Brushes.LightSkyBlue, null,
                            new Rect(x, y, w, h));
                    }

                    // last rectangle (right transcated)
                    if (endBlockNo > startBlockNo)
                    {
                        offsetBp = end % numberOfSequencePerLine;
                        numChar = offsetBp + offsetBp / wordSize;
                        w = charWidth * numChar;
                        y = endBlockNo * blockHeight + parent.LayerManager.Ascent;
                        if (x >= 0 && y >= 0 && w >= 0 && h >= 0)
                        {
                            dc.DrawRectangle(Brushes.LightSkyBlue, null,
                                new Rect(x, y, w, h));
                        }
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
                double locY = location.Y -  parent.LayerManager.Ascent - blockHeight * Math.Floor(location.Y / blockHeight);
                double locX = location.X - leftNumberingWidth;
                if (locX > -charWidth && locX <= (numberOfCharPerLine + 1) * charWidth &&
                    locY >= 0 && locY <= parent.LayerManager.GetLayer(Layers.Layer.LAYER_MAINSTRAND).Width)
                {
                    long bp = (long)Math.Round(locX / charWidth);
                    //Console.WriteLine("bp: {0}, {1}", bp, bp - (long)Math.Floor(1.0 * bp / (wordSize + 1)));
                    bp = bp - (long)Math.Floor(1.0 * bp / (wordSize+1)); // minus empty character
                    int blockNo = (int)Math.Floor((location.Y -  parent.LayerManager.Ascent) / blockHeight);
                    bp = bp + blockNo * numberOfSequencePerLine;
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
                        parent.PlasmidRecord.SelectTo(bp);
                    }
                }
                else
                {
                    Cursor = Cursors.Arrow;
                }

                base.OnMouseMove(e);
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
                        parent.PlasmidRecord.SelectStart(bp);
                        //Console.WriteLine("To: " + bp + blockNo * numberOfCharPerLine);
                    }
                }
                else
                {
                    Cursor = Cursors.Arrow;
                }
                base.OnMouseDown(e);
            }

            protected override void OnMouseEnter(MouseEventArgs e)
            {
                this.Cursor = Cursors.IBeam;
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(MouseEventArgs e)
            {
                Cursor = Cursors.Arrow;
                base.OnMouseLeave(e);
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
    }
}
