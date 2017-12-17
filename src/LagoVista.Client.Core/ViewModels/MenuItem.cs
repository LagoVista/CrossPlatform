using System;
using System.Windows.Input;

namespace LagoVista.Client.Core.ViewModels
{
    public class MenuItem
    {
        public event EventHandler<MenuItem> MenuItemTapped;

        public String FontIconKey { get; set; }

        public String Name { get; set; }

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public void RaiseMenuItemTapped()
        {
            MenuItemTapped?.Invoke(this, this);
        }
    }
}
