using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Phone.BackgroundAudio;

namespace Lb.Ting.Common
{
    public class Cfg
    {
        public const String CHANNELS_FILENAME = "channels.json";
        public const String SONGS_FILENAME = "songs.json";
        public const String CFG_FILENAME = "cfg.json";
        public const String GET_CHANNEL_LIST_URL = "http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.radio.getCategoryList&format=json";
        public const String GET_CHANNEL_SONG_URL = "http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.radio.getChannelSong&format=json";
        public const String GET_ARTIST_SONG_URL = "http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.radio.getArtistChannelSong&format=json";
        public const String GET_SONGS_DETAIL_URL = "http://ting.baidu.com/data/music/links?songIds=";

        public Channel channel { get; set; }
        public Boolean channelChanged { get; set; }

        public void updateChannelChanged(Channel c)
        {
            this.channelChanged = (this.channel == null || this.channel.channelid != c.channelid);
        }

        public static Cfg FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Cfg>(json);
        }

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Cfg getCfg()
        {
            Cfg cfg = null;
            try
            {
                cfg = FromJson(Utils.ReadFile(Cfg.CFG_FILENAME));
            }
            catch (Exception)
            {
                
            }
            if (cfg == null) 
                return new Cfg();
            return cfg;
        }

        public void write()
        {
            Utils.WriteFile(Cfg.CFG_FILENAME, ToJson());
        }
    }
    //GET_CHANNEL_LIST_URL
    public class Channel
    {
        public string name { get; set; }
        public string channelid { get; set; }
        public string thumb { get; set; }
        public string artistid { get; set; }
        public string avatar { get; set; }
    }

    public class ChannelList
    {
        public string title { get; set; }
        public int cid { get; set; }
        public List<Channel> channellist { get; set; }
    }

    public class ChannelListRoot
    {
        public int error_code { get; set; }
        public List<ChannelList> result { get; set; }
        public static ChannelListRoot FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ChannelListRoot>(json);
        }

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    //GET_CHANNEL_SONG_URL
    public class Song
    {
        public string songid { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public string thumb { get; set; }
        public string resource_type { get; set; }
    }

    public class SongList
    {
        public string channel { get; set; }
        public int channelid { get; set; }
        public object artistid { get; set; }
        public object avatar { get; set; }
        public string count { get; set; }
        public List<Song> songlist { get; set; }

        public string GetSongIds()
        {
            string ids = "";
            foreach (Song song in songlist)
            {
                ids += song.songid + ",";
            }
            return ids;
        }
    }

    public class SongListRoot
    {
        public int error_code { get; set; }
        public SongList result { get; set; }

        public static SongListRoot FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SongListRoot>(json);
        }

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    //Song Detail
    public class SongDetail
    {
        public int queryId { get; set; }
        public int songId { get; set; }
        public string songName { get; set; }
        public string artistId { get; set; }
        public string artistName { get; set; }
        public int albumId { get; set; }
        public string albumName { get; set; }
        public string songPicSmall { get; set; }
        public string songPicBig { get; set; }
        public string songPicRadio { get; set; }
        public string lrcLink { get; set; }
        public string version { get; set; }
        public int copyType { get; set; }
        public int time { get; set; }
        public int linkCode { get; set; }
        public string songLink { get; set; }
        public string showLink { get; set; }
        public string format { get; set; }
        public int rate { get; set; }
        public int size { get; set; }
        public string relateStatus { get; set; }
        public string resourceType { get; set; }
        public AudioTrack toTrack(string xcode)
        {
            String url = songLink + "?xcode=" + xcode;
            return new AudioTrack(new Uri(url, UriKind.Absolute),
                songName, artistName, albumName, new Uri(songPicBig, UriKind.Absolute));

        }
    }

    public class SongDetailList
    {
        public string xcode { get; set; }
        public List<SongDetail> songList { get; set; }
    }

    public class SongDetailListRoot
    {
        public int errorCode { get; set; }
        public SongDetailList data { get; set; }

        public static SongDetailListRoot FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SongDetailListRoot>(json);
        }

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
