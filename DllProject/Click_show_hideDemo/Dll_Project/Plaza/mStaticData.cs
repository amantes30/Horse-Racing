using System;
using System.Collections.Generic;
using System.Text;

namespace Dll_Project.Plaza
{
    public class mStaticData
    {
        public static ChannelInformatica CannelInformatica = new ChannelInformatica();
        public static AllURL AllURL = new AllURL();
        public static Identity Identity = new Identity();
        public static List<SceneImage> SceneImage = new List<SceneImage>();
        public static List<AllMusic> AllMusics = new List<AllMusic>();

        public static string PhotoFileName;
    }

    #region 频道信息
    public class ChannelInformatica
    {
        public List<Channel> OffLineCannels = new List<Channel>();
        public List<Channel> AChannels = new List<Channel>();//展厅A
        public List<Channel> BChannels = new List<Channel>();//展厅B
        public List<Channel> CChannels = new List<Channel>();//展厅C
        public List<Channel> DChannels = new List<Channel>();//会
    }
    public class Channel
    {
        public int ID { get; set; }
        public string RootRoomID { get; set; }
        public string RootVoiceID { get; set; }
        public string Name { get; set; }
        public string RoomURL { get; set; }
        public int AreaMaxCount { get; set; }
    }
    #endregion

    #region URL信息
    public class AllURL
    {
        public string MyInfoURL { get; set; }
        public string ExchangeCard { get; set; }
        public string TakePhotoURL { get; set; }
    }
    #endregion

    #region VIP账号信息
    public class Identity
    {
        public bool IsConfig { get; set; }
        public int VIPCount { get; set; }
        public List<string> VIP = new List<string>();
        public List<string> Camera = new List<string>();
    }
    #endregion

    #region 场景图片
    public class SceneImage
    {
        public int ID { get; set; }
        public string ImageURL { get; set; }
        public string ImageMD5 { get; set; }
    }
    #endregion

    #region 背景音乐
    public class AllMusic 
    {
        public float Volume { get; set; }
        public string MusicUrl { get; set; }
    }
    #endregion
}
