using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace PersonelIzinTakip.Controllers
{
    // İŞTE SİHİRLİ SATIR: Bu sayfaya giriş yapmadan da erişilebilsin diyoruz!
    [AllowAnonymous]
    public class TestMailController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TestMailController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Mailler.txt");
            string icerik = "";

            if (System.IO.File.Exists(filePath))
            {
                icerik = System.IO.File.ReadAllText(filePath);
            }
            else
            {
                icerik = "<div class='alert alert-warning'>Henüz düşen bir şifre sıfırlama maili bulunamadı. Lütfen 'Şifremi Unuttum' butonunu kullanın.</div>";
            }

            ViewBag.Mailler = icerik;
            return View();
        }
    }
}
