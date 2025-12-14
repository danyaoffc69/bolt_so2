using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Axlebolt.Bolt.Analytics;
using Axlebolt.Bolt.Api;
using Axlebolt.Bolt.Auth;
using Axlebolt.Bolt.Clans;
using Axlebolt.Bolt.Friends;
using Axlebolt.Bolt.GameEvents;
using Axlebolt.Bolt.Inventory;
using Axlebolt.Bolt.Marketplace;
using Axlebolt.Bolt.Matchmaking;
using Axlebolt.Bolt.Chat;
using Axlebolt.Bolt.Player;
using Axlebolt.Bolt.Protobuf;
using Axlebolt.Bolt.Settings;
using Axlebolt.Bolt.Stats;
using Axlebolt.Bolt.Storage;
using Axlebolt.Bolt.Avatar;

namespace Axlebolt.Bolt
{
    public class BoltPlayerApi : BoltApi
    {
        private static BoltPlayerApi _instance;

        public readonly BoltEvent ConnectionFailedEvent = new BoltEvent();

        private readonly GoogleAuthRemoteService _googleAuthRemoteService;

        private readonly GameCenterAuthRemoteService _gameCenterAuthRemoteService;

        private readonly TestAuthRemoteService _testAuthRemoteService;

        private readonly FacebookAuthRemoteService _facebookAuthRemoteService;

        private readonly HandshakeRemoteService _handshakeRemoteService;

        private readonly BoltRemoteService _boltRemoteService;

        private ICredential _credential;

        private Verification _verification;

        private string _ticket;

        private SynchronizationContext _synchronizationContext;

        private readonly string _gameVersion;

        private readonly Platform _platform;

        public static string TempPath { get; set; } = Path.GetTempPath();


        public bool SyncContextEnabled { get; set; } = true;


        public static BoltPlayerApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new BoltException(string.Format("{0} is not initialized", "BoltPlayerApi"));
                }
                return _instance;
            }
        }

        public static bool IsInitialized => _instance != null;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_ticket);

        public static void Init(string gameId, string gameVersion, Platform platform)
        {
            if (_instance != null)
            {
                throw new BoltException(string.Format("{0} already initialized", "BoltPlayerApi"));
            }
            _instance = new BoltPlayerApi(gameId, gameVersion, platform);
            _instance.Services.Add(new BoltAvatarService(TempPath));
            _instance.Services.Add(new BoltPlayerService());
            _instance.Services.Add(new BoltPlayerStatsService());
            _instance.Services.Add(new BoltFriendsService());
            _instance.Services.Add(new BoltMatchmakingService());
            _instance.Services.Add(new BoltChatService());
            _instance.Services.Add(new BoltInventoryService());
            _instance.Services.Add(new BoltGameEventService());
            _instance.Services.Add(new BoltStorageService());
            _instance.Services.Add(new BoltSettingsService());
            _instance.Services.Add(new BoltAnalytics());
            _instance.Services.Add(new BoltMarketplaceService());
            _instance.Services.Add(new BoltClanService());
        }

        public static void Destroy()
        {
            if (_instance == null)
            {
                return;
            }
            _instance.ClientService.Disconnect();
            _instance.Jobs.Clear();
            foreach (Service service in _instance.Services)
            {
                service.Destroy();
            }
            _instance.Services.Clear();
            _instance = null;
        }

        private BoltPlayerApi(string gameId, string gameVersion, Platform platform)
            : base(gameId)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException("gameId");
            }
            _gameVersion = gameVersion;
            _platform = platform;
            _googleAuthRemoteService = new GoogleAuthRemoteService(base.ClientService);
            _gameCenterAuthRemoteService = new GameCenterAuthRemoteService(base.ClientService);
            _facebookAuthRemoteService = new FacebookAuthRemoteService(base.ClientService);
            _testAuthRemoteService = new TestAuthRemoteService(base.ClientService);
            _handshakeRemoteService = new HandshakeRemoteService(base.ClientService);
            _boltRemoteService = new BoltRemoteService(base.ClientService);
        }

        public Task SignInAsync(ICredential credential, Verification verification = null)
        {
            if (credential == null)
            {
                throw new ArgumentNullException("credential");
            }
            _synchronizationContext = SynchronizationContext.Current;
            return Async(delegate
            {
                SignIn(credential, verification);
            });
        }

        public void SignIn(ICredential credential, Verification verification = null)
        {
            _credential = credential;
            _verification = verification;
            ConnectAndAuthorize();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void Auth()
        {
            if (!IsAuthenticated)
            {
                AppVerification appVerification = null;
                if (_verification != null)
                {
                    appVerification = new AppVerification
                    {
                        ApkHash = _verification.ApkHash,
                        IsRooted = _verification.IsRooted
                    };
                    appVerification.JsonForbiddenApps.AddRange(_verification.JsonForbiddenApps);
                }
                if (_credential is GoogleCredential googleCredential)
                {
                    string serverAuthCode = googleCredential.ServerAuthCode;
                    AuthGoogle auth = new AuthGoogle
                    {
                        GameId = base.GameId,
                        GameVersion = _gameVersion,
                        Platform = EnumMapper<Axlebolt.Bolt.Protobuf.Platform, Platform>.ToProto(_platform),
                        AuthCode = serverAuthCode
                    };
                    _ticket = _googleAuthRemoteService.Auth(auth, appVerification);
                    UnityEngine.Debug.Log("Ticket: " + _ticket);
                }
                else if (_credential is GameCenterCredential gameCenterCredential)
                {
                    string playerId = gameCenterCredential.PlayerId;
                    string bundleId = gameCenterCredential.BundleId;
                    string publicKeyUrl = gameCenterCredential.PublicKeyUrl;
                    byte[] signature = gameCenterCredential.Signature;
                    long timestamp = gameCenterCredential.Timestamp;
                    byte[] salt = gameCenterCredential.Salt;
                    string defaultName = gameCenterCredential.DefaultName;
                    _ticket = _gameCenterAuthRemoteService.Auth(base.GameId, _gameVersion, EnumMapper<Axlebolt.Bolt.Protobuf.Platform, Platform>.ToProto(_platform), playerId, bundleId, publicKeyUrl, signature, salt, timestamp, defaultName);
                }
                else if (_credential is FacebookCredential facebookCredential)
                {
                    AuthFacebook auth2 = new AuthFacebook
                    {
                        GameId = base.GameId,
                        GameVersion = _gameVersion,
                        Platform = EnumMapper<Axlebolt.Bolt.Protobuf.Platform, Platform>.ToProto(_platform),
                        Token = facebookCredential.Token
                    };
                    _ticket = _facebookAuthRemoteService.Auth(auth2, appVerification);
                }
                else if (_credential is TokenCredential tokenCredential)
                {
                    string token = tokenCredential.Token;
                    _ticket = _testAuthRemoteService.Auth(base.GameId, _gameVersion, EnumMapper<Axlebolt.Bolt.Protobuf.Platform, Platform>.ToProto(_platform), token);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void Handshake()
        {
            Handshake handshake = new Handshake
            {
                Ticket = _ticket
            };
            _handshakeRemoteService.Handshake(handshake);
        }

        public Task LogoutAsync()
        {
            return Async(Disconnect);
        }

        public override void Disconnect()
        {
            _ticket = null;
            base.Disconnect();
        }

        protected override void OnConnectionFailedEvent()
        {
            base.OnConnectionFailedEvent();
            ConnectionFailedEvent?.Invoke();
        }

        internal void ExecuteInMainThread(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (SyncContextEnabled)
            {
                _synchronizationContext.Post(delegate
                {
                    action();
                }, null);
            }
            else
            {
                action();
            }
        }

        internal Task Subscribe(string topic)
        {
            return Async(delegate
            {
                _boltRemoteService.Subscribe(topic);
            });
        }

        internal Task Unsubscribe(string topic)
        {
            return Async(delegate
            {
                _boltRemoteService.Unsubscribe(topic);
            });
        }

        public static long GetTimeInMilliseconds()
        {
            return DateTime.Now.Ticks / 10000;
        }
    }
}
