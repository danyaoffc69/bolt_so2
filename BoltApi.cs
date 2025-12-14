using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Axlebolt.RpcSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Axlebolt.Bolt
{
    public abstract class BoltApi
    {
        private bool _handshakeSuccess;

        protected readonly List<Service> Services = new List<Service>();

        protected readonly HashSet<Action> Jobs = new HashSet<Action>();

        private Thread _jobThread;

        private bool _jobThreadAlive;

        public string Ip { get; set; } = "46.101.200.35";


        public string Dns { get; set; } = "fra02.bolt.bolt-api.com";


        public int JobIntervalMilliseconds { get; set; } = 200;


        public bool IsConnectedAndReady => ClientService.ConnectionState.Equals(ConnectionState.Connected) && _handshakeSuccess;

        public ClientService ClientService { get; }

        public string GameId { get; }

        protected BoltApi(string gameId)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException("gameId");
            }
            GameId = gameId;
            ClientService = new ClientService(Assembly.GetExecutingAssembly());
            ClientService.ConnectionFailed += OnConnectionFailedEvent;
            ClientService.RequestTimeout = 10000L;
        }

        public void AddJob(Action jobAction)
        {
            Jobs.Add(jobAction);
        }

        protected void ConnectAndAuthorize()
        {
            Connect();
            Auth();
            Handshake();
            OnHandshakeSuccess();
        }

        public string GetServer(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Connect()
        {
            //var jobj = (JObject)JsonConvert.DeserializeObject(GetServer("https://pastebin.com/raw/TzY0XyDS"));
            //var jobj = (JObject)JsonConvert.DeserializeObject(GetServer("https://pastebin.com/raw/GWVHamn7")); // base
            var jobj = (JObject)JsonConvert.DeserializeObject(GetServer("https://pastebin.com/raw/T4m5YPyQ"));
            string _ip = jobj["ip"].Value<string>();
            int _port = jobj["port"].Value<int>();
            try
            {
                ClientService.Connect("26.78.39.143", 2222);
            }
            catch (ConnectionFailedException)
            {
                ClientService.Connect("26.78.39.143", 2222);
            }
            if (Logger.LogDebug)
            {
                Logger.Debug("Connected successfully!");
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected abstract void Auth();

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected abstract void Handshake();

        public Task ReconnectAsync()
        {
            return Async(Reconnect);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void Reconnect()
        {
            Connect();
            Handshake();
            OnHandshakeSuccess();
        }

        protected void OnHandshakeSuccess()
        {
            LoadServices();
            _handshakeSuccess = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual void Disconnect()
        {
            ClientService.Disconnect();
            UnloadService();
            _handshakeSuccess = false;
            if (Logger.LogDebug)
            {
                Logger.Debug("Logout successfully!");
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DisconnectImmediate()
        {
            ClientService.Disconnect();
            OnConnectionFailedEvent();
            if (Logger.LogDebug)
            {
                Logger.Debug("DisconnectImmediate successfully!");
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void OnConnectionFailedEvent()
        {
            if (Logger.LogDebug)
            {
                Logger.Debug("OnConnectionFailedEvent");
            }
            if (Logger.LogDebug)
            {
                Logger.Debug("Unloading services");
            }
            UnloadService();
            if (_handshakeSuccess)
            {
                _handshakeSuccess = false;
            }
        }

        private void LoadServices()
        {
            try
            {
                ClientService.ProcessEvents = false;
                Load();
                PostLoad();
                BindEvents();
                StartJobThread();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                throw;
            }
            finally
            {
                ClientService.ProcessEvents = true;
            }
        }

        private void UnloadService()
        {
            try
            {
                ClientService.ProcessEvents = false;
                UnbindEvents();
                Unload();
                StopJobThread();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                throw;
            }
            finally
            {
                ClientService.ProcessEvents = true;
            }
        }

        private void BindEvents()
        {
            foreach (Service service in Services)
            {
                service.BindEvents();
            }
        }

        private void Unload()
        {
            foreach (Service service in Services)
            {
                service.Unload();
            }
        }

        private void StartJobThread()
        {
            if (_jobThread != null)
            {
                throw new Exception("Job already running");
            }
            _jobThreadAlive = true;
            _jobThread = new Thread((ThreadStart)delegate
            {
                while (_jobThreadAlive)
                {
                    Thread.Sleep(JobIntervalMilliseconds);
                    foreach (Action job in Jobs)
                    {
                        try
                        {
                            job();
                        }
                        catch (Exception ex)
                        {
                            Logger.Exception(ex);
                        }
                    }
                }
            });
            _jobThread.Start();
        }

        private void StopJobThread()
        {
            if (_jobThread != null)
            {
                _jobThreadAlive = false;
                _jobThread.Join();
                _jobThread = null;
            }
        }

        private void Load()
        {
            Task[] tasks = Services.Select((Service service) => Async(delegate
            {
                try
                {
                    service.Load();
                }
                catch (Exception msg)
                {
                    Logger.Error(msg);
                    throw;
                }
            })).ToArray();
            Task.WaitAll(tasks);
        }

        private void PostLoad()
        {
            foreach (Service service in Services)
            {
                service.PostLoad();
            }
        }

        private void UnbindEvents()
        {
            foreach (Service service in Services)
            {
                service.UnbindEvents();
            }
        }

        internal Task Async(Action action)
        {
            return Task.Factory.StartNew(action);
        }

        internal Task<T> Async<T>(Func<T> action)
        {
            return Task.Factory.StartNew(action);
        }

        internal void AddEventListener(object eventListener)
        {
            ClientService.AddEventListener(eventListener);
        }

        internal void RemoveEventListener(object eventListener)
        {
            ClientService.RemoveEventListener(eventListener);
        }
    }
}
