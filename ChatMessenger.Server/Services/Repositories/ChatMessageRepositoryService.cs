using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Services.Bases;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Services.Repositories
{
    public class ChatMessageRepositoryService : BaseRepositoryService, IChatMessageRepositoryService
    {
        public ChatMessageRepositoryService(AppDbContext context) : base(context) { }
        /// <inheritdoc/>
        public async Task<List<ChatMessage>> GetLastFiftyMessageListAsync(Guid roomId, long entryMessageId)
        {
            return await ExecuteDbActionAsync(() =>
                // 1. 채팅방의 최근 메세지 50개 추출
                _context.ChatMessages
                    .AsNoTracking()
                    .Where(cm => cm.ChatRoomId == roomId && cm.Id >=  entryMessageId)
                    .OrderByDescending(cm => cm.SentAt)         // 최근 메세지 순서로 정렬
                    .Take(50)                                                // 50개 추출
                    .OrderBy(cm => cm.SentAt)                        // 다시 과거 -> 최신 메세지 순서로 정렬
                    .ToListAsync());
        }
        /// <inheritdoc/>
        public async Task<ChatMessage?> AddMessageAsnyc(string? userEmail, SendMessageRequest request)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                // 1. 등록할 메세지 Entity 생성
                ChatMessage newMessage = new()
                {
                    ChatRoomId = request.RoomId,
                    MessageType = request.MessageType,
                    SenderEmail = request.MessageType == ChatMessageType.System ? null : userEmail,
                    Content = request.Content
                };
                // 2. Db에 등록
                _context.ChatMessages.Add(newMessage);
                bool isSuccess = await _context.SaveChangesAsync() > 0;
                // 3. 등록 여부에따라 return
                return isSuccess ? newMessage : null;
            });
        }
        /// <inheritdoc/>
        public async Task<long> GetLastMessageIdAsync(Guid roomId)
        {
            return await ExecuteDbActionAsync(() =>
                _context.ChatMessages
                    .Where(m => m.ChatRoomId == roomId)
                    .OrderByDescending(m => m.Id)
                    .Select(m => m.Id)
                    .FirstOrDefaultAsync()
            );
        }
    }
}
