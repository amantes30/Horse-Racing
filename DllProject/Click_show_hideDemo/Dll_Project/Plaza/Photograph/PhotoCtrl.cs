using com.ootii.Messages;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Plaza.Photograph
{
    public class PhotoCtrl : DllGenerateBase
    {
        private Button takePhotoBtn;
        public override void Init()
        {
            takePhotoBtn = BaseMono.ExtralDatas[0].Target.GetComponent<Button>();
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            takePhotoBtn.onClick.AddListener(ScreenShotClick);
        }
        public override void OnEnable()
        {
            //MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void OnDisable()
        {
            //MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void Update()
        {
            //if (Input.GetKeyUp(KeyCode.H)) 
            //{
            //    ScreenShotClick();
            //}
            if (takePhotoBtn.gameObject.activeSelf==false && mStaticData.Identity.Camera.Contains(mStaticThings.I.mAvatarID))
            {
                takePhotoBtn.gameObject.SetActive(true);
            }
        }
        #endregion
        /// <summary>
        /// 地面检测
        /// </summary>
        /// <param name="msg"></param>
        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("area_photo") && string.IsNullOrEmpty(host) && mStaticData.Identity.Camera.Contains(mStaticThings.I.mAvatarID))
            {
                //BaseMono.StartCoroutine(Geturl());
            }
            //if (name.Equals("area_photo") && mStaticData.Identity.Camera.Contains(mStaticThings.I.mAvatarID))
            //{
            //    takePhotoBtn.gameObject.SetActive(true);
            //}
            //else 
            //{
            //    takePhotoBtn.gameObject.SetActive(false);
            //}
        }

        #region
        /// <summary>
        /// 获取上传签名url
        /// </summary>
        private string GetUrl;
        string key;
        string policy;
        string signature;
        string OSSAccessKeyId;
        string host;
        private IEnumerator Geturl()
        {
            GetUrl = mStaticData.AllURL.TakePhotoURL + "/oss/post/url" + "?dir_name=" + mStaticData.PhotoFileName;
            UnityWebRequest uwr = UnityWebRequest.Get(GetUrl);
            yield return uwr.SendWebRequest();
            JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);
            Debug.LogError(uwr.downloadHandler.text);
            key = jd["key"].ToString();
            policy = jd["policy"].ToString();
            signature = jd["signature"].ToString();
            OSSAccessKeyId = jd["OSSAccessKeyId"].ToString();
            host = jd["host"].ToString();
        }

        private void UpLoadImage(byte[] aa)
        {
            GetUrl = mStaticData.AllURL.TakePhotoURL + "/oss/images";
            long timeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("key", key + "/" + timeStamp + ".jpg");
            dic.Add("policy", policy);
            dic.Add("signature", signature);
            dic.Add("OSSAccessKeyId", OSSAccessKeyId);
            dic.Add("success_action_status", "200");
            UploadFilesToRemoteUrl(host, aa, dic);
        }

        /// <summary>
        /// 请求上传图片到阿里云
        /// </summary>
        /// <param name="url">上传地址</param>
        /// <param name="filepath">本地文件路径</param>
        /// <param name="dic">上传的数据信息</param>
        /// <returns></returns>
        public bool UploadFilesToRemoteUrl(string url, byte[] imagebyte, Dictionary<string, string> dic)
        {
            try
            {
                //string boundary = DateTime.Now.Ticks.ToString("x");
                string boundary = System.Guid.NewGuid().ToString("N");

            byte[] boundarybytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "\r\n");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";

            request.ContentType = "multipart/form-data; boundary=" + boundary;

            Stream rs = request.GetRequestStream();

            var endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n" + "\r\n" + "{1}" + "\r\n";
            if (dic != null)
            {
                foreach (string key in dic.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);

                    string formitem = string.Format(formdataTemplate, key, dic[key]);

                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);

                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
            }

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n\r\n";
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);

                var header = string.Format(headerTemplate, "file", "aa.png");

                var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);

                rs.Write(headerbytes, 0, headerbytes.Length);

                rs.Write(imagebyte, 0, imagebyte.Length);

                var cr = Encoding.UTF8.GetBytes("\r\n");

                rs.Write(cr, 0, cr.Length);
            }

            rs.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

            var response = request.GetResponse() as HttpWebResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return false;
        }

        #endregion

        private IEnumerator UpLoadData(byte[] bytes) 
        {
            GetUrl = mStaticData.AllURL.TakePhotoURL + "/oss/images";
            long timeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
            WWWForm wWWForm = new WWWForm();
            wWWForm.AddField("dir_name", mStaticData.PhotoFileName+"/" + timeStamp + ".jpg");
            wWWForm.AddField("file", System.Convert.ToBase64String(bytes));
            UnityWebRequest uwr = UnityWebRequest.Post(GetUrl, wWWForm);
            yield return uwr.SendWebRequest();
            //Debug.LogError(GetUrl);

            //if (uwr.isNetworkError || uwr.isHttpError)
            //{
            //    Debug.LogError("上传失败:" + uwr.error);
            //}
            //else
            //{
            //    Debug.LogError(uwr.downloadHandler.text);
            //}
        }

        #region 相机截图

        private void ScreenShotClick()
        {
            var temp = CaptureCamera(mStaticThings.I.Maincamera.GetComponent<Camera>(), new Rect(0, 0, 1920, 1080));
        }
        /// <summary>
        /// 对相机看到的画面截图保存
        /// </summary>
        /// <param name="camera">截图的相机</param>
        /// <param name="rect">截图大小</param>
        /// <param name="filePath">保存路径</param>
        /// <param name="maskLayer">如果需要隐藏某层，传入层对应的数字</param>
        /// <returns></returns>
        Texture2D CaptureCamera(Camera camera, Rect rect, int maskLayer = -1)
        {
            if (maskLayer != -1)
            {
                camera.cullingMask &= ~(1 << maskLayer); // 关闭层x
            }
            // 创建一个RenderTexture对象  
            RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);
            //rt.depth<1 的时候会截取不到场景里的物体，只能截取到UI
            rt.depth = 10;
            // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机  
            camera.targetTexture = rt;
            //camera.RenderDontRestore(); //编辑器模式下可用，打包后运行报错
            camera.Render();
            // 激活这个rt, 并从中中读取像素。  
            RenderTexture.active = rt;
            //TextureFormat.ARGB32截取出来的UI 会显示半透明图片样式
            //Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素  
            screenShot.Apply();
            // 重置相关参数，以使用camera继续在屏幕上显示  
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors  
            GameObject.Destroy(rt);
            if (maskLayer != -1)
            {
                camera.cullingMask |= (1 << maskLayer); //打开层
            }
            // 最后将这些纹理数据，成一个png图片文件  
            byte[] bytes = screenShot.EncodeToJPG();
            //UpLoadImage(bytes);
            BaseMono.StartCoroutine(UpLoadData(bytes));
            return screenShot;
        }
        #endregion


    }
}
