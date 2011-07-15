using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BioCSharp.Seqs;
using System.Windows.Media;
using System.Windows.Controls;

namespace iMoBio.PlasmidCanvas
{
    public class HorizontalCanvas : LinearCanvas
    {
        static public string CanvasType = "Horizontal";

        public HorizontalCanvas()
            : base()
        {
            _top = 100;
            _left = 50;
            MinHeight = 200;
            MinWidth = 600;
        }

        override public void Redraw()
        {
            #region Draw annotation
            int bp = PlasmidRecord.BaseCount;
            Point plasmidLocation = Location;
            double plasmidLength = Length;
            foreach (FeatureUI annotation in annotations)
            {
                Layer layer = layerManager[annotation.Key];

                PathSegmentCollection segments = new PathSegmentCollection();
                bool isStroke = true;

                double shoulderRatio = 0.25;
                double shoulder2neckLengthRatio = 2;

                // top-left start point
                double x = plasmidLocation.X + (annotation.Start * plasmidLength / bp);
                double y = plasmidLocation.Y;

                double height = layer.Width;
                double length = Math.Abs((annotation.End - annotation.Start) * plasmidLength / bp);
                double breadth = height * (1 - shoulderRatio);
                double shoulder = height * shoulderRatio;
                double neckLength = shoulder * shoulder2neckLengthRatio;

                Point startPoint = new Point(x, y + shoulder);
                if (neckLength <= 0 || shoulder <= 0 || neckLength > length)
                {
                    segments.Add(new LineSegment(new Point(x + length, y + shoulder), isStroke));
                    segments.Add(new LineSegment(new Point(x + length, y + height - shoulder), isStroke));
                    segments.Add(new LineSegment(new Point(x, y + height - shoulder), isStroke));
                }
                else
                {
                    segments.Add(new LineSegment(new Point(x + length - neckLength, y + shoulder), isStroke));
                    segments.Add(new LineSegment(new Point(x + length - neckLength, y), isStroke));
                    segments.Add(new LineSegment(new Point(x + length, y + height / 2), isStroke));
                    segments.Add(new LineSegment(new Point(x + length - neckLength, y + height), isStroke));
                    segments.Add(new LineSegment(new Point(x + length - neckLength, y + height - shoulder), isStroke));
                    segments.Add(new LineSegment(new Point(x, y + height - shoulder), isStroke));
                }

                PathFigure annotationFigure = new PathFigure(startPoint, segments, true);

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(annotationFigure);
                pathGeometry.Freeze();
                annotationFigure.Freeze();

                annotation.Geometry = pathGeometry;

                // place label
                annotation.LabelPosition = new Point(x + length / 2.0, y - height - 10);
            }
            #endregion

            foreach (FeatureUI a in annotations) a.Update();

            // seleciton canvas
            double additionalSpace = 5;
            Canvas.SetTop(backRectangle, Top - additionalSpace);
            Canvas.SetLeft(backRectangle, Left);
            backRectangle.Height = layerManager.Width + additionalSpace * 2;
            backRectangle.Width = Length;

            OnSelectionChanged(this, null);
        }
    }
}
