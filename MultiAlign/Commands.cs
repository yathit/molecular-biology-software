using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace iMoBio.Controls
{
    public class Commands
    {
        private static RoutedUICommand multiAlign;
        private static RoutedUICommand phyTree;

        static Commands()
        {
            InputGestureCollection inputsMultiAlign = new InputGestureCollection();
            inputsMultiAlign.Add(new KeyGesture(Key.M, ModifierKeys.Control, "Ctrl+M"));

            multiAlign = new RoutedUICommand(
                "Create multialignment", "MultiAlign", typeof(Commands), inputsMultiAlign);

            InputGestureCollection inputsPhyTree = new InputGestureCollection();
            inputsPhyTree.Add(new KeyGesture(Key.T, ModifierKeys.Control, "Ctrl+T"));

            phyTree = new RoutedUICommand(
                "Create phylogenetic tree", "PhyTree", typeof(Commands), inputsPhyTree);
        }

        public static RoutedUICommand MultiAlign
        {
            get { return multiAlign; }
        }

        public static RoutedUICommand PhyTree
        {
            get { return phyTree; }
        }
    }
}
