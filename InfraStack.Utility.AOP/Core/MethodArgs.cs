using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using PostSharp.Aspects;

namespace InfraStack.Utility.AOP
{
    public class MethodArgs
    {
        internal MethodInterceptionArgs Interception { get; set; } = null!;
        public object?[] Arguments => Interception.Arguments.ToArray();
        public object? Instance => Interception.Instance;
        public MethodInfo Method => (MethodInfo)Interception.Method;
        public Exception? Exception { get; set; }
        public MethodFlowBehavior FlowBehavior { get; set; } = MethodFlowBehavior.Default;
        public object? ReturnValue { get; set; }

        private readonly List<Func<Task>> _OnExitActions = new();
        public void OnFinally(Func<Task> Act) => _OnExitActions.Add(Act);

        internal async Task ExecuteFinalActionsAsync()
        {
            foreach (var Act in _OnExitActions) await Act();
        }

        public void Return(object? Result)
        {
            FlowBehavior = FlowBehavior switch
            {
                MethodFlowBehavior.Default => MethodFlowBehavior.Return,
                MethodFlowBehavior.ThrowException => MethodFlowBehavior.Continue,
                _ => MethodFlowBehavior.Return
            };
            ReturnValue = Result;
        }

        public Type? ClassType => Instance != null ? Instance.GetType() : Method.ReflectedType;
    }
}
