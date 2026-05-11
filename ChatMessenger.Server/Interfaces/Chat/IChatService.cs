using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses;

namespace ChatMessenger.Server.Interfaces.Chat
{
    /// <summary>
    /// 채팅과 관련된 로직(방 목록 요청, 방 생성, 방 입장 등)을 처리하는 Service의 Interface입니다.
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// 채팅방 목록을 가져옵니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <returns>로그인한 User의 친구 목록</returns>
        Task<List<ChatRoomSummaryResponse>> GetChatRoomListAsync(string myEmail);

        /// <summary>
        /// 1:1 채팅방을 검색하거나 새로 생성합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="targetEmail">채팅 상대 User의 Email</param>
        /// <returns></returns>
        Task<Guid> SearchOrCreatePrivateChatRoomAsync(string myEmail, string targetEmail);

        /// <summary>
        /// 채팅방의 상세 정보(메시지 내역 포함)를 가져옵니다.
        /// </summary>
        /// <param name="roomId">채팅방의 식별 번호</param>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <returns></returns>
        Task<ChatRoomDetailResponse?> GetChatRoomDetailAsync(Guid roomId, string myEmail);

        /// <summary>
        /// 메시지를 전송하고 DB에 기록합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="request">전송하려는 메세지의 정보가 담긴 Request DTO</param>
        /// <returns></returns>
        Task<ChatMessageResponse?> SendMessageAsync(string myEmail, SendMessageRequest request);

        /// <summary>
        /// 채팅방의 모든 참여자 이메일 목록을 가져옵니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>채팅방에 참여한 User들의 Email List</returns>
        Task<List<string>?> GetParticipantEmailsAsync(string myEmail, Guid roomId);

        /// <summary>
        /// 마지막으로 읽은 메시지 정보를 갱신합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="request">갱신하려는 데이터의 정보가 담긴 Request DTO</param>
        /// <returns></returns>
        Task<UserReadUpdateResponse?> UpdateReadStatusAsync(string myEmail, UpdateLastReadedMessageRequest request);
    }
}
