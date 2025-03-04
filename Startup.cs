using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using App.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using App.Services;
using App.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            var mailSettings = Configuration.GetSection("MailSettings");
            services.Configure<MailSettings>(mailSettings);
            services.AddSingleton<IEmailSender, SendMailService>();

            services.AddRazorPages();
            services.AddDbContext<Models.AppDbContext>(options =>
            {
                string connectString = Configuration.GetConnectionString("MyBlogContext");
                options.UseNpgsql(connectString);
            });

            // Dang ký Identity
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            //services.AddDefaultIdentity<AppUser>()
            //    .AddEntityFrameworkStores<MyBlogContext>()
            //    .AddDefaultTokenProviders();

            // Truy cập IdentityOptions
            services.Configure<IdentityOptions>(options =>
            {
                // Thiết lập về Password
                options.Password.RequireDigit = false; // Không bắt phải có số
                options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
                options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
                options.Password.RequireUppercase = false; // Không bắt buộc chữ in
                options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
                options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

                // Cấu hình Lockout - khóa user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
                options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 5 lần thì khóa
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về User.
                options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;  // Email là duy nhất

                // Cấu hình đăng nhập.
                options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
                options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
                options.SignIn.RequireConfirmedAccount = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login/";
                options.LogoutPath = "/logout/";
                options.AccessDeniedPath = "/khongduoctruycap.html";
            });

            services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AllowEditRole", policyBuilder =>
                {
                    //điều kiện của policy
                    policyBuilder.RequireAuthenticatedUser();
                    // policyBuilder.RequireRole("Admin");
                    // policyBuilder.RequireRole("Editor");
                    policyBuilder.RequireClaim("canedit", "user");

                    //Claims-based authorization
                    // policyBuilder.RequireClaim("Ten Claims", new string[] {
                    //     "value1",
                    //     "value2"
                    // });
                });

                options.AddPolicy("InGenZ", policyBuilder =>
                {
                    //điều kiện của policy
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.Requirements.Add(new GenZRequirement()); // GenZRequirement

                    // new GenZRequirement() => Authorization handler
                });

                options.AddPolicy("ShowAdminMenu", policyBuilder =>
                {
                    //điều kiện của policy
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.RequireRole("Admin");

                    // new GenZRequirement() => Authorization handler
                });

                options.AddPolicy("CanUpdateArticle", policyBuilder =>
                {
                     policyBuilder.Requirements.Add(new ArticleUpdateRequirement()); // GenZRequirement
                });
            });

            services.AddTransient<IAuthorizationHandler, AppAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            //IdentityUser user;
            //IdentityDbContext context;

        }
    }
}

/*
CREATE, READ, UPDATE, DELETE (CRUD)

dotnet aspnet-codegenerator razorpage -m App.Models.Article -dc App.Models.MyBlogContext -outDir Pages/Blog -udl --referenceScriptLibraries


Identity:
    Authentication: Xác định danh tính -> Login logout

    Authorization: Xác thực quyền truy cập -> có quyền gì 
        Role-based authorization - Xác thực quyền theo vai trò
        + Role (vai trò):
            Admin, Editor, Manager, Member

        // /Areas/Admin/Pages/Role
        + Index
        + Create
        + Edit
        + Delete

        // Xác thực quyền
        dotnet new page -n Index -o Areas/Admin/Pages/Role --namespace App.Admin.Role
        dotnet new page -n Create -o Areas/Admin/Pages/Role --namespace App.Admin.Role
        dotnet new page -n claimsInUserClaim -o Areas/Admin/Pages/User --namespace App.Admin.User

        *Policy-based authorization
        *Claims-based authorization
            -> Claims là đặt tính hay tính chất của một đối tượng (User)
            VD: 
                Bằng B2 coi là vai trò (quyền được phép lái xe)
                -Ngày sinh -> claim
                -Nơi sinh -> claim

                Mua rượu ( > 18 tuổi)
                


    Quản lý user(từ đăng ký): Sign up, User, Role,..
- Identity/Account/Login
- Identity/Account/Manage

dotnet aspnet-codegenerator identity -dc App.Models.MyBlogContext

*/