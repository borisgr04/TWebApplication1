using ConsoleApp1Firma;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TWebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly DocumentService documentService;

        public ValuesController(DocumentService documentService)
        {
            this.documentService = documentService;
        }

        //[HttpGet]
        //public IActionResult Get()
        //{
        //    var pdfFile = documentService.GeneratePdfFromString();
        //    return File(pdfFile, "application/octet-stream", "SimplePdf.pdf");
        //}

        [HttpGet]
        public IActionResult GetPdf()
        {
            var sinFirma = @"OpenSsl/Example/EjemploPdf.pdf";
            var sinFirmaByte = System.IO.File.ReadAllBytes(sinFirma);
            var sign = new SignPdf();
            var firmado = sign.ReviewerSign(sinFirmaByte, "ALEXANDRO PONGUTÁ XXX", "101010", "318-8780598", "alexandro@puerto.com.co");

            var firmado2 = sign.ApproverSign(firmado, "OSCAR M. VERA PEÑA", "101010", "318-9897878", "oscar@puerto.com.co");
            
            return File(firmado2, "application/pdf", "SimplePdf.pdf"); ;
        }
    }
}
