using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AddRelatedClassesTest
{
    [ServiceContract(Namespace="urn:vm.Test")]
    internal interface ITestInterface1Tasks
    {
        [OperationContract(
            Action="urn:vm.Test/ITestInterface1/Foo",
            ReplyAction="urn:vm.Test/ITestInterface1/FooResponse")]
        Task FooAsync();

        [OperationContract(
            Action="urn:vm.Test/ITestInterface1/Bar",
            ReplyAction="urn:vm.Test/ITestInterface1/BarResponse")]
        Task<int> BarAsync();

        [OperationContract(
            Action="urn:vm.Test/ITestInterface1/Tar",
            ReplyAction="urn:vm.Test/ITestInterface1/TarResponse")]
        Task<string> TarAsync(

            [NotNullValidator]
            string a,

            [NonnegativeValidator]
            int i);
    }
}
