namespace BlazorApp.Models
{
    [Serializable]
    public class Tag
    {
        public string Name { get; set; }
        public Commit Commit { get; set; }
    }
}
