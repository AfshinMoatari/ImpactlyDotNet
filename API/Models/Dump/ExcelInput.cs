using System.Collections.Generic;

namespace API.Models.Dump;

public class ExcelInput
{
    public const string ExcelTypeDataDump = "data dump";
    public const string ExcelTypeImportPatientsLog = "CitizenImportLogs";
    public const string ExcelTypeDataDumpRaw = "data dump raw";
    
    public string Type { set; get; }
    
    public List<ImportPatient> ImportPatients { get; set; }
    
    public DumpRequest DumpRequest { get; set; }
    
    public List<DumpData> DumpDataList { get; set; }
    
    public string ProjectName { get; set; }
    
    public string UserName { get; set; }
}