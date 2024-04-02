using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OptiMate.ViewModels;

namespace OptiMate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ScriptWindow : Window
    {
        private bool DragSelected = false;

        public ScriptWindow(ViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Close_GUI(object sender, RoutedEventArgs e)
        {
            Close(); // this closes the script GUI
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            var VM = (ViewModel)DataContext;
            if (e.Key == Key.Enter)
                VM.IsErrorAcknowledged = true;
        }

      

        private void TemplateStructures_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TemplateStructureListView.AllowDrop = true;
            DragSelected = true;
        }

        private void ListView_MouseLeave(object sender, MouseEventArgs e)
        {
            // This method disables drag after the mouse leaves the listbox, so user needs to click on drag icon again to restart
            // However, if the mouse doen't leave the listbox, drag can still be accomplished by clicking the listboxitem

            var LV = sender as ListView; // all this to get the listbox!
            if (LV.AllowDrop && Mouse.LeftButton == MouseButtonState.Released)
            {
                LV.AllowDrop = false;
                DragSelected = false; // this needs to be at protocol level as it's used to suppress selection/expansion of constraint in XAML
            }
        }

        delegate Point GetPositionDelegate(IInputElement element);
        
        ListViewItem GetListViewItem(int index, ListView LV)
        {
            if (LV.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return LV.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }
        bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }

        private void DragTemplateStructure_ListView_DragOver(object sender, DragEventArgs e)
        {
            var VM = (DataContext as ViewModels.ViewModel).ActiveTemplate;
            var SelectedIndex = DragTemplateStructure_GetCurrentIndex(e.GetPosition);
            //

            var DropIndex = VM.SelectedTSIndex;
            if (SelectedIndex < 0)
                return;
            if (DropIndex < 0)
                return;
            if (SelectedIndex == DropIndex)
                return;
            if (DragSelected)
            {
                int inc = 1;
                if (SelectedIndex < DropIndex)
                    inc = -1;
                int CurrentIndex = DropIndex;
                while (CurrentIndex != SelectedIndex)
                {

                    VM.ReorderTemplateStructures(CurrentIndex + inc, CurrentIndex);
                    CurrentIndex = CurrentIndex + inc;
                }
            }
        }

        private void GenStructures_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GenStructureListView.AllowDrop = true;
            DragSelected = true;
        }
        int DragTemplateStructure_GetCurrentIndex(GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0; i < TemplateStructureListView.Items.Count; ++i)
            {
                ListViewItem item = GetListViewItem(i, TemplateStructureListView);
                if (item != null)
                    if (this.IsMouseOverTarget(item, getPosition))
                    {
                        index = i;
                        break;
                    }
            }
            return index;
        }
        int DragGenStructure_GetCurrentIndex(GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0; i < GenStructureListView.Items.Count; ++i)
            {
                ListViewItem item = GetListViewItem(i, GenStructureListView);
                if (item != null)
                    if (this.IsMouseOverTarget(item, getPosition))
                    {
                        index = i;
                        break;
                    }
            }
            return index;
        }
        private void DragGenStructure_ListView_DragOver(object sender, DragEventArgs e)
        {
            var VM = (DataContext as ViewModels.ViewModel).ActiveTemplate;
            var SelectedIndex = DragGenStructure_GetCurrentIndex(e.GetPosition);
            //

            var DropIndex = VM.SelectedGSIndex;
            if (SelectedIndex < 0)
                return;
            if (DropIndex < 0)
                return;
            if (SelectedIndex == DropIndex)
                return;
            if (DragSelected)
            {
                int inc = 1;
                if (SelectedIndex < DropIndex)
                    inc = -1;
                int CurrentIndex = DropIndex;
                while (CurrentIndex != SelectedIndex)
                {

                    VM.ReorderGenStructures(CurrentIndex + inc, CurrentIndex);
                    CurrentIndex = CurrentIndex + inc;
                }
            }
        }
        private void ListView_Drop(object sender, DragEventArgs e)
        {
            var LV = sender as ListView; // all this to get the listbox!
            if (LV.AllowDrop && Mouse.LeftButton == MouseButtonState.Released)
            {
                LV.AllowDrop = false;
                DragSelected = false;
            }
        }

        
    }
}
