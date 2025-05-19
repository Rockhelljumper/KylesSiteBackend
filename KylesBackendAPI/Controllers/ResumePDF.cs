using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;

namespace KylesBackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]  // Enable CORS for this controller
    public class ResumePDF : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ResumePDF> _logger;

        public ResumePDF(IConfiguration configuration, IWebHostEnvironment environment, ILogger<ResumePDF> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetPdf(string fileName)
        {
            // Explicitly add CORS headers to this response
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
            Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
            Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");

            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogWarning("ResumePDF API call with empty filename");
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

            // Log detailed directory information for debugging Docker volume issues
            _logger.LogInformation("ResumePDF API - Content Root Path: {ContentRootPath}", _environment.ContentRootPath);
            _logger.LogInformation("ResumePDF API - Resume Directory Path: {ResumeDirectory}", resumeDirectory);

            string filePath = Path.Combine(resumeDirectory, fileName);

            // Log the file path for this API call
            _logger.LogInformation("ResumePDF API call - Requested file path: {FilePath}", filePath);

            // Check if the directory exists and log detailed information
            if (!Directory.Exists(resumeDirectory))
            {
                _logger.LogError("ResumePDF API call - Directory not found: {ResumeDirectory}", resumeDirectory);

                // Try to create the directory if it doesn't exist
                try
                {
                    Directory.CreateDirectory(resumeDirectory);
                    _logger.LogInformation("ResumePDF API call - Created directory: {ResumeDirectory}", resumeDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ResumePDF API call - Failed to create directory: {ResumeDirectory}", resumeDirectory);
                }

                // List parent directory contents for debugging
                try
                {
                    string parentDir = Path.GetDirectoryName(resumeDirectory);
                    if (Directory.Exists(parentDir))
                    {
                        var parentContents = Directory.GetFileSystemEntries(parentDir);
                        _logger.LogInformation("ResumePDF API call - Parent directory ({ParentDir}) contents: {Contents}",
                            parentDir, string.Join(", ", parentContents));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ResumePDF API call - Error listing parent directory");
                }

                return NotFound("Resume directory not found");
            }

            // Log directory contents for debugging
            try
            {
                var dirContents = Directory.GetFileSystemEntries(resumeDirectory);
                _logger.LogInformation("ResumePDF API call - Directory contents: {Contents}",
                    string.Join(", ", dirContents));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResumePDF API call - Error listing directory contents");
            }

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("ResumePDF API call - File not found: {FilePath}", filePath);
                return NotFound($"Resume '{fileName}' not found");
            }

            // Read the file
            try
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                _logger.LogInformation("ResumePDF API call - Successfully served file: {FileName}, Size: {FileSize} bytes",
                    fileName, fileBytes.Length);

                // Create a FileContentResult that will carry our CORS headers
                return new FileContentResult(fileBytes, "application/pdf")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResumePDF API call - Error reading file: {FilePath}", filePath);
                return StatusCode(500, "Error reading resume file");
            }
        }
    }
}