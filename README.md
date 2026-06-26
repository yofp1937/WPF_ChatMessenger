# WPF_ChatMessenger 한 줄 소개
다른 유저를 친구로 등록하고 1대1 채팅, 그룹 채팅을 시작할 수 있는 간단한 메신저 프로그램

---

# 목차
 1. [개요](#1-개요)
 
 2. [프로그램 작동 영상과 설명](#2-프로그램-작동-영상과-설명)
 
 3. [주요 로직 설명](#3-주요-로직-설명)
 
 4. [개발 중 어려웠던 부분](#4-개발-중-어려웠던-부분)
 
 5. [아쉬웠던 점](#5-아쉬웠던-점)
 
 6. [업데이트 예정](#6-업데이트-예정)

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
 ### 3-1. 메세지 전송

  <img width="1355" height="347" alt="image" src="https://github.com/user-attachments/assets/a09cca85-ab35-4a8e-b769-f441c50c1a83" />

   -  ① Client가 메세지를 전송하면 Server에선 ChatService의 SendMessageAsync 메서드를 사용해 메세지 전송 요청을 처리합니다.
   -  부모 클래스에 작성된 ExecutedBusinessLogicAsnyc를 사용해 try-catch 내부에서 로직을 실행하고 Repository에서 오류 발생시 throw를 발생시켜 HandleException으로 로그를 남깁니다.

  <img width="608" height="388" alt="image" src="https://github.com/user-attachments/assets/0339fda6-1fc6-4bc2-a326-7d5e8e868317" />

   -  ② GetValidatedParticipantAsync 메서드로 메세지 전송자가 해당 방에 접근 권한이 있는지 확인합니다.

  <img width="1427" height="497" alt="image" src="https://github.com/user-attachments/assets/0cba5dad-74ff-477f-96a9-a34ce89a3bae" />

   -  ③ Transaction을 이용해 메세지 등록, 메세지 전송자의 마지막 읽은 메세지 식별 번호를 갱신합니다.
   -  부모 클래스에 작성된 ExecutedTransactionLogicAsnyc를 사용해 Transaction을 이용합니다.
  
   -  ④ Transaction이 성공적으로 실행됐으면 (①번 사진 4번 주석으로 이동)채팅방 참가자들의 Email을 추출하고, 생성된 여러 데이터들을 Client측에 필요한 데이터만 담긴 ChatMessageResponse로 매핑합니다.
  
  <img width="574" height="222" alt="image" src="https://github.com/user-attachments/assets/786396a1-e68e-475c-a090-9be5b01f0066" />

   -  ⑤ 이후 BroadcastToUsersAsync 메서드를 이용해 참가자들의 Eamil로 매핑된 Socket 라인에 ChatMessageResponse를 전송합니다.

 ### 3-2. 메세지 수신
 
 <img width="583" height="264" alt="image" src="https://github.com/user-attachments/assets/ca50e48f-61a3-4953-9a3c-bd3af9552d3b" />

  - ① 채팅방 상세 정보를 관리하는 ChatRoomViewModel이 생성되면 ChatHubService의 Action에 메서드를 연결합니다.

 <img width="735" height="328" alt="image" src="https://github.com/user-attachments/assets/a9bfa90d-fd09-4927-bf03-07e5ccbdfc78" />

  - ② 누군가 메세지를 전송해서 ChatHubService의 MessageReceivedEvent에 데이터가 도착하면 연결된 OnMessageReceived가 동작해 유효성 검사를 진행하고, Response를 Model 객체로 변환한 뒤,   
 
 ### 3-3. 메세지 읽음 처리
 
  - 설명
 
 ### 3-4. 채팅방 입장과 퇴장
 
  - 설명
 
 ### 3-5. 그룹 채팅방 생성
 
  - 설명
 
-----

# 4. 개발 중 어려웠던 부분
 
 ### 발생한 문제
  > [문제]<br/>
  문제

  > [원인]<br/>
  원인

  > [해결]<br/>
  해결
  
 ### ② 발생한 문제
  > [문제]<br/>
  문제

  > [원인]<br/>
  원인

  > [해결]<br/>
  해결

 ### ③ 발생한 문제
  > [문제]<br/>
  문제

  > [원인]<br/>
  원인
 
  > [해결]<br/>
  해결

 ### ④ 발생한 문제
  > [문제]<br/>
  문제

  > [원인]<br/>
  원인

  > [해결]<br/>
  해결

-----

# 5. 아쉬웠던 점
 ### ① 제목
  > 설명.<br/>

 ### ② 제목
  > 설명.<br/>

 ### ③ 제목
  > 설명.<br/>

-----

# 6. 업데이트 예정
 ### ① 메세지 스크롤 기능
  > 설명.<br/>

 ### ② 미디어 데이터 전송
  > 설명.<br/>

 ### ③ 토큰 유효시간 추가
  > 설명.<br/>

 ### ④ 사용자 패스워드 암호화
  > 설명.<br/>

 ### ⑤ 다중 사용자 접속 환경에서 네트워크 처리 속도 체크 및 개선
  > 설명.<br/>
