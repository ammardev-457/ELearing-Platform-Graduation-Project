using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ELProject.DataAccess.Seed
{
    public static class ComprehensiveDataSeeder
    {
        public static async Task SeedAllDataAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles
            await SeedRolesAsync(roleManager);

            // 2. Seed Categories
            await SeedCategoriesAsync(context);

            // 3. Seed Users (Instructors & Students)
            await SeedUsersAsync(userManager, context);

            // 4. Seed Courses
            await SeedCoursesAsync(context);

            // 5. Seed Sections
            await SeedSectionsAsync(context);

            // 6. Seed Lessons
            await SeedLessonsAsync(context);

            // 7. Seed Quizzes
            await SeedQuizzesAsync(context);

            // 8. Seed Questions
            await SeedQuestionsAsync(context);

            // 9. Seed Reviews
            await SeedReviewsAsync(context);

            // 10. Seed Options
            await SeedOptionsAsync(context);

            await SeedEnrollAsync(context);

            // 11. Seed Orders
            //await SeedOrdersAsync(context);

            // 12. Seed Enrollments
            //await SeedEnrollmentsAsync(context);

            // 13. Seed Transactions
            //await SeedTransactionsAsync(context);
        }

        private static async Task SeedEnrollAsync(AppDbContext context)
        {
            // 1. تحديد بيانات الطالب والكورس (يفضل جلبهم من الداتا بيز لضمان وجودهم)
            var student = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == "student1");
            var course = await context.Courses.FirstOrDefaultAsync(c => c.Id == 1);

            if (student == null || course == null) return;

            // التحقق من عدم وجود تسجيل مسبق لتجنب تكرار البيانات عند كل تشغيل للبرنامج
            var isAlreadyEnrolled = await context.Enrollments
                .AnyAsync(e => e.CourseId == course.Id && e.StudentId == student.Id);

            if (!isAlreadyEnrolled)
            {
                // 2. إنشاء الطلب (Order)
                // الحالة Success لتخطي نظام الدفع
                var order = new Order
                {
                    StudentId = student.Id,
                    CourseId = course.Id,
                    Amount = (long)(course.Price * 100), // تحويل السعر لـ long (مثلاً بالقرش)
                    Status = PaymentStatus.Success.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync(); // ضروري للحصول على order.Id

                // 3. إنشاء المعاملة (Transaction)
                var transaction = new Transaction
                {
                    OrderId = order.Id,
                    TransactionId = "MANUAL_TXN_" + Guid.NewGuid().ToString("N").Substring(0, 10),
                    Amount = order.Amount,
                    Status = PaymentStatus.Success.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Transactions.AddAsync(transaction);

                // 4. إنشاء التسجيل (Enrollment)
                var enrollment = new Enrollment
                {
                    StudentId = student.Id,
                    CourseId = course.Id,
                    OrderId = order.Id, // الربط الإجباري بالطلب
                    EnrollDate = DateTime.UtcNow,
                    Progress = 0,
                    IsCompleted = false
                };

                await context.Enrollments.AddAsync(enrollment);

                // حفظ المعاملة والتسجيل معاً
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in Enum.GetNames(typeof(UserRole)))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedCategoriesAsync(AppDbContext context)
        {
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Technology" },
                    new() { Name = "Marketing" },
                    new() { Name = "Design" },
                    new() { Name = "AI" },
                    new() { Name = "Content Creation" },
                    new() { Name = "Personal Development" },
                    new() { Name = "Web Development" },
                    new() { Name = "Mobile Development" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            if (!await context.ApplicationUsers.AnyAsync())
            {
                // Seed Instructors
                var instructors = new List<ApplicationUser>
                {
                    new()
                    {
                        UserName = "instructor1",
                        Email = "instructor1@email.com",
                        EmailConfirmed = true,
                        Bio = "Expert in Web Development",
                        Gender = Gender.Male,
                        JoinDate = DateTime.UtcNow.AddDays(-365)
                    },
                    new()
                    {
                        UserName = "instructor2",
                        Email = "instructor2@email.com",
                        EmailConfirmed = true,
                        Bio = "UI/UX Design Specialist",
                        Gender = Gender.Female,
                        JoinDate = DateTime.UtcNow.AddDays(-180)
                    },
                    new()
                    {
                        UserName = "instructor3",
                        Email = "instructor3@email.com",
                        EmailConfirmed = true,
                        Bio = "AI and Machine Learning Expert",
                        Gender = Gender.Male,
                        JoinDate = DateTime.UtcNow.AddDays(-120)
                    }
                };

                foreach (var instructor in instructors)
                {
                    await userManager.CreateAsync(instructor, "Password@123");
                    await userManager.AddToRoleAsync(instructor, UserRole.Instructor.ToString());
                }

                // Seed Students
                var students = new List<ApplicationUser>
                {
                    new()
                    {
                        Name = "Ahmed Mohamed",
                        UserName = "student1",
                        Email = "student1@email.com",
                        EmailConfirmed = true,
                        Bio = "Learning Web Development",
                        Gender = Gender.Male,
                        JoinDate = DateTime.UtcNow.AddDays(-90)
                    },
                    new()
                    {
                        Name = "Sara Ali",
                        UserName = "student2",
                        Email = "student2@email.com",
                        EmailConfirmed = true,
                        Bio = "Interested in Design",
                        Gender = Gender.Female,
                        JoinDate = DateTime.UtcNow.AddDays(-60)
                    },
                    new()
                    {
                        Name = "Omar Hassan",
                        UserName = "student3",
                        Email = "student3@email.com",
                        EmailConfirmed = true,
                        Bio = "AI Enthusiast",
                        Gender = Gender.Male,
                        JoinDate = DateTime.UtcNow.AddDays(-30)
                    },
                    new()
                    {
                        Name = "Nour Khalid",
                        UserName = "student4",
                        Email = "student4@email.com",
                        EmailConfirmed = true,
                        Bio = "Learning Marketing",
                        Gender = Gender.Female,
                        JoinDate = DateTime.UtcNow.AddDays(-15)
                    }
                };

                foreach (var student in students)
                {
                    await userManager.CreateAsync(student, "Password@123");
                    await userManager.AddToRoleAsync(student, "Student");
                }
            }
        }

        private static async Task SeedCoursesAsync(AppDbContext context)
        {
            if (!await context.Courses.AnyAsync())
            {
                var users = await context.ApplicationUsers.Where(u => u.UserName!.StartsWith("instructor")).ToListAsync();
                var categories = await context.Categories.ToListAsync();

                var courses = new List<Course>
                {
                    new()
                    {
                        Title = "Complete C# and .NET Course",
                        ShortDescription = "Learn C# and .NET from scratch",
                        LongDescription = "Master C#, ASP.NET Core, Entity Framework, and more in this comprehensive course",
                        UserId = users[0].Id,
                        CategoryId = categories.FirstOrDefault(c => c.Name == "Technology")?.Id ?? 1,
                        Level = CourseLevel.Beginner,
                        Price = 99.99m,
                        CreatedDate = DateTime.UtcNow.AddMonths(-6),
                        Thumbnail = "https://via.placeholder.com/400x300?text=CSharp"
                    },
                    new()
                    {
                        Title = "Web Design Fundamentals",
                        ShortDescription = "Create beautiful web designs",
                        LongDescription = "Learn HTML, CSS, and modern web design principles",
                        UserId = users[1].Id,
                        CategoryId = categories.FirstOrDefault(c => c.Name == "Design")?.Id ?? 3,
                        Level = CourseLevel.Beginner,
                        Price = 79.99m,
                        CreatedDate = DateTime.UtcNow.AddMonths(-4),
                        Thumbnail = "https://via.placeholder.com/400x300?text=WebDesign"
                    },
                    new()
                    {
                        Title = "Introduction to AI and Machine Learning",
                        ShortDescription = "Start your AI journey",
                        LongDescription = "Learn the fundamentals of artificial intelligence and machine learning with Python",
                        UserId = users[2].Id,
                        CategoryId = categories.FirstOrDefault(c => c.Name == "AI")?.Id ?? 4,
                        Level = CourseLevel.Intermediate,
                        Price = 149.99m,
                        CreatedDate = DateTime.UtcNow.AddMonths(-3),
                        Thumbnail = "https://via.placeholder.com/400x300?text=AI"
                    },
                    new()
                    {
                        Title = "Advanced JavaScript",
                        ShortDescription = "Master JavaScript concepts",
                        LongDescription = "Deep dive into advanced JavaScript patterns and techniques",
                        UserId = users[0].Id,
                        CategoryId = categories.FirstOrDefault(c => c.Name == "Web Development")?.Id ?? 7,
                        Level = CourseLevel.Advanced,
                        Price = 89.99m,
                        CreatedDate = DateTime.UtcNow.AddMonths(-2),
                        Thumbnail = "https://via.placeholder.com/400x300?text=JavaScript"
                    }
                };

                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSectionsAsync(AppDbContext context)
        {
            if (!await context.Sections.AnyAsync())
            {
                var courses = await context.Courses.ToListAsync();

                var sections = new List<Section>();
                foreach (var course in courses)
                {
                    sections.AddRange(new[]
                    {
                        new Section { Title = $"Getting Started with {course.Title}", CourseId = course.Id },
                        new Section { Title = $"Core Concepts", CourseId = course.Id },
                        new Section { Title = $"Advanced Topics", CourseId = course.Id }
                    });
                }

                await context.Sections.AddRangeAsync(sections);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedLessonsAsync(AppDbContext context)
        {
            if (!await context.Lessons.AnyAsync())
            {
                var sections = await context.Sections.ToListAsync();

                var lessons = new List<Lesson>();
                foreach (var section in sections)
                {
                    lessons.AddRange(new[]
                    {
                        new Lesson
                        {
                            Title = "Introduction",
                            Order = 1,
                            Type = FileType.Video,
                            FileUrl = "https://example.com/video1.mp4",
                            DurationInSeconds = 900,
                            SectionId = section.Id
                        },
                        new Lesson
                        {
                            Title = "Main Content",
                            Order = 2,
                            Type = FileType.Video,
                            FileUrl = "https://example.com/video2.mp4",
                            DurationInSeconds = 1800,
                            SectionId = section.Id
                        },
                        new Lesson
                        {
                            Title = "Course Materials PDF",
                            Order = 3,
                            Type = FileType.Pdf,
                            FileUrl = "https://example.com/materials.pdf",
                            SectionId = section.Id
                        }
                    });
                }

                await context.Lessons.AddRangeAsync(lessons);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedQuizzesAsync(AppDbContext context)
        {
            if (!await context.Quizzes.AnyAsync())
            {
                var courses = await context.Courses.ToListAsync();

                var quizzes = new List<Quiz>();
                foreach (var course in courses)
                {
                    quizzes.AddRange(new[]
                    {
                        new Quiz
                        {
                            Title = $"{course.Title} - Midterm Quiz",
                            TotalMarks = 100,
                            TimeLimitInMinutes = 60,
                            QuizType = QuizType.Mcq,
                            CourseId = course.Id
                        },
                        new Quiz
                        {
                            Title = $"{course.Title} - Final Exam",
                            TotalMarks = 100,
                            TimeLimitInMinutes = 120,
                            QuizType = QuizType.TrueFalse,
                            CourseId = course.Id
                        }
                    });
                }

                await context.Quizzes.AddRangeAsync(quizzes);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedQuestionsAsync(AppDbContext context)
        {
            if (!await context.Questions.AnyAsync())
            {
                var quizzes = await context.Quizzes.ToListAsync();

                var questions = new List<Question>();
                foreach (var quiz in quizzes)
                {
                    questions.AddRange(new[]
                    {
                        new Question
                        {
                            QuestionText = "What is the first concept in this course?",
                            QuizId = quiz.Id
                        },
                        new Question
                        {
                            QuestionText = "Are You Student?",
                            QuizId = quiz.Id
                        }
                    });
                }

                await context.Questions.AddRangeAsync(questions);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedOptionsAsync(AppDbContext context)
        {
            if (!await context.Options.AnyAsync())
            {
                var questions = await context.Questions.ToListAsync();
                var options = new List<Option>();
                foreach (var question in questions)
                {
                    options.AddRange(new[]
                    {
                        new Option { Text = "Option A", QuestionId = question.Id },
                        new Option { Text = "Option B", QuestionId = question.Id },
                        new Option { Text = "Option C", QuestionId = question.Id },
                        new Option { Text = "Option D", QuestionId = question.Id }
                    });
                }
                await context.Options.AddRangeAsync(options);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedReviewsAsync(AppDbContext context)
        {
            if (!await context.Reviews.AnyAsync())
            {
                var students = await context.ApplicationUsers.Where(u => u.UserName!.StartsWith("student")).ToListAsync();
                var courses = await context.Courses.ToListAsync();

                var reviews = new List<Review>();
                foreach (var course in courses)
                {
                    for (int i = 0; i < students.Count - 1; i++)
                    {
                        reviews.Add(new Review
                        {
                            Comment = $"Great course! Very informative and well-structured. Student {i + 1}'s review.",
                            Rating = (i % 5) + 1,
                            CreatedAt = DateTime.UtcNow.AddDays(-(i + 1)),
                            UserId = students[i].Id,
                            CourseId = course.Id
                        });
                    }
                }

                await context.Reviews.AddRangeAsync(reviews);
                await context.SaveChangesAsync();
            }
        }

        //private static async Task SeedOrdersAsync(AppDbContext context)
        //{
        //    if (!await context.Orders.AnyAsync())
        //    {
        //        var students = await context.ApplicationUsers.Where(u => u.UserName!.StartsWith("student")).ToListAsync();
        //        var courses = await context.Courses.ToListAsync();

        //        var orders = new List<Order>();
        //        foreach (var student in students)
        //        {
        //            foreach (var course in courses.Take(2))
        //            {
        //                orders.Add(new Order
        //                {
        //                    StudentId = student.Id,
        //                    CourseId = course.Id,
        //                    Amount = (long)(course.Price * 100),
        //                    Currency = "EGP",
        //                    Status = PaymentStatus.Success.ToString(),
        //                    CreatedAt = DateTime.UtcNow.AddDays(-10),
        //                    UpdatedAt = DateTime.UtcNow
        //                });
        //            }
        //        }

        //        await context.Orders.AddRangeAsync(orders);
        //        await context.SaveChangesAsync();
        //    }
        //}

        //private static async Task SeedEnrollmentsAsync(AppDbContext context)
        //{
        //    if (!await context.Enrollments.AnyAsync())
        //    {
        //        var students = await context.ApplicationUsers.Where(u => u.UserName!.StartsWith("student")).ToListAsync();
        //        var orders = await context.Orders.Include(o => o.Course).ToListAsync();

        //        var enrollments = new List<Enrollment>();
        //        foreach (var order in orders)
        //        {
        //            enrollments.Add(new Enrollment
        //            {
        //                StudentId = order.StudentId,
        //                CourseId = order.CourseId,
        //                EnrollDate = DateTime.UtcNow.AddDays(-8),
        //                Progress = Random.Shared.Next(0, 101),
        //                IsCompleted = Random.Shared.Next(0, 100) > 70,
        //                CompletedAt = Random.Shared.Next(0, 100) > 70 ? DateTime.UtcNow.AddDays(-2) : null,
        //                OrderId = order.Id
        //            });
        //        }

        //        await context.Enrollments.AddRangeAsync(enrollments);
        //        await context.SaveChangesAsync();
        //    }
        //}

        //private static async Task SeedTransactionsAsync(AppDbContext context)
        //{
        //    if (!await context.Transactions.AnyAsync())
        //    {
        //        var orders = await context.Orders.ToListAsync();

        //        var transactions = new List<Transaction>();
        //        foreach (var order in orders)
        //        {
        //            transactions.Add(new Transaction
        //            {
        //                OrderId = order.Id,
        //                Amount = order.Amount,
        //                Currency = order.Currency,
        //                Status = PaymentStatus.Success.ToString(),
        //                CreatedAt = DateTime.UtcNow.AddDays(-9)
        //            });
        //        }

        //        await context.Transactions.AddRangeAsync(transactions);
        //        await context.SaveChangesAsync();
        //    }
        //}
    }
}