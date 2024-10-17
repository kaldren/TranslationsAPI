namespace TranslationsAPI.Models;
public class TranslationRequest
{
    public List<Input> Inputs { get; set; }

    public class Input
    {
        public Source Source { get; set; }
        public List<Target> Targets { get; set; }
        public string StorageType { get; set; }
    }

    public class Source
    {
        public string SourceUrl { get; set; }
        public Filter Filter { get; set; }
        public string Language { get; set; }
        public string StorageSource { get; set; }
    }

    public class Filter
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }
    }

    public class Target
    {
        public string TargetUrl { get; set; }
        public string Category { get; set; }
        public string Language { get; set; }
        public List<Glossary> Glossaries { get; set; }
        public string StorageSource { get; set; }
    }

    public class Glossary
    {
        public string GlossaryUrl { get; set; }
        public string Format { get; set; }
        public string Version { get; set; }
        public string StorageSource { get; set; }
    }
}