using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Sections;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    // Interfaces
    public interface ISectionRepository : IRepository<Section, int> 
    {
        public Task<int> CreateSection(int courseId, CreateSectionDto dto);
        public Task<Section?> GetSectionwithCourseById(int sectionId);
        public Task<bool> UpdateSection(Section sectionFromDb, UpdateSectionDto dto);
    }
}