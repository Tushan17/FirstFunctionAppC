namespace FirstFunctionApp.Models;

public class XmlUploadRequest
{
    public string XmlContent { get; set; } = string.Empty;
}

public class XmlValidationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? ValidationErrors { get; set; }
    public object? Data { get; set; }
}
