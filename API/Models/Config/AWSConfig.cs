namespace API.Models.Config
{
    public class AWSConfig
    {
        public string BucketName { get; set; }
        public string DynamoDbUrl { get; set; }

        public string ElasticUrl { get; set; }
        public bool LocalMode { get; set; } = false;
        public string LockTableName { get; set; }
    }
}