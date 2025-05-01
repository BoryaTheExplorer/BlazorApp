namespace BlazorApp.Models
{
    public class TagGetResult
    {
        public bool IsSuccess { get; set; }
        public List<Tag>? Tags { get; set; }
        public string? ErrorMessage { get; set; }

        public static TagGetResult Success(List<Tag> tags)
        {
            return new TagGetResult
            {
                IsSuccess = true,
                Tags = tags
            };
        }
        public static TagGetResult Fail(string message)
        {
            return new TagGetResult
            {
                IsSuccess = false,
                ErrorMessage = message
            };
        }
    }
}
