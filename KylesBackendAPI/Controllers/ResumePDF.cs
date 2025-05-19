using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;

namespace KylesBackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")] // Enable CORS for this controller
    public class ResumePDF : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public ResumePDF(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetPdf(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("Filename cannot be empty");
            }

            // Sanitize the filename to prevent directory traversal attacks
            fileName = Path.GetFileName(fileName);

            // Add .pdf extension if not present
            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                fileName = $"{fileName}.pdf";
            }

            // Get the resume directory from configuration or use a default path
            string resumeDirectory = _configuration["ResumeDirectory"] ??
                Path.Combine(_environment.ContentRootPath, "Resumes");

            // Make sure the directory exists
            if (!Directory.Exists(resumeDirectory))
            {
                return NotFound("Resume directory not found");
            }

            string filePath = Path.Combine(resumeDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"Resume '{fileName}' not found");
            }

            // Read the file
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            // Return the file with appropriate content type
            return File(fileBytes, "application/pdf", fileName);
        }
    }
}