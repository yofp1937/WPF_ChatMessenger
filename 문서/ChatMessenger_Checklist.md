# 체크리스트 (턴바이턴 개발 진행 상황 트래커)

> 코드베이스 분석(2026-07-14) 결과를 기반으로 현재까지의 구현 완료 항목과, TODO 주석/미구현 화면으로 확인된 잔여 작업을 정리했습니다. 순서는 하위 로직(DB/Model) → 상위 로직(Service → ViewModel → View) 의존성 흐름을 따릅니다.

## 목차
* [1. 서버 - 데이터 계층](#1-서버---데이터-계층)
* [2. 서버 - Repository 계층](#2-서버---repository-계층)
* [3. 서버 - Service(비즈니스) 계층](#3-서버---service비즈니스-계층)
* [4. 서버 - Controller / API](#4-서버---controller--api)
* [5. 서버 - SignalR Hub](#5-서버---signalr-hub)
* [6. 서버 - 인증/인프라](#6-서버---인증인프라)
* [7. 클라이언트 - Model / DI / 공통 인프라](#7-클라이언트---model--di--공통-인프라)
* [8. 클라이언트 - Service 계층](#8-클라이언트---service-계층)
* [9. 클라이언트 - ViewModel / Messenger](#9-클라이언트---viewmodel--messenger)
* [10. 클라이언트 - View / Style](#10-클라이언트---view--style)
* [12. 향후 개선 권장 사항](#12-향후-개선-권장-사항)
* [13. 리팩토링 작업 (2026-07-14 진행)](#13-리팩토링-작업-2026-07-14-진행)
* [14. 향후 로드맵 (우선순위순)](#14-향후-로드맵-우선순위순)

---

## 1. 서버 - 데이터 계층
- [x] 1-1. `User` 엔티티 정의 (`Data/Entities/User.cs`)
- [x] 1-2. `ChatRoom` 엔티티 정의 (Guid PK, `NEWSEQUENTIALID()` 적용)
- [x] 1-3. `ChatParticipant` 엔티티 정의 (RenamedRoomName, EntryMessageId, LastReadMessageId, IsLeft)
- [x] 1-4. `ChatMessage` 엔티티 정의 (MessageType Enum, SenderEmail nullable)
- [x] 1-5. `Friendship` 엔티티 정의 (IsBlocked, IsFavorite)
- [x] 1-6. `AppDbContext`에 5개 DbSet 등록 및 `OnModelCreating` 관계/인덱스 설정
- [x] 1-7. `DbInitializer`로 `EnsureCreatedAsync()` + 테스트 데이터 시딩 구현

## 2. 서버 - Repository 계층
- [x] 2-1. `IBaseRepositoryService` / `BaseRepositoryService` (트랜잭션 시작 공통 로직)
- [x] 2-2. `UserRepositoryService` (이메일 조회, 신규 유저 추가, 닉네임 일괄 조회)
- [x] 2-3. `FriendshipRepositoryService` (친구 목록 프로젝션 쿼리 포함)
- [x] 2-4. `ChatRoomRepositoryService` (요약 정보 프로젝션, 채팅방 제목 결정 로직)
- [x] 2-5. `ChatParticipantRepositoryService` (AsNoTracking 분리, 1:1 채팅 재활성화 로직)
- [x] 2-6. `ChatMessageRepositoryService` (최근 50개 메시지 조회, 마지막 메시지 ID 조회)

## 3. 서버 - Service(비즈니스) 계층
- [x] 3-1. `BaseBusinessService` (예외 처리 템플릿, 트랜잭션 실행 템플릿, SignalR 브로드캐스트 헬퍼)
- [x] 3-2. `AuthService.RegisterAsync` / `LoginAsync`
- [x] 3-3. `TokenService.CreateToken` (JWT 발급, 8시간 유효)
- [x] 3-4. `ChatService.CreateGroupChatRoomAsync` / `GetOrCreatePrivateChatAsync`
- [x] 3-5. `ChatService.SendMessageAsync` (저장 + SignalR 브로드캐스트)
- [x] 3-6. `ChatService.UpdateLastReadedMessageAsync`
- [x] 3-7. `ChatService.RemoveParticipantAndCreateLeaveMessageAsync` (전원 퇴장 시 방 삭제 포함)
- [x] 3-8. `ChatService.AddParticipantsToRoomAsync` (초대)
- [x] 3-9. `SocialService` (친구 추가/삭제/즐겨찾기/차단 전체 기능)

## 4. 서버 - Controller / API
- [x] 4-1. `BaseController` / `AnonymousBaseController` / `AuthorizedBaseController` 계층 구조
- [x] 4-2. `AuthController` (login, register)
- [x] 4-3. `ChatController` (9개 엔드포인트 전체)
- [x] 4-4. `FriendController` (6개 엔드포인트 전체)

## 5. 서버 - SignalR Hub
- [x] 5-1. `ChatHub.JoinRoom` / `LeaveRoom` / `OnConnectedAsync`(이메일 그룹 자동 가입)
- [x] 5-2. `ChatHubEvents` 상수 정의 (Shared 프로젝트, 서버/클라이언트 공유)
- [x] 5-3. `Program.cs`에 `/chathub` 매핑

## 6. 서버 - 인증/인프라
- [x] 6-1. JWT Bearer 인증 미들웨어 등록 (`JWTConfig.cs`)
- [x] 6-2. `ServiceConfig.cs` DI 등록 (Business/Repository 전체)
- [x] 6-3. `Dockerfile` (Multi-stage 빌드, AWS 배포용)
- [x] 6-4. `appsettings.Production.json` 배포 환경 설정

## 7. 클라이언트 - Model / DI / 공통 인프라
- [x] 7-1. `FriendModel`, `ChatMessageModel`, `ChatRoomDetailModel`, `ChatRoomSummaryModel` (Models)
- [x] 7-2. `DependencyInjectionConfig` (리플렉션 기반 ViewModel/View 자동 등록)
- [x] 7-3. `App.xaml.cs` — `Application_Startup` 기반 동적 Window 생성
- [x] 7-4. `WindowService` / `WindowControlService` (창 이동/최대화/최소화, P/Invoke)
- [x] 7-5. `AuthHeaderHandler` (DelegatingHandler, JWT 자동 첨부)
- [x] 7-6. `Common/Converters` 8종 (Bool/Null/String → Visibility 등)

## 8. 클라이언트 - Service 계층
- [x] 8-1. `BaseService.ExecuteAsync` 공통 HTTP 실행 템플릿
- [x] 8-2. `AuthService(Client)` (SignInAsync, RegisterAsync)
- [x] 8-3. `ChatService(Client)` 9개 메서드 전체
- [x] 8-4. `FriendService(Client)` 6개 메서드 전체
- [x] 8-5. `IdentityService` (세션 토큰/프로필 싱글톤 보관)
- [x] 8-6. `ChatHubService` (ConnectAsync/DisconnectAsync/JoinRoomAsync/LeaveRoomAsync + 3개 이벤트 구독)

## 9. 클라이언트 - ViewModel / Messenger
- [x] 9-1. `BaseViewModel` (`CleanUp()` 공통 규약)
- [x] 9-2. `LoginViewModel` / `RegisterViewModel` (로그인/회원가입 + 화면 전환 메시지 발행)
- [x] 9-3. `MainWindowViewModel` (`ChangePageMessage`, `ForceLogoutMessage` 구독 및 페이지 전환)
- [x] 9-4. `MainShellViewModel` (탭 Navigate 커맨드, Logout 커맨드)
- [x] 9-5. `ListPanelViewModel` (좌측 탭 캐시 전환)
- [x] 9-6. `ContentPanelViewModel` (5종 메시지 구독 기반 우측 패널 전환)
- [x] 9-7. `FriendListViewModel` / `FriendDetailViewModel` (검색, 추가, 삭제, 즐겨찾기, 차단)
- [x] 9-8. `ChatListViewModel` (채팅방 목록, 실시간 이벤트 구독)
- [x] 9-9. `ChatRoomViewModel` (메시지 목록, 전송, 읽음 처리, 초대, 나가기)
- [x] 9-10. `CreateChatRoomViewModel` (그룹/1:1 채팅 생성, 초대 흐름)
- [x] 9-11. 14종 Messenger 메시지 클래스 전체 정의 및 발행/구독 연결

## 10. 클라이언트 - View / Style
- [x] 10-1. `MainWindowView` / `WindowCaptionView` (커스텀 타이틀바, 드래그 이동)
- [x] 10-2. `LoginView` / `RegisterView`
- [x] 10-3. `MainShellView` (좌/우 패널 분할 레이아웃)
- [x] 10-4. `FriendListView` / `FriendDetailView`
- [x] 10-5. `ChatListView` / `ChatRoomView` / `CreateChatRoomView`
- [x] 10-6. `Styles/*.xaml` DataTemplate 매핑 전체 (MainWindowStyle, MainShellStyle 등)
- [ ] 10-7. `SettingListView` / `SettingDetailView` 실제 설정 화면 구현 (현재 플레이스홀더 TextBlock만 존재)

---

## 12. 향후 개선 권장 사항
- [x] 12-1. `AuthService.cs:60` 비밀번호 평문 비교/저장 → 해시(PBKDF2) 적용으로 전환 (13-3/R-3에서 완료)
- [x] 12-2. `appsettings.Production.json`에 평문 커밋된 JWT Key / DB 비밀번호를 환경변수 또는 시크릿 매니저로 이전 완료(13-4/R-4). 실제 운영 DB 비밀번호는 사용자가 SSMS에서 `sa` 계정 값 순환 완료, 로컬 JWT 키도 새 값으로 회전 완료(운영 배포용 JWT 키는 사용자가 별도 생성 필요 — 남은 절차 안내됨)
- [x] 12-3. `ChatRoomViewModel.cs:69` `async void SetChatRoom` → `async Task` 전환 완료(13-6/R-6에서 완료)
- [x] 12-4. `EnsureCreatedAsync()` → EF Core Migrations로 전환 완료 — `dotnet-ef` 로컬 도구 설치, `InitialCreate` 마이그레이션 생성, `DbInitializer`가 `MigrateAsync()` 사용. 로컬·운영(13.53.43.132) DB 모두 삭제 후 재생성해 실기동 검증 완료(운영은 실제 배포 후 로그인·해시 저장까지 end-to-end 확인)
- [x] 12-5. `IWindowService` 접근제한자 `internal` → `public` 통일 완료(13-7/R-7에서 완료)
- [x] 12-6. `DbInitializer.ConnectionDbAsync`가 DB 연결 실패 시 로그 없이 크래시하던 버그 수정 — `CheckAndCreateTablesAsync` 호출을 try-catch 안으로 이동, `Console.WriteLine` 전체를 `ILogger`로 전환(R-8 검증 중 발견, 즉시 수정 완료)

---

## 13. 리팩토링 작업 (2026-07-14 진행)
> 전체 코드베이스 진단 결과, 아키텍처 골격은 견고하므로 전면 재작성 대신 가이드라인 위반·일관성 결함을 잘게 쪼개어 순차 개선합니다. (우선순위 순)

- [x] 13-1. **R-1** `ChatService.cs` 정리 — 필드 `readonly`화, 디버그 `Console.WriteLine` 제거, `Count()==0`→`!Any()` (빌드 검증 완료)
- [x] 13-2. **R-2** `BaseBusinessService`/`BaseRepositoryService` 로깅을 `Console.WriteLine` → `ILogger<T>` 표준 로깅으로 전환 (자식 8종 생성자 파급, 빌드 검증 완료, BackEnd §6)
- [x] 13-3. **R-3** `AuthService` 비밀번호 해시 적용 — `Rfc2898DeriveBytes`(PBKDF2) 채택, NuGet 불필요. `IPasswordHasher` 신설, `DbInitializer` 시딩 로직도 동기화 (빌드 검증 완료, 12-1과 동일)
- [x] 13-4. **R-4** `appsettings.Production.json`/`appsettings.json` 비밀값 외부화 — 환경변수(운영) + User Secrets(로컬) 전환, Config 클래스에 fail-fast 검증 추가 (빌드+실기동+테스트 검증 완료, 12-2와 동일. DB 비번 실제 순환은 사용자 조치 필요)
- [x] 13-5. **R-5** 식별자 오타 일괄 정정 — `HandleExcetpion`/`AddMessageAsnyc`/`ExecutedBusinessLogicAsync`/`ExecutedTransactionAsync`/`previouseId`/`previouseLastReadMessageId` 등 8개 파일 정정 + `SocialService` readonly 누락 추가 발견·정리 (빌드+테스트 검증 완료)
- [x] 13-6. **R-6** `ChatRoomViewModel.SetChatRoom` `async void`→`async Task` — 호출부(`ContentPanelViewModel`)도 기존 Discard 패턴에 맞춰 갱신 (빌드 검증 완료, 12-3과 동일)
- [x] 13-7. **R-7** `IWindowService` 접근제한자 명시 — `internal`(기본값) → `public`, 폴더 내 다른 6개 인터페이스와 통일 (빌드 검증 완료, 12-5와 동일)
- [x] 13-8. **R-8** 실행 환경별 로그 Provider 분리 및 영속화 구현 — Serilog(+File+Async+Debug Sink) 도입, `Configs/LoggingConfig.cs` 신설. Development는 Console/Debug만, Production은 Error 이상 로그를 `logs/` 파일에 영속 저장 (빌드+격리환경 Sink 검증+테스트 11/11 통과 완료. 배포 시 Docker 볼륨 마운트 필요 — 사용자 조치 항목)

---

## 14. 향후 로드맵 (우선순위순)
> 초기 기능 구현이 완료된 뒤, 포트폴리오 관점에서 "규모와 실패를 고려하는 설계"를 증명하기 위한 후속 작업입니다. 우선순위는 **① 다른 작업의 기반이 되는가 ② 실무·확장성 감각을 드러내는가**를 기준으로 매겼습니다. P1~P3이 서로 코드를 재사용하므로 이 순서로 진행하는 것을 전제로 분해했습니다. (착수 시점에 세부 API 시그니처는 재확정)

### P1 — 메시지 커서 페이징 + 무한 스크롤 (기반 작업)
현재 입장 시 최근 50개만 조회하고 이전 메시지를 불러올 수단이 없습니다. 이후 P3(재연결 gap 복구)이 이 조회 API를 그대로 재사용하므로 최우선입니다. 메시지는 계속 쌓이므로 offset(`Skip/Take`)이 아니라 **keyset(커서) 페이징**으로 구현합니다(offset은 새 메시지 유입 시 페이지가 밀려 중복·누락 발생). `ChatMessage.Id`가 `long` 단조 증가라 커서로 그대로 사용 가능합니다.
- [ ] 14-1. 서버: `ChatMessageRepositoryService`에 커서 기반 이전 메시지 조회 메서드 추가 (`WHERE Id < @cursor ORDER BY Id DESC` + 개수 제한, `AsNoTracking()`)
- [ ] 14-2. 서버: `ChatController`에 이전 메시지 조회 엔드포인트 추가 (예: `GET api/chat/messages/{roomId}?before={messageId}&size=50`)
- [ ] 14-3. 클라이언트: `ChatService(Client)` + `IChatService`에 이전 메시지 조회 메서드 추가
- [ ] 14-4. 클라이언트: `ChatRoomViewModel`에 상단 로드 커맨드 + 로딩 중복 방지 플래그(`_isLoadingOlder`) + 커서 상태 보관
- [ ] 14-5. View: `ChatRoomView` 스크롤 상단 도달 감지 + 프리펜드 시 스크롤 위치 보정(점프 방지)

### P2 — 부하 테스트 하네스 + 통신 최적화
"측정 → 병목 발견 → 개선 → 재측정" 사이클을 before/after 수치로 남기는 것이 목표입니다. WPF 클라이언트를 다수 띄우는 대신 **헤드리스 SignalR 커넥션**을 N개 생성하는 별도 콘솔 프로젝트로 구성합니다.
- [ ] 14-6. `ChatMessenger.LoadTester` 콘솔 프로젝트 신설(헤드리스 SignalR 클라이언트 N개 생성) + 솔루션(`*.sln*`) 등록 + 전체 빌드 검증
- [ ] 14-7. 봇 시나리오 구현: 로그인 → 방 입장 → 주기적 메시지 전송, 동시 접속 수·전송 주기를 파라미터화
- [ ] 14-8. baseline 측정: 메시지 e2e 지연 p50/p95/p99, 서버 CPU/메모리, SQL Server 커넥션 풀 고갈 시점 기록
- [ ] 14-9. 최적화①: 읽음 처리 호출 디바운스 — 현재 수신 메시지마다 POST 발생(`ChatRoomViewModel.cs:189`). 일정 시간 뭉쳐 마지막 위치만 1회 전송하도록 개선
- [ ] 14-10. 최적화②: `ChatService.SendMessageAsync`의 메시지당 트랜잭션 경로 부하 검토 → 병목이면 쓰기 배치(채널/큐로 모아 처리) 도입 여부 판단
- [ ] 14-11. 개선 후 재측정하여 14-8 지표와 before/after 비교표 작성

### P3 — 재연결 시 메시지 동기화(gap 복구)
`WithAutomaticReconnect`는 연결만 복구할 뿐, 끊긴 동안 놓친 메시지는 받지 못합니다. 재연결 시 마지막 수신 Id 이후를 REST로 당겨와 병합합니다(14-2 엔드포인트 재사용).
- [ ] 14-12. 클라이언트: 방별 마지막 수신 메시지 Id 보관
- [ ] 14-13. SignalR `Reconnected` 콜백에서 14-2 엔드포인트로 누락분 조회 후 목록 병합(중복 제거)

### P4 — Presence / 타이핑 인디케이터
- [ ] 14-14. `ChatHub.OnConnectedAsync`/`OnDisconnectedAsync`에서 접속 상태 전파(같은 유저 다중 접속 카운팅 처리 포함)
- [ ] 14-15. 타이핑 이벤트 SignalR 채널 추가 + 클라이언트 표시(디바운스/쓰로틀)

### P5 — 완성도 마감 (임팩트 낮음, 빈 구멍 메우기)
- [ ] 14-16. 채팅방 나가기 확인 다이얼로그 (`ChatRoomViewModel.cs:139` TODO)
- [ ] 14-17. 설정 화면 실구현 — 현재 플레이스홀더(`SettingListView`/`SettingDetailView`). 다크모드 토글 등
- [ ] 14-18. 창 닫기 시 시스템 트레이 이동 (`WindowViewModelBase.cs:43` TODO)
