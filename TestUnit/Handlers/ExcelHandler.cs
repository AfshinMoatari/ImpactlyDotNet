using API.Handlers;
using Xunit;

namespace TestUnit.Handlers;

[Collection("Unit Test")]
public class ExcelHandler
{
    [SetUp]
    public void SetUp()
    {
        
    }

    [Test]
    public void TestCreateExcel()
    {
        var handler = new DataDumpExcelHandler();
    }
}