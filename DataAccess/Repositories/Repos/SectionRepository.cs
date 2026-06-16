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

        public async Task<Section?> GetSectionwithCourseById(int sectionId) => await context.Sections
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId);

        public async Task<bool> UpdateSection(Section sectionFromDb, UpdateSectionDto dto)
        {
            sectionFromDb.Title = dto.Title ?? sectionFromDb.Title;
            sectionFromDb.Order = dto.Order == 0 ? sectionFromDb.Order : dto.Order;
            return true;
        }

    }
}
