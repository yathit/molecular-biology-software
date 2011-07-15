using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using BioCSharp.Seqs;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using MathLib;
using BaseLibrary;

namespace iMoBio.PlasmidCanvas
{
    abstract public class LinearCanvas : PlasmidCanvas
    {

        /// <summary>
        /// Default spacing between annotation shape and label
        /// </summary>
        protected double spacingLabel = 20.0;

        /// <summary>
        /// Rectangle to recieve mouse event.
        /// 
        /// Note: selection canvas is not good for receiving mouse event because it is
        /// over the annotation. Other layers such as <code>Layer.LAYER_MAINSTRAND</code>
        /// is not reliable in the sense that they can assume their shape.
        /// </summary>
        protected Rectangle backRectangle;

        public LinearCanvas()
            : base()
        {
            layerManager.LayerWidth = UserSetting.INSTANT.Layer_Linear_DefaultLayerWidth;


            MainStrand = new Layer(layerManager, Layer.LAYER_MAINSTRAND, false);
            Children.Add(MainStrand.Shape);
            ReverseStrand = new Layer(layerManager, Layer.LAYER_REVERSESTRAND, false);
            Children.Add(ReverseStrand.Shape);
            RestrictSite = new RestrictionLayer(layerManager, layerManager.LayerWidth + 6.0);            
            Children.Add(RestrictSite.Shape);

            backRectangle = new Rectangle();
            Children.Add(backRectangle);
        }


        public override void Init()
        {
            if (plasmid == null)
                return;


            #region Add layers
            // Each feature is a layer by our convention
            foreach (Feature ft in plasmid.Features)
            {
                Layer layer = layerManager[ft.Key];
                // for those don't have a layer
                if (layer == null)
                {
                    layer = new Layer(layerManager, ft.Key, true);
                    Children.Add(layer.Shape);
                }

            }
            #endregion

            // Remove redundent annotation
            // TODO:

            #region Add annotation
            int bp = PlasmidRecord.BaseCount;
            Point plasmidLocation = Location;
            double plasmidLength = Length;
            annotations = new FeatureUI[plasmid.FeatureCount];
            for (int i = 0; i < plasmid.FeatureCount; i++)
            {
                Layer layer = layerManager[plasmid.Features[i].Key];
                FeatureUI annotation = new FeatureUI(this, layer, plasmid.Features[i]);
         
                annotations[i] = annotation;

            }
            #endregion

            // draw layers and annotations
            Redraw();

            // prepare to received selection
            backRectangle.Fill = Brushes.Transparent;
            backRectangle.Cursor = CircularCanvas.IbeamCursors[0];
            backRectangle.MouseMove += new MouseEventHandler(backRectangle_MouseMove);
            backRectangle.MouseDown += new MouseButtonEventHandler(backRectangle_MouseDown);

            plasmid.SelectionToEvent += OnSelectionChanged;
            plasmid.EnzymeChangedEvent += new RoutedEventHandler(Plasmid_EnzymeChangedEvent);


        }


        public void Plasmid_EnzymeChangedEvent(object sender, RoutedEventArgs e)
        {

        }




        void backRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pointer = e.GetPosition(backRectangle);
            long bp = (long)(plasmid.BaseCount * pointer.X / backRectangle.Width);
            plasmid.SelectStart(bp);
        }

        void backRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            Point pointer = e.GetPosition(backRectangle);
            long bp = (long)(plasmid.BaseCount * pointer.X / backRectangle.Width);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                plasmid.SelectTo(bp);
            }
            e.Handled = true;
        }



        /// <summary>
        /// Top left location of plasmid
        /// </summary>
        protected Point Location
        {
            get
            {
                return new Point(Left, Top);
            }
        }

        /// <summary>
        /// Length of plasmid
        /// </summary>
        protected double Length
        {
            get
            {
                return MinWidth - Left * 2;
            }
        }

        public override void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (plasmid.Selection == null)
            {
                return;
            }

            DrawingContext dc = selectionCanvas.selectionVisual.RenderOpen();
            long bp = plasmid.BaseCount;
            foreach (Range range in plasmid.Selection)
            {
                long start = range.Start;
                long end = range.End;
                long diff = end - start;
                if (diff < 0)
                {
                    start = range.End;
                    end = range.Start;
                    diff = -diff;
                }

                dc.DrawRectangle(PlasmidCanvas.SelectionBrush, null,
                    new Rect(Left + start * Length / bp, Top, diff * Length / bp, layerManager.Width));
            }
            dc.Close();
        }

        public override string Zoom(string zoomValue)
        {
            Match m = _regexNumber.Match(zoomValue);
            if (m.Success)
            {

                double zoom = MinWidth / MinWidthValue; // current zoom
                int value = Int32.Parse(m.Value);
                if (zoomValue.EndsWith("Delta"))
                {
                    zoom = (MinWidth + value) / MinWidthValue;
                }
                else
                {
                    zoom = value / 100.0;
                }


                double minZoom = (2 * (Left + 10)) / MinWidthValue;
                if (zoom < minZoom)
                    zoom = minZoom;

                // Console.WriteLine("zoom: {0}, value: {1}, h: {2}", zoom, value, MinHeight);

                MinWidth = MinWidthValue * zoom;

                Redraw();

                return ((int)(zoom * 100)) + "%";
            }
            return zoomValue;
        }



    }

}
