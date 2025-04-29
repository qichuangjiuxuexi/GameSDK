using System;
using System.Collections;
using System.Net;
using AppBase;
using AppBase.CommonDeath;
using AppBase.CommonDeath.Timing;
using AppBase.Module;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AppBase.NetworkDeath
{
    public class NetworkManager : ModuleBase
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        private string url;
        /// <summary>
        /// 服务器地址
        /// </summary>
        private string ServerUrl
        {
            get
            {
                return "http://127.0.0.1:12138/";
            }
        }
        
        protected override void OnInit()
        {
            base.OnInit();
            url = ServerUrl;
        }

        /// <summary>
        /// 发送网络请求
        /// </summary>
        /// <param name="request">请求协议</param>
        /// <param name="callback">结果回调</param>
        public void Send<T>(T request, Action<bool, T> callback) where T : NetworkProtocol
            => Send((NetworkProtocol)request, (e, r) => callback?.Invoke(e, (T)r));

        /// <summary>
        /// 发送网络请求
        /// </summary>
        /// <param name="request">请求协议</param>
        public UniTask<T> Send<T>(T request) where T : NetworkProtocol
        {
            var tcs = new UniTaskCompletionSource<T>();
            Send(request, (e, r) => tcs.TrySetResult(r));
            return tcs.Task;
        }
        
         /// <summary>
        /// 发送网络请求
        /// </summary>
        /// <param name="request">请求协议</param>
        /// <param name="callback">结果回调</param>
        public void Send(NetworkProtocol request, Action<bool, NetworkProtocol> callback)
        {
            //检查请求合法性
            if (request == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(url))
            {
                return;
            }
            if (string.IsNullOrEmpty(request.service) || string.IsNullOrEmpty(request.action) || string.IsNullOrEmpty(request.contentType))
            {
                return;
            }
            //检查登录状态
            request.OnBeforeSend(loginSuccess =>
            {
                //检查登录状态
                if (!loginSuccess)
                {
                    return;
                }
                //生成请求
                if (!request.OnSend())
                {
                    return;
                }
                //准备请求数据
                var requestBytes = request.requestBytes;
                if (requestBytes == null)
                {
                    return;
                }
                //发送请求
                GameBase.Instance.GetModule<TimingManager>().StartCoroutine(HandleSend(requestBytes, request, callback));
            });
        }

        /// <summary>
        /// 发送请求数据
        /// </summary>
        private IEnumerator HandleSend(byte[] requestBytes, NetworkProtocol request,
            Action<bool, NetworkProtocol> callback)
        {
            UnityWebRequest webRequest = new UnityWebRequest($"{url}/{request.service}/{request.action}",  "POST");
            webRequest.SetRequestHeader("Content-Type", request.contentType);
            webRequest.SetRequestHeader("Device-ID", AppUtil.DeviceId);
            var reqId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            webRequest.SetRequestHeader("Req-ID", reqId);
            //发送请求
            webRequest.timeout = request.timeout;
            webRequest.uploadHandler = new UploadHandlerRaw(requestBytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
    
            // 请求完成后处理
            if (webRequest.result == UnityWebRequest.Result.Success) {
                Debug.Log("请求成功: " + webRequest.downloadHandler.text);
                HandleResponse(webRequest.downloadHandler.data, request, callback);
            } else {
                string errorMsg = webRequest.downloadHandler != null ?webRequest.downloadHandler.error : "No response";
                Debug.LogError($"请求失败: {webRequest.result} - {errorMsg}");
                callback.Invoke(false, request);
            }
            
        }
        
        /// <summary>
        /// 处理请求成功
        /// </summary>
        private void HandleResponse(byte[] responseBytes, NetworkProtocol request, Action<bool, NetworkProtocol> callback)
        {
            try
            {
                request.responseBytes = responseBytes;
            }
            catch (Exception e)
            {
                return;
            }
            if (request.OnInternalResponse() && request.OnResponse())
            {
                callback?.Invoke(true, request);
            }
            else
            {
                callback?.Invoke(false, request);
            }
        }
    }
}