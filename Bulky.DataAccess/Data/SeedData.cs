using BulkyBook.Data;
using BulkyBook.DataAccess.Extend;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Data
{
    public class SeedData
    {
        private readonly ApplicationDbContext _context;

        public SeedData(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedRolesAndUsersAsync(List<UserConfig> usersConfig)
        {
            // Retrieve existing roles from the database
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var companyRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Company");
            var employeeRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Employee");
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");

            if (adminRole == null || companyRole == null || employeeRole == null || customerRole == null)
            {
                throw new Exception("One or more roles are missing in the database.");
            }

            // Create users
            if (usersConfig != null && usersConfig.Any())
            {
                PasswordHasher<ApplicationUser> hasher = new PasswordHasher<ApplicationUser>();

                foreach (var config in usersConfig)
                {
                    // Check if the user already exists in the database based on UserName
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == config.UserName);

                    if (existingUser == null)
                    {
                        // Create new user if not found
                        var newUser = new ApplicationUser
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = config.Name,
                            UserName = config.UserName,
                            NormalizedUserName = config.UserName.ToUpper(),
                            Email = config.Email,
                            NormalizedEmail = config.Email.ToUpper(),
                            EmailConfirmed = true,
                            PasswordHash = hasher.HashPassword(new ApplicationUser(), config.Password),
                            StreetAddress = config.StreetAddress,
                            City = config.City,
                            State = config.State,
                            PostalCode = config.PostalCode,
                            PhoneNumber=config.PhoneNumber,
                            CompanyId = config.CompanyId
                        };

                        // Add the user to the DbSet
                        _context.Users.Add(newUser);
                        await _context.SaveChangesAsync(); // Save to generate user ID

                        // Assign the role to the new user
                        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == config.Role);
                        if (role != null)
                        {
                            _context.UserRoles.Add(new IdentityUserRole<string>
                            {
                                UserId = newUser.Id,
                                RoleId = role.Id
                            });
                        }
                    }
                    else
                    {
                        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == config.Role);
                        if (role != null)
                        {
                            var existingRole = await _context.UserRoles
                                .FirstOrDefaultAsync(ur => ur.UserId == existingUser.Id && ur.RoleId == role.Id);

                            // If the user doesn't have the expected role, assign it
                            if (existingRole == null)
                            {
                                _context.UserRoles.Add(new IdentityUserRole<string>
                                {
                                    UserId = existingUser.Id,
                                    RoleId = role.Id
                                });
                            }
                        }
                    }
                }

                // Save changes after all user-role assignments
                await _context.SaveChangesAsync();
            }
        }
    }
}
