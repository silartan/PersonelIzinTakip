using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace PersonelIzinTakip.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailSender(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            string wwwrootPath = _webHostEnvironment.WebRootPath;
            // Dosya adını .html yerine .txt yapıyoruz ki tarayıcı doğrudan güvenlik duvarına takılmasın, 
            // biz bunu içeride okuyup şıkça sunacağız.
            string filePath = Path.Combine(wwwrootPath, "Mailler.txt");

            StringBuilder mailLog = new StringBuilder();

            mailLog.AppendLine("<div class='card shadow-sm border-0 mb-4'>");
            mailLog.AppendLine("  <div class='card-header bg-dark text-white d-flex justify-content-between align-items-center'>");
            mailLog.AppendLine($"    <span class='fw-bold'>Kime: {email}</span>");
            mailLog.AppendLine($"    <span class='badge bg-secondary'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</span>");
            mailLog.AppendLine("  </div>");
            mailLog.AppendLine("  <div class='card-body bg-white'>");
            mailLog.AppendLine($"    <h5 class='card-title text-primary fw-bold'>Konu: {subject}</h5>");
            mailLog.AppendLine("    <hr />");
            mailLog.AppendLine($"    <div class='card-text'>{htmlMessage}</div>");
            mailLog.AppendLine("  </div>");
            mailLog.AppendLine("</div>");

            await File.AppendAllTextAsync(filePath, mailLog.ToString(), Encoding.UTF8);
        }
    }
}
