using System;
using System.Collections.Generic;
using Fusion;
using GorillaExtensions;
using UnityEngine;

namespace GorillaGameModes
{
	public class GameMode : MonoBehaviour
	{
		private void Awake()
		{
			if (GameMode.instance.IsNull())
			{
				GameMode.instance = this;
				foreach (GorillaGameManager gorillaGameManager in base.gameObject.GetComponentsInChildren<GorillaGameManager>(true))
				{
					int num = (int)gorillaGameManager.GameType();
					string text = gorillaGameManager.GameTypeName();
					if (GameMode.gameModeTable.ContainsKey(num))
					{
						Debug.LogWarning("Duplicate gamemode type, skipping this instance", gorillaGameManager);
					}
					else
					{
						GameMode.gameModeTable.Add((int)gorillaGameManager.GameType(), gorillaGameManager);
						GameMode.gameModeKeyByName.Add(text, num);
						GameMode.gameModes.Add(gorillaGameManager);
						GameMode.gameModeNames.Add(text);
					}
				}
				return;
			}
			Object.Destroy(this);
		}

		private void OnDestroy()
		{
			if (GameMode.instance == this)
			{
				GameMode.instance = null;
			}
		}

		public static event GameMode.OnStartGameModeAction OnStartGameMode;

		public static GorillaGameManager ActiveGameMode
		{
			get
			{
				return GameMode.activeGameMode;
			}
		}

		internal static GameModeSerializer ActiveNetworkHandler
		{
			get
			{
				return GameMode.activeNetworkHandler;
			}
		}

		public static GameModeZoneMapping GameModeZoneMapping
		{
			get
			{
				return GameMode.instance.gameModeZoneMapping;
			}
		}

		public static GameModeType CurrentGameModeType { get; private set; } = GameModeType.None;

		public static event Action<List<NetPlayer>, List<NetPlayer>> ParticipatingPlayersChanged;

		static GameMode()
		{
			GameMode.StaticLoad();
		}

		[OnEnterPlay_Run]
		private static void StaticLoad()
		{
			RoomSystem.LeftRoomEvent += new Action(GameMode.ResetGameModes);
			RoomSystem.JoinedRoomEvent += new Action(GameMode.RefreshPlayers);
			RoomSystem.PlayersChangedEvent += new Action(GameMode.RefreshPlayers);
		}

		public static bool IsPlaying(GameModeType type)
		{
			return type == GameMode.CurrentGameModeType;
		}

		internal static bool LoadGameModeFromProperty()
		{
			return GameMode.LoadGameMode(GameMode.FindGameModeFromRoomProperty());
		}

		internal static bool ChangeGameFromProperty()
		{
			return GameMode.ChangeGameMode(GameMode.FindGameModeFromRoomProperty());
		}

		internal static bool LoadGameModeFromProperty(string prop)
		{
			return GameMode.LoadGameMode(GameMode.FindGameModeInString(prop));
		}

		internal static bool ChangeGameFromProperty(string prop)
		{
			return GameMode.ChangeGameMode(GameMode.FindGameModeInString(prop));
		}

		public static int GetGameModeKeyFromRoomProp()
		{
			string text = GameMode.FindGameModeFromRoomProperty();
			int num;
			if (string.IsNullOrEmpty(text) || !GameMode.gameModeKeyByName.TryGetValue(text, out num))
			{
				GTDev.LogWarning<string>("Unable to find game mode key for " + text, null);
				return -1;
			}
			return num;
		}

		private static string FindGameModeFromRoomProperty()
		{
			if (!NetworkSystem.Instance.InRoom || string.IsNullOrEmpty(NetworkSystem.Instance.GameModeString))
			{
				return null;
			}
			return GameMode.FindGameModeInString(NetworkSystem.Instance.GameModeString);
		}

		public static bool IsValidGameMode(string gameMode)
		{
			return !string.IsNullOrEmpty(gameMode) && GameMode.gameModeKeyByName.ContainsKey(gameMode);
		}

		private static string FindGameModeInString(string gmString)
		{
			for (int i = 0; i < GameMode.gameModes.Count; i++)
			{
				string text = GameMode.gameModes[i].GameTypeName();
				if (gmString.EndsWith(text))
				{
					return text;
				}
			}
			return null;
		}

		public static bool LoadGameMode(string gameMode)
		{
			if (gameMode == null)
			{
				Debug.LogError("GAME MODE NULL");
				return false;
			}
			int num;
			if (!GameMode.gameModeKeyByName.TryGetValue(gameMode, out num))
			{
				Debug.LogWarning("Unable to find game mode key for " + gameMode);
				return false;
			}
			return GameMode.LoadGameMode(num);
		}

		public static bool LoadGameMode(int key)
		{
			foreach (KeyValuePair<int, GorillaGameManager> keyValuePair in GameMode.gameModeTable)
			{
			}
			if (!GameMode.gameModeTable.ContainsKey(key))
			{
				Debug.LogWarning("Missing game mode for key " + key.ToString());
				return false;
			}
			PrefabType prefabType;
			VRRigCache.Instance.GetComponent<PhotonPrefabPool>().networkPrefabs.TryGetValue("GameMode", out prefabType);
			GameObject prefab = prefabType.prefab;
			if (prefab == null)
			{
				GTDev.LogError<string>("Unable to find game mode prefab to spawn", null);
				return false;
			}
			if (NetworkSystem.Instance.NetInstantiate(prefab, Vector3.zero, Quaternion.identity, true, 0, new object[] { key }, delegate(NetworkRunner runner, NetworkObject no)
			{
				no.GetComponent<GameModeSerializer>().Init(key);
			}).IsNull())
			{
				GTDev.LogWarning<string>("Unable to create GameManager with key " + key.ToString(), null);
				return false;
			}
			return true;
		}

		internal static bool ChangeGameMode(string gameMode)
		{
			if (gameMode == null)
			{
				return false;
			}
			int num;
			if (!GameMode.gameModeKeyByName.TryGetValue(gameMode, out num))
			{
				Debug.LogWarning("Unable to find game mode key for " + gameMode);
				return false;
			}
			return GameMode.ChangeGameMode(num);
		}

		internal static bool ChangeGameMode(int key)
		{
			GorillaGameManager gorillaGameManager;
			if (!NetworkSystem.Instance.IsMasterClient || !GameMode.gameModeTable.TryGetValue(key, out gorillaGameManager) || gorillaGameManager == GameMode.activeGameMode)
			{
				return false;
			}
			if (GameMode.activeNetworkHandler.IsNotNull())
			{
				NetworkSystem.Instance.NetDestroy(GameMode.activeNetworkHandler.gameObject);
			}
			GameMode.StopGameModeSafe(GameMode.activeGameMode);
			GameMode.activeGameMode = null;
			GameMode.activeNetworkHandler = null;
			return GameMode.LoadGameMode(key);
		}

		internal static void SetupGameModeRemote(GameModeSerializer networkSerializer)
		{
			GorillaGameManager gameModeInstance = networkSerializer.GameModeInstance;
			bool flag = gameModeInstance != GameMode.activeGameMode;
			if (GameMode.activeGameMode.IsNotNull() && gameModeInstance.IsNotNull() && flag)
			{
				GameMode.StopGameModeSafe(GameMode.activeGameMode);
			}
			GameMode.activeNetworkHandler = networkSerializer;
			GameMode.activeGameMode = gameModeInstance;
			GameMode.activeGameMode.NetworkLinkSetup(networkSerializer);
			GameMode.CurrentGameModeType = GameMode.activeGameMode.GameType();
			if (!GameMode.activatedGameModes.Contains(GameMode.activeGameMode))
			{
				GameMode.activatedGameModes.Add(GameMode.activeGameMode);
			}
			if (flag)
			{
				GameMode.StartGameModeSafe(GameMode.activeGameMode);
				if (GameMode.OnStartGameMode != null)
				{
					GameMode.OnStartGameMode(GameMode.activeGameMode.GameType());
				}
			}
		}

		internal static void RemoveNetworkLink(GameModeSerializer networkSerializer)
		{
			if (GameMode.activeGameMode.IsNotNull() && networkSerializer == GameMode.activeNetworkHandler)
			{
				GameMode.activeGameMode.NetworkLinkDestroyed(networkSerializer);
				GameMode.activeNetworkHandler = null;
				return;
			}
		}

		public static GorillaGameManager GetGameModeInstance(GameModeType type)
		{
			return GameMode.GetGameModeInstance((int)type);
		}

		public static GorillaGameManager GetGameModeInstance(int type)
		{
			GorillaGameManager gorillaGameManager;
			if (GameMode.gameModeTable.TryGetValue(type, out gorillaGameManager))
			{
				if (gorillaGameManager == null)
				{
					Debug.LogError("Couldnt get mode from table");
					foreach (KeyValuePair<int, GorillaGameManager> keyValuePair in GameMode.gameModeTable)
					{
					}
				}
				return gorillaGameManager;
			}
			return null;
		}

		public static T GetGameModeInstance<T>(GameModeType type) where T : GorillaGameManager
		{
			return GameMode.GetGameModeInstance<T>((int)type);
		}

		public static T GetGameModeInstance<T>(int type) where T : GorillaGameManager
		{
			T t = GameMode.GetGameModeInstance(type) as T;
			if (t != null)
			{
				return t;
			}
			return default(T);
		}

		public static void ResetGameModes()
		{
			GameMode.CurrentGameModeType = GameModeType.None;
			GameMode.activeGameMode = null;
			GameMode.activeNetworkHandler = null;
			GameMode.optOutPlayers.Clear();
			GameMode.ParticipatingPlayers.Clear();
			for (int i = 0; i < GameMode.activatedGameModes.Count; i++)
			{
				GorillaGameManager gorillaGameManager = GameMode.activatedGameModes[i];
				GameMode.StopGameModeSafe(gorillaGameManager);
				GameMode.ResetGameModeSafe(gorillaGameManager);
			}
			GameMode.activatedGameModes.Clear();
		}

		private static void StartGameModeSafe(GorillaGameManager gameMode)
		{
			try
			{
				gameMode.StartPlaying();
			}
			catch (Exception)
			{
			}
		}

		private static void StopGameModeSafe(GorillaGameManager gameMode)
		{
			try
			{
				gameMode.StopPlaying();
			}
			catch (Exception)
			{
			}
		}

		private static void ResetGameModeSafe(GorillaGameManager gameMode)
		{
			try
			{
				gameMode.ResetGame();
			}
			catch (Exception)
			{
			}
		}

		public static void ReportTag(NetPlayer player)
		{
			if (NetworkSystem.Instance.InRoom && GameMode.activeNetworkHandler.IsNotNull())
			{
				GameMode.activeNetworkHandler.SendRPC("RPC_ReportTag", false, new object[] { player.ActorNumber });
			}
		}

		public static void ReportHit()
		{
			if (GorillaGameManager.instance.GameType() == GameModeType.Custom)
			{
				CustomGameMode.TaggedByEnvironment();
			}
			if (NetworkSystem.Instance.InRoom && GameMode.activeNetworkHandler.IsNotNull())
			{
				GameMode.activeNetworkHandler.SendRPC("RPC_ReportHit", false, Array.Empty<object>());
			}
		}

		public static bool LocalIsTagged(NetPlayer player)
		{
			return !GameMode.ActiveGameMode.IsNull() && GameMode.ActiveGameMode.LocalIsTagged(player);
		}

		public static void BroadcastRoundComplete()
		{
			if (NetworkSystem.Instance.IsMasterClient && NetworkSystem.Instance.InRoom && GameMode.activeNetworkHandler.IsNotNull())
			{
				GameMode.activeNetworkHandler.SendRPC("RPC_BroadcastRoundComplete", true, Array.Empty<object>());
			}
		}

		public static void BroadcastTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
		{
			if (NetworkSystem.Instance.IsMasterClient && NetworkSystem.Instance.InRoom && GameMode.activeNetworkHandler.IsNotNull())
			{
				GameMode.activeNetworkHandler.SendRPC("RPC_BroadcastTag", true, new object[] { taggedPlayer.ActorNumber, taggingPlayer.ActorNumber });
			}
		}

		public static List<NetPlayer> ParticipatingPlayers
		{
			get
			{
				return GameMode._participatingPlayers;
			}
		}

		public static void RefreshPlayers()
		{
			GameMode._oldPlayersCount = GameMode._participatingPlayers.Count;
			for (int i = 0; i < GameMode._oldPlayersCount; i++)
			{
				GameMode._oldPlayersBuffer[i] = GameMode._participatingPlayers[i];
			}
			GameMode._participatingPlayers.Clear();
			List<NetPlayer> playersInRoom = RoomSystem.PlayersInRoom;
			int num = Mathf.Min(playersInRoom.Count, 10);
			for (int j = 0; j < num; j++)
			{
				if (GameMode.CanParticipate(playersInRoom[j]))
				{
					GameMode.ParticipatingPlayers.Add(playersInRoom[j]);
				}
			}
			GameMode._tempRemovedPlayers.Clear();
			for (int k = 0; k < GameMode._oldPlayersCount; k++)
			{
				NetPlayer netPlayer = GameMode._oldPlayersBuffer[k];
				if (!GameMode.ContainsNetPlayer(GameMode._participatingPlayers, netPlayer))
				{
					GameMode._tempRemovedPlayers.Add(netPlayer);
				}
			}
			GameMode._tempAddedPlayers.Clear();
			int count = GameMode._participatingPlayers.Count;
			for (int l = 0; l < count; l++)
			{
				NetPlayer netPlayer2 = GameMode._participatingPlayers[l];
				if (!GameMode.ContainsNetPlayer(GameMode._oldPlayersBuffer, netPlayer2, GameMode._oldPlayersCount))
				{
					GameMode._tempAddedPlayers.Add(netPlayer2);
				}
			}
			if ((GameMode._tempAddedPlayers.Count > 0 || GameMode._tempRemovedPlayers.Count > 0) && GameMode.ParticipatingPlayersChanged != null)
			{
				GameMode.ParticipatingPlayersChanged(GameMode._tempAddedPlayers, GameMode._tempRemovedPlayers);
			}
		}

		private static bool ContainsNetPlayer(List<NetPlayer> list, NetPlayer candidate)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				if (list[i] == candidate)
				{
					return true;
				}
			}
			return false;
		}

		private static bool ContainsNetPlayer(NetPlayer[] array, NetPlayer candidate, int length)
		{
			for (int i = 0; i < length; i++)
			{
				if (array[i] == candidate)
				{
					return true;
				}
			}
			return false;
		}

		public static void OptOut(VRRig rig)
		{
			GameMode.OptOut(rig.creator.ActorNumber);
		}

		public static void OptOut(NetPlayer player)
		{
			GameMode.OptOut(player.ActorNumber);
		}

		public static void OptOut(int playerActorNumber)
		{
			if (GameMode.optOutPlayers.Add(playerActorNumber))
			{
				GameMode.RefreshPlayers();
			}
		}

		public static void OptIn(VRRig rig)
		{
			GameMode.OptIn(rig.creator.ActorNumber);
		}

		public static void OptIn(NetPlayer player)
		{
			GameMode.OptIn(player.ActorNumber);
		}

		public static void OptIn(int playerActorNumber)
		{
			if (GameMode.optOutPlayers.Remove(playerActorNumber))
			{
				GameMode.RefreshPlayers();
			}
		}

		private static bool CanParticipate(NetPlayer player)
		{
			return player.InRoom() && !GameMode.optOutPlayers.Contains(player.ActorNumber) && NetworkSystem.Instance.GetPlayerTutorialCompletion(player.ActorNumber) && (!(GorillaGameManager.instance != null) || GorillaGameManager.instance.CanPlayerParticipate(player));
		}

		[SerializeField]
		private GameModeZoneMapping gameModeZoneMapping;

		[OnEnterPlay_SetNull]
		private static GameMode instance;

		[OnEnterPlay_Clear]
		private static Dictionary<int, GorillaGameManager> gameModeTable = new Dictionary<int, GorillaGameManager>();

		[OnEnterPlay_Clear]
		private static Dictionary<string, int> gameModeKeyByName = new Dictionary<string, int>();

		[OnEnterPlay_Clear]
		private static Dictionary<int, FusionGameModeData> fusionTypeTable = new Dictionary<int, FusionGameModeData>();

		[OnEnterPlay_Clear]
		private static List<GorillaGameManager> gameModes = new List<GorillaGameManager>(10);

		[OnEnterPlay_Clear]
		public static readonly List<string> gameModeNames = new List<string>(10);

		[OnEnterPlay_Clear]
		private static readonly List<GorillaGameManager> activatedGameModes = new List<GorillaGameManager>(12);

		[OnEnterPlay_SetNull]
		private static GorillaGameManager activeGameMode = null;

		[OnEnterPlay_SetNull]
		private static GameModeSerializer activeNetworkHandler = null;

		[OnEnterPlay_Clear]
		private static readonly HashSet<int> optOutPlayers = new HashSet<int>(10);

		[OnEnterPlay_Clear]
		private static readonly List<NetPlayer> _participatingPlayers = new List<NetPlayer>(10);

		private static readonly NetPlayer[] _oldPlayersBuffer = new NetPlayer[10];

		private static int _oldPlayersCount;

		private static readonly List<NetPlayer> _tempAddedPlayers = new List<NetPlayer>(10);

		private static readonly List<NetPlayer> _tempRemovedPlayers = new List<NetPlayer>(10);

		public delegate void OnStartGameModeAction(GameModeType newGameModeType);
	}
}
