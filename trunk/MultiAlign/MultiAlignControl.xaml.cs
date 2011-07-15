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
using iMoBio.Controls;
using BioCSharp.Algo;
using BioCSharp.Algo.Phylo;

namespace iMoBio.Controls
{
    

    /// <summary>
    /// Multiple alignment viewer
    /// </summary>
    public partial class MultiAlignControl : UserControl
    {
        private MultiAlign multiAlign;
        private ConcensusBar concensusBar;
        private double SeqLabelLength = Double.NaN;
        Ruler ruler = new Ruler();

        public MultiAlignControl()
        {
            InitializeComponent();

            seqPanel.Children.Add(ruler);
            seqScroller.ScrollChanged += seqScroller_ScrollChanged;
        }

        void seqScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
           
             concensusScroller.ScrollToHorizontalOffset(seqScroller.HorizontalOffset);
          
        }

        /// <summary>
        /// Load an alignment.
        /// 
        /// </summary>
        /// <param name="records"></param>
        public MultiAlign MultiAlignment
        {
            set
            {
                multiAlign = value;

                foreach (SeqRecord seq in multiAlign)
                {
                    if (seq.Name == null)
                    {

                    }
                    SeqLabelCanvas label = new SeqLabelCanvas((Seq)seq);
                    if (Double.IsNaN(SeqLabelLength))
                    {
                        SeqLabelLength = label.Width;
                    }
                    seqPanel.Children.Add(label);
                }

                ruler.Width = SeqLabelLength;
                ruler.BaseCount = multiAlign.Concensus.Length;
                ruler.Margin = new Thickness(0, 0, 0, 5.0);

                concensusBar = new ConcensusBar(this);
                concensusBar.Margin = new Thickness(0, 0, 0, 5);
                concensusScroller.Content = concensusBar;
            }
        }

        public void BuildTree()
        {
            multiAlign.BuildTree();
        }

        public Tree Tree { get { return multiAlign.Tree; } }

        public SeqRecord[] Input
        {
            get
            {
                return multiAlign.Input;
            }
        }

        private class ConcensusBar : Canvas
        {
            protected MultiAlignControl parent;
            private SeqLabelCanvas concensusLabel;
            private double gap = 5.0;
            

            public ConcensusBar(MultiAlignControl parent)
                : base()
            {
                this.parent = parent;
                char[] cChars = parent.multiAlign.Concensus.ToCharArray();
                // substitute with '-' for those alignment having half of them are '-'
                // FIXME: not sure this is a right way
                for (int i = 0; i < cChars.Length; ++i)
                {
                    int n = 0;
                    for (int j = 0; j < parent.multiAlign.Aligment.Length; ++j )
                        if (parent.multiAlign.Aligment[j][i] == '-')
                            ++n;

                    if (n > parent.multiAlign.Aligment.Length / 2.0)
                        cChars[i] = '-';
                }
                string concensus = new string(cChars);
                //Console.WriteLine(concensus);
                double[] scores = parent.multiAlign.ConservationScore;
                double maxScore = 0.0; // we should able to know this value
                for (int i = 0; i < scores.Length; ++i)
                {
                    if (maxScore < scores[i])
                        maxScore = scores[i];
                }
                
                Height = 50;
                Width = parent.SeqLabelLength;
                

                concensusLabel = new SeqLabelCanvas(concensus, ProteinColorScheme.None);
                Canvas.SetTop(concensusLabel, Height - concensusLabel.Height);
                Children.Add(concensusLabel);

                double h = Height - 2 * gap - concensusLabel.Height;
                double w = concensusLabel.Width / scores.Length;
                for (int i = 0; i < scores.Length; ++i)
                {
                    if (concensus[i] == '-')
                        continue;

                    Rectangle r = new Rectangle();
                    r.Height = h * (maxScore - scores[i]) / maxScore;
                    r.Width = w;
                    r.Fill = Brushes.DarkSeaGreen;
                    // r.ToolTip = String.Format("consensus score: {0}", scores[i]);
                    Canvas.SetTop(r, gap + (h - r.Height));
                    Canvas.SetLeft(r, i * w);
                    Children.Add(r);
                }
            }
        }

    }


    
}
