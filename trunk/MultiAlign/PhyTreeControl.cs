using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioCSharp.Algo.Phylo;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Media;

namespace iMoBio.Controls
{
    public class PhyTreeControl : Canvas
    {
        Tree tree;
        double scaleBarWidth = 0.0;
        double scaleBarHeight = 0.0;
        double branchSize = 7.5;
        double gap = 20.0;
        /// <summary>
        /// gap + title height
        /// </summary>
        double top = 20.0;
        double DefaultHeight = 400.0;
        double DefaultWidth = 400.0;

        const string tagNode = "node";
        const string tagBranch = "branch";
        const string tagLine = "line";
        const string tagDistance = "dist";

        Pen penOutline = new Pen();

        Label scaleLabel;
        Line scaleBar;
        Label lblTitle;
        EditorObject editorObject;

        public PhyTreeControl(Tree tree)
        {
            this.tree = tree;

            penOutline.Brush = Brushes.Black;
            penOutline.Thickness = 1;
            penOutline.Freeze();
            Size availableSize = new Size(400, 400);

            lblTitle = new Label();
            lblTitle.Content = tree.Name;
            lblTitle.Measure(availableSize);
            Canvas.SetTop(lblTitle, gap);
            Canvas.SetLeft(lblTitle, (DefaultWidth + 2 * gap - lblTitle.DesiredSize.Width) / 2);
            top = gap + lblTitle.DesiredSize.Height;
            Children.Add(lblTitle);

            DefaultHeight = lblTitle.DesiredSize.Height * tree.NumOfLeaves * 1.0;
            DefaultWidth = DefaultHeight;

            double maxX = tree.MaxX;
            double maxY = tree.MaxY;
            double scaleX = DefaultHeight / maxX;
            double scaleY = DefaultWidth / maxY;
            double maxLabelWidth = 0.0;


            int zIndexLine = 10;
            int zIndexLeave = 50;
            int zIndexBranch = 30;

            #region draw lines
            foreach (Node node in tree)
            {
                Line line = new Line();
                line.Y1 = top + node.Y * scaleY;
                line.Y2 = top + node.Y * scaleY;
                line.X1 = gap + node.X * scaleX;
                line.X2 = gap + (node.X - node.Distance) * scaleX;
                line.ToolTip = node.Distance;
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;
                line.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                line.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                Canvas.SetZIndex(line, zIndexLine);
                line.Tag = tagLine;
                Children.Add(line);

                #region distance label
                Label lblDist = new Label();
                lblDist.Content = Math.Round(node.Distance, 3);
                lblDist.FontSize -= 2;
                lblDist.Measure(availableSize);
                lblDist.Tag = tagDistance;

                if ((node is Branch) && tree.Parent(node).Y < node.Y)
                {
                    // parent is below
                    Canvas.SetTop(lblDist, top + node.Y * scaleY - lblDist.DesiredSize.Height * 0.2);
                }
                else
                {
                    Canvas.SetTop(lblDist, top + node.Y * scaleY - lblDist.DesiredSize.Height * 0.8);
                }
                Canvas.SetLeft(lblDist, gap + node.X * scaleX - lblDist.DesiredSize.Width);
                Children.Add(lblDist);
                #endregion

                if (node is Branch)
                {
                    // draw a shoulder line
                    Branch b = node as Branch;
                    line = new Line();
                    line.X1 = gap + b.X * scaleX;
                    line.X2 = gap + b.X * scaleX;
                    line.Y1 = top + (b.Child1.Y) * scaleY;
                    line.Y2 = top + (b.Child2.Y) * scaleY;
                    line.Stroke = Brushes.Black;
                    line.StrokeThickness = 1;
                    line.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    line.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    Canvas.SetZIndex(line, zIndexLine);
                    line.Tag = tagLine;
                    Children.Add(line);
                }
            }
            #endregion

            #region draw nodes
            foreach (Node node in tree)
            {
                if (node is Leaf)
                {
                    Label nodeLabel = new Label();
                    nodeLabel.Content = node.Name;
                    nodeLabel.ToolTip = node.ToString();
                    // nodeLabel.Background = System.Windows.Media.Brushes.AliceBlue;
                    nodeLabel.Tag = tagNode;
                    Children.Add(nodeLabel);
                    nodeLabel.Measure(availableSize);
                    if (nodeLabel.DesiredSize.Width > maxLabelWidth)
                        maxLabelWidth = nodeLabel.DesiredSize.Width;
                    Canvas.SetTop(nodeLabel, top + (node.Y * scaleY) - nodeLabel.DesiredSize.Height / 2);
                    Canvas.SetLeft(nodeLabel, gap + node.X * scaleX);
                    Canvas.SetZIndex(nodeLabel, zIndexLeave);
                    // Console.WriteLine(nodeLabel.DesiredSize.ToString());
                }
                else // a branch
                {
                    Ellipse shape = new Ellipse();
                    shape.Tag = tagBranch;
                    shape.Fill = Brushes.Blue;
                    shape.Width = branchSize;
                    shape.Height = branchSize;
                    shape.ToolTip = node.ToString();
                    shape.Stroke = Brushes.Black;
                    Canvas.SetTop(shape, top + (node.Y * scaleY) - branchSize / 2);
                    Canvas.SetLeft(shape, gap + (node.X * scaleX) - branchSize / 2);
                    Canvas.SetZIndex(shape, zIndexBranch);
                    Children.Add(shape);
                }
            }
            #endregion

            #region draw scale bar
            scaleBarWidth = DefaultWidth / 8.0;
            double scale = scaleBarWidth / scaleX;
            scale = Math.Round(scale, 1); // round off
            scaleBarWidth = scale * scaleX;
            scaleLabel = new Label();
            scaleLabel.Content = scale;
            scaleLabel.FontSize -= 2;
            scaleLabel.Measure(availableSize);
            scaleBarHeight = scaleLabel.DesiredSize.Height;
            Canvas.SetTop(scaleLabel, gap + top + DefaultHeight);
            Canvas.SetLeft(scaleLabel, gap * 2);
            Children.Add(scaleLabel);

            scaleBar = new Line();
            scaleBar.Y1 = top + gap + DefaultHeight + scaleBarHeight;
            scaleBar.Y2 = top + gap + DefaultHeight + scaleBarHeight;
            scaleBar.X1 = gap * 2;
            scaleBar.X2 = gap * 2 + scaleBarWidth;
            scaleBar.ToolTip = "Scale";
            scaleBar.Stroke = Brushes.Black;
            scaleBar.StrokeThickness = 2;
            scaleBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            scaleBar.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Canvas.SetZIndex(scaleBar, zIndexLeave + 1);
            Children.Add(scaleBar);
            #endregion

            if (Double.IsNaN(scaleBarHeight))
                scaleBarHeight = 0;
            if (Double.IsNaN(maxLabelWidth))
                maxLabelWidth = 0;
            Height = GetHeight();
            Width = DefaultWidth + maxLabelWidth + branchSize * 2 + gap * 2;

            editorObject = new EditorObject(this);
        }

        public EditorObject Editor
        {
            get { return editorObject; }
        }

        private double GetHeight()
        {
            return DefaultHeight + gap + top + branchSize * 2 + scaleBarHeight;
        }


        public class EditorObject
        {
            private Tree tree;
            private PhyTreeControl parent;
            public EditorObject(PhyTreeControl parent)
            {
                this.parent = parent;
                tree = parent.tree;
            }


            [Description("Tree name")]
            public string TreeName
            {
                get { return tree.Name; }
                set
                {
                    tree.Name = value;
                    parent.lblTitle.Content = tree.Name;
                }
            }

            [Description("Title"), Category("Apperance")]
            public Visibility Title
            {
                get { return parent.lblTitle.Visibility; }
                set
                {
                    parent.lblTitle.Visibility = value;

                    if (parent.lblTitle.Visibility == Visibility.Visible)
                        parent.top = parent.gap + parent.lblTitle.DesiredSize.Height;
                    else
                        parent.top = parent.gap;

                    parent.Height = parent.GetHeight();
                }
            }




            [Description("Scale bar"), Category("Apperance")]
            public Visibility ScaleBar
            {
                get { return parent.scaleLabel.Visibility; }
                set
                {
                    parent.scaleLabel.Visibility = value;
                    parent.scaleBar.Visibility = value;
                    if (value == Visibility.Visible)
                        parent.scaleBarHeight = parent.scaleLabel.DesiredSize.Height;
                    else
                        parent.scaleBarHeight = 0.0;

                    parent.Height = parent.GetHeight();
                }
            }

            [Description("Distance labeling"), Category("Apperance")]
            public Visibility DistanceLabel
            {
                get
                {
                    foreach (UIElement ui in parent.Children)
                    {
                        if (ui is Label)
                        {
                            Label label = ui as Label;
                            if (PhyTreeControl.tagDistance.Equals(label.Tag))
                                return label.Visibility;
                        }
                    }
                    return Visibility.Collapsed;
                }
                set
                {
                    foreach (UIElement ui in parent.Children)
                    {
                        if (ui is Label)
                        {
                            Label label = ui as Label;
                            if (tagDistance.Equals(label.Tag))
                                label.Visibility = value;
                        }
                    }
                }
            }

            [Description("Label font size"), Category("Apperance")]
            public double LabelFont
            {
                get
                {
                    foreach (UIElement ui in parent.Children)
                    {
                        if (ui is Label)
                        {
                            Label label = ui as Label;
                            if (tagNode.Equals(label.Tag))
                            {
                                // because all label has same font size
                                return label.FontSize;
                            }
                        }
                    }
                    return 0.0; // not possible anyways
                }
                set
                {
                    foreach (UIElement ui in parent.Children)
                    {
                        if (ui is Label)
                        {
                            Label label = ui as Label;
                            if (tagNode.Equals(label.Tag))
                            {
                                // because all label has same font size
                                label.FontSize = value;
                            }
                        }
                    }
                }
            }
        }
    }
}
