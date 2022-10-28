using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableAttach
{
    internal class LoggerInfo
    {
        public static string InsertLogEntity(LogInfo loginfo)
        {
            string cnn = Environment.GetEnvironmentVariable("Storageconnection");
            string tabel = "AttachLogs";

            if (loginfo != null)
            {
                try
                {
                    LogInfo logEntity = new LogInfo();
                    logEntity.PartitionKey = loginfo.PartitionKey;  
                    logEntity.RowKey = loginfo.RowKey;
                    logEntity.Method_Name = loginfo.Method_Name;
                    logEntity.ReqInput = loginfo.ReqInput;
                    logEntity.ResOutput = loginfo.ResOutput;
                    logEntity.ErrorException = loginfo.ErrorException;
                    logEntity.LogMessage = loginfo.LogMessage;

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cnn);
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    CloudTable cloudTable = tableClient.GetTableReference(tabel);
                    TableOperation operation = TableOperation.Insert(logEntity);
                    cloudTable.ExecuteAsync(operation);

                    return "Record Saved";
                }
                catch (Exception ex)
                {
                    return "Error: " + Convert.ToString(ex);
                }
            }
            else
            {
                return "loginfo is empty! Rercord not saved.";
            }           
        }
    }

    public class LogInfo : TableEntity
    {
        public string Method_Name { get; set; }     
        public string ReqInput { get; set; }
        public string ResOutput { get; set; }
        public string ErrorException { get; set; }
        public string LogMessage { get; set; }
        public string StatusCode { get; set; }

        public LogInfo()
        {
            Method_Name = "NA"; ReqInput = "NA"; ResOutput = "NA"; ErrorException = "NA"; LogMessage = "NA"; StatusCode = "NA";
        }
    }
}
