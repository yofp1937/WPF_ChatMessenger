namespace ChatMessenger.Client.Common.Enums
{
    /// <summary>
    /// Win32Api.cs에서 사용하는 모니터 결정 옵션입니다.
    /// </summary>
    public enum MonitorOptions : uint
    {
        /// <summary>
        /// 윈도우가 어느 모니터에도 겹쳐있지 않은경우 null을 반환합니다.
        /// </summary>
        MONITOR_DEFAULTTONULL = 0x00000000, // null

        /// <summary>
        /// 윈도우가 어느 모니터에도 걸쳐있지않을경우 주 모니터를 반환합니다.
        /// </summary>
        MONITOR_DEFAULTTOPRIMARY = 0x00000001, // 주 모니터

        /// <summary>
        /// 윈도우와 가장 가까운 모니터의 Handle을 반환합니다.
        /// </summary>
        MONITOR_DEFAULTTONEAREST = 0x00000002 // 창과 가장 가까운 모니터
    }
}
