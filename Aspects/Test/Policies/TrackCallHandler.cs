using System.Threading.Tasks;

using Microsoft.Practices.Unity.InterceptionExtension;

using vm.Aspects.Facilities;

namespace vm.Aspects.Policies.Tests
{
    class TrackCallHandler : BaseCallHandler<bool>
    {
        protected override bool Prepare(
            IMethodInvocation input)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Prepare");
            return base.Prepare(input);
        }

        protected override IMethodReturn PreInvoke(
            IMethodInvocation input,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Pre-invoke");
            base.PreInvoke(input, callData);
            return null;
        }

        protected override IMethodReturn DoInvoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Do-invoke");
            return base.DoInvoke(input, getNext, callData);
        }

        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Post-invoke");
            return base.PostInvoke(input, methodReturn, callData);
        }

        protected override Task<TResult> ContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: ContinueWith");
            return base.ContinueWith<TResult>(input, methodReturn, callData);
        }

        protected override Task DoContinueWith(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Task DoContinueWith");
            return base.DoContinueWith(input, methodReturn, callData);
        }

        protected override Task<TResult> DoContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData,
            TResult result)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Task<TResult> DoContinueWith");
            return base.DoContinueWith<TResult>(input, methodReturn, callData, result);
        }
    }
}
