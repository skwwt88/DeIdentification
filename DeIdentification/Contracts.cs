using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeIdentification
{
    public class TransformRequest
    {
        public string InputContainer { get; set; }

        public string InputFileName { get; set; }

        public string OutputContainer { get; set; }
    }

    public class TransformResponse
    {
        public string OutputContainer { get; set; }

        public string OutputFileName { get; set; }
    }

    public interface ITransformer
    {
        Task<TransformResponse> TransformAsync(TransformRequest request, ILogger log);
    }
}
