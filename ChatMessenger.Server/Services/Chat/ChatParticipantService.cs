using ChatMessenger.Server.Common.Interfaces.Chats;
using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Server.Common.Services.Chats
{
    public class ChatParticipantService : IChatParticipantService
    {
        private readonly AppDbContext _context;

        public ChatParticipantService(AppDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<ChatParticipant?> GetParticipantEntityAsync(Guid roomId, string myEmail)
        {
            return await _context.ChatParticipants.FirstOrDefaultAsync(cp => cp.ChatRoomId == roomId && cp.UserEmail == myEmail);
        }
        /// <inheritdoc/>
        public async Task<bool> AddParticipantsToRoomAsync(Guid roomId, IEnumerable<string> emails)
        {
            // 1. Email 추출해서 ChatParticipant 객체 생성
            IEnumerable<ChatParticipant> participants = emails.Select(email => new ChatParticipant
            {
                ChatRoomId = roomId,
                UserEmail = email
            });
            // 2. Db에 등록, 성공하면 true 반환
            _context.ChatParticipants.AddRange(participants);
            if (await _context.SaveChangesAsync() > 0)
                return true;
            // 3. 실패시 false 반환
            return false;
        }
        /// <inheritdoc/>
        public async Task<List<string>> GetParticipantEmailsAsync(Guid roomId)
        {
            return await _context.ChatParticipants
                .Where(cp => cp.ChatRoomId == roomId)
                .Select(cp => cp.UserEmail)
                .ToListAsync();
        }
        /// <inheritdoc/>
        public async Task<List<string>> GetParticipantNicknamesAsync(Guid roomId)
        {
            return await _context.ChatParticipants
                .Where(cp => cp.ChatRoomId == roomId)
                .Select(cp => cp.User.Nickname)
                .ToListAsync();
        }
        /// <inheritdoc/>
        public async Task<int> GetParticipantCountAsync(Guid roomId)
        {
            return await _context.ChatParticipants.CountAsync(cp => cp.ChatRoomId == roomId);
        }
        /// <inheritdoc/>
        public async Task<bool> UpdateParticipantAsync(ChatParticipant participant)
        {
            // 1. 변경 사항을 Db에 적용
            _context.ChatParticipants.Update(participant);
            // 2. Db 변경사항 저장
            return await _context.SaveChangesAsync() > 0;
        }
        /// <inheritdoc/>
        public async Task<bool> RemoveParticipantAsync(ChatParticipant participant)
        {
            _context.ChatParticipants.Remove(participant);
            return await _context.SaveChangesAsync() > 0;
        }
        /// <inheritdoc/>
        public async Task<bool> HasAnyParticipantsAsync(Guid roomId)
        {
            return await _context.ChatParticipants.AnyAsync(cp => cp.ChatRoomId == roomId);
        }
    }
}
