using com.ootii.Messages;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;

namespace Dll_Project.Showroom
{
    public class XiaoTeleportCtrl : DllGenerateBase
    {
        private AudioSource clickAudioSource;//BaseMono
        private AudioClip clickAudioClip;
        private AudioSource errorAudioSource;//BaseMono
        private AudioClip errorAudioClip;

        private Transform TeleportTitlePanel;
        private Transform TeleportPasswordPanel;

        private ExtralData[] extralDatas;
        public override void Init()
        {
            clickAudioSource = BaseMono.ExtralDatas[0].Target.Find("Click").GetComponent<AudioSource>();
            clickAudioClip = clickAudioSource.clip;
            errorAudioSource = BaseMono.ExtralDatas[0].Target.Find("ErrorClick").GetComponent<AudioSource>();
            errorAudioClip = errorAudioSource.clip;

            TeleportTitlePanel = BaseMono.ExtralDatas[1].Target;
            //TeleportPasswordPanel = BaseMono.ExtralDatas[2].Target;

            extralDatas = BaseMono.ExtralDatas[3].Info;
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);

        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);

            BaseMono.StopAllCoroutines();
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Start()
        {
            TeleportTitlePanel.Find("CancelButton").GetComponent<Button>().onClick.AddListener(TeleportCanelClick);
            TeleportTitlePanel.Find("SureButton").GetComponent<Button>().onClick.AddListener(TeleportSureClick);

            //TeleportPasswordPanel.Find("CancelButton").GetComponent<Button>().onClick.AddListener(TeleportCanelClick);
            //TeleportPasswordPanel.Find("SureButton").GetComponent<Button>().onClick.AddListener(TeleportSureClick);

            BaseMono.StartCoroutine(ShowTeleportInfo(1.1f));//展示传送门数据

            if (mStaticThings.I.isVRApp) 
            {
                passwordTrigger();
            }
        }

        public override void Update()
        {
            //if (mStaticThings.I != null)
            //{
            //    if (mStaticThings.I.isVRApp)
            //    {
            //        FollowCamera(-15, 0.65f);
            //    }
            //}
        }
        public void FollowCamera(float angle, float distocamera)
        {
            if (mStaticThings.I == null) return;
            if (mStaticThings.I.Maincamera == null&&!mStaticThings.I.isVRApp) return;
            if (TeleportTitlePanel != null)
            {
                TeleportTitlePanel.transform.forward = mStaticThings.I.Maincamera.forward;
                Quaternion rotate = Quaternion.AngleAxis(angle, mStaticThings.I.Maincamera.right);

                TeleportTitlePanel.transform.position = mStaticThings.I.Maincamera.position + rotate * mStaticThings.I.Maincamera.forward.normalized * distocamera;

                TeleportTitlePanel.transform.position = new Vector3(TeleportTitlePanel.transform.position.x, TeleportTitlePanel.transform.position.y - 0.2f, TeleportTitlePanel.transform.position.z);
            }
            if (TeleportPasswordPanel != null)
            {
                TeleportPasswordPanel.transform.forward = mStaticThings.I.Maincamera.forward;
                Quaternion rotate = Quaternion.AngleAxis(angle, mStaticThings.I.Maincamera.right);

                TeleportPasswordPanel.transform.position = mStaticThings.I.Maincamera.position + rotate * mStaticThings.I.Maincamera.forward.normalized * distocamera;

                TeleportPasswordPanel.transform.position = new Vector3(TeleportPasswordPanel.transform.position.x, TeleportPasswordPanel.transform.position.y - 0.2f, TeleportPasswordPanel.transform.position.z);
            }
        }

        #region 展示传送门初始数据
        int count;
        public IEnumerator ShowTeleportInfo(float time)
        {
            yield return new WaitForSeconds(time);
            if (mStaticData.CompanyAsset.posTeleports.Count== 0)
            {
                BaseMono.StartCoroutine(ShowTeleportInfo(1.1f));
            }
            else 
            {
                GetTelelportAsset();
            }
            
        }
        public void GetTelelportAsset()
        {
            count = 0;
            BaseMono.StartCoroutine(GetTelelportAsset(mStaticData.CompanyAsset.posTeleports[count], extralDatas[count].Target, BackCall));
        }
        private IEnumerator GetTelelportAsset(TeleportInfo teleportInfo,Transform tf, Action action) 
        {
            if (teleportInfo.HaveTeleport == "0") 
            {
                tf.gameObject.SetActive(false);
                count++;
                if (mStaticData.CompanyAsset.posTeleports.Count > count)
                {
                    action();
                }
                yield return null;
            }
            if (!teleportInfo.ImgUrl.StartsWith("http"))
            {
                yield break;
            }
            var uwr = UnityWebRequestTexture.GetTexture(teleportInfo.ImgUrl);
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error) || uwr.isNetworkError || uwr.isHttpError)
            {
                uwr.Dispose();
                BaseMono.StartCoroutine(GetTelelportAsset(teleportInfo, tf, action));
            }
            else 
            {
                Texture2D mTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                tf.Find("传送门/UICanvas/Image").GetComponent<Image>().sprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), new Vector2(0.5f, 0.5f));
                tf.Find("传送门/UICanvas/Text").GetComponent<Text>().text = teleportInfo.Name;
                count++;
                if (mStaticData.CompanyAsset.posTeleports.Count > count)
                {
                    action();
                }
            }
            
        }
        private void BackCall() 
        {
            BaseMono.StartCoroutine(GetTelelportAsset(mStaticData.CompanyAsset.posTeleports[count], extralDatas[count].Target, BackCall));
        }
        #endregion
        #region 其他场景跳转
        private void TeleportCanelClick() 
        {
            clickAudioSource.PlayOneShot(clickAudioClip);
            TeleportTitlePanel.gameObject.SetActive(false);
            TeleportPasswordPanel.gameObject.SetActive(false);
            TeleportPasswordPanel.Find("TitleText").GetComponent<Text>().text = "";

        }
        private string Teleportname;
        private void TeleportSureClick() 
        {
            if (mStaticThings.I != null)
            {
                if (!string.IsNullOrEmpty(mStaticData.CompanyAsset.posTeleports[int.Parse(Teleportname.Replace("teleport_", ""))-1].Password))
                {
                    if (TeleportPasswordPanel.Find("InputField").GetComponent<InputField>().text == mStaticData.CompanyAsset.posTeleports[int.Parse(Teleportname.Replace("teleport_", ""))-1].Password)
                    {
                        clickAudioSource.PlayOneShot(clickAudioClip);
                        if (mStaticThings.I.isAdmin)
                        {
                            WsCChangeInfo wsinfo = new WsCChangeInfo()
                            {
                                a = "OutSceneTeleport",
                                b = Teleportname
                            };
                            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
                        }
                        else
                        {
                            TeleportTitlePanel.gameObject.SetActive(false);
                            TeleportPasswordPanel.gameObject.SetActive(false);

                            ChangeRoomByPosName(Teleportname);

                            Resources.UnloadUnusedAssets();
                        }
                    }
                    else 
                    {
                        errorAudioSource.PlayOneShot(errorAudioClip);
                        TeleportPasswordPanel.Find("TitleText").GetComponent<Text>().text = "密码错误";
                    }
                }
                else 
                {
                    clickAudioSource.PlayOneShot(clickAudioClip);
                    if (mStaticThings.I.isAdmin)
                    {
                        WsCChangeInfo wsinfo = new WsCChangeInfo()
                        {
                            a = "OutSceneTeleport",
                            b = Teleportname
                        };
                        MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
                    }
                    else
                    {
                        TeleportTitlePanel.gameObject.SetActive(false);
                        TeleportPasswordPanel.gameObject.SetActive(false);

                        ChangeRoomByPosName(Teleportname);

                        Resources.UnloadUnusedAssets();
                    }
                }

            }
        }

        /// <summary>
        /// 同步传送
        /// </summary>
        WsCChangeInfo info;
        bool isPlayAnim = true;
        private void RecieveCChangeObj(IMessage msg)
        {
            info = msg.Data as WsCChangeInfo;
            if (info.a.Equals("OutSceneTeleport"))
            {
                TeleportTitlePanel.gameObject.SetActive(false);
                TeleportPasswordPanel.gameObject.SetActive(false);

                if (Teleportname != info.b)
                    return;
                ChangeRoomByPosName(info.b);
                Resources.UnloadUnusedAssets();
            }
            else if (info.a.Equals("OutSceneTeleportAnim") && isPlayAnim)
            {
                if (mStaticThings.I.mAvatarID == info.c)
                {
                    var temp = extralDatas[int.Parse(info.b.Replace("teleport_", "")) - 1].Target.Find("传送门/ChuanSong/ZheZhao_Nei");
                    temp.DOLocalMoveY(0, 1);
                }
                else 
                {
                    isPlayAnim = false;
                    var temp = extralDatas[int.Parse(info.b.Replace("teleport_", "")) - 1].Target.Find("传送门/ChuanSong/ZheZhao_Wai");
                    temp.DOLocalMoveY(0, 1).OnComplete(() =>
                    {
                        temp.DOLocalMoveY(0, 3).OnComplete(() => 
                        { 
                            temp.DOLocalMoveY(-3.4325f, 1).OnComplete(() => { isPlayAnim = true; }); 
                        });
                    });
                }
            }
        }
        #endregion

        private void TelePortToMesh(IMessage msg)
        {
            var name = msg.Data.ToString();
            Teleportname = name;
            if (name.Equals("teleport_1"))
            {
                if (!string.IsNullOrEmpty(mStaticData.CompanyAsset.posTeleports[0].Password))
                {
                    TeleportTitlePanel.gameObject.SetActive(false);
                    TeleportPasswordPanel.gameObject.SetActive(true);
                    TeleportPasswordPanel.Find("InputField").GetComponent<InputField>().text = null;
                    TeleportPasswordPanel.Find("ground_wai").GetComponent<Image>().sprite = extralDatas[0].Target.Find("传送门/UICanvas/Image").GetComponent<Image>().sprite;
                }
                else
                {
                    TeleportTitlePanel.gameObject.SetActive(true);
                    TeleportPasswordPanel.gameObject.SetActive(false);
                    TeleportTitlePanel.Find("ground_wai/Text").GetComponent<Text>().text = "是否确认传送到"+ mStaticData.CompanyAsset.posTeleports[0].Name;
                    TeleportTitlePanel.Find("ground_wai").GetComponent<Image>().sprite = extralDatas[0].Target.Find("传送门/UICanvas/Image").GetComponent<Image>().sprite;
                }

            }
            else if (name.Equals("teleport_2"))
            {
                if (!string.IsNullOrEmpty(mStaticData.CompanyAsset.posTeleports[1].Password))
                {
                    TeleportTitlePanel.gameObject.SetActive(false);
                    TeleportPasswordPanel.gameObject.SetActive(true);
                    TeleportPasswordPanel.Find("InputField").GetComponent<InputField>().text = null;
                    TeleportPasswordPanel.Find("ground_wai").GetComponent<Image>().sprite = extralDatas[1].Target.Find("传送门/UICanvas/Image").GetComponent<Image>().sprite;
                }
                else
                {
                    TeleportTitlePanel.gameObject.SetActive(true);
                    TeleportPasswordPanel.gameObject.SetActive(false);
                    TeleportTitlePanel.Find("ground_wai/Text").GetComponent<Text>().text = "是否确认传送到" + mStaticData.CompanyAsset.posTeleports[1].Name;
                    TeleportTitlePanel.Find("ground_wai").GetComponent<Image>().sprite = extralDatas[1].Target.Find("传送门/UICanvas/Image").GetComponent<Image>().sprite;
                }

            }
            else if (name.Equals("teleport_3"))
            {
                if (!string.IsNullOrEmpty(mStaticData.CompanyAsset.posTeleports[2].Password))
                {
                    TeleportTitlePanel.gameObject.SetActive(false);
                    TeleportPasswordPanel.gameObject.SetActive(true);
                    TeleportPasswordPanel.Find("InputField").GetComponent<InputField>().text = null;
                    TeleportPasswordPanel.Find("ground_wai").GetComponent<Image>().sprite = extralDatas[2].Target.Find("传送门/UICanvas/Image").GetComponent<Image>().sprite;
                }
                else
                {
                    TeleportTitlePanel.gameObject.SetActive(true);
                    TeleportPasswordPanel.gameObject.SetActive(false);
                    TeleportTitlePanel.Find("ground_wai/Text").GetComponent<Text>().text = "是否确认传送到" + mStaticData.CompanyAsset.posTeleports[2].Name;
                    TeleportTitlePanel.Find("ground_wai").GetComponent<Image>().sprite = extralDatas[2].Target.Find("传送门/UICanvas/Image").GetComponent<Image>().sprite;
                }

            }
            else if (name.Equals("teleport_4"))
            {
                if (!string.IsNullOrEmpty(mStaticData.CompanyAsset.posTeleports[3].Password))
                {
                    TeleportTitlePanel.gameObject.SetActive(false);
                    TeleportPasswordPanel.gameObject.SetActive(true);
                    TeleportPasswordPanel.Find("InputField").GetComponent<InputField>().text = null;
                    TeleportPasswordPanel.Find("ground_wai").GetComponent<Image>().sprite = extralDatas[3].Target.Find("传送门/UICanvas/Image").GetComponent<Image>().sprite;
                }
                else
                {
                    TeleportTitlePanel.gameObject.SetActive(true);
                    TeleportPasswordPanel.gameObject.SetActive(false);
                    TeleportTitlePanel.Find("ground_wai/Text").GetComponent<Text>().text = "是否确认传送到" + mStaticData.CompanyAsset.posTeleports[3].Name;
                    TeleportTitlePanel.Find("ground_wai").GetComponent<Image>().sprite = extralDatas[3].Target.Find("传送门/UICanvas/Image").GetComponent<Image>().sprite;
                }
            }
            else if (name.Contains("ground_"))
            {
                //TeleportPasswordPanel.gameObject.SetActive(false);
                TeleportTitlePanel.gameObject.SetActive(false);
                //TeleportPasswordPanel.Find("TitleText").GetComponent<Text>().text = "";
                Teleportname = null;
            }
        }

        private void ChangeRoomByPosName(string meshName)
        {
            if (string.IsNullOrEmpty(meshName))
                return;
            SaveInfo.instance.SaveActionData(meshName, 7);
            WsCChangeInfo wsinfoanim = new WsCChangeInfo()
            {
                a = "OutSceneTeleportAnim",
                b = Teleportname,
                c = mStaticThings.I.mAvatarID
            };
            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfoanim, 0);

            for (int i = 0; i < mStaticData.CompanyAsset.posTeleports.Count; i++)
            {
                if (mStaticData.CompanyAsset.posTeleports[i].MeshName == meshName)
                {
                    BaseMono.StartCoroutine(ChangeRoom(mStaticData.CompanyAsset.posTeleports[i].RootRoomID, mStaticData.CompanyAsset.posTeleports[i].RootVoiceID, 0.8f));
                    return;
                }
            }
        }
        private IEnumerator ChangeRoom(string RootRoomID, string RootVoiceID,float time)
        {
            yield return new WaitForSeconds(time);
            if (string.IsNullOrEmpty(RootRoomID) || string.IsNullOrEmpty(RootVoiceID))
                yield return null;
            VRRootChanelRoom ch = new VRRootChanelRoom
            {
                roomid = RootRoomID,
                voiceid = RootVoiceID,
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.ConnectToNewChanel.ToString(), ch, 0);
        }
        #region inputField添加vr输入功能
        private void passwordTrigger()
        {
            EventTrigger eventTrigger = TeleportPasswordPanel.Find("InputField").GetComponent<EventTrigger>();
            eventTrigger.triggers = new List<EventTrigger.Entry>();
            EventTrigger.Entry enterpriseEntry = new EventTrigger.Entry
            {
                //事件类型
                eventID = EventTriggerType.PointerClick,
                callback = new EventTrigger.TriggerEvent()
                //创建回调函数
            };
            enterpriseEntry.callback.AddListener((data) =>
            {
                InputFieldClick(TeleportPasswordPanel.Find("InputField").GetComponent<InputField>());
            });
            eventTrigger.triggers.Add(enterpriseEntry);
        }
        private void InputFieldClick(InputField fd)
        {
            MessageDispatcher.SendMessage(fd, VrDispMessageType.InputFildClicked.ToString(), fd.text, 0);
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
    }
}
