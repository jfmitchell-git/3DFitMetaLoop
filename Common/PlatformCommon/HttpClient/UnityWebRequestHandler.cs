#if !BACKOFFICE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MetaLoop.Common.PlatformCommon.HttpClient
{
    public class UnityWebRequestHandler : MonoBehaviour
    {

        private static UnityWebRequestHandler instance;
        public static UnityWebRequestHandler Instance
        {
            get
            {
                return instance;
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void GetBodyFromHttp(string url, WWWForm postData, Action<UnityWebRequest> callback)
        {
            StartCoroutine(CreateStandardWebRequest(url, postData, callback));
        }

        public void GetBodyFromHttpWithProgress(string url, WWWForm postData, Action<UnityWebRequest> callback)
        {
            StartCoroutine(CreateStandardWebRequest(url, postData, callback, true));
        }



        public void GetImageFromUrl(string url, Action<Sprite> callback)
        {
            StartCoroutine(GetImageFromUrlWebRequest(url, callback));
        }

        private IEnumerator GetImageFromUrlWebRequest(string url, Action<Sprite> callback)
        {
            Sprite sprite = null;
            WWW www = new WWW(url);
            yield return www;
            sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
            callback.Invoke(sprite);
        }

        private IEnumerator CreateStandardWebRequest(string url, WWWForm postData, Action<UnityWebRequest> callback)
        {

            UnityWebRequest www;
            if (postData != null)
            {
                www = UnityWebRequest.Post(url, postData);
            }
            else
            {
                www = UnityWebRequest.Get(url);
            }



            yield return www.SendWebRequest();
            callback.Invoke(www);
            yield break;

        }


        private List<ulong> lastDownloadValue;
        private IEnumerator CreateStandardWebRequest(string url, WWWForm postData, Action<UnityWebRequest> callback, bool reportProgress)
        {
            lastDownloadValue = new List<ulong>();
            UnityWebRequest www;

            if (postData != null)
            {
                www = UnityWebRequest.Post(url, postData);
            }
            else
            {
                www = UnityWebRequest.Get(url);
            }

            www.SendWebRequest();

            while (!www.isDone || www.isNetworkError || www.isHttpError)
            {


                if (lastDownloadValue.Where(p => p == lastDownloadValue.Last()).Count() > 15)
                {
                    Debug.Log("FILE DOWNLOAD IS STALLED, RETRYING");
                    www.Abort();
                    StartCoroutine(CreateStandardWebRequest(url, postData, callback, reportProgress));
                    yield break;
                }
                else
                {
                    lastDownloadValue.Add(www.downloadedBytes);
                    callback.Invoke(www);
                    yield return new WaitForSecondsRealtime(0.1f);
                }


            }

            callback.Invoke(www);

        }

    }
}
#endif