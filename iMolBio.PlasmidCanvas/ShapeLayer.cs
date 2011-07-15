using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace iMoBio.PlasmidCanvas
{
    ///// <summary>
    ///// The layer redraw itself
    ///// </summary>
    //public class ShapeLayer : Layer
    //{
    //    protected readonly Path shapeLayer = null;

    //    public ShapeLayer(LayerManager parent, string name, bool isFeatureLayer,
    //        double offset, double width) : base(parent, name, isFeatureLayer, offset, width)
    //    {
    //        shapeLayer = new Path();
            
    //        shapeLayer.SnapsToDevicePixels = true;
    //        shapeLayer.Fill = Brushes.Transparent;
    //        shapeLayer.Stroke = Brushes.Transparent;
    //        shapeLayer.StrokeThickness = 0.5;
    //        shapeLayer.Visibility = visibility;
    //        shapeLayer.Focusable = true;
    //        shapeLayer.MouseDown += new System.Windows.Input.MouseButtonEventHandler(shape_MouseDown);
    //        shapeLayer.LostFocus += new RoutedEventHandler(shapeLayer_LostFocus);

    //        parent.Add(this);
    //        Canvas.SetZIndex(shapeLayer, parent.GetZIndexNewLayer());
    //    }

    //    /// <summary>
    //    /// Get shape of this layer.
    //    /// </summary>
    //    [Browsable(false)]
    //    protected internal Path Shape
    //    {
    //        get
    //        {
    //            return shapeLayer;
    //        }
    //    }

    //    void shapeLayer_LostFocus(object sender, RoutedEventArgs e)
    //    {
    //        //Window1.PropertyEditor.SelectedObject = this;
    //        //Window1.PropertyEditor.Text = AppResource.PropertyEditor_Title;
    //    }

    //    void shape_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    //    {
    //        //Window1.PropertyEditor.SelectedObject = this;
    //        //Window1.PropertyEditor.Text = "Layer - " + AppResource.PropertyEditor_Title;
    //        // e.Handled = true;
    //    }
    //}
}
