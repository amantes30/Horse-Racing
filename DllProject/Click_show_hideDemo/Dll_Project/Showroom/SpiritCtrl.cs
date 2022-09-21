using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using com.ootii.Messages;

namespace Dll_Project.Showroom
{
    public class SpiritCtrl : DllGenerateBase
    {
        private Transform cameraTemp;
        private GameObject spiritObj;
        private Transform otherTemp;
        private GameObject uiCanvas;
        private Button vrCloseButton;

        float time = 2;
        
        public override void Init()
        {
            cameraTemp = BaseMono.ExtralDatas[0].Target;
            spiritObj = BaseMono.ExtralDatas[1].Target.gameObject;
            otherTemp = BaseMono.ExtralDatas[2].Target;
            vrCloseButton = BaseMono.ExtralDatas[3].Target.GetComponent<Button>();
            uiCanvas = BaseMono.ExtralDatas[4].Target.gameObject;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            if (!mStaticThings.I.isVRApp)
            {
                spiritObj.SetActive(false);
            }
            else 
            {
                vrCloseButton.gameObject.SetActive(true);
                uiCanvas.SetActive(false);
                UIControl.Instance.IsVR(spiritObj.transform.Find("GameObject"));
                vrCloseButton.onClick.AddListener(()=> 
                {
                    uiCanvas.SetActive(false);
                });
                
            }
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.SelfPlaceTo.ToString(), SelfPlaceTo);
            MessageDispatcher.AddListener(VrDispMessageType.VRTelePortToMesh.ToString(), VRTelePortToMesh);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.SelfPlaceTo.ToString(), SelfPlaceTo);
            MessageDispatcher.RemoveListener(VrDispMessageType.VRTelePortToMesh.ToString(), VRTelePortToMesh);
        }

        public override void Update()
        {
            if (mStaticThings.I.isVRApp)
            {
                time += Time.deltaTime;
                if (time > 0.55f)
                {
                    //SpiritPos();
                    
                    if (uiCanvas.activeSelf == false)
                    {
                        SetSpiritPos();
                    }
                    time = 0;
                }
                spiritObj.transform.LookAt(mStaticThings.I.Maincamera);
                spiritObj.transform.GetChild(0).LookAt(mStaticThings.I.Maincamera);

            }
        }
        #endregion
        private void SelfPlaceTo(IMessage rMessage)
        {
            if (mStaticThings.I.isVRApp) 
            {
                SpiritPos();
            }
        }
        private void VRTelePortToMesh(IMessage rMessage)
        {
            if (mStaticThings.I.isVRApp)
            {
                SpiritPos();
            }
        }
        /// <summary>
        /// 同步精灵位置
        /// </summary>
        private void SetSpiritPos()
        {
            if (!isInView(spiritObj.transform.position))
            {
                spiritFly();
            }
        }
        /// <summary>
        /// 判断位置是否在相机视野内
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        private bool isInView(Vector3 worldPos)
        {
            Vector2 viewPos = mStaticThings.I.Maincamera.GetComponent<Camera>().WorldToViewportPoint(worldPos);
            Vector3 dir = (worldPos - mStaticThings.I.Maincamera.position).normalized;
            float dot = Vector3.Dot(mStaticThings.I.Maincamera.forward, dir);//判断物体是否在相机前面

            if (dot > 0 && viewPos.x >= 0.4f && viewPos.x <= 0.9f && viewPos.y >= 0.1f && viewPos.y <= 0.9f) return true;
            else return false;
        }

        private void spiritFly() 
        {
            if (mStaticThings.I.isVRApp) 
            {
                if (spiritObj.transform.parent != cameraTemp) 
                {
                    spiritObj.transform.SetParent(cameraTemp);
                    spiritObj.transform.DOLocalMove(new Vector3(0, 0, 1f), 0.5f).OnComplete(() =>
                    {
                        spiritObj.transform.SetParent(otherTemp);

                    });
                }
            }
        }
        
        private void SpiritPos() 
        {
            if (mStaticThings.I.isVRApp)
            {
                spiritObj.transform.SetParent(cameraTemp);
                spiritObj.transform.DOLocalMove(new Vector3(0, 0, 1f), 0.1f).OnComplete(() =>
                {
                    spiritObj.transform.SetParent(otherTemp);

                });
            }
        }
    }
}
