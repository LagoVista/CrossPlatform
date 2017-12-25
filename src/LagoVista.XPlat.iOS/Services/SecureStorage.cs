using System;
using System.Runtime.CompilerServices;
using Foundation;
using Security;
using LagoVista.Client.Core.Auth;

namespace LagoVista.XPlat.iOS.Services
{
    /// <summary>
    /// Implements <see cref="ISecureStorage"/> for iOS using <see cref="SecKeyChain"/>.
    /// </summary>
    public class SecureStorage : ISecureStorage
    {
        public bool IsUnlocked => true;

        /// <summary>
        /// Stores data.
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <param name="dataBytes">Data bytes to store.</param>
        public void Store(string key, byte[] dataBytes)
        {
            using (var data = NSData.FromArray(dataBytes))
            using (var newRecord = GetKeyRecord(key, data))
            {
                Delete(key);
                CheckError(SecKeyChain.Add(newRecord));
            }
        }

        /// <summary>
        /// Retrieves stored data.
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <returns>Byte array of stored data.</returns>
        public byte[] RetrieveBuffer(string key)
        {
            SecStatusCode resultCode;

            using (var existingRecord = GetKeyRecord(key))
            using (var record = SecKeyChain.QueryAsRecord(existingRecord, out resultCode))
            {
                CheckError(resultCode);
                return record.Generic.ToArray();
            }
        }

        /// <summary>
        /// Deletes data.
        /// </summary>
        /// <param name="key">Key for the data to be deleted.</param>
        public void Delete(string key)
        {
            using (var record = GetExistingRecord(key))
            {
                if (record != null) CheckError(SecKeyChain.Remove(record));
            }
        }

        /// <summary>
        /// Checks if the storage contains a key.
        /// </summary>
        /// <param name="key">The key to search.</param>
        /// <returns>True if the storage has the key, otherwise false.</returns>
        public bool Contains(string key)
        {
            using (var existingRecord = GetExistingRecord(key))
            {
                return existingRecord != null;
            }
        }

        private static void CheckError(SecStatusCode resultCode, [CallerMemberName] string caller = null)
        {
            if (resultCode != SecStatusCode.Success)
            {
                throw new Exception(string.Format("Failed to execute {0}. Result code: {1}", caller, resultCode));
            }
        }

        private static SecRecord GetKeyRecord(string key, NSData data = null)
        {
            var record = new SecRecord(SecKind.GenericPassword)
            {
                Service = NSBundle.MainBundle.BundleIdentifier,
                Account = key
            };

            if (data != null) record.Generic = data;

            return record;
        }

        private static SecRecord GetExistingRecord(string key)
        {
            var existingRecord = GetKeyRecord(key);

            SecStatusCode resultCode;
            SecKeyChain.QueryAsRecord(existingRecord, out resultCode);

            return resultCode == SecStatusCode.Success ? existingRecord : null;
        }

        public void Store(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                var buffer = System.Text.UTF8Encoding.UTF8.GetBytes(value);
                Store(key, buffer);
            }
            else
            {
                Delete(key);
            }
        }
        

        public string Retrieve(string key)
        {
            var buffer = RetrieveBuffer(key);
            if (buffer != null && buffer.Length > 0)
            {
                return System.Text.UTF8Encoding.UTF8.GetString(buffer);
            }

            return null;
        }

        public bool UnlockSecureStorage(string password)
        {
            return true;
        }

        public void Reset(string newPassword)
        {
            
        }
    }
}