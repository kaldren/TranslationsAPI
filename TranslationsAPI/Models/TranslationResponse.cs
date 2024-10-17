namespace TranslationsAPI.Models;

//{
//    "id": "ac196b64-99dc-42ab-b41d-78875de735cb",
//    "status": "NotStarted"
//}

public class TranslationResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public string Location { get; set; }
}