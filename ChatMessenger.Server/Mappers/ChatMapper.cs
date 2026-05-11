using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Data.Projections;
using ChatMessenger.Shared.DTOs.Responses;

namespace ChatMessenger.Server.Mappers
{
    /// <summary>
    /// ChatService에서 사용되는 DTO들의 Mapper 클래스입니다. <br/>
    /// Controller와 Service 사이에서 데이터를 목적에 적합한 형태로 가공해줍니다.
    /// </summary>
    public static class ChatMapper
    {
        /// <summary>
        /// DB에서 가져온 ChatRoomSummaryData를 Response DTO로 변환합니다.
        /// </summary>
        /// <param name="dto">DB에서 가져온 데이터 목록</param>
        /// <returns>Client에게 반환해줄 Response</returns>
        public static ChatRoomSummaryResponse ToSummaryResponse(ChatRoomSummaryDTO dto)
        {
            return new ChatRoomSummaryResponse
            {
                RoomId = dto.ChatRoomId,
                Title = dto.Title,
                RoomProfileImageURL = dto.ChatRoom.RoomProfileImageURL,
                ParticipantCount = dto.ParticipantCount,
                LastMessage = dto.LastMessage,
                LastMessageSentAt = dto.LastMessageSentAt,
                UnreadCount = dto.UnreadCount,
                IsGroupChat = dto.IsGroupChat,
            };
        }

        /// <summary>
        /// 방금 보낸 메세지 Entity와 전송자 정보, 안 읽은 사람 수를 조합하여 Response DTO로 변환합니다.
        /// </summary>
        /// <param name="message">전송할 메세지 Entity</param>
        /// <param name="sender">전송자 Entity</param>
        /// <param name="unreadPeopleCount">메세지를 읽지 않은 사람의 수</param>
        /// <returns>Client에게 반환해줄 메세지 Response</returns>
        public static ChatMessageResponse ToMessageResponse(ChatMessage message, User sender, int unreadPeopleCount)
        {
            return new ChatMessageResponse
            {
                RoomId = message.ChatRoomId,
                MessageId = message.Id,
                Sender = new FriendResponse
                {
                    Email = sender.Email,
                    Nickname = sender.Nickname,
                    StatusMessage = sender.StatusMessage,
                    ProfileImageURL = sender.ProfileImageURL,
                    IsMe = true // 내가 보낸 것이므로
                },
                Content = message.Content,
                SentAt = message.SentAt,
                UnreadPeopleCount = unreadPeopleCount
            };
        }

        /// <summary>
        /// 다른 참가자들의 View 갱신을 위해 User가 마지막으로 읽은 메세지 번호와 이전 메세지 번호를 Response로 DTO로 변환합니다. 
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="email">User의 Email</param>
        /// <param name="lastReadMessageId">마지막으로 읽은 메세지 식별 번호</param>
        /// <param name="previouseLastReadMessageId">변경 전의 메세지 식별 번호</param>
        /// <returns>다른 참가자들의 View 갱신을 위해 전송할 Response</returns>
        public static UserReadUpdateResponse ToReadUpdateResponse(Guid roomId, string email, long lastReadMessageId, long previouseLastReadMessageId)
        {
            return new UserReadUpdateResponse
            {
                RoomId = roomId,
                UserEmail = email,
                LastReadMessageId = lastReadMessageId,
                PreviousLastReadMessageId = previouseLastReadMessageId
            };
        }

        /// <summary>
        /// 채팅방의 Title을 결정하여 반환해줍니다.
        /// </summary>
        /// <param name="room">채팅방 Entity</param>
        /// <param name="me">채팅방에 대한 User의 참가 정보</param>
        /// <param name="participants">참가자 목록</param>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <returns></returns>
        public static string DetermineRoomTitle(ChatRoom room, ChatParticipant me, IEnumerable<FriendResponse> participants, string myEmail)
        {
            // 1. 지정한 별명이 있으면 최우선
            if (!string.IsNullOrEmpty(me.RenamedRoomName))
                return me.RenamedRoomName;

            // 2. 그룹 채팅이면 방 설정 제목
            if (room.IsGroupChat)
                return room.Title ?? "그룹 채팅";

            // 3. 1:1 채팅이면 상대방 닉네임
            FriendResponse? partner = participants.FirstOrDefault(p => p.Email != myEmail);
            return partner?.Nickname ?? "알 수 없는 사용자";
        }

        /// <summary>
        /// 채팅방 참가자 정보 List와 메세지 정보 List로 ChatMessageResponse를 만들어 반환합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="participants">참가자 정보 List</param>
        /// <param name="messages">메세지 정보 List</param>
        /// <param name="rawParticipants">안 읽은 사람 수 계산을 위한 참가자들의 원본 데이터 포인터</param>
        /// <returns>메세지 전송자의 정보를 포함한 Response DTO</returns>
        public static List<ChatMessageResponse> ToMessageResponseList(Guid roomId, List<FriendResponse> participants, List<ChatMessage> messages,
            IEnumerable<ChatParticipantProjection> rawParticipants)
        {
            return messages.Select(m => new ChatMessageResponse
            {
                RoomId = roomId,
                MessageId = m.Id,
                Content = m.Content,
                SentAt = m.SentAt,
                Sender = participants.FirstOrDefault(p => p.Email == m.SenderEmail) ?? GetUnknownUser(),
                UnreadPeopleCount = rawParticipants.Count(p => p.LastReadMessageId < m.Id)
            }).OrderBy(m => m.MessageId)
            .ToList();
        }

        /// <summary>
        /// 여러가지 정보들을 ChatRoomDetailResponse로 Mapping해줍니다.
        /// </summary>
        /// <param name="room">채팅방 정보 Entity</param>
        /// <param name="myPartiEntity">채팅방에 대한 나의 참가 정보 Entity</param>
        /// <param name="participants">참가자 정보 DTO</param>
        /// <param name="messages">메세지 정보 DTO</param>
        /// <param name="displayTitle">실제로 보여질 채팅방 제목</param>
        /// <param name="originalTitle">채팅방의 제목 원본</param>
        /// <returns>채팅방의 모든 정보를 품고있는 Response DTO</returns>
        public static ChatRoomDetailResponse ToChatRoomDetailResponse(ChatRoom room, ChatParticipant myPartiEntity, List<FriendResponse> participants,
            List<ChatMessageResponse> messages, string displayTitle, string? originalTitle)
        {
            return new ChatRoomDetailResponse
            {
                RoomId = room.Id,
                Title = displayTitle,
                OriginalRoomTitle = originalTitle,
                RoomProfileImageURL = room.RoomProfileImageURL,
                ParticipantCount = participants.Count,
                IsGroupChat = room.IsGroupChat,
                Participants = participants,
                Messages = messages,
                UnreadCount = messages.Count(m => m.MessageId > myPartiEntity.LastReadMessageId),
                LastReadMessageId = myPartiEntity.LastReadMessageId,
            };
        }

        private static FriendResponse GetUnknownUser() => new() { Email = "Unknown", Nickname = "알 수 없는 사용자" };
    }
}
