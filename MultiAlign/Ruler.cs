using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace iMoBio.Controls
{
    public class Ruler : Panel
    {
        public enum RulerType
        {
            DoubleTick,
            SingleTick
        }

        public Ruler()
        {
            FontSize = 10;
            MajorTick = 10;
            Type = RulerType.DoubleTick;
            TickPen = new Pen(Brushes.Gray, 1.0);
            TickPen.Freeze();
            BorderPen = new Pen(Brushes.Gray, 1.0);
            BorderPen.Freeze();
        }

        public RulerType Type { set; get; }
        public int BaseCount { set; get; }
        public double BaseWidth { set; get; }
        public double FontSize { set; get; }
        public Pen TickPen { set; get; }
        public Pen BorderPen { set; get; }
        public int MajorTick { set; get; }
        public int MinorTick { set; get; }

        private double gap = 2.0;

        protected override Size MeasureOverride(Size availableSize)
        {
            double w = Width;
            if (Double.IsNaN(w))
                w = availableSize.Width;
            if (Double.IsNaN(w))
                w = 400;
            double h = Height;
            if (Double.IsNaN(h))
                h = Math.Min(availableSize.Height, 20);
            return new Size(w, h);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));
            dc.DrawLine(BorderPen, new Point(0, 1.0), new Point(ActualWidth, 1.0));
            dc.DrawLine(BorderPen, new Point(0, ActualHeight - 1.0), new Point(ActualWidth, ActualHeight - 1.0));

            if (BaseCount == 0)
                return;

            if (Double.IsNaN(BaseWidth) || BaseWidth <= 0.0)
                BaseWidth = ActualWidth / BaseCount;

            CultureInfo cf = CultureInfo.GetCultureInfo("en-us");
            Typeface tf = new Typeface("Lucida Console");
            FormattedText text = new FormattedText("1",
                        cf, FlowDirection.LeftToRight, tf, FontSize, Brushes.Black);

            double y0 = 0;
            double y1 = (ActualHeight - text.Height - 2 * gap) / 2;
            double y3 = ActualHeight;
            double y2 = ActualHeight - y1;

            for (int i = 0; i < BaseCount; i += MajorTick)
            {
                text = new FormattedText(i.ToString(),
                        cf, FlowDirection.LeftToRight, tf, FontSize, Brushes.Black);
                
                
                double baseX = i * BaseWidth;
                double x = baseX + text.Width / 2.0;

                dc.DrawText(text, new Point(baseX, y1 + gap));
                dc.DrawLine(TickPen, new Point(x, y0), new Point(x, y1));
                dc.DrawLine(TickPen, new Point(x, y2), new Point(x, y3));
            }


            

        }
    }
}
