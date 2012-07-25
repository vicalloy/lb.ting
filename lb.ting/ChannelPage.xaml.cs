using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Lb.Ting.Common;
using Microsoft.Phone.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace lb.ting
{
    public partial class ChannelPage : PhoneApplicationPage
    {
        private static WebClient webClient = new WebClient();
        public const String GET_CHANNEL_LIST_URL = "http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.radio.getCategoryList&format=json";

        private void GetChannelListFromNetComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            Utils.WriteFile(Cfg.CHANNELS_FILENAME, e.Result);
            InitChannelList(ChannelListRoot.FromJson(e.Result));
        }

        private ChannelListRoot LoadChannelList()
        {
            String s = Utils.ReadFile(Cfg.CHANNELS_FILENAME);
            if (s.Length > 0)
            {
                return ChannelListRoot.FromJson(s);
            }
            return null;
        }

        private void InitChannelListFromNet()
        {
            if (webClient.IsBusy)
                webClient.CancelAsync();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(GetChannelListFromNetComplete);
            webClient.DownloadStringAsync(new Uri(GET_CHANNEL_LIST_URL, UriKind.Absolute));
        }

        private Grid CreateChannelGrid(int channelCount)
        {
            Grid g = new Grid();
            if (channelCount == 0) return g;
            int rowCount = channelCount / 2 + 1;
            //3 column
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            // n row
            for (int i = 0; i < rowCount; i++)
            {
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
            }
            return g;
        }

        private void BtnSelChannelClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Cfg cfg = Cfg.getCfg();
            Channel c = (Channel)btn.Tag;
            cfg.updateChannelChanged(c);
            cfg.channel = c;
            cfg.write();
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void InitChannelList(ChannelListRoot clr)
        {
            foreach (ChannelList cl in clr.result)
            {
                PivotItem pi = new PivotItem();
                this.MainPivot.Items.Add(pi);
                pi.Header = cl.title;
                if (cl.channellist.Count == 0) return;
                Grid g = CreateChannelGrid(cl.channellist.Count);
                ScrollViewer sv = new ScrollViewer();
                sv.Content = g;
                pi.Content = sv;
                for (int i = 0; i < cl.channellist.Count; i++)
                {
                    Channel c = cl.channellist[i];
                    Button btn = new Button();
                    g.Children.Add(btn);
                    btn.Content = c.name;
                    btn.SetValue(Grid.ColumnProperty, i%2);
                    btn.SetValue(Grid.RowProperty, i/2);
                    btn.Click += BtnSelChannelClick;
                    btn.Tag = c;
                    ImageBrush ib = new ImageBrush();
                    String url = c.thumb;
                    if (url == null) {
                        url = c.avatar;
                    }
                    if (url == null)
                    {
                        //default img
                        continue;
                    }
                    Utils.SetImageUrl(ib, new Uri(url, UriKind.Absolute), null);
                    ib.Stretch = Stretch.None;
                    ib.AlignmentY = 0;//AlignmentY.Top
                    btn.VerticalContentAlignment = System.Windows.VerticalAlignment.Bottom;
                    btn.Background = ib;
                    btn.BorderBrush = null;
                    btn.Padding = new Thickness(2);
                }
            }
        }

        private void InitChannelList() 
        {
            ChannelListRoot clr = LoadChannelList();
            if (clr != null)  {
                InitChannelList(clr);
            } 
            else 
            {
                InitChannelListFromNet();
            }
        }

        public ChannelPage()
        {
            InitializeComponent();
            //TODO 显示等待界面
            //数据库中是否已经有播放列表
            //是，直接从数据库加载
            //否，获取播放列表保存到数据库，从数据库加载播放列表
            InitChannelList();
        }
    }
}