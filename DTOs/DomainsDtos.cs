namespace IDMSBackend.DTOs;

public class CreateDomainsDtos
{
    public string Name { get; set; } = string.Empty;
}

public  class ResponseDomainsDtos
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

