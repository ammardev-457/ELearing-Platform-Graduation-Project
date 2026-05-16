using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Sections;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    // Interfaces
    public interface ISectionRepository : IRepository<Section, int> 
    {
        public Task<int> CreateSection(CreateSectionDto dto);
        public Task<Section?> GetSectionById(int sectionId);
        public Task<IEnumerable<Section>> GetSectionsWithLessonsByCourseId(int courseId);
        public Task<Section?> GetSectionwithCourseById(int sectionId);

        public Task<bool> UpdateSection(int sectionId, UpdateSectionDto dto);
        public Task<bool> DeleteSection(int sectionId);
    }
}