using System.Xml.Serialization;

namespace FirstFunctionApp.Models;

[XmlRoot("DataRoot", Namespace = "http://example.com/data")]
public class DataRoot
{
    [XmlAttribute("version")]
    public string Version { get; set; } = string.Empty;

    [XmlElement("Company")]
    public List<Company> Companies { get; set; } = new();
}

public class Company
{
    [XmlElement("CompanyId")]
    public int CompanyId { get; set; }

    [XmlElement("CompanyName")]
    public string CompanyName { get; set; } = string.Empty;

    [XmlElement("Address")]
    public Address Address { get; set; } = new();

    [XmlElement("Employees")]
    public Employees Employees { get; set; } = new();
}

public class Address
{
    [XmlElement("Street")]
    public string Street { get; set; } = string.Empty;

    [XmlElement("City")]
    public string City { get; set; } = string.Empty;

    [XmlElement("State")]
    public string State { get; set; } = string.Empty;

    [XmlElement("ZipCode")]
    public string ZipCode { get; set; } = string.Empty;

    [XmlElement("Country")]
    public string Country { get; set; } = string.Empty;
}

public class Employees
{
    [XmlElement("Employee")]
    public List<Employee> EmployeeList { get; set; } = new();
}

public class Employee
{
    [XmlElement("EmployeeId")]
    public int EmployeeId { get; set; }

    [XmlElement("FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [XmlElement("LastName")]
    public string LastName { get; set; } = string.Empty;

    [XmlElement("Email")]
    public string Email { get; set; } = string.Empty;

    [XmlElement("Department")]
    public string Department { get; set; } = string.Empty;

    [XmlElement("Position")]
    public string Position { get; set; } = string.Empty;

    [XmlElement("Salary")]
    public decimal Salary { get; set; }

    [XmlElement("HireDate")]
    public DateTime HireDate { get; set; }
}
