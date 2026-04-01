using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Serialization;
using System.Threading.Tasks;

namespace InfraStack.Utility.AOP
{
    [PSerializable(AttributeInheritance = MulticastInheritance.Multicast)]
    public abstract class MethodBoundaryAttribute : MethodInterceptionAspect
    {
        protected MethodBoundaryAttribute() => SemanticallyAdvisedMethodKinds = SemanticallyAdvisedMethodKinds.None;

        public virtual Task OnEntryAsync(MethodArgs Args) => Task.CompletedTask;
        public virtual Task OnSuccessAsync(MethodArgs Args) => Task.CompletedTask;
        public virtual Task OnExceptionAsync(MethodArgs Args) => Task.CompletedTask;
        public virtual Task OnExitAsync(MethodArgs Args) => Task.CompletedTask;
        public virtual MethodArgs ConfigureArgs() => new();

        internal async Task OnFinallyAsync(MethodArgs Args)
        {
            await Args.ExecuteFinalActionsAsync();
            await OnExitAsync(Args);
        }

        public override void OnInvoke(MethodInterceptionArgs Interception)
        {
            var Args = ConfigureArgs();
            Args.Interception = Interception;
            IOnInvoke.Create(Args.Method.ReturnType)
                .Run(new OnInvokeModel(this, Args, Interception));
        }
    }
}
