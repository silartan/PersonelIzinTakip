
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonelIzinTakip.Models;
//using System.Collections.Generic;
namespace PersonelIzinTakip.Data
{
    // IdentityDbContext<ApplicationUser> sınıfından türetiyoruz.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet'leri burada tanımlıyoruz (tablo olarak kullanılacak).
        public DbSet<Departman> Departmanlar { get; set; }
        public DbSet<IzinTalep> IzinTalepleri { get; set; }
        public DbSet<LogKaydi> LogKayitlari { get; set; }
    }

}
