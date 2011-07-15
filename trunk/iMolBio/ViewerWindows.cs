using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Divelements.SandDock;
using iMoBio.PlasmidCanvas;
using BioCSharp.Seqs;
using System.Windows.Input;
using BioCSharp.Misc;
using System.Windows.Controls;
using iMoBio.Controls;
using BioCSharp.Algo;
using BioCSharp.Algo.Phylo;
using System.IO;

namespace iMoBio.Controls
{
    
    public class MultiSeqWindow : Divelements.SandDock.DocumentWindow
    {
        private MultiSeqControl seqPanel = new MultiSeqControl();

        public MultiSeqWindow()
            : base()
        {
            Child = seqPanel;
        }

        public void Load(SneakResult result)
        {
            seqPanel.Load(result);

        }

        public IList<SeqRecord> Records
        {
            get
            {
                return seqPanel.Records;
            }
        }

        public IList<SeqRecord> SelectedRecords
        {
            get
            {
                return seqPanel.SelectedRecords;
            }
        }

    }

    public class TracePlotWindow : Divelements.SandDock.DocumentWindow
    {
        TracePlot plot;

        public TracePlotWindow()
            : base()
        {
            plot = new TracePlot();
            ScrollViewer sc = new ScrollViewer();
            sc.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            sc.Content = plot;
            this.Child = sc;
        }

        public void Open(DockSite dockSite, WindowOpenMethod method, string fileName)
        {
            plot.Load(fileName);
            this.Title = Path.GetFileName(fileName);
            this.DockSite = dockSite;
            base.Open(method);
        }
    }

    public class MultiAlignWindow : Divelements.SandDock.DocumentWindow
    {
        private MultiAlignControl multiAlignControl;

        public MultiAlignWindow()
            : base()
        {
            multiAlignControl = new MultiAlignControl();
            
            this.Child = multiAlignControl;
        }

        public void Load(MultiAlign ma)
        {
            multiAlignControl.MultiAlignment = ma;
        }

        public void Load(SneakResult result)
        {
            SeqRecord[] seqs = SeqRecord.parse(result);

            multiAlignControl.MultiAlignment = new MultiAlign(seqs);
        }

        public SeqRecord[] Input
        {
            get
            {
                return multiAlignControl.Input;
            }
        }

        public void DoAlignment()
        {
            SeqRecord[] seqs = multiAlignControl.Input;

            multiAlignControl = new MultiAlignControl();
            MultiAlign ma = new MultiAlign(seqs);
            ma.DoMuscle();
            multiAlignControl.MultiAlignment = ma;
            
            this.Child = multiAlignControl;
        }

        public void BuildTree()
        {
            multiAlignControl.BuildTree();
        }

        public Tree Tree { get { return multiAlignControl.Tree; } }



        public void Open(DockSite dockSite, WindowOpenMethod method, string title)
        {
            this.Title = title;
            this.DockSite = dockSite;
            base.Open(method);
        }
    }

    public class PhyTreeWindow : Divelements.SandDock.DocumentWindow
    {
        private PhyTreeControl phyTreeControl;
        private ScrollViewer vBox = new ScrollViewer();

        public PhyTreeWindow()
            : base()
        {
            vBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            vBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Child = vBox;
        }

        public void Load(Tree tree)
        {
            phyTreeControl = new PhyTreeControl(tree);
            
            vBox.Content = phyTreeControl;
        }

        public void Open(DockSite dockSite, WindowOpenMethod method, string title)
        {
            this.Title = title;
            this.DockSite = dockSite;
            base.Open(method);
        }
    }
}
