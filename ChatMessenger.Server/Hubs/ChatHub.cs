using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatMessenger.Server.Hubs
{
    /// <summary>
    /// 채팅 서비스의 실시간 통신을 담당하는 SignalR 클래스입니다.
    /// </summary>
    /// <remarks>
    /// 1. 클라이언트와 서버간의 전이중 통신 통로를 제공합니다.<br/>
    /// 2. 채팅방별 글부 관리를 통해 메세지 브로드캐스팅 범위를 제어합니다.<br/>
    /// 3. 특정 이벤트(메세지 수신, 읽음 확인 등) 발생 시 연결된 클라이언트들에게 실시간 알림을 전송합니다.<br/>
    /// 4. 이 허브는 DB 저장 로직을 직접 수행하지 않으며 API 컨트롤러에서 저장된 데이터를 클라이언트에게 실시간으로 중계하는 역할만 수행합니다.
    /// </remarks>
    public class ChatHub : Hub
    {
        /// <summary>
        /// 사용자가 채팅방을 열었을때 실시간 메세지를 받기위해 메세지 수신 명단에 사용자를 등록합니다.
        /// </summary>
        /// <remarks>
        /// 사용자의 ConnectionId를 roomId를 이름으로 가진 그룹에 등록하여 메세지를 구독하게합니다.
        /// </remarks>
        /// <param name="roomId">입장할 채팅방의 고유 식별자</param>
        public async Task JoinRoom(Guid roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        }
        /// <summary>
        /// 사용자가 채팅방을 닫았을때 실시간 메세지를 받지않기위해 메세지 수신 명단에서 사용자를 제외합니다.
        /// </summary>
        /// <remarks>
        /// 사용자의 ConnectionId를 roomId를 이름으로 가진 그룹에서 제거하여 더이상 실시간 메세지 전송을 받지않도록 합니다.
        /// </remarks>
        /// <param name="roomId">퇴장할 채팅방의 고유 식별자</param>
        public async Task LeaveRoom(Guid roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        }
        /// <summary>
        /// 사용자가 SignalR과 연결되면 해당 유저와의 파이프라인을 연결합니다.
        /// </summary>
        /// <remarks>
        /// 사용자는 로그인시 자신의 email 이름으로 된 그룹에 가입되고, 유저별 email로 메세지를 전송할수있게됩니다. 
        /// </remarks>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            string? email = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(email)) return;
            // 유저가 접속하자마자 자신의 이메일 그룹에 가입시킴
            // 이제 이 유저는 어떤 방의 메시지든 서버가 이 그룹으로 쏘면 받을 수 있음
            await Groups.AddToGroupAsync(Context.ConnectionId, email);

            await base.OnConnectedAsync();
        }
    }
}
