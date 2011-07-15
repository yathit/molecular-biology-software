using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Windows.Controls;

namespace BaseLibrary
{
    public static class AppBase
    {
        /// <summary>
        /// Single instant of property grid in iMolBio application.
        /// </summary>
        static public PropertyGrid PropertyGrid { get; set; }
        static public Microsoft.Windows.Controls.DataGrid LayerGrid { get; set; }

        

        static public void Log(object logger, string message)
        {
            Console.WriteLine(message);
        }
    }
}
