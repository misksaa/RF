using System.Collections.Generic;

namespace RegistrationApp.Domain.Entities;

public class Governorate
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<City> Cities { get; set; } = new List<City>();
}
