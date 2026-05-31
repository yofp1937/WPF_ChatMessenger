using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.DTOs;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Services.Bases;
using ChatMessenger.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Services.Repositories
{
    /// <summary>
    /// ChatRoom Table과 관련된 요청을 처리해주는 Service입니다.
    /// </summary>
    public class ChatRoomRepositoryService : BaseRepositoryService, IChatRoomRepositoryService
    {
        public ChatRoomRepositoryService(AppDbContext context) : base(context) { }

        #region public Method

        /// <inheritdoc/>
        public async Task<ChatRoomSummaryDTO?> GetChatRoomSummaryDTOAsync(ChatParticipant participant)
        {
            return await ExecuteDbActionAsync(() => FetchChatRoomSummaryAsync(participant));
        }
        /// <inheritdoc/>
        public async Task<List<ChatRoomSummaryDTO>> GetChatRoomSummaryDTOListAsync(string userEmail)
        {
            return await ExecuteDbActionAsync(() => FetchChatRoomSummaryListAsync(userEmail));
        }
        /// <inheritdoc/>
        public async Task<bool> RemoveRoomAsync(ChatRoom room)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                _context.ChatRooms.Remove(room);
                return await _context.SaveChangesAsync() > 0;
            });
        }
        /// <inheritdoc/>
        public async Task<ChatRoom?> CreateChatRoomAsync(string? title, string? profileImageURL, bool isGroupChat)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                // 1. 새로운 채팅방 Entity 생성
                ChatRoom newRoom = new() { Title = title, RoomProfileImageURL = profileImageURL, IsGroupChat = isGroupChat };
                // 2. Db에 등록
                _context.ChatRooms.Add(newRoom);
                bool isSuccess = await _context.SaveChangesAsync() > 0;
                // 3. 등록 성공시 newRoom 반환, 실패시 null
                return isSuccess ? newRoom : null;
            });
        }
        /// <inheritdoc/>
        public async Task<bool> UpdateChatRoomAsync(ChatRoom room, Action<ChatRoom> updateAction)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                // 1. 외부에서 전달된 updateAction 실행
                updateAction(room);
                // 2. Db 저장 시도 후 결과 값 반환
                return await _context.SaveChangesAsync() > 0;
            });
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
                    LatestMessage = p.ChatRoom.Messages
                        .OrderByDescending(m => m.Id)
                        .Select(m => new { m.Content, m.SentAt })
                        .FirstOrDefault(),
                    // 채팅방의 참여자가 몇명인지 카운트
                    ParticipantCount = p.ChatRoom.Participants.Count(),
                    // 안읽은 메세지 수
                    UnreadCount = p.ChatRoom.Messages.Count(m =>
                        m.Id > p.LastReadMessageId &&
                        m.SenderEmail != myEmail &&
                        m.MessageType != ChatMessageType.System)
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
                            : x.Room.Participants
                                .Where(cp => cp.UserEmail != myEmail)
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
        /// <param name="participant">채팅방 정보를 조회할 User의 ChatParticipant Entity</param>
        /// <returns>ChatRoomSummaryResponse 생성에 필요한 DTO</returns>
        private async Task<ChatRoomSummaryDTO?> FetchChatRoomSummaryAsync(ChatParticipant participant)
        {
            // 내부적으로 사용할 변수 분리
            Guid roomId = participant.ChatRoomId;
            string myEmail = participant.UserEmail;
            // DTO 추출
            var lastMessage = await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId)
                .OrderByDescending(m => m.Id)
                .Select(m => new { m.Content, m.SentAt })       // 정렬된 Entity 전체를 가져오지 말고 Content와 SentAt만 꺼내서 익명 객체를 만들겠다.
                .FirstOrDefaultAsync();
            if (lastMessage == null) return null;

            return await _context.ChatRooms
                .AsNoTracking()
                .Where(r => r.Id == roomId)
                .Select(r => new ChatRoomSummaryDTO
                {
                    ChatRoomId = roomId,
                    ChatRoom = r,
                    IsGroupChat = r.IsGroupChat,

                    Title = participant.RenamedRoomName                                                                        // 1. 제목은 User가 따로 설정한 RenameRoomName이 우선
                        ?? (r.IsGroupChat                                                                                                 // 2. 커스텀 제목이 없는 경우 그룹 채팅인지, 1대1 채팅인지 확인
                        ? r.Title                                                                                                              // 3. 그룹 채팅인 경우 방을 생성할때 입력한 Title이 제목 
                        : r.Participants                                                                                                     // 4. 1대1 채팅인 경우 해당 채팅방의 다른 참가자 Nickname이 제목
                            .Where(cp => cp.UserEmail != myEmail)
                            .Select(cp => cp.User.Nickname)
                            .FirstOrDefault()) ?? "알 수 없는 사용자",

                    LastMessage = lastMessage.Content,
                    LastMessageSentAt = lastMessage.SentAt,
                    LastReadMessageId = participant.LastReadMessageId,

                    ParticipantCount = r.Participants.Count(),

                    UnreadCount = r.Messages.Count(m =>
                        m.Id > participant.LastReadMessageId &&
                        m.SenderEmail != myEmail &&
                        m.MessageType != ChatMessageType.System)
                })
                .FirstOrDefaultAsync();
        }
        #endregion Projection
    }
}
