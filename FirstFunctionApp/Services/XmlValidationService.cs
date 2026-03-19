using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace FirstFunctionApp.Services;

public class XmlValidationService
{
    private readonly XmlSchemaSet _schemaSet;
    private readonly List<string> _validationErrors;

    public XmlValidationService(string schemaPath)
    {
        _schemaSet = new XmlSchemaSet();
        _validationErrors = new List<string>();

        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException($"Schema file not found: {schemaPath}");
        }

        _schemaSet.Add(XmlSchema.Read(XmlReader.Create(schemaPath), null));
        _schemaSet.Compile();
    }

    public (bool IsValid, List<string> Errors) ValidateXml(string xmlContent)
    {
        _validationErrors.Clear();

        try
        {
            var document = XDocument.Parse(xmlContent);
            document.Validate(_schemaSet, ValidationCallback);

            return (_validationErrors.Count == 0, _validationErrors);
        }
        catch (Exception ex)
        {
            _validationErrors.Add($"XML parsing error: {ex.Message}");
            return (false, _validationErrors);
        }
    }

    private void ValidationCallback(object? sender, ValidationEventArgs e)
    {
        if (e.Severity == XmlSeverityType.Warning)
        {
            _validationErrors.Add($"Warning: {e.Message}");
        }
        else if (e.Severity == XmlSeverityType.Error)
        {
            _validationErrors.Add($"Error: {e.Message}");
        }
    }
}
