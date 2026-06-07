using ChatMessenger.Shared.DTOs.Requests.Base;

namespace ChatMessenger.Shared.DTOs.Requests.Chat
{
    public class InviteParticipantsRequest : BaseRequest
    {
        public Guid RoomId { get; set; }
        public List<string> ParticipantEmails { get; set; } = new();
    }
}
