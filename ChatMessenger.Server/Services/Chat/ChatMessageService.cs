using ChatMessenger.Server.Common.Interfaces.Chats;
using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Server.Common.Services.Chats
{
    public class ChatMessageService : IChatMessageService
    {
        private readonly AppDbContext _context;

        public ChatMessageService(AppDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<ChatMessage?> AddMessageAsnyc(string? userEmail, SendMessageRequest request)
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
            // 3. 등록 성공시 newMessage 반환, 실패시 null 반환
            if (await _context.SaveChangesAsync() > 0)
                return newMessage;
            return null;
        }
    }
}
