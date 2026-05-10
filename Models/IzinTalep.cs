using System.ComponentModel.DataAnnotations;

namespace PersonelIzinTakip.Models

{
    public class IzinTalep
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser ?User { get; set; }

        [Required]
        public DateTime BaslangicTarihi { get; set; }

        [Required]
        public DateTime BitisTarihi { get; set; }

        public int GunSayisi { get; set; }

        public string Aciklama { get; set; }

        public string Durum { get; set; } // Bekliyor, Onaylandi, Reddedildi

    }
}
