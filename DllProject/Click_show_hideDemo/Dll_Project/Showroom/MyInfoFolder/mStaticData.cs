using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dll_Project.Showroom
{
    public class mStaticData
    {
        public static string uuid;//数据采集时UserID

        public static AvatorData AvatorData = new AvatorData();
        public static bool IsOpenIconPanel;//控制表情倒计时开关
        public static bool IsOpenPointClick = true;//控制点击事件是否可用
        public static bool IsPointAssetOrCard = true; //控制资料屏和名片只能展示一个
        public static string FollowAvatorID;//跟随人员的ID
        public static List<string> FollowList = new List<string>();//被跟随人员列表
        public static List<string> MovePlayerList = new List<string>();//支持人移动人员列表

        public static Dictionary<string, string> SaveAssetDir = new Dictionary<string, string>();

        public static CompanyAsset CompanyAsset = new CompanyAsset();
        public static BoothAsset BoothAsset = new BoothAsset();
    }
    /// <summary>
    /// 我的信息
    /// </summary>
    public class AvatorData
    {
        public string company_name { get; set; }//企业名称
        public string name { get; set; }//名称
        public string position { get; set; }//职位
        public string contact_info { get; set; }//邮箱电话
        public string identity { get; set; }//身份列表中的id
        public string uid { get; set; }//用户id

    }

    public class TeleportInfo
    {
        public string ID;
        public string MeshName;
        public string RootRoomID;
        public string RootVoiceID;
        public string ImgUrl;
        public string Name;
        public string Password;
        public string HaveTeleport;
    }

    #region 公司信息
    public class CompanyAsset
    {
        public List<CompanyFiles> companyFilesList = new List<CompanyFiles>();
        public List<TeleportInfo> posTeleports = new List<TeleportInfo>();
        //public List<TeleportInfo> floorTeleports = new List<TeleportInfo>();
        public FloorTeleport floorTeleport = new FloorTeleport();
        public List<Card_Info> CardInfo = new List<Card_Info>();
        public List<Companyscreen> companyscreens = new List<Companyscreen>();
        public List<Identity> IdentityInfo = new List<Identity>();
        public List<Multicast> MulticastInfo = new List<Multicast>();
        public BigScreenLoge BigScreenLoge = new BigScreenLoge();
    }
    public class CompanyFiles
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string MediaPath { get; set; }
        public string MediaMD5 { get; set; }
        public string MediaType { get; set; }//0:PDF,1:mp4,2:jpg、png
    }
    public class FloorTeleport 
    {
        public string HaveTeleport;
        public string FloorNumber;
        public List<TeleportInfo> floorTeleports = new List<TeleportInfo>();
    }
    public class BigScreenLoge
    {
        public string MediaPath { get; set; }
        public string MediaMD5 { get; set; }
    }
    public class Identity
    {
        public string Name { get; set; }
        public string mAvatorID { get; set; }
        public string Sign { get; set; }
    }
    public class Multicast
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public class Companyscreen 
    {
        public string ID { get; set; }
        public string screenImageUrl { get; set; }
        public string screenMd5 { get; set; }
        public string name { get; set; }
    }
    #endregion

    #region 展位信息
    public class BoothAsset
    {
        public List<Card_Info> cardInfo = new List<Card_Info>();
        public List<BoothPicInfo> boothAssets = new List<BoothPicInfo>();
        public List<GuideToVisitors> guideToVisitors = new List<GuideToVisitors>();
    }

    //展位上的数据结构
    public class BoothPicInfo
    {
        public string exhibition_id { get; set; }
        public string LogeUrl { get; set; }
        public string LogeMD5 { get; set; }
        public string PictureUrl { get; set; }
        public string PictureMD5 { get; set; }
    }
    //导览的数据结构
    public class GuideToVisitors
    {
        public string ID { get; set; }
        public string GuideUrl { get; set; }
        public string GuideMD5 { get; set; }
    }
    #endregion
}
