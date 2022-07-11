using ConsoleApp1Firma;

namespace SignTest
{
    public class Tests
    {
        public static readonly string DEST = @"C:\PoC\TWebApplication1\ConsoleApp1Firma\OpenSsl\Example\dest\";
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var sinFirma = @"C:\PoC\TWebApplication1\ConsoleApp1Firma\OpenSsl\Example\EjemploPdf.pdf";
            var sinFirmaByte = File.ReadAllBytes(sinFirma);
            var sign = new SignPdf();
            var firmado=sign.ReviewerSign(sinFirmaByte, "ALEXANDRO PONGUTÁ XXX", "101010","318-8780598","alexandro@puerto.com.co");
            File.WriteAllBytes(DEST + "FirmadoConUnaFirma.pdf", firmado);
            var firmado2 = sign.ApproverSign(firmado, "OSCAR M. VERA PEÑA", "101010", "318-9897878", "oscar@puerto.com.co");
            File.WriteAllBytes(DEST + "FirmadoConDosFirmas.pdf", firmado2);
            var response=sign.Verify(firmado2);

            foreach (var item in response)
            {
                TestContext.WriteLine("Signature Name: " + item.SignatureFieldName);
                TestContext.WriteLine("Signature covers whole document: " + item.SignatureCoversWholeDocument);
                TestContext.WriteLine("Document revision: " + item.DocumentRevision + " of " + item.TotalRevisions);
                TestContext.WriteLine("Integrity check OK? " + item.IntegrityAndAuthenticity);
                Assert.True(item.IntegrityAndAuthenticity);
            }

            TestContext.WriteLine(nameof(SignEnum.Reviewer));

            Assert.IsNotNull(firmado2);
        }
    }
}