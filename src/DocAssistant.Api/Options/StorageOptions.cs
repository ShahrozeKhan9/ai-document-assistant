namespace DocAssistant.Api.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Folder (relative to the project) where uploaded PDFs are kept.</summary>
    public string RootPath { get; set; } = "storage";

    public int MaxFileSizeMb { get; set; } = 20;
}
