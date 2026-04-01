using System;
using System.Threading.Tasks;

namespace InfraStack.Utility.AOP
{
    internal class OnInvokeTaskResult<ResultT> : OnInvokeTask
    {
        protected override Task<ResultT> OnEntryReturn(OnInvokeModel Model) =>
            Task.FromResult((ResultT)Model.Args.ReturnValue!);

        protected override Task<ResultT> OnExceptionBeforeProceed(OnInvokeModel Model, Exception Ex) =>
            Model.OnExceptionAsync(Ex).ContinueWith(async t =>
            {
                try
                {
                    var (IsOk, Exception) = await GetStatusAsync(t);
                    if (!IsOk) throw Exception!;
                    if (Model.Args.FlowBehavior != MethodFlowBehavior.Continue) throw Model.Args.Exception!;

                    return (ResultT)Model.Args.ReturnValue!;
                }
                finally
                {
                    await Model.OnFinallyAsync();
                }
            }).Unwrap();

        protected override Task<ResultT> OnProceeded(OnInvokeModel Model) =>
            ((Task<ResultT>)Model.Args.ReturnValue!).ContinueWith(async t =>
            {
                try
                {
                    var (IsOk, Exception) = await GetStatusAsync(t);
                    if (!IsOk)
                    {
                        await Model.OnExceptionAndCheckFlowAsync(Exception!);
                        return (ResultT)Model.Args.ReturnValue!;
                    }

                    Model.Args.ReturnValue = t.Result;
                    try
                    {
                        await Model.OnSuccessAsync();
                    }
                    catch (Exception Ex)
                    {
                        await Model.OnExceptionAndCheckFlowAsync(Ex);
                    }
                    return (ResultT)Model.Args.ReturnValue!;
                }
                finally
                {
                    await Model.OnFinallyAsync();
                }
            }).Unwrap();
    }
}
