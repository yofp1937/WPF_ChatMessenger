using ChatMessenger.Server.Common.Interfaces.Chats;
using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Data.Projections;
using ChatMessenger.Server.Mappers;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using ChatMessenger.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Common.Services.Chats
{
    /// <summary>
    /// ChatRoom Table과 관련된 요청을 처리해주는 Service입니다.
    /// </summary>
    public class ChatRoomService : IChatRoomService
    {
        private readonly AppDbContext _context;
        
        public ChatRoomService(AppDbContext context)
        {
            _context = context;
        }

        #region public Method
        /// <inheritdoc/>
        public async Task<ChatRoom?> GetChatRoomAsync(Guid roomId)
        {
            return await _context.ChatRooms.Where(r => r.Id == roomId).FirstOrDefaultAsync();
        }
        /// <inheritdoc/>
        /// <remarks>
        /// 입력받은 두 이메일간의 개인 채팅방 식별 번호를 찾은 뒤, 식별 번호를 이용해 채팅방을 찾아서 반환해줍니다.
        /// </remarks>
        public async Task<ChatRoom?> GetPrivateChatRoomAsync(string email1, string email2)
        {
            // 1. email1, email2 사이에 개설된 개인 채팅방의 Guid 추출
            Guid roomId = await _context.ChatParticipants
                .Where(p => p.UserEmail == email1)                                                                    // 1. ChatParticipant에서 myEmail과 UserEmail 컬럼 값이 같은 모든 튜플 추출
                .Join(_context.ChatParticipants.Where(p => p.UserEmail == email2),                            // 2. ChatParticipant에서 targetEamil과 UserEmail 컬럼 값이 같은 모든 튜플 추출
                        me => me.ChatRoomId,
                        other => other.ChatRoomId,                                                                       // 3. 1번에서 얻은 결과를 me로 선언, 2번에서 얻은 결과를 other로 선언
                        (me, other) => me.ChatRoomId)                                                                  // 4. me와 other의 ChatRoomId가 같은 튜플만 골라서 me에서 ChatRoomId 추출
                .Where(roomId => _context.ChatRooms.Any(r => r.Id == roomId && !r.IsGroupChat))    // 5. ChatRoom 테이블에서 추출한 ChatRoomId와 RoomId가 같고 그룹채팅이 아닌 방 추출
                .FirstOrDefaultAsync();                                                                                       // 6. 그중 첫번째 데이터의 roomId를 내보낸다.
            if (roomId == Guid.Empty) return null;
            // 2. Guid로 방 찾아서 반환
            return await GetChatRoomAsync(roomId);
        }

        /// <inheritdoc/>
        public async Task<ChatRoom?> CreateChatRoomAsync(string? title, bool isGroupChat)
        {
            // 1. 새로운 채팅방 Entity 생성
            ChatRoom newRoom = new() { Title = title, IsGroupChat = isGroupChat };
            // 2. Db에 등록
            _context.ChatRooms.Add(newRoom);
            if (await _context.SaveChangesAsync() <= 0)
                return null;
            // 3. return
            return newRoom;
        }

        /// <inheritdoc/>
        public async Task<ChatRoomSummaryDTO?> GetChatRoomSummaryDTOAsync(string myEmail, Guid roomId)
        {
            return await FetchChatRoomSummaryAsync(myEmail, roomId);
        }
        /// <inheritdoc/>
        public async Task<List<ChatRoomSummaryDTO>> GetChatRoomSummaryDTOListAsync(string myEmail)
        {
            return await FetchChatRoomSummaryListAsync(myEmail);
        }
        /// <inheritdoc/>
        public async Task<ChatRoomDetailDTO?> GetChatRoomDetailDTOAsync(Guid roomId, string userEmail)
        {
            return await FetchChatRoomDetailAsync(roomId, userEmail);
        }
        /// <inheritdoc/>
        public async Task<bool> RemoveChatRoomAsync(Guid roomId)
        {
            ChatRoom? room = await _context.ChatRooms.FindAsync(roomId);
            if (room == null) return false;

            _context.ChatRooms.Remove(room);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateChatRoomAsync(ChatRoom room)
        {
            _context.Update(room);
            return await _context.SaveChangesAsync() > 0;
        }
        #endregion public Method
        #region Projection
        /// <summary>
        /// Db에서 ChatRoomSummaryResponse 생성에 필요한 Data를 최적화된 Sql 쿼리로 추출하여 DTO를 생성해 반환합니다.<br/>
        /// </summary>
        /// <remarks>
        /// * Db에서 정렬까지해서 반환해주므로 서버에서는 Response로 변환하여 Client에게 전송해주면 됩니다. <br/>
        /// 1. 프로젝션(Select)을 사용하여 필요한 컬럼만 SQL 수준에서 선별적으로 추출 <br/>
        /// 2. 익명 객체 프로젝션을 통해 테이블 중복 조회 방지 <br/>
        /// 3. 1차적으로 전송시간 기준 내림차순, 2차적으론 방 ID 기준 정렬까지 수행하여 외부에서 정렬 작업 불필요
        /// </remarks>
        /// <param name="myEmail">채팅방 목록을 조회할 사용자 Email</param>
        /// <returns>ChatRoomSummaryResponse 생성에 필요한 DTO</returns>
        private async Task<List<ChatRoomSummaryDTO>> FetchChatRoomSummaryListAsync(string myEmail)
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
                        m.SenderEmail != myEmail &&
                        m.MessageType != ChatMessageType.System) // 시스템 메세지는 제외
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
        /// Db에서 ChatRoomSummaryResponse 생성에 필요한 Data를 최적화된 Sql 쿼리로 추출하여 DTO를 생성해 반환합니다.<br/>
        /// </summary>
        /// <remarks>
        /// * Db에서 정렬까지해서 반환해주므로 서버에서는 Response로 변환하여 Client에게 전송해주면 됩니다. <br/>
        /// 1. 프로젝션(Select)을 사용하여 필요한 컬럼만 SQL 수준에서 선별적으로 추출 <br/>
        /// 2. 익명 객체 프로젝션을 통해 테이블 중복 조회 방지 <br/>
        /// 3. 1차적으로 전송시간 기준 내림차순, 2차적으론 방 ID 기준 정렬까지 수행하여 외부에서 정렬 작업 불필요
        /// </remarks>
        /// <param name="myEmail">채팅방 목록을 조회할 사용자 Email</param>
        /// <returns>ChatRoomSummaryResponse 생성에 필요한 DTO</returns>
        private async Task<ChatRoomSummaryDTO?> FetchChatRoomSummaryAsync(string myEmail, Guid roomId)
        {
            return await _context.ChatParticipants
                .AsNoTracking()
                .Where(p => p.UserEmail == myEmail && p.ChatRoomId == roomId) // 단일 방만 필터링
                .Select(p => new
                {
                    Participant = p,
                    Room = p.ChatRoom,
                    LatestMessage = _context.ChatMessages
                        .Where(m => m.ChatRoomId == p.ChatRoomId)
                        .OrderByDescending(m => m.Id)
                        .Select(m => new { m.Content, m.SentAt })
                        .FirstOrDefault(),
                    ParticipantCount = _context.ChatParticipants.Count(cp => cp.ChatRoomId == p.ChatRoomId),
                    UnreadCount = _context.ChatMessages.Count(m =>
                        m.ChatRoomId == p.ChatRoomId &&
                        m.Id > p.LastReadMessageId &&
                        m.SenderEmail != myEmail &&
                        m.MessageType != ChatMessageType.System)
                })
                .Select(x => new ChatRoomSummaryDTO
                {
                    ChatRoomId = x.Participant.ChatRoomId,
                    ChatRoom = x.Room,
                    IsGroupChat = x.Room.IsGroupChat,
                    Title = x.Participant.RenamedRoomName
                        ?? (x.Room.IsGroupChat
                            ? x.Room.Title
                            : _context.ChatParticipants
                                .Where(cp => cp.ChatRoomId == x.Participant.ChatRoomId && cp.UserEmail != myEmail)
                                .Select(cp => cp.User.Nickname)
                                .FirstOrDefault()) ?? "알 수 없는 사용자",
                    LastMessage = x.LatestMessage != null ? x.LatestMessage.Content : string.Empty,
                    LastMessageSentAt = x.LatestMessage != null ? x.LatestMessage.SentAt : null,
                    LastReadMessageId = x.Participant.LastReadMessageId,
                    ParticipantCount = x.ParticipantCount,
                    UnreadCount = x.UnreadCount,
                })
                .FirstOrDefaultAsync(); // 단일 객체이므로 FirstOrDefault
        }

        /// <summary>
        /// Db에서 ChatRoomDetailResponse 생성에 필요한 Data를 최적화된 Sql 쿼리로 추출하여 DTO를 생성해 반환합니다.
        /// </summary>
        /// <remarks>
        /// * Db에서 정렬까지해서 반환해주므로 서버에서는 Response로 변환하여 Client에게 전송해주면 됩니다. <br/>
        /// 1. 해당 방의 참가자인게 확인되면 정보를 추출
        /// </remarks>
        /// <param name="roomId">조회할 채팅방 식별 번호</param>
        /// <param name="userEmail">조회할 사용자 Email</param>
        /// <returns>ChatRoomDetailResponse 생성에 필요한 DTO</returns>
        private async Task<ChatRoomDetailDTO?> FetchChatRoomDetailAsync(Guid roomId, string userEmail)
        {
            // 1. roomId 방의 기본 정보, 참여자 정보, 메세지 정보를 한번의 쿼리로 가져옴
            return await _context.ChatRooms
                .AsNoTracking()
                .Where(r => r.Id == roomId && _context.ChatParticipants
                    .Any(cp => cp.ChatRoomId == roomId && cp.UserEmail == userEmail))
                .Select(r => new ChatRoomDetailDTO
                {
                    Room = r,
                    Participants = _context.ChatParticipants
                        .Where(cp => cp.ChatRoomId == roomId)
                        .Select(cp => new ChatParticipantProjection
                        {
                            User = cp.User,
                            LastReadMessageId = cp.LastReadMessageId,
                            RenamedRoomName = cp.UserEmail == userEmail ? cp.RenamedRoomName : null,
                        }).ToList(),
                    Messages = _context.ChatMessages
                        .Where(m => m.ChatRoomId == roomId)
                        .OrderByDescending(m => m.Id)
                        .Take(50)
                        .ToList(),
                })
                .FirstOrDefaultAsync();
        }
        #endregion Projection
    }
}
