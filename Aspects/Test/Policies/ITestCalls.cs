using System.Threading.Tasks;

namespace vm.Aspects.Policies.Tests
{
    interface ITestCalls
    {
        void Test1();

        int Test2();

        int Test3(string i);

        Task AsyncTest1();

        Task<int> AsyncTest2();

        Task<int> AsyncTest3(string i);
    }
}
