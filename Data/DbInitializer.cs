using IDMSBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace IDMSBackend.Data;

public class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        context.Database.Migrate();
        
        
        
        if (!context.Roles.Any())
        {
            var roles = new List<Roles>
            {
                new Roles { Id = Guid.Parse("A1111111-1111-1111-1111-111111111111"), Name = "Admin", Description = "Full system access" },
                new Roles { Id = Guid.Parse("B2222222-2222-2222-2222-222222222222"), Name = "Manager", Description = "Event and Report management" },
                new Roles { Id = Guid.Parse("C3333333-3333-3333-3333-333333333333"), Name = "Scanner", Description = "Restricted scanning access" }
            };
            context.Roles.AddRange(roles);
            context.SaveChanges();
            Console.WriteLine("--> Seeding Roles...");
        }

        if (context.Students.Any())
            return;

        var students = new List<Students>
        {
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SJOHND 2311",
                FullName = "John Doe",
                Email = "john.doe@univ.edu",
                YearOfStudy = "1",
                Residence = "Hall A",
                AcademicStatus = "Active"
            },
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SJANES 2312",
                FullName = "Jane Smith",
                Email = "jane.smith@univ.edu",
                YearOfStudy = "2",
                Residence = "Hall B",
                AcademicStatus = "Active"
            },
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SMIKER 2313",
                FullName = "Mike Ross",
                Email = "mike.ross@univ.edu",
                YearOfStudy = "1",
                Residence = "Off-Campus",
                AcademicStatus = "Active"
            },
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SRACHZ 2314",
                FullName = "Rachel Zane",
                Email = "rachel.zane@univ.edu",
                YearOfStudy = "3",
                Residence = "Hall A",
                AcademicStatus = "Active"
            },
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SHARVS 2315",
                FullName = "Harvey Specter",
                Email = "harvey.specter@univ.edu",
                YearOfStudy = "4",
                Residence = "Off-Campus",
                AcademicStatus = "Active"
            },
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SDONNP 2316",
                FullName = "Donna Paulsen",
                Email = "donna.paulsen@univ.edu",
                YearOfStudy = "2",
                Residence = "Hall C",
                AcademicStatus = "Active"
            },
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SLOUIS 2317",
                FullName = "Louis Litt",
                Email = "louis.litt@univ.edu",
                YearOfStudy = "3",
                Residence = "Hall B",
                AcademicStatus = "Probation"
            },
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SJESSP 2318",
                FullName = "Jessica Pearson",
                Email = "jessica.pearson@univ.edu",
                YearOfStudy = "4",
                Residence = "Off-Campus",
                AcademicStatus = "Active"
            },
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SKATRB 2319",
                FullName = "Katrina Bennett",
                Email = "katrina.bennett@univ.edu",
                YearOfStudy = "1",
                Residence = "Hall C",
                AcademicStatus = "Active"
            },
            new Students
            {
                Id = Guid.NewGuid(),
                SchoolId = "SALEXW 2320",
                FullName = "Alex Williams",
                Email = "alex.williams@univ.edu",
                YearOfStudy = "2",
                Residence = "Hall A",
                AcademicStatus = "Suspended"
            }
        };

        Console.WriteLine("SEED METHOD EXECUTED");

        context.Students.AddRange(students);
        context.SaveChanges();
    }
}