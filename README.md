# WPF_ChatMessenger 한 줄 소개
다른 유저를 친구로 등록하고 1대1 채팅, 그룹 채팅을 시작할 수 있는 간단한 메신저 프로그램

---

# 목차
 1. [개요](#1-개요)
 
 2. [프로그램 작동 영상과 설명](#2-프로그램-작동-영상과-설명)
 
 3. [주요 로직 설명](#3-주요-로직-설명)
 
 4. [아쉬웠던 점](#5-아쉬웠던-점)
 
 5. [업데이트 예정](#6-업데이트-예정)

 ---

# 1. 개요
 ### 1-1. 프로젝트 설명
 WPF와 MSSQL 연동, C# Web Server 구축하여 클라우드 서비스에 업로드하여 REST API, Socket 통신에대해 공부하기위해 진행한 개인 프로젝트입니다.
 
 ### 1-2. 개발 기간
　2026.03.18 ~ 26.06.08 (ver 1.0 - 기본 기능 구현 완료)

 ### 1-3. 사용 기술
  #### 　① C#, WPF(.NET)
  #### 　② MVVM 패턴
  #### 　③ Messenger 패턴
  #### 　④ Web API, SignalR
  #### 　⑤ JWT 인증
  #### 　⑥ EF Core, MS-SQL
  #### 　⑦ AWS EC2


 ### 1-4. 사용 라이브러리
  #### 　① MaterialDesignThemes
  #### 　② CommunityToolkit.Mvvm
  #### 　③ System.IdentityModel.Tokens.Jwt
  #### 　④ System.Net.Http
  #### 　⑤ Microsoft.AspNetCore.MVC
  #### 　⑥ Microsoft.AspNetCore.SignalR
  #### 　⑦ Microsoft.EntityFrameworkCore
  #### 　⑧ Microsoft.EntityFrameworkCore.SqlServer
  #### 　⑨ Microsoft.Extensions.DependencyInjection

-----

# 2. 프로그램 작동 영상과 설명
 ### 2-1. 회원 가입, 로그인
 
https://github.com/user-attachments/assets/4288449a-9ef2-41c8-8a6d-56f3a7148c3e

<img width="1123" height="491" alt="로그인, 회원가입" src="https://github.com/user-attachments/assets/802797a5-77b2-4c8e-a63d-4062b4b18031" />

  - 회원가입시 ViewModel에서 선제적으로 아이디, 닉네임, 비밀번호 유효성 검사를 진행합니다.
  - 로그인 성공 시 서버측 Service에서 해당 유저의 Token을 생성해 반환합니다.
  - 클라이언트측 Service에서 Token을 반환받으면 메세지 수신 알림을 받기위해 Socket 통신을 연결합니다.

 ### 2-2. 친구 목록, 친구 추가, 즐겨찾기 등록, 친구 삭제, 차단

https://github.com/user-attachments/assets/4f2d7b5f-b479-4a58-972a-58040a0415ad

<img width="1121" height="154" alt="친구 이벤트 drawio" src="https://github.com/user-attachments/assets/f187a2ed-3679-4bbd-8732-ebb87d8890f5" />

  - 친구 추가, 즐겨찾기, 친구 삭제, 차단 이벤트 발생으로 인한 친구 목록 변동은 위 데이터 흐름을 따릅니다.
  - 친구 관계는 친구 등록시 Friendship 테이블에 튜플을 생성하고, 친구 삭제시 Friendship 튜플을 삭제하는 방식으로 관리합니다.
  - Friendship 테이블에서 bool 타입으로 즐겨찾기와 차단 상태를 관리합니다.

 ### 2-3. 채팅
 #### 　① 채팅 목록

https://github.com/user-attachments/assets/2ff49dba-2308-47e7-bdbd-289f59540a89

<img width="1121" height="113" alt="채팅방 목록 drawio" src="https://github.com/user-attachments/assets/2747d1fc-c302-47e1-b7ef-e530a30a9b53" />

  - 유저가 로그인에 성공하면 가입된 채팅방 목록을 비동기로 가져옵니다.
  - 채팅 목록에서 채팅방 정렬 기준은 (① 읽지 않은 메세지 여부에 따라 내림차순 정렬, ② 마지막 메세지의 전송 시간 기준 내림차순 정렬) 두가지 입니다.
  - 채팅방에 입장시 어떤 유저가 해당 채팅방에 참여중인지 목록을 확인할 수 있습니다.

 #### 　② 개인 채팅

https://github.com/user-attachments/assets/6ebc0238-5645-4f74-806a-d6746ddc202c

<img width="1150" height="812" alt="메세지 전송, 수신 흐름도" src="https://github.com/user-attachments/assets/511ddfaf-c3bf-498f-a73d-31ebfed38b39" />

  - 왼쪽 화면은 공유기를 사용하는 데스크탑, 오른쪽 화면은 모바일 핫스팟을 사용하는 노트북으로 테스트 진행했습니다.
  - 채팅방에 입장시 실시간으로 메세지를 수신받기위해 해당 채팅방의 식별번호로 Socket 라인과 통신을 연결합니다.
  - 채팅방 탈퇴시 데이터를 삭제하지 않고 IsLeft 컬럼을 수정해 데이터 이력을 보존합니다.
  - 채팅방 재입장시 EntryMessageId 컬럼 값을 기점으로 이전 메세지의 노출을 차단하고 신규 메세지만 접근 가능하도록 설계했습니다.
  - 메세지를 전송해 Db에 등록할때는 트랜잭션을 사용해 (내 메세지 Db에 등록, 내 채팅방 참가 정보에서 마지막으로 읽은 메세지 식별번호 수정) 두 행동을 한번에 진행해서 내가 보낸 메세지는 바로 읽음 처리되도록 설계했습니다.
  - 각 요청에따른 Db 반영이 성공적으로 이루어지면 Socket 라인을 통해 실시간 채팅방에 입장중인 유저들에게 UI 갱신을 위해 데이터를 전송합니다.

 #### 　③ 그룹 채팅

https://github.com/user-attachments/assets/ebf248f3-1f4e-485c-9de8-0a6e5946576c
 
  - 개인 채팅과 동일한 로직으로 동작합니다.

 #### 　④ 그룹 채팅 생성

https://github.com/user-attachments/assets/92cf161d-5f6e-4d9c-ae3c-9e2e2b47c253

<img width="1121" height="511" alt="그룹 채팅 생성 흐름도" src="https://github.com/user-attachments/assets/367a6bea-56e1-4e06-9520-3459e5de3068" />

 
  - 왼쪽 화면은 공유기를 사용하는 데스크탑, 오른쪽 화면은 모바일 핫스팟을 사용하는 노트북으로 테스트 진행했습니다.
  - 그룹 채팅은 생성할때 유효성 검사를 진행해 채팅방 제목과 인원이 3명 이상인지 체크합니다.
  - 그룹 채팅방 생성시 ViewModel에서 선제적으로 채팅방 이름, 참가 인원 유효성 검사를 진행합니다.
  - 그룹 채팅방을 생성할때 여러 테이블에 데이터가 동시에 추가돼야해서 Transaction을 사용해 원자성을 보장했습니다.
  - Transaction이 성공적으로 이루어지면 Socket 라인을 통해 채팅방 참가 유저들에게 UI 갱신을 위해 채팅방 정보를 전송합니다.

-----

# 3. 주요 로직 설명
 ### 3-1. Service 계층화와 공통 예외 처리 메서드 설계
  #### ① 데이터 책임 분리를 위한 Service 계층화
   - BusinessService: API 컨트롤러의 요청을 처리하기위해 비즈니스 로직을 관리하고, 데이터를 가공해 응답 객체로 변환하는 역할을 전담합니다.
   - RepositoryService: 비즈니스 로직에 종속되지않고, Db에 직접 접근하여 데이터 CRUD를 처리하는 역할을 전담합니다.
   - 관심사 분리를 극대화해 Db 엔진이나 쿼리 구조가 변경되도 상위 비즈니스 로직은 영향을 받지 않는 느슨한 결합 구조를 구현했습니다

  #### ② ExecutedBusinessLogicAsync 메서드를 통한 공통 예외 처리
  
  <img width="608" height="847" alt="image" src="https://github.com/user-attachments/assets/738d1e21-d328-42de-84c9-0d8bf86d9255" />
  
   - try-catch 블록이 중복되어 코드가 비대해지는걸 방지하기위해 공통 예외 처리 메서드를 작성해 동일한 런타임 오류 발생시 어디서든 동일하게 예외 처리를 진행하도록 설계했습니다.
  > [매커니즘]<br/>
  ① RepositoryService에서 요청한 데이터를 찾는데 실패해 null 반환시 throw 던짐<br/>
  ② throw 발생시 ExecutedBusinessLogicAsync가 catch<br/>
  ③ HandleException이 동작하여 호출 클래스명, 호출 메서드명, 타임 스탬프가 결합된 로그를 찍고, 규격화된 오류 상태 객체를 반환함

  #### ③ ExecutedTransactionAsync 메서드를 통한 데이터 불일치 방지
  
  <img width="737" height="482" alt="image" src="https://github.com/user-attachments/assets/a906485b-f0a4-43ce-9ff3-0c033901c3b6" />
  
   - 여러 Db 테이블에 동시 데이터 삽입이 발생할 때, 중간에 오류가 발생해 일부 데이터만 반영되는 데이터 불일치 현상을 방지하기위해 메서드를 작성했습니다.

 ### 3-2. 공통 결과 래퍼 객체를 설계해 예외 처리 및 비즈니스 규격화
 
 <img width="775" height="582" alt="image" src="https://github.com/user-attachments/assets/7f77bbae-12c4-4474-afe5-868cae1bf7ab" />
 
   - 기존 BusinessService에서 요청받은 비즈니스 로직을 실행하는데 실패해 null을 반환했을때, 어떤 이유로 실패했는지 외부에서 알 방법이 없어서 공통 결과 래퍼 객체를 설계했습니다.
   - 오류 발생시 오류 코드와, 에러 메세지를 주입해 반환하면 외부에서 이를 이용해 View에 오류 내용을 표시하거나, 분기를 나눠 데이터를 처리할 수 있게 만들었습니다.
 
 ### 3-3. HTTP API와 SignalR(WebSocket) 하이브리드 기반 실시간 메세지 송수신 파이프라인
  #### ① HTTP API와 실시간 소켓 채널의 역할 분담 및 결합
   - 메세지 전송: HTTP POST API 요청을 사용해 데이터를 Db에 저장합니다.
   - 메세지 수신: 서버 내부에서 전송된 메세지 저장이 끝나면 실시간 채팅에 참여중인 유저들의 SignalR Socket 라인으로 메세지 패킷을 전송합니다.
   
  #### ② 브로드캐스팅 채널 분리
  
  <img width="708" height="206" alt="image" src="https://github.com/user-attachments/assets/7588d1b8-eed4-45e8-92a9-65256052e08f" />

   - 사용자가 특정 채팅방에 입장하면 해당 채팅방의 식별 번호(RoomId) 소켓 채널에 가입됩니다.
   - 이후 누군가 메세지를 읽으면 채팅방 채널 브로드캐스팅을 통해 "읽음 상태 업데이트" 메서드가 동작해 채팅방에 입장중인 유저들의 화면만 UI 업데이트가 실행됩니다.

<img width="572" height="222" alt="image" src="https://github.com/user-attachments/assets/e12accb7-781d-442f-837b-cc71f2821ba2" />

   - 사용자가 로그인하면 사용자 이메일 소켓 채널에 가입됩니다.
   - 이후 누군가 메세지를 보내면 이메일 채널 브로드캐스팅을 통해 "메세지 수신" 메서드가 동작해 읽지 않은 메세지 카운트, 새 채팅방 초대 알림 등 UI 업데이트가 실행됩니다.
 
 ### 3-4. 논리 삭제를 통한 채티방 입,퇴장 상태 관리 및 진입점 제한
  #### ① 논리 삭제 기반 기존 데이터 유실 방지
  - 유저가 채팅방을 나갈때 Db에서 참가 데이터를 삭제하면 해당 유저의 메세지 삭제 등으로 인한 데이터 무결성이 파괴되는 문제가 있었습니다.
  - 이를 방지하기위해 채팅방 참가자 데이터를 관리하는 'ChatParticipant' 테이블에 IsLeft 컬럼을 추가해 논리 삭제 패턴으로 데이터 유실을 방지했습니다.

  #### ② EntryMessageId 컬럼 도입을 통한 진입점 제한
  - 채팅방에 새로 입장하거나 퇴장했던 유저가 동일 채팅방에 재입장할 경우, 과거 대화 내역 유출을 차단하기 위해 입장 시점의 마지막 메세지 식별 번호를 EntryMessageId에 등록합니다.
  - 클라이언트가 과거 대화 내역을 조회하는 쿼리를 요청할때, EntryMessageId보다 메세지 식별 번호가 큰 메세지만 조회할 수 있게해 과거 대화 내역 접근을 차단했습니다. 
 
 ### 3-5. 메신저를 통한 화면 전환 및 메모리 누수 차단
 
 <img width="957" height="778" alt="image" src="https://github.com/user-attachments/assets/7352e60f-6d0e-43ae-a86e-add7f1dc0fa4" />

  #### ① Messenger를 이용한 화면 전환
  - 메인 윈도우와 뷰모델간의 직접적인 참조를 제거하기위해 메세지를 사용해 화면 전환을 요청하게 설계했습니다.

  #### ② 메모리 누수 차단
  - 뷰모델이 파괴될때 Register로 연결한 이벤트들이 해제되지않아 발생하는 메모리 누수를 방지하기위해 자원 정리를 담당하는 CleanUp 메서드를 작성했습니다.
  - 강제 로그아웃이나 뷰모델이 파괴되면 최상위 뷰모델의 CleanUp이 호출되고 상위 뷰모델들은 하위 뷰모델들의 CleanUp을 호출하여 연쇄적으로 자원을 해제하게끔 설계했습니다.

 ### 3-6. DelegatingHandler를 사용한 JWT 토큰 자동 주입
 
 <img width="1153" height="546" alt="image" src="https://github.com/user-attachments/assets/d2e859a0-86ed-4458-b618-23e997f82f84" />

  - HTTP API 요청을 진행하는 Service에서 매번 수동으로 토큰을 주입하는 중복 코드가 발생해 HTTP 요청을 중간에 가로채 Header에 토큰을 주입해주는 AuthHeaderHandler를 구현했습니다.
  - 메모리에 토큰이 존재하지 않으면 HTTP API 요청을 전송하지않고 자체 임시 응답 객체를 생성해 리턴합니다.(불필요 네트워크 신호 전송, 서버 인증 연산 차단)
  - 검증이 끝난 HTTP API 요청에만 Header에 토큰을 추가한 뒤 base.SendAsync를 사용해 안전하게 패킷을 전송합니다.
  - AuthHeaderHandler가 자동으로 API 요청을 가로채 Header에 토큰을 추가해주므로 Service 클래스들은 순수 데이터 요청 로직에만 집중할 수 있는 관심사 분리를 설계했습니다.

-----

# 4. 아쉬웠던 점
 ### ① 로직, 아키텍처 문서화 부재로 인해 개발 시간 증가
  > [문제 현상]<br/>
  명확한 데이터 흐름이나 예외 시나리오 등 중요 로직들을 정리하지 않은채 코딩을 시작했습니다.<br/>
  개발 중 예상치 못한 버그가 발생하여 테스트 -> 버그 발견 -> 수정 과정을 반복하거나<br/>
  우선 기능을 구현한 뒤 서비스 계층화 혹은 메서드 재사용을 위한 리팩토링을 반복하다보니 개발 시간이 과해진것같습니다.

  > [교훈 및 다짐]<br/>
  이전 프로젝트를 통해서도 문서화의 중요성을 체감했지만, 빨리 포트폴리오를 완성하고자하는 조급함에 정리 없이 진행한것이 오히려 시간을 더 소모하게 만든걸 다시금 깨달았습니다.<br/>
  이후 프로젝트에서는 개발전 세부적인 로직들까지 정리하진 못해도 큰 틀의 아키텍처, 데이터 흐름도 등은 정리해둔채 시작해보겠습니다.

-----

# 5. 업데이트 예정
 ### ① 메세지 스크롤 기능
  > 채팅방 입장시 메세지 50개만 읽어오는데 최상단 메세지에서 다시 스크롤하면 메세지 30개씩 로딩해오는 기능.<br/>

 ### ② 미디어 데이터 전송
  > 이미지, 동영상 전송<br/>

 ### ③ 토큰 유효시간 추가
  > 토큰 유효시간 추가해서 1시간마다 재발급해주기<br/>

 ### ④ 사용자 패스워드 암호화
  > 패스워드 원본을 db에 저장하지않고 Client측에서 암호화, 복호화 진행해서 Server와 통신하게 만들기<br/>

 ### ⑤ 다중 사용자 접속 환경에서 네트워크 처리 속도 체크 및 개선
  > AI 만들어서 메세지 패킷 여러개 전송하면서 네트워크 처리 속도 확인하고 개선해보기<br/>
