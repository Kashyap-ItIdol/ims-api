using IMS_Domain.Entities;

public class ClientAssetAttachment
{
    public int Id { get; set; }

    public int ClientAssetId { get; set; }
    public ClientAsset ClientAsset { get; set; }

    public string FileName { get; set; }
    public string FilePath { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
