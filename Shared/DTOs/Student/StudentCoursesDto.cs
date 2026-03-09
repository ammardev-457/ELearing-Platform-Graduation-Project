namespace ELProject.Shared.DTOs.Student
{
    public class StudentCoursesDto
    {
        public required string Title {get; set;}
        public required string Category {get;set;}
        public required string Thumbnail {get;set;}
        public double Rate {get; set;}
        public int LessonsCount {get; set;}
        public int Hours {get; set;}
        public int Progress {get; set;}
    }
}