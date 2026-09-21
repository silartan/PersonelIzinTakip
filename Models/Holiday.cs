namespace PersonelIzinTakip.Models
{
    public class Holiday
    {
        public int Id { get; set; }
        public string Name { get; set; } // Tatilin Adı (Örn: Ramazan Bayramı)
        public DateTime Date { get; set; } // Tatilin Tarihi
        public bool IsHalfDay { get; set; } // Yarım gün mü? (Arife günleri için true yapacağız)
    }
}
