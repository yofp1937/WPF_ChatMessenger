using ChatMessenger.Client.Models.Chats;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests.Chat;

namespace ChatMessenger.Client.Common.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// 특정 채팅방의 간략한 정보를 서버로부터 가져옵니다.
        /// </summary>
        /// <param name="roomId">정보를 가져올 채팅방의 식별 번호</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<ChatRoomSummaryModel>> GetChatRoomSummaryModelAsync(Guid roomId);
        /// <summary>
        /// 현재 로그인한 사용자가 참여중인 채팅방들의 간략한 정보를 서버로부터 비동기로 가져옵니다.
        /// </summary>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<List<ChatRoomSummaryModel>>> GetChatRoomSummaryModelListAsync();
        /// <summary>
        /// 특정 채팅방의 상세 데이터를 서버로부터 비동기로 가져옵니다.
        /// </summary>
        /// <param name="roomId">상세 데이터를 가져올 채팅방의 식별 번호</param>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<ChatRoomDetailModel>> GetChatRoomDetailModelAsync(Guid roomId, string myEmail);
        /// <summary>
        /// 특정 채팅방의 내가 마지막으로 읽은 Message Id값을 수정해달라고 요청합니다.
        /// </summary>
        /// <param name="request">채팅방 식별 번호와 메세지 식별 번호가 포함된 request DTO</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<bool>> UpdateLastReadedMessageAsync(UpdateLastReadedMessageRequest request);
        /// <summary>
        /// 특정 채팅방에 메세지 전송을 요청합니다.
        /// </summary>
        /// <param name="request">채팅방 식별번호와 메세지가 포함된 request DTO</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<bool>> SendMessageAsync(SendMessageRequest request);
        /// <summary>
        /// 특정 채팅방에서 나갑니다.
        /// </summary>
        /// <param name="roomId">채팅방의 식별 번호</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<bool>> LeaveRoomAsync(Guid roomId);
        /// <summary>
        /// 그룹 채팅방을 생성합니다.
        /// </summary>
        /// <param name="title">채팅방 제목</param>
        /// <param name="profileIMG">프로필 이미지</param>
        /// <param name="emails">참가자들의 Email</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<Guid>> CreateGroupChatAsync(string title, string? profileIMG, List<string> emails);
        /// <summary>
        /// 기존 채팅방에 참가자를 추가합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="emails">초대하려는 참가자들의 Email List</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<bool>> InviteParticipantsAsync(Guid roomId, List<string> emails);
        /// <summary>
        /// 특정 유저와의 1대1 채팅방 식별 번호를 요청합니다.
        /// 채팅방이 없으면 생성한 뒤 식별 번호를 반환합니다.
        /// </summary>
        /// <param name="targetEmail">채팅 상대 User의 Email</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<Guid>> GetOrCreatePersonalChatAsync(string targetEmail);
    }
}
