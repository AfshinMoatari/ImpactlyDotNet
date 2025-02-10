using System;
using API.Constants;

namespace API.Views;

public class BulkUploadEmail : BaseEmail
{
    public string FileName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string Email { get; set; }


    public BulkUploadEmail(ISystemMessage message) : base(message)
    {
        Lines = message.BulkUploadEmailLines();
    }

    public string GetTitle()
    {
        return SystemMessage.BulkUploadTitle(FileName);
    }
    
}