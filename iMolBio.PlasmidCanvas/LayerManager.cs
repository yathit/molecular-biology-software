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
using BioCSharp.Seqs;
using System.Collections;

namespace iMoBio.PlasmidCanvas
{

    /// <summary>
    /// Manage layers and provide default properties to the layers. LayerManager export
    /// properties to manupulate by property grid. Default setting are saved in 
    /// persistent storage.
    /// 
    /// Default layers:
    /// <list type="number">
    /// <item><code>Layer.LAYER_RULER</code></item>
    /// <item><code>Layer.LAYER_MAINSTRAND</code></item>
    /// <item><code>Layer.LAYER_REVERSESTRAND</code></item>
    /// 
    /// Feature layers:
    /// Each type of feature correspond to one layer.
    /// </list>
    /// </summary>
    public class LayerManager : ICollection, ITypedList
    {
        #region Members
        protected internal double plasmidLength = 500;
        protected internal double _left = 100;
        protected internal double _right = 100;
        protected internal Point plasmidLocation = new Point(0, 0);
        protected internal ISeqCanvas parent;

        /// <summary>
        /// Default annotation shape
        /// </summary>
        protected AnnotationShape _annotationShape = AnnotationShape.Rectangle;

        protected internal double LayerWidth { get; set; }
        /// <summary>
        /// Default extend of space after a layer
        /// </summary>
        protected internal double LayerAscent { get; set; }
        /// <summary>
        /// Default extend of space before a alyer
        /// </summary>
        protected internal double LayerDescent { get; set; }

        /// <summary>
        /// Spacing for wrapping layers.
        /// </summary>
        protected double _wrappingSpacing = 0;
        /// <summary>
        /// Distance between adjacent layers
        /// </summary>
        protected double _offset = 0;

        /// <summary>
        /// Spacing before block of layers
        /// </summary>
        protected double ascent = 5.0;
        /// <summary>
        /// Spacing after block of layers
        /// </summary>
        protected double descent = 5.0;

        private static Dictionary<string, Brush> _defaultLayerFill;
        private static Dictionary<string, Pen> _defaultLayerPen;

        /// <summary>
        /// Default layer fill 
        /// </summary>
        private Brush _fill = Brushes.Transparent;

        /// <summary>
        /// Default layer stoke
        /// </summary>
        public Brush Stoke = null;

        public Pen Pen { set; get; }

        /// <summary>
        /// This list is updated by <code>Layer</code> constructor.
        /// </summary>
        protected List<Layer> layers;

        /// <summary>
        /// We start with 10 as defined in <code>ISeqCanvas</code> and add 1 for each layer.
        /// </summary>
        protected int layerZIndex = Layer.ZINDEX_LAYER; // we start at 10
        #endregion

        #region Feature layer default setting
        public static DataSet FeatureSetting;
        public const string FEATURESETTING_TABLE = "featuresetting";
        public const string FEATURESETTING_KEY = "id";
        /// <summary>
        /// Boolean value for default visibility
        /// </summary>
        public const string FEATURESETTING_ISVISIBLE = "isvisible";
        public const string FEATURESETTING_FILL = "fill";
        /// <summary>
        /// Column name for <code>FEATURESETTING_TABLE</code>. Popularity range from 0 to 10,
        /// being 10 is most popular.
        /// </summary>
        public const string FEATURESETTING_POPULARITY = "popularity";

        static LayerManager()
        {

            /// Layer setting ///
            bool ok = false;
            if (System.IO.File.Exists(UserSetting.INSTANT.FileName_Annotation))
            {
                try
                {
                    System.IO.FileStream fileStream = new System.IO.FileStream(
                        UserSetting.INSTANT.FileName_Annotation,
                        System.IO.FileMode.Open);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    FeatureSetting = (DataSet)deserializer.Deserialize(fileStream);
                    fileStream.Close();
                    ok = true;
                }
                catch
                {
                }
            }

            if (!ok)
            {
                // do some default
                FeatureSetting = new DataSet();
                DataTable tbl = new DataTable(FEATURESETTING_TABLE);
                DataColumn col = tbl.Columns.Add(FEATURESETTING_KEY, typeof(string));
                col.Unique = true;
                col.AllowDBNull = false;
                tbl.PrimaryKey = new DataColumn[] { col };
                tbl.Columns.Add(FEATURESETTING_ISVISIBLE, typeof(bool));
                tbl.Columns.Add(FEATURESETTING_POPULARITY, typeof(int));
                // it should be note that wpf color System.Windows.Media.Color is not serializable
                tbl.Columns.Add(FEATURESETTING_FILL, typeof(string));

                AddDbRow(tbl, Layer.LAYER_MAINSTRAND, 10, false, "Black");
                AddDbRow(tbl, Layer.LAYER_REVERSESTRAND, 10, false, "Black");
                AddDbRow(tbl, Layer.LAYER_RESTRICT, 8, true, "Black");
                AddDbRow(tbl, Layer.LAYER_RULER, 0, false, "Black");
                AddDbRow(tbl, Feature.KEY_source, 4, false, "Transparent");
                AddDbRow(tbl, Feature.KEY_CDS, 9, true, "BlueViolet");
                AddDbRow(tbl, Feature.KEY_gene, 8, false, "CadetBlue");
                AddDbRow(tbl, Feature.KEY_misc_feature, 8, false, "Coral");
                AddDbRow(tbl, Feature.KEY_misc_binding, 6, false, "Corel");
                AddDbRow(tbl, Feature.KEY_mRNA, 8, true, "Crimson");
                AddDbRow(tbl, Feature.KEY_Promoter, 8, true, "Cyan");
                AddDbRow(tbl, Feature.KEY_Rep_Origin, 8, true, "Gainsboro");
                AddDbRow(tbl, Feature.KEY_repeat_unit, 6, false, "Gray");
                AddDbRow(tbl, Feature.KEY_unknown, 6, true, "Indigo");
                FeatureSetting.Tables.Add(tbl);
            }

            // get ready
            DataTable ftbl = FeatureSetting.Tables[FEATURESETTING_TABLE];
            _defaultLayerFill = new Dictionary<string, Brush>(ftbl.Rows.Count);
            _defaultLayerPen = new Dictionary<string, Pen>(ftbl.Rows.Count);
            for (int i = 0; i < ftbl.Rows.Count; ++i)
            {
                try
                {
                    string color = ftbl.Rows[i].Field<string>(FEATURESETTING_FILL);
                    Brush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                    brush.Freeze();
                    _defaultLayerFill.Add(ftbl.Rows[i].Field<string>(FEATURESETTING_KEY), brush);
                    Pen pen = new Pen(brush, 1.0);
                    pen.Freeze();
                    _defaultLayerPen.Add(ftbl.Rows[i].Field<string>(FEATURESETTING_KEY), pen);
                }
                catch
                {
                }
            }
        }

        private static void AddDbRow(DataTable tbl, string key, int popularity, bool isVisible, string color)
        {
            DataRow row = tbl.NewRow();
            row[FEATURESETTING_KEY] = key;
            row[FEATURESETTING_POPULARITY] = popularity;
            row[FEATURESETTING_ISVISIBLE] = isVisible;
            row[FEATURESETTING_FILL] = color;
            tbl.Rows.Add(row);
        }

        /// <summary>
        /// Save setting. Main application window should call this function on closing.
        /// </summary>
        public static void SaveSetting()
        {
            IFormatter formatter = new BinaryFormatter();
            System.IO.Stream stream = new System.IO.FileStream(UserSetting.INSTANT.FileName_Annotation,
                System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);
            formatter.Serialize(stream, FeatureSetting);
            stream.Close();
        }
        #endregion

        public LayerManager(ISeqCanvas parent)
        {
            this.parent = parent;

            Pen = new Pen(Brushes.Black, 1.0);
            Pen.Freeze();

            layers = new List<Layer>(5);

        }


        /// <summary>
        /// Call when plasmid change or initialization.
        /// 
        /// Parent canvas should call this method after adding new Annotation since it mights
        /// belong to new feature type.
        /// </summary>
        virtual public void Update()
        {
            parent.Redraw();
        }

        /// <summary>
        /// Get default fill brush for a layer of respective feature key
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public Brush GetLayerFill(string layerName)
        {
            if (_defaultLayerFill.ContainsKey(layerName))
                return _defaultLayerFill[layerName];
            else
                return Fill;
        }

        /// <summary>
        /// Get default pen for a layer of respective feature key
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public Pen GetLayerPen(string layerName)
        {
            if (_defaultLayerPen.ContainsKey(layerName))
                return _defaultLayerPen[layerName];
            else
                return Pen;
        }

        #region Setters and Getters

        /// <summary>
        /// Center to center distance between logically adjacent layers
        /// </summary>
        [Description("Center to center distance between logically adjacent layers.")]
        public double Offset
        {
            get
            {
                if (Double.IsNaN(_offset))
                    return 0;
                else
                    return _offset;
            }
            set { _offset = value; }
        }

        /// <summary>
        /// Horizontal space between start of plasmid and canvas
        /// </summary>
        public double Left
        {
            get { return _left; }
            set { _left = value; }
        }

        /// <summary>
        /// Horizontal space between end of plasmid and canvas
        /// </summary>
        public double Right
        {
            get { return _right; }
            set { _right = value; }
        }

        /// <summary>
        /// geometrical length of plasmid.
        /// </summary>
        public double PlasmidLength
        {
            // TODO: this have to be moved to LayerManager
            get { return plasmidLength; }
            set { plasmidLength = value; }
        }

        /// <summary>
        /// Default annotation shape defined by layer manager.
        /// </summary>
        public AnnotationShape AnnotationShape
        {
            get { return _annotationShape; }
            set { _annotationShape = value; }
        }

        /// <summary>
        /// Spacing before block of layers
        /// </summary>
        public double Ascent { get { return ascent; } }
        /// <summary>
        /// Spacing after block of layers
        /// </summary>
        public double Descent { get { return descent; } }

        public Brush Fill
        {
            get { return _fill; }
        }


        /// <summary>
        /// Orderred list of layers
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        public IEnumerator GetEnumerator()
        {
            foreach (Layer layer in layers)
            {
                yield return layer;
            }
        }

        internal void Add(Layer layer)
        {
            if (layers.Select(x => x.Name).Contains(layer.Name))
            {
                throw new InvalidConstraintException("Layer name: " + layer.Name + " already exists.");
            }
            layers.Add(layer);
        }

        /// <summary>
        /// Get ZIndex of new layer.
        /// </summary>
        /// <returns></returns>
        public int GetZIndexNewLayer() { return layerZIndex++; }


       

        

        [DescriptionAttribute("Wrapping spacing"), CategoryAttribute("Appearance")]
        public double WrappingSpacing
        {
            get { return _wrappingSpacing; }
            set { _wrappingSpacing = value; }
        }

      

        /// <summary>
        /// Get layer by its name. The name of layer is the same as feature key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Layer as specified by name. <code>null</code> if not found.</returns>
        internal Layer this[string key]
        {
            get { return layers.SingleOrDefault(q => q.Name == key); }
        }

        /// <summary>
        /// Logical index position of layer
        /// </summary>
        /// <param name="LayerName"></param>
        /// <returns></returns>
        public int IndexOf(string LayerName)
        {
            Layer layer = layers.Single(q => q.Name == LayerName);

            return layers.IndexOf(layer);
        }



        virtual public double GetWidth(Layer layer)
        {
            return LayerWidth;
        }

        /// <summary>
        /// Total width of all visual layers including spacing between layers.
        /// <code>Ascent</code> and <code>Descent</code> of <code>LayerManager</code>
        /// is not accounted in the total width.
        /// </summary>
        virtual public double Width
        {
            get
            {
                Layer lastVisibleLayer = null;
                double w = 0.0;
                foreach (Layer layer in layers)
                {
                    if (layer.Visibility == Visibility.Visible)
                    {
                        w += layer.Offset;
                        lastVisibleLayer = layer;
                    }
                    //Console.Write("{0}({1}); ", layer.Name, layer.Visibility);
                }
                //Console.WriteLine();
                if (lastVisibleLayer != null)
                {
                    w += lastVisibleLayer.Width + lastVisibleLayer.Descent;
                }
                return w;
            }
        }


        #endregion



        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        int ICollection.Count
        {
            get { return layers.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return layers.GetEnumerator();
        }

        #endregion

        #region ITypedList Members

        PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            return TypeDescriptor.GetProperties(typeof(Layer));
        }

        string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
        {
            return this.GetType().Name;
        }

        #endregion
    }



}