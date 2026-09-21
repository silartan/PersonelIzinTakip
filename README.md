# 🏢 Personel İzin Takip Sistemi

Kurumsal şirketlerdeki izin süreçlerini dijitalleştirmek, onay mekanizmalarını hızlandırmak ve departman bazlı iş gücü yönetimini optimize etmek amacıyla geliştirilmiş **ASP.NET Core MVC** tabanlı web uygulaması.

---

## 🌟 Öne Çıkan Özellikler

### 👥 Rol Bazlı Yetkilendirme ve Hiyerarşik Yönetim
* **Admin (Genel Müdür):** 
  * Şirketteki tüm departmanları ve personelleri görüntüleme ve yönetme tam yetkisi.
  * Resmi tatil ve izin günlerini sisteme tanımlama.
  * Kullanıcılara Departman Müdürü rolü atama veya yetkilerini güncelleme.
* **Departman Müdürü:** 
  * Yalnızca kendi departmanına bağlı çalışanları ve bu çalışanların izin taleplerini görüntüleme.
  * Departman çalışanlarının izin taleplerini onaylama veya gerekçeli reddetme.
  * Şirket genel tatil takvimini görüntüleme ve yönetme.
* **Kullanıcı (Personel):** 
  * İzin türüne göre (yıllık izin, mazeret, sağlık vb.) talep oluşturma.
  * Kalan izin gün sayılarını ve geçmiş taleplerin durumunu (Onaylandı / Reddedildi / Beklemede) anlık takip etme.

### 📊 Raporlama & Excel Dışa Aktarma (Export)
* İzin kayıtları ve onay geçmişi, İK süreçlerinde kullanılmak veya arşivlenmek üzere dinamik olarak **Excel (.xlsx)** formatında dışa aktarılabilir.

### 📅 Resmi Tatil & Takvim Entegrasyonu
* Sistem üzerinden tanımlanan resmi tatiller, izin hesaplamalarında ve onay süreçlerinde dinamik olarak hesaba katılır.

---

## 🛠️ Kullanılan Teknolojiler ve Mimariler

* **Backend:** C#, .NET / ASP.NET Core MVC
* **Veritabanı & ORM:** Microsoft SQL Server (MSSQL), Entity Framework Core (Code-First & Migrations)
* **Kimlik Doğrulama & Yetkilendirme:** ASP.NET Core Identity (Role-Based Authorization)
* **Raporlama:** Excel Export (EPPlus / ClosedXML)
* **Frontend:** HTML5, CSS3, JavaScript, Bootstrap

