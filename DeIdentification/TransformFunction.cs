using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json;

namespace DeIdentification
{
    public class TransformFunction
    {
        private ITransformer transformer;
        public TransformFunction(ITransformer transformer)
        {
            this.transformer = transformer;
        }

        [FunctionName("TransformFromStorage_Orchestrator")]
        public async Task<TransformResponse> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            TransformRequest request = context.GetInput<TransformRequest>();

            log.LogInformation($"Start to transform. UTC: { DateTime.UtcNow.ToLongTimeString() }");
            try
            {
                return await context.CallActivityAsync<TransformResponse>("TransformFromStorage", request);
            }
            catch(Exception ex)
            {
                log.LogError($"Transform failed with error: {ex.Message} \r\n {ex.StackTrace}");
                throw;
            }
            finally
            {
                log.LogInformation($"Transform completed. UTC: { DateTime.UtcNow.ToLongTimeString() }");
            }
        }

        [FunctionName("TransformFromStorage")]
        public async Task<TransformResponse> Transform([ActivityTrigger] TransformRequest request, ILogger log)
        {
            return await transformer.TransformAsync(request, log);
        }

        [FunctionName("Transform_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            TransformRequest request = await req.Content.ReadAsAsync<TransformRequest>();
            string instanceId = await starter.StartNewAsync("TransformFromStorage_Orchestrator", request);

            log.LogInformation($"Started transform orchestration with ID = '{instanceId}'. Request = '{ JsonConvert.SerializeObject(request)}' ");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}