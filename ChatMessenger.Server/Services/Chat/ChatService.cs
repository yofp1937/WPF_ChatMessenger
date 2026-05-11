using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Data.Projections;
using ChatMessenger.Server.Interfaces.Chat;
using ChatMessenger.Server.Mappers;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChatMessenger.Server.Services.Chat
{
    /// <summary>
    /// ChatController의 요청에따라 채팅과 관련된 요청(방 목록 요청, 방 생성, 방 입장 등)을 처리해주는 Service입니다.
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        #region public Method
        /// <inheritdoc/>
        public async Task<List<ChatRoomSummaryResponse>> GetChatRoomListAsync(string myEmail)
        {
            // 1. ChatRoomSummaryResponse 생성에 필요한 DTO 추출
            List<ChatRoomSummaryDTO> rawData = await FetchChatRoomSummaryAsync(myEmail);
            // 2. Mapper를 사용해 DTO로 Response로 변환하고 List화하여 반환
            return rawData.Select(ChatMapper.ToSummaryResponse).ToList();
        }

        /// <inheritdoc/>
        public async Task<Guid> SearchOrCreatePrivateChatRoomAsync(string myEmail, string targetEmail)
        {
            // 1. myEmail과 targetEmail간의 1대1 채팅방이 존재하는지 검색
            Guid existingRoomId = await SearchPrivateChatRoomAsync(myEmail, targetEmail);
            // 2. 방이 존재하면 해당 방의 Guid 반환
            if (existingRoomId != Guid.Empty)
                return existingRoomId;

            // 3. 방이 없으면 생성
            Guid newRoomId = await CreateNewChatRoomAndParticipantsAsync(new[] { myEmail, targetEmail }, null, false);
            if (newRoomId == Guid.Empty)
                return Guid.Empty;
            return newRoomId;
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDetailResponse?> GetChatRoomDetailAsync(Guid roomId, string myEmail)
        {
            // 1. 실제 해당 채팅방의 참가자인지 보안 검사
            if (await GetParticipantEntityAsync(roomId, myEmail) == null)
                return null;

            return await FetchChatRoomDetailAsync(roomId, myEmail);
        }

        /// <inheritdoc/>
        public async Task<ChatMessageResponse?> SendMessageAsync(string myEmail, SendMessageRequest request)
        {
            // 1. 실제 해당 채팅방의 참가자인지 보안 검사
            if (await GetParticipantEntityAsync(request.RoomId, myEmail) == null)
                return null;

            try
            {
                // 1. 사용자 요청에따라 Db에 메세지 등록
                ChatMessage? newMessage = await SendMessageAndUpdateAsync(myEmail, request.RoomId, request.Content);
                if (newMessage == null) return null;

                // 2. Response에 발신자 정보를 포함하기위해 User 정보 조회
                User? user = await SearchUserByEmail(myEmail);
                if (user == null) return null;

                // 3. 안 읽은 사람 수 계산 (전체 참여자 수 - 1)(내 LastReadMessageId를 갱신하기때문에 -1 해주면 됨)
                int participantCount = await _context.ChatParticipants.CountAsync(cp => cp.ChatRoomId == request.RoomId);
                int unreadPeopleCount = participantCount - 1;

                // 4. Mapper를 사용하여 Response 반환
                return ChatMapper.ToMessageResponse(newMessage, user, unreadPeopleCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(SendMessageAsync)}]: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>?> GetParticipantEmailsAsync(string myEmail, Guid roomId)
        {
            // 1. 실제 해당 채팅방의 참가자인지 보안 검사
            if (await GetParticipantEntityAsync(roomId, myEmail) == null)
                return null;

            return await GetParticipantEmailsInternalAsync(roomId);
        }

        /// <inheritdoc/>
        public async Task<UserReadUpdateResponse?> UpdateReadStatusAsync(string myEmail, UpdateLastReadedMessageRequest request)
        {
            // 1. 실제 해당 채팅방의 참가자인지 보안 검사
            ChatParticipant? participant = await GetParticipantEntityAsync(request.RoomId, myEmail);
            if (participant == null) return null;
            long previouseId = participant.LastReadMessageId;

            try
            {
                // 2. 채팅방에서 내가 마지막으로 읽은 메세지 갱신
                // UpdateLastReadMessage를 사용할 수 있지만 사용하면 DB 조회를 두번 하는꼴이라 해당 메서드 내에서 직접 수정
                participant.LastReadMessageId = request.LastReadMessageId;
                await _context.SaveChangesAsync();

                // 3. Mapper를 사용하여 Response 반환
                return ChatMapper.ToReadUpdateResponse(request.RoomId, myEmail, request.LastReadMessageId, previouseId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(UpdateReadStatusAsync)}]: {ex.Message}");
                return null;
            }
        }
        #endregion public Method
        #region private Method
        /// <summary>
        /// 두 유저 사이의 1대1 채팅방을 찾습니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="targetEmail">채팅 상대 User의 Email</param>
        /// <returns>방 존재 시 해당 방의 Guid, 방 없으면 Guid.Empty</returns>
        private async Task<Guid> SearchPrivateChatRoomAsync(string myEmail, string targetEmail)
        {
            return await _context.ChatParticipants
                .Where(p => p.UserEmail == myEmail)                                                                 // 1. ChatParticipant에서 myEmail과 UserEmail 컬럼 값이 같은 모든 튜플 추출
                .Join(_context.ChatParticipants.Where(p => p.UserEmail == targetEmail),                      // 2. ChatParticipant에서 targetEamil과 UserEmail 컬럼 값이 같은 모든 튜플 추출
                        me => me.ChatRoomId,
                        other => other.ChatRoomId,                                                                       // 3. 1번에서 얻은 결과를 me로 선언, 2번에서 얻은 결과를 other로 선언
                        (me, other) => me.ChatRoomId)                                                                  // 4. me와 other의 ChatRoomId가 같은 튜플만 골라서 me에서 ChatRoomId 추출
                .Where(roomId => _context.ChatRooms.Any(r => r.Id == roomId && !r.IsGroupChat))    // 5. ChatRoom 테이블에서 추출한 ChatRoomId와 RoomId가 같고 그룹채팅이 아닌 방 추출
                .FirstOrDefaultAsync();                                                                                       // 6. 그중 첫번째 데이터의 roomId를 내보낸다.
        }

        /// <summary>
        /// 새로운 채팅방을 생성하고 참가자들을 등록해줍니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="targetEmail">채팅 상대 User의 Email</param>
        /// <returns>새로 생성한 방의 Guid</returns>
        private async Task<Guid> CreateNewChatRoomAndParticipantsAsync(IEnumerable<string> emails, string? title, bool isGroupChat)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. 방 생성 (실패 시 throw)
                ChatRoom newRoom = await CreateChatRoomEntityAsync(title, isGroupChat);
                // 2. 참가자 추가 (실패 시 throw)
                await AddParticipantsToRoomAsync(newRoom.Id, emails);

                // 3. 임시 저장 상태인걸 실제 DB에 적용
                await transaction.CommitAsync();
                return newRoom.Id;
            }
            catch (Exception ex)
            {
                // 4. 에러 발생시 임시 저장 상태를 롤백
                await transaction.RollbackAsync();
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(CreateNewChatRoomAndParticipantsAsync)}]: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 새로운 채팅방을 생성합니다. 방 생성에 실패하면 throw 신호를 보냅니다.
        /// </summary>
        /// <param name="title">생성하려는 채팅방의 Title</param>
        /// <param name="isGroupChat">생성하려는 채팅방이 그룹 채팅인지 여부</param>
        /// <returns>새로 생성한 채팅방 Entity</returns>
        private async Task<ChatRoom> CreateChatRoomEntityAsync(string? title, bool isGroupChat)
        {
            ChatRoom newRoom = new() { Title = title, IsGroupChat = isGroupChat };
            _context.ChatRooms.Add(newRoom);
            if (await _context.SaveChangesAsync() <= 0)
                throw new Exception("채팅방 생성에 실패했습니다.");
            return newRoom;
        }

        /// <summary>
        /// 채팅방에 참가자들을 등록합니다. 참가자 등록에 실패하면 throw 신호를 보냅니다.
        /// </summary>
        /// <param name="roomId">참가자를 등록하려는 채팅방</param>
        /// <param name="emails">추가하려는 참가자 List</param>
        /// <returns>참가자 등록 성공 시 true 반환, 등록 실패 시 false 반환</returns>
        private async Task AddParticipantsToRoomAsync(Guid roomId, IEnumerable<string> emails)
        {
            IEnumerable<ChatParticipant> participants = emails.Select(email => new ChatParticipant
            {
                ChatRoomId = roomId,
                UserEmail = email
            });
            _context.ChatParticipants.AddRange(participants);
            if (await _context.SaveChangesAsync() <= 0)
                throw new Exception("참가자 등록에 실패했습니다.");
        }

        /// <summary>
        /// User가 해당 방의 실제 참가자인지 확인하고 Entity를 반환합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="myEmail">참가여부 확인할 User의 Email</param>
        /// <returns>ChatParticipant Entity</returns>
        private async Task<ChatParticipant?> GetParticipantEntityAsync(Guid roomId, string myEmail)
        {
            return await _context.ChatParticipants.FirstOrDefaultAsync(cp => cp.ChatRoomId == roomId && cp.UserEmail == myEmail);
        }

        /// <summary>
        /// 메세지를 Db에 등록합니다.
        /// </summary>
        /// <param name="myEmail">메세지를 전송한 User의 Email</param>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="content">메세지 내용</param>
        private async Task<ChatMessage?> SendMessageAndUpdateAsync(string myEmail, Guid roomId, string content)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                ChatMessage newMessage = new()
                {
                    ChatRoomId = roomId,
                    SenderEmail = myEmail,
                    Content = content
                };
                _context.ChatMessages.Add(newMessage);
                if (0 >= await _context.SaveChangesAsync())
                    throw new Exception("메세지 등록에 실패했습니다.");

                // 메세지를 전송하면 자신의 LastReadMessagId를 업데이트해줘야함
                await UpdateLastReadMessage(myEmail, roomId, newMessage.Id);
                // 임시 저장 상태인걸 실제 DB에 적용
                await transaction.CommitAsync();
                return newMessage;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[{nameof(ChatService)}_{nameof(SendMessageAndUpdateAsync)}]: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 채팅방에서 자신이 마지막으로 읽은 메세지 번호를 갱신합니다. 갱신에 실패하면 throw 신호를 보냅니다.
        /// </summary>
        /// <param name="myEmail">마지막 읽은 메세지를 갱신할 User의 Email</param>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="lastReadMessageId">메세지 식별 번호</param>
        /// <returns>갱신 성공 시 true 반환, 갱신 실패시 false 반환</returns>
        private async Task UpdateLastReadMessage(string myEmail, Guid roomId, long lastReadMessageId)
        {
            ChatParticipant? myParticipantData = await _context.ChatParticipants
                .FirstOrDefaultAsync(cp => cp.ChatRoomId == roomId && cp.UserEmail == myEmail);

            if (myParticipantData == null)
                throw new Exception($"{myEmail}님의 {roomId}에 대한 정보를 찾을 수 업습니다.");

            myParticipantData.LastReadMessageId = lastReadMessageId;
            if (await _context.SaveChangesAsync() <= 0)
                throw new Exception("마지막으로 읽은 메세지 갱신에 실패했습니다.");
        }

        /// <summary>
        /// 이메일로 유저 정보를 찾아줍니다.
        /// </summary>
        /// <param name="email">찾을 User의 Email</param>
        /// <returns>User의 정보(수정 불가능)</returns>
        private async Task<User?> SearchUserByEmail(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// 특정 채팅방의 모든 참여자 Email을 찾아줍니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>채팅방 참여자들의 Email List</returns>
        private async Task<List<string>> GetParticipantEmailsInternalAsync(Guid roomId)
        {
            return await _context.ChatParticipants
                .Where(cp => cp.ChatRoomId == roomId)
                .Select(cp => cp.UserEmail)
                .ToListAsync();
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
        #region Projection
        /// <summary>
        /// Db에서 ChatRoomSummaryResponse DTO 생성에 필요한 Data를 최적화된 Sql 쿼리로 추출합니다.<br/>
        /// </summary>
        /// <remarks>
        /// * Db에서 정렬까지해서 반환해주므로 서버에서는 DTO로 변환하여 Client에게 전송해주면 됩니다. <br/>
        /// 1. 프로젝션(Select)을 사용하여 필요한 컬럼만 SQL 수준에서 선별적으로 추출 <br/>
        /// 2. 익명 객체 프로젝션을 통해 테이블 중복 조회 방지 <br/>
        /// 3. 1차적으로 전송시간 기준 내림차순, 2차적으론 방 ID 기준 정렬까지 수행하여 외부에서 정렬 작업 불필요
        /// </remarks>
        /// <param name="myEmail">채팅방 목록을 조회할 사용자 Email</param>
        /// <returns>ChatRoomSummaryResponse DTO 생성에 필요한 Data</returns>
        private async Task<List<ChatRoomSummaryDTO>> FetchChatRoomSummaryAsync(string myEmail)
        {
            return await _context.ChatParticipants
                .AsNoTracking() // 데이터를 읽어오기만하고 수정하진 않을거라 데이터 추적 프로세스는 건너뜀
                .Where(p => p.UserEmail == myEmail)
                .Select(p => new
                {
                    Participant = p,
                    Room = p.ChatRoom,
                    // 임시로 최신 메시지 정보를 하나의 익명 객체로 추출
                    LatestMessage = _context.ChatMessages
                        .Where(m => m.ChatRoomId == p.ChatRoomId)
                        .OrderByDescending(m => m.Id)
                        .Select(m => new { m.Content, m.SentAt })
                        .FirstOrDefault(),
                    // 채팅방의 참여자가 몇명인지 카운트
                    ParticipantCount = _context.ChatParticipants.Count(cp => cp.ChatRoomId == p.ChatRoomId),
                    // 안읽은 메세지 수
                    UnreadCount = _context.ChatMessages.Count(m =>
                        m.ChatRoomId == p.ChatRoomId &&
                        m.Id > p.LastReadMessageId &&
                        m.SenderEmail != myEmail)
                })
                .Select(x => new ChatRoomSummaryDTO
                {
                    // 1. 기본 정보 설정
                    ChatRoomId = x.Participant.ChatRoomId,
                    ChatRoom = x.Room,
                    IsGroupChat = x.Room.IsGroupChat,

                    // 2. Title 정하는 로직
                    // 기준 1. RenamedRoomName이 존재하면 그걸 Title로 설정
                    // 기준 2. (그룹 채팅일 경우)방을 생성할때 방장이 정한 Title
                    // 기준 3. (1대1 채팅일 경우)상대방의 닉네임
                    Title = x.Participant.RenamedRoomName
                        ?? (x.Room.IsGroupChat
                            ? x.Room.Title
                            : _context.ChatParticipants
                                .Where(cp => cp.ChatRoomId == x.Participant.ChatRoomId && cp.UserEmail != myEmail)
                                .Select(cp => cp.User.Nickname)
                                .FirstOrDefault()) ?? "알 수 없는 사용자",

                    // 3. 마지막 메세지, 참가자 수, 안읽은 메세지 수
                    LastMessage = x.LatestMessage != null ? x.LatestMessage.Content : string.Empty,
                    LastMessageSentAt = x.LatestMessage != null ? x.LatestMessage.SentAt : null,
                    LastReadMessageId = x.Participant.LastReadMessageId,
                    ParticipantCount = x.ParticipantCount,
                    UnreadCount = x.UnreadCount,
                })
                .OrderByDescending(x => x.LastMessageSentAt ?? x.ChatRoom.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Db에서 ChatRoomDetailResponse DTO 생성에 필요한 Data를 최적화된 Sql 쿼리로 추출합니다.<br/>
        /// Db에서 정렬까지해서 반환해주고, 그 데이터들로 Service에서 Response로 변환합니다.
        /// </summary>
        /// <remarks>
        /// 1.해당 방의 참가자인게 확인되면 정보를 추출
        /// </remarks>
        /// <param name="roomId">조회할 채팅방 식별 번호</param>
        /// <param name="myEmail">조회할 사용자 Email</param>
        /// <returns>채팅방의 상세 정보를 담고있는 Response DTO</returns>
        private async Task<ChatRoomDetailResponse?> FetchChatRoomDetailAsync(Guid roomId, string myEmail)
        {
            // roomId 방의 기본 정보, 참여자 정보, 메세지 정보를 한번의 쿼리로 가져옴
            var rawData = await _context.ChatRooms
                .AsNoTracking() // 데이터를 읽어오기만하고 수정하진 않을거라 데이터 추적 프로세스는 건너뜀
                .Where(r => r.Id == roomId && _context.ChatParticipants
                                                                .Any(cp => cp.ChatRoomId == roomId && cp.UserEmail == myEmail))
                .Select(r => new
                {
                    Room = r,
                    Participants = _context.ChatParticipants
                        .Where(cp => cp.ChatRoomId == roomId)
                        .Select(cp => new ChatParticipantProjection
                        {
                            User = cp.User,
                            LastReadMessageId = cp.LastReadMessageId,
                            RenamedRoomName = cp.UserEmail == myEmail ? cp.RenamedRoomName : null,
                        }).ToList(),
                    Messages = _context.ChatMessages
                        .Where(m => m.ChatRoomId == roomId)
                        .OrderByDescending(m => m.Id)
                        .Take(50)
                        .ToList(),
                })
                .FirstOrDefaultAsync();
            if (rawData == null) return null;
            // 5번 Title 결정을 위해 필요한 내 정보
            ChatParticipant? myInfo = await GetParticipantEntityAsync(roomId, myEmail);
            if (myInfo == null) return null;

            // 2. 친구 관계 데이터는 별도로 가져오기
            List<string>? participantEmails = rawData.Participants.Select(p => p.User.Email).ToList();
            // 내 이메일을 기준으로 participantEmails에 등록된 사람들과의 Friendships 튜플들을 가져옴
            Dictionary<string, Friendship> friendships = await GetFriendshipsAsync(myEmail, participantEmails);

            // 3. FriendResponse 객체 생성
            List<FriendResponse> participantsResponses = FriendMapper.ToFriendResponseList(myEmail, rawData.Participants, friendships);

            // 4. MessageResponse 객체 생성
            List<ChatMessageResponse> messageResponses = ChatMapper.ToMessageResponseList(roomId, participantsResponses, rawData.Messages, rawData.Participants);

            // 5. Title 결정
            string displayTitle = ChatMapper.DetermineRoomTitle(rawData.Room, myInfo, participantsResponses, myEmail);
            string? originalTitle = null;
            // 6. 내가 설정한 채팅방 이름이 존재하면 OrigianlTitle 설정해야함
            if (!string.IsNullOrEmpty(myInfo.RenamedRoomName))
            {
                ChatParticipant temp = new() { RenamedRoomName = null };
                originalTitle = ChatMapper.DetermineRoomTitle(rawData.Room, temp, participantsResponses, myEmail);
            }

            return ChatMapper.ToChatRoomDetailResponse(rawData.Room, myInfo, participantsResponses, messageResponses, displayTitle, originalTitle);
        }
        #endregion Projection
        #endregion private Method
    }
}
