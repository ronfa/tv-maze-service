using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using CodingChallenge.Cdk.Extensions;
using CodingChallenge.Infrastructure.Extensions;
using CodingChallenge.Infrastructure.Models;
using CodingChallenge.Infrastructure.Persistence.TvMaze;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace CodingChallenge.Cdk.Stacks;

public sealed class DatabaseStack : Stack
{
    public const string arnSuffixValue = "arn";

    public DatabaseStack(Construct parent, string id, IStackProps props, AWSAppProject awsApplication) : base(parent,
        id, props)
    {
        SetupTableForTVMazeRecord(awsApplication);
    }

    public void SetupTableForTVMazeRecord(AWSAppProject awsApplication)
    {
        var dynamoDBTableFullName = awsApplication.GetDynamodbTableName(typeof(TvMazeShowRecordDataModel));
        var dynamoDbTableProps = new TableProps
        {
            TableName = dynamoDBTableFullName,
            BillingMode = BillingMode.PAY_PER_REQUEST
        };
        dynamoDbTableProps.PartitionKey = new Attribute
        {
            Type = AttributeType.NUMBER,
            Name = nameof(TvMazeShowRecordDataModel.TVMazeIndex)
        };

        // dynamoDbTableProps.SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute()
        // {
        //     Type = Amazon.CDK.AWS.DynamoDB.AttributeType.STRING,
        //     Name = nameof(Infrastructure.Persistence.TVMazeRecord.TvMazeShowRecordDataModel.TVMazeIndex)
        // };

        var table = new Table(this, dynamoDBTableFullName, dynamoDbTableProps);
        awsApplication.SetCfOutput(this, $"{typeof(TvMazeShowRecordDataModel).Name.ToLower()}-{arnSuffixValue}",
            table.TableArn);
    }
}