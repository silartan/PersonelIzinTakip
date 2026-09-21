using Microsoft.AspNetCore.Mvc;
using PersonelIzinTakip.Data;
using PersonelIzinTakip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace PersonelIzinTakip.Controllers
{
    // Sadece "Mudur" rolüne sahip kullanıcılar bu sayfalara girebilir
    [Authorize(Roles = "Mudur")]
    public class TatilController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TatilController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TATİLLERİ LİSTELEME SAYFASI
        public async Task<IActionResult> Index()
        {
            // Tatilleri tarihe göre eskiden yeniye doğru sıralayıp getiriyoruz
            var tatiller = await _context.Holidays
                .OrderBy(t => t.Date)
                .ToListAsync();

            return View(tatiller);
        }

        // 2. YENİ TATİL EKLEME SAYFASINI AÇMA
        public IActionResult Ekle()
        {
            return View();
        }

        // 3. YENİ TATİL FORMU GÖNDERİLDİĞİNDE ÇALIŞAN KISIM
        [HttpPost]
        public async Task<IActionResult> Ekle(string Name, DateTime StartDate, DateTime EndDate, bool IsHalfDay = false)
        {
            // Kullanıcı başlangıç tarihini bitiş tarihinden daha ileri bir tarih seçerse uyaralım
            if (StartDate > EndDate)
            {
                TempData["ErrorMessage"] = "Hata: Başlangıç tarihi, bitiş tarihinden büyük olamaz!";
                return RedirectToAction(nameof(Ekle));
            }

            // Seçilen başlangıç tarihinden, bitiş tarihine kadar gün gün dönüyoruz (Harika Kısım Burası!)
            for (DateTime date = StartDate.Date; date <= EndDate.Date; date = date.AddDays(1))
            {
                var yeniTatil = new Holiday
                {
                    Name = Name,
                    Date = date, // O anki döngüdeki gün
                    IsHalfDay = IsHalfDay
                };

                _context.Holidays.Add(yeniTatil);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Belirtilen tarih aralığındaki tatiller başarıyla sisteme eklendi.";
            return RedirectToAction(nameof(Index));
        }

        // 4. TATİL SİLME İŞLEMİ (GÜNCELLENDİ: Artık isme göre tüm grubu siliyor)
        [HttpPost]
        public async Task<IActionResult> Sil(string name)
        {
            // O isme sahip olan tüm tatil günlerini bul
            var silinecekTatiller = await _context.Holidays.Where(h => h.Name == name).ToListAsync();

            if (silinecekTatiller.Any())
            {
                _context.Holidays.RemoveRange(silinecekTatiller); // Hepsini tek seferde sil
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{name} adlı tatil sistemden tamamen silindi.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
