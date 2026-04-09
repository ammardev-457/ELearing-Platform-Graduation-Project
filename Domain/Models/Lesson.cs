using ELProject.Domain.Enums;

namespace ELProject.Domain.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        
        public string Title { get; set; } = null!;
        public int Order { get; set; } // ترتيب الدرس في السكشن
        public bool IsFreePreview { get; set; }

        // 1. تحديد نوع المحتوى
        public LessonType Type { get; set; } 

        // ==========================================
        // 2. المحتوى الخفيف (Simple Content)
        // ==========================================
        
        // Video لو النوع
        public string? VideoUrl { get; set; }       //  رابط الفيديو (S3/Azure)
        public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Pending; // حالة المعالجة
        public int? DurationInSeconds { get; set; } // مدة الفيديو

        // File (PDF/Zip) لو النوع
        public string? AttachmentUrl { get; set; }  // رابط التحميل

        // ==========================================
        // 3. المحتوى المعقد (Complex Content)
        // ==========================================
        
        // Quiz لو النوع
        public int? QuizId { get; set; } // Foreign Key (Optional)
        public Quiz? Quiz { get; set; }  // Navigation Property

        // Foreign Keys for Hierarchy
        public int SectionId { get; set; }
        public Section Section { get; set; } = null!;
    }

}