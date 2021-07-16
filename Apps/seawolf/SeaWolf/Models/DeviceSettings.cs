using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeaWolf.Models
{
    public class DeviceSettings<T> : ModelBase
    {
        private T _initialValue;

        public DeviceSettings(String label, T initialValue)
        {
            Label = label;
            Value = initialValue;
            _initialValue = initialValue;
        }
        public string Label { get; }

        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                Set(ref _value, value);
                RaisePropertyChanged(nameof(IsDirty));
            }
        }

        public bool IsDirty {get { return !Value.Equals(_initialValue); } }
    }
}
