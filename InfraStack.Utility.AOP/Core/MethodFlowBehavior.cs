namespace InfraStack.Utility.AOP
{
    public enum MethodFlowBehavior
    {
        /// <summary>
        /// 初始值
        /// </summary>
        Default,

        /// <summary>
        /// 表示方法執行過程遇到例外
        /// </summary>
        ThrowException,

        /// <summary>
        /// 在`OnEntry`時, 假如改為此值, 就不會執行方法, 而是提早回傳
        /// </summary>
        Return,

        /// <summary>
        /// 在`OnException`時, 假如改為此值, 就不會抛出例外
        /// </summary>
        Continue
    }
}
