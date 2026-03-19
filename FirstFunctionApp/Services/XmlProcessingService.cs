using System.Xml.Serialization;
using FirstFunctionApp.Models;

namespace FirstFunctionApp.Services;

public class XmlProcessingService
{
    public T? DeserializeXml<T>(string xmlContent) where T : class
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xmlContent);
            return serializer.Deserialize(reader) as T;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize XML: {ex.Message}", ex);
        }
    }

    public DataRoot? ProcessXmlData(string xmlContent)
    {
        return DeserializeXml<DataRoot>(xmlContent);
    }
}
