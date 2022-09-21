using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.IO;

namespace Dll_Project.Showroom.PCCameraControl
{
    public class CameraControl : DllGenerateBase
    {
        private List<Transform> onetfList = new List<Transform>();
        private List<Transform> twotfList = new List<Transform>();

        private Transform oneTfParent;
        private Transform ctrlOneTfParent;
        private GameObject ControlUI;
        private Slider fieldOfViewSlider;
        private Slider moveSpeedSlider;
        private Button fullScreenOnBtn;
        private Button fullScreenOffBtn;
        private Button screenShotBtn;
        private Transform showPicture;
        private InputField pathInputField;

        bool Pg = false;
        public override void Init()
        {
            oneTfParent = BaseMono.ExtralDatas[0].Target;
            ctrlOneTfParent = BaseMono.ExtralDatas[1].Target;
            ControlUI = BaseMono.ExtralDatas[2].Target.gameObject;
            fieldOfViewSlider = BaseMono.ExtralDatas[3].Target.GetComponent<Slider>();
            moveSpeedSlider = BaseMono.ExtralDatas[4].Target.GetComponent<Slider>();
            fullScreenOnBtn = BaseMono.ExtralDatas[5].Target.GetComponent<Button>();
            fullScreenOffBtn = BaseMono.ExtralDatas[6].Target.GetComponent<Button>();
            screenShotBtn = BaseMono.ExtralDatas[7].Target.GetComponent<Button>();
            showPicture = BaseMono.ExtralDatas[7].Info[0].Target;
            pathInputField = BaseMono.ExtralDatas[8].Target.GetComponent<InputField>();
        }
        #region 初始化
        public override void Awake()
        {
        }

        public override void Start()
        {
            for (int i = 0; i < oneTfParent.childCount; i++)
            {
                onetfList.Add(oneTfParent.GetChild(i));
            }
            for (int i = 0; i < ctrlOneTfParent.childCount; i++)
            {
                twotfList.Add(ctrlOneTfParent.GetChild(i));
            }
            fieldOfViewSlider.onValueChanged.AddListener(FieldOfViewSlider);
            moveSpeedSlider.onValueChanged.AddListener(MoveSpeedSlider);
            fullScreenOnBtn.onClick.AddListener(() => { PatternClick(true); });
            fullScreenOffBtn.onClick.AddListener(() => { PatternClick(false); });
            screenShotBtn.onClick.AddListener(ScreenShotClick);
        }

        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }

        public override void Update()
        {
            if (!mStaticThings.I.ismobile)
            {
                if (!UIControl.commandson)
                {
                    return;
                }

                if (Input.GetKeyUp(KeyCode.PageUp))
                {
                    Pg = false;
                    ShowOrHideUI(false);
                }

                if (Input.GetKeyUp(KeyCode.PageDown))
                {
                    Pg = true;
                    ShowOrHideUI(true);
                }

                if (Pg == true)
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        if (Input.GetKeyUp(KeyCode.Alpha1))
                        {
                            CameraTwoClick(twotfList[0], twotfList[1], twotfList[2]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha2))
                        {
                            CameraNineClick(twotfList[3], twotfList[4], twotfList[5], twotfList[6], twotfList[7], twotfList[8], twotfList[9], twotfList[10], twotfList[11]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha3))
                        {
                            CameraOneClick(twotfList[12], twotfList[13]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha4))
                        {
                            CameraOneClick(twotfList[14], twotfList[15]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha5)) 
                        {
                            CameraOneClick(twotfList[16], twotfList[17]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha6))
                        {
                            CameraOneClick(twotfList[18], twotfList[19]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha7))
                        {
                            CameraOneClick(twotfList[20], twotfList[21]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha8))
                        {
                            CameraOneClick(twotfList[22], twotfList[23]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha9))
                        {
                            CameraOneClick(twotfList[24], twotfList[25]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha0))
                        {
                            CameraOneClick(twotfList[26], twotfList[27]);
                        }
                    }
                    else
                    {
                        if (Input.GetKeyUp(KeyCode.K))
                        {
                            ScreenShotClick();
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha1))
                        {
                            FixedCameraClick(onetfList[0]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha2))
                        {
                            FixedCameraClick(onetfList[1]);
                        }
                        else if (Input.GetKeyUp(KeyCode.Alpha3))
                        {
                            FixedCameraClick(onetfList[2]);
                        }
                    }


                }
            }
        }
        #endregion

        #region 按键1
        private void FixedCameraClick(Transform tf)//固定相机位置
        {
            if (isCameraOne) 
            {
                mStaticThings.I.PCCamra.position = tf.position;
                mStaticThings.I.PCCamra.rotation = tf.rotation;
            }
        }

        /// <summary>
        /// 不同机位运动轨迹
        /// </summary>
        bool isCameraOne = true;
        private void CameraOneClick(Transform tf1, Transform tf2)//滑轨相机位置
        {
            if (isCameraOne)
            {
                isCameraOne = false;
                mStaticThings.I.PCCamra.position = tf1.position;
                mStaticThings.I.PCCamra.rotation = tf1.rotation;

                MoveAndRotatel(tf2, 6, Ease.Linear, () => {
                    isCameraOne = true;
                });
            }
        }
        private void CameraTwoClick(Transform tf1, Transform tf2, Transform tf3)//滑轨相机位置
        {
            if (isCameraOne)
            {
                isCameraOne = false;
                mStaticThings.I.PCCamra.position = tf1.position;
                mStaticThings.I.PCCamra.rotation = tf1.rotation;

                MoveAndRotatel(tf2, 6, Ease.Linear,()=> {
                    MoveAndRotatel(tf3, 3, Ease.Linear, () => {
                        isCameraOne = true;
                    });
                });
            }
        }
        private void CameraNineClick(Transform tf1, Transform tf2, Transform tf3, Transform tf4, Transform tf5, Transform tf6, Transform tf7, Transform tf8, Transform tf9)//滑轨相机位置
        {
            if (isCameraOne)
            {
                isCameraOne = false;
                mStaticThings.I.PCCamra.position = tf1.position;
                mStaticThings.I.PCCamra.rotation = tf1.rotation;

                MoveAndRotatel(tf2, 2, Ease.Linear,()=> {
                        MoveAndRotatel(tf3, 3, Ease.Linear,()=> {
                            MoveAndRotatel(tf4, 7, Ease.Linear, () => {
                                MoveAndRotatel(tf5, 3, Ease.Linear, () => {
                                    MoveAndRotatel(tf6, 2, Ease.Linear, () => {
                                        MoveAndRotatel(tf7, 3, Ease.Linear, () => {
                                            MoveAndRotatel(tf8, 7, Ease.Linear, () => {
                                                MoveAndRotatel(tf9, 2, Ease.Linear, () => {
                                                    isCameraOne = true;
                                                });
                                            });
                                        });
                                    });
                                });
                            });
                        });
                    });
            }
        }
        

        private void MoveAndRotatel(Transform tf, float time, Ease ease, Action action = null)
        {
            Tweener twPostion = mStaticThings.I.PCCamra.DOLocalMove(tf.position, time).OnComplete(() => { action(); });
            twPostion.SetEase(ease);
            twPostion.SetAutoKill(true);
            twPostion.Play();

            Tweener twRotate = mStaticThings.I.PCCamra.DOLocalRotateQuaternion(tf.rotation, time);
            twRotate.SetEase(ease);
            twRotate.SetAutoKill(true);
            twRotate.Play();
        }
        #endregion

        private void ShowOrHideUI(bool isOpen) 
        {
            var temp = GameObject.Find("LoginUICanvas");
            if (temp != null) 
            {
                temp.transform.Find("RoomInPanel/room/CameraControl").gameObject.SetActive(false);
                ControlUI.SetActive(isOpen);
            }
        }

        /// <summary>
        /// 摄像机深度调节
        /// </summary>
        /// <param name="value"></param>
        private void FieldOfViewSlider(float value)
        {
            fieldOfViewSlider.transform.GetComponentInChildren<Text>().text = value.ToString();
            mStaticThings.I.PCCamra.GetComponent<Camera>().fieldOfView = value;
        }
        /// <summary>
        /// 摄像移动速度
        /// </summary>
        /// <param name="value"></param>
        private void MoveSpeedSlider(float value)
        {
            moveSpeedSlider.transform.GetComponentInChildren<Text>().text = value.ToString();
            var temp = GameObject.Find("LoginUICanvas");
            if (temp != null)
            {
                temp.transform.Find("RoomInPanel/room/CameraControl/InputField").GetComponent<InputField>().text = value.ToString();

            }
        }
        /// <summary>
        /// 控制exe全屏和非全屏
        /// </summary>
        /// <param name="isBool"></param>
        private void PatternClick(bool isBool) 
        {
            if (isBool)
            {
                Resolution[] resolutions = Screen.resolutions;
                Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);

                Screen.fullScreen = true;
                PlayerPrefs.SetInt("fullScreenMode", 1);
            }
            else 
            {
                Screen.SetResolution(1280, 720, true);
                Screen.fullScreen = false;
                Screen.fullScreenMode = FullScreenMode.Windowed;
                PlayerPrefs.SetInt("fullScreenMode", 0);
            }
        }

        #region 相机截图
        string photoPath = "D:/Sina/Screenshot/";//PC端路径

        private void ScreenShotClick() 
        {
            if (!string.IsNullOrEmpty(pathInputField.text)) 
            {
                photoPath = pathInputField.text;
            }
            if (showPicture.GetComponent<RectTransform>().localScale.x <0.02f) 
            {
                var temp = CaptureCamera(mStaticThings.I.PCCamra.GetComponent<Camera>(), new Rect(0, 0, 1920, 1080), photoPath);

                showPicture.GetComponent<RawImage>().texture = temp;

                showPicture.GetComponent<RectTransform>().localScale = Vector3.one;

                showPicture.DOScale(0, 1.5f);
            }
        }
        /// <summary>
        /// 对相机看到的画面截图保存
        /// </summary>
        /// <param name="camera">截图的相机</param>
        /// <param name="rect">截图大小</param>
        /// <param name="filePath">保存路径</param>
        /// <param name="maskLayer">如果需要隐藏某层，传入层对应的数字</param>
        /// <returns></returns>
        Texture2D CaptureCamera(Camera camera, Rect rect, string filePath, int maskLayer = -1)
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
            byte[] bytes = screenShot.EncodeToPNG();
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string filename = filePath + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-0-I-.png";
            File.WriteAllBytes(filename, bytes);
            return screenShot;
        }
        #endregion
    }
}
