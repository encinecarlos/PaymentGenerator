using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PainGeneratorFunction.Generator;

namespace PainGeneratorFunction
{
    public static class GeneratorFunction
    {
        [FunctionName("GeneratorFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"Start file generation process.");

            var content = await new StreamReader(req.Body).ReadToEndAsync();

            var payRequest = JsonConvert.DeserializeObject<GeneratorRequest>(content);

            var generator = new PainGenerator().Generate(payRequest);

            return new OkObjectResult(generator);
        }
    }
}
