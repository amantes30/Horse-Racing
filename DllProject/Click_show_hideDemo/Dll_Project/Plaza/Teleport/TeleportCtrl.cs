using com.ootii.Messages;
using LitJson;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.Plaza.Teleport
{
    public class TeleportCtrl : DllGenerateBase
    {
        private Transform contentParent;
        private Button leftBtn;
        private Button rightBtn;
        private Text pageText;
        private GameObject uiPanel;
        private GameObject infoPrafeb;
        private GameObject uiCanvas;
        public override void Init()
        {
            contentParent = BaseMono.ExtralDatas[0].Target;
            leftBtn = BaseMono.ExtralDatas[1].Target.GetComponent<Button>();
            rightBtn = BaseMono.ExtralDatas[2].Target.GetComponent<Button>();
            pageText = BaseMono.ExtralDatas[3].Target.GetComponent<Text>();
            uiPanel = BaseMono.ExtralDatas[4].Target.gameObject;
            infoPrafeb = BaseMono.ExtralDatas[5].Target.gameObject;
            uiCanvas = BaseMono.ExtralDatas[6].Target.gameObject;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            leftBtn.onClick.AddListener(LeftClick);
            rightBtn.onClick.AddListener(RightClick);
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void Update()
        {
        }
        #endregion
        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("area_A"))
            {
                selectPage = 0;
                uiPanel.SetActive(true);
                if (mStaticThings.I.isVRApp)
                {
                    uiCanvas.SetActive(true);
                }
                GetChannelList("展A");
            }
            else if (name.Equals("area_B"))
            {
                selectPage = 0;
                uiPanel.SetActive(true);
                if (mStaticThings.I.isVRApp)
                {
                    uiCanvas.SetActive(true);
                }
                GetChannelList("展B");
            }
            else if (name.Equals("area_C"))
            {
                selectPage = 0;
                uiPanel.SetActive(true);
                if (mStaticThings.I.isVRApp)
                {
                    uiCanvas.SetActive(true);
                }
                GetChannelList("展C");
            }
            else if (name.Equals("area_D"))
            {
                selectPage = 0;
                uiPanel.SetActive(true);
                if (mStaticThings.I.isVRApp)
                {
                    uiCanvas.SetActive(true);
                }
                GetChannelList("会");
            }
            else if (name.Equals("area_all"))
            {
                selectPage = 0;
                uiPanel.SetActive(true);
                if (mStaticThings.I.isVRApp)
                {
                    uiCanvas.SetActive(true);
                }
                GetChannelList("会+展");
            }
            else 
            {
                
                uiPanel.SetActive(false);
            }
        }
        
        #region 频道列表
        private List<Channel> channels = new List<Channel>();
        /// <summary>
        /// 获取展示频道列表
        /// </summary>
        private void GetChannelList(string name)
        {
            switch (name)
            {
                case "展A":
                    channels.Clear();
                    if (mStaticData.Identity.VIP.Contains(mStaticThings.I.mAvatarID))
                    {
                        if (mStaticData.CannelInformatica.AChannels.Count!=0) 
                        {
                            channels.Add(mStaticData.CannelInformatica.AChannels[0]);
                        }
                    }
                    else 
                    {
                        channels.AddRange(mStaticData.CannelInformatica.AChannels);
                    }
                    CreatPrafeb(channels);
                    break;
                case "展B":
                    channels.Clear();
                    if (mStaticData.Identity.VIP.Contains(mStaticThings.I.mAvatarID))
                    {
                        if (mStaticData.CannelInformatica.BChannels.Count!=0) 
                        {
                            channels.Add(mStaticData.CannelInformatica.BChannels[0]);
                        }
                    }
                    else
                    {
                        channels.AddRange(mStaticData.CannelInformatica.BChannels);
                    }
                    CreatPrafeb(channels);
                    break;
                case "展C":
                    channels.Clear();
                    if (mStaticData.Identity.VIP.Contains(mStaticThings.I.mAvatarID))
                    {
                        if (mStaticData.CannelInformatica.CChannels.Count!=0) 
                        {
                            channels.Add(mStaticData.CannelInformatica.CChannels[0]);
                        }
                    }
                    else
                    {
                        channels.AddRange(mStaticData.CannelInformatica.CChannels);
                    }
                    CreatPrafeb(channels);
                    break;
                case "会":
                    channels.Clear();
                    if (mStaticData.Identity.VIP.Contains(mStaticThings.I.mAvatarID))
                    {
                        if (mStaticData.CannelInformatica.DChannels.Count!=0) 
                        {
                            channels.Add(mStaticData.CannelInformatica.DChannels[0]);
                        }
                    }
                    else
                    {
                        channels.AddRange(mStaticData.CannelInformatica.DChannels);
                    }
                    CreatPrafeb(channels);
                    break;
                case "会+展":
                    channels.Clear();
                    if (mStaticData.Identity.VIP.Contains(mStaticThings.I.mAvatarID))
                    {
                        if (mStaticData.CannelInformatica.AChannels.Count != 0)
                        {
                            channels.Add(mStaticData.CannelInformatica.AChannels[0]);
                        }
                        if (mStaticData.CannelInformatica.BChannels.Count != 0)
                        {
                            channels.Add(mStaticData.CannelInformatica.BChannels[0]);
                        }
                        if (mStaticData.CannelInformatica.CChannels.Count != 0)
                        {
                            channels.Add(mStaticData.CannelInformatica.CChannels[0]);
                        }
                        if (mStaticData.CannelInformatica.DChannels.Count != 0)
                        {
                            channels.Add(mStaticData.CannelInformatica.DChannels[0]);
                        }
                    }
                    else
                    {
                        channels.AddRange(mStaticData.CannelInformatica.AChannels);
                        channels.AddRange(mStaticData.CannelInformatica.BChannels);
                        channels.AddRange(mStaticData.CannelInformatica.CChannels);
                        channels.AddRange(mStaticData.CannelInformatica.DChannels);
                    }
                    CreatPrafeb(channels);
                    break;
                default:
                    break;
            }
        }

        int page;//总页数
        float allCount;//所有的频道数
        int singePage = 4;//单页多少频道
        int selectPage;//第几页

        private void CreatPrafeb(List<Channel> channels) 
        {
            for (int i = 1; i < contentParent.childCount; i++)
            {
                GameObject.Destroy(contentParent.GetChild(i).gameObject);
            }
            if (channels.Count != 0) 
            {
                allCount = channels.Count;
                page = Mathf.CeilToInt(allCount / singePage);
                pageText.text = selectPage+1 + "/" + page;
                if ((selectPage + 1) * singePage < allCount)
                {
                    for (int i = selectPage * singePage; i < (selectPage + 1) * singePage; i++)
                    {
                        var temp = JsonMapper.ToJson(channels[i]);
                        var tp = GameObject.Instantiate(infoPrafeb, contentParent);
                        tp.transform.localScale = Vector3.one;
                        tp.GetComponent<GeneralDllBehavior>().OtherData = temp;
                        tp.gameObject.SetActive(true);
                    }
                }
                else 
                {
                    for (int i = selectPage * singePage; i < allCount; i++)
                    {
                        var temp = JsonMapper.ToJson(channels[i]);
                        var tp = GameObject.Instantiate(infoPrafeb, contentParent);
                        tp.transform.localScale = Vector3.one;
                        tp.GetComponent<GeneralDllBehavior>().OtherData = temp;
                        tp.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void LeftClick() 
        {
            selectPage--;
            if (selectPage >= 0)
            {
                CreatPrafeb(channels);
            }
            else 
            {
                selectPage = 0;
            }
        }
        private void RightClick()
        {
            selectPage++;
            if (selectPage < page)
            {
                CreatPrafeb(channels);
            }
            else 
            {
                selectPage = page-1;
            }
        }
        #endregion
    }
}
