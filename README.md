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
<img width="1123" height="453" alt="회원가입, 로그인 drawio" src="https://github.com/user-attachments/assets/bd634d70-4d4b-4889-98da-bf79036b6e88" />
  - 회원가입시 아이디, 닉네임, 비밀번호 유효성 검사를 진행합니다.
  - 클라이언트측 Service에서 입력받은 데이터로 Request를 생성해 API를 요청합니다.
  - 서버측 Service에서 Request 요청을 처리하고, Response를 생성해 Controller에게 반환합니다.
  - 로그인 성공 시 해당 유저의 Token을 생성해 반환합니다.
  - 클라이언트측 Service에서 Token을 반환받으면 메세지 수신 알림을 받기위해 Socket 통신을 연결합니다.

 ### 2-2. 친구
[https://영상](https://영상)
 
  - 설명

 #### 　① 친구 목록
[영상](https://영상)
 
  - 설명

 #### 　② 친구 등록
[영상](https://영상)
 
  - 설명

 #### 　③ 친구 삭제
[영상](https://영상)
 
  - 설명

 #### 　④ 차단
[영상](https://영상)
 
  - 설명

 ### 2-3. 채팅
[https://](https://영상)
 
  - 설명
 #### 　① 채팅 목록
[영상](https://영상)
 
  - 설명

 #### 　② 개인 채팅
[영상](https://영상)
 
  - 설명

 #### 　③ 그룹 채팅
[영상](https://영상)
 
  - 설명

 #### 　④ 그룹 채팅 생성
[영상](https://영상)
 
  - 설명

-----

# 3. 주요 로직 설명
 ### 3-1. 메세지 전송/수신
  
  - 설명

 ### 3-2. 메세지 읽음 처리
 
  - 설명
 
 ### 3-3. 채팅방 입장과 퇴장
 
  - 설명
 
 ### 3-4. 그룹 채팅방 생성
 
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
