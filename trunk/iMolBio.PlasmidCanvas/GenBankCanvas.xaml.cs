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

namespace iMoBio.PlasmidCanvas
{
    /// <summary>
    /// Interaction logic for GenBankCanvas.xaml
    /// </summary>
    public partial class GenBankCanvas : UserControl, ISeqCanvas
    {
        static public string CanvasType = "GenBank";

        protected internal PlasmidRecord plasmid = null;

        public GenBankCanvas()
        {
            InitializeComponent();
        }

        public void Redraw() { }

        #region ISeqCanvas Members

        public PlasmidRecord PlasmidRecord
        {
            get
            {
                return plasmid;
            }
            set
            {
                plasmid = value;
                Init();
            }
        }

        public LayerManager LayerManager
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Init()
        {
            if (plasmid == null)
                return;

            txtName.Text = plasmid.Name;
            lblBaseCount.Content = plasmid.BaseCount;
            string s = "";
            plasmid.Keys.TryGetValue(InsdcRecord.INSDSeq_topology, out s);
            txtTopology.Text = s;
            s = "";
            plasmid.Keys.TryGetValue(InsdcRecord.INSDSeq_update_date, out s);
            txtUpdate_date.Text = s;
            s = "";
            plasmid.Keys.TryGetValue(InsdcRecord.INSDSeq_definition, out s);
            txtDefinition.Text = s;
            s = "";
            plasmid.Keys.TryGetValue(InsdcRecord.INSDSeq_division, out s);
            txtDivision.Text = s;
            s = "";
            plasmid.Keys.TryGetValue(InsdcRecord.KEY_ORGANISM, out s);
            txtOrganism.Text = s;
        }

        public void AddChild(UIElement child) { throw new InvalidOperationException("Sequence Canvas draw itself"); }


        public string Zoom(string zoomValue)
        {
            return zoomValue;
        }

        #endregion
    }
}
