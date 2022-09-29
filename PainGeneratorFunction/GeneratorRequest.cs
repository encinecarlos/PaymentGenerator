using System.Collections.Generic;

namespace PainGeneratorFunction
{
    public class GeneratorRequest
    {
        public IList<GenericPayments> Payments { get; set; }
        public bool EncodeResult { get; set; }
    }
}
