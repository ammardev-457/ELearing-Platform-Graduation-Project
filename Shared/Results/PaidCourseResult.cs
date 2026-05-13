using ELProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ELProject.Shared.Results
{
    public class PaidCourseResult
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime CreatedDate { get; set; }
        public CourseLevel? Level { get; set; }
        public decimal Price { get; set; }
        public string InstructorId { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public IEnumerable<SectionResult> Sections { get; set; } = [];
    }

    public class SectionResult
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public IEnumerable<LessonResult> Lessons { get; set; } = [];
    }

    public class LessonResult
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public FileType Type { get; set; }
        public int DurationInSeconds { get; set; }
    }
}
