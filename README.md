## 💣 WPF Minesweeper (지뢰찾기)
C#과 WPF(Windows Presentation Foundation)를 기반으로<br>
MVVM 패턴을 준수하여 제작된 윈도우 지뢰찾기 아이디어를 기반으로 한 게임입니다. 
> LIGNex1 The SSEN 강의 중 C#와 WPF 강의를 토대로 제작했으며,<br>
>일부 내용은 Google Gemini의 도움을 받아 생성되었습니다.
---
## 👥 Members
* KangJunBeom : 프로젝트 총괄, MVVM 아키텍처 설계 및 게임 로직 구현, UI 디자인 담당
* ckstjrl : 음향 환경설정 및 음향 기능 구현 담당
* cheoljun99  : 멀티플레이 기능 구현 담당
* mindidii : 채팅 기능 구현 담당

|![준범](https://avatars.githubusercontent.com/u/80073573?v=4)|![찬석](https://avatars.githubusercontent.com/u/221036337?v=4)|![철준](https://avatars.githubusercontent.com/u/56952340?v=4)|![민지](https://avatars.githubusercontent.com/u/144782994?v=4)
|:---:|:---:|:---:|:---:|
| **강준범** <br> [KangJunBeom](https://github.com/KangJunBeom) <br>|**박찬석** <br> [ckstjrl](https://github.com/ckstjrl) <br>|**박철준** <br> [cheoljun99](https://github.com/cheoljun99)<br>|**김민지** <br>[mindidii](https://github.com/mindidii)|
---
#### 📺 미리보기 (Screenshots)
<img src="./img/로그인.png" width="250px" height="300px"></img>
<img src="./img/메인.png" width="250px" height="300px"></img>
<img src="./img/게임.png" width="250px" height="300px"></img>
<img src="./img/환경설정.png" width="250px" height="300px"></img>
---
### ✨ 핵심 기능 (Main Features)

#### 🎮 게임 플레이 로직
지뢰찾기 표준 규칙을 모두 준수합니다.
1. 빈 공간 자동 확장(Flood Fill)
2. 우클릭 깃발 토글 기능을 지원
3. 첫 클릭 안전 구역 보장

#### 🔥 난이도 시스템
- **쉬움** : 10x10 / 지뢰 6개
- **보통** : 20x20 / 지뢰 40개
- **어려움** : 30x30 / 지뢰 150개
- **극한** : 30x60 / 지뢰 400개

#### 👀 가독성 최적화
- 1~8번 숫자별 색상을 적용해 가독성 극대화

#### 🔖 데이터 및 시스템
- 🔐 **로그인 & 세션** : 사용자 닉네임을 기반으로 플레이 기록을 관리
- ⏱️ **실시간 타이머** : 첫 클릭 시점부터 종료 시점까지의 플레이 시간을 초 단위로 측정하고 시각적으로 표시

#### 💾 저장 및 이어하기 (Save & Load):
- **게임 도중 종료 시,** 현재 판의 상태와 진행 시간을 *savegame.json*에 저장
- **메인 메뉴 진입 시,** 저장 파일이 존재할 때만 '게임 이어하기' 버튼이 활성화
- **데이터 무결성 보장**을 위해 새 게임 시작 시 기존 저장 데이터를 자동으로 삭제하여 데이터 충돌 방지

#### 🏆 랭킹 시스템
- **난이도별 관리** : 각 난이도별로 가장 빠른 클리어 기록 3개를 보관
- **데이터 저장 방식** : rankings.json 파일을 통해 영구 저장되며, 1~3위에게 금(🥇), 은(🥈), 동(🥉) 메달을 부여
- **랭킹 초기화** : 랭킹 초기화를 통해 언제든지 다시 새로운 랭킹 기록에 도전할 수 있음

#### 🎬 시각적/음향 연출
- **승리 애니메이션** : 게임 클리어 시 화면이 어두워지며 "VICTORY!" 문구가 통통 튀어 오르는 스토리보드 기반 애니메이션이 실행
- **다이나믹 UI** : 시작 화면과 메인 메뉴는 크기가 고정되어 변경되지 않지만 게임 진입 시, 지뢰판 크기에 따라 윈도우 창 크기가 자동으로 변경
- **사운드 효과** : 기본적인 백그라운드 음악 설정이 가능하며, 승리/패배 시 SFX 재생

#### 🎬 실시간 멀티 P2P 지뢰 찾기 대결
- **승리 조건** : 맵상의 모든 지뢰를 빠르게 찾은 상대가 승리, 지뢰를 밣은 경우 각각 독립적으로 처음 부터 다시 시작
- **승리 애니메이션** : 상대방과 대결에서 승리 시 화면이 어두워지며 "VICTORY!" 문구가 통통 튀어 오르는 스토리보드 기반 애니메이션이 실행
- **패배 애니메이션** : 상대방과 대결에서 패배 시 화면이 어두워지며 "DEFEAT..." 문구가 통통 튀어 오르는 스토리보드 기반 애니메이션이 실행
- **TCP 기반 통신** : 클릭 이벤트 발생 시 통신 패킷을 상대방에게 전송하고 마찬가지로 상대방의 통신 패킷을 스레드로 받아서 실시간 화면 동기화 
- **세션 무결성 보장** : 나와 상대방의 통신 중 문제가 발생할 시 안전하게 멀티 플레이를 종료하고 로비 화면으로 화면 이동
<br>

#### 🛠️ 기술 스택 (Tech Stack)
- **Language** : C# (.NET)
- **Framework** : WPF
- **Pattern** : MVVM (Model-View-ViewModel)
- **Storage** : JSON (System.Text.Json)
<br>

#### 📂 프로젝트 구조
MinesweeperProject<br>
┣ 📂 Models<br>
┃ ┣ 📜 AudioService.cs  # 음향 기능 관련 속성<br>
┃ ┣ 📜 Cell.cs          # 지뢰 칸 속성 (IsMine, IsOpened 등)<br>
┃ ┣ 📜 SaveData.cs      # 저장 파일 구조체<br>
┃ ┣ 📜 RankingData.cs   # 랭킹 데이터 구조체<br>
┃ ┗ 📜 NetworkData.cs   # 네트워크 통신 데이터 구조체<br>
┣ 📂 View<br>
┃ ┣ 📜 GameView.xaml         # 지뢰판 및 애니메이션 레이어<br>
┃ ┣ 📜 LoginView.xaml        # 로그인 레이어<br>
┃ ┣ 📜 MainMenuView.xaml     # 메인메뉴 레이어<br>
┃ ┣ 📜 RankingView.xaml      # 명예의 전당 UI<br>
┃ ┣ 📜 SettingView.xaml      # 환경설정 레이어<br>
┃ ┣ 📜 MultiGameView.xaml    # 멀티 플레이 지뢰판 및 애니메이션 레이어<br>
┃ ┣ 📜 MultiSettingView.xaml # 멀티 플레이 방 생성 및 방 참가 레이어<br>
┃ ┗ 📜 WaitRoomView.xaml     # 멀티 플레이 대기실 레이어<br>
┣ 📂 ViewModels<br>
┃ ┣ 📜 GameViewModel.cs         # 지뢰 로직, 타이머, 승패 판정<br>
┃ ┣ 📜 LoginViewModel.cs        # 사용자 로그인 처리<br>
┃ ┣ 📜 MainMenuViewModel.cs     # 메인 메뉴 화면 전환 제어<br>
┃ ┣ 📜 MainViewModel.cs         # 화면 전환 및 윈도우 속성 제어<br>
┃ ┣ 📜 RankingViewModel.cs      # 랭킹 가공 및 메달 배정<br>
┃ ┣ 📜 SettingViewModel.cs      # 음향 효과에 대한 속성 제어<br>
┃ ┣ 📜 ViewModelBase.cs         # 특성 변화에 대한 판정<br>
┃ ┣ 📜 MultiGameViewModel.cs    # 멀티 플레이 통신, 지뢰 로직, 승패 판정<br>
┃ ┣ 📜 MultiSettingViewModel.cs # 멀티 플레이 서버 및 클라이언트 소켓 생성<br>
┃ ┗ 📜 WaitRoomViewModel.cs     # 멀티 플레이 통신 소켓 커넥트 및 수신 스레드 생성<br>
┣ 📂 Services<br>
┃ ┣ 📜 RelayCommand.cs          # 커맨드 바인딩 공통 클래스<br>
┃ ┣ 📜 SocketClientService.cs   # 클라이언트 소켓을 관리하는 클래스<br>
┃ ┗ 📜 SocketServerService.cs   # 서버 소켓을 관리하는 클래스<br>
┣ 📂 Resources<br>
┃ ┣ 📂 BGM    # 백그라운드 음악 파일 저장 폴더<br>
┃ ┗ 📂 SFX    # 효과음 파일 저장 폴더<br>
┗ 📜 App.xaml              # 데이터 템플릿(View-ViewModel 매핑) 정의<br>
(일부 생략)<br>
<br>

#### 🚀 실행 방법 (Getting Started)
- **필수** : Visual Studio 2022 이상, .NET 6.0 이상 환경이 필요합니다.
- **Build** : 솔루션 파일을 열고 Build (F6)를 수행합니다.
- **Run** : F5를 눌러 게임을 실행합니다.
- **Data** : 실행 파일 폴더 내에 savegame.json과 rankings.json이 자동 생성됩니다.
