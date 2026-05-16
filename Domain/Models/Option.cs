using System.Text.Json.Serialization;

namespace ELProject.Domain.Models
{
    public class Option
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public int QuestionId { get; set; }

        [JsonIgnore]
        public Question Question { get; set; } = null!;
    }
}
