﻿using LagoVista.Core.Interfaces;
using LagoVista.XPlat.Core.Resources;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;

namespace LagoVista.XPlat.Sample.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            var app = new LagoVista.XPlat.Sample.App();

            var keys = typeof(IMobileStyle).GetProperties().Select(prop => prop.Name).ToList();
            keys.AddRange(typeof(IAppStyle).GetProperties().Select(prop => prop.Name));

            foreach (var key in keys)
            {
                if (app.Resources.TryGetValue(key, out object value))
                {
                    switch (value.GetType().Name)
                    {
                        case nameof(Xamarin.Forms.Color):
                            {
                                var sourceColor = (Xamarin.Forms.Color)value;
                                if (Application.Current.Resources.ContainsKey(key))
                                {
                                    Debug.WriteLine("Setting: " + key);
                                    Application.Current.Resources[key] = sourceColor.ToUWPColor();
                                }
                                else
                                {
                                    Debug.WriteLine("Adding: " + key);
                                    Application.Current.Resources.Add(key, sourceColor.ToUWPColor());
                                }
                            }
                            break;
                        case nameof(System.String):
                            {
                                var strValue = (string)value;
                                if (Application.Current.Resources.ContainsKey(key))
                                {
                                    Debug.WriteLine("Setting: " + key);
                                    Application.Current.Resources[key] = strValue;
                                }
                                else
                                {
                                    Debug.WriteLine("Adding: " + key);
                                    Application.Current.Resources.Add(key, strValue);
                                }
                            }
                            break;
                        case nameof(System.Double):
                            {
                                var doubleValue = (double)value;
                                if (Application.Current.Resources.ContainsKey(key))
                                {
                                    Debug.WriteLine("Setting: " + key);
                                    Application.Current.Resources[key] = doubleValue;
                                }
                                else
                                {
                                    Debug.WriteLine("Adding: " + key);
                                    Application.Current.Resources.Add(key, doubleValue);
                                }
                            }
                            break;
                    }
                }
            }

            /*
            var buttonDictionary = new ResourceDictionary();
            buttonDictionary.Source = new Uri("ms-appx:///Resources/ButtonStyle.xaml");
            Application.Current.Resources.MergedDictionaries.Add(buttonDictionary);

            var editTextStyle = new ResourceDictionary();
            editTextStyle.Source = new Uri("ms-appx:///Resources/EditTextStyle.xaml");
            Application.Current.Resources.MergedDictionaries.Add(editTextStyle);

            var comboBoxTextStyle = new ResourceDictionary();
            comboBoxTextStyle.Source = new Uri("ms-appx:///Resources/ComboBoxStyle.xaml");
            Application.Current.Resources.MergedDictionaries.Add(comboBoxTextStyle);

            var iconButtonStyle = new ResourceDictionary();
            iconButtonStyle.Source = new Uri("ms-appx:///Resources/IconButtonStyle.xaml");
            Application.Current.Resources.MergedDictionaries.Add(iconButtonStyle);
            */

            this.InitializeComponent();

            LoadApplication(app);
        }
    }
}
