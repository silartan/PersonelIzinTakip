using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using PersonelIzinTakip.Models;
using PersonelIzinTakip.Data; // Veritabaný context'i için
using System.Linq;

namespace PersonelIzinTakip.Controllers
{
    [Authorize] // Sadece giriþ yapanlar ana sayfayý görebilsin
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Yapýcý metot (Constructor) ile veritabaný ve kullanýcý yönetimini dahil ediyoruz
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Giriþ yapan kullanýcýnýn veritabaný kaydýný bulalým
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                // Kalan izin sayýsýný ve kullanýcýnýn bekleyen taleplerini gönderiyoruz
                ViewBag.KalanIzin = user.KalanIzinGun;
                ViewBag.BekleyenSayisi = _context.IzinTalepleri
                    .Count(x => x.UserId == user.Id && x.Durum == "Bekliyor");
            }

            // --- BURASI YENÝ: MÜDÜR ÝÇÝN EKSTRA ÝSTATÝSTÝKLER ---
            if (User.IsInRole("Mudur"))
            {
                // Þirketteki toplam personel sayýsý
                ViewBag.ToplamPersonel = _context.Users.Count();

                // Bugün tarihi itibariyle izni devam eden kiþi sayýsý
                var bugun = DateTime.Today;
                ViewBag.BugunIzinliOlanlar = _context.IzinTalepleri
                    .Count(x => x.Durum == "Onaylandý" &&
                                bugun >= x.BaslangicTarihi.Date &&
                                bugun <= x.BitisTarihi.Date);
                ViewBag.OnayBekleyenToplam = _context.IzinTalepleri.Count(x => x.Durum == "Bekliyor");
            }
            // ---------------------------------------------------

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
