using System;
using System.IO;
using System.Windows.Media;

namespace MinesweeperProject.Models
{
    public class AudioService
    {
        private MediaPlayer _bgmPlayer = new MediaPlayer();
        private MediaPlayer _sfxPlayer = new MediaPlayer();

        public void PlayBGM(string fileName)
        {
            // Resources/BGM 폴더 내의 파일을 실행 경로 기준으로 찾음
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "BGM", fileName);
            if (File.Exists(path))
            {
                _bgmPlayer.Open(new Uri(path));
                _bgmPlayer.MediaEnded += (s, e) => { _bgmPlayer.Position = TimeSpan.Zero; _bgmPlayer.Play(); }; // 반복 재생
                _bgmPlayer.Play();
            }
        }

        public void SetBgmVolume(double volume) => _bgmPlayer.Volume = volume;
        public void SetSfxVolume(double volume) => _sfxPlayer.Volume = volume;

        public void SetBgmMute(bool isMuted) => _bgmPlayer.IsMuted = isMuted;
        public void SetSfxMute(bool isMuted) => _sfxPlayer.IsMuted = isMuted;

        // 효과음 재생 (지뢰 클릭 등에서 호출용)
        public void PlaySFX(string fileName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SFX", fileName);
            if (File.Exists(path))
            {
                _sfxPlayer.Open(new Uri(path));
                _sfxPlayer.Play();
            }
        }
    }

    // 노래 정보를 담는 간단한 클래스
    public class MusicItem
    {
        public string Title { get; set; }
        public string FileName { get; set; }
        public MusicItem(string title, string fileName) { Title = title; FileName = fileName; }
    }
}