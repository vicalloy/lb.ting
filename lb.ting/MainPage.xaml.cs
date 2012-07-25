using System;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.BackgroundAudio;
using Lb.Ting.Common;

namespace lb.ting
{
    public partial class MainPage : PhoneApplicationPage
    {
        private WebClient wcSongList = new WebClient();
        private WebClient wcSongDetailList = new WebClient();
        private Channel channel;//用于判断是否进行了电台切换

        // 构造函数
        public MainPage()
        {
            InitializeComponent();
            wcSongList.DownloadStringCompleted += GetSongListComplated;
            wcSongDetailList.DownloadStringCompleted += GetSongDetailListComplated;
            BackgroundAudioPlayer.Instance.PlayStateChanged += new EventHandler(Instance_PlayStateChanged);
            System.Windows.Threading.DispatcherTimer dt = new System.Windows.Threading.DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 1);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();
        }

        void dt_Tick(object sender, EventArgs e)
        {
            TimeSpan t = BackgroundAudioPlayer.Instance.Position;
            if (t != null)
            {
                txtTrackPos.Text = t.ToString("mm':'ss");
            }
            else
            {
                txtTrackPos.Text = "00:00";
            }
            
        }

        private void InitUI()
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    btnPlay.Content = "| |";     // Change to pause button
                    //btnPlay.IsEnabled = false;
                    break;

                case PlayState.Paused:
                case PlayState.Stopped:
                    btnPlay.Content = ">";     // Change to play button
                    break;
            }

            AudioTrack track = BackgroundAudioPlayer.Instance.Track;
            if (null != track)
            {
                txtCurrentTrack.Text = track.Title;
                txtArtist.Text = track.Artist;
                Utils.SetImageUrl(imgCover, track.AlbumArt, null);
            }
            Cfg cfg = Cfg.getCfg();
            if (cfg.channel != null) txtChannel.Text = "电台： " + cfg.channel.name;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //TODO 是否已经有设置电台
            //否：转到电台列表
            Cfg cfg = Cfg.getCfg();
            if (cfg.channel == null)
            {
                NavigationService.Navigate(new Uri("/ChannelPage.xaml", UriKind.Relative));
            }
            else
            {
                InitUI();
                UpdateTracks();
                //BackgroundAudioPlayer.Instance.Play();
            }
            //是：播放歌曲
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipNext();
            UpdateTracks();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    BackgroundAudioPlayer.Instance.Pause();
                    break;
                case PlayState.Paused:
                    BackgroundAudioPlayer.Instance.Play();
                    break;
                case PlayState.Stopped:
                    BackgroundAudioPlayer.Instance.SkipNext();
                    break;
            }
            InitUI();
        }

        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            InitUI();
        }

        private void GetSongDetailListComplated(object sender, DownloadStringCompletedEventArgs e)
        {
            SongDetailListRoot songsRoot = null;
            try
            {
                songsRoot = SongDetailListRoot.FromJson(e.Result);
            }
            catch (Exception)
            {
                UpdateTracks();
            }

            Utils.WriteFile(Cfg.SONGS_FILENAME, songsRoot.ToJson());
            if (!BackgroundAudioPlayer.Instance.PlayerState.Equals(PlayState.Playing))
            {
                BackgroundAudioPlayer.Instance.SkipNext();
            }
            //load more song
        }

        private void GetSongListComplated(object sender, DownloadStringCompletedEventArgs e)
        {
            SongListRoot songListRoot = null;
            try
            {
                songListRoot = SongListRoot.FromJson(e.Result);
            }
            catch (Exception)
            {
                UpdateTracks();
                return;
            }
            string url = Cfg.GET_SONGS_DETAIL_URL + songListRoot.result.GetSongIds();
            Utils.WebClientAsync(wcSongDetailList, url);
        }

        private void UpdateTracks()
        {
            Cfg cfg = Cfg.getCfg();
            //if change channel, stop music
            Channel c = cfg.channel;
            if (cfg.channelChanged && BackgroundAudioPlayer.Instance.Track != null)
            {
                BackgroundAudioPlayer.Instance.Stop();
            }
            cfg.channelChanged = false;
            cfg.write();
            String url = null;
            if (c.artistid != null)
            {
                url = Cfg.GET_ARTIST_SONG_URL + "&artistid=" + c.artistid;
            }
            else
            {
                url = Cfg.GET_CHANNEL_SONG_URL + "&channelid=" + c.channelid;
            }
            Utils.WebClientAsync(wcSongList, url);
        }

        private void btnChannel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ChannelPage.xaml", UriKind.Relative));
        }

    }

}