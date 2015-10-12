using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AddRelatedClassesTest
{
    [ServiceContract(Namespace="urn:vm.Test")]
    interface ITestInterface1
    {
        [OperationContract]
        void Foo();

        [OperationContract]
        int Bar();

        [OperationContract]
        string Tar(
            [NotNullValidator]
            string a, 
            [NonnegativeValidator]
            int i);
    }
}
