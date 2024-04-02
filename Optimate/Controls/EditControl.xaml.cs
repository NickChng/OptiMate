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

namespace OptiMate.Controls
{
    /// <summary>
    /// Interaction logic for EditControl.xaml
    /// </summary>
    public partial class EditControl : UserControl
    {
        public EditControl()
        {
            InitializeComponent();
        }

        public bool DragSelected { get; set; }
        delegate Point GetPositionDelegate(IInputElement element);

        ListBoxItem GetListViewItem(int index, ListBox LV)
        {
            if (LV.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return LV.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        }
        bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }

        private void AliasList_DragOver(object sender, DragEventArgs e)
        {
            var VM = (DataContext as ViewModels.EditControlViewModel);
            var SelectedIndex = DragAlias_GetCurrentIndex(e.GetPosition);
            //

            var DropIndex = VM.SelectedAliasIndex;
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

                    VM.ReorderAliases(CurrentIndex + inc, CurrentIndex);
                    CurrentIndex = CurrentIndex + inc;
                }
            }
        }
        int DragAlias_GetCurrentIndex(GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0; i < structure_aliaslist.Items.Count; ++i)
            {
                ListBoxItem item = GetListViewItem(i, structure_aliaslist);
                if (item != null)
                    if (this.IsMouseOverTarget(item, getPosition))
                    {
                        index = i;
                        break;
                    }
            }
            return index;
        }
        private void AliasList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            structure_aliaslist.AllowDrop = true;
            DragSelected = true;
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            var LV = sender as ListBox; // all this to get the listbox!
            if (LV.AllowDrop && Mouse.LeftButton == MouseButtonState.Released)
            {
                LV.AllowDrop = false;
                DragSelected = false;
            }
        }
    }
}
