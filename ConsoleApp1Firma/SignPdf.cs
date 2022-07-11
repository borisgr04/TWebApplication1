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
        Reviewer,
        Approver
    }

    public record SignPdfVerifyResponse (string SignatureFieldName, bool SignatureCoversWholeDocument, int DocumentRevision, int TotalRevisions, bool IntegrityAndAuthenticity);

  
    public class SignPdf 
    {
        #region Verify
        public List<SignPdfVerifyResponse> Verify(byte[] document) 
        {
            var response= new List<SignPdfVerifyResponse>();
            var pdfDoc = new PdfDocument(new PdfReader(new MemoryStream(document)));
            var signUtil = new SignatureUtil(pdfDoc);
            var names = signUtil.GetSignatureNames();
            
            foreach (string name in names)
            {
               var result= VerifySignature(signUtil, name);
               response.Add(result);
            }

            pdfDoc.Close();
            return response;
        }

        private SignPdfVerifyResponse VerifySignature(SignatureUtil signUtil, string name)
        {
            PdfPKCS7 pkcs7 = signUtil.ReadSignatureData(name);
            return new SignPdfVerifyResponse(name,signUtil.SignatureCoversWholeDocument(name), signUtil.GetRevision(name), signUtil.GetTotalRevisions(), pkcs7.VerifySignatureIntegrityAndAuthenticity());
        }
        #endregion Verify

        #region Sign
        public byte[] ApproverSign(byte[] documento, string name, string code, string telefono, string email) 
        {
            var reason = $"Revisado por: \n {name.ToUpper()} \n Revisión electrónica: {code} \n Fecha Revisión: {DateTime.Now}";
            var configSign = new SignConfiguration(nameof(SignEnum.Approver), "Revisión electrónica:", new SignRectangule(100, 500, 250, 100));
            return Sign(documento, SignEnum.Approver, reason, $"Cel - {telefono} - email: {email}", configSign);
        }

        public byte[] ReviewerSign(byte[] documento, string name, string code, string telefono, string email)
        {
            var razon = $"Aprobado por: \n {name.ToUpper()} \n Aprobación electrónica: {code} \n Fecha Aprobación: {DateTime.Now}";
            var configSign = new SignConfiguration(nameof(SignEnum.Reviewer), "Aprobación electrónica:", new SignRectangule(320, 500, 250, 100));
            return Sign(documento, SignEnum.Reviewer, razon, $"Cel - {telefono} -email: {email}", configSign);
        }
            
        private static byte[] Sign(byte[] documento, SignEnum type, string reason, string contact, SignConfiguration signConfiguration) 
        {
            var kv = new CertificadoSslService();
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
            var firmado = Sign(documento, chain, pk, DigestAlgorithms.SHA384, PdfSigner.CryptoStandard.CMS, reason,  contact, signConfiguration);
            return firmado;
        }

        private static byte[] Sign(byte[] document,  X509Certificate[] chain, ICipherParameters pk,
         string digestAlgorithm, PdfSigner.CryptoStandard subfilter, string reason, string contact,   SignConfiguration signConfiguration)
        {
            var dest = new MemoryStream();
            var reader = new PdfReader(new MemoryStream(document));
            var signer = new PdfSigner(reader, dest, new StampingProperties().UseAppendMode());
            var rect = new Rectangle(signConfiguration.Rectangulo.X, signConfiguration.Rectangulo.Y, signConfiguration.Rectangulo.Width, signConfiguration.Rectangulo.Height);
            
            var appearance = signer.GetSignatureAppearance();
            appearance
                .SetReasonCaption(signConfiguration.Nombre)
                .SetReason(reason)
                .SetSignatureCreator("Plataforma Certificados")
                .SetLayer2FontSize(9).SetLayer2Font(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetContact(contact)
                .SetLayer2Text(reason)
                // Specify if the appearance before field is signed will be used
                // as a background for the signed field. The "false" value is the default value.
                .SetReuseAppearance(false)
                .SetPageRect(rect)
                .SetPageNumber(1);

            signer.SetFieldName(signConfiguration.SignFieldName);

            IExternalSignature pks = new global::iText.Signatures.PrivateKeySignature(pk, digestAlgorithm);

            // Sign the document using the detached mode, CMS or CAdES equivalent.
            signer.SignDetached(pks, chain, null, null, null, 0, subfilter);
            return dest.ToArray();
        }
        #endregion Sign

        public record SignConfiguration 
        (
            string SignFieldName,
            string Nombre,
            SignRectangule Rectangulo
        );

        public record SignRectangule (float X, float Y, float Width, float Height);
    }

}
