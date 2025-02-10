using System;
using API.Constants;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Views;

public class DataDumpEmail : BaseEmail
{
    public string UserName { get; set; }
    public string ProjectName { get; set; }
    public string ExportType { get; set; }
    public string FileName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public override string GetThisClassName()
    {
        return GetType().Name;
    }

    public DataDumpEmail(ISystemMessage message) : base(message)
    {
        Lines = message.DataDumpEmailLines();
    }

    

}