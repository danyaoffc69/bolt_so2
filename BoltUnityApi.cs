using System;
using System.Collections.Concurrent;
using Axlebolt.Bolt.Player;
using Axlebolt.RpcSupport;
using UnityEngine;

namespace Axlebolt.Bolt.Unity
{
    public class BoltUnityApi
    {
        private class BoltUnityBridge : MonoBehaviour, ILogStream
        {
            private readonly ILogger _logger = UnityEngine.Debug.unityLogger;

            private const string Tag = "Bolt";

            private ConcurrentQueue<Exception> _exceptions = new ConcurrentQueue<Exception>();

            private void Awake()
            {
                Axlebolt.RpcSupport.Logger.Stream = this;
                Axlebolt.RpcSupport.Logger.LogDebug = false;
                UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
                InitArgs();
            }

            private static void InitArgs()
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaObject androidJavaObject = @static.Call<AndroidJavaObject>("getIntent", new object[0]);
                    if (androidJavaObject.Call<bool>("hasExtra", new object[1] { "bolt" }))
                    {
                        AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getExtras", new object[0]);
                        string text = androidJavaObject2.Call<string>("getString", new object[1] { "bolt" });
                        _boltNotification = (string.IsNullOrEmpty(text) ? null : JsonUtility.FromJson<BoltNotification>(text));
                    }
                }
            }

            public void Debug(object msg)
            {
                _logger.Log("Bolt", msg);
            }

            public void Error(object msg)
            {
                _logger.LogError("Bolt", msg);
            }

            public void Exception(Exception ex)
            {
                _exceptions.Enqueue(ex);
            }

            private void OnApplicationPause(bool pauseStatus)
            {
                if (!BoltPlayerApi.Instance.IsConnectedAndReady)
                {
                    return;
                }
                try
                {
                    if (pauseStatus)
                    {
                        _logger.Log("SetAwayStatus");
                        BoltService<BoltPlayerService>.Instance.SetAwayStatus().Wait();
                    }
                    else
                    {
                        _logger.Log("SetOnlineStatus");
                        BoltService<BoltPlayerService>.Instance.SetOnlineStatus().Wait();
                    }
                }
                catch (ConnectionFailedException message)
                {
                    _logger.Log("Bolt", message);
                }
                catch (Exception message2)
                {
                    _logger.LogError("Bolt", message2);
                }
            }

            private void Update()
            {
                if (_exceptions.TryDequeue(out var result))
                {
                    throw result;
                }
            }

            private void OnDestroy()
            {
                BoltPlayerApi.Destroy();
            }
        }

        private static BoltNotification _boltNotification;

        private static readonly string[] DomainNames = new string[6] { "ams01.boltgaming.io", "ams02.boltgaming.io", "ams03.boltgaming.io", "ams04.boltgaming.io", "ams05.boltgaming.io", "ams06.boltgaming.io" };

        private static readonly string[] Ips = new string[6] { "45.32.235.217", "209.250.248.185", "209.250.242.154", "45.32.233.204", "45.76.43.92", "95.179.139.110" };

        public static void Init()
        {
            GameObject gameObject = new GameObject("BoltUnityApi");
            gameObject.AddComponent<BoltUnityBridge>();
            BoltPlayerApi.TempPath = Application.temporaryCachePath;
            BoltPlayerApi.Init(BoltSettings.Instance.GameId, Application.version, GetPlatform());
            BoltPlayerApi.Instance.Dns = "staging.boltgaming.io";
            BoltPlayerApi.Instance.Ip = "staging.boltgaming.io";
        }

        private static string GetRandomDomain()
        {
            return DomainNames[UnityEngine.Random.Range(0, DomainNames.Length)];
        }

        private static string GetRandomIp()
        {
            return Ips[UnityEngine.Random.Range(0, Ips.Length)];
        }

        private static Platform GetPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return Platform.Android;
                case RuntimePlatform.IPhonePlayer:
                    return Platform.Ios;
                default:
                    return Platform.Unknown;
            }
        }

        public static bool IsLaunchedViaNotification()
        {
            return _boltNotification != null;
        }

        public static BoltNotification GetNotification()
        {
            BoltNotification boltNotification = _boltNotification;
            _boltNotification = null;
            return boltNotification;
        }
    }
}
