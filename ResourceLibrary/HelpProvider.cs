using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace BaseLibrary
{
    public static class HelpProvider
    {

        public static int GetHelpID(DependencyObject obj)
        {
            return (int)obj.GetValue(HelpIDProperty);
        }

        public static void SetHelpID(DependencyObject obj, int value)
        {
            obj.SetValue(HelpIDProperty, value);
        }

        public static readonly DependencyProperty HelpIDProperty =
            DependencyProperty.RegisterAttached(
                           "HelpID", typeof(Int32), typeof(HelpProvider), 
                           new PropertyMetadata(0));

        static HelpProvider()
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(FrameworkElement),
                new CommandBinding(
                    ApplicationCommands.Help,
                    new ExecutedRoutedEventHandler(Executed),
                    new CanExecuteRoutedEventHandler(CanExecute)));

        }
        

        static private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;

            if (HelpProvider.GetHelpID(senderElement) > 0)
                e.CanExecute = true;

        }



        static private void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.molecular-biology-software.org/hdhelp.aspx?id=" +
                HelpProvider.GetHelpID(sender as FrameworkElement));
            //MessageBox.Show("Help: " + HelpProvider.GetHelpID(sender as FrameworkElement));
        } 


    }
}
