namespace ELProject.DataAccess.Results
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
