using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Models.Dump;

public class ImportPatientRequest
{



    public DateTime? BirthDate { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public string LastName { get; set; }
    public string Municipality { get; set; }
    
    public string Region { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string PostalNumber { get; set; }
    public string Sex { get; set; }
    public string Strategy { get; set; }
    public string Tags { get; set; }
    



    public List<string> TagsToList()
    { 
        var tags = Tags.Split(',');
        return tags.ToList();
    }
}