using System;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System.IO;
using System;
using Org.BouncyCastle.X509;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;


namespace ConsoleApp1Firma
{
    internal class Program
    {
        public static readonly string DEST = @"D:\OpenSsl\Example\dest\";

        public static readonly string KEYSTORE = @"D:\OpenSsl\Example\cert_key.p12";
        public static readonly string SRC = @"D:\OpenSsl\Example\EjemploPdf.pdf";

        public static readonly char[] PASSWORD = "ecp-ig".ToCharArray();

        public static readonly string[] RESULT_FILES =
        {
            "hello_signed5.pdf",
            "hello_signed6.pdf",
            "hello_signed7.pdf",
            "hello_signed8.pdf"
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

        public static void Main(string[] args)
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
     
    }
}
