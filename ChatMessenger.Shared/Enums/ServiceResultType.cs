namespace ChatMessenger.Shared.Enums
{
    /// <summary>
    /// ServiceResult의 결과 반환 코드를 결정하는 enum입니다.
    /// </summary>
    public enum ServiceResultType
    {
        Success,                // Code 200: 요청 처리 성공
        BadRequest,          // Code 400: 잘못된 요청 (전달받은 데이터가 조건에 맞지 않음)
        Unauthorized,        // Code 401: 인증 실패 (로그인 정보, 토큰 정보 등 인증 실패)
        Forbidden,            // Code 403: 권한 부족
        NotFound,            // Code 404: 리소스 발견 실패 (Db에 정보 없음)
        InternalServerError  // Code 500: 서버 내부 오류 (Db 연결 끊김, 런타임 예외 등)
    }
}
