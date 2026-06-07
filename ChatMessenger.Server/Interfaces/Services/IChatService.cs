using ChatMessenger.Server.Data.DTOs;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.DTOs.Responses.Chat;

namespace ChatMessenger.Server.Interfaces.Services
{
    /// <summary>
    /// 채팅과 관련된 로직(방 목록 요청, 방 생성, 방 입장 등)을 처리하는 Service의 Interface입니다.
    /// </summary>
    public interface IChatService
    {
        #region 채팅방 Search, Add, Remove, Update
        /// <summary>
        /// 특정 채팅방의 ChatRoomSummaryResponse를 가져옵니다.
        /// </summary>
        /// <param name="roomId">정보를 가져올 채팅방의 식별 번호</param>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<ChatRoomSummaryResponse>> GetChatRoomSummaryResponseAsync(Guid roomId, string myEmail);
        /// <summary>
        /// User가 참가중인 모든 채팅방의 ChatRoomSummaryResponse를 가져옵니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<List<ChatRoomSummaryResponse>>> GetChatRoomSummaryResponseListAsync(string myEmail);
        /// <summary>
        /// 채팅방의 상세 정보(메시지 내역 포함)를 가져옵니다.
        /// </summary>
        /// <param name="roomId">채팅방의 식별 번호</param>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<ChatRoomDetailResponse>> GetChatRoomDetailResponseAsync(Guid roomId, string myEmail);
        /// <summary>
        /// 그룹 채팅방을 생성합니다.
        /// </summary>
        /// <param name="myEmail">채팅방 개설자의 Email</param>
        /// <param name="request">채팅방 생성 정보</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<Guid>> CreateGroupChatRoomAsync(string myEmail, CreateGroupChatRequest request);
        /// <summary>
        /// 1대1 채팅방이 있으면 채팅방의 식별 번호를 반환하고, 없으면 생성한 뒤 식별 번호를 반환합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="request">1대1 채팅방 생성에 필요한 정보 DTO</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<Guid>> GetOrCreatePrivateChatAsync(string myEmail, CreatePrivateChatRequest request);
        #endregion 채팅방 Search, Add, Remove, Update
        #region 채팅 참가자 Search, Add, Remove, Update
        /// <summary>
        /// 특정 채팅방에대한 User의 참가자 정보를 제거(채팅방 탈퇴)하고, User의 채팅방 탈퇴 메세지를 생성해서 반환합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="userEmail">채팅방을 나갈 User의 Email</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<bool>> RemoveParticipantAndCreateLeaveMessageAsync(Guid roomId, string userEmail);
        /// <summary>
        /// 채팅방에 참가자들을 등록하고, 시스템 메세지와 참가자 상태 정보를 브로드캐스팅합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="myEmail">초대하는 User의 Email</param>
        /// <param name="emails">추가하려는 참가자들의 Email이 담긴 List</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<bool>> AddParticipantsToRoomAsync(Guid roomId, string myEmail, IEnumerable<string> emails);
        #endregion 채팅 참가자 Search, Add, Remove, Update
        #region 메세지 Add, Remove, Update
        /// <summary>
        /// 로그인한 User가 특정 채팅방에서 마지막으로 읽은 메세지 정보를 갱신합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="request">갱신하려는 데이터의 정보가 담긴 Request DTO</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<UserReadUpdateResponse>> UpdateLastReadedMessageAsync(string myEmail, UpdateLastReadedMessageRequest request);
        /// <summary>
        /// 메시지를 Db에 저장하고 ChatMessageResponse를 반환합니다. 
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="request">전송하려는 메세지의 정보가 담긴 Request DTO</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<ServiceResult<ChatMessageResponse>> SendMessageAsync(string myEmail, SendMessageRequest request);
        #endregion 메세지 Add, Remove, Update
    }
}
