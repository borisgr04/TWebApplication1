﻿using System;
using Azure.Identity;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Azure.Security.KeyVault.Certificates;
//using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp1Firma
{
    internal class KvCertificate
    {
        public async Task<byte[]> GetPkcs12()
        {
            var clientId = "fea0de43-0469-4577-a50e-c02d8f282442";
            var clientSecret = "KeQ8Q~ZHka_TxGhfYhqseXx41RpSXYikczYYHaDq";
            var tenantId = "83a8a1a0-dfd1-43e6-976d-73889e9bc230";
            var kvUrl = "https://testbya.vault.azure.net/";
            var certificateName = "ByaPkcs12";
            return await GetCertificate(certificateName, clientId, clientSecret, kvUrl, tenantId);
        }
        private async Task<byte[]> GetCertificate(string certificateName, string clientId, string clientSecret, string keyVaultAddress, string tenantId)
        {
            ClientSecretCredential clientCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var secretClient = new SecretClient(new Uri(keyVaultAddress), clientCredential);
            var response = await secretClient.GetSecretAsync(certificateName);
            var keyVaultSecret = response?.Value;
            if (keyVaultSecret != null)
            {
                var privateKeyBytes = Convert.FromBase64String(keyVaultSecret.Value);
                return privateKeyBytes;
            }
            return null;
        }
               
        public byte[] GetCer()
        {
            var clientSecret = new ClientSecretCredential("83a8a1a0-dfd1-43e6-976d-73889e9bc230", "fea0de43-0469-4577-a50e-c02d8f282442", "KeQ8Q~ZHka_TxGhfYhqseXx41RpSXYikczYYHaDq");
            var client = new CertificateClient(vaultUri: new Uri("https://testbya.vault.azure.net/"), credential: clientSecret);
            KeyVaultCertificateWithPolicy certificateWithPolicy = client.GetCertificate("ByACert");
            //client.GetCertificate()
            return certificateWithPolicy.Cer;
        }
    }
    
}
