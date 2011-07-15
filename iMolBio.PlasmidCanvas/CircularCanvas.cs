using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BioCSharp.Seqs;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Resources;
using System.Text.RegularExpressions;
using MathLib;
using System.Windows.Controls;
using BaseLibrary;


namespace iMoBio.PlasmidCanvas
{
  

    public class CircularCanvas : PlasmidCanvas
    {
        static public string CanvasType = "Circular";

        /// <summary>
        /// Space between border and outermost circle
        /// </summary>
        private double OuterSpacing = 50;

        /// <summary>
        /// Center point of plasmid
        /// </summary>
        private Point Center;

        private double Radius;

        private double Scale = 1.0;

        public CircularCanvas()
            : base()
        {
            layerManager.LayerWidth = UserSetting.INSTANT.Layer_Circular_DefaultLayerWidth;

            layerManager.Offset = 0; // layerManager.DefaultLayerWidth;
            MainStrand = new Layer(layerManager, Layer.LAYER_MAINSTRAND, false);
            Children.Add(MainStrand.Shape);
            ReverseStrand = new Layer(layerManager, Layer.LAYER_REVERSESTRAND, false);
            Children.Add(ReverseStrand.Shape);
            RulerLayer = new Layer(layerManager, Layer.LAYER_RULER, false, 10, 10);
            RulerLayer.Visibility = Visibility.Visible;
            Children.Add(RulerLayer.Shape); 
            RestrictSite = new RestrictionLayer(layerManager, layerManager.Width);
            Children.Add(RestrictSite.Shape);


            // Background = Brushes.AliceBlue;
        }

       

        public override void Init()
        {

            if (plasmid == null)
                return;



            // Each feature is a layer by our convention
            foreach (Feature ft in plasmid.Features)
            {
                Layer layer = layerManager[ft.Key];
                // for those don't have a layer
                if (layer == null)
                {
                    layer = new Layer(layerManager, ft.Key, false);
                    layer.Shape.MouseMove += new System.Windows.Input.MouseEventHandler(LayerShape_MouseMove);
                    layer.Shape.MouseDown += new MouseButtonEventHandler(LayerShape_MouseDown);
                    Children.Add(layer.Shape);
                }
               
            }
            



            // Remove redundent annotation
            // TODO:

            #region Add annotation
            int bp = plasmid.BaseCount;
            annotations = new FeatureUI[plasmid.FeatureCount];
            for (int i = 0; i < plasmid.FeatureCount; i++)
            {
                Layer layer = layerManager[plasmid.Features[i].Key];

                annotations[i] = new FeatureUI(this, layer, plasmid.Features[i]);

            }
            #endregion


            Redraw();


        }



        #region Layer
        void LayerShape_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Path)
            {
                Path path = sender as Path;
                Point loc = e.GetPosition(this);
                Cursor selectionCursor = IbeamCursor(loc);
                if (path.Cursor != selectionCursor)
                {
                    path.Cursor = selectionCursor;
                }

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    // Do a selection
                    PlasmidRecord.SelectTo(GetSelectionBase(loc));
                }
                e.Handled = true;
            }
        }

        void LayerShape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point loc = e.GetPosition(this);
            long bp = GetSelectionBase(loc);
            if (bp >= 0)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    // TODO: multiple selection feature
                    // Start a selection
                    PlasmidRecord.SelectStart(bp);
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


        #region utilities methods

        /// <summary>
        /// Get base pair number at the point
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        private long GetSelectionBase(Point loc)
        {
            double deg = GetDegree(loc);
            long bp = (long)Math.Round(plasmid.BaseCount * deg / (2 * Math.PI));
            //Console.WriteLine("bp: {0}, deg: {1}, loc: {2}", bp, deg * 180/Math.PI, loc);
            return bp;
        }

        /// <summary>
        /// Get degree for a point
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        private double GetDegree(Point loc)
        {
            double dx = loc.X - Center.X;
            double dy = loc.Y - Center.Y;
            double deg = Math.Atan(dy / dx) + Math.PI / 2;
            if (dx < 0)
                deg += Math.PI;
            if (deg < 0)
                deg += 2 * Math.PI;
            if (deg >= 2 * Math.PI)
                deg -= 2 * Math.PI;
            return deg;
        }

        /// <summary>
        /// Get cursor for layer for given mouse location
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Cursor IbeamCursor(Point point)
        {
            double deg = GetDegree(point);
            int idx = (int)Math.Round(deg / (Math.PI * 15 / 180));

            if (idx >= IbeamCursors.Length || idx < 0)
            {
                idx = 0;
            }

            //Console.WriteLine("deg: {0}, idx: {1}", deg, idx);
            return IbeamCursors[idx];
        }
        #endregion


        override public void Redraw()
        {
            /// Drawing process ///
            #region Initialize variables
            /// Base values are defined base on zooms, visible layers
            long bp = PlasmidRecord.BaseCount;
            Radius = Scale * 100;
            if (Radius < 20) Radius = 20;

            var vAnnots = annotations.Where(a => a.LabelVisibility);
            int numVisibles = vAnnots.Count();

            // calculate the default position for every label
            double textHeight = MediaExt.CharHeight(10.0);
            double[] textWidth = vAnnots.Select(a=>a.LabelWidth).ToArray();
            

            // define canvas size
            double RadiusOut = Radius + LayerManager.Width;
            MinWidth = 2 * (RadiusOut + OuterSpacing + Math.Min(textWidth.Max(), 500));
            MinHeight = 2 * (RadiusOut + OuterSpacing);
            Center = new Point(MinWidth / 2.0, MinHeight / 2.0);


            #endregion

            #region Draw layers
            // draw ruler layer
            double thicknessCircle = 1.0;
            double spaceTick = RulerLayer.Width / 2.0;
            double lengthTick = RulerLayer.Width / 2.0;
            double r2 = Radius + RulerLayer.Outter;
            double r1 = r2 - thicknessCircle;
            EllipseGeometry circle2 = new EllipseGeometry(Center, r2, r2);
            // EllipseGeometry circle1 = new EllipseGeometry(Center, r1, r1);
            // PathGeometry donut = Geometry.Combine(circle2, circle1, GeometryCombineMode.Exclude, null); ;
            // draw marking
            // TODO: intelligently set number of ticks base on size available
            double bpt = Math.Floor(1.0 * plasmid.BaseCount / NumTick);
            int bpTick = ((int)Math.Floor(bpt / 100) * 100);
            r2 = Radius + RulerLayer.Outter - spaceTick;
            r1 = Radius + RulerLayer.Outter - spaceTick - lengthTick;
            PathGeometry pg = new PathGeometry();
            for (int i = 0; i <= NumTick; ++i)
            {
                if (i * bpTick >= plasmid.BaseCount) break;
                double deg = (2.0 * Math.PI * i * bpTick) / plasmid.BaseCount - Math.PI / 2.0;
                double cos = Math.Cos(deg);
                double sin = Math.Sin(deg);
                LineGeometry line = new LineGeometry(new Point(Center.X + r2 * cos, Center.Y + r2 * sin),
                    new Point(Center.X + r1 * cos, Center.Y + r1 * sin));
                pg.AddGeometry(line);

                int n = i * bpTick;
                if (n == 0) n = 1;
                FormattedText label = new FormattedText(n.ToString(),
                        BaseLibrary.MediaExt.CultureInfoEnUS,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        10,
                        Brushes.Black);

                double txtSpacing = 0.5;
                double x = Center.X + r1 * cos;
                double y = Center.Y + r1 * sin;
                if (cos > 0) // left
                {
                    x -= label.Width + txtSpacing;
                }
                else
                {
                    x += txtSpacing;
                }
                if (sin > 0) // above
                {
                    y -= label.Height + txtSpacing;
                }
                else
                {
                    y += txtSpacing + txtSpacing;
                }

                Geometry glabel = label.BuildGeometry(new Point(x, y));
                pg.AddGeometry(glabel);
            }
            // pg.AddGeometry(circle2);
            pg.Freeze();
            RulerLayer.Shape.Data = pg;
            RulerLayer.Shape.Fill = Brushes.Black;
            RulerLayer.Shape.Stroke = null;
            // RulerLayer.Shape.StrokeThickness = 2.0;

   

            foreach (Layer layer in layerManager)
            {
                if (layer.Visibility != Visibility.Visible) continue;
                if (layer.Name == Layer.LAYER_RESTRICT)
                {
                    // Render restriction layer 
                    r1 = Radius + layer.Inner + layer.Width / 2.0;
                    EllipseGeometry circle1 = new EllipseGeometry(Center, r1, r1);
                    layer.Shape.Data = circle1;
                    layer.Shape.StrokeThickness = 1;
                    layer.Shape.Stroke = Brushes.Black;
                    layer.Shape.Fill = Brushes.Transparent;
                }
                else
                {
                    // In circular canvas, layers are represent by concentric donut
                    r1 = Radius + layer.Inner;
                    r2 = Radius + layer.Outter;
                    EllipseGeometry circle1 = new EllipseGeometry(Center, r1, r1);
                    circle2 = new EllipseGeometry(Center, r2, r2);
                    layer.Shape.Data = Geometry.Combine(circle2, circle1, GeometryCombineMode.Exclude, null);
                    layer.Shape.Fill = Brushes.Transparent;
                }
            }
            #endregion

            #region draw annotation
            double cx = Center.X; // center x
            double cy = Center.Y; // center y
            

            // circularAnnotations(RadiusLayerOuter, RadiusLayerOuter + 40, 10, 10, 10, alpha);


            // some hard coded constants
            double adjustTextAtBottomTopOfCircularMaps = 0.5 * Radius;

            int j = 0;
            foreach (FeatureUI annotation in annotations.Where(a => a.Visible)) 
            {
                Layer layer = layerManager[annotation.Key];

                /// Main Annotation Shape Drawing ///
                
                
                r1 = layer.Inner + Radius;
                r2 = layer.Outter + Radius;
                //Window1.log(this, Layer.Name +  ": Length: " + Layer.PlasmidLength + ", width: " + Layer.Width + ", Inner: " + Layer.Inner + ", Outer: " + Layer.Outter);

                PathGeometry pathGeometry = new PathGeometry();

                if (annotation.Key == Feature.KEY_restriction_site)
                {
                    // restriction site are represented by two triangles
                    double theta = Math.PI / 2 - (annotation.Start * Math.PI * 2.0 / bp);
                    double hfAng = 30*Math.PI/180;
                    double r3 = (r2 + r1) / 2.0;
                    double len = (layer.Width / 2.0 * 0.8) / Math.Cos(hfAng);

                    Point point1 = new Point(cx + r3 * Math.Cos(theta), cy - r3 * Math.Sin(theta));
                    Point point2 = new Point(point1.X + len * Math.Cos(theta - hfAng),
                        point1.Y - len * Math.Sin(theta - hfAng));
                    Point point3 = new Point(point1.X + len * Math.Cos(theta + hfAng),
                        point1.Y - len * Math.Sin(theta + hfAng));

                    PathSegmentCollection segments = new PathSegmentCollection();
                    segments.Add(new LineSegment(point2, true));
                    segments.Add(new LineSegment(point3, true));

                    PathFigure triangle1 = new PathFigure(point1, segments, true);
                    triangle1.Freeze();

                    double thetaold = theta;
                    theta = Math.PI / 2 - (annotation.End * Math.PI * 2.0 / bp);
                    point1 = new Point(cx + r3 * Math.Cos(theta), cy - r3 * Math.Sin(theta));
                    point2 = new Point(point1.X + len * Math.Cos(theta - hfAng + Math.PI),
                        point1.Y - len * Math.Sin(theta - hfAng + Math.PI));
                    point3 = new Point(point1.X + len * Math.Cos(theta + hfAng + Math.PI),
                        point1.Y - len * Math.Sin(theta + hfAng + Math.PI));

                    segments = new PathSegmentCollection();
                    segments.Add(new LineSegment(point2, true));
                    segments.Add(new LineSegment(point3, true));

                    PathFigure triangle2 = new PathFigure(point1, segments, true);
                    triangle2.Freeze();

                    pathGeometry.Figures.Add(triangle1);
                    pathGeometry.Figures.Add(triangle2);
                    // pathGeometry.AddGeometry(new EllipseGeometry(point1, 10, 10));
                }
                else
                {
                    double theta = Math.PI / 2 - (annotation.Start * Math.PI * 2.0 / bp);
                    double tSpan = (annotation.End - annotation.Start) * Math.PI * 2.0 / bp;


                    Point point1 = new Point(cx + r1 * Math.Cos(theta), cy - r1 * Math.Sin(theta));
                    Point point2 = new Point(cx + r1 * Math.Cos(theta - tSpan), cy - r1 * Math.Sin(theta - tSpan));
                    Point point3 = new Point(cx + r2 * Math.Cos(theta - tSpan), cy - r2 * Math.Sin(theta - tSpan));
                    Point point4 = new Point(cx + r2 * Math.Cos(theta), cy - r2 * Math.Sin(theta));


                    PathSegmentCollection segments = new PathSegmentCollection();

                    // segments.Add(new LineSegment(point2, isStroke));
                    segments.Add(BaseLibrary.MediaExt.ArcSegment(Center, point1, tSpan, true));
                    segments.Add(new LineSegment(point3, true));

                    segments.Add(BaseLibrary.MediaExt.ArcSegment(Center, point3, -tSpan, true));
                    // segments.Add(new LineSegment(point1, isStroke));

                    PathFigure annotationFigure = new PathFigure(point1, segments, true);
                    annotationFigure.Freeze();

                    pathGeometry.Figures.Add(annotationFigure);
                }

                pathGeometry.Freeze();
                annotation.Geometry = pathGeometry;

            }

            /// Place labels and link ///
            double[] alpha = vAnnots.Select(annotation => Math.PI / 2 - (annotation.Start * Math.PI * 2.0 / bp)).ToArray();
            // double[] span = vAnnots.Select(annotation => (annotation.End - annotation.Start) * Math.PI * 2.0 / bp).ToArray();
            double[] midAlpha = vAnnots.Select(annotation => Math.PI / 2 - 0.5 * (annotation.End + annotation.Start) * Math.PI * 2.0 / bp).ToArray();

            double r4 = RadiusOut + 10;
            double[] lx = midAlpha.Select(x => r4 * Math.Cos(x)).ToArray();
            double[] ly = midAlpha.Select(x => -r4 * Math.Sin(x)).ToArray();

            j = 0;
            foreach (FeatureUI annotation in vAnnots)
            {
                Layer layer = layerManager[annotation.Key];

                // place labels
                double x, y;
                if (ly[j] < 0) y = cy + ly[j] - annotation.LabelHeight;
                else y = cy + ly[j];
                if (lx[j] >= 0) x = cx + lx[j];
                else x = cx + lx[j] - textWidth[j];
                annotation.LabelPosition = new Point(x, y);
                j++;
            }

            foreach (FeatureUI a in annotations) a.Update();

            #endregion

            OnSelectionChanged(this, null);
        }

        public override void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            DrawingContext dc = selectionCanvas.selectionVisual.RenderOpen();
            if (plasmid.Selection == null)
            {
                dc.Close();
                return;
            }

            long bp = plasmid.BaseCount;
            foreach (Range range in plasmid.Selection)
            {

                double startDegree = Math.PI / 2 - (range.Start * Math.PI * 2.0 / bp);
                double endDegree = Math.PI / 2 - (range.End * Math.PI * 2.0 / bp);
                Point center = Center;
                double radius = Radius + layerManager.Width;
                double x = center.X; // center x
                double y = center.Y; // center y

                PathSegmentCollection segments = new PathSegmentCollection();
                bool isStroke = true;

                Point point2 = new Point(x + radius * Math.Cos(startDegree), y - radius * Math.Sin(startDegree));

                segments.Add(new LineSegment(point2, isStroke));
                segments.Add(BaseLibrary.MediaExt.ArcSegment(center, point2, -(endDegree - startDegree), isStroke));


                PathFigure annotationFigure = new PathFigure(
                    center,
                    segments, true);

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(annotationFigure);

                pathGeometry.Freeze();
                annotationFigure.Freeze();

                dc.DrawGeometry(PlasmidCanvas.SelectionBrush, null, pathGeometry);

                //Console.WriteLine("sDeg: {0}, eDeg: {1}, r: {2}, p2: {3}, p3: {4}, c: {5}", 
                //    startDegree, endDegree, radius, point2, point3, center);
            }
            dc.Close();
        }



        static CircularCanvas()
        {
            string bUri = "Resources/cursors/";
            IbeamCursors = new Cursor[24];

            for (int i = 0; i < 24; i++)
            {
                int deg = i * 15;
                if (deg >= 180)
                {
                    deg -= 180;
                }
                try
                {
                    StreamResourceInfo sri = Application.GetRemoteStream(
                    new Uri(bUri + "ibeam" + deg + ".cur", UriKind.Relative));
                    IbeamCursors[i] = new Cursor(sri.Stream);
                }
                catch
                {
                    IbeamCursors[i] = Cursors.IBeam;
                }
            }
        }

        /// <summary>
        /// Cursor for layers varies 15 degrees
        /// </summary>
        public static readonly Cursor[] IbeamCursors;

        /// <summary>
        /// Set desire zoom
        /// </summary>
        /// <param name="zoomValue"></param>
        /// <returns></returns>
        public override string Zoom(string zoomValue)
        {
            Match m = _regexNumber.Match(zoomValue);
            if (m.Success)
            {

                int value = Int32.Parse(m.Value);
                if (zoomValue.EndsWith("Delta"))
                {
                    Scale = (1 + value) * Scale;
                }
                else
                {
                    Scale = value / 100.0;
                }


                Redraw();

                return ((int)(Scale * 100)) + "%";
            }
            return zoomValue;
        }

        #region Helper functions


        /// <summary>
        /// 
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="vertext">vertical extent of the labels</param>
        /// <param name="horline">length of the horizontal part of the line to the annotation</param>
        /// <param name="margin">extend horizontal line to margins ? (not implemented yet)</param>
        /// <param name="alpha">origin angles for every label</param>
        static void circularAnnotations(double r1, double r2, double vertext, 
           double horline, double margin, double[] alpha)
        {
            double adjustTextAtBottomTopOfCircularMaps = 0.5 * r1;

            // calculate the default position for every label
            List<double> lx = new List<double>((double[])alpha.Select(x => -r2 * Math.Sin(x)).ToArray());
            List<double> ly = new List<double>((double[])alpha.Select(x => -r2 * Math.Cos(x)).ToArray());

            // adjust the vertical position of the right labels
            int[] h1 = (int[])lx.Select((x, i) => { if (x >= 0) return i; else return -1; }).Where(x => x != -1).ToArray();

            double[] lyh1 = (double[])ly.TakeWhile((x, i) => h1.Contains(i)).ToArray();
            int[] h2 = lyh1.SortIndex();
            double[] lyAdj = LinearCanvas.SpaceAdjust((double[])ly.TakeWhile((x, idx) => h2.Contains(idx)).ToArray(), vertext);
            for (int idx = 0; idx < h1.Length; idx++)
            {
                ly[h2[h1[idx]]] = lyAdj[idx];
            }

            // adjust the vertical position of the left labels
            h1 = (int[])lx.Select((x, i) => { if (x < 0) return i; else return -1; });
            h1 = (int[])h1.TakeWhile(x => x != -1);
            lyh1 = (double[])ly.TakeWhile((x, i) => h1.Contains(i));
            lyAdj = LinearCanvas.SpaceAdjust((double[])ly.TakeWhile((x, idx) => h2.Contains(idx)), vertext);
            for (int idx = 0; idx < h1.Length; idx++)
            {
                ly[h2[h1[idx]]] = lyAdj[idx];
            }

            // shifts the horizontal position to r2 at least
            for (int idx = 0; idx < lx.Count; idx++)
            {
                double tmp = Math.Sqrt(r2 * r2 - ly[idx] * ly[idx]);
                if (tmp * tmp < 0) tmp = Double.MinValue * 1000;
                lx[idx] = Math.Sign(lx[idx]) * tmp;
            }

            // tries to give some horizontal shifts to the labels at the bottom and top
            // to avoid the overlapping of lines
            bool[] h = (bool[])lx.Select((x, i) => lx[i] >= 0 && ly[i] < 0);
            bool[] h21 = (bool[])lx.Select((x, i) => x < adjustTextAtBottomTopOfCircularMaps && h[i]);
            if (h21.Any())
            {
                int[] h3 = (int[])(h2.Select((x, i) => { if (x > 0) return i; else return -1; })).Where(x => x != -1);                
                int[] h4 = h3.SortIndex();
                List<double> lxf = (List<double>)lx.TakeWhile((x, i) => h[i] && h21[i]);
                double p = r1;
                if (lxf.Count > 0)
                {
                    p = lxf.Min() / h21.Sum(b => b ? 1 : 0);
                }
                int[] lxp = (int[])Enumerable.Range(0, h2.Sum() - 1);
                double[] dlxp = (double[])lxp.Select(x => (double)x);
                dlxp = dlxp.Times(p);
                for (int idx = 0; idx < lxp.Length; idx++)
                {
                    lx[h3[h4[idx]]] = lxp[idx];
                }
            }

            h = (bool[])lx.Select((x, i) => lx[i] < 0 && ly[i] < 0);
            h21 = (bool[])ly.Select((x, i) => x > adjustTextAtBottomTopOfCircularMaps && h[i]);
            if (h21.Any())
            {
                int[] h3 = (int[])(h2.Select((x, i) => { if (x > 0) return i; else return -1; })).Where(x => x != 1);               
                int[] h4 = h3.SortIndex();
                List<double> lyf = (List<double>)ly.TakeWhile((x, i) => h[i] && h21[i]);
                double p = -r1;
                if (lyf.Count > 0)
                {
                    p = lyf.Max() / h21.Sum(b => b ? 1 : 0);
                }
                int[] lyp = (int[])Enumerable.Range(0, h2.Sum() - 1);
                double[] dlyp = (double[])lyp.Select(x => (double)x);
                dlyp = dlyp.Times(-Double.MinValue * 1000);
                for (int idx = 0; idx < dlyp.Length; idx++)
                {
                    ly[h3[h4[idx]]] = dlyp[idx];
                }
            }

            h = (bool[])lx.Select((x, i) => lx[i] >= 0 && ly[i] >= 0);
            h21 = (bool[])lx.Select((x, i) => x < adjustTextAtBottomTopOfCircularMaps && h[i]);
            if (h21.Any())
            {
                int[] h3 = (int[])(h2.Select((x, i) => { if (x > 0) return i; else return -1; })).Where(x => x != 1);
                int[] h4 = h3.SortIndex();
                List<double> lyf = (List<double>)ly.TakeWhile((x, i) => h[i] && h21[i]);
                double p = -r1;
                if (lyf.Count > 0)
                {
                    p = lyf.Min() / h21.Sum(b => b ? 1 : 0);
                }
                int[] lyp = (int[])Enumerable.Range(0, h2.Sum() - 1);
                double[] dlyp = (double[])lyp.Select(x => (double)x);
                dlyp = dlyp.Times(-Double.MinValue * 1000);
                for (int idx = 0; idx < dlyp.Length; idx++)
                {
                    ly[h3[h4[idx]]] = dlyp[idx];
                }
            }

            h = (bool[])lx.Select((x, i) => lx[i] < 0 && ly[i] >= 0);
            h21 = (bool[])lx.Select((x, i) => x > -adjustTextAtBottomTopOfCircularMaps && h[i]);
            if (h21.Any())
            {
                int[] h3 = (int[])(h2.Select((x, i) => { if (x > 0) return i; else return -1; })).Where(x => x != 1);
                int[] h4 = h3.SortIndex();
                List<double> lyf = (List<double>)lx.TakeWhile((x, i) => h[i] && h21[i]);
                double p = -r1;
                if (lyf.Count > 0)
                {
                    p = lyf.Min() / h21.Sum(b => b ? 1 : 0);
                }
                int[] lyp = (int[])Enumerable.Range(0, h2.Sum() - 1);
                double[] dlyp = (double[])lyp.Select(x => (double)x);
                dlyp = dlyp.Times(-Double.MinValue * 1000);
                for (int idx = 0; idx < dlyp.Length; idx++)
                {
                    ly[h3[h4[idx]]] = dlyp[idx];
                }
            }

            // a final check to avoid reversed angles of the annotation lines
            for (int i = 0; i < lx.Count; i++)
            {
                if (lx[i] >= 0) 
                {
                    lx[i] = Math.Max(lx[i], -r1 * Math.Sin(alpha[i]));
                }
                else {
                    lx[i] = Math.Min(lx[i], -r1 * Math.Sin(alpha[i]));
                }
            }

        }


        #endregion
    }
}
