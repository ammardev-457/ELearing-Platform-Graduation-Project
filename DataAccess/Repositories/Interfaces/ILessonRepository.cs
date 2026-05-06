using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface ILessonRepository : IRepository<Lesson, int>
    {
        Task<IEnumerable<Lesson>> GetLessonsBySectionId(int sectionId);
    }
}
