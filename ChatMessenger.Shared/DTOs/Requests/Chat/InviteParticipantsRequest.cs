namespace ChatMessenger.Shared.DTOs.Requests.Chat
{
    public class InviteParticipantsRequest
    {
        public Guid RoomId { get; set; }
        public List<string> ParticipantEmails { get; set; } = new();
    }
}
