using LagoVista.Core.Models.UIMetaData;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.FormControls
{
    public class LinkButton : FormControl
    {
        FormFieldHeader _header;
        Label _linkLabel;

        public LinkButton(FormViewer formViewer, FormField field) : base(formViewer, field)
        {
            _header = new FormFieldHeader(field.Label);

            _linkLabel = new Label()
            {
                TextColor = Color.Blue
            };
            Children.Add(_header);

            Children.Add(_header);
            Children.Add(_linkLabel);

            var tapRecognizer = new TapGestureRecognizer();
            tapRecognizer.Tapped += TapRecognizer_Tapped;
            _linkLabel.GestureRecognizers.Add(tapRecognizer);

            Margin = RowMargin;

            Refresh();
        }

        public override void Refresh()
        {
            if (string.IsNullOrEmpty(Field.Value))
            {
                _linkLabel.Text = Field.Watermark;
            }
            else
            {
                _linkLabel.Text = Field.Display;
            }
        }

        private void TapRecognizer_Tapped(object sender, System.EventArgs e)
        {
            if(Field.Command != null && Field.Command.CanExecute(null))
            {
                Field.Command.Execute(null);
            }
        }

        public override bool Validate()
        {
            return !(String.IsNullOrEmpty(Field.Value)) || !Field.IsRequired;
        }


    }
}
