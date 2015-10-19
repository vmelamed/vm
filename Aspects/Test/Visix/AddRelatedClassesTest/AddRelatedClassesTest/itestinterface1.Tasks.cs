using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AddRelatedClassesTest
{
    /// <summary>
    /// Interface ITestInterface1 blah, blah
    /// </summary>
    [ServiceContract(Namespace="urn:vm.Test")]
    internal interface ITestInterface1Tasks
    {
        /// <summary>
        /// Fooes this instance.
        /// </summary>
        [OperationContract(
            Action="urn:vm.Test/ITestInterface1/Foo",
            ReplyAction="urn:vm.Test/ITestInterface1/FooResponse")]
        Task FooAsync();

        /// <summary>
        /// Bars this instance.
        /// </summary>
        /// <returns>System.Int32.</returns>
        [OperationContract(
            Action="urn:vm.Test/ITestInterface1/Bar",
            ReplyAction="urn:vm.Test/ITestInterface1/BarResponse")]
        Task<int> BarAsync();

        /// <summary>
        /// Tars the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="i">The i.</param>
        /// <returns>System.String.</returns>
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
