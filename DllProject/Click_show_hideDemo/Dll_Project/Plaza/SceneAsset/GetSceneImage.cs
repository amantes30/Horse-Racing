using com.ootii.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Dll_Project.Plaza.SceneAsset
{
    public class GetSceneImage : DllGenerateBase
    {
        private ExtralDataObj[] extralDataObjs;
        public override void Init()
        {
            extralDataObjs = BaseMono.ExtralDataObjs[0].Info;
        }
        #region 初始
        public override void Awake()
        {
        }
        public override void Start()
        {
            BaseMono.StartCoroutine(GetAsset());
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
        }

        public override void Update()
        {
        }
        #endregion
        private IEnumerator GetAsset()
        {
            yield return new WaitForSeconds(0.3f);
            if (mStaticData.SceneImage.Count == 0)
            {
                BaseMono.StartCoroutine(GetAsset());
            }
            else
            {
                GetImage();
            }
        }
        #region 加载图片
        void GetCacheFile(IMessage msg)
        {
            LocalCacheFile sendfile = msg.Data as LocalCacheFile;
            if (md5List.Contains(sendfile.sign)) 
            {
                BaseMono.StartCoroutine(LoadImage(sendfile.path, md5List.IndexOf(sendfile.sign)));
            }
        }
        private IEnumerator LoadImage(string imageUrl,int index) 
        {
            var uwr = UnityWebRequestTexture.GetTexture("File://"+imageUrl);
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error) || uwr.isNetworkError || uwr.isHttpError)
            {
                uwr.Dispose();
                BaseMono.StartCoroutine(LoadImage(imageUrl, index));
            }
            else
            {
                Texture2D mTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                Material var = extralDataObjs[index].Target as Material;
                //var.SetTexture("_BaseMap", mTexture);
                //var.SetTexture("_EmissionMap", mTexture);
                var.SetTexture("Texture2D_794AD0AE", mTexture);

                count++;
                if (mStaticData.SceneImage.Count > count)
                {
                    BackCall();
                }
            }
        }
        private void SendFile(string url, string sign)
        {
            LocalCacheFile sendfile = new LocalCacheFile()
            {
                path = url,
                isURLSign = false,
                sign = sign,
                hasPrefix = false,
                isKOD = false,
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.SendCacheFile.ToString(), sendfile, 0.01f);
        }
        #endregion
        #region 逐条下载图片
        private List<string> md5List = new List<string>();
        int count;
        private void GetImage()
        {
            count = 0;
            GetImage(mStaticData.SceneImage[count]);
        }
        private void GetImage(SceneImage imageLiset)
        {
            md5List.Add(imageLiset.ImageMD5 + imageLiset.ID);
            SendFile(imageLiset.ImageURL, imageLiset.ImageMD5 + imageLiset.ID);
        }


        private void BackCall()
        {
            GetImage(mStaticData.SceneImage[count]);
        }
        #endregion
    }
}
