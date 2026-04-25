namespace ChatMessenger.Client.Common.Messages
{
    /// <summary>
    /// 사용자를 강제 로그아웃 시킵니다.
    /// </summary>
    /// <remarks>
    /// 비정상적인 로그아웃 상태일때 호출할 수 있습니다.
    /// </remarks>
    public record ForceLogoutMessage();
}
