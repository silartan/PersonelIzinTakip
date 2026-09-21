using Microsoft.AspNetCore.Mvc;
using PersonelIzinTakip.Data;
using PersonelIzinTakip.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text;

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

            // 1. TARİH ÇAKIŞMA ALGORİTMASI 
            var cakismaVarMi = await _context.IzinTalepleri
                .AnyAsync(x => x.UserId == userId &&
                               x.Durum != "Reddedildi" &&
                               model.BaslangicTarihi <= x.BitisTarihi &&
                               model.BitisTarihi >= x.BaslangicTarihi);

            if (cakismaVarMi)
            {
                ModelState.AddModelError("", "Hata: Seçtiğiniz tarihlerde zaten onaylı veya bekleyen bir izniniz bulunuyor!");
                return View(model);
            }

            // 1.5 YETERSIZ IZIN KREDISI KONTROLÜ
            var user = await _userManager.GetUserAsync(User);

            // DÜZELTİLEN KISIM 1: Yeni fonksiyonu çağırıyoruz
            int talepEdilenGun = CalculateNetWorkingDays(model.BaslangicTarihi, model.BitisTarihi);

            if (user != null && talepEdilenGun > user.KalanIzinGun)
            {
                ModelState.AddModelError("", $"Hata: Yetersiz izin kredisi! Kalan izniniz ({user.KalanIzinGun} gün), talep ettiğiniz süreden ({talepEdilenGun} gün) daha az.");
                return View(model);
            }

            // 2. VERİLERİ DOLDURMA
            model.UserId = userId;
            model.Durum = "Bekliyor";

            // DÜZELTİLEN KISIM 2: Net günü veritabanı modeline aktarıyoruz
            model.GunSayisi = talepEdilenGun;

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
       
        // Sadece ilgili departmanın müdürü veya Genel Müdür bu sayfayı görebilsin
        [Authorize(Roles = "Mudur")]
        public async Task<IActionResult> OnayBekleyenler()
        {
            // 1. Sisteme giriş yapan müdürün bilgilerini alıyoruz
            var mudur = await _userManager.GetUserAsync(User);
            if (mudur == null) return Challenge();

            // 2. Temel sorguyu oluşturuyoruz: Durumu "Bekliyor" olan izinler
            var sorgu = _context.IzinTalepleri.Include(x => x.User).Where(x => x.Durum == "Bekliyor");

            // SİHİRLİ DOKUNUŞ: Eğer giren kişi Sıla (Genel Müdür) DEĞİLSE, sadece kendi departmanının izinlerini görsün
            if (mudur.Email != "silartann@gmail.com")
            {
                sorgu = sorgu.Where(x => x.User.DepartmanId == mudur.DepartmanId);
            }

            var bekleyenler = await sorgu.ToListAsync();
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
        // YENİ EKLENEN YARDIMCI METOT: Tatilleri ve Hafta Sonlarını Çıkarır
        private int CalculateNetWorkingDays(DateTime startDate, DateTime endDate)
        {
            int totalWorkingDays = 0;

            var holidays = _context.Holidays
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .ToList();

            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                bool isHoliday = holidays.Any(h => h.Date.Date == date.Date && !h.IsHalfDay);

                if (!isWeekend && !isHoliday)
                {
                    totalWorkingDays++;
                }
            }

            return totalWorkingDays;
        }
        // ADRES: /Izin/ExcelIndir
        [Authorize(Roles = "Mudur")]
        public async Task<IActionResult> ExcelIndir()
        {
            // 1. Veritabanından izin verilerini çekiyoruz
            var izinler = await _context.IzinTalepleri
                .Include(x => x.User)
                .OrderByDescending(x => x.BaslangicTarihi)
                .ToListAsync();

            // 2. CSV formatında Excel tablosu oluşturuyoruz (Lisans istemez!)
            var csv = new StringBuilder();

            // Excel'in Türkçe karakterleri düzgün okuması için BOM (Byte Order Mark) ekliyoruz
            csv.AppendLine("Personel Adı;E-Posta;Başlangıç Tarihi;Bitiş Tarihi;Gün Sayısı;Onay Durumu");

            foreach (var item in izinler)
            {
                var adSoyad = item.User?.AdSoyad ?? "Bilinmeyen";
                var email = item.User?.Email ?? "-";
                var baslangic = item.BaslangicTarihi.ToString("dd.MM.yyyy");
                var bitis = item.BitisTarihi.ToString("dd.MM.yyyy");
                var gun = item.GunSayisi.ToString();
                var durum = item.Durum ?? "-";

                csv.AppendLine($"{adSoyad};{email};{baslangic};{bitis};{gun};{durum}");
            }

            // 3. Dosyayı Excel'in doğrudan açabileceği .csv formatında fırlatıyoruz
            var buffer = Encoding.UTF8.GetBytes(csv.ToString());
            var bom = new byte[] { 0xEF, 0xBB, 0xBF }; // Türkçe karakterler Excel'de düzgün açılsın diye sihirli dokunuş
            var fileContent = bom.Concat(buffer).ToArray();

            string fileName = $"Sirket_Izin_Raporu_{DateTime.Now:dd_MM_yyyy}.csv";
            return File(fileContent, "text/csv; charset=utf-8", fileName);
        }

        // ADRES: /Izin/Calisanlar
        [Authorize(Roles = "Mudur")]
        public async Task<IActionResult> Calisanlar()
        {
            var mudur = await _userManager.GetUserAsync(User);
            if (mudur == null) return Challenge();

            // Temel sorgu: Tüm kullanıcılar
            var sorgu = _userManager.Users.AsQueryable();

            // Eğer giren kişi Sıla (Genel Müdür) DEĞİLSE, sadece kendi departmanındaki çalışanları listele
            if (mudur.Email != "silartann@gmail.com")
            {
                sorgu = sorgu.Where(x => x.DepartmanId == mudur.DepartmanId);
            }

            var liste = await sorgu.ToListAsync();
            return View(liste);
        }

        [Authorize(Roles = "Mudur")]
        [HttpPost]
        public async Task<IActionResult> MakeManager(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();

            // ApplicationUser türünde arama yapıyoruz
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                var result = await _userManager.AddToRoleAsync(user, "Mudur");
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"{user.Email} başarıyla Müdür yapıldı!";
                }
            }
            return RedirectToAction(nameof(Calisanlar));
        }
    }
}