using ChatMessenger.Client.Models.Chats;
using ChatMessenger.Shared.DTOs.Requests.Chat;

namespace ChatMessenger.Client.Common.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// 현재 로그인한 사용자가 참여중인 채팅방 목록을 서버로부터 비동기로 가져옵니다.
        /// </summary>
        /// <returns>내가 참여한 채팅방 목록(실패시 null)</returns>
        Task<List<ChatRoomSummaryModel>?> GetMyChatRoomsAsync();

        /// <summary>
        /// 상대방과 1대1 채팅방을 생성하거나, 이미 존재하면 기존 방의 Guid를 받아옵니다.
        /// </summary>
        /// <param name="targetEmail">상대방 이메일</param>
        /// <returns>채팅방의 정보(실패시 null)</returns>
        Task<ChatRoomSummaryModel?> CreatePrivateChatRoomAsync(string targetEmail);

        /// <summary>
        /// roomId 채팅방의 상세 데이터를 서버로부터 비동기로 가져옵니다.
        /// </summary>
        /// <param name="roomId">상세 데이터를 가져올 채팅방의 식별 번호</param>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <returns>채팅방의 상세 데이터</returns>
        Task<ChatRoomDetailModel?> GetChatRoomDetailAsync(Guid roomId, string myEmail);

        /// <summary>
        /// reqeust에 포함된 roomId 채팅방의 내가 마지막으로 읽은 Message Id값을 수정해달라고 요청합니다.
        /// </summary>
        /// <param name="request">채팅방 식별번호와 메세지 식별번호가 포함된 DTO</param>
        Task<bool> UpdateLastReadedMessageAsync(UpdateLastReadedMessageRequest request);

        /// <summary>
        /// request에 포함된 roomId 채팅방에 메세지를 전달합니다.
        /// </summary>
        /// <param name="request">채팅방 식별번호와 메세지가 포함된 DTO</param>
        Task<bool> SendMessageAsync(SendMessageRequest request);

        /// <summary>
        /// 특정 채팅방에서 나갑니다.
        /// </summary>
        /// <param name="roomId">채팅방의 식별 번호</param>
        Task<bool> LeaveRoomAsync(Guid roomId);

        /// <summary>
        /// 그룹 채팅방을 생성합니다.
        /// </summary>
        /// <param name="title">채팅방 제목</param>
        /// <param name="profileIMG">프로필 이미지</param>
        /// <param name="emails">참가자들의 Email</param>
        /// <returns>채팅방의 정보(실패시 null)</returns>
        Task<ChatRoomSummaryModel?> CreateGroupChatAsync(string title, string? profileIMG, List<string> emails);
    }
}
