using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using BaseLibrary;
using BioCSharp;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.Linq.Mapping;
using System.Data;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections;
using BioCSharp.Seqs;


namespace iMoBio.PlasmidCanvas
{
    /// <summary>
    /// Plasmid features are represented in layers depending on feature key.
    /// 
    /// This class is responsble for generating visual style of a layer.
    /// 
    /// There are some four default layers and all feature layers graphic settings
    /// are predefined. User preferences of the layer are also save in configuration
    /// file in <code>LayerManager</code> by the function <code>SaveSetting()</code>
    /// </summary>
    /// 
    [DefaultPropertyAttribute("Name")]
    public class Layer : IEnumerable
    {
        /// <summary>
        /// Start ZIndex for layer and increase one by one
        /// </summary>
        public static int ZINDEX_LAYER = 10;
        public static int ZINDEX_BACKBONE = 1;
        /// <summary>
        /// All annotaiton Zindex
        /// </summary>
        public static int ZINDEX_ANNOTATION = 100;
        public static int ZINDEX_LABEL = 150;
        public static int ZINDEX_SELECTION = 100000;

        public const string LAYER_RULER = "Ruler";
        public const string LAYER_MAINSTRAND = "5' strand";
        public const string LAYER_REVERSESTRAND = "3' strand";
        public const string LAYER_RESTRICT = Feature.KEY_restriction_site;
        public static readonly string[] DEFAULT_LAYERS = { LAYER_MAINSTRAND, LAYER_REVERSESTRAND, LAYER_RULER, LAYER_RESTRICT };

        private Visibility visibility = Visibility.Visible;
        private AnnotationShape annotationShape = AnnotationShape.Undefined;
        private string name;

        public static readonly Brush[] LayerBrushes = new Brush[] 
                    {Brushes.AliceBlue, Brushes.Aquamarine, Brushes.Black, Brushes.Blue, Brushes.CadetBlue, 
                    Brushes.Green, Brushes.Red, Brushes.Chocolate};

        protected Brush fill = null;
        protected Brush stoke = null;
        protected Pen pen = null;
        protected readonly Path shapeLayer = null;
        protected readonly LayerManager Parent;
        protected double width = Double.NaN;
        protected double ascent = Double.NaN;
        protected double descent = Double.NaN;
        protected double offset = Double.NaN;
        internal bool IsFeaturelayer { get; set; }


        public Layer(LayerManager parent, string name, bool isFeatureLayer) :
            this(parent, name, isFeatureLayer, Double.NaN, Double.NaN) { }
        

        public Layer(LayerManager parent, string name, bool isFeatureLayer,
            double offset, double width)
        {
            shapeLayer = new Path();
            this.Parent = parent;
            this.name = name;
            this.offset = offset;
            this.width = width;
            IsFeaturelayer = isFeatureLayer;
            DataRow row = LayerManager.FeatureSetting.Tables[LayerManager.FEATURESETTING_TABLE].Rows.Find(name);
            if (row != null)
            {
                // we have some default setting for this layer
                if (row.Field<bool>(LayerManager.FEATURESETTING_ISVISIBLE))
                {
                    visibility = Visibility.Visible;
                }
                else
                {
                    visibility = Visibility.Hidden;
                }
                // note for fill and stroke, we do lazy initialization
            }
            shapeLayer.SnapsToDevicePixels = true;
            shapeLayer.Fill = Brushes.Transparent;
            shapeLayer.Stroke = Brushes.Transparent;
            shapeLayer.StrokeThickness = 0.5;
            shapeLayer.Visibility = visibility;
            shapeLayer.Focusable = true;
            shapeLayer.MouseDown += new System.Windows.Input.MouseButtonEventHandler(shape_MouseDown);
            shapeLayer.LostFocus += new RoutedEventHandler(shapeLayer_LostFocus);

            parent.Add(this);
            Canvas.SetZIndex(shapeLayer, parent.GetZIndexNewLayer());
        }

        void shapeLayer_LostFocus(object sender, RoutedEventArgs e)
        {
            //Window1.PropertyEditor.SelectedObject = this;
            //Window1.PropertyEditor.Text = AppResource.PropertyEditor_Title;
        }

        void shape_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Window1.PropertyEditor.SelectedObject = this;
            //Window1.PropertyEditor.Text = "Layer - " + AppResource.PropertyEditor_Title;
            // e.Handled = true;
        }

        /// <summary>
        /// Draw outline shape of the layer.
        /// </summary>
        //virtual public void Draw()
        //{
        //}

        #region Getters and Setters


        [Description("Feature layer name")]
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Set visibile of layer and its content
        /// </summary>
        public bool Visible
        {
            get { return Visibility == Visibility.Visible; }
            set
            {
                if (value)
                    Visibility = Visibility.Visible;
                else
                    Visibility = Visibility.Hidden;

                foreach (FeatureUI anot in this)
                {
                    anot.Update();
                }

                Parent.Update();
            }
        }

        /// <summary>
        /// Center to center distance between logically adjacent layers
        /// </summary>
        [Description("Space between adjacent layer")]
        public double Offset
        {
            get
            {
                if (Double.IsNaN(offset))
                {
                    return Parent.Offset;
                }
                else
                {
                    return offset;
                }
            }
            set
            {
                offset = value;
                Parent.Update();
            }
        }

        [Description("Annotation shape")]
        internal AnnotationShape AnnotationShape
        {
            get
            {
                if (annotationShape == AnnotationShape.Undefined)
                    return Parent.AnnotationShape;
                else
                    return annotationShape;
            }
            set { annotationShape = value; }
        }


        /// <summary>
        /// Vasibility of layer representing its contents <code>Annotation</code> visibility.
        /// 
        /// If visibility is <code>Visibility.Collapsed</code> its logical location cannot be arranged from 
        /// layer manager editor.
        /// 
        /// For example default ruler layer in <code>SequenceCanvas</code> is collapsed but shown as
        /// non-scrollable heading at the top of the canvas.
        /// </summary>
        virtual internal Visibility Visibility
        {
            get { return Shape.Visibility; }
            set
            {
                //_visibility = value;
                Shape.Visibility = value;

            }
        }


        /// <summary>
        /// Content width of layer. Unless overrided by user, <code>Annotation</code> should not
        /// generally be drawn within this width.
        /// </summary>
        [Description("With")]
        public double Width
        {
            get
            {
                if (Double.IsNaN(width))
                    return Parent.GetWidth(this);
                else
                    return width;
            }
            set
            {
                width = value;
                Parent.Update();
            }
        }


        internal double Inner
        {
            get
            {
                double len = 0;
                foreach (Layer layer in Parent)
                {
                    if (layer == this)
                    {
                        break;
                    }
                    if (layer.Visibility == Visibility.Visible)
                    {
                        len += layer.Offset;
                    }
                }
                return len;
            }
        }

        internal double Outter
        {
            get
            {
                return Inner + Width;
            }
        }


        /// <summary>
        /// Extend of space before this layer
        /// </summary>
        [Description("Extend of space before this layer")]
        virtual public double Ascent
        {
            get
            {
                if (Double.IsNaN(ascent))
                    return Parent.LayerAscent;
                else
                    return ascent;
            }
            set
            {
                ascent = value;
                Parent.Update();
            }
        }

        /// <summary>
        /// Extend of space after this layer
        /// </summary>
        [Description("Extend of space after this layer")]
        virtual public double Descent
        {
            get
            {
                if (Double.IsNaN(descent))
                    return Parent.LayerDescent;
                else
                    return descent;
            }
            set
            {
                descent = value;
                Parent.Update();
            }
        }

        [Browsable(false)]
        internal Brush Fill
        {
            get
            {
                if (fill == null)
                {
                    return Parent.GetLayerFill(Name);
                }
                else
                    return fill;
            }
        }

        [Browsable(false)]
        internal Brush Stroke
        {
            get
            {
                if (stoke == null)
                    return Parent.Stoke;
                else
                    return stoke;
            }
        }

        [Browsable(false)]
        internal Pen Pen
        {
            get
            {
                if (pen == null)
                    return Parent.GetLayerPen(Name);
                else
                    return pen;
            }
        }
        #endregion

        /// <summary>
        /// Get shape of this layer.
        /// </summary>
        [Browsable(false)]
        protected internal Path Shape
        {
            get
            {
                return shapeLayer;
            }
        }



        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Parent.parent is PlasmidCanvas)
            {
                return ((PlasmidCanvas)Parent.parent).GetAnnotation(this);
            }
            else
            {
                return null;
            }
        }

        #endregion
    }


    /// <summary>
    /// Special layer for restriction sites. LayerManager must have at least one 
    /// <code>RestrictionLayer</code>
    /// </summary>
    public class RestrictionLayer : Layer
    {

        public RestrictionLayer(LayerManager parent, double width)
            : base(parent, Layer.LAYER_RESTRICT, false)
        {
            fill = Brushes.Blue;
            stoke = Brushes.Black;

            offset = width;
            this.width = width;
        }

        
    }

}

