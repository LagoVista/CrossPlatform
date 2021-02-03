using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace LagoVista.AppLoader.Controls
{
    public class AutoScrollListBox : ListBox
    {
        public AutoScrollListBox()
        {
            if (Items == null)
            {
                return;
            }
            
                // Hook to the CollectionChanged event of your ObservableCollection
             ((INotifyCollectionChanged)Items).CollectionChanged += CollectionChange;
        }

        // Is called whenever the item collection changes
        private void CollectionChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Items.Count > 0)
            {
                // Get the ScrollViewer object from the ListBox control
                Border border = (Border)VisualTreeHelper.GetChild(this, 0);
                ScrollViewer SV = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);

                // Scroll to bottom
                SV.ScrollToBottom();
            }
        }
    }
}
