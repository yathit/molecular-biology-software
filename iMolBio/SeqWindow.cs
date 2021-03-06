﻿using System;
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
using Divelements.SandDock;
using BioCSharp.Misc;
using iMoBio.PlasmidCanvas;
using BioCSharp.Seqs;

namespace iMoBio
{
    /// <summary>
    /// Interaction logic for SeqWindow.xaml
    /// </summary>
    public class SeqWindow : Divelements.SandDock.DocumentWindow
    {
        private string _fileName;
        private PlasmidRecord plasmid;
        //private Dictionary<string, UIElement> canvases = new Dictionary<string, UIElement>();

        TabControl tabCon = new TabControl();


        public SeqWindow()
        {
            tabCon.TabStripPlacement = System.Windows.Controls.Dock.Bottom;
            TabItem ti = new TabItem();
            ti.Header = CircularCanvas.CanvasType;
            tabCon.Items.Add(ti);
            ti = new TabItem();
            ti.Header = HorizontalCanvas.CanvasType;
            tabCon.Items.Add(ti);
            ti = new TabItem();
            ti.Header = VerticalCanvas.CanvasType;
            tabCon.Items.Add(ti);
            ti = new TabItem();
            ti.Header = SequenceCanvas.CanvasType;
            tabCon.Items.Add(ti);
            ti = new TabItem();
            ti.Header = GenBankCanvas.CanvasType;
            tabCon.Items.Add(ti);
            //ti = new TabItem();
            //ti.Header = MosaicCanvas.CanvasType;
            //tabCon.Items.Add(ti);

            tabCon.SelectionChanged += new SelectionChangedEventHandler(tabCon_SelectionChanged);
            
            this.Child = tabCon; 

        }

        void tabCon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem ti = tabCon.SelectedItem as TabItem;
            if (ti.Content == null)
            {
                CreateCanvas(ti.Header.ToString());
            }
            if (ti.Content is PlasmidCanvas.PlasmidCanvas)
            {
                PlasmidCanvas.PlasmidCanvas plasmid = (PlasmidCanvas.PlasmidCanvas) ti.Content;
                MainWindow.LayerData = plasmid.LayerManager;
            }
            else if (ti.Content is ScrollViewer)
            {
                ScrollViewer sv = (ScrollViewer) ti.Content;
                PlasmidCanvas.PlasmidCanvas plasmid = (PlasmidCanvas.PlasmidCanvas)sv.Content;
                MainWindow.LayerData = plasmid.LayerManager;
            }
            else
            {
                MainWindow.LayerData = null;
            }

        }

       

        public void Open(DockSite dockSite, WindowOpenMethod method, string title, SneakResult result)
        {
            if (result.Result == SneakAnswer.OK)
            {
                _fileName = result.FileName;
                InsdcRecord[] records = GenBank.parse(_fileName);
                plasmid = new PlasmidRecord(records[0]);


                CreateCanvas(result.Topology.ToString());

            }
            else
            {
                throw new InvalidOperationException("Cannot open invalid format.");
            }

            base.DockSite = dockSite;
            base.Title = title;
            base.Open(method);
        }

        private void CreateCanvas(string canvasType)
        {
            if (canvasType == CircularCanvas.CanvasType)
            {
                CircularCanvas activeCanvas = new CircularCanvas();
                activeCanvas.PlasmidRecord = plasmid;
                ScrollViewer sc = new ScrollViewer();
                sc.Content = activeCanvas;
                sc.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                sc.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                (tabCon.Items[0] as TabItem).Content =  sc;
                tabCon.SelectedIndex = 0;
            }
            else if (canvasType == HorizontalCanvas.CanvasType)
            {
                HorizontalCanvas activeCanvas = new HorizontalCanvas();
                activeCanvas.PlasmidRecord = plasmid;
                ScrollViewer sc = new ScrollViewer();
                sc.Content = activeCanvas;
                sc.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                sc.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                (tabCon.Items[1] as TabItem).Content = sc;
                tabCon.SelectedIndex = 1;
            }
            else if (canvasType == VerticalCanvas.CanvasType)
            {
                VerticalCanvas activeCanvas = new VerticalCanvas();
                activeCanvas.PlasmidRecord = plasmid;
                ScrollViewer sc = new ScrollViewer();
                sc.Content = activeCanvas;
                sc.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                sc.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                (tabCon.Items[2] as TabItem).Content = sc;
                tabCon.SelectedIndex = 2;
            }
            else if (canvasType == SequenceCanvas.CanvasType)
            {
                SequenceCanvas activeCanvas = new SequenceCanvas();
                activeCanvas.PlasmidRecord = plasmid;
                (tabCon.Items[3] as TabItem).Content = activeCanvas;
                tabCon.SelectedIndex = 3;
            }
            else if (canvasType == GenBankCanvas.CanvasType)
            {
                GenBankCanvas activeCanvas = new GenBankCanvas();
                activeCanvas.PlasmidRecord = plasmid;
                (tabCon.Items[4] as TabItem).Content = activeCanvas;
                tabCon.SelectedIndex = 4;
            }
            else if (canvasType == MosaicCanvas.CanvasType)
            {
                MosaicCanvas activeCanvas = new MosaicCanvas();
                activeCanvas.PlasmidRecord = plasmid;
                (tabCon.Items[5] as TabItem).Content = activeCanvas;
                tabCon.SelectedIndex = 5;
            }
            
        }


        internal string Zoom(string zoomValue)
        {
            if (tabCon.SelectedContent is PlasmidCanvas.PlasmidCanvas)
            {
                return ((PlasmidCanvas.PlasmidCanvas)tabCon.SelectedContent).Zoom(zoomValue);
            }
            else if (tabCon.SelectedContent is ScrollViewer)
            {
                ScrollViewer sc = (ScrollViewer)tabCon.SelectedContent;
                if (sc.Content is PlasmidCanvas.PlasmidCanvas)
                {
                    return ((PlasmidCanvas.PlasmidCanvas)sc.Content).Zoom(zoomValue);
                }
            }
            return zoomValue;
        }
    }


}
