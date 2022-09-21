using com.ootii.Messages;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Showroom.InfoFolder
{
    public class GetAvatorImage : DllGenerateBase
    {
        private GameObject avatarImgBG;
        private GameObject avatarImgCM;
        private GameObject avatarImg;
        public override void Init()
        {
            avatarImgBG = BaseMono.ExtralDatas[0].Target.gameObject;
            avatarImgCM = BaseMono.ExtralDatas[1].Target.gameObject;
            avatarImg = BaseMono.ExtralDatas[2].Target.gameObject;
        }
        #region
        public override void Awake()
        {
        }

        public override void Start()
        {
            BaseMono.StartCoroutine(IE_GetAvatars());
        }
        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }

        public override void Update()
        {
        }
        #endregion

        IEnumerator IE_GetAvatars()
        {
            string url = mStaticThings.serverhttp + mStaticThings.I.now_ServerURL + "/" + mStaticThings.apiversion + "/getavatars?userid=" + mStaticThings.I.mAvatarID + "&sex=" + mStaticThings.I.msex + "&appname=" + "vsvr" + "&groupid=" + mStaticThings.I.now_groupid + "&device=" + mStaticThings.I.nowdevicename;
            url += mStaticThings.urltokenfix;
            UnityWebRequest request = UnityWebRequest.Get(@url);
            yield return request.SendWebRequest();
            if (request.error != null)
            {
                request.Dispose();
                yield break;
            }

            JsonData resjson = JsonMapper.ToObject(request.downloadHandler.text);
            if (resjson["info"].ToString() == "ok")
            {
                if (resjson["data"][0]["vr_avatars_id"] != null)
                {
                    Getavatars(resjson["data"]);
                }
            }
            request.Dispose();
        }

        private void Getavatars(JsonData resjson)
        {
            for (int i = 0; i < resjson.Count; i++)
            {
                JsonData avatar = resjson[i];
                if (avatar["vr_avatars_id"] != null)
                {
                    string aid = avatar["vr_avatars_id"].ToString();
                    if (aid == mStaticThings.I.aid)
                    {
                        GetURl(avatar);
                        return;
                    }
                }
            }
        }

        private void GetURl(JsonData lavatar)
        {
            string avticon = lavatar["vr_avatars_icon"].ToString();
            BaseMono.StartCoroutine(GetImage(avticon));
        }

        IEnumerator GetImage(string url)
        {
            var uwr = UnityWebRequestTexture.GetTexture(url);

            yield return uwr.SendWebRequest();

            Texture2D mTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;


            avatarImgBG.GetComponent<Image>().sprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), new Vector2(0.5f, 0.5f));
            avatarImgCM.GetComponent<Image>().sprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), new Vector2(0.5f, 0.5f));
            avatarImg.GetComponent<Image>().sprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
