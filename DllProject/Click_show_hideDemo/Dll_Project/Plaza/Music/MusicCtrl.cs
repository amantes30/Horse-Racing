using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Dll_Project.Plaza.Music
{
    public class MusicCtrl : DllGenerateBase
    {
        public static MusicCtrl Instance;
        private Dictionary<string, AudioClip> myMusic = new Dictionary<string, AudioClip>();

        public AudioSource bgAudioSource;
        public AudioSource flyAudioSource;
        public override void Init()
        {
            bgAudioSource = BaseMono.ExtralDatas[0].Target.GetComponent<AudioSource>();
            flyAudioSource = BaseMono.ExtralDatas[1].Target.GetComponent<AudioSource>();
        }
        #region 初始
        public override void Awake()
        {
            Instance = this;
        }

        public override void Start()
        {
            BaseMono.StartCoroutine(ShowBGMusic());
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

        private IEnumerator ShowBGMusic()
        {
            yield return new WaitForSeconds(0.2f);
            if (mStaticData.AllMusics.Count == 0)
            {
                BaseMono.StartCoroutine(ShowBGMusic());
            }
            else 
            {
                PlayBgMusic(true);
            }
        }

        public void PlayBgMusic(bool isOpen) 
        {
            if (isOpen) 
            {
                BaseMono.StartCoroutine(LoadOrPlayMusic(bgAudioSource, mStaticData.AllMusics[0].MusicUrl, mStaticData.AllMusics[0].Volume));
            }
            else 
            {
                bgAudioSource.Stop();
            }
        }
        public void PlayFlyMusic(bool isOpen)
        {
            if (isOpen)
            {
                BaseMono.StartCoroutine(LoadOrPlayMusic(flyAudioSource, mStaticData.AllMusics[1].MusicUrl, mStaticData.AllMusics[1].Volume));
            }
            else
            {
                flyAudioSource.Stop();
            }
        }
        //下载或播放音乐【防止未下载完就播放】
        public IEnumerator LoadOrPlayMusic(AudioSource audioSource,string musicPath,float volume)
        {
            if (myMusic.ContainsKey(musicPath))
            {
                audioSource.clip = myMusic[musicPath];
                audioSource.loop = true;
                audioSource.volume = volume;
                audioSource.Play();
                yield return null;

            }
            else
            {

                UnityWebRequest _unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(musicPath, AudioType.WAV);
                yield return _unityWebRequest.SendWebRequest();

                if (_unityWebRequest.isHttpError || _unityWebRequest.isNetworkError)
                {
                    _unityWebRequest.Dispose();
                    BaseMono.StartCoroutine(LoadOrPlayMusic(audioSource, musicPath, volume));
                }
                else
                {
                    AudioClip _audioClip = DownloadHandlerAudioClip.GetContent(_unityWebRequest);
                    if (myMusic.ContainsKey(musicPath))
                    {
                        myMusic[musicPath] = _audioClip;
                    }
                    else
                    {
                        myMusic.Add(musicPath, _audioClip);
                    }

                    audioSource.clip = _audioClip;
                    audioSource.loop = true;
                    audioSource.volume = volume;
                    audioSource.Play();
                }
            }

        }
    }
}
