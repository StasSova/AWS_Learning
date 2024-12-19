using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

namespace DynamoDb;

internal static class Program
{
    private static async Task Main()
    {
        try
        {
            const string accessKey = "**************";
            const string secretKey = "******************************************";

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var config = new AmazonDynamoDBConfig() { RegionEndpoint = Amazon.RegionEndpoint.EUNorth1 };

            using var client = new AmazonDynamoDBClient(credentials, config);
            const string tableName = "Students";
            var request = new ScanRequest { TableName = tableName };

            var response = await client.ScanAsync(request);
            var studentList = new List<string>();

            while (true)
            {
                studentList.AddRange(response.Items.Select(ConvertItemToText));
                if (response.LastEvaluatedKey is { Count: > 0 })
                {
                    request.ExclusiveStartKey = response.LastEvaluatedKey;
                    response = await client.ScanAsync(request);
                }
                else
                {
                    break;
                }
            }
abdasd
            const string filePath = "Students.txt";
            await File.WriteAllLinesAsync(filePath, studentList, Encoding.UTF8);

            Console.WriteLine($"Student data has been written to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static string ConvertItemToText(Dictionary<string, AttributeValue> item)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Student Information:");

        foreach (var key in item.Keys)
        {
            var value = item[key];
            sb.AppendLine($"{key}: {GetValue(value)}");
        }

        sb.AppendLine(new string('-', 30));
        return sb.ToString();
    }

    private static string? GetValue(AttributeValue value)
    {
        if (value.S != null) return value.S;
        if (value.N != null) return value.N;
        if (value.BOOL != null) return value.BOOL.ToString();
        if (value.L != null) return string.Join(", ", value.L.Select(GetValue));
        return value.M != null
            ? string.Join(", ", value.M.Select(kvp => $"{kvp.Key}: {GetValue(kvp.Value)}"))
            : "Unknown";
    }
}
