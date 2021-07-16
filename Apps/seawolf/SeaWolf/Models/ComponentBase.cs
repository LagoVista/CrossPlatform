using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeaWolf.Models
{
    public class ComponentBase : ModelBase
    {
        public ComponentBase(string key)
        {
            Key = key;
        }

        public String Key { get; }
    }
}
