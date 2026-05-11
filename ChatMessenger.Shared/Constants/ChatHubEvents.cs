namespace ChatMessenger.Shared.Constants
{
    /// <summary>
    /// ChatHub에서 사용하는 SignalR 이벤트 및 메서드 이름을 정의합니다.
    /// </summary>
    public static class ChatHubEvents
    {
        /// <summary>
        /// 클라이언트가 서버의 Hub 메서드 호출
        /// </summary>
        public static class ChatHubRequestEvent
        {
            public const string JoinRoom = "JoinRoom";
            public const string LeaveRoom = "LeaveRoom";
        }

        /// <summary>
        /// 서버가 클라이언트의 특정 메서드 호출
        /// </summary>
        public static class ChatHubResponseEvent
        {
            public const string ReceiveMessage = "ReceiveMessage";
            public const string UserReadMessage = "UserReadMessage";
        }
    }
}
