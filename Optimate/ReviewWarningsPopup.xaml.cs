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


namespace Optimate
{
    /// <summary>
    /// Interaction logic for HorizontalPopup.xaml
    /// </summary>
    public partial class ReviewWarningsPopup : UserControl
    {
        public ReviewWarningsPopup()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    ScrollViewer scv = (ScrollViewer)sender;
        //    scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
        //    e.Handled = true;
        //}
    }
}
