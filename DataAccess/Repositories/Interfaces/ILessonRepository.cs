using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface ILessonRepository : IRepository<Lesson, int>
    {
        Task<IEnumerable<Lesson>> GetLessonsBySectionId(int sectionId);
        Task<Lesson?> GetLessonWithInstructorId(string instructorId, int lessonId);
        Task<int?> GetOrderOfLastLessonInSection(int sectionId);
    }
}
