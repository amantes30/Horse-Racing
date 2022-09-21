using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dll_Project.Showroom
{
    public class SaveInfo : DllGenerateBase
    {
        public static SaveInfo instance;
        Transform mainCamera;
        Transform mainVRROOT;
        public override void Init()
        {
            instance = this;
        }
        public override void Awake()
        {
            
        }
        public string[] temp;
        public override void Start()
        {
            if (mStaticThings.I!=null) 
            {
                mainCamera = mStaticThings.I.Maincamera;
                mainVRROOT = mStaticThings.I.MainVRROOT;
            }
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VRVoiceModelEventType.VRVoiceSetMicMulttempEvent.ToString(), MicEvent);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VRVoiceModelEventType.VRVoiceSetMicMulttempEvent.ToString(), MicEvent);
        }
        public override void OnDestroy()
        {
            CollectPlatform(12);
            
            //OnDisable();
        }
        bool isOpen = true;
        float frameNum;
        float time;
        Vector3 lastpos;
        Quaternion lastrotion;
        public override void Update()
        {
            if (InfoCollectController.Instance.isOpen) 
            {
                if (isOpen && !string.IsNullOrEmpty(mStaticData.uuid))
                {
                    isOpen = false;
                    CollectPlatform(11);
                }
                if (InfoCollectController.Instance.isSaveTimeZoom) 
                {
                    frameNum += Time.deltaTime;
                    if (frameNum >= 1f)
                    {
                        SaveTimeZoom();
                        frameNum = 0;
                    }
                }
                if (InfoCollectController.Instance.isSaveViewData) 
                {
                    time += Time.deltaTime;
                    if (time > 1.5f)
                    {
                        if (mainVRROOT.position != lastpos || mainCamera.rotation != lastrotion)
                        {
                            SaveViewData();
                        }
                        else
                        {

                        }
                        lastpos = mainVRROOT.position;
                        lastrotion = mainCamera.rotation;
                        time = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 实时保存时空数据
        /// </summary>
        long timeStamp;
        private void SaveTimeZoom()
        {
            if (string.IsNullOrEmpty(mStaticData.uuid))
                return;
            timeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
            InfoCollectController.Instance.saveTimeZoomInfo(mStaticData.uuid, mainVRROOT.position.ToString(), mainCamera.rotation.ToString(), timeStamp,mStaticThings.I.nowRoomID);
        }
        /// <summary>
        /// 实时保存视野物体
        /// </summary>
        Collider[] cols;
        string namelist = null;
        long ViewTimeStamp;
        private void SaveViewData() 
        {
            if (string.IsNullOrEmpty(mStaticData.uuid))
                return;
            cols = Physics.OverlapSphere(mainCamera.position, 7);
            if (cols.Length > 0)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    if (isInView(cols[i].transform.position)) 
                    {
                        namelist += cols[i].name + ",";
                    }
                }
                ViewTimeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
                InfoCollectController.Instance.saveViewDataInfo(mStaticData.uuid, namelist, "", ViewTimeStamp, mStaticThings.I.nowRoomID);
            }
        }
        /// <summary>
        /// 判断位置是否在相机视野内
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        private bool isInView(Vector3 worldPos)
        {
            Vector2 viewPos = mainCamera.GetComponent<Camera>().WorldToViewportPoint(worldPos);
            Vector3 dir = (worldPos - mainCamera.position).normalized;
            float dot = Vector3.Dot(mainCamera.forward, dir);//判断物体是否在相机前面

            if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1) return true;
            else return false;
        }

        /// <summary>
        /// 保存交互动作信息
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="actionId"></param>
        private float startTime;
        private float persistTime = 1f;
        private string nm;//模型名字
        private int aid;//动作id
        public void SaveActionData(string modelName, int actionId)
        {
            if (InfoCollectController.Instance.isOpen)
            {
                if (string.IsNullOrEmpty(mStaticData.uuid))
                    return;
                if (Time.time - startTime > persistTime)
                {
                    if (string.IsNullOrEmpty(modelName))
                        return;
                    long actionTimeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
                    InfoCollectController.Instance.saveActionDataInfo(mStaticData.uuid, modelName, actionId.ToString(), actionTimeStamp, mStaticThings.I.nowRoomID);
                    startTime = Time.time;
                    nm = modelName; aid = actionId;
                }
                else 
                {
                    if (modelName != nm || actionId != aid) 
                    {
                        long actionTimeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
                        InfoCollectController.Instance.saveActionDataInfo(mStaticData.uuid, modelName, actionId.ToString(), actionTimeStamp, mStaticThings.I.nowRoomID);
                        startTime = Time.time;
                        nm = modelName;aid = actionId;
                    }
                }
            }
        }

        /// <summary>
        /// 麦克风事件监听
        /// </summary>
        /// <param name="msg">传过来的值是改变前的状态</param>
        bool isMic;
        private void MicEvent(IMessage msg)
        {
            if (InfoCollectController.Instance.isOpen) 
            {
                bool mul = (bool)msg.Data;

                if (mul && isMic)
                {
                    isMic = false;
                    //关闭麦克风
                    SaveActionData("Mic", 2);
                }
                else if (mul == false && isMic == false)
                {
                    isMic = true;
                    //打开麦克风
                    SaveActionData("Mic", 1);
                }
            }
        }
        /// <summary>
        /// 平台数据采集
        /// </summary>
        public void CollectPlatform(int actionID) 
        {
            if (InfoCollectController.Instance.isOpen)
            {
                if (mStaticThings.I.isVRApp)
                {
                    SaveActionData("VRPlatform", actionID);
                }
                else
                {
                    switch (mStaticThings.I.now_ScenePrefix)
                    {
                        case "a":
                            SaveActionData("AndroidPlatform", actionID);
                            break;
                        case "m":
                            SaveActionData("MacPlatform", actionID);
                            break;
                        case "w":
                            SaveActionData("PCPlatform", actionID);
                            break;
                        case "i":
                            SaveActionData("iosPlatform", actionID);
                            break;

                    }
                }
            }
        }

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
