using System.Text.Json.Serialization;

namespace ExamPrep2.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("foodId")]
        public string? FoodId { get; set; }
    }
}
