using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinesweeperProject.Models;

namespace MinesweeperProject.ViewModels
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        private readonly AudioService _audioService = new AudioService();

        public ObservableCollection<MusicItem> BgmList { get; } = new ObservableCollection<MusicItem>
        {
            new MusicItem("기본 테마", "theme1.mp3"),
            new MusicItem("긴장감 넘치는 곡", "tension.mp3"),
            new MusicItem("평화로운 곡", "peaceful.mp3")
        };

        // 1. 전체 소리 On/Off
        private bool _isMasterSoundOn = true;
        public bool IsMasterSoundOn
        {
            get => _isMasterSoundOn;
            set
            {
                _isMasterSoundOn = value;
                _audioService.SetBgmMute(!value);
                _audioService.SetSfxMute(!value);
                OnPropertyChanged();
            }
        }

        // 2. 노래 선택
        private MusicItem _selectedBgm;
        public MusicItem SelectedBgm
        {
            get => _selectedBgm;
            set
            {
                _selectedBgm = value;
                if (value != null) _audioService.PlayBGM(value.FileName);
                OnPropertyChanged();
            }
        }

        // 3. 효과음 활성화
        private bool _isSfxEnabled = true;
        public bool IsSfxEnabled
        {
            get => _isSfxEnabled;
            set { _isSfxEnabled = value; _audioService.SetSfxMute(!value); OnPropertyChanged(); }
        }

        // 4. 볼륨 조절
        private double _bgmVolume = 50;
        public double BgmVolume
        {
            get => _bgmVolume;
            set { _bgmVolume = value; _audioService.SetBgmVolume(value / 100); OnPropertyChanged(); }
        }

        private double _sfxVolume = 50;
        public double SfxVolume
        {
            get => _sfxVolume;
            set { _sfxVolume = value; _audioService.SetSfxVolume(value / 100); OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}