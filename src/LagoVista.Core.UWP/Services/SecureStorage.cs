using LagoVista.Client.Core.Auth;
using System.Linq;

namespace LagoVista.Core.UWP.Services
{
    public class SecureStorage : ISecureStorage
    {
        public bool IsUnlocked =>  true;

        public bool Contains(string key)
        {
            try
            {
                var vault = new Windows.Security.Credentials.PasswordVault();
                var resources = vault.FindAllByResource(key);
                return resources.Any();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return false;
            }
        }

        public void Delete(string key)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            try
            {

                var resources = vault.FindAllByResource(key);
                if (resources.Any())
                {
                    vault.Remove(resources.First());
                }

            }
            catch (System.Runtime.InteropServices.COMException)
            {
                /* try to remove something that isn't there...not gonna throw an exception */
            }

        }

        public void Reset(string newPassword)
        {
            /* Not applicable for UWP, will reset the password and android */    
        }

        public string Retrieve(string key)
        {
            try
            {
                var vault = new Windows.Security.Credentials.PasswordVault();
                var resources = vault.FindAllByResource(key);
                if (resources.Any())
                {
                    var resource = resources.First();
                    resource.RetrievePassword();
                    return resource.Password;
                }
            }
            catch(System.Runtime.InteropServices.COMException)
            {
                return null;
            }

            return null;
        }

        public void Store(string key, string value)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();

            var pwdCred = new Windows.Security.Credentials.PasswordCredential()
            {
                Password = value,
                UserName = key,
                Resource = key,
            };


            vault.Add(pwdCred);
        }

        public bool UnlockSecureStorage(string password)
        {
            return true;
        }
    }
}
