using com.ootii.Messages;
using Dll_Project.Showroom;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Plaza.Photograph
{
    public class files//获取图片的属性结构
    {
        public string url { get; set; }
        public string path { get; set; }
    }
    public class ShowPhoto : DllGenerateBase
    {
        private Transform photoParent;
        private Transform ShowImagePanel;
        private Image bGImagePanel;
        private Button leftBtn;
        private Button rightBtn;

        //展示下载图片
        private GameObject pCPanel;
        private Button pCLoadBtn;
        private GameObject andPanel;

        private Transform uiCanvas;
        private float uiWidth;
        private float uiHeight;
        public override void Init()
        {
            photoParent = BaseMono.ExtralDatas[0].Target;
            ShowImagePanel = BaseMono.ExtralDatas[1].Target;
            bGImagePanel = BaseMono.ExtralDatas[2].Target.GetComponent<Image>();
            leftBtn = BaseMono.ExtralDatas[3].Target.GetComponent<Button>();
            rightBtn = BaseMono.ExtralDatas[4].Target.GetComponent<Button>();

            pCPanel = BaseMono.ExtralDatas[5].Target.gameObject;
            pCLoadBtn = BaseMono.ExtralDatas[6].Target.GetComponent<Button>();
            andPanel = BaseMono.ExtralDatas[7].Target.gameObject;

            uiCanvas = BaseMono.ExtralDatas[8].Target;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            //photoParent.parent.GetComponent<Canvas>().worldCamera = mStaticThings.I.Maincamera.GetComponent<Camera>();
            BaseMono.StartCoroutine(GetPhotoList(9));
            leftBtn.onClick.AddListener(UpPageClick);
            rightBtn.onClick.AddListener(NextPageClick);

            ShowImagePanel.GetComponent<Button>().onClick.AddListener(ShowImageClick);
            pCLoadBtn.onClick.AddListener(PcLoadImageClick);

            uiWidth = uiCanvas.GetComponent<CanvasScaler>().referenceResolution.x;
            uiHeight = uiCanvas.GetComponent<CanvasScaler>().referenceResolution.y;
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointEnter.ToString(), PointEnterClick);
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointExit.ToString(), PointExitClick);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointEnter.ToString(), PointEnterClick);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointExit.ToString(), PointExitClick);
        }

        public override void Update()
        {
        }
        #endregion

        private void PointEnterClick(IMessage msg) 
        {
            GameObject temp = msg.Data as GameObject;
            if (temp.name.Contains("Img-"))
            {
                temp.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
        private void PointExitClick(IMessage msg)
        {
            GameObject temp = msg.Data as GameObject;
            if (temp.name.Contains("Img-"))
            {
                temp.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        #region 获取图片文件
        private string GetUrl;
        int count;
        List<string> paths = new List<string> {""};
        List<files> files = new List<files>();
        private IEnumerator GetPhotoList(int size,string marker=null)
        {
            yield return new WaitForSeconds(0);
            if (string.IsNullOrEmpty(mStaticData.AllURL.TakePhotoURL))
            {
                BaseMono.StartCoroutine(GetPhotoList(size));
            }
            else 
            {
                GetUrl = mStaticData.AllURL.TakePhotoURL + "/oss/list/files" + "?dir_name=" + mStaticData.PhotoFileName + "&size=" + size + "&marker=" + marker;
                UnityWebRequest uwr = UnityWebRequest.Get(GetUrl);
                yield return uwr.SendWebRequest();
                JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);
                JsonData jsonData = jd["data"];
                files.Clear();
                for (int i = 0; i < photoParent.childCount; i++)
                {
                    photoParent.GetChild(i).gameObject.SetActive(false);
                }
                files = JsonMapper.ToObject<List<files>>(jsonData["files"].ToJson());
                if (jsonData["next"].ToJson() == "true") 
                {
                    if (!paths.Contains(files[files.Count - 1].path)) 
                    {
                        paths.Add(files[files.Count - 1].path);
                    }
                }
                Showphoto();
            }
        }

        private void Showphoto() 
        {
            if (files.Count == 0)
                return;
            count = 0;
            string imageUrl = files[count].url;
            BaseMono.StartCoroutine(LoadImage(photoParent.GetChild(count), imageUrl, count, Back));
        }
        private IEnumerator LoadImage(Transform tf,string imageUrl,int index,Action back)
        {
            var uwr = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error) || uwr.isNetworkError || uwr.isHttpError)
            {
                yield return new WaitForSeconds(1);
                uwr.Dispose();
                BaseMono.StartCoroutine(LoadImage(tf,imageUrl, index, back));
            }
            else
            {
                Texture2D mTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                tf.GetComponent<Image>().sprite= Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), new Vector2(0.5f, 0.5f));
                tf.GetChild(0).GetComponent<Text>().text = imageUrl;
                tf.GetComponent<Button>().onClick.AddListener(() => { ShowImage(tf, mTexture.width, mTexture.height, mTexture); });
                tf.gameObject.SetActive(true);
                count++;
                if (count < files.Count) 
                {
                    Back();
                }
            }
        }

        private void Back() 
        {
            if (files.Count !=0) 
            {
                string imageUrl = files[count].url;
                BaseMono.StartCoroutine(LoadImage(photoParent.GetChild(count), imageUrl, count, Back));
            }
        }
        #endregion

        #region 图片翻页
        private int pageCount;
        private void UpPageClick()
        {
            pageCount--;
            if (pageCount < 0)
            {
                pageCount = 0;
            }
            else if (pageCount < paths.Count)
            {
                BaseMono.StartCoroutine(GetPhotoList(9, paths[pageCount]));
            }

        }

        private void NextPageClick()
        {
            pageCount++;
            if (pageCount < paths.Count)
            {
                BaseMono.StartCoroutine(GetPhotoList(9, paths[pageCount]));
            }
            else if (pageCount >= paths.Count)
            {
                pageCount = paths.Count - 1;
            }
        }
        #endregion

        #region 展示图片与下载
        bool isopen = true;
        private void ShowImageClick() 
        {
            if (!mStaticThings.I.isVRApp)
            {
                if (isopen)
                {
                    if (mStaticThings.I.ismobile)
                    {
                        andPanel.SetActive(true);
                        isopen = false;
                    }
                    else
                    {
                        pCPanel.SetActive(false);
                        ShowImagePanel.gameObject.SetActive(false);
                        ShowImagePanel.parent.GetComponent<Image>().enabled = false;
                        ShowOrHideUI(true);
                    }

                }
                else
                {
                    ShowImagePanel.gameObject.SetActive(false);
                    ShowImagePanel.parent.GetComponent<Image>().enabled = false;
                    isopen = true;
                    ShowOrHideUI(true);
                }
            }
            else 
            {
                ShowImagePanel.gameObject.SetActive(false);
                ShowImagePanel.parent.GetComponent<Image>().enabled = false;
            }
        }

        private void ShowImage(Transform tf,float width,float height, Texture2D mTexture)
        {
            if (ShowImagePanel.gameObject.activeSelf == false)
            {
                if (mStaticThings.I.isVRApp)
                {
                    if (!uiCanvas.gameObject.activeSelf)
                    {
                        uiCanvas.gameObject.SetActive(true);
                    }
                    ShowImagePanel.GetComponent<Image>().sprite = tf.GetComponent<Image>().sprite;

                    ShowImagePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1246, 712);
                    ShowImagePanel.gameObject.SetActive(true);
                    bGImagePanel.enabled = true;
                    texture2D = mTexture;
                }
                else 
                {
                    if (mStaticThings.I.ismobile)
                    {
                        ShowImagePanel.GetComponent<Image>().sprite = tf.GetComponent<Image>().sprite;

                        ShowImagePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(uiWidth, uiHeight);
                        ShowImagePanel.gameObject.SetActive(true);
                        bGImagePanel.enabled = true;
                        texture2D = mTexture;
                        ShowOrHideUI(false);
                    }
                    else
                    {
                        ShowImagePanel.GetComponent<Image>().sprite = tf.GetComponent<Image>().sprite;

                        ShowImagePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(uiWidth - 40, uiHeight - 40); ;
                        ShowImagePanel.gameObject.SetActive(true);
                        bGImagePanel.enabled = true;
                        texture2D = mTexture;
                        ShowOrHideUI(false);
                        pCPanel.SetActive(true);
                        pCPanel.transform.Find("Text").GetComponent<Text>().text = "图片保存在：" + photoPath;
                    }
                }
            }
        }

        string photoPath = "D:/Sina/Screenshot/";//PC端路径
        Texture2D texture2D;
        /// <summary>
        /// pc端下载图片到本地
        /// </summary>
        private void PcLoadImageClick() 
        {
            Texture2D screenShot = texture2D;
            // 最后将这些纹理数据，成一个png图片文件  
            byte[] bytes = screenShot.EncodeToPNG();
            if (!Directory.Exists(photoPath))
            {
                Directory.CreateDirectory(photoPath);
            }
            string filename = photoPath + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-0-I-.png";
            File.WriteAllBytes(filename, bytes);
            pCPanel.SetActive(false);
            SaveInfo.instance.SaveActionData("SavePhoto", 10);
        }

        //控制屏幕UI的显隐
        private void ShowOrHideUI(bool isBool)
        {
            var temp = GameObject.Find("LoginUICanvas");
            if (temp != null)
            {
                temp.transform.Find("RoomInPanel").gameObject.SetActive(isBool);
            }
        }
        #endregion
    }

}
