using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

SQLitePCL.Batteries_V2.Init();

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "VgcCollege.Web")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<VgcCollege.Web.Services.BranchService>();
builder.Services.AddScoped<BranchService>();
builder.Services.AddScoped<VgcCollege.Web.Services.CourseService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<VgcCollege.Web.Services.StudentProfileService>();
builder.Services.AddScoped<StudentProfileService>();
builder.Services.AddScoped<VgcCollege.Web.Services.CourseEnrolmentService>();
builder.Services.AddScoped<CourseEnrolmentService>();
builder.Services.AddScoped<VgcCollege.Web.Services.AttendanceRecordService>();
builder.Services.AddScoped<AttendanceRecordService>();
builder.Services.AddScoped<VgcCollege.Web.Services.AssignmentService>();
builder.Services.AddScoped<VgcCollege.Web.Services.AssignmentResultService>();
builder.Services.AddScoped<VgcCollege.Web.Services.ExamService>();
builder.Services.AddScoped<VgcCollege.Web.Services.ExamResultService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var userName = context.User?.Identity?.IsAuthenticated == true
        ? context.User.Identity!.Name
        : "Anonymous";

    using (LogContext.PushProperty("UserName", userName))
    {
        await next();
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

await SeedData.InitializeAsync(app.Services);

app.Run();
