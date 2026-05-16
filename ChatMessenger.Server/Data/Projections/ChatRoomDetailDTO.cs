using ChatMessenger.Server.Data.Entities;

namespace ChatMessenger.Server.Data.Projections
{
    /// <summary>
    /// 채팅방 상세 정보 조회시 효율적인 데이터 추출을 위해 정의된 Projection입니다.
    /// </summary>
    /// <remarks>
    /// Db Entity의 원본을 가공하여 ChatRoomDetailResponse를 생성하기 위한 중간 DTO입니다.<br/>
    /// 복잡한 Join 결과를 단일 객체로 캡슐화하여 컨트롤러 로직을 간소화시킵니다.
    /// </remarks>
    public class ChatRoomDetailDTO
    {
        public ChatRoom Room { get; set; } = null!;
        public List<ChatParticipantProjection> Participants { get; set; } = new();
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
