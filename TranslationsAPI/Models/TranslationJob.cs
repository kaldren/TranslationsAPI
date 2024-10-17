namespace TranslationsAPI.Models;

//{
//    "id": "8ec87ed4-6b73-4f63-ade2-88984cf26d19",
//    "createdDateTimeUtc": "2024-10-16T15:41:01.3106662Z",
//    "lastActionDateTimeUtc": "2024-10-16T15:41:06.7644504Z",
//    "status": "Succeeded",
//    "summary": {
//        "total": 5,
//        "failed": 0,
//        "success": 5,
//        "inProgress": 0,
//        "notYetStarted": 0,
//        "cancelled": 0,
//        "totalCharacterCharged": 7430
//    }
//}

public class TranslationJob
{
    public Guid Id { get; set; }
    public DateTime CreatedDateTimeUtc { get; set; }
    public DateTime LastActionDateTimeUtc { get; set; }
    public string Status { get; set; }
    public Summary Summary { get; set; }
}

public class Summary
{
    public int Total { get; set; }
    public int Failed { get; set; }
    public int Success { get; set; }
    public int InProgress { get; set; }
    public int NotYetStarted { get; set; }
    public int Cancelled { get; set; }
    public int TotalCharacterCharged { get; set; }
}