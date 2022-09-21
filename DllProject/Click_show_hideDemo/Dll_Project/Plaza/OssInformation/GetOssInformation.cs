using Dll_Project.Showroom;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Dll_Project.Plaza.OssInformation
{
    public class GetOssInformation : DllGenerateBase
    {
        private string urlpath;

        int count;
        public override void Init()
        {
            if (!string.IsNullOrEmpty(mStaticThings.I.nowRoomActionAPI))
            {
                urlpath = mStaticThings.I.nowRoomActionAPI;
            }
            else
            {
                urlpath = BaseMono.ExtralDatas[0].OtherData;
            }
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
        }
        public override void OnEnable()
        {
            BaseMono.StartCoroutine(LoadIniConfigFile(urlpath));
        }

        public override void OnDisable()
        {
        }

        public override void Update()
        {
        }
        #endregion

        #region json
        IEnumerator LoadIniConfigFile(string mPath) 
        {
            if (!mPath.StartsWith("http"))
            {
                yield break;
            }
            var uwr = UnityWebRequest.Get(mPath);
            count++;
            if (count > 60) yield break;
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error) || string.IsNullOrEmpty(uwr.downloadHandler.text) || uwr.isNetworkError || uwr.isHttpError)
            {
                uwr.Dispose();
                BaseMono.StartCoroutine(LoadIniConfigFile(mPath));
            }
            else
            {
                mStaticData.CannelInformatica = ReadCannelJson(uwr.downloadHandler.text);
                mStaticData.AllURL = ReadURLJson(uwr.downloadHandler.text);
                mStaticData.Identity = ReadIdentityJson(uwr.downloadHandler.text);
                mStaticData.SceneImage = ReadSceneImageJson(uwr.downloadHandler.text);
                mStaticData.AllMusics = ReadAllMusicJson(uwr.downloadHandler.text);
                GetOtherInfo(uwr.downloadHandler.text);
                DataCollectJson(uwr.downloadHandler.text);
                uwr.Dispose();
            }
        }

        /// <summary>
        /// 获取频道数据
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private ChannelInformatica ReadCannelJson(string str) 
        {
            ChannelInformatica cannelInformatica = new ChannelInformatica();
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("ChannelList"))
                return null;
            JsonData jsonData = jd["ChannelList"];
            if (!string.IsNullOrEmpty(jsonData.ToJson())) 
            {
                if (jsonData.ToJson().Contains("OffLine")) 
                {
                    JsonData jsonOffLine = jsonData["OffLine"];
                    if (!jsonOffLine.ToJson().Equals("[]"))
                    {
                        cannelInformatica.OffLineCannels = ReadOddLineCannel(jsonOffLine);
                    }
                }

                if (jsonData.ToJson().Contains("AChannel")) 
                {
                    JsonData jsonA = jsonData["AChannel"];
                    if (!jsonA.ToJson().Equals( "[]"))
                    {
                        cannelInformatica.AChannels = ReadOddLineCannel(jsonA);
                    }
                }

                if (jsonData.ToJson().Contains("BChannel")) 
                {
                    JsonData jsonB = jsonData["BChannel"];
                    if (!jsonB.ToJson().Equals("[]"))
                    {
                        cannelInformatica.BChannels = ReadOddLineCannel(jsonB);
                    }
                }

                if (jsonData.ToJson().Contains("CChannel")) 
                {
                    JsonData jsonC = jsonData["CChannel"];
                    if (!jsonC.ToJson().Equals("[]"))
                    {
                        cannelInformatica.CChannels = ReadOddLineCannel(jsonC);
                    }
                }

                if (jsonData.ToJson().Contains("DChannel")) 
                {
                    JsonData jsonD = jsonData["DChannel"];
                    if (!jsonD.ToJson().Equals("[]"))
                    {
                        cannelInformatica.DChannels = ReadOddLineCannel(jsonD);
                    }
                }

            }
            return cannelInformatica;
        }
        /// <summary>
        /// 获取所有URL
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private AllURL ReadURLJson(string str) 
        {
            AllURL allURL = new AllURL();
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("AllURL"))
            {
                return null;
            }
            JsonData jsonData = jd["AllURL"];
            if (!string.IsNullOrEmpty(jsonData.ToJson()))
            {
                if (jsonData.ToJson().Contains("MyInfoURL")) 
                {
                    allURL.MyInfoURL = jsonData["MyInfoURL"].ToString();
                }
                if (jsonData.ToJson().Contains("ExchangeCard")) 
                {
                    allURL.ExchangeCard = jsonData["ExchangeCard"].ToString();
                }
                if (jsonData.ToJson().Contains("TakePhotoURL"))
                {
                    allURL.TakePhotoURL = jsonData["TakePhotoURL"].ToString();
                }
            }
            return allURL;
        }
        /// <summary>
        /// 获取场景图片
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private List<SceneImage> ReadSceneImageJson(string str) 
        {
            List<SceneImage> sceneImages = new List<SceneImage>();
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("SceneImage")) 
            {
                return null;
            }
            JsonData jsonData = jd["SceneImage"];
            for (int i = 0; i < jsonData.Count; i++)
            {
                SceneImage sceneImage = new SceneImage();
                sceneImage.ID = int.Parse(jsonData[i]["ID"].ToString());
                sceneImage.ImageURL = jsonData[i]["ImageURL"].ToString();
                sceneImage.ImageMD5 = jsonData[i]["ImageMD5"].ToString();

                sceneImages.Add(sceneImage);
            }
            return sceneImages;
        }
        /// <summary>
        /// 获取VIP列表
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private Identity ReadIdentityJson(string str)
        {
            Identity identity = new Identity();
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("Identity")) 
            {
                return null;
            }
            JsonData jsonData = jd["Identity"];
            if (!string.IsNullOrEmpty(jsonData.ToJson()))
            {
                identity.IsConfig = bool.Parse(jsonData["IsConfig"].ToString());
                identity.VIPCount = int.Parse(jsonData["VIPCount"].ToString());
                string[] temp = JsonMapper.ToObject<string[]>(jsonData["VIP"].ToJson());
                for (int i = 0; i < temp.Length; i++)
                {
                    identity.VIP.Add(temp[i]);
                }

                string[] temp1 = JsonMapper.ToObject<string[]>(jsonData["Camera"].ToJson());
                for (int i = 0; i < temp1.Length; i++)
                {
                    identity.Camera.Add(temp1[i]);
                }
            }
            return identity;
        }
        /// <summary>
        /// 频道列表赋值
        /// </summary>
        /// <param name="jd"></param>
        /// <returns></returns>
        private List<Channel> ReadOddLineCannel(JsonData jd) 
        {
            List<Channel> channels = new List<Channel>();
            if (jd.Count == 0)
                return null;
            for (int i = 0; i < jd.Count; i++)
            {
                Channel channel = new Channel();
                channel.ID = int.Parse(jd[i]["ID"].ToString());
                channel.RootRoomID = jd[i]["RootRoomID"].ToString();
                channel.RootVoiceID = jd[i]["RootVoiceID"].ToString();
                channel.Name = jd[i]["Name"].ToString();
                channel.RoomURL = jd[i]["RoomURL"].ToString();
                channel.AreaMaxCount = int.Parse(jd[i]["AreaMaxCount"].ToString());

                channels.Add(channel);
            }
            return channels;
        }
        /// <summary>
        /// 音乐列表获取
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private List<AllMusic> ReadAllMusicJson(string str) 
        {
            List<AllMusic> allMusics = new List<AllMusic>();
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("Music"))
            {
                return null;
            }
            JsonData jsonData = jd["Music"];
            for (int i = 0; i < jsonData.Count; i++)
            {
                AllMusic allMusic = new AllMusic();
                allMusic.Volume = float.Parse(jsonData[i]["Volume"].ToString());
                allMusic.MusicUrl = jsonData[i]["MusicURL"].ToString();

                allMusics.Add(allMusic);
            }
            return allMusics;
        }
        /// <summary>
        /// 其他配置
        /// </summary>
        /// <param name="str"></param>
        private void GetOtherInfo(string str) 
        {
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("Other"))
                return;
            JsonData jsonData = jd["Other"];
            if (jsonData.ToJson().Contains("PhotoFileName"))
            {
                mStaticData.PhotoFileName = jsonData["PhotoFileName"].ToString();
            }
        }
        /// <summary>
        /// 数据采集开关
        /// </summary>
        /// <param name="str"></param>
        private void DataCollectJson(string str)
        {
            JsonData jd = JsonMapper.ToObject(str);
            JsonData jsonData = jd["DataCollection"];
            if (!string.IsNullOrEmpty(jsonData.ToJson()))
            {
                InfoCollectController.Instance.isOpen = bool.Parse(jsonData["DataCollection"].ToString());
                InfoCollectController.Instance.isSaveTimeZoom = bool.Parse(jsonData["SaveTimeZoom"].ToString());
                InfoCollectController.Instance.isSaveViewData = bool.Parse(jsonData["SaveViewData"].ToString());
            }
        }
        #endregion
    }
}
