using System;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ToyStorage
{
    public class RequestContext
    {
        public RequestMethods RequestMethod { get; set; }

        public object Entity { get; set; }

        public Type EntityType { get; set; }

        public byte[] Content { get; set; }

        public CloudBlockBlob CloudBlockBlob { get; set; }

        public AccessCondition AccessCondition { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }
}