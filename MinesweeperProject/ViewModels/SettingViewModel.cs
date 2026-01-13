using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MinesweeperProject.Models;
using MinesweeperProject.Services;

namespace MinesweeperProject.ViewModels
{
    // 1. ViewModelBase 상속으로 변경 (SetProperty 사용 가능)
    public class SettingViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainParent;
        private readonly AudioService _audioService = AudioService.Instance;

        public ObservableCollection<MusicItem> BgmList { get; } = new ObservableCollection<MusicItem>
        {
            new MusicItem("Drum_Or_Bass", "Drum_Or_Bass.mp3"),
            new MusicItem("Brave", "Brave.mp3"),
            new MusicItem("Numb_Sky", "Numb_Sky.mp3"),
            new MusicItem("On_The_Eve", "On_The_Eve.mp3"),
            new MusicItem("Scale_The_Well", "Scale_The_Well.mp3")
        };

        // 돌아가기 커맨드 추가
        public ICommand ReturnToMenuCommand { get; }

        // 2. 생성자에서 MainViewModel을 받도록 수정
        public SettingViewModel(MainViewModel mainParent)
        {
            _mainParent = mainParent;

            // 메인 메뉴로 돌아가는 커맨드 연결
            ReturnToMenuCommand = new RelayCommand(o =>
            {
                // MainViewModel에 정의된 ShowMainMenuView 호출
                _mainParent.ShowMainMenuView(_mainParent.Nickname ?? "Guest");
            });

            // 초기 값 설정 (예시)
            _selectedBgm = BgmList[0];
        }

        #region 오디오 설정 프로퍼티 (기존 로직 유지하되 SetProperty 적용)

        private bool _isMasterSoundOn = true;
        public bool IsMasterSoundOn
        {
            get => _isMasterSoundOn;
            set
            {
                if (SetProperty(ref _isMasterSoundOn, value))
                {
                    _audioService.SetBgmMute(!value);
                    _audioService.SetSfxMute(!value);
                }
            }
        }

        private MusicItem _selectedBgm;
        public MusicItem SelectedBgm
        {
            get => _selectedBgm;
            set
            {
                if (SetProperty(ref _selectedBgm, value) && value != null)
                {
                    _audioService.PlayBGM(value.FileName);
                }
            }
        }

        private bool _isSfxEnabled = true;
        public bool IsSfxEnabled
        {
            get => _isSfxEnabled;
            set { if (SetProperty(ref _isSfxEnabled, value)) _audioService.SetSfxMute(!value); }
        }

        private double _bgmVolume = 50;
        public double BgmVolume
        {
            get => _bgmVolume;
            set { if (SetProperty(ref _bgmVolume, value)) _audioService.SetBgmVolume(value / 100); }
        }

        private double _sfxVolume = 50;
        public double SfxVolume
        {
            get => _sfxVolume;
            set { if (SetProperty(ref _sfxVolume, value)) _audioService.SetSfxVolume(value / 100); }
        }

        #endregion
    }
}