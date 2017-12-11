using LagoVista.Client.Core.Auth;
using System.Linq;

namespace LagoVista.Core.UWP.Services
{
    public class SecureStorage : ISecureStorage
    {
        public bool Contains(string key)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            var resources = vault.FindAllByResource(key);
            return resources.Any();            
        }

        public void Delete(string key)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            
            var resources = vault.FindAllByResource(key);
            if(resources.Any())
            {
                vault.Remove(resources.First());
            }
        }
       
        public string Retrieve(string key)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            var resources = vault.FindAllByResource(key);
            if (resources.Any())
            {
                var resource = resources.First();
                resource.RetrievePassword();
                return resource.Password;
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
    }
}
