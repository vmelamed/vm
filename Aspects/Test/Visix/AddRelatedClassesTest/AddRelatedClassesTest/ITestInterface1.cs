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
    interface ITestInterface1
    {
        /// <summary>
        /// Fooes this instance.
        /// </summary>
        [OperationContract]
        void Foo();

        /// <summary>
        /// Bars this instance.
        /// </summary>
        /// <returns>System.Int32.</returns>
        [OperationContract]
        int Bar();

        /// <summary>
        /// Tars the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="i">The i.</param>
        /// <returns>System.String.</returns>
        [OperationContract]
        string Tar(
            [NotNullValidator]
            string a, 
            [NonnegativeValidator]
            int i);
    }
}
