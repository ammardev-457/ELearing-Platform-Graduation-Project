using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Courses;
using ELProject.Shared.DTOs.Sections;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class SectionRepository : Repository<Section, int>, ISectionRepository
    {
        private readonly AppDbContext context;
        public SectionRepository(AppDbContext _context) : base(_context) => context = _context;

        public async Task<int> CreateSection(int courseId, CreateSectionDto dto)
        {
            var newSection = new Section
            {
                CourseId = courseId,
                Title = dto.Title,
                Order = dto.Order
            };
            await context.Sections.AddAsync(newSection);
            await context.SaveChangesAsync();

            return newSection.Id; 
        }

        public async Task<Section?> GetSectionById(int sectionId) => await context.Sections.FindAsync(sectionId);

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
                    DurationInSeconds = l.DurationInSeconds ?? 60,
                    SectionId = l.SectionId
                }).ToList()
            })
            .OrderBy(s => s.Order)
            .ToListAsync();
        }

        public async Task<Section?> GetSectionwithCourseById(int sectionId) => await context.Sections
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId);

        public async Task<bool> UpdateSection(int sectionId, UpdateSectionDto dto)
        {
            var section = await context.Sections.FindAsync(sectionId);
            if (section == null) return false;
            section.Title = dto.Title;
            section.Order = dto.Order;
            return true;
        }

        public async Task<bool> DeleteSection(int sectionId)
        {
            var section = await context.Sections.FindAsync(sectionId);
            if (section == null) return false;
            context.Sections.Remove(section);
            return true;
        }
    }
}
