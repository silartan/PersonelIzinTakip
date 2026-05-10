using Microsoft.AspNetCore.Mvc;
using PersonelIzinTakip.Data;
using PersonelIzinTakip.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace PersonelIzinTakip.Controllers
{
    [Authorize]
    public class IzinController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IzinController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Şimdi buradaki kırmızı çizgi gidecek
        }
        // Adres: /Izin/Index
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var model = await _context.IzinTalepleri
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.BaslangicTarihi)
                .ToListAsync();

            return View(model);
        }

        // 2. İZİN TALEP FORMUNU AÇAN SAYFA
        // Adres: /Izin/TalepEt
        public IActionResult TalepEt()
        {
            return View();
        }
        // Formu gösterir


        // Form gönderildiğinde çalışır
        [HttpPost]
        [Authorize] // Sadece giriş yapanlar
        public async Task<IActionResult> TalepEt(IzinTalep model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. TARİH ÇAKIŞMA ALGORİTMASI (En teknik kısım)
            // Mantık: (YeniBaslangic <= MevcutBitis) VE (YeniBitis >= MevcutBaslangic)
            var cakismaVarMi = await _context.IzinTalepleri
                .AnyAsync(x => x.UserId == userId &&
                               x.Durum != "Reddedildi" &&
                               model.BaslangicTarihi <= x.BitisTarihi &&
                               model.BitisTarihi >= x.BaslangicTarihi);

            if (cakismaVarMi)
            {
                // Kullanıcıya uyarı mesajı gönderiyoruz
                ModelState.AddModelError("", "Hata: Seçtiğiniz tarihlerde zaten onaylı veya bekleyen bir izniniz bulunuyor!");
                return View(model);
            }
            // 1.5 YETERSIZ IZIN KREDISI KONTROLÜ
            var user = await _userManager.GetUserAsync(User);
            int talepEdilenGun = (model.BitisTarihi.Date - model.BaslangicTarihi.Date).Days + 1;

            if (user != null && talepEdilenGun > user.KalanIzinGun)
            {
                ModelState.AddModelError("", $"Hata: Yetersiz izin kredisi! Kalan izniniz ({user.KalanIzinGun} gün), talep ettiğiniz süreden ({talepEdilenGun} gün) daha az.");
                return View(model);
            }

            // 2. VERİLERİ DOLDURMA
            model.UserId = userId;
            model.Durum = "Bekliyor";
            model.GunSayisi = (model.BitisTarihi.Date - model.BaslangicTarihi.Date).Days + 1;

            try
            {
                _context.IzinTalepleri.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "İzin talebiniz başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var error = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content("Sistemsel Hata: " + error);
            }
        }
        // Müdürün onay bekleyen tüm izinleri göreceği sayfa
        [Authorize(Roles = "Mudur")]
        public async Task<IActionResult> OnayBekleyenler()
        {
            var bekleyenler = await _context.IzinTalepleri
                .Include(x => x.User) // Personel adını görebilmek için
                .Where(x => x.Durum == "Bekliyor")
                .ToListAsync();

            return View(bekleyenler);
        }

        // İzni Onaylama İşlemi
        [HttpPost]
        [Authorize(Roles = "Mudur")]
        public async Task<IActionResult> Onayla(int id)
        {
            // 1. İzin talebini bul
            var talep = await _context.IzinTalepleri.FindAsync(id);

            if (talep != null && talep.Durum == "Bekliyor")
            {
                // 2. İzni isteyen personeli bul
                var user = await _userManager.FindByIdAsync(talep.UserId);

                if (user != null)
                {
                    // 3. HESAPLAMA: Kalan izinden talebin gün sayısını düş (14 - 4 = 10 gibi)
                    user.KalanIzinGun -= talep.GunSayisi;

                    // 4. Durumu güncelle
                    talep.Durum = "Onaylandı";

                    // 5. Değişiklikleri kaydet
                    await _userManager.UpdateAsync(user); // Personelin yeni izin sayısını günceller
                    await _context.SaveChangesAsync();    // İzin talebinin durumunu günceller
                    TempData["SuccessMessage"] = "İzin onaylandı, bakiyeden düşüldü.";
                }
            }
            return RedirectToAction(nameof(OnayBekleyenler));
        }
        // İzni Reddetme İşlemi
        [HttpPost]
        [Authorize(Roles = "Mudur")]
        public async Task<IActionResult> Reddet(int id)
        {
            var talep = await _context.IzinTalepleri.FindAsync(id);
            if (talep != null)
            {
                talep.Durum = "Reddedildi";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(OnayBekleyenler));
        }
    }
}