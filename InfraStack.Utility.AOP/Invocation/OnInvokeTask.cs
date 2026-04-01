using System;
using System.Linq;
using System.Threading.Tasks;

namespace InfraStack.Utility.AOP
{
    internal class OnInvokeTask : IOnInvoke
    {
        public void Run(OnInvokeModel Model)
        {
            try
            {
                Model.OnEntry();
                if (Model.Args.FlowBehavior == MethodFlowBehavior.Return)
                {
                    Model.SetReturn(OnEntryReturn(Model));
                    Model.OnFinally();
                    return;
                }

                Model.Proceed();
                Model.SetReturn(OnProceeded(Model));
            }
            catch (Exception Ex)
            {
                Model.SetReturn(OnExceptionBeforeProceed(Model, Ex));
            }
        }

        protected virtual Task OnEntryReturn(OnInvokeModel Model) => Task.CompletedTask;

        protected virtual Task OnExceptionBeforeProceed(OnInvokeModel Model, Exception Ex) =>
            Model.OnExceptionAsync(Ex).ContinueWith(async t =>
            {
                try
                {
                    var (IsOk, Exception) = await GetStatusAsync(t);
                    if (!IsOk) throw Exception!;
                    if (Model.Args.FlowBehavior != MethodFlowBehavior.Continue) throw Model.Args.Exception!;
                }
                finally
                {
                    await Model.OnFinallyAsync();
                }
            }).Unwrap();

        protected virtual Task OnProceeded(OnInvokeModel Model) =>
            ((Task)Model.Args.ReturnValue!).ContinueWith(async t =>
            {
                try
                {
                    var (IsOk, Exception) = await GetStatusAsync(t);
                    if (!IsOk)
                    {
                        await Model.OnExceptionAndCheckFlowAsync(Exception!);
                        return;
                    }

                    try
                    {
                        await Model.OnSuccessAsync();
                    }
                    catch (Exception Ex)
                    {
                        await Model.OnExceptionAndCheckFlowAsync(Ex);
                    }
                }
                finally
                {
                    await Model.OnFinallyAsync();
                }
            }).Unwrap();

        protected static async Task<(bool IsOk, Exception? Exception)> GetStatusAsync(Task Task)
        {
            if (Task.IsCanceled)
            {
                try
                {
                    await Task;
                }
                catch (Exception e)
                {
                    return (false, e);
                }

                throw new NotImplementedException();
            }
            else if (Task.IsFaulted)
            {
                return (false, Task.Exception!.InnerException);
            }
            else
            {
                return (true, null);
            }
        }
    }
}
