﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NuvIoT.HardwareClient {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class DefaultConfig : global::System.Configuration.ApplicationSettingsBase {
        
        private static DefaultConfig defaultInstance = ((DefaultConfig)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new DefaultConfig())));
        
        public static DefaultConfig Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-select-")]
        public string DefaultPortName {
            get {
                return ((string)(this["DefaultPortName"]));
            }
            set {
                this["DefaultPortName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-select-")]
        public string DefaultBaudRate {
            get {
                return ((string)(this["DefaultBaudRate"]));
            }
            set {
                this["DefaultBaudRate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DefaultSendCR {
            get {
                return ((bool)(this["DefaultSendCR"]));
            }
            set {
                this["DefaultSendCR"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DefaultSendLF {
            get {
                return ((bool)(this["DefaultSendLF"]));
            }
            set {
                this["DefaultSendLF"] = value;
            }
        }
    }
}