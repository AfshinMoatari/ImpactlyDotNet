using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using API.Models.Codes;
using API.Models.Reports;

namespace API.Repositories
{
    public interface ICodeContext
    {
        public DynamoDBContext DynamoDbContext { get; }
        
        public ICrudRepository<SurveyCode> SurveyCodes { get; }
        public ICrudRepository<ReportCode> ReportCodes { get; }
    }

    public class CodeContext : ICodeContext
    {
        public DynamoDBContext DynamoDbContext { get; }
        public ICrudRepository<SurveyCode> SurveyCodes { get; }
        public ICrudRepository<ReportCode> ReportCodes { get; }

        public CodeContext(IAmazonDynamoDB client)
        {
            DynamoDbContext = new DynamoDBContext(client);
            SurveyCodes = new SurveyCodeRepository(client);
            ReportCodes = new ReportCodeRepository(client);
        }
        
        public class SurveyCodeRepository : CrudRepository<SurveyCode>
        {
            public override string ParentPrefix => "CODE";
            public override string ModelPrefix => SurveyCode.Prefix;
        
            public SurveyCodeRepository(IAmazonDynamoDB client) : base(client)
            {
            }
        }
        
        public class ReportCodeRepository : CrudRepository<ReportCode>
        {
            public override string ParentPrefix => "CODE";
            public override string ModelPrefix => ReportCode.Prefix;

            public ReportCodeRepository(IAmazonDynamoDB client) : base(client)
            {
            }
        }
    }
}