using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PersonelIzinTakip.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Kişisel bilgileri ekliyoruz
        [Required]
        public string AdSoyad { get; set; }
        [Required]
        public int KalanIzinGun { get; set; }


        // Departman ile ilişki
        [Required]
        public int DepartmanId { get; set; }
        [Required]
        public Departman Departman { get; set; }
    }
}