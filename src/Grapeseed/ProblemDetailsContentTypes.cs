namespace Grapevine
{
    public interface IProblemDetailsContentTypes
    {
        ContentType Json { get; }
        ContentType Xml { get; }
    }

    internal class ProblemDetailsContentTypes : IProblemDetailsContentTypes
    {
        public ContentType Json { get; } = new ContentType("application/problem+json", "", false, "UTF-8");

        public ContentType Xml { get; } = new ContentType("application/problem+xml", "", false, "UTF-8");
    }
}
