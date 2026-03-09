using System.Text.Json.Serialization;

namespace ELProject.Shared.DTOs.Student
{
    public class StudentDashboardDto 
    {
        // Student.Enrollment.Count
        public int EnrollmentCourses {get; set;}
        // Student.Enrollment.CountAsync(e => e.Compeleted)
        public int Completed {get;set;}
        // Student.Enrollment.CountAsync(e => e.Progress > 0)
        public int InProgressCount {get;set;}
        // calculate watch hours
        public int LearningHours {get; set;}
        /* Student.Enrollment.Courses.Select(c => new DashboardCourse{
            CourseName = c.Title,
            PictureUrl = c.PictureUrl,
            InstructorName = c.Instructor.Name,
            Progress = Student.Enrollment.Progress
        })

        */
        public IReadOnlyList<StudentDashboardCourse>  Courses {get;set;} = [];
    }

    public class StudentDashboardCourse
    {
        public required string CourseName {get;set;}
        public string? PictureUrl {get;set;}
        public required string InstructorName {get;set;} 
        public int Progress {get;set;}
    }
}