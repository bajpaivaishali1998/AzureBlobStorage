using GithubActionIntergrationApp.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace TableAttach
{

    public static class RunbookTableAttachData
    {
        static string GetLongDayName(string day)
        {
            string longday = "";
            switch (day)
            {
                case "Mon":
                    longday = "Monday";
                    break;
                case "Tues":
                    longday = "Tuesday";
                    break;
                case "Wed":
                    longday = "Wednesday";
                    break;
                case "Thurs":
                    longday = "Thursday";
                    break;
                case "Fri":
                    longday = "Friday";
                    break;
                case "Sat":
                    longday = "Saturday";
                    break;
                case "Sun":
                    longday = "Sunday";
                    break;
                default:
                    break;

            }
            return longday;
        }

        [FunctionName("RunbookTableAttachData")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiRequestBody("application/json", typeof(string), Description = "JSON request body containing")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            LogInfo logInfo;

            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string Storageconn = Environment.GetEnvironmentVariable("Storageconnection");
            CloudStorageAccount storageAcc = CloudStorageAccount.Parse(Storageconn);
            logInfo = new LogInfo();
            logInfo.PartitionKey = "Step: 1, Function Invoke RunbookTableAttachData";
            logInfo.RowKey = Guid.NewGuid().ToString();
            logInfo.Method_Name = "HttpTrigger Function Invvoke";
            logInfo.ReqInput=requestBody;
            LoggerInfo.InsertLogEntity(logInfo);
            UploadLoggerToBlob.AzureBlobStorage(logInfo);

            Response Res_Msg;

            List<string> VM_Names = new List<string>();

            string CTASK = "";

            logInfo = new LogInfo();
            logInfo.PartitionKey = "Step: 2, Starting Provisioning RunbookTableAttachData";
            logInfo.RowKey = Guid.NewGuid().ToString();
            logInfo.Method_Name = "Starting Provisioning";
            LoggerInfo.InsertLogEntity(logInfo);
            UploadLoggerToBlob.AzureBlobStorage(logInfo);


            try
            {

                Rootobject data = JsonConvert.DeserializeObject<Rootobject>(requestBody);

                List<Item> data_list = new List<Item>();

                foreach (var item in data.Vmdetails)
                {
                    var _hschedules = data.Hibernation_Schedule.Split(",");
                    Item _item = new Item();
                    _item.RITM = data.RITM;
                    _item.CTASK = data.CTASK;
                    _item.Schedule = data.Schedule;
                    _item.Hibernation_Schedule = data.Hibernation_Schedule;
                    _item.vm_name = item.VM_Name;
                    _item.rg_Name = item.Resource_Group;
                    _item.subscription_Name = item.Subscription_Name;
                    data_list.Add(_item);
                }

                logInfo = new LogInfo();
                logInfo.PartitionKey = "Step: 2, Adding Data to data_list RunbookTableAttachData";
                logInfo.RowKey = Guid.NewGuid().ToString();
                logInfo.Method_Name = "Adding Data to data_list";
                logInfo.ReqInput = data.ToString();
                logInfo.ResOutput = data_list.ToString();
                LoggerInfo.InsertLogEntity(logInfo);
                UploadLoggerToBlob.AzureBlobStorage(logInfo);



                List<string> vmnameList = new List<string>();

                foreach (var item in data_list)
                {
                    VMSchedule vMSchedule = new VMSchedule();
                    vMSchedule.VM_Name = item.vm_name;
                    vMSchedule.Resource_Group = item.rg_Name;

                    vmnameList.Add(item.vm_name);

                    string[] schedules = item.Hibernation_Schedule.Split(",");

                    char[] MyChar = { '(', ')' };

                    foreach (string schedule in schedules)
                    {
                        if (schedule.Split(" ").Length > 1)
                        {
                            string _schedule = schedule.Trim();
                            vMSchedule.TimeZone = _schedule.Split(" ")[1];
                            string sched = _schedule.Split(" ")[0].TrimStart(MyChar).TrimEnd(MyChar);
                            vMSchedule.StartDay = GetLongDayName(sched.Split('-')[0]);
                            vMSchedule.StopDay = GetLongDayName(sched.Split('-')[3]);

                            vMSchedule.StartTime = sched.Split('-')[1];

                            vMSchedule.StopTime = sched.Split('-')[2];
                        }
                        else
                        {
                            string sched = schedule.TrimStart(MyChar).TrimEnd(MyChar);
                            vMSchedule.StartDay = GetLongDayName(sched.Split('-')[0]);
                            vMSchedule.StopDay = GetLongDayName(sched.Split('-')[3]);
                            vMSchedule.StartTime = sched.Split('-')[1];
                            vMSchedule.StopTime = sched.Split('-')[2];
                            vMSchedule.TimeZone = "NoTimeZone";
                        }

                        vMSchedule.PartitionKey = Guid.NewGuid().ToString();
                        vMSchedule.RowKey = Guid.NewGuid().ToString();
                        vMSchedule.Timestamp = DateTime.UtcNow;

                        Microsoft.Azure.Cosmos.Table.CloudTableClient tableClient = storageAcc.CreateCloudTableClient();
                        Microsoft.Azure.Cosmos.Table.CloudTable cloudTable = tableClient.GetTableReference(vMSchedule.TimeZone + vMSchedule.StartDay + "Schedule");
                        cloudTable.CreateIfNotExists();
                        var operation = Microsoft.Azure.Cosmos.Table.TableOperation.Insert(vMSchedule);
                        await cloudTable.ExecuteAsync(operation);
                    }
                }

                logInfo = new LogInfo();
                logInfo.PartitionKey = "Step: 3, Adding Data to vmnameList and Table RunbookTableAttachData";
                logInfo.RowKey = Guid.NewGuid().ToString();
                logInfo.Method_Name = "Adding Data to List and Table";
                logInfo.ResOutput = vmnameList.ToString();
                logInfo.LogMessage = "Data saved successfully to the Table";
                LoggerInfo.InsertLogEntity(logInfo);
                UploadLoggerToBlob.AzureBlobStorage(logInfo);


                Res_Msg = new Response();
                Res_Msg.VM_Names = String.Join(", ", vmnameList);
                Res_Msg.CTASK = data.CTASK;
                Res_Msg.Response_Message = "Hibernation Schedule is Attached Succesfully";

                var Response = System.Text.Json.JsonSerializer.Serialize(Res_Msg);

                CheckAccess ChkAccessObjSl = new CheckAccess();
                var objResponse = ChkAccessObjSl.ServiceCafeHTTPPosttClient(new Uri(Environment.GetEnvironmentVariable("serviceNowUrl", EnvironmentVariableTarget.Process)), Response);

                var Func_res = "Success";

                logInfo = new LogInfo();
                logInfo.PartitionKey = "Step: 4, Response from ServiceNow RunbookTableAttachData";
                logInfo.RowKey = Guid.NewGuid().ToString();
                logInfo.Method_Name = "Successfully Send Data to ServiceNow";
                logInfo.ReqInput = requestBody;
                logInfo.ResOutput =Func_res+"\n"+ Response;
                logInfo.StatusCode = objResponse;
                logInfo.LogMessage = "Successfully Send to Service Now";
                LoggerInfo.InsertLogEntity(logInfo);
                UploadLoggerToBlob.AzureBlobStorage(logInfo);


                return new OkObjectResult(Func_res);
            }

            catch (Exception ex)
            {
                Res_Msg = new Response();

                Res_Msg.VM_Names = VM_Names.ToString();
                Res_Msg.CTASK = CTASK.ToString();
                Res_Msg.Response_Message = ex.Message;

                var Response = System.Text.Json.JsonSerializer.Serialize(Res_Msg);

                log.LogInformation(ex.Message);

                logInfo = new LogInfo();
                logInfo.PartitionKey = "Error: RunbookTableAttachData";
                logInfo.RowKey = Guid.NewGuid().ToString();
                logInfo.Method_Name = "Error: When Run the RunbookTableAttachData";
                logInfo.ReqInput = requestBody;
                logInfo.ErrorException = ex.ToString();
                LoggerInfo.InsertLogEntity(logInfo);
                UploadLoggerToBlob.AzureBlobStorage(logInfo);


                return new BadRequestObjectResult(Response);
            }
        }
    }
   

    public class Item
    {
        public string RITM { get; set; }
        public string CTASK { get; set; }
        public string Schedule { get; set; }
        public string Hibernation_Schedule { get; set; }
        public string vm_name { get; set; }
        public string rg_Name { get; set; }
        public string subscription_Name { get; set; }


    }

    public class VMSchedule : Microsoft.Azure.Cosmos.Table.TableEntity
    {
        public string VM_Name { get; set; }
        public string Resource_Group { get; set; }
        public string TimeZone { get; set; }
        public string StartDay { get; set; }
        public string StopDay { get; set; }
        public string StartTime { get; set; }
        public string StopTime { get; set; }
        public string Subscription_Name { get; set; }
    }

    public class Rootobject
    {
        public string RITM { get; set; }
        public string CTASK { get; set; }
        public string Schedule { get; set; }
        public string Hibernation_Schedule { get; set; }
        public Vmdetail[] Vmdetails { get; set; }
    }

    public class Vmdetail
    {
        public string VM_Name { get; set; }
        public string Resource_Group { get; set; }
        public string Subscription_Name { get; set; }
    }

    public class Response
    {
        public string CTASK { get; set; }
        public string VM_Names { get; set; }
        public string Response_Message { get; set; }
    }
}

