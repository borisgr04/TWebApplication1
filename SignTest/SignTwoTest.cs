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
            var firmado=sign.Sign(sinFirmaByte,SignEnum.Revisor,"Teresa","101010");
            File.WriteAllBytes(DEST + "FirmadoConUnaFirma.pdf", firmado);
            var firmado2 = sign.Sign(firmado, SignEnum.Aprobador, "Oscar", "101010");
            File.WriteAllBytes(DEST + "FirmadoConDosFirmas.pdf", firmado2);
            sign.Verify(firmado2);
            Assert.IsNotNull(firmado2);
        }
    }
}