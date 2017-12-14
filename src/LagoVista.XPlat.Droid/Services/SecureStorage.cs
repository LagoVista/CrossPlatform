using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LagoVista.Client.Core.Auth;
using Java.Security;
using System.IO;
using Javax.Crypto;

namespace LagoVista.XPlat.Droid.Services
{
    public class SecureStorage : ISecureStorage
    {
        KeyStore _keyStore;
        KeyStore.PasswordProtection _passwordProtection;
        char[] _password;

        static readonly object _fileLocker = new object();

        const string FileName = "LagoVista.SecureStorage";

        public bool IsUnlocked { get; private set; }

        public bool Contains(string key)
        {
            if (!IsUnlocked)
            {
                throw new Exception("Please unlock the key store prior to using it.");
            }

            var aliases = _keyStore.Aliases();
            while (aliases.HasMoreElements)
            {
                var alias = aliases.NextElement().ToString();
                if (alias == key)
                {
                    return true;
                }
            }

            return false;
        }

        public void Delete(string key)
        {
            if (!IsUnlocked)
            {
                throw new Exception("Please unlock the key store prior to using it.");
            }

            _keyStore.DeleteEntry(key);
            Save();
        }

        public string Retrieve(string key)
        {
            if (!IsUnlocked)
            {
                throw new Exception("Please unlock the key store prior to using it.");
            }

            var aliases = _keyStore.Aliases();
            while (aliases.HasMoreElements)
            {
                var alias = aliases.NextElement().ToString();
                if (alias == key)
                {
                    var e = _keyStore.GetEntry(alias, _passwordProtection) as KeyStore.SecretKeyEntry;
                    if (e != null)
                    {
                        var bytes = e.SecretKey.GetEncoded();
                        return System.Text.Encoding.UTF8.GetString(bytes);
                    }
                }
            }

            return null;
        }

        private void Save()
        {
            lock (_fileLocker)
            {
                using (var s = Android.App.Application.Context.OpenFileOutput(FileName, FileCreationMode.Private))
                {
                    _keyStore.Store(s, _password);
                    s.Flush();
                    s.Close();
                }

                System.Diagnostics.Debug.WriteLine("saved " + FileName);
            }

        }

        public void Store(string key, string value)
        {
            if (!IsUnlocked)
            {
                throw new Exception("Please unlock the key store prior to using it.");
            }

            if (String.IsNullOrEmpty(value))
            {
                _keyStore.DeleteEntry(key);
            }
            else
            {
                var secretKey = new SecretValue(value);
                Java.Security.KeyStore.SecretKeyEntry entry = new KeyStore.SecretKeyEntry(secretKey);
                _keyStore.SetEntry(key, entry, _passwordProtection);

                Save();
            }
        }

        static IntPtr id_load_Ljava_io_InputStream_arrayC;

        private bool FileExists(Context context, String filename)
        {
            var file = context.GetFileStreamPath(filename);
            if (file == null || !file.Exists())
            {
                return false;
            }

            return true;
        }

        void LoadEmptyKeyStore(char[] password)
        {
            if (id_load_Ljava_io_InputStream_arrayC == IntPtr.Zero)
            {
                id_load_Ljava_io_InputStream_arrayC = JNIEnv.GetMethodID(_keyStore.Class.Handle, "load", "(Ljava/io/InputStream;[C)V");
            }
            IntPtr intPtr = IntPtr.Zero;
            IntPtr intPtr2 = JNIEnv.NewArray(password);
            JNIEnv.CallVoidMethod(_keyStore.Handle, id_load_Ljava_io_InputStream_arrayC, new JValue[]
            {
                new JValue (intPtr),
                new JValue (intPtr2)
            });
            JNIEnv.DeleteLocalRef(intPtr);
            if (password != null)
            {
                JNIEnv.CopyArray(intPtr2, password);
                JNIEnv.DeleteLocalRef(intPtr2);
            }
        }

        public void Reset(String newPassword)
        {
            var ctx = Android.App.Application.Context;
            var file = ctx.GetFileStreamPath(FileName);
            if (file != null && file.Exists())
            {
                Android.App.Application.Context.DeleteFile(FileName);
            }

            UnlockSecureStorage(newPassword);
        }

        public bool UnlockSecureStorage(string password)
        {
            _password = password.ToCharArray();
            _passwordProtection = new KeyStore.PasswordProtection(_password);

            var ctx = Android.App.Application.Context;

            _keyStore = KeyStore.GetInstance(KeyStore.DefaultType);

            try
            {
                lock (_fileLocker)
                {
                    if (!this.FileExists(ctx, FileName))
                    {
                        LoadEmptyKeyStore(password.ToCharArray());
                        System.Diagnostics.Debug.WriteLine("created " + FileName);
                    }
                    else
                    {
                        using (var s = ctx.OpenFileInput(FileName))
                        {
                            _keyStore.Load(s, password.ToCharArray());
                            System.Diagnostics.Debug.WriteLine("loaded " + FileName);
                        }
                    }

                    IsUnlocked = true;
                }
            }
            catch (FileNotFoundException)
            {
                LoadEmptyKeyStore(password.ToCharArray());
                IsUnlocked = true;
                Save();
            }
            catch (Java.IO.IOException)
            {
                IsUnlocked = false;

            }

            return IsUnlocked;
        }        

        class SecretValue : Java.Lang.Object, ISecretKey
        {
            byte[] _encoded;
            public SecretValue(string value)
            {
                _encoded = System.Text.ASCIIEncoding.ASCII.GetBytes(value);
            }

            public string Algorithm => "RAW";

            public string Format => "RAW";

            public byte[] GetEncoded()
            {
                return _encoded;
            }
        }
    }
}