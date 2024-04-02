using HTMLConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Input;

namespace OptiMate.Behaviours
{
    public static class HtmlTextBoxProperties
    {
        public static string GetHtmlText(TextBlock wb)
        {
            return wb.GetValue(HtmlTextProperty) as string;
        }
        public static void SetHtmlText(TextBlock wb, string html)
        {
            wb.SetValue(HtmlTextProperty, html);
        }
        public static readonly DependencyProperty HtmlTextProperty =
            DependencyProperty.RegisterAttached("HtmlText", typeof(string), typeof(HtmlTextBoxProperties), new UIPropertyMetadata("", OnHtmlTextChanged));

        private static void OnHtmlTextChanged(
            DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            // Go ahead and return out if we set the property
            //on something other than a textblock, or set a value that is not a string.
            var txtBox = depObj as TextBlock;
            if (txtBox == null)
                return;
            if (!(e.NewValue is string))
                return;
            var html = e.NewValue as string;
            string xaml;
            InlineCollection xamLines;
            try
            {
                xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, false);
                xamLines = ((Paragraph)((Section)System.Windows.Markup.XamlReader.Parse(xaml)).Blocks.FirstBlock).Inlines;
            }
            catch
            {
                // There was a problem parsing the html, return out. 
                return;
            }
            // Create a copy of the Inlines and add them to the TextBlock.
            Inline[] newLines = new Inline[xamLines.Count];
            xamLines.CopyTo(newLines, 0);
            txtBox.Inlines.Clear();
            foreach (var l in newLines)
            {
                txtBox.Inlines.Add(l);
            }
        }
    }
    public class TextBoxExtensions
    {
        public static readonly DependencyProperty UpdateSourceOnKeyProperty = DependencyProperty.RegisterAttached("UpdateSourceOnKey", typeof(Key), typeof(TextBox), new FrameworkPropertyMetadata(Key.None));

        public static void SetUpdateSourceOnKey(UIElement element, Key value)
        {
            element.PreviewKeyUp += TextBoxKeyUp;
            element.SetValue(UpdateSourceOnKeyProperty, value);
        }

        static void TextBoxKeyUp(object sender, KeyEventArgs e)
        {

            var textBox = sender as TextBox;
            if (textBox == null) return;

            var propertyValue = (Key)textBox.GetValue(UpdateSourceOnKeyProperty);
            if (e.Key != propertyValue) return;

            var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression != null)
            {
                try
                {
                    bindingExpression.UpdateSource();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0} {1} {2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
        }

        public static Key GetUpdateSourceOnKey(UIElement element)
        {
            return (Key)element.GetValue(UpdateSourceOnKeyProperty);
        }

    }
}
