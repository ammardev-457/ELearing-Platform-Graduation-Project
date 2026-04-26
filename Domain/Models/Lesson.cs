using ELProject.Domain.Enums;

namespace ELProject.Domain.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        
        public string Title { get; set; } = null!;
        public int Order { get; set; } // ترتيب الدرس في السكشن
        public bool IsFreePreview => Order <= 2;

        // 1. تحديد نوع المحتوى
        public FileType Type { get; set; }

        // pdf او Video لو النوع
        public string? FileUrl { get; set; }       //  رابط الفيديو (S3/Azure)
        public int? DurationInSeconds { get; set; } // مدة الفيديو

        // Quiz لو النوع
        public int? QuizId { get; set; } // Foreign Key (Optional)
        public Quiz? Quiz { get; set; }  // Navigation Property

        // Foreign Keys for Hierarchy
        public int SectionId { get; set; }
        public Section Section { get; set; } = null!;
    }

}