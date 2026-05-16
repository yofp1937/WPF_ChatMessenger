using Azure.Core;
using ChatMessenger.Server.Common.Interfaces.Chats;
using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Data.Projections;
using ChatMessenger.Server.Interfaces.Chat;
using ChatMessenger.Server.Mappers;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using ChatMessenger.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChatMessenger.Server.Services.Chat
{
    /// <summary>
    /// ChatController의 요청에따라 채팅과 관련된 요청(방 목록 요청, 방 생성, 방 입장 등)을 처리해주는 Service입니다.
    /// </summary>
    public class ChatService : IChatService
    {
        private AppDbContext _context;
        private IChatRoomService _chatRoomService;
        private IChatParticipantService _chatParticipantService;
        private IChatMessageService _chatMessageService;

        public ChatService(AppDbContext context, IChatRoomService chatRoomService, IChatParticipantService chatParticipantService,
            IChatMessageService chatMessageService)
        {
            _context = context;
            _chatRoomService = chatRoomService;
            _chatParticipantService = chatParticipantService;
            _chatMessageService = chatMessageService;
        }

        #region public Method
        /// <inheritdoc/>
        public async Task<List<ChatRoomSummaryResponse>> GetChatRoomListAsync(string myEmail)
        {
            // 1. Response 생성에 필요한 DTO 추출
            List<ChatRoomSummaryDTO> dtoList = await _chatRoomService.GetChatRoomSummaryDTOListAsync(myEmail);
            // 2. Mapper를 사용해 DTO를 Response로 변환하고 List화하여 반환
            return dtoList.Select(ChatMapper.ToSummaryResponse).ToList();
        }
        /// <inheritdoc/>
        public async Task<ChatRoomSummaryResponse?> GetOrCreatePrivateChatRoomAsync(string myEmail, string targetEmail)
        {
            // 1. myEmail과 targetEmail간의 1대1 채팅방이 존재하는지 검색
            ChatRoom? chatRoom = await _chatRoomService.GetPrivateChatRoomAsync(myEmail, targetEmail);
            // 2. 채팅방이 존재하지않으면 1대1 채팅방 생성
            if (chatRoom == null)
            {
                List<string> participants = new() { myEmail, targetEmail };
                chatRoom = await CreateChatRoomAndRegisterParticipantsAsync(participants, null, false);
                if (chatRoom == null) return null;
            }
            // 3. ChatRoom Entity로 ChatRoomSummaryDTO 추출
            ChatRoomSummaryDTO? dto = await _chatRoomService.GetChatRoomSummaryDTOAsync(myEmail, chatRoom.Id);
            if (dto == null) return null;
            // 4. ChatRoomSummaryResponse로 변환해서 반환
            return ChatMapper.ToSummaryResponse(dto);
        }
        /// <inheritdoc/>
        public async Task<ChatRoomDetailResponse?> GetChatRoomDetailAsync(Guid roomId, string myEmail)
        {
            // 1. 실제 해당 채팅방의 참가자인지 보안 검사
            ChatParticipant? myParticipant = await _chatParticipantService.GetParticipantEntityAsync(roomId, myEmail);
            if (myParticipant == null) return null;

            // 2. 채팅방의 상세 정보 추출하여 반환
            return await GetChatRoomDetailResponseInternalAsync(roomId, myEmail, myParticipant);
        }
        /// <inheritdoc/>
        public async Task<ChatMessageResponse?> SendMessageAsync(string myEmail, SendMessageRequest request)
        {
            // 1. 실제 해당 채팅방의 참가자인지 보안 검사
            ChatParticipant? myParticipant = await _chatParticipantService.GetParticipantEntityAsync(request.RoomId, myEmail);
            if (myParticipant == null) return null;

            try
            {
                // 2. 메세지 전송하고 나의 LastReadedMessageId 업데이트
                ChatMessage? newMessage = await AddMessageAndUpdateLastReadedInternalAsync(myEmail, request, myParticipant);
                if (newMessage == null) return null;

                // 3. Response에 발신자 정보를 포함하기위해 User 정보 조회
                List<User>? users = await SearchUserByEmailsAsync(new[] { myEmail });
                if (users == null || users.Count == 0) return null;
                User user = users.First();

                // 4. 안 읽은 사람 수 계산 (전체 참여자 수 - 1)(내 LastReadMessageId를 갱신하기때문에 -1 해주면 됨)
                int participantCount = await _chatParticipantService.GetParticipantCountAsync(request.RoomId);
                int unreadPeopleCount = participantCount - 1;

                // 5. Mapper를 사용하여 Response 반환
                return ChatMapper.ToMessageResponse(newMessage, user, unreadPeopleCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(SendMessageAsync)}]: {ex.Message}");
                return null;
            }
        }
        /// <inheritdoc/>
        public async Task<List<string>?> GetParticipantEmailsAsync(Guid roomId)
        {
            return await _chatParticipantService.GetParticipantEmailsAsync(roomId);
        }
        /// <inheritdoc/>
        public async Task<UserReadUpdateResponse?> UpdateReadStatusAsync(string myEmail, UpdateLastReadedMessageRequest request)
        {
            // 1. 실제 해당 채팅방의 참가자인지 보안 검사
            ChatParticipant? myParticipant = await _chatParticipantService.GetParticipantEntityAsync(request.RoomId, myEmail);
            if (myParticipant == null) return null;
            long previouseId = myParticipant.LastReadMessageId;

            try
            {
                // 2. 채팅방에서 내가 마지막으로 읽은 메세지 갱신
                myParticipant.LastReadMessageId = request.LastReadMessageId;
                bool isSuccess = await _chatParticipantService.UpdateParticipantAsync(myParticipant);

                // 3. Mapper를 사용하여 Response 반환
                return ChatMapper.ToReadUpdateResponse(request.RoomId, myEmail, request.LastReadMessageId, previouseId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(UpdateReadStatusAsync)}]: {ex.Message}");
                return null;
            }
        }
        /// <inheritdoc/>
        public async Task<ChatMessageResponse?> DeleteParticipantAsync(Guid roomId, string myEmail)
        {
            // 1. 실제 해당 채팅방의 참가자인지 보안 검사
            ChatParticipant? myParticipant = await _chatParticipantService.GetParticipantEntityAsync(roomId, myEmail);
            if (myParticipant == null) return null;
            string myNickname = myParticipant.User.Nickname;

            try
            {
                // 2. 채팅방 참가 정보 제거
                bool isSuccess = await _chatParticipantService.RemoveParticipantAsync(myParticipant);
                if (!isSuccess) return null;

                // 3. 누군가가 채팅방 나갔으면 채팅방 삭제 시도
                bool isRoomDeleted = await RemoveChatRoom(roomId);
                if (isRoomDeleted) return null; // 방이 지워졌으면 SystemMessage 전달할 필요 없으니 return

                // 4. 채팅방 남아있는지, 그룹 채팅인지 확인
                ChatRoom? room = await _chatRoomService.GetChatRoomAsync(roomId);
                if (room == null || !room.IsGroupChat) return null;

                // 5. 채팅방이 남아있고 그룹 채팅이면 채팅방에 퇴장 SystemMessage 생성
                SendMessageRequest tempReq = new()
                {
                    RoomId = room.Id,
                    MessageType = ChatMessageType.System,
                    Content = ChatMapper.CreateSystemMessagesContent(new[] { myNickname }, false)
                };

                // 6. 메세지 전송
                ChatMessage? message = await AddSystemMessageInternalAsync(tempReq);
                if (message == null) return null;

                // 7. Response로 변환하여 반환
                return ChatMapper.ToSystemMessageResponse(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(DeleteParticipantAsync)}]: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatRoomSummaryResponse?> CreateChatRoomAsync(string myEmail, IEnumerable<string> participantEmails
            , CreateGroupChatRequest request)
        {
            // 1. 방 생성 및 참가자 등록
            ChatRoom? newRoom = await CreateChatRoomAndRegisterParticipantsAsync(participantEmails, request.Title, true);
            if (newRoom == null) return null;
            // 2. 방 정보 업데이트
            bool result = await UpdateGroupChatEntityAsync(newRoom.Id, request.Title, request.ProfileImageURL);
            if (!result) return null;

            try
            {
                // 3. 채팅방 참가자들의 Nickname 추출
                List<User>? users = await SearchUserByEmailsAsync(participantEmails);
                if (users == null || users.Count == 0) return null;
                List<string> nicknames = users.Select(u => u.Nickname).ToList();

                // 4. 입장 SystemMessage 객체 생성
                SendMessageRequest tempReq = new()
                {
                    RoomId = newRoom.Id,
                    MessageType = ChatMessageType.System,
                    Content = ChatMapper.CreateSystemMessagesContent(nicknames, true)
                };

                // 5. 메세지 Db에 등록
                ChatMessage? message = await AddSystemMessageInternalAsync(tempReq);
                if (message == null) return null;

                // 6. Response 형태로 반환
                return ChatMapper.ToSummaryResponse(newRoom, message, users.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(CreateChatRoomAsync)}]: {ex.Message}");
                return null;
            }
        }
        #endregion public Method
        #region private Method
        /// <summary>
        /// 새로운 채팅방을 생성하고 참가자들을 등록합니다..
        /// </summary>
        /// <param name="emails">참가자로 등록할 User들의 Email List</param>
        /// <param name="title">채팅방 제목</param>
        /// <param name="isGroupChat">그룹 채팅인지 여부</param>
        /// <returns>새로 생성한 채팅방 Entity</returns>
        private async Task<ChatRoom?> CreateChatRoomAndRegisterParticipantsAsync(IEnumerable<string> emails, string? title, bool isGroupChat)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. 방 생성 (실패 시 throw)
                ChatRoom? newRoom = await _chatRoomService.CreateChatRoomAsync(title, isGroupChat);
                if (newRoom == null) return null;
                // 2. 참가자 추가 (실패 시 throw)
                bool isSuccess = await _chatParticipantService.AddParticipantsToRoomAsync(newRoom.Id, emails);
                if (!isSuccess) return null;

                // 3. 임시 저장 상태인걸 실제 DB에 적용
                await transaction.CommitAsync();
                return newRoom;
            }
            catch (Exception ex)
            {
                // 4. 에러 발생시 임시 저장 상태를 롤백
                await transaction.RollbackAsync();
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(CreateChatRoomAndRegisterParticipantsAsync)}]: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// 채팅방의 상세 정보 Response를 생성해서 반환해줍니다.
        /// </summary>
        /// <remarks>
        /// 하위 Service들에서 정보를 취합한 뒤 ChatRoomDetailResponse를 생성하여 반환해줍니다.
        /// </remarks>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="myParticipant">나의 채팅방 참가 정보</param>
        /// <returns>채팅방의 ChatRoomDetailResponse DTO</returns>
        private async Task<ChatRoomDetailResponse?> GetChatRoomDetailResponseInternalAsync(Guid roomId, string myEmail, ChatParticipant myParticipant)
        {
            // 1. Response 생성을 위한 채팅방 DTO 추출
            ChatRoomDetailDTO? dto = await _chatRoomService.GetChatRoomDetailDTOAsync(roomId, myEmail);
            if (dto == null) return null;
            // 2. 친구 관계 데이터 수집
            List<string> participantEmails = dto.Participants.Select(p => p.User.Email).ToList();
            Dictionary<string, Friendship> friendships = await GetFriendshipsAsync(myEmail, participantEmails);
            // 3. 매퍼들을 활용하여 Response에 필요한 Data들 생성
            List<FriendResponse> participantList = FriendMapper.ToFriendResponseList(myEmail, dto.Participants, friendships);
            List<ChatMessageResponse> messageList = ChatMapper.ToMessageResponseList(roomId, participantList, dto.Messages, dto.Participants);
            string displayTitle = ChatMapper.DetermineRoomTitle(dto.Room, myParticipant, participantList, myEmail);
            string? originalTitle = null;
            // 4. 사용자가 방 제목을 수정했으면 originalTitle 설정
            if (!string.IsNullOrEmpty(myParticipant.RenamedRoomName))
            {
                ChatParticipant temp = new() { RenamedRoomName = null };
                originalTitle = ChatMapper.DetermineRoomTitle(dto.Room, temp, participantList, myEmail);
            }
            // 5. Response 생성하여 반환
            return ChatMapper.ToChatRoomDetailResponse(dto.Room, myParticipant, participantList, messageList, displayTitle, originalTitle);
        }
        /// <summary>
        /// 메세지를 Db에 등록하고, 작성자의 LastReadedMessageId를 신규 메세지 Id로 변경합니다.
        /// </summary>
        /// <param name="myEmail">메세지 작성자의 Email</param>
        /// <param name="request">메세지 정보가 담긴 Request</param>
        /// <param name="myParticipant">메세지 작성자의 ChatParticipant Entity</param>
        /// <returns>등록한 ChatMessage 객체</returns>
        private async Task<ChatMessage?> AddMessageAndUpdateLastReadedInternalAsync(string myEmail, SendMessageRequest request,
            ChatParticipant myParticipant)
        {
            using IDbContextTransaction? transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. 사용자 요청에따라 Db에 메세지 등록
                ChatMessage? newMessage = await _chatMessageService.AddMessageAsnyc(myEmail, request);
                if (newMessage == null) return null;

                // 2. 메세지가 성공적으로 등록됐으면 LastReadMessagId를 업데이트해줘야함
                myParticipant.LastReadMessageId = newMessage.Id;
                bool isSuccess = await _chatParticipantService.UpdateParticipantAsync(myParticipant);
                if (!isSuccess) return null;

                // 3. 임시 저장된 메세지 등록, ChatParticipant 업데이트 상황을 Db에 저장 
                await transaction.CommitAsync();
                return newMessage;
            }
            catch (Exception ex)
            {
                // 4. 에러 발생시 임시 저장 상태를 롤백
                await transaction.RollbackAsync();
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(AddMessageAndUpdateLastReadedInternalAsync)}]: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 이메일 목록으로 해당하는 유저 정보들을 조회합니다.
        /// </summary>
        /// <param name="emails">조회할 유저들의 이메일 리스트</param>
        /// <returns>조회된 User 엔티티 리스트 (없거나 에러 발생 시 null)</returns>
        private async Task<List<User>?> SearchUserByEmailsAsync(IEnumerable<string> emails)
        {
            return await _context.Users
                .AsNoTracking() // 읽기 전용 최적화
                .Where(u => emails.Contains(u.Email))
                .ToListAsync();
        }
        /// <summary>
        /// 채팅방에 시스템 메시지(입장/퇴장 등)를 등록합니다.
        /// </summary>
        /// <param name="request">시스템 메시지 정보가 담긴 Request</param>
        /// <returns>등록된 ChatMessage 객체, 실패 시 null</returns>
        private async Task<ChatMessage?> AddSystemMessageInternalAsync(SendMessageRequest request)
        {
            // 시스템 메시지는 발신자가 없으므로 주입할 이메일을 null로 전달
            return await _chatMessageService.AddMessageAsnyc(null, request);
        }
        /// <summary>
        /// 모든 User가 채팅방을 나갔으면 방 정보를 제거합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>유저가 전부 나가서 채팅방 삭제에 성공했으면 true, 조건 부적합으로 실패하면 false</returns>
        private async Task<bool> RemoveChatRoom(Guid roomId)
        {
            // 1. 채팅방에 남은 유저가 있는지 확인
            bool hasParticipants = await _chatParticipantService.HasAnyParticipantsAsync(roomId);
            if (hasParticipants) return false;

            // 2. 남은 유저가 없으면 채팅방 삭제
            bool isSuccess = await _chatRoomService.RemoveChatRoomAsync(roomId);
            if (!isSuccess) return false;

            return true;
        }
        /// <summary>
        /// User와 채팅방 참가자들간의 친구 관계를 가져옵니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="participantEmails">채팅방 참가자들의 Email</param>
        /// <returns>User와 참가자들간의 Friendship 관계 정보</returns>
        private async Task<Dictionary<string, Friendship>> GetFriendshipsAsync(string myEmail, List<string> participantEmails)
        {
            return await _context.Friendships
                .Where(f => f.UserEmail == myEmail && participantEmails.Contains(f.FriendEmail))
                .ToDictionaryAsync(f => f.FriendEmail);
        }
        /// <summary>
        /// 그룹 채팅방을 찾아서 제목, 프로필 이미지를 업데이트합니다.
        /// </summary>
        /// <param name="roomId">그룹 채팅방 식별 번호</param>
        /// <param name="title">바꾸려는 제목</param>
        /// <param name="profileIMGURL">바꾸려는 프로필 이미지</param>
        /// <returns></returns>
        private async Task<bool> UpdateGroupChatEntityAsync(Guid roomId, string? title, string? profileIMGURL)
        {
            // 1. 방 찾기
            ChatRoom? room = await _chatRoomService.GetChatRoomAsync(roomId);
            if (room == null) return false;

            // 2. 업데이트
            room.Title = title;
            room.RoomProfileImageURL = profileIMGURL;
            return await _chatRoomService.UpdateChatRoomAsync(room);
        }
        #endregion private Method
    }
}
