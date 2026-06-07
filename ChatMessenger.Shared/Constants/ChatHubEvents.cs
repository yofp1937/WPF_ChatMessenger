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
            /// <summary>
            /// 채팅방에 입장할시 송신하는 이벤트
            /// </summary>
            public const string JoinRoom = "JoinRoom";
            /// <summary>
            /// 채팅방에서 퇴장할시 송신하는 이벤트
            /// </summary>
            public const string LeaveRoom = "LeaveRoom";
        }

        /// <summary>
        /// 서버가 클라이언트의 특정 메서드 호출
        /// </summary>
        public static class ChatHubResponseEvent
        {
            /// <summary>
            /// 누군가 메세지 전송시 수신하는 이벤트
            /// </summary>
            public const string ReceiveMessage = "ReceiveMessage";
            /// <summary>
            /// 누군가 메세지 읽을시 수신하는 이벤트
            /// </summary>
            public const string UserReadMessage = "UserReadMessage";
            /// <summary>
            /// 참가자 입장, 퇴장시 수신하는 이벤트
            /// </summary>
            public const string UpdateParticipantStatus = "UpdateParticipantStatus";
        }
    }
}
