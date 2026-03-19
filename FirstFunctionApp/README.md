# XML Upload and Validation System

This Azure Function application provides XML file upload with schema validation and JSON response generation.

## Features

- ✅ XML Schema (XSD) validation
- ✅ XML to C# model deserialization
- ✅ JSON response generation
- ✅ Comprehensive error handling
- ✅ Structured logging

## Project Structure

```
FirstFunctionApp/
├── Function1.cs              # Azure Functions endpoints
├── Models/
│   ├── XmlModels.cs          # XML data models (DataRoot, Company, Employee, etc.)
│   └── ApiModels.cs          # API request/response models
├── Services/
│   ├── XmlValidationService.cs    # XSD schema validation
│   └── XmlProcessingService.cs    # XML deserialization
├── Schemas/
│   └── DataSchema.xsd        # XSD schema definition
└── SampleData/
    └── SampleData.xml        # Sample XML for testing
```

## API Endpoints

### 1. UploadXml (POST)

Validates and processes XML content against the schema.

**Endpoint:** `/api/UploadXml`

**Request Body:**
```json
{
  "xmlContent": "<?xml version=\"1.0\" encoding=\"utf-8\"?><DataRoot xmlns=\"http://example.com/data\" version=\"1.0\">...</DataRoot>"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "XML validated and processed successfully",
  "data": {
    "version": "1.0",
    "companies": [
      {
        "companyId": 1,
        "companyName": "Tech Solutions Inc.",
        "address": {
          "street": "123 Main Street",
          "city": "Seattle",
          "state": "WA",
          "zipCode": "98101",
          "country": "USA"
        },
        "employees": {
          "employeeList": [
            {
              "employeeId": 101,
              "firstName": "John",
              "lastName": "Doe",
              "email": "john.doe@techsolutions.com",
              "department": "Engineering",
              "position": "Senior Developer",
              "salary": 95000.00,
              "hireDate": "2020-01-15"
            }
          ]
        }
      }
    ]
  }
}
```

**Error Response (400):**
```json
{
  "success": false,
  "message": "XML validation failed",
  "validationErrors": [
    "Error: The element 'Company' has invalid child element 'InvalidElement'..."
  ]
}
```

## XML Schema Structure

The schema (`DataSchema.xsd`) defines:

- **DataRoot** (root element)
  - Attribute: `version` (required)
  - **Company** (1 or more)
    - **CompanyId** (integer)
    - **CompanyName** (string)
    - **Address**
      - **Street**, **City**, **State**, **ZipCode**, **Country**
    - **Employees**
      - **Employee** (0 or more)
        - **EmployeeId**, **FirstName**, **LastName**
        - **Email**, **Department**, **Position**
        - **Salary** (decimal), **HireDate** (date)

## Testing

### Using cURL:

```bash
curl -X POST http://localhost:7071/api/UploadXml \
  -H "Content-Type: application/json" \
  -d "{\"xmlContent\":\"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><DataRoot xmlns=\\\"http://example.com/data\\\" version=\\\"1.0\\\"><Company><CompanyId>1</CompanyId><CompanyName>Test Corp</CompanyName><Address><Street>123 Main</Street><City>Seattle</City><State>WA</State><ZipCode>98101</ZipCode><Country>USA</Country></Address><Employees></Employees></Company></DataRoot>\"}"
```

### Using PowerShell:

```powershell
$xmlContent = Get-Content "FirstFunctionApp\SampleData\SampleData.xml" -Raw
$body = @{
    xmlContent = $xmlContent
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/UploadXml" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### Using Postman:

1. Method: POST
2. URL: `http://localhost:7071/api/UploadXml`
3. Headers: `Content-Type: application/json`
4. Body (raw JSON):
```json
{
  "xmlContent": "<?xml version=\"1.0\" encoding=\"utf-8\"?><DataRoot xmlns=\"http://example.com/data\" version=\"1.0\">...</DataRoot>"
}
```

## Running Locally

1. Open the project in Visual Studio
2. Press F5 to start debugging
3. The function will be available at `http://localhost:7071`
4. Test using the sample XML in `SampleData/SampleData.xml`

## Customizing the Schema

To modify the XML structure:

1. Update `Schemas/DataSchema.xsd` with your desired structure
2. Update the model classes in `Models/XmlModels.cs` to match
3. Rebuild the project

## Error Handling

The system handles:
- Missing or empty request body
- Invalid JSON in request
- XML parsing errors
- Schema validation errors
- Deserialization failures

All errors return appropriate HTTP status codes and descriptive error messages.
