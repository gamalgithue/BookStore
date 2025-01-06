using BulkyBook.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Migrations;
using System.Configuration;
using BulkyBook.DataAccess.Extend;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddControllersWithViews().AddNewtonsoftJson(opt => {
//    opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
//});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();

#region ConnectionString
var connectionstring = builder.Configuration.GetConnectionString("ApplicatonConnection");

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(connectionstring));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<SeedData>();

builder.Services.AddRazorPages();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";


});
builder.Services.AddAuthentication().AddFacebook(
    options =>
    {
        options.AppId = builder.Configuration["ExternalLogins:Facebook:AppId"];
        options.AppSecret = builder.Configuration["ExternalLogins:Facebook:AppSecret"];

    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(ops =>
{
    ops.IdleTimeout = TimeSpan.FromMinutes(100);
    ops.Cookie.HttpOnly = true;
    ops.Cookie.IsEssential = true;


});


#endregion


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seedData = scope.ServiceProvider.GetRequiredService<SeedData>();

    // Retrieve users config from the configuration
    var usersConfig = builder.Configuration.GetSection("Users").Get<List<UserConfig>>();

    // Seed roles and users
    if (usersConfig != null && usersConfig.Any())
    {
        await seedData.SeedRolesAndUsersAsync(usersConfig);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:Secretkey").Get<string>();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();
app.UseSession();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
