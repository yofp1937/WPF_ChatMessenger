using ChatMessenger.Server.Data.Entities;

namespace ChatMessenger.Server.Data.Projections
{
    /// <summary>
    /// 채팅방 상세 정보 조회시, 필요한 데이터만 선별적으로 추출하기위한 클래스입니다.
    /// </summary>
    /// <remarks>
    /// DB 엔티티 전체를 가져오는 대신, 필요한 속성만 필터링하여 가져와 메모리 사용량을 최적화합니다.
    /// </remarks>
    public class ChatParticipantProjection
    {
        public required User User { get; set; }
        public long LastReadMessageId { get; set; }
        public string? RenamedRoomName { get; set; }
    }
}
