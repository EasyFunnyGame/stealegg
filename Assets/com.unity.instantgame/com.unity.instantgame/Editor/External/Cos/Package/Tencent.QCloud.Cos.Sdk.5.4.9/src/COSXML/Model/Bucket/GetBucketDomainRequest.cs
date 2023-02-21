using COSXML.Common;

namespace COSXML.Model.Bucket
{
    public sealed class GetBucketDomainRequest : BucketRequest
    {
        public GetBucketDomainRequest(string bucket) : base(bucket)
        {
            this.method = CosRequestMethod.GET;
            this.queryParameters.Add("domain", null);
        }
    }
}
