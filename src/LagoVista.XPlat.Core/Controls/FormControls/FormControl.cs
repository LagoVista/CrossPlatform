using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.UIMetaData;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.FormControls
{
    public abstract class FormControl : StackLayout, INotifyPropertyChanged
    {
        FormField _field;
        FormViewer _viewer;

        public FormControl(FormViewer viewer, FormField field)
        {
            _field = field;
            _viewer = viewer;
            IsVisible = field.IsVisible;
            OriginalValue = _field.Value;
        }

        public FormField Field { get { return _field; } }
        protected FormViewer Viewer { get { return _viewer; } }

        public abstract bool Validate();

        public virtual void Refresh()
        {
            IsVisible = _field.IsVisible;
            IsEnabled = _field.IsUserEditable;
        }

        public IAppStyle AppStyle { get { return SLWIOC.Get<IAppStyle>(); } }

        public FieldTypes FieldType
        {
            get
            {
                FieldTypes fieldType;
                if (Enum.TryParse<FieldTypes>(Field.FieldType, out fieldType))
                {
                    return fieldType;
                }
                else
                {
                    throw new Exception("Could not create field type from field type: " + Field.FieldType);
                }
            }
        }

        private bool _isDirty;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                OnPropertyChanged();
            }
        }

        public String OriginalValue { get; set; }

        protected Thickness RowMargin
        {
            get { return new Thickness(10, 5, 10, 8); }
        }
    }
}