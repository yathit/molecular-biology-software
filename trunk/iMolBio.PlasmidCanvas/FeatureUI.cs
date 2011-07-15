using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioCSharp;
using System.Windows.Media;
using iMoBio;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.ComponentModel;
using BioCSharp.Seqs;
using BaseLibrary;
using BioCSharp.Bio;
using iMoBio.PlasmidCanvas.Layers;

namespace iMoBio.PlasmidCanvas
{
    public enum AnnotationShape { Rectangle, RoundedRectangle, Arrow, Triangle, Undefined }

  

    /// <summary>
    /// Class representation UI for <code>Feature</code> to provide interactivity
    /// 
    /// This is basic elements in <code>ISeqCanvas</code>.
    /// </summary>
    public class FeatureUI
    {
        private bool? visible = null;
        private bool? labelVisible = null;

        private static readonly Regex regDigits = new Regex("(?<start>\\d+)..(?<end>\\d+)");

        protected internal AnnotationShape _annotationShape = AnnotationShape.Undefined;

        protected Path shape;
        protected Path link;
        protected Label label;


        Brush brush = null;
        Brush stroke = null;
        double strokeThickNess = Double.NaN;
        //Pen _pen = null;


        protected internal readonly LayerUI Layer;
        protected internal readonly ISeqCanvas parent;
        protected internal readonly Feature feature;

        private int _start = -1;
        private int _end = -1;

        public FeatureUI(ISeqCanvas parent, iMoBio.PlasmidCanvas.Layers.LayerUI layer, Feature feature)
        {
            this.feature = feature;
            this.parent = parent;
            this.Layer = layer;

            shape = new Path();          
            stroke = null;
            shape.ToolTip = AnnotationFeature.ToolTip;
            shape.Focusable = true;
            Canvas.SetZIndex(shape, Layers.Layer.ZINDEX_ANNOTATION);
            shape.LostFocus += new RoutedEventHandler(shapeDrawing_LostFocus);
            shape.MouseDown += new System.Windows.Input.MouseButtonEventHandler(OnMouseDown);
            parent.AddChild(shape);

            link = new Path();
            label = new Label();
            label.Focusable = true;
            label.Content = Label;
            
            label.BorderBrush = null;
            label.Background = null;
            label.LostFocus += new RoutedEventHandler(shapeDrawing_LostFocus);
            label.MouseDown += new System.Windows.Input.MouseButtonEventHandler(OnMouseDown);
            parent.AddChild(label);
            label.Measure(new Size(400, 50));


            Match m = regDigits.Match(feature.Location);
            if (m.Success)
            {
                _start = Int32.Parse(m.Groups["start"].Value);
                _end = Int32.Parse(m.Groups["end"].Value);
            }
        }


        void shapeDrawing_LostFocus(object sender, RoutedEventArgs e)
        {
            if (AppBase.PropertyGrid.SelectedObject == this)
            {
                AppBase.PropertyGrid.SelectedObject = null;
            }
        }

       


        [Browsable(false)]
        public Brush Fill
        {
            get
            {
                return shape.Fill;
            }
        }


        public AnnotationShape AnnotationShape
        {
            get
            {
                if (_annotationShape == AnnotationShape.Undefined)
                    return AnnotationShape.Rectangle; // parent.LayerManager[this.Key].AnnotationShape;
                else
                    return _annotationShape;
            }
            set { _annotationShape = value; }
        }


        Feature AnnotationFeature
        {
            get { return feature; }
        }


        virtual public void OnMouseDown(object sender, RoutedEventArgs e)
        {
            if (AppBase.PropertyGrid.SelectedObject != this)
            {
                AppBase.PropertyGrid.Text = this.AnnotationFeature.Key;
                AppBase.PropertyGrid.SelectedObject = this;
                AppBase.LayerGrid.SelectedItem = this.Layer;
            }
        }

        /// <summary>
        /// Get or set location of label defining left and top relative to canvas
        /// </summary>
        public Point LabelPosition
        {
            get { return new Point(Canvas.GetLeft(label), Canvas.GetTop(label)); }
            set { Canvas.SetLeft(label, value.X); Canvas.SetTop(label, value.Y); }
        }

        /// <summary>
        /// Get or set visibility of label
        /// </summary>
        public bool LabelVisibility
        {
            get
            {
                if (labelVisible != null) return (bool)labelVisible;
                else return Visible;
            }
            set
            {
                labelVisible = value;
                label.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

      

        public double LabelWidth
        {
            get
            {
                return label.DesiredSize.Width;
                //return MediaExt.FindTextWidth(label.Text, label.FontSize, label.FontFamily,
                //    label.FontStyle, label.FontWeight, label.FontStretch);
            }
        }

        public double LabelHeight
        {
            get
            {
                return label.DesiredSize.Height;
                // return MediaExt.CharHeight(label.FontSize);
            }
        }

        #region Getters and Setters
        #region Internal properties

        [Browsable(false)]
        /// <summary>
        /// Start base pair number (for first segnment if more than one). -1 if invalid.
        /// </summary>
        protected internal int Start
        {
            get
            {
                return _start;
            }
        }
        /// <summary>
        /// End base pair number (for first segnment if more than one). -1 if invalid.
        /// </summary>
        protected internal int End
        {
            get
            {
                return _end;
            }
        }

        internal Geometry Geometry
        {
            set
            {
                shape.Data = value;
            }
        }

       
        #endregion

        #region Properties
        [CategoryAttribute("Appearance"), DescriptionAttribute("Show or hide the annotation.")]
        public bool Visible
        {
            get 
            {
                if (visible == null) return Layer.Visible;
                else return (bool)visible;
            }
            set
            {
                visible = value;
                Update();
            }
        }


        internal void Update()
        {
            if (visible == null)
            {
                shape.Visibility = Layer.Visibility;
                label.Visibility = Layer.Visibility;
                link.Visibility = Layer.Visibility;
            }
            else
            {
                shape.Visibility = (bool)visible ? Visibility.Visible : Visibility.Hidden;
                label.Visibility = (bool)visible ? Visibility.Visible : Visibility.Hidden;
                link.Visibility = (bool)visible ? Visibility.Visible : Visibility.Hidden;
            }
            if (labelVisible != null)
            {
                label.Visibility = (bool)labelVisible ? Visibility.Visible : Visibility.Hidden;
                link.Visibility = (bool)labelVisible ? Visibility.Visible : Visibility.Hidden;
            }

            if (brush == null)
            {
                if (feature.Key == Feature.KEY_restriction_site)
                {
                    Brush b = new SolidColorBrush(Enzyme.GetColor(feature.Label));
                    b.Freeze();
                    shape.Fill = b;
                }
                else
                {
                    shape.Fill = Layer.Fill;
                }
            }
            else
            {
                shape.Fill = brush;
            }

            if (stroke == null)
            {
                shape.Stroke = Layer.Stroke;
            }

            if (double.IsNaN(strokeThickNess))
            {
                if (feature.Key == Feature.KEY_restriction_site)
                {
                    shape.StrokeThickness = 1.0;
                }
                else
                {
                    shape.StrokeThickness = 2.0;
                }
            }


        }

        /// <summary>
        /// Feature key for the annotation representing name of a layer.
        /// </summary>
        [CategoryAttribute("Feature"), DescriptionAttribute("Feature key for the annotation representing name of a layer.")]        
        public string Key
        {
            get { return feature.Key; }
        }

        [CategoryAttribute("Feature"), DescriptionAttribute("The location indicates the region of the presented sequence which corresponds to a feature.")]
        public string Location
        {
            get { return feature.Location; }
        }

        [CategoryAttribute("Common qualifier"), DescriptionAttribute("Note.")]
        public string Note
        {
            get
            {
                if (feature.Qualifiers.ContainsKey(Feature.QUALIFIER_note))
                    return feature.Qualifiers[Feature.QUALIFIER_note];
                else
                    return "";
            }
        }

        [CategoryAttribute("Common qualifier"), DescriptionAttribute("A feature label.")]
        public string Label
        {
            get
            {
                string label = feature.Label;
                if (label.Length > UserSetting.INSTANT.Feature_Label_MaxLength)
                    label = label.Substring(0, UserSetting.INSTANT.Feature_Label_MaxLength);
                return label;
            }
        }

        [CategoryAttribute("Common qualifier"), DescriptionAttribute("A gene name.")]
        public string Gene
        {
            get
            {
                if (feature.Qualifiers.ContainsKey(Feature.KEY_gene))
                    return feature.Qualifiers[Feature.KEY_gene];
                else
                    return "";
            }
        }

        [CategoryAttribute("Qualifier"), DescriptionAttribute("Repeat unit.")]
        public string Repeat_unit
        {
            get
            {
                if (feature.Qualifiers.ContainsKey(Feature.KEY_repeat_unit))
                    return feature.Qualifiers[Feature.KEY_repeat_unit];
                else
                    return "";
            }
        }

        #endregion

        #endregion

        #region Properties

        [DescriptionAttribute("Fill color"), CategoryAttribute("Appearance")]
        public System.Drawing.Color FillColor
        {
            get
            {
                if (Fill.GetType() == typeof(SolidColorBrush))
                {
                    SolidColorBrush scb = (SolidColorBrush)Fill;
                    System.Drawing.Color c = System.Drawing.Color.FromArgb(scb.Color.R, scb.Color.G, scb.Color.B);
                    return c;
                }
                else
                {
                    // FIXME: we only support for solid color brush
                    return System.Drawing.Color.Transparent;
                }
            }
            set
            {
                Color color = Color.FromRgb(value.R, value.G, value.B);
                brush = new SolidColorBrush(color);
                brush.Freeze();
                shape.Fill = brush;
            }
        }
        #endregion
    }
}