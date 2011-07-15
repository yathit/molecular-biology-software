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
using System.Windows.Shapes;
using System.IO;
using BioCSharp.Misc;
using Divelements.SandDock;
using BaseLibrary;
using System.ComponentModel;
using System.Collections;
using BioCSharp.Seqs;
using BioCSharp.Algo;
using iMoBio.Controls;
using BioCSharp.Algo.Phylo;
using Divelements.SandRibbon;
using Xceed.Wpf;


namespace iMoBio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow INSTANT;
        System.Windows.Forms.SaveFileDialog saveFileDialog = null;
        System.Windows.Forms.OpenFileDialog openFileDialog = null;

        public MainWindow()
        {
            InitializeComponent();
            INSTANT = this;
            AppBase.PropertyGrid = propertyGrid;
            AppBase.LayerGrid = dgdLayer;
           


            
            #region command binding
            // command binding
            CommandBinding cmdOpen = new CommandBinding(ApplicationCommands.Open);
            cmdOpen.CanExecute += new CanExecuteRoutedEventHandler(cmdOpen_CanExecute);
            cmdOpen.Executed += new ExecutedRoutedEventHandler(cmdOpen_Executed);
            this.CommandBindings.Add(cmdOpen);

            CommandBinding cmdNew = new CommandBinding(ApplicationCommands.New);
            cmdNew.CanExecute += new CanExecuteRoutedEventHandler(cmdNew_CanExecute);
            cmdNew.Executed += new ExecutedRoutedEventHandler(cmdNew_Executed);
            this.CommandBindings.Add(cmdNew);

            CommandBinding cmdSave = new CommandBinding(ApplicationCommands.Save);
            cmdSave.Executed += new ExecutedRoutedEventHandler(cmdSave_Executed);
            cmdSave.CanExecute += new CanExecuteRoutedEventHandler(cmdSave_CanExecute);
            this.CommandBindings.Add(cmdSave);

            CommandBinding cmdCut = new CommandBinding(ApplicationCommands.Cut);
            cmdCut.CanExecute += new CanExecuteRoutedEventHandler(cmdCut_CanExecute);
            cmdCut.Executed += new ExecutedRoutedEventHandler(cmdCut_Executed);
            this.CommandBindings.Add(cmdCut);

            CommandBinding cmdCopy = new CommandBinding(ApplicationCommands.Copy);
            cmdCopy.Executed += new ExecutedRoutedEventHandler(cmdCopy_Executed);
            cmdCopy.CanExecute += new CanExecuteRoutedEventHandler(cmdCopy_CanExecute);
            this.CommandBindings.Add(cmdCopy);

            CommandBinding cmdPaste = new CommandBinding(ApplicationCommands.Paste);
            cmdPaste.CanExecute += new CanExecuteRoutedEventHandler(cmdPaste_CanExecute);
            cmdPaste.Executed += new ExecutedRoutedEventHandler(cmdPaste_Executed);
            this.CommandBindings.Add(cmdPaste);

            CommandBinding cmdUndo = new CommandBinding(ApplicationCommands.Undo);
            cmdUndo.CanExecute += new CanExecuteRoutedEventHandler(cmdUndo_CanExecute);
            cmdUndo.Executed += new ExecutedRoutedEventHandler(cmdUndo_Executed);
            this.CommandBindings.Add(cmdUndo);

            CommandBinding cmdRedo = new CommandBinding(ApplicationCommands.Redo);
            cmdRedo.Executed += new ExecutedRoutedEventHandler(cmdRedo_Executed);
            cmdRedo.CanExecute += new CanExecuteRoutedEventHandler(cmdRedo_CanExecute);
            this.CommandBindings.Add(cmdRedo);

            CommandBinding cmdPrint = new CommandBinding(ApplicationCommands.Print);
            cmdPrint.CanExecute += new CanExecuteRoutedEventHandler(cmdPrint_CanExecute);
            cmdPrint.Executed += new ExecutedRoutedEventHandler(cmdPrint_Executed);
            this.CommandBindings.Add(cmdPrint);

            CommandBinding cmdPrintPreview = new CommandBinding(ApplicationCommands.PrintPreview);
            cmdPrintPreview.Executed += new ExecutedRoutedEventHandler(cmdPrintPreview_Executed);
            cmdPrintPreview.CanExecute += new CanExecuteRoutedEventHandler(cmdPrintPreview_CanExecute);
            this.CommandBindings.Add(cmdPrintPreview);

            CommandBinding cmdMultiAlign = new CommandBinding(Commands.MultiAlign);
            cmdMultiAlign.Executed += new ExecutedRoutedEventHandler(MultiAlignCommand_Executed);
            cmdMultiAlign.CanExecute += new CanExecuteRoutedEventHandler(MultiAlignCommand_CanExecute);
            this.CommandBindings.Add(cmdMultiAlign);
            btnMultiAlign.Command = Commands.MultiAlign;

            CommandBinding cmdPhyTree = new CommandBinding(Commands.PhyTree);
            cmdPhyTree.CanExecute += new CanExecuteRoutedEventHandler(cmdPhyTree_CanExecute);
            cmdPhyTree.Executed += new ExecutedRoutedEventHandler(cmdPhyTree_Executed);
            this.CommandBindings.Add(cmdPhyTree);
            btnPhyTree.Command = Commands.PhyTree;
            #endregion

            propertyGrid.SelectedObject = this;
        }


        #region Commands
        void cmdPhyTree_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (dockSite.LastActiveWindow is MultiAlignWindow)
            {
                MultiAlignWindow maw = dockSite.LastActiveWindow as MultiAlignWindow;
                Tree tree = maw.Tree;

                PhyTreeWindow ptw = new PhyTreeWindow();
                //tree.Dump();
                tree = tree.PrettyOrder();
                //tree.Dump();
                ptw.Load(tree);
                ptw.Open(dockSite, WindowOpenMethod.OpenSelectActivate, "Phylogenetic tree");
            }
            else
            {
                // TODO: show wizard
            }
        }

        void cmdPhyTree_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void cmdPrintPreview_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void cmdPrintPreview_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void cmdPrint_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void cmdPrint_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void cmdRedo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void cmdRedo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void cmdUndo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void cmdUndo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }


        void cmdSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void cmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void cmdPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void cmdPaste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void cmdCopy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void cmdCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void cmdCut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void cmdCut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void cmdNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void cmdNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void cmdOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void cmdOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string fullName = null;
            // Open can be fired programatically, such as, tree item
            // it already pass the file name
            if (e.Parameter is FileInfo)
            {
                FileInfo file = (FileInfo)e.Parameter;
                fullName = file.FullName;
            }
            else
            {
                if (openFileDialog == null)
                {
                    openFileDialog = new System.Windows.Forms.OpenFileDialog();
                    openFileDialog.InitialDirectory = UserSetting.INSTANT.Folder_LastOpen;
                    openFileDialog.RestoreDirectory = false;
                }

                openFileDialog.Filter = "GenBank files (*.gb, *.gbk)|*.gb; *.gbk|" +
                    "Multiple alignment files (*.aln, *.msf)|*.aln; *.msf|" +
                    "Trace files (*.scf)|*.scf|" +
                    "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    fullName = openFileDialog.FileName;
                    if (!File.Exists(fullName))
                    {
                        MessageBox.Show(AppResource.Error_File_Not_Found + "\n" + fullName,
                            openFileDialog.Title, MessageBoxButton.OK, MessageBoxImage.Hand);
                        return;
                    }
                }
            }

            if (fullName == null)
                return;

            string fileName = System.IO.Path.GetFileName(fullName);

            try
            {
                SneakResult result = Misc.Sneak(fullName);

                if (result.Result == SneakAnswer.UnknownFormat || result.Result == SneakAnswer.Error)
                {
                    MessageBox.Show(AppResource.Error_File_NotSupported, "Open", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                if (result.FileType == FileType.MutliAlign)
                {
                    MultiAlignWindow win = new MultiAlignWindow();
                    win.Load(result);
                    win.Open(dockSite, WindowOpenMethod.OpenSelectActivate, "Multi alignment");
                }
                else if (result.FileType == FileType.Trace)
                {
                    TracePlotWindow tpw = new TracePlotWindow();
                    tpw.Open(dockSite, WindowOpenMethod.OpenSelectActivate, fullName);
                }
                else if (result.Quantity == Quantity.Single)
                {
                    SeqWindow seqWindow = new SeqWindow();
                    seqWindow.Open(dockSite, WindowOpenMethod.OpenSelectActivate, fileName, result);
                }

                else
                {
                    MultiSeqWindow mseqWindow = new MultiSeqWindow();
                    mseqWindow.DockSite = dockSite;
                    mseqWindow.Title = fileName;
                    mseqWindow.Load(result);
                    mseqWindow.Open(WindowOpenMethod.OpenSelectActivate);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(AppResource.Error_File_Parse + fullName + "\n" + err.Message, "File open error", MessageBoxButton.OK, MessageBoxImage.Error);
                // TODO: remove from dock
            }

        }

        private void MultiAlignCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter != null)
            {
                // FIXME: this is workaround, e.Parameter is suppose to pass list of seq
            }
            if (dockSite.LastActiveWindow is MultiSeqWindow)
            {
                MultiSeqWindow msw = (MultiSeqWindow)dockSite.LastActiveWindow;
                IList<SeqRecord> selectedItems = msw.SelectedRecords;
                if (selectedItems == null || selectedItems.Count < 3)
                {
                    selectedItems = msw.Records;
                }
                if (selectedItems == null || selectedItems.Count < 3)
                {
                    MessageBox.Show("At least 3 input sequences must be supplied to multi align",
                        "Multiple alignment", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }

                SeqRecord[] seqs = selectedItems.ToArray<SeqRecord>();
                BioCSharp.Algo.MultiAlign ma = new BioCSharp.Algo.MultiAlign(seqs);
                ma.DoMuscle();

                MultiAlignWindow win = new MultiAlignWindow();
                win.Load(ma);
                win.Open(dockSite, WindowOpenMethod.OpenSelectActivate, "Multi alignment");

            }
            else if (dockSite.LastActiveWindow is MultiAlignWindow)
            {
                MultiAlignWindow maw = (MultiAlignWindow)dockSite.LastActiveWindow;

                maw.DoAlignment();
            }
            else
            {
                // TODO: Show wizard
            }
        }

        private void MultiAlignCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // should always return true, because if current focus windows is not application
            // and wizard will be fired.
            e.CanExecute = true;
        }

        #endregion

        static public void Log(object sender, string message)
        {
            INSTANT.logTextBox.Text += ("\n" + message);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(UserSetting.INSTANT.Folder_Document);

                TreeViewItem item = new TreeViewItem();
                item.Tag = info;
                item.Header = info.Name;
                item.Items.Add("*");
                explorerTreeView.Items.Add(item);
            }
            catch (Exception e1)
            {
                Log(sender, e1.Message);
            }

            // locating
            try
            {
                Rect bound = Rect.Parse(UserSetting.INSTANT.MainForm_Bound);
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = bound.Left;
                this.Top = bound.Top;
                this.Width = bound.Width;
                this.Height = bound.Height;
            }
            catch
            {
            }

            // layout
            if (File.Exists(UserSetting.INSTANT.LayoutFileName))
            {
                try
                {
                    StreamReader reader = new StreamReader(UserSetting.INSTANT.LayoutFileName);
                    string s = reader.ReadToEnd();
                    reader.Close();
                    dockSite.SetLayout(s);
                }
                catch (Exception ec)
                {
                    Log(this, ec.Message);
                }

            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                // save data
                try
                {
                    //iMolBio.PlasmidCanvas.Layers.LayerManager.SaveSetting();
                }
                catch
                {
                    MessageBoxResult result = MessageBox.Show("Data cannot be save. Do you want to exit?",
                        "Saving file fail.", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                // save setting
                if (openFileDialog != null && openFileDialog.FileName.Length > 1)
                    UserSetting.INSTANT.Folder_LastOpen = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                if (saveFileDialog != null && saveFileDialog.FileName.Length > 1)
                    UserSetting.INSTANT.Folder_LastSave = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
                UserSetting.INSTANT.MainForm_Bound = this.RestoreBounds.ToString();


                // save layout
                try
                {
                    string layoutXML = dockSite.GetLayout(false);
                    StreamWriter sw = new StreamWriter(UserSetting.INSTANT.LayoutFileName);
                    sw.Write(layoutXML);
                    sw.Close();
                }
                catch
                {
                    Log(this, "unable to save layout file: " + UserSetting.INSTANT.LayoutFileName);
                }
                try { UserSetting.INSTANT.Save(); }
                catch { }
            }
            catch
            {
                // what can we do?, just let them close.
            }
        }

        #region Tree view
        void newItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem)
            {
                TreeViewItem item = (TreeViewItem)sender;
                if (item.Tag is FileInfo)
                {
                    FileInfo file = (FileInfo)item.Tag;
                    ApplicationCommands.Open.Execute(file, this);
                }
            }
        }

        private void item_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;


            DirectoryInfo dir;
            if (item.Tag is DriveInfo)
            {
                DriveInfo drive = (DriveInfo)item.Tag;
                dir = drive.RootDirectory;
            }
            else if (item.Tag is DirectoryInfo)
            {
                dir = (DirectoryInfo)item.Tag;
            }
            else
            {
                return;
            }

            item.Items.Clear();

            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = subDir;
                    newItem.Header = subDir.Name;
                    newItem.Items.Add("*");
                    item.Items.Add(newItem);
                }
                foreach (FileInfo file in dir.GetFiles())
                {
                    //if (file.Name.EndsWith(".gbk") || file.Name.EndsWith(".gb") || file.Name.EndsWith(".fasta") ||
                    //    file.Name.EndsWith(".faa") || file.Name.EndsWith(".fsa") || file.Name.EndsWith(".aln")
                    //     || file.Name.EndsWith(".msf") || file.Name.EndsWith(".scf"))
                    //{
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = file;
                    newItem.Header = file.Name;
                    newItem.MouseDoubleClick += new MouseButtonEventHandler(newItem_MouseDoubleClick);
                    item.Items.Add(newItem);
                    //}
                }
            }
            catch
            {
                //Window1.log(this, ec.Message);
            }
        }
        #endregion

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dockSite == null || dockSite.LastActiveWindow == null || e.AddedItems == null || e.AddedItems.Count == 0)
            {
                return;
            }
            DockableWindow activeWindow = dockSite.LastActiveWindow;
            if (activeWindow is SeqWindow)
            {
                SeqWindow seqWindow = (SeqWindow)activeWindow;
                seqWindow.Zoom(e.AddedItems[0].ToString());
                e.Handled = true;
            }

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }


        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (dockSite == null || dockSite.LastActiveWindow == null || e.Key != Key.Enter)
            {
                return;
            }
            DockableWindow activeWindow = dockSite.LastActiveWindow;
            if (activeWindow is SeqWindow)
            {
                SeqWindow seqWindow = (SeqWindow)activeWindow;
                string zoomValue = seqWindow.Zoom(comboBoxZoom.Text);
                comboBoxZoom.Text = zoomValue;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Set LayerManager of active canvas to main windows's layer table
        /// </summary>
        public static IEnumerable LayerData
        {
            set
            {

                INSTANT.dgdLayer.ItemsSource = value;
                

                // Make combobox
                // INSTANT.dgdLayer.Columns["Visibility"].CellEditor = INSTANT.FindResource("supplierEditor") as Xceed.Wpf.DataGrid.CellEditor;
                // INSTANT.dgdLayer.Columns["Visibility"].CellContentTemplate = INSTANT.FindResource("periodCellDataTemplate") as DataTemplate;
            }
        }

       
    }
}
