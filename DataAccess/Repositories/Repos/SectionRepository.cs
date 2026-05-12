using ELProject.Domain.Models;
using ELProject.Shared.DTOs;
using ELProject.Shared.DTOs.Courses;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class SectionRepository
    {
        private readonly AppDbContext context;

        public SectionRepository(AppDbContext _context)
        {
            context = _context;
        }

        public async Task<int> CreateSection(CreateSectionDto dto)
        {
            var newSection = new Section
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                Order = dto.Order
            };

            context.Sections.Add(newSection);
            await context.SaveChangesAsync();

            return newSection.Id;

        }

        public async Task<Section?> GetSectionById(int sectionId)
        {
            return await context.Sections.FindAsync(sectionId);
        }

        public async Task<IEnumerable<Section>> GetSectionsWithLessonsByCourseId(int courseId)
        {
            return await context.Sections
                .Where(s => s.CourseId == courseId)
                .Include(s => s.Lessons)
                .Select(s => new Section
                {
                    Id = s.Id,
                    Title = s.Title,
                    Order = s.Order,
                    Lessons = s.Lessons.Select(l => new Lesson
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Order = l.Order,
                        Type = l.Type,
                        DurationInSeconds = l.DurationInSeconds ?? 60, // if file type is pdf, then duration is 60 seconds by default
                        SectionId = l.SectionId
                    }).ToList()
                })
                .OrderBy(s => s.Order)
                .ToListAsync();
        }

        public async Task<Section?> GetSectionwithCourseById(int sectionId)
        {
            return await context.Sections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == sectionId);
        }

        public async Task<bool> UpdateSection(int sectionId, UpdateSectionDto dto)
        {
            var section = await context.Sections.FindAsync(sectionId);

            if (section == null)
                return false;

            section.Title = dto.Title;
            section.Order = dto.Order;

            try
            {
                context.Sections.Update(section);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSection(int sectionId)
        {
            var section = await context.Sections.FindAsync(sectionId);

            if (section == null)
                return false;

            try
            {
                context.Sections.Remove(section);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
