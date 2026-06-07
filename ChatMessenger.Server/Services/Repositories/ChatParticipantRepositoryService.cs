using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.DTOs;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Services.Bases;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Services.Repositories
{
    public class ChatParticipantRepositoryService : BaseRepositoryService, IChatParticipantRepositoryService
    {
        public ChatParticipantRepositoryService(AppDbContext context) : base(context) { }

        #region public Method
        /// <inheritdoc/>
        public async Task<ChatParticipant?> GetParticipantEntityAsync(Guid roomId, string userEmail)
        {
            return await ExecuteDbActionAsync(() =>
                _context.ChatParticipants
                .AsNoTracking()
                .Include(cp => cp.User)
                .Include(cp => cp.ChatRoom)
                .FirstOrDefaultAsync(cp => cp.ChatRoomId == roomId && cp.UserEmail == userEmail && !cp.IsLeft));
        }
        /// <inheritdoc/>
        public async Task<ChatParticipant?> GetTrackingParticipantEntityAsync(Guid roomId, string userEmail)
        {
            return await ExecuteDbActionAsync(() =>
                _context.ChatParticipants
                .Include(cp => cp.User)
                .Include(cp => cp.ChatRoom)
                .FirstOrDefaultAsync(cp => cp.ChatRoomId == roomId && cp.UserEmail == userEmail));
        }
        /// <inheritdoc/>
        public async Task<List<ChatParticipantProjection>> GetParticipantProjectionListAsync(Guid roomId)
        {
            return await ExecuteDbActionAsync(() =>
                // 1. 채팅방 참가자들의 Projection List 추출
                _context.ChatParticipants
                    .AsNoTracking()
                    .Where(cp => cp.ChatRoomId == roomId && !cp.IsLeft)
                    .Select(cp => new ChatParticipantProjection
                    {
                        User = cp.User,
                        LastReadMessageId = cp.LastReadMessageId,
                        RenamedRoomName = cp.RenamedRoomName,
                    })
                    .ToListAsync()
            );
        }
        /// <inheritdoc/>
        public async Task<bool> UpdateChatParticipantAsync(ChatParticipant participant, Action<ChatParticipant> updateAction)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                // 1. 외부에서 전달된 updateAction 실행
                updateAction(participant);

                // 2. Db 저장 시도 후 결과 값 반환
                return await _context.SaveChangesAsync() > 0;
            });
        }
        /// <inheritdoc/>
        public async Task<int> GetParticipantsCountAsync(Guid roomId)
        {
            return await ExecuteDbActionAsync(() =>
                _context.ChatParticipants.CountAsync(cp => cp.ChatRoomId == roomId && !cp.IsLeft));
        }
        /// <inheritdoc/>
        public async Task<List<ChatParticipantDTO>> GetParticipantDTOListAsync(Guid roomId)
        {
            return await ExecuteDbActionAsync(() =>
                // 1. DTO 추출
                _context.ChatParticipants
                    .AsNoTracking()
                    .Where(cp => cp.ChatRoomId == roomId && !cp.IsLeft)
                    .Select(cp => new ChatParticipantDTO
                    {
                        Email = cp.UserEmail,
                        Nickname = cp.User.Nickname,
                    })
                    .ToListAsync());
        }
        /// <inheritdoc/>
        public async Task<bool> RemoveParticipantAsync(ChatParticipant participant)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                // 2. Entitiy Remove 진행
                _context.ChatParticipants.Remove(participant);
                return await _context.SaveChangesAsync() > 0;
            });
        }
        /// <inheritdoc/>
        public async Task<bool> AddParticipantsToRoomAsync(Guid roomId, IEnumerable<string> emails, long entryMessageId)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                // 1. Email 추출해서 ChatParticipant 객체 생성
                IEnumerable<ChatParticipant> participants = emails.Select(email => new ChatParticipant
                {
                    ChatRoomId = roomId,
                    UserEmail = email,
                    EntryMessageId = entryMessageId,
                    LastReadMessageId = entryMessageId
                });
                // 2. Db에 등록
                _context.ChatParticipants.AddRange(participants);
                return await _context.SaveChangesAsync() > 0;
            });
        }
        /// <inheritdoc/>
        public async Task<ChatRoom?> GetPrivateChatRoomEntityAsync(string userEmail, string targetEmail)
        {
            return await ExecuteDbActionAsync(() =>
                _context.ChatParticipants
                    .AsNoTracking()
                    .Where(p => p.UserEmail == userEmail && !p.ChatRoom.IsGroupChat)
                    .Where(p => p.ChatRoom.Participants.Any(other => other.UserEmail == targetEmail))
                    .Select(p => p.ChatRoom)
                    .FirstOrDefaultAsync());
        }
        /// <inheritdoc/>
        public async Task<bool> ActivateParticipantIsLeftStatusIfPrivateAsync(Guid roomId, string myEmail, long messageId)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                List<ChatParticipant> participants = await _context.ChatParticipants
                    .Include(cp => cp.ChatRoom)
                    .Where(cp => cp.ChatRoomId == roomId)
                    .ToListAsync();
                // 1대1 채팅인 경우에만 동작
                if(participants.Count > 0 && !participants.First().ChatRoom.IsGroupChat)
                {
                    ChatParticipant? target = participants.FirstOrDefault(cp => cp.UserEmail != myEmail);
                    // 상대방이 퇴장 상태라면 메세지 전송을 위해 입장 상태로 변경
                    if (target != null && target.IsLeft)
                    {
                        target.IsLeft = false;
                        target.EntryMessageId = messageId;
                        target.LastReadMessageId = messageId - 1;
                    }
                    return await _context.SaveChangesAsync() > 0;
                }
                return false;
            });
        }
        #endregion public Method
    }
}
