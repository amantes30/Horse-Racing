using com.ootii.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Showroom.BoothInformation
{
    public class ShowBoothPicture : DllGenerateBase
    {
        public static ShowBoothPicture instance;
        private ExtralData[] BoothNumber;

        private ExtralDataObj[] BoothLoge1;
        private ExtralDataObj[] BoothPicture1;
        private ExtralDataObj[] GuideImage;
        public override void Init()
        {
            BoothNumber = BaseMono.ExtralDatas[2].Info;

            BoothLoge1 = BaseMono.ExtralDataObjs[0].Info;
            BoothPicture1 = BaseMono.ExtralDataObjs[1].Info;
            GuideImage = BaseMono.ExtralDataObjs[2].Info;
        }
        #region 初始
        public override void Awake()
        {
            instance = this;
        }

        public override void Start()
        {
            //BaseMono.StartCoroutine(LoadData(0.5f));
        }
        public override void OnEnable()
        {
            //MessageDispatcher.AddListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
        }

        public override void OnDisable()
        {
            //MessageDispatcher.RemoveListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
        }

        public override void Update()
        {
           
        }
        #endregion

        public IEnumerator LoadData(float delayTime = 0) 
        {
            yield return new WaitForSeconds(delayTime);
            for (int i = 0; i < mStaticData.BoothAsset.guideToVisitors.Count; i++)
            {
                DirInfo dirInfo = new DirInfo();
                dirInfo.ObjMat = GuideImage[i].Target as Material;
                dirInfo.Url = mStaticData.BoothAsset.guideToVisitors[i].GuideUrl;
                dirInfo.Md5 = mStaticData.BoothAsset.guideToVisitors[i].GuideMD5;
                ImgDir.Add(i, dirInfo);
            }
            for (int i = 0; i < mStaticData.BoothAsset.boothAssets.Count; i++)
            {
                DirInfo dirInfo = new DirInfo();
                dirInfo.ObjMat = BoothLoge1[i].Target as Material;
                dirInfo.Url = mStaticData.BoothAsset.boothAssets[i].LogeUrl;
                dirInfo.Md5 = mStaticData.BoothAsset.boothAssets[i].LogeMD5;
                ImgDir.Add(mStaticData.BoothAsset.guideToVisitors.Count + i, dirInfo);
            }
            for (int i = 0; i < mStaticData.BoothAsset.boothAssets.Count; i++)
            {
                DirInfo dirInfo = new DirInfo();
                dirInfo.ObjMat = BoothPicture1[i].Target as Material;
                dirInfo.Url = mStaticData.BoothAsset.boothAssets[i].PictureUrl;
                dirInfo.Md5 = mStaticData.BoothAsset.boothAssets[i].PictureMD5;
                ImgDir.Add(mStaticData.BoothAsset.boothAssets.Count+ mStaticData.BoothAsset.guideToVisitors.Count + i, dirInfo);
            }
            yield return new WaitForSeconds(delayTime/2);
            for (int i = 0; i < mStaticData.BoothAsset.boothAssets.Count; i++)
            {
                ShowBoothNum(mStaticData.BoothAsset.boothAssets[i].exhibition_id, i);
            }
            GetImage();
            //GetImg();
        }
        private void ShowBoothNum(string boothNum,int index) 
        {
            if (BoothNumber.Length < index) 
            {
                return;
            }
            for (int i = 0; i < BoothNumber[index].Target.childCount; i++)
            {
                BoothNumber[index].Target.GetChild(i).GetComponent<Text>().text = boothNum;
            }
        }

        #region 逐条下载图片

        private Dictionary<int, DirInfo> ImgDir = new Dictionary<int, DirInfo>();
        int count;
        private void GetImage() 
        {
            count = 0;
            BaseMono.StartCoroutine(GetImage(ImgDir[count], count, BackCall));
        }
        private IEnumerator GetImage(DirInfo dirInfo, int index,Action action)
        {
            if (!dirInfo.Url.StartsWith("http"))
            {
                yield break;
            }
            var uwr = UnityWebRequestTexture.GetTexture(dirInfo.Url);
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error) || uwr.isNetworkError || uwr.isHttpError)
            {
                uwr.Dispose();
                BaseMono.StartCoroutine(GetImage(dirInfo, index, action));
            }
            else 
            {
                Texture2D mTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                Material var = dirInfo.ObjMat;
                var.SetTexture("_BaseMap", mTexture);
                count++;
                if (ImgDir.Count > count)
                {
                    action();
                }
            }
        }
        private void BackCall() 
        {
            BaseMono.StartCoroutine(GetImage(ImgDir[count], count, BackCall));
        }
        #endregion
    }
    public class DirInfo 
    {
        public Material ObjMat;
        public string Url;
        public string Md5;
    }

    
}
