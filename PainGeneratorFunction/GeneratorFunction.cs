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
            int payments, ILogger log)
        {
            log.LogInformation($"payment(s) to generate on this request: {payments}.");

            var generator = new PainGenerator().Generate(payments);

            return new OkObjectResult(generator);
        }
    }
}
