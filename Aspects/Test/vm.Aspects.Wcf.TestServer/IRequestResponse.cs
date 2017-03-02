using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace vm.Aspects.Wcf.TestServer
{
    [ServiceContract]
    public interface IRequestResponse
    {
        [OperationContract]
        [WebGet(UriTemplate = "?n={numberOfStrings}")]
        ICollection<string> GetStrings(int numberOfStrings);
    }
}
