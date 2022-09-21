using com.ootii.Messages;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

namespace Dll_Project.Showroom.BoothInformation
{
    public class GetBoothInfo : DllGenerateBase
    {
        string urlpath;

        private AudioSource bgAudioSource;//BaseMono
        bool bgStarted;
        int count;

        private Transform MulticastParent;
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

            bgAudioSource = BaseMono.ExtralDatas[1].Target.Find("BG").GetComponent<AudioSource>();

            MulticastParent = BaseMono.ExtralDatas[2].Target;
        }
        #region 初始
        public override void Awake()
        {
            BaseMono.StartCoroutine(LoadIniConfigFile(urlpath, 0));
        }

        public override void Start()
        {
            
        }
        public override void OnEnable()
        {
            
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Update()
        {
        }
        #endregion

        public void MDebug(string msg, int level = 0)
        {
            if (level == 0)
            {
                if (mStaticThings.I == null)
                {
                    Debug.Log(msg);
                    return;
                }
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = msg,
                    b = InfoColor.black.ToString(),
                    c = "3",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
            }
        }

        #region json
        IEnumerator LoadIniConfigFile(string mPath, float delayTime = 0)
        {
            //yield return new WaitForSeconds(delayTime);
            if (!mPath.StartsWith("http"))
            {
                yield break;
            }
            var uwr = UnityWebRequest.Get(mPath);
            count++;
            if (count>60) yield break;
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error)|| string.IsNullOrEmpty(uwr.downloadHandler.text) || uwr.isNetworkError || uwr.isHttpError)
            {
                uwr.Dispose();
                BaseMono.StartCoroutine(LoadIniConfigFile(mPath, 2f));
            }
            else
            {
                mStaticData.CompanyAsset = ReadCompanyAssetJson(uwr.downloadHandler.text);
                mStaticData.BoothAsset = ReadBoothAssetJson(uwr.downloadHandler.text);
                DataCollectJson(uwr.downloadHandler.text);
                BaseMono.StartCoroutine(ShowBoothPicture.instance.LoadData(0.2f));//展位资源加载

                uwr.Dispose();
            }
        }
        private CompanyAsset ReadCompanyAssetJson(string str)
        {
            CompanyAsset companyAsset = new CompanyAsset();
            JsonData jd = JsonMapper.ToObject(str);
            JsonData jsonData = jd["CompanyAsset"];
            if (!string.IsNullOrEmpty(jsonData.ToJson()))
            {
                JsonData jsonScreenList = jsonData["ScreenAsset"];
                if (!string.IsNullOrEmpty(jsonScreenList.ToString()))
                {
                    companyAsset.companyFilesList = ReadCompanyFiles(jsonScreenList);
                }

                JsonData jsonPosTeleportList = jsonData["PosTeleport"];
                if (!string.IsNullOrEmpty(jsonPosTeleportList.ToString()))
                {
                    companyAsset.posTeleports = ReadPosTeleport(jsonPosTeleportList);
                }

                JsonData jsonFloorTeleport = jsonData["FloorTeleport"];
                if (!string.IsNullOrEmpty(jsonFloorTeleport.ToString()))
                {
                    companyAsset.floorTeleport.HaveTeleport = jsonFloorTeleport["HaveTeleport"].ToString();
                    companyAsset.floorTeleport.FloorNumber = jsonFloorTeleport["FloorNumber"].ToString();
                    companyAsset.floorTeleport.floorTeleports = ReadFloorTeleport(jsonFloorTeleport["FloorAsset"]);
                }

                JsonData jsonCardInfo = jsonData["CardInfo"];
                if (!string.IsNullOrEmpty(jsonCardInfo.ToString()))
                {
                    companyAsset.CardInfo = ReadCardInfo(jsonCardInfo);
                }

                JsonData jsonCompanyScreen = jsonData["CompanyScreen"];
                if (!string.IsNullOrEmpty(jsonCompanyScreen.ToString())) 
                {
                    companyAsset.companyscreens = ReadCompanyscreen(jsonCompanyScreen);
                }

                JsonData jsonIdentityInfo = jsonData["Identity"];
                if (!string.IsNullOrEmpty(jsonIdentityInfo.ToString()))
                {
                    companyAsset.IdentityInfo = ReadIdentityInfo(jsonIdentityInfo);
                }

                JsonData jsonBigScreen = jsonData["BigScreenLogo"];
                if (!string.IsNullOrEmpty(jsonBigScreen.ToString()))
                {
                    string mediaPath = jsonBigScreen["MediaPath"].ToString();
                    string mediaMD5 = jsonBigScreen["MediaMD5"].ToString();

                    companyAsset.BigScreenLoge.MediaPath = mediaPath;
                    companyAsset.BigScreenLoge.MediaMD5 = mediaMD5;

                    if (!string.IsNullOrEmpty(mediaPath) && !string.IsNullOrEmpty(mediaMD5))
                    {
                        ShowImage(companyAsset.BigScreenLoge.MediaPath, companyAsset.BigScreenLoge.MediaMD5);
                    }
                }

                JsonData jsonMulticast = jsonData["Multicast"];
                if (!string.IsNullOrEmpty(jsonMulticast.ToString()))
                {
                    companyAsset.MulticastInfo = ReadMulticastInfo(jsonMulticast);

                    for (int i = 0; i < MulticastParent.childCount; i++)
                    {
                        MulticastParent.GetChild(i).Find("Image/Text").GetComponent<Text>().text = companyAsset.MulticastInfo[i].Name;
                    }
                }
            }
            return companyAsset;
        }

        private BoothAsset ReadBoothAssetJson(string str) 
        {
            BoothAsset boothAsset = new BoothAsset();
            JsonData jd = JsonMapper.ToObject(str);
            JsonData jsonData = jd["BoothAsset"];
            if (!string.IsNullOrEmpty(jsonData.ToJson()))
            {
                JsonData jsonCarInfoList = jsonData["CardInfo"];
                if (!string.IsNullOrEmpty(jsonCarInfoList.ToString()))
                {
                    boothAsset.cardInfo = ReadCardInfo(jsonCarInfoList);
                }

                JsonData jsonPictureInfoList = jsonData["PictureInfo"];
                if (!string.IsNullOrEmpty(jsonPictureInfoList.ToString()))
                {
                    boothAsset.boothAssets = ReadBoothPicInfo(jsonPictureInfoList);
                }

                JsonData jsonGuideToVisitorsList = jsonData["GuideToVisitors"];
                if (!string.IsNullOrEmpty(jsonGuideToVisitorsList.ToString()))
                {
                    boothAsset.guideToVisitors = ReadGuideToVisitors(jsonGuideToVisitorsList);
                }
            }
            return boothAsset;
        }
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
        private List<CompanyFiles> ReadCompanyFiles(JsonData jd)
        {
            List<CompanyFiles> companyFilesList = new List<CompanyFiles>();
            for (int i = 0; i < jd.Count; i++)
            {
                string id = jd[i]["ID"].ToString();
                string name = jd[i]["Name"].ToString();
                string mediaPath = jd[i]["MediaPath"].ToString();
                string mediaMD5 = jd[i]["MediaMD5"].ToString();
                string mediaType = jd[i]["MediaType"].ToString();

                CompanyFiles companyFiles = new CompanyFiles();
                companyFiles.ID = id;
                companyFiles.Name = name;
                companyFiles.MediaPath = mediaPath;
                companyFiles.MediaMD5 = mediaMD5;
                companyFiles.MediaType = mediaType;

                companyFilesList.Add(companyFiles);
            }
            return companyFilesList;
        }
        private List<TeleportInfo> ReadPosTeleport(JsonData jd)
        {
            List<TeleportInfo> posTeleports = new List<TeleportInfo>();
            for (int i = 0; i < jd.Count; i++)
            {
                string meshName = jd[i]["MeshName"].ToString();
                string rootRoomID = jd[i]["RootRoomID"].ToString();
                string rootVoiceID = jd[i]["RootVoiceID"].ToString();
                string ImgUrl = jd[i]["ImgUrl"].ToString();
                string Name = jd[i]["Name"].ToString();
                string Password = jd[i]["Password"].ToString();
                string HaveTeleport = jd[i]["HaveTeleport"].ToString();


                TeleportInfo teleport = new TeleportInfo();
                teleport.MeshName = meshName;
                teleport.RootRoomID = rootRoomID;
                teleport.RootVoiceID = rootVoiceID;
                teleport.ImgUrl = ImgUrl;
                teleport.Name = Name;
                teleport.Password = Password;
                teleport.HaveTeleport = HaveTeleport;
                posTeleports.Add(teleport);
            }
            return posTeleports;
        }
        private List<TeleportInfo> ReadFloorTeleport(JsonData jd)
        {
            List<TeleportInfo> floorTeleports = new List<TeleportInfo>();
            for (int i = 0; i < jd.Count; i++)
            {
                string id = jd[i]["ID"].ToString();
                string meshName = jd[i]["MeshName"].ToString();
                string rootRoomID = jd[i]["RootRoomID"].ToString();
                string rootVoiceID = jd[i]["RootVoiceID"].ToString();
                string imgUrl = jd[i]["ImgUrl"].ToString();
                string name = jd[i]["Name"].ToString();

                TeleportInfo floorTeleport = new TeleportInfo();
                floorTeleport.ID = id;
                floorTeleport.MeshName = meshName;
                floorTeleport.RootRoomID = rootRoomID;
                floorTeleport.RootVoiceID = rootVoiceID;
                floorTeleport.ImgUrl = imgUrl;
                floorTeleport.Name = name;
                floorTeleports.Add(floorTeleport);
            }
            return floorTeleports;
        }

        private void ShowImage(string path, string md5)
        {
            if (string.IsNullOrEmpty(path))
                return;
            if (mStaticThings.I.nowAvatarFrameList != null && mStaticThings.I.nowAvatarFrameList.chdata != null)
            {
                if (!mStaticThings.I.nowAvatarFrameList.chdata.ContainsKey("nowroomPDFshowing") || mStaticThings.I.nowAvatarFrameList.chdata["nowroomPDFshowing"] == "")
                {
                    WsMediaFile newfile = new WsMediaFile()
                    {
                        url = path,
                        fileMd5 = md5,
                        name = path.Substring(path.LastIndexOf('/') + 1),
                        mtime = md5
                    };

                    MessageDispatcher.SendMessage(this, VrDispMessageType.KODGetOneImage.ToString(), newfile, 0);
                }
            }
            else 
            {
                WsMediaFile newfile = new WsMediaFile()
                {
                    url = path,
                    fileMd5 = md5,
                    name = path.Substring(path.LastIndexOf('/') + 1),
                    mtime = md5
                };

                MessageDispatcher.SendMessage(this, VrDispMessageType.KODGetOneImage.ToString(), newfile, 0);
            }
        }


        private List<Card_Info> ReadCardInfo(JsonData jd)
        {
            List<Card_Info> cardInfo = new List<Card_Info>();
            for (int i = 0; i < jd.Count; i++)
            {
                string exhibition_id = jd[i]["exhibition_id"].ToString();
                string company_name = jd[i]["company_name"].ToString();
                string name = jd[i]["name"].ToString();
                string position = jd[i]["position"].ToString();
                string to_info = jd[i]["to_info"].ToString();
                string weixin = jd[i]["weixin"].ToString();
                string email = jd[i]["email"].ToString();
                string namepinyin = jd[i]["namepinyin"].ToString();

                Card_Info card_Info = new Card_Info();
                card_Info.exhibition_id = exhibition_id;
                card_Info.company_name = company_name;
                card_Info.name = name;
                card_Info.position = position;
                card_Info.to_info = to_info;
                card_Info.weixin = weixin;
                card_Info.email = email;
                card_Info.namepinyin = namepinyin;
                cardInfo.Add(card_Info);
            }
            return cardInfo;
        }
        private List<Companyscreen> ReadCompanyscreen(JsonData jd)
        {
            List<Companyscreen> companyscreen = new List<Companyscreen>();
            for (int i = 0; i < jd.Count; i++)
            {
                string id = jd[i]["ID"].ToString();
                string screenImageUrl = jd[i]["screenImageUrl"].ToString();
                string screenMd5 = jd[i]["screenMd5"].ToString();
                string name = jd[i]["name"].ToString();

                Companyscreen cs = new Companyscreen();
                cs.ID = id;
                cs.screenImageUrl = screenImageUrl;
                cs.screenMd5 = screenMd5;
                cs.name = name;
                companyscreen.Add(cs);
            }
            return companyscreen;
        }
        private List<Identity> ReadIdentityInfo(JsonData jd)
        {
            List<Identity> identityInfo = new List<Identity>();
            for (int i = 0; i < jd.Count; i++)
            {
                string name = jd[i]["Name"].ToString();
                string mAvatorID = jd[i]["mAvatorID"].ToString();
                string sign = jd[i]["Sign"].ToString();

                Identity identity = new Identity();
                identity.Name = name;
                identity.mAvatorID = mAvatorID;
                identity.Sign = sign;
                identityInfo.Add(identity);
            }
            return identityInfo;
        }
        private List<Multicast> ReadMulticastInfo(JsonData jd)
        {
            List<Multicast> multicastInfo = new List<Multicast>();
            for (int i = 0; i < jd.Count; i++)
            {
                string name = jd[i]["Name"].ToString();
                string id = jd[i]["ID"].ToString();

                Multicast multicast = new Multicast();
                multicast.ID = id;
                multicast.Name = name;
                multicastInfo.Add(multicast);
            }
            return multicastInfo;
        }
        private List<BoothPicInfo> ReadBoothPicInfo(JsonData jd)
        {
            List<BoothPicInfo> boothPicInfo = new List<BoothPicInfo>();
            for (int i = 0; i < jd.Count; i++)
            {
                string exhibition_id = jd[i]["exhibition_id"].ToString();
                string logeUrl = jd[i]["LogeUrl"].ToString();
                string logeMD5 = jd[i]["LogeMD5"].ToString();
                string pictureUrl = jd[i]["PictureUrl"].ToString();
                string pictureMD5 = jd[i]["PictureMD5"].ToString();

                BoothPicInfo booth_PicInfo = new BoothPicInfo();
                booth_PicInfo.exhibition_id = exhibition_id;
                booth_PicInfo.LogeUrl = logeUrl;
                booth_PicInfo.LogeMD5 = logeMD5;
                booth_PicInfo.PictureUrl = pictureUrl;
                booth_PicInfo.PictureMD5 = pictureMD5;
                boothPicInfo.Add(booth_PicInfo);
            }
            return boothPicInfo;
        }
        private List<GuideToVisitors> ReadGuideToVisitors(JsonData jd)
        {
            List<GuideToVisitors> guideToVisitors = new List<GuideToVisitors>();
            for (int i = 0; i < jd.Count; i++)
            {
                string id = jd[i]["ID"].ToString();
                string guideUrl = jd[i]["GuideUrl"].ToString();
                string guideMD5 = jd[i]["GuideMD5"].ToString();

                GuideToVisitors guideToVisitor = new GuideToVisitors();
                guideToVisitor.ID = id;
                guideToVisitor.GuideUrl = guideUrl;
                guideToVisitor.GuideMD5 = guideMD5;
                guideToVisitors.Add(guideToVisitor);
            }
            return guideToVisitors;
        }
        #endregion

        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("ground_jiaban"))
            {
                if (bgStarted)
                {
                    bgAudioSource.UnPause();//开始播放背景音乐
                }
                else
                {
                    bgAudioSource.Play();
                    bgStarted = true;
                }
            }
            else 
            {
                bgAudioSource.Pause();
            }
        }

    }
}
