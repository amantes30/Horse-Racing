using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Dll_Project.Showroom
{
    public class ActionData
    {
        public string user_id;
        public string model_name;
        public int action;
        public long createAt;
    }
    public class InfoCollectController : DllGenerateBase
    {
        public static InfoCollectController Instance;

        //public static string url = "http://192.168.2.106:7001";//"/user/add";
        public static string url = "http://121.37.129.57:80/col";//"/user/add";

        private string urlTimezoom = string.Format("{0}{1}", url, "/timezoom/add");//添加时空信息
        private string urlViewdata = string.Format("{0}{1}", url, "/viewdata/add");//添加视野交互信息
        private string urlActiondata = string.Format("{0}{1}", url, "/actiondata/add");//添加行为交互信息
        private string urlKeyevent = string.Format("{0}{1}", url, "/keyevent/add");//添加关键事件信息
        private string urlModel = string.Format("{0}{1}", url, "/model/add");//添加模型信息
        private string urlAction = string.Format("{0}{1}", url, "/action/add");//添加行为信息
        private string urlCategory = string.Format("{0}{1}", url, "/category/add");//添加类别信息

        public bool isOpen = false;
        public bool isSaveTimeZoom = false;
        public bool isSaveViewData = false;

        public override void Init()
        {
            Instance = this;
        }
       
        /// <summary>
        /// 保存时空信息
        /// </summary>
        /// <returns></returns>
        public void saveTimeZoomInfo(string id, string position, string rotation, long createAt,string roomId)
        {
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("userId", id);
            wwwForm.AddField("position", position);
            wwwForm.AddField("rotation", rotation);
            wwwForm.AddField("createAt", createAt.ToString());
            wwwForm.AddField("room_id", roomId);
            UnityWebRequest uwr = UnityWebRequest.Post(urlTimezoom, wwwForm);
            uwr.SendWebRequest();
            
        }

        /// <summary>
        /// 保存视野交互信息
        /// </summary>
        /// <returns></returns>
        public void saveViewDataInfo(string id, string model_names, string users, long createAt, string roomId)
        {
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("userId", id);
            wwwForm.AddField("model_names", model_names);
            wwwForm.AddField("users", users);
            wwwForm.AddField("createAt", createAt.ToString());
            wwwForm.AddField("room_id", roomId);
            UnityWebRequest uwr = UnityWebRequest.Post(urlViewdata, wwwForm);
            uwr.SendWebRequest();

        }

        /// <summary>
        /// 保存行为交互信息
        /// </summary>
        /// <returns></returns>
        public void saveActionDataInfo(string id, string model_name, string action, long createAt, string roomId)
        {
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("userId", id);
            wwwForm.AddField("model_name", model_name);
            wwwForm.AddField("action", action);
            wwwForm.AddField("createAt", createAt.ToString());
            wwwForm.AddField("room_id", roomId);
            UnityWebRequest uwr = UnityWebRequest.Post(urlActiondata, wwwForm);
            uwr.SendWebRequest();
        }

        /// <summary>
        /// 保存关键事件信息
        /// </summary>
        /// <returns></returns>
        public void saveKeyEventInfo(string id, string model_name, int action, long createAt,string et, string roomId)
        {
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("userId", id);
            wwwForm.AddField("model_name", model_name);
            wwwForm.AddField("action", action);
            wwwForm.AddField("createAt", createAt.ToString());
            wwwForm.AddField("event", et);
            wwwForm.AddField("room_id", roomId);
            UnityWebRequest uwr = UnityWebRequest.Post(urlKeyevent, wwwForm);
            uwr.SendWebRequest();

        }

        /// <summary>
        /// 保存模型信息
        /// </summary>
        /// <returns></returns>
        public void saveModelInfo(string name, int category, string position, string rotation,string alias)
        {
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("name", name);
            wwwForm.AddField("category", category);
            wwwForm.AddField("position", position);
            wwwForm.AddField("rotation", rotation);
            wwwForm.AddField("alias", alias);
            UnityWebRequest uwr = UnityWebRequest.Post(urlModel, wwwForm);
            uwr.SendWebRequest();

        }

        /// <summary>
        /// 保存行为信息
        /// </summary>
        /// <returns></returns>
        public void saveActionInfo(string name, int id)
        {
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("name", name);
            wwwForm.AddField("id", id);
            UnityWebRequest uwr = UnityWebRequest.Post(urlAction, wwwForm);
            uwr.SendWebRequest();

        }

        /// <summary>
        /// 保存类别信息
        /// </summary>
        /// <returns></returns>
        public void saveCategoryInfo(string name, int id)
        {
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("name", name);
            wwwForm.AddField("id", id);
            UnityWebRequest uwr = UnityWebRequest.Post(urlCategory, wwwForm);
            uwr.SendWebRequest();
        }
    }
}
