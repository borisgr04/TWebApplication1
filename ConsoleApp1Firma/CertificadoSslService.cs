using System;
using Azure.Identity;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Azure.Security.KeyVault.Certificates;
//using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp1Firma
{
    internal class CertificadoSslService
    {
        private string _clientId;
        private string _clientSecret;
        private string _tenantId;
        private string _keyVaultUrl ;
        private string _certificateName;
        public CertificadoSslService()
        {
            _clientId = "fea0de43-0469-4577-a50e-c02d8f282442";
            _clientSecret = "KeQ8Q~ZHka_TxGhfYhqseXx41RpSXYikczYYHaDq";
            _tenantId = "83a8a1a0-dfd1-43e6-976d-73889e9bc230";
            _keyVaultUrl = "https://testbya.vault.azure.net/";
            _certificateName = "ByaPkcs12";
        }
     
        public byte[] GetCer()
        {
            var clientSecret = new ClientSecretCredential(_tenantId, _clientId, _clientSecret);
            var client = new CertificateClient(vaultUri: new Uri(_keyVaultUrl), credential: clientSecret);
            KeyVaultCertificateWithPolicy certificateWithPolicy = client.GetCertificate(_certificateName);
            return certificateWithPolicy.Cer;
        }
        public async Task<byte[]> GetPkcs12()
        {
            ClientSecretCredential clientCredential = new ClientSecretCredential(_tenantId, _clientId, _clientSecret);
            var secretClient = new SecretClient(new Uri(_keyVaultUrl), clientCredential);
            var response = await secretClient.GetSecretAsync(_certificateName);
            var keyVaultSecret = response?.Value;
            if (keyVaultSecret != null)
            {
                var privateKeyBytes = Convert.FromBase64String(keyVaultSecret.Value);
                return privateKeyBytes;
            }
            return null;
        }
    }
    
}
