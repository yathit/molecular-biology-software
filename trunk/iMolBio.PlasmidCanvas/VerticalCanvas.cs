using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BioCSharp.Seqs;
using System.Windows.Controls;
using System.Windows.Media;
using MathLib;
using BaseLibrary;

namespace iMoBio.PlasmidCanvas
{
    public class VerticalCanvas : LinearCanvas
    {
        static public string CanvasType = "Vertical";

        public VerticalCanvas()
            : base()
        {
            _top = 100;
            _left = 50;
            MinHeight = 600;
            MinWidth = 600;
        }

        override public void Redraw()
        {
            #region Draw annotation
            int bp = PlasmidRecord.BaseCount;
            Point plasmidLocation = Location;
            double plasmidLength = Length;
            var visibles = annotations.Where(annotation => annotation.Visible);
            int numVisible = visibles.Count();
            double[] pos = (double[])visibles.Select(annotation => plasmidLocation.Y + (annotation.Start * plasmidLength / bp)).ToArray();

            // calculate the default position for every label
            double[] ly = new double[pos.Length];
            pos.CopyTo(ly, 0);


            // adjust the vertical position of the labels
            int[] h = pos.Sort();
            // ly = new double[]{0.00255161215495245, 0.00939457202505219, 0.116794247274414, 0.19925771282765, 0.606123869171886, 0.737763859893296, 0.744722802134076, 0.795523080491765};
            // ly = new double[]{0.351426583159360,   0.618070053351890,   0.918116446300162,   0.875550916260728};
            double labelHeight = visibles.First().LabelHeight;
            pos = LinearCanvas.SpaceAdjust(ly, labelHeight);
            for (int idx = 0; idx < h.Length; idx++)
            {
                ly[h[idx]] = pos[idx];
            }


            int county = -1;
            foreach (FeatureUI annotation in visibles)
            {
                Layer layer = layerManager[annotation.Key];
                county++;

                PathGeometry pathGeometry = new PathGeometry();

                double shoulderRatio = 0.25;
                double shoulder2neckLengthRatio = 2;


                double height = layer.Width;
                // top-left start point
                double x = plasmidLocation.X;
                double y = annotation.Start * plasmidLength / bp;

                if (annotation.Key == Feature.KEY_restriction_site)
                {
                    // restriction site are indicate by two-segment line
                    double y2 = annotation.End * plasmidLength / bp;

                    PathSegmentCollection segments = new PathSegmentCollection();
                    segments.Add(new LineSegment(new Point(x + height / 2.0, y), true));
                    segments.Add(new LineSegment(new Point(x + height, y2), true));

                    PathFigure fig = new PathFigure(new Point(x, y), segments, false);

                    fig.Freeze();
                    pathGeometry.Figures.Add(fig);
                }
                else
                {
                    double length = Math.Abs((annotation.End - annotation.Start) * plasmidLength / bp);
                    double breadth = height * (1 - shoulderRatio);
                    double shoulder = height * shoulderRatio;
                    double neckLength = shoulder * shoulder2neckLengthRatio;

                    PathSegmentCollection segments = new PathSegmentCollection();

                    Point startPoint = new Point(x, y);
                    //if (neckLength <= 0 || shoulder <= 0 || neckLength > width)
                    //{
                    segments.Add(new LineSegment(new Point(x, y + length), true));
                    segments.Add(new LineSegment(new Point(x + height, y + length), true));
                    segments.Add(new LineSegment(new Point(x + height, y), true));


                    //}
                    //else
                    //{
                    //    segments.Add(new LineSegment(new Point(x + length - neckLength, y + shoulder), isStroke));
                    //    segments.Add(new LineSegment(new Point(x + length - neckLength, y), isStroke));
                    //    segments.Add(new LineSegment(new Point(x + length, y + height / 2), isStroke));
                    //    segments.Add(new LineSegment(new Point(x + length - neckLength, y + height), isStroke));
                    //    segments.Add(new LineSegment(new Point(x + length - neckLength, y + height - shoulder), isStroke));
                    //    segments.Add(new LineSegment(new Point(x, y + height - shoulder), isStroke));
                    //}

                    PathFigure fig = new PathFigure(startPoint, segments, true);

                    fig.Freeze();
                    pathGeometry.Figures.Add(fig);

                }
                pathGeometry.Freeze();
                annotation.Geometry = pathGeometry;

                // place label
                annotation.LabelPosition = new Point(plasmidLocation.X + height + spacingLabel, ly[county]);

                // draw line


            }
            #endregion



            // seleciton canvas
            double additionalSpace = 5;
            Canvas.SetTop(backRectangle, Top - additionalSpace);
            Canvas.SetLeft(backRectangle, Left);
            backRectangle.Height = layerManager.Width + additionalSpace * 2;
            backRectangle.Width = Length;

            foreach (FeatureUI a in annotations) a.Update();

            OnSelectionChanged(this, null);
        }

        #region Helper functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l1">internal limits</param>
        /// <param name="l2">external limits</param>
        /// <param name="vertext">vertical extent of the labels</param>
        /// <param name="horline">length of the horizontal part of the line to the annotation</param>
        /// <param name="pos">origin points for every label</param>
        /// <param name="nLabels">number of cell string with the annotations</param>
        static double[] linearAnnotations(double l1, double l2, double vertext, double pos, int nLabels)
        {
            // calculate the default position for every label
            double[] lx = new double[nLabels];
            double[] ly = new double[nLabels];
            for (int i = 0; i < nLabels; i++) lx[i] = l2;
            for (int i = 0; i < nLabels; i++) ly[i] = pos;


            // adjust the vertical position of the labels
            int[] h = ly.SortIndex();
            double[] lyAdj = LinearCanvas.SpaceAdjust((double[])ly.Select((x, i) => ly[i]), vertext);
            for (int idx = 0; idx < h.Length; idx++)
            {
                ly[h[idx]] = lyAdj[idx];
            }

            return ly.ToArray();
        }

        #endregion
    }
}
