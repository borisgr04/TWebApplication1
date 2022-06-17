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

        [HttpGet]
        public IActionResult Get()
        {
            var pdfFile = documentService.GeneratePdfFromString();
            return File(pdfFile, "application/octet-stream", "SimplePdf.pdf");
        }
    }
}
