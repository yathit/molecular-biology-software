using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace iMoBio.PlasmidCanvas
{
    /// <summary>
    /// This class maintain Visuals in the canvas
    /// </summary>
    public abstract class DrawingPlasmidCanvas : PlasmidCanvasUI
    {


        private List<Visual> visuals = new List<Visual>();

        public DrawingPlasmidCanvas()
        {
            Background = Brushes.White; // to received mouse input
            ClipToBounds = true;

            // we want to get exact available content
            MinHeight = 0.0;
            MinWidth = 0.0;
            Height = Double.NaN;
            Width = Double.NaN;
        }



        #region Override
        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }
        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

       
        #endregion 

        #region Public method
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
}
