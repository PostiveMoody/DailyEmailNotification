namespace DailyEmailNotification.Infrastructure.AI
{
    public class OllamaSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";

        public string Model { get; set; } = "qwen3.5:4b";
    }
}
