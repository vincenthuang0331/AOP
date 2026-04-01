namespace InfraStack.Utility.AOP
{
    /// <summary>
    /// 快取Duration列舉
    /// </summary>
    public enum CacheDurationEnum
    {
        /// <summary>
        /// 24小時(86400秒)
        /// </summary>
        HoursOf24 = 86400,

        /// <summary>
        /// 永久
        /// </summary>
        Forver = -1,

        /// <summary>
        /// 存到半夜12點
        /// </summary>
        UntilAM1200 = -2
    }
}
