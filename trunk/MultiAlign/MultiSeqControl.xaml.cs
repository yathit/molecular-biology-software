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
using BioCSharp.Misc;
using System.Collections.ObjectModel;
using BioCSharp.Seqs;

namespace iMoBio.Controls
{
    /// <summary>
    /// Interaction logic for MultiSeqControl.xaml
    /// </summary>
    public partial class MultiSeqControl : UserControl
    {
        private ObservableCollection<SeqRecord> records;


        public MultiSeqControl()
        {
            InitializeComponent();

            records = new ObservableCollection<SeqRecord>();
        }

        public void Load(SneakResult result)
        {
            records.Clear();
            if (result.Result == SneakAnswer.OK)
            {
                if (result.FileType == FileType.Fasta || result.FileType == FileType.MutliAlign)
                {
                    SeqRecord[] seqs = SeqRecord.parse(result);

                    foreach (SeqRecord r in seqs)
                    {
                        records.Add(r);
                    }
                    listView.ItemsSource = records;

                }
                else
                {
                    throw new NotSupportedException("Unknown file format.");
                }
            }

        }

        public IList<SeqRecord> SelectedRecords
        {
            get
            {
                return listView.SelectedItems as IList<SeqRecord>;
            }
        }

        public ObservableCollection<SeqRecord> Records
        {
            get { return records; }
        }
    }
}
