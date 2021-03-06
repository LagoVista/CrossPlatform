﻿using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeaWolf.Models
{
    public class TemperatureComponent : ComponentBase
    {
        public TemperatureComponent(string key) : base(key)
        {

        }

        private string _label;
        public string Label { get { return _label; } set { Set(ref _label, value); } }

        private string _value;
        public string Value { get { return _value; } set { Set(ref _value, value); } }

        public string Image => "thermo.png";
    }
}