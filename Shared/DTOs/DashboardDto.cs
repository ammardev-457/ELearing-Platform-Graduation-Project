using ELProject.Domain.Models;

namespace ELProject.Shared.DTOs
{
    public class DashbroadResult
    {
        public List<Enrollment> Enrollments {get;set;} = [];
        public List<Course> CompletedCourses{get;set;} = [];
        public int InProgress {get;set;}

    }

    public class MyCoursesResult
    {
        public List<Course> EnrolledCourses {get;set;} = [];
    }

    public class Profile
    {
        public string? ProfileImage {get;set;}
        public string UserName {get;set;} = null!;
        public string Email {get;set;} = null!;
        public int NumberOfCourses {get;set;}
        public DateTime JoinDate {get;set;}
    }

    public class ProfileEditRequest
    {
        public string? Name {get;set;}
        public string? Email {get;set;}
        public string? Bio {get;set;}

        
    }
}