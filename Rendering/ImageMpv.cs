using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace XiboClient.Rendering
{
    /// <summary>
    /// libmpv(HwndHost) 기반 이미지 렌더러.
    /// mpv 모드에서 image 타입도 동일한 Win32 경로로 렌더링한다.
    /// </summary>
    class ImageMpv : Media
    {
        private MpvHost _mpvHost;
        private readonly string _filePath;
        private readonly bool _stretch;
        private readonly int _volume;
        private readonly bool _muted;

        public ImageMpv(MediaOptions options) : base(options)
        {
            _filePath = Uri.UnescapeDataString(options.uri).Replace('+', ' ');
            _stretch = options.Dictionary.Get("scaleType", "aspect").ToLowerInvariant() == "stretch";
            _volume = options.Dictionary.Get("volume", 100);
            _muted = options.Dictionary.Get("mute", "0") == "1";
        }

        public override void RenderMedia(double position)
        {
            Uri uri = new Uri(_filePath);
            if (uri.IsFile && !File.Exists(_filePath))
            {
                Trace.WriteLine(new LogMessage("ImageMpv", "RenderMedia: " + this.Id + ", File " + _filePath + " not found."));
                throw new FileNotFoundException();
            }

            _mpvHost = new MpvHost
            {
                Width = Width,
                Height = Height,
                Visibility = Visibility.Visible
            };

            base.RenderMedia(position);

            try
            {
                MediaScene.Children.Add(_mpvHost);

                _mpvHost.SetStretch(_stretch);
                _mpvHost.SetVolume(_volume);
                _mpvHost.SetMute(_muted);
                // Still image should stay visible for widget duration.
                _mpvHost.SetPause(true);
                _mpvHost.Load(_filePath);

                Trace.WriteLine(new LogMessage("ImageMpv", "RenderMedia: " + this.Id + " loaded."), LogType.Audit.ToString());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(new LogMessage("ImageMpv", "RenderMedia: " + ex.Message), LogType.Error.ToString());
                throw;
            }
        }

        public override void Stopped()
        {
            Trace.WriteLine(new LogMessage("ImageMpv", "Stopped: " + this.Id), LogType.Audit.ToString());

            if (_mpvHost != null)
            {
                _mpvHost.Dispose();
                _mpvHost = null;
            }

            base.Stopped();
        }
    }
}
