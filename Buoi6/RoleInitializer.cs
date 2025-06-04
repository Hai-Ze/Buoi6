using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Buoi6
{
    public static class RoleInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                Console.WriteLine("=== BẮT ĐẦU TẠO ROLES ===");

                // Tạo role Admin
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole("Admin"));
                    Console.WriteLine($"Tạo role Admin: {result.Succeeded}");
                }
                else
                {
                    Console.WriteLine("Role Admin đã tồn tại");
                }

                // Tạo role Member
                if (!await roleManager.RoleExistsAsync("Member"))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole("Member"));
                    Console.WriteLine($"Tạo role Member: {result.Succeeded}");
                }
                else
                {
                    Console.WriteLine("Role Member đã tồn tại");
                }

                // Tạo tài khoản admin
                string adminEmail = "admin@example.com";
                string adminPassword = "Admin@123";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    Console.WriteLine("Bắt đầu tạo tài khoản admin...");
                    adminUser = new IdentityUser
                    {
                        UserName = "admin",
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                    Console.WriteLine($"Tạo tài khoản admin: {createResult.Succeeded}");

                    if (createResult.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine($"Gán role Admin: {roleResult.Succeeded}");
                        Console.WriteLine("✅ Tạo admin thành công!");
                    }
                    else
                    {
                        Console.WriteLine("❌ Lỗi tạo admin:");
                        foreach (var error in createResult.Errors)
                        {
                            Console.WriteLine($"  - {error.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("✅ Admin đã tồn tại");

                    // Kiểm tra role
                    var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"Admin có role: {isInRole}");

                    if (!isInRole)
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine($"Thêm role Admin: {addRoleResult.Succeeded}");
                    }
                }

                Console.WriteLine("=== HOÀN THÀNH ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI: {ex.Message}");
            }
        }
    }
}