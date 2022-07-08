using System;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System.IO;
using Org.BouncyCastle.X509;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using Azure.Security.KeyVault.Certificates;
using Azure.Identity;

namespace ConsoleApp1Firma
{

    /*
    internal class Program 
    {
        public static async Task Main(string[] args)
        {
            var clientId = "fea0de43-0469-4577-a50e-c02d8f282442";
            var clientSecret = "KeQ8Q~ZHka_TxGhfYhqseXx41RpSXYikczYYHaDq";
            var tenantId = "83a8a1a0-dfd1-43e6-976d-73889e9bc230";
            var kvUrl = "https://testbya.vault.azure.net/";
            var certificateName = "ByaPkcs12";
            await GetCertificate(certificateName, clientId, clientSecret, kvUrl, tenantId);
        }
        public async static Task<X509Certificate2> GetCertificate(string certificateName, string clientId, string clientSecret, string keyVaultAddress, string tenantId)
        {
            ClientSecretCredential clientCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var secretClient = new SecretClient(new Uri(keyVaultAddress), clientCredential);
            var response = await secretClient.GetSecretAsync(certificateName);
            var keyVaultSecret = response?.Value;
            if (keyVaultSecret != null)
            {
                var privateKeyBytes = Convert.FromBase64String(keyVaultSecret.Value);
                //Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[ce.Length];

                Pkcs12Store pk12 = new Pkcs12Store(new MemoryStream(privateKeyBytes), "".ToCharArray());

                string alias = null;
                foreach (var a in pk12.Aliases)
                {
                    alias = ((string)a);
                    if (pk12.IsKeyEntry(alias))
                        break;
                }

                ICipherParameters pk = pk12.GetKey(alias).Key;
                X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
                Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[ce.Length];
                for (int k = 0; k < ce.Length; ++k)
                {
                    chain[k] = ce[k].Certificate;
                }

                return new X509Certificate2(privateKeyBytes);
            }
            return null;
        }
    }
    */


    internal class Program
    {
        public static readonly string DEST = @"C:\PoC\TWebApplication1\ConsoleApp1Firma\OpenSsl\Example\dest\";

        public static readonly string KEYSTORE = @"C:\PoC\TWebApplication1\ConsoleApp1Firma\OpenSsl\Example\cert_key.p12";
        public static readonly string SRC = @"C:\PoC\TWebApplication1\ConsoleApp1Firma\OpenSsl\Example\EjemploPdf.pdf";

        public static readonly char[] PASSWORD = "ecp-ig".ToCharArray();
        //ppDomain.CurrentDomain.BaseDirectory
        public static readonly string[] RESULT_FILES =
        {
            "hello_signed51.pdf",
            "hello_signed61.pdf",
            "hello_signed71.pdf",
            "hello_signed81.pdf"
        };

        public void Sign(string src, string dest, X509Certificate[] chain, ICipherParameters pk,
            string digestAlgorithm, PdfSigner.CryptoStandard subfilter, String reason, String location)
        {
            PdfReader reader = new PdfReader(src);
            PdfSigner signer = new PdfSigner(reader, new FileStream(dest, FileMode.Create), new StampingProperties());

            // Create the signature appearance
            Rectangle rect = new Rectangle(36, 500, 200, 100);
            PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
            appearance
                .SetReason(reason)
                .SetLocation(location)

                // Specify if the appearance before field is signed will be used
                // as a background for the signed field. The "false" value is the default value.
                .SetReuseAppearance(false)
                .SetPageRect(rect)
                .SetPageNumber(1);
            signer.SetFieldName("sig");

            IExternalSignature pks = new iText.Signatures.PrivateKeySignature(pk, digestAlgorithm);

            // Sign the document using the detached mode, CMS or CAdES equivalent.
            signer.SignDetached(pks, chain, null, null, null, 0, subfilter);
        }

       

        private static void Sign()
        {
            DirectoryInfo directory = new DirectoryInfo(DEST);
            directory.Create();

            Pkcs12Store pk12 = new Pkcs12Store(new FileStream(KEYSTORE, FileMode.Open, FileAccess.Read), PASSWORD);
            string alias = null;
            foreach (var a in pk12.Aliases)
            {
                alias = ((string)a);
                if (pk12.IsKeyEntry(alias))
                    break;
            }

            ICipherParameters pk = pk12.GetKey(alias).Key;
            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
            X509Certificate[] chain = new X509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
            {
                chain[k] = ce[k].Certificate;
            }

            Program app = new Program();

            app.Sign(SRC, DEST + RESULT_FILES[0], chain, pk, DigestAlgorithms.SHA256,
                PdfSigner.CryptoStandard.CMS, "Test 1", "Ghent");
            app.Sign(SRC, DEST + RESULT_FILES[1], chain, pk, DigestAlgorithms.SHA512,
                PdfSigner.CryptoStandard.CMS, "Test 2", "Ghent");
            app.Sign(SRC, DEST + RESULT_FILES[2], chain, pk, DigestAlgorithms.SHA256,
                PdfSigner.CryptoStandard.CADES, "Test 3", "Ghent");
            app.Sign(SRC, DEST + RESULT_FILES[3], chain, pk, DigestAlgorithms.RIPEMD160,
                PdfSigner.CryptoStandard.CADES, "Test 4", "Ghent");
        }


        public static void Main(string[] args)
        {
            //Sign();
            SignKv();
            //KeyVault();
        }
        private static byte[] KeyVault() 
        {
            var clientSecret = new ClientSecretCredential("83a8a1a0-dfd1-43e6-976d-73889e9bc230", "fea0de43-0469-4577-a50e-c02d8f282442", "KeQ8Q~ZHka_TxGhfYhqseXx41RpSXYikczYYHaDq");
            var client = new CertificateClient(vaultUri: new Uri("https://testbya.vault.azure.net/"), credential: clientSecret);
            KeyVaultCertificateWithPolicy certificateWithPolicy = client.GetCertificate("ByACert");
            //client.GetCertificate()
            return certificateWithPolicy.Cer;
        }
        private static void SignKv()
        {
            DirectoryInfo directory = new DirectoryInfo(DEST);
            directory.Create();

            //string KEYSTORE = @"C:\PoC\TWebApplication1\ConsoleApp1Firma\OpenSsl\Example\testbya-ByaPkcs12-20220707.pfx";
            //char[] PASSWORD = "".ToCharArray(); 
            var kv = new KvCertificate();
            var pkc12= kv.GetPkcs12().Result;
            Pkcs12Store pk12 = new Pkcs12Store(new MemoryStream(pkc12),"".ToCharArray());
            string alias = null;
            foreach (var a in pk12.Aliases)
            {
                alias = ((string)a);
                if (pk12.IsKeyEntry(alias))
                    break;
            }

            ICipherParameters pk = pk12.GetKey(alias).Key;
            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);


            Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
            {
                chain[k] = ce[k].Certificate;
            }

            Program app = new Program();

            app.Sign(SRC, DEST + RESULT_FILES[0], chain, pk, DigestAlgorithms.SHA256,
                PdfSigner.CryptoStandard.CMS, "Test 1", "Ghent");
            app.Sign(SRC, DEST + RESULT_FILES[1], chain, pk, DigestAlgorithms.SHA512,
                PdfSigner.CryptoStandard.CMS, "Test 2", "Ghent");
            app.Sign(SRC, DEST + RESULT_FILES[2], chain, pk, DigestAlgorithms.SHA256,
                PdfSigner.CryptoStandard.CADES, "Test 3", "Ghent");
            app.Sign(SRC, DEST + RESULT_FILES[3], chain, pk, DigestAlgorithms.RIPEMD160,
                PdfSigner.CryptoStandard.CADES, "Test 4", "Ghent");
        }

     

        //
        //

        //$Env:AZURE_CLIENT_ID="fea0de43-0469-4577-a50e-c02d8f282442"
        //$Env:AZURE_CLIENT_SECRET="KeQ8Q~ZHka_TxGhfYhqseXx41RpSXYikczYYHaDq"
        //$Env:AZURE_TENANT_ID="83a8a1a0-dfd1-43e6-976d-73889e9bc230"


        //$Env:AZURE_CLIENT_ID="45c27a2a-ac34-4302-b3b2-e54fdd3f8641"
        //$Env:AZURE_CLIENT_SECRET="BUd8Q~idwsJ3ABfXLkR.RSTIpga4L4mAJR2vhdsZ"
        //$Env:AZURE_TENANT_ID="a4305987-cf78-4f93-9d64-bf18af65397b"
    }
    
}
