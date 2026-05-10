using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonelIzinTakip.Data;
using PersonelIzinTakip.Models;


var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Yapılandırması (Sadece bu blok yeterli)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Geliştirme aşamasında kolaylık sağlar
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // Scaffolding ile gelen sayfaları aktif eder

builder.Services.AddControllersWithViews();
// 1. Önce kuralı tanımlıyoruz
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// 2. Login, Register gibi sayfaların bu kurala takılmamasını sağlıyoruz
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Logout");
});

builder.Services.AddRazorPages(); // Razor Pages desteği şart

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. Sıralama Çok Önemli: Önce Authentication, sonra Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Sayfaların yönlendirmesi için şart
// app.Run() satırının hemen üstüne ekle:
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Rolleri Otomatik Oluştur
    string[] roleNames = { "Mudur", "Personel" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 2. İlk Müdürü Ata (Kendi kayıt olacağın maili yaz)
    var adminEmail = "silartann@gmail.com";
    var mudurUser = await userManager.FindByEmailAsync(adminEmail);

    if (mudurUser != null && !(await userManager.IsInRoleAsync(mudurUser, "Mudur")))
    {
        await userManager.AddToRoleAsync(mudurUser, "Mudur");
    }
}
app.Run();