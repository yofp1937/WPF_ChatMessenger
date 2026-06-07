using ChatMessenger.Server.Data.DTOs;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using ChatMessenger.Shared.DTOs.Responses.Friend;

namespace ChatMessenger.Server.Mappers
{
    /// <summary>
    /// ChatService에서 사용되는 DTO들의 Mapper 클래스입니다. <br/>
    /// Controller와 Service 사이에서 데이터를 목적에 적합한 형태로 가공해줍니다.
    /// </summary>
    public static class ChatMapper
    {
        #region ChatRoom 관련
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
        /// 채팅방의 Title을 결정하여 반환해줍니다. User가 채팅방 별명을 설정했으면 채팅방의 원래 이름도 함께 반환합니다.
        /// </summary>
        /// <param name="room">채팅방 Entity</param>
        /// <param name="myParticipant">채팅방에 대한 User의 참가 정보</param>
        /// <param name="participants">참가자 목록</param>
        /// <returns>(표시할 채팅방 이름, 실제 채팅방 이름)</returns>
        public static (string Title, string? OriginalTitle) DetermineRoomTitle(ChatRoom room, ChatParticipant myParticipant, IEnumerable<FriendResponse> participants)
        {
            // 1. 대화 상대 Nickname 추출
            FriendResponse? partner = participants.FirstOrDefault(p => p.Email != myParticipant.UserEmail);
            string partnerName = partner?.Nickname ?? "알 수 없는 사용자";
            // 2. 설정한 별명이 있으면 최우선
            if (!string.IsNullOrEmpty(myParticipant.RenamedRoomName))
            {
                string? originalTitle = room.IsGroupChat ? room.Title : partnerName;
                return (myParticipant.RenamedRoomName, originalTitle);
            }
            // 3. 지정한 별명이 없고, 그룹 채팅일 때
            if (room.IsGroupChat)
            {
                string? groupTitle = room.Title ?? "그룹 채팅";
                return (groupTitle, null);
            }
            // 4. 지정한 별명이 없고, 1대1 채팅일 때
            return (partnerName, null);
        }
        /// <summary>
        /// 여러가지 Data들을 ChatRoomDetailResponse 객체로 매핑시켜줍니다. 
        /// </summary>
        /// <param name="myParticipant">나의 채팅방 참가 정보</param>
        /// <param name="projections">채팅방 참가자들의 정보</param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static ChatRoomDetailResponse ToChatRoomDetailResponse(ChatParticipant myParticipant, IEnumerable<ChatParticipantProjection> projections,
            IEnumerable<ChatMessage> messages)
        {
            ChatRoom room = myParticipant.ChatRoom;
            // 1. projections의 User 정보를 FriendResponse로 변환
            List<FriendResponse> participants = MapToFriendResponseList(projections);
            // 2. 표시할 채팅방 제목 결정 
            (string title, string? originalTitle) = DetermineRoomTitle(room, myParticipant, participants);
            // 3. Response로 매핑하여 반환
            return new ChatRoomDetailResponse
            {
                RoomId = room.Id,
                Title = title,
                OriginalRoomTitle = originalTitle,
                RoomProfileImageURL = room.RoomProfileImageURL,
                ParticipantCount = participants.Count,
                IsGroupChat = room.IsGroupChat,
                Participants = participants,
                Messages = ToMessageResponseList(room.Id, projections, messages),
                UnreadCount = messages.Count(m => m.Id > myParticipant.LastReadMessageId),
                LastReadMessageId = myParticipant.LastReadMessageId,
            };
        }
        #endregion ChatRoom 관련
        #region ChatMessage 관련
        /// <summary>
        /// 채팅방 참가자 정보 List와 메세지 정보 List로 ChatMessageResponse를 만들어 반환합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="projections">채팅방 참가자들의 정보</param>
        /// <param name="messages">메세지 정보 List</param>
        /// <returns>메세지 전송자의 정보를 포함한 Response DTO</returns>
        public static List<ChatMessageResponse> ToMessageResponseList(Guid roomId, IEnumerable<ChatParticipantProjection> projections,
            IEnumerable<ChatMessage> messages)
        {
            // 1. projections의 User 정보를 FriendResponse로 변환
            List<FriendResponse> friends = MapToFriendResponseList(projections);

            // 2. Response로 매핑하여 반환
            return messages.Select(m => new ChatMessageResponse
            {
                RoomId = roomId,
                MessageId = m.Id,
                MessageType = m.MessageType,
                Content = m.Content,
                SentAt = m.SentAt,
                Sender = friends.FirstOrDefault(f => f.Email == m.SenderEmail) ?? GetUnknownUser(),
                UnreadPeopleCount = projections.Count(p => p.LastReadMessageId < m.Id)
            }).OrderBy(m => m.MessageId)
            .ToList();
        }
        /// <summary>
        /// 다른 참가자들의 View 갱신을 위해 User가 마지막으로 읽은 메세지 번호와 이전 메세지 번호를 Response로 DTO로 변환합니다. 
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="userEmail">갱신할 User의 Email</param>
        /// <param name="lastReadMessageId">마지막으로 읽은 메세지 식별 번호</param>
        /// <param name="previouseLastReadMessageId">변경 전 마지막으로 읽었던 메세지 식별 번호</param>
        /// <returns>다른 참가자들의 View 갱신을 위해 전송할 Response</returns>
        public static UserReadUpdateResponse ToReadUpdateResponse(Guid roomId, string userEmail, long lastReadMessageId, long previouseLastReadMessageId)
        {
            return new UserReadUpdateResponse
            {
                RoomId = roomId,
                UserEmail = userEmail,
                LastReadMessageId = lastReadMessageId,
                PreviousLastReadMessageId = previouseLastReadMessageId
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
                MessageType = message.MessageType,
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
        /// 시스템 메시지 전용 Response DTO를 생성합니다.
        /// </summary>
        public static ChatMessageResponse ToSystemMessageResponse(ChatMessage message)
        {
            return new ChatMessageResponse
            {
                RoomId = message.ChatRoomId,
                MessageId = message.Id,
                MessageType = message.MessageType,
                Sender = null, // 시스템 메시지는 발신자가 없음
                Content = message.Content,
                SentAt = message.SentAt,
                UnreadPeopleCount = 0
            };
        }
        /// <summary>
        /// 입장 또는 퇴장에 따른 시스템 메시지 문자열을 생성합니다.
        /// </summary>
        /// <param name="nicknames">대상자 닉네임</param>
        /// <param name="isJoin">true면 입장, false면 퇴장</param>
        public static string CreateSystemMessagesContent(IEnumerable<string> nicknames, bool isJoin)
        {
            if (nicknames == null || !nicknames.Any()) return string.Empty;

            // 이름들 사이를 "철수, 영희, 민수" 같은 형태로 합치기
            string combinedNames = string.Join(", ", nicknames);
            return isJoin
                ? $"{combinedNames}님이 입장하셨습니다."
                : $"{combinedNames}님이 퇴장하셨습니다.";
        }
        #endregion ChatMessage 관련
        #region ChatParticipant 관련
        /// <summary>
        /// 참가자가 입장, 퇴장하면 시스템 메세지와 참가자 변동 상황을 response로 생성합니다.
        /// </summary>
        /// <remarks>
        /// FriendResponse는 채팅방 참가자 정보에서 데이터를 추가하거나 삭제해야하기때문에 필요합니다.
        /// </remarks>
        /// <param name="msgResponse">참가자들에게 전송할 시스템 메세지 Response</param>
        /// <param name="currentParticipantsCount">변동 이후 채팅방 참가자 수</param>
        /// <param name="users">채팅방 참가, 퇴장 대상자들의 FriendRespons List</param>
        /// <param name="isJoined">입장시 ture, 퇴장시 false</param>
        /// <returns></returns>
        public static ChatParticipantStatusResponse ToParticipantStatusResponse(ChatMessageResponse msgResponse, int currentParticipantsCount, List<FriendResponse> users, bool isJoined)
        {
            return new()
            {
                Message = msgResponse,
                CurrentParticipantCount = currentParticipantsCount,
                TargetUsers = users,
                IsJoined = isJoined
            };
        }
        #endregion ChatParticipant 관련
        #region private Method
        /// <summary>
        /// 상대방의 정보를 찾을 수 없을때 임시 사용자 정보를 생성합니다.
        /// </summary>
        /// <returns>Unknown 사용자 정보</returns>
        private static FriendResponse GetUnknownUser() => new() { Email = "Unknown", Nickname = "알 수 없는 사용자" };
        /// <summary>
        /// 각 ChatParticipantProjection의 User 필드를 FriendMapper를 사용해 FriendModel 타입으로 변환해 List로 만들어 반환해줍니다.
        /// </summary>
        /// <param name="projections"></param>
        /// <returns></returns>
        private static List<FriendResponse> MapToFriendResponseList(IEnumerable<ChatParticipantProjection> projections)
        {
            return projections.Select(p => FriendMapper.MapToFriendResponse(p.User)).ToList();
        }
        #endregion private Method
    }
}
