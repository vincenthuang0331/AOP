using System;

namespace InfraStack.Utility.AOP
{
    internal class OnInvokeSync : IOnInvoke
    {
        public void Run(OnInvokeModel Model)
        {
            try
            {
                Model.OnEntry();
                if (Model.Args.FlowBehavior == MethodFlowBehavior.Return) return;

                Model.Proceed();
                Model.OnSuccess();
            }
            catch (Exception Ex)
            {
                Model.OnExceptionAndCheckFlow(Ex);
            }
            finally
            {
                Model.SetReturn(Model.Args.ReturnValue);
                Model.OnFinally();
            }
        }
    }
}
