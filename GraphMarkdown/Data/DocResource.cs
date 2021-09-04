namespace GraphMarkdown.Data
{
    public class DocResource
    {
        public string ResourceName { get; set; }
        public string SourceFile { get; set; }
        public bool IsBeta { get; set; }
        public string PropertiesMarkdown { get; set; }
    }
}
