using System;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System.IO;
using Org.BouncyCastle.X509;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.Collections.Generic;

namespace ConsoleApp1Firma
{
    public enum SignEnum 
    {
        Revisor,
        Aprobador
    }
    public class SignPdf 
    {
        public void Verify(byte[] document) 
        {

            PdfDocument pdfDoc = new PdfDocument(new PdfReader(new MemoryStream(document)));
            SignatureUtil signUtil = new SignatureUtil(pdfDoc);
            IList<string> names = signUtil.GetSignatureNames();
            
            foreach (string name in names)
            {
                Console.Out.WriteLine("===== " + name + " =====");
                VerifySignature(signUtil, name);
            }

            pdfDoc.Close();
        }

        public PdfPKCS7 VerifySignature(SignatureUtil signUtil, String name)
        {
            PdfPKCS7 pkcs7 = signUtil.ReadSignatureData(name);

            Console.Out.WriteLine("Signature covers whole document: " + signUtil.SignatureCoversWholeDocument(name));
            Console.Out.WriteLine("Document revision: " + signUtil.GetRevision(name) + " of "
                                  + signUtil.GetTotalRevisions());
            Console.Out.WriteLine("Integrity check OK? " + pkcs7.VerifySignatureIntegrityAndAuthenticity());
            
            return pkcs7;
        }

        //
        public byte[] Sign(byte[] documento, SignEnum type, string name, string code) 
        {
            var kv = new KvCertificate();
            var pkc12 = kv.GetPkcs12().Result;
            var pk12 = new Pkcs12Store(new MemoryStream(pkc12), "".ToCharArray());
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
            byte[] firmado=null;

            if (type.Equals(SignEnum.Revisor)) 
            {
                firmado = Sign(documento, chain, pk, DigestAlgorithms.SHA256, PdfSigner.CryptoStandard.CMS, name, code, "revisor");
            }
            if (type.Equals(SignEnum.Aprobador))
            {
                firmado = Sign(documento, chain, pk, DigestAlgorithms.SHA256, PdfSigner.CryptoStandard.CMS, name, code, "aprobador", 220);
            }

            return firmado;
        }

        public byte[] Sign(byte[] document,  X509Certificate[] chain, ICipherParameters pk,
         string digestAlgorithm, PdfSigner.CryptoStandard subfilter, string reason, string location, string firma , int posx = 0)
        {
            var dest = new MemoryStream();
            PdfReader reader = new PdfReader(new MemoryStream(document));
            PdfSigner signer = new PdfSigner(reader, dest, new StampingProperties().UseAppendMode());
            
            // Create the signature appearance
            Rectangle rect = new Rectangle(100 + posx, 500, 200, 100);
            
            PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
            appearance
                .SetLocationCaption("Código:")
                .SetLocation(location)
                .SetReasonCaption("Revisor:")
                .SetReason(reason)
                .SetSignatureCreator("Plataforma Certificados")
                //.SetLayer2Text("Boris Gonzalez \n Codigo 10101 \n Fecha 12222")
                .SetLayer2FontSize(9).SetLayer2Font(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetContact("Nombre")

                // Specify if the appearance before field is signed will be used
                // as a background for the signed field. The "false" value is the default value.
                .SetReuseAppearance(false)
                .SetPageRect(rect)
                .SetPageNumber(1);

            signer.SetFieldName(firma);
            

            IExternalSignature pks = new global::iText.Signatures.PrivateKeySignature(pk, digestAlgorithm);

            // Sign the document using the detached mode, CMS or CAdES equivalent.
            signer.SignDetached(pks, chain, null, null, null, 0, subfilter);
            return dest.ToArray();
        }

    }

}
