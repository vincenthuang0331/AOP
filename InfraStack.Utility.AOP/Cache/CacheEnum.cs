namespace InfraStack.Utility.AOP
{
    /// <summary>
    /// 快取策略列舉
    /// </summary>
    public enum CacheEnum
    {
        ///<summary>
        ///不管有沒有快取，強迫更新快取
        ///</summary>
        ForceUpdate,

        ///<summary>
        ///有快取則拿快取值，沒有，則產生快取
        ///</summary>
        Normal,

        ///<summary>
        ///有快取則拿快取值，沒有，回傳NULL
        ///</summary>
        AlwaysFromCache,

        /// <summary>
        /// 清除當前快取值
        /// </summary>
        Expire
    }
}
