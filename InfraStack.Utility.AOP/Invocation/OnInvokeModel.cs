using System;
using System.Threading.Tasks;
using PostSharp.Aspects;

namespace InfraStack.Utility.AOP
{
    internal record OnInvokeModel(
        MethodBoundaryAttribute Attr,
        MethodArgs Args,
        MethodInterceptionArgs Interception)
    {
        public void SetReturn(object? Value) => Interception.ReturnValue = Value;
        public void OnEntry() => Attr.OnEntryAsync(Args).GetAwaiter().GetResult();
        public void OnSuccess() => OnSuccessAsync().GetAwaiter().GetResult();
        public Task OnSuccessAsync() => Attr.OnSuccessAsync(Args);
        public Task OnExceptionAsync(Exception Ex)
        {
            Args.Exception = Ex;
            Args.FlowBehavior = MethodFlowBehavior.ThrowException;
            return Attr.OnExceptionAsync(Args);
        }

        public void OnExceptionAndCheckFlow(Exception Ex) => OnExceptionAndCheckFlowAsync(Ex).GetAwaiter().GetResult();

        public async Task OnExceptionAndCheckFlowAsync(Exception Ex)
        {
            await OnExceptionAsync(Ex);
            if (Args.FlowBehavior == MethodFlowBehavior.Continue) return;
            throw Args.Exception!;
        }

        public Task OnFinallyAsync() => Attr.OnFinallyAsync(Args);
        public void OnFinally() => OnFinallyAsync().GetAwaiter().GetResult();

        public void Proceed()
        {
            Interception.Proceed();
            Args.ReturnValue = Interception.ReturnValue;
        }
    }
}
