﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Lb.Ting.Common;

namespace lb.ting
{
    public partial class MainPage : PhoneApplicationPage
    {
        private WebClient wcSongList = new WebClient();
        private WebClient wcSongDetailList = new WebClient();
        private Boolean paused = false;//用于判断是否进行了电台切换
        private ApplicationBarIconButton btnPlay;

        // 构造函数
        public MainPage()
        {
            InitializeComponent();
            btnPlay = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;
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
                    btnPlay.Text = "暂停";
                    btnPlay.IconUri = new Uri("/Images/pause.png", UriKind.Relative);
                    //btnPlay.IsEnabled = false;
                    break;

                case PlayState.Paused:
                case PlayState.Stopped:
                    btnPlay.Text = "播放";
                    btnPlay.IconUri = new Uri("/Images/play.png", UriKind.Relative);
                    break;
            }

            AudioTrack track = BackgroundAudioPlayer.Instance.Track;
            if (null != track)
            {
                txtCurrentTrack.Text = track.Title;
                txtArtist.Text = track.Artist;
                imgCover.Source = Utils.GetImgFromUri(track.AlbumArt, null);
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
                return;
            }

            Utils.WriteFile(Cfg.SONGS_FILENAME, songsRoot.ToJson());
            if (!BackgroundAudioPlayer.Instance.PlayerState.Equals(PlayState.Playing) && !paused)
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
                paused = false;
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

        private void btnChannel_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ChannelPage.xaml", UriKind.Relative));
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    BackgroundAudioPlayer.Instance.Pause();
                    paused = true;
                    break;
                case PlayState.Paused:
                    BackgroundAudioPlayer.Instance.Play();
                    paused = false;
                    break;
                case PlayState.Stopped:
                    BackgroundAudioPlayer.Instance.SkipNext();
                    paused = false;
                    break;
            }
            InitUI();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipNext();
            UpdateTracks();
        }

        private void miAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void miMarketplaceReview_Click(object sender, EventArgs e)
        {
            new MarketplaceReviewTask().Show();
        }

    }

}