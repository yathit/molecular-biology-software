using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace iMoBio.PlasmidCanvas
{
    /// <summary>
    /// Interface for plasmid canvas. There is abstract class <code>PlasmidCanvas</code>, which
    /// implement basic functionality for displaying plasmid.
    /// </summary>
    public interface ISeqCanvas
    {

        /// <summary>
        /// Get or set the plasmid
        /// </summary>
        PlasmidRecord PlasmidRecord { get; set; }

        /// <summary>
        /// Set zoom level. Generally zoom level is specify in percentage. However mouse wheel
        /// event are suppose to append with "Delta" to its delta value.
        /// </summary>
        /// <param name="zoomValue"></param>
        /// <returns>actual zoom in parcentage</returns>
        string Zoom(string zoomValue);
        
        /// <summary>
        /// Get or set the layout
        /// </summary>
        LayerManager LayerManager { get; }

        /// <summary>
        /// Initialize layers and annotation and finally call <see cref="Redraw()"/>.
        /// Request to update. This will not call when setting <code>PlasmidRecord</code> or
        /// <code>LayerMangager</code>. It is implementer responsiblity to update during
        /// these setting processes.
        /// </summary>
        void Init();

        /// <summary>
        /// Redraw the graphic component.
        /// 
        /// ISeqCanvas are responsbile to redraw layer and annotation.
        /// <seealso cref="Init()"/>
        /// </summary>
        void Redraw();

        /// <summary>
        /// Add child component to the canvas
        /// </summary>
        /// <param name="child"></param>
        void AddChild(UIElement child);
    }
}
