using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioCSharp.Seqs;
using System.Text.RegularExpressions;
using MathLib;

namespace iMoBio.PlasmidCanvas
{
    /// <summary>
    /// Canvas to display plasmid record in circular or linear from
    /// 
    /// ISeqCanvas that extend from this <code>PlasmidCanvas</code> are
    /// able to do addition functionily in the UI that include:
    /// <list>Restriction enzyme digestion</list>
    /// <list>Primer design</list>
    /// </summary>
    public abstract partial class PlasmidCanvas : Canvas, ISeqCanvas
    {
        /// <summary>
        /// Layer representing 5' strand
        /// </summary>
        protected Layer MainStrand { get; set; }

        /// <summary>
        /// Layer representing 3' strand
        /// </summary>
        protected Layer ReverseStrand { get; set; }

        /// <summary>
        /// Restriction cutting site layer
        /// </summary>
        protected Layer RestrictSite { get; set; }

        /// <summary>
        /// Ruler layer
        /// </summary>
        protected Layer RulerLayer { get; set; }

        protected FeatureUI[] annotations;


        protected LayerManager layerManager;

        protected double _top = 0;

        protected double _left = 0;

        protected PlasmidRecord plasmid = null;

        protected SelectionCanvas selectionCanvas;

        public static readonly Brush SelectionBrush;


        /// <summary>
        /// Number of ticks on ruler for marking bp
        /// </summary>
        protected int NumTick = 4;

        /// <summary>
        /// Zoom scale depend on this value
        /// </summary>
        protected double MinHeightValue = 600;
        /// <summary>
        /// Zoom scale depend on this value
        /// </summary>
        protected double MinWidthValue = 600;

        protected static Regex _regexNumber = new Regex(@"(-?\d+)");


        static PlasmidCanvas()
        {
            SelectionBrush = Brushes.LightSkyBlue.Clone();
            SelectionBrush.Opacity = 0.50;
            SelectionBrush.Freeze();
        }

        public PlasmidCanvas()
        {

            layerManager = new LayerManager(this);
            MinHeight = MinHeightValue;
            MinWidth = MinWidthValue;
            selectionCanvas = new SelectionCanvas(this);
            Children.Add(selectionCanvas);
            Canvas.SetZIndex(selectionCanvas, Layer.ZINDEX_SELECTION);
        }

        public IEnumerator<FeatureUI> GetAnnotation(Layer layer)
        {
            foreach (FeatureUI ft in annotations)
            {
                if (ft.Key == layer.Name)
                {
                    yield return ft;
                }
            }
        }

        public double Top
        {
            get { return _top; }
            set { _top = value; }
        }

        public double Left
        {
            get { return _left; }
            set { _left = value; }
        }

        #region ISeqCanvas Members

        public abstract string Zoom(string zoomValue);

        /// <summary>
        /// Get the plasmid record or set for the first time. Changing this will throw error.
        /// </summary>
        public PlasmidRecord PlasmidRecord
        {
            get
            {
                return plasmid;
            }
            set
            {
                // One reason that we cannot change is DigestionLayer attached property to the plasmid.
                if (plasmid != null) throw new InvalidOperationException("Plasmid cannot be changed on this canvas.");
                plasmid = value;
                if (plasmid != null)
                {
                    plasmid.SelectionToEvent += new RoutedEventHandler(OnSelectionChanged);
                }
                Init();
            }
        }

        public void AddChild(UIElement child) { Children.Add(child); }

        public abstract void OnSelectionChanged(object sender, RoutedEventArgs e);

        public LayerManager LayerManager
        {
            get
            {
                return layerManager;
            }
        }

        /// <summary>
        /// Implement <see cref="ISeqCanvas.Update()"/>
        /// </summary>
        abstract public void Init();

        /// <summary>
        /// Redraw the graphic component
        /// </summary>
        abstract public void Redraw();

        protected internal class SelectionCanvas : Canvas
        {
            private readonly List<Visual> visuals = new List<Visual>();
            protected internal DrawingVisual selectionVisual = new DrawingVisual();
            private readonly PlasmidCanvas plasmidCanvas;

            protected internal SelectionCanvas(PlasmidCanvas parent)
                : base()
            {
                plasmidCanvas = parent;


                AddVisual(selectionVisual);
            }

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

        #endregion



        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                Zoom(e.Delta + "Delta");
                e.Handled = true;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="u">goal (should be ordered)</param>
        /// <param name="v">minimum separation between output points</param>
        /// <returns></returns>
        public static double[] SpaceAdjust(double[] u, double v)
        {
            double eps1000 = 1000 * General.Eps;
            int n = u.Length;

            double[] w = new double[n];
            u.CopyTo(w, 0);
            for (int i = 1; i < n; i++)
            {
                w[i] = Math.Max(w[i - 1] + v, u[i]);
            }

            bool moving = true;
            while (moving)
            {
                moving = false;

                // move block by block
                int i = 0;
                while (i < n)
                {
                    // find next block
                    double[] diffs = w.Skip(i).ToArray();
                    diffs = General.Diff(diffs).Plus(-v);
                    int[] diff = diffs.Select(x => { if (x > eps1000) return 1; else return 0; }).ToArray();
                    int b = n - i - 1; // gets last block             
                    for (int iDiff = 0; iDiff < diff.Length; iDiff++)
                    {
                        if (diff[iDiff] != 0) { b = iDiff; break; }
                    }

                    // estimate shift
                    double[] bu = u.Skip(i).Take(b + 1).ToArray();
                    double[] bw = w.Skip(i).Take(b + 1).ToArray();
                    double sh = bu.Minus(bw).Sum() / bu.Length;


                    double leftLim = Double.NegativeInfinity;
                    double rightLim = Double.PositiveInfinity;
                    if (Math.Abs(sh) > eps1000)
                    {
                        if (i != 0) leftLim = w[i - 1] + v;
                        if (i + b + 1 < n) rightLim = w[i + b + 1] - v;
                        if (w[i] + sh < leftLim) sh = leftLim - w[i];
                        if (w[i + b] + sh > rightLim) sh = rightLim - w[i + b];
                        for (int wi = i; wi <= i + b; wi++) w[wi] += sh;
                        moving = true;
                    }

                    i = i + b + 1; // next block
                }

                // move singles
                while (true)
                {
                    double[] k = General.Diff(w).Plus(-v).ToArray();
                    bool[] k0 = k.Select(x => x > eps1000).ToArray();

                    bool[] k0_f1 = new bool[k0.Length + 1];
                    bool[] k0_b1 = new bool[k0.Length + 1];
                    k0_f1[0] = true;
                    k0_b1[k0.Length] = true;
                    for (int j = 0; j < k0.Length; j++)
                    {
                        k0_f1[j + 1] = k0[j];
                        k0_b1[j] = k0[j];
                    }
                    // i = find(([1 k0] & w>u) | ([k0 1] & w<u))
                    int[] idxs = k0_f1.Select((x, j) =>
                        {
                            if ((k0_f1[j] & w[j] > u[j]) | (k0_b1[j] & w[j] < u[j])) return j;
                            else return -1;
                        }).TakeWhile(x => x != -1).ToArray();
                    if (idxs.Length == 0) break;
                    foreach (int idx in idxs)
                    {
                        double leftLim = Double.NegativeInfinity;
                        double rightLim = Double.PositiveInfinity;
                        if (idx == 0) leftLim = w[idx - 1] + v;
                        if (idx == n) rightLim = w[i + 1] - v;
                        w[i] = Math.Max(Math.Min(u[idx], rightLim), leftLim);
                        moving = true;
                    }
                }
            }

            return w;
        }
    }
}
