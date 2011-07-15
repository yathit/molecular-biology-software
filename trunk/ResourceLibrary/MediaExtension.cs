using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace BaseLibrary
{
    /// <summary>
    /// Utitlities related to <code>System.Windows.Media</code>
    /// </summary>
    public class MediaExt
    {
        public static CultureInfo CultureInfoEnUS = CultureInfo.GetCultureInfo("en-us");
        public static double CharWidth(double size)
        {
            size = ((int)size * 10) / 10;
            if (!_charWidths.ContainsKey(size))
            {
                FindCharSize(size);
            }
            return _charWidths[size];
        }
        public static double CharHeight(double size)
        {
            size = ((int)size * 10) / 10;
            if (!_charHeight.ContainsKey(size))
            {
                FindCharSize(size);
            }
            return _charHeight[size];
        }
        private static Dictionary<double, double> _charWidths = new Dictionary<double, double>();
        private static Dictionary<double, double> _charHeight = new Dictionary<double, double>();
        private static void FindCharSize(double size)
        {
            FormattedText txt = new FormattedText(
                        "a",
                        CultureInfoEnUS,
                        FlowDirection.LeftToRight,
                        new Typeface("Lucida Console"),
                        size,
                        Brushes.Black);
            _charWidths[size] = txt.Width;
            _charHeight[size] = txt.Height;
        }

        public static double FindTextWidth(string text, double fontSize, FontFamily tf, FontStyle style,
            FontWeight weight, FontStretch st)
        {
            FormattedText txt = new FormattedText(
                        text,
                        CultureInfoEnUS,
                        FlowDirection.LeftToRight,
                        new Typeface(tf, style, weight, st),
                        fontSize,
                        Brushes.Black);

            return txt.Width;
        }

        /// <summary>
        /// Contruct an arc segment from center point, angle, start point input arguemnts
        /// </summary>
        /// <param name="centerPoint">center point</param>
        /// <param name="startPoint">segment start point</param>
        /// <param name="theta">angle (radian) of arc in counter-clockwise direction from x-axis</param>
        /// <returns>create</returns>
        public static ArcSegment ArcSegment(Point centerPoint, Point startPoint, double theta, bool isStroked)
        {
            // Note: y direction of graphic axis and cartisian coordinate are opposite direction
            double dx = (startPoint.X - centerPoint.X);
            double dy = -(startPoint.Y - centerPoint.Y);
            double radious = Math.Sqrt(dx * dx + dy * dy);
            double baseAngle = Math.Atan(dy / dx);
            if (dx < 0)
            {
                baseAngle = Math.PI + baseAngle;
            }

            Point endPoint = new Point(centerPoint.X + radious * Math.Cos(baseAngle - theta),
                centerPoint.Y - radious * Math.Sin(baseAngle - theta));
            Size size = new Size(radious, radious);
            double rotationAngle = 0;

            bool isLargeArc = Math.Abs(theta) > Math.PI;
            SweepDirection sweepDirection = theta < 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

            return new ArcSegment(endPoint, size, rotationAngle, isLargeArc, sweepDirection, isStroked);
        }

        /// <summary>
        /// Contruct an arc segment from center point, angle, start point input arguemnts
        /// </summary>
        /// <param name="centerPoint">center point</param>
        /// <param name="startPoint">segment start point</param>
        /// <param name="theta">angel of arc</param>
        /// <returns></returns>
        public static ArcSegment ArcSegment(Point centerPoint, Point startPoint, double theta)
        {
            return ArcSegment(centerPoint, startPoint, theta, true);
        }
    }
}

