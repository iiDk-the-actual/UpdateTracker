using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CosmeticRoom;
using CustomMapSupport;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion.Swimming;
using GorillaNetworking;
using GorillaNetworking.Store;
using GorillaTag.Rendering;
using GorillaTagScripts;
using GorillaTagScripts.CustomMapSupport;
using GorillaTagScripts.VirtualStumpCustomMaps;
using GT_CustomMapSupportRuntime;
using Modio;
using Modio.Mods;
using Newtonsoft.Json;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;

public class CustomMapLoader : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitOnLoad()
	{
		GTDev.Log<string>("CML::InitOnLoad", null);
		CustomMapLoader.instance = null;
		CustomMapLoader.hasInstance = false;
		CustomMapLoader.isLoading = false;
		CustomMapLoader.isUnloading = false;
		CustomMapLoader.runningAsyncLoad = false;
		CustomMapLoader.attemptedLoadID = 0L;
		CustomMapLoader.attemptedSceneToLoad = null;
		CustomMapLoader.shouldAbortMapLoading = false;
		CustomMapLoader.shouldAbortSceneLoad = false;
		CustomMapLoader.errorEncounteredDuringLoad = false;
		CustomMapLoader.unloadMapCallback = null;
		CustomMapLoader.cachedExceptionMessage = "";
		CustomMapLoader.mapBundle = null;
		CustomMapLoader.initialSceneNames = new List<string>();
		CustomMapLoader.initialSceneIndexes = new List<int>();
		CustomMapLoader.maxPlayersForMap = 10;
		CustomMapLoader.loadedMapModId = ModId.Null;
		CustomMapLoader.loadedMapModFileId = -1L;
		CustomMapLoader.loadedMapPackageInfo = null;
		CustomMapLoader.cachedLuauScript = null;
		CustomMapLoader.devModeEnabled = false;
		CustomMapLoader.disableHoldingHandsAllModes = false;
		CustomMapLoader.disableHoldingHandsCustomMode = false;
		CustomMapLoader.mapLoadProgressCallback = null;
		CustomMapLoader.mapLoadFinishedCallback = null;
		CustomMapLoader.zoneLoadingCoroutine = null;
		CustomMapLoader.sceneLoadedCallback = null;
		CustomMapLoader.sceneUnloadedCallback = null;
		CustomMapLoader.queuedLoadZoneRequests = new List<CustomMapLoader.LoadZoneRequest>();
		CustomMapLoader.assetBundleSceneFilePaths = null;
		CustomMapLoader.loadedSceneFilePaths = new List<string>();
		CustomMapLoader.loadedSceneNames = new List<string>();
		CustomMapLoader.loadedSceneIndexes = new List<int>();
		CustomMapLoader.leafGliderIndex = 0;
		CustomMapLoader.usingDynamicLighting = false;
		CustomMapLoader.totalObjectsInLoadingScene = 0;
		CustomMapLoader.objectsProcessedForLoadingScene = 0;
		CustomMapLoader.objectsProcessedThisFrame = 0;
		CustomMapLoader.initializePhaseTwoComponents = new List<Component>();
		CustomMapLoader.entitiesToCreate = new List<MapEntity>(Constants.aiAgentLimit);
		CustomMapLoader.lightmaps = null;
		CustomMapLoader.lightmapsToKeep = new List<Texture2D>();
		CustomMapLoader.placeholderReplacements = new List<GameObject>();
		CustomMapLoader.customMapATM = null;
		CustomMapLoader.storeCheckouts = new List<GameObject>();
		CustomMapLoader.storeDisplayStands = new List<GameObject>();
		CustomMapLoader.storeTryOnConsoles = new List<GameObject>();
		CustomMapLoader.storeTryOnAreas = new List<GameObject>();
	}

	private void Awake()
	{
		if (CustomMapLoader.instance == null)
		{
			CustomMapLoader.instance = this;
			CustomMapLoader.hasInstance = true;
			return;
		}
		if (CustomMapLoader.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		byte[] array = new byte[]
		{
			Convert.ToByte(68),
			Convert.ToByte(111),
			Convert.ToByte(110),
			Convert.ToByte(116),
			Convert.ToByte(68),
			Convert.ToByte(101),
			Convert.ToByte(115),
			Convert.ToByte(116),
			Convert.ToByte(114),
			Convert.ToByte(111),
			Convert.ToByte(121),
			Convert.ToByte(79),
			Convert.ToByte(110),
			Convert.ToByte(76),
			Convert.ToByte(111),
			Convert.ToByte(97),
			Convert.ToByte(100)
		};
		this.dontDestroyOnLoadSceneName = Encoding.ASCII.GetString(array);
		if (this.publicJoinTrigger != null)
		{
			this.publicJoinTrigger.SetActive(false);
		}
	}

	public static void Initialize(Action<MapLoadStatus, int, string> onLoadProgress, Action<bool> onLoadFinished, Action<string> onSceneLoaded, Action<string> onSceneUnloaded)
	{
		CustomMapLoader.mapLoadProgressCallback = onLoadProgress;
		CustomMapLoader.mapLoadFinishedCallback = onLoadFinished;
		CustomMapLoader.sceneLoadedCallback = onSceneLoaded;
		CustomMapLoader.sceneUnloadedCallback = onSceneUnloaded;
	}

	public static void LoadMap(long mapModId, string mapFilePath)
	{
		if (!CustomMapLoader.hasInstance)
		{
			return;
		}
		if (CustomMapLoader.isLoading)
		{
			return;
		}
		if (CustomMapLoader.isUnloading)
		{
			Action<bool> action = CustomMapLoader.mapLoadFinishedCallback;
			if (action == null)
			{
				return;
			}
			action(false);
			return;
		}
		else
		{
			if (!CustomMapLoader.IsMapLoaded(mapModId))
			{
				GorillaNetworkJoinTrigger.DisableTriggerJoins();
				CustomMapLoader.CanLoadEntities = false;
				CustomMapLoader.instance.StartCoroutine(CustomMapLoader.LoadAssetBundle(mapModId, mapFilePath, new Action<bool, bool>(CustomMapLoader.OnAssetBundleLoaded)));
				return;
			}
			Action<bool> action2 = CustomMapLoader.mapLoadFinishedCallback;
			if (action2 == null)
			{
				return;
			}
			action2(true);
			return;
		}
	}

	public static bool OpenDoorToMap()
	{
		if (!CustomMapLoader.hasInstance)
		{
			return false;
		}
		if (CustomMapLoader.instance.accessDoor != null)
		{
			CustomMapLoader.instance.accessDoor.OpenDoor();
			return true;
		}
		return false;
	}

	private static IEnumerator LoadAssetBundle(long mapModID, string packageInfoFilePath, Action<bool, bool> OnLoadComplete)
	{
		CustomMapLoader.isLoading = true;
		CustomMapLoader.errorEncounteredDuringLoad = false;
		CustomMapLoader.attemptedLoadID = mapModID;
		CustomMapLoader.refreshReviveStations = false;
		CustomMapLoader.instance.ghostReactorManager.reactor.RefreshReviveStations(false);
		Action<MapLoadStatus, int, string> action = CustomMapLoader.mapLoadProgressCallback;
		if (action != null)
		{
			action(MapLoadStatus.Loading, 1, "CACHING LIGHTMAP DATA");
		}
		CustomMapLoader.CacheLightmaps();
		Action<MapLoadStatus, int, string> action2 = CustomMapLoader.mapLoadProgressCallback;
		if (action2 != null)
		{
			action2(MapLoadStatus.Loading, 2, "LOADING PACKAGE INFO");
		}
		try
		{
			CustomMapLoader.loadedMapPackageInfo = CustomMapLoader.GetPackageInfo(packageInfoFilePath);
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("[CML.LoadAssetBundle] GetPackageInfo Exception: {0}", ex));
			Action<MapLoadStatus, int, string> action3 = CustomMapLoader.mapLoadProgressCallback;
			if (action3 != null)
			{
				action3(MapLoadStatus.Error, 0, ex.ToString());
			}
			OnLoadComplete(false, false);
			yield break;
		}
		if (CustomMapLoader.loadedMapPackageInfo == null)
		{
			Action<MapLoadStatus, int, string> action4 = CustomMapLoader.mapLoadProgressCallback;
			if (action4 != null)
			{
				action4(MapLoadStatus.Error, 0, "FAILED TO READ FILE AT " + packageInfoFilePath);
			}
			OnLoadComplete(false, false);
			yield break;
		}
		CustomMapLoader.LoadInitialSceneNames();
		Action<MapLoadStatus, int, string> action5 = CustomMapLoader.mapLoadProgressCallback;
		if (action5 != null)
		{
			action5(MapLoadStatus.Loading, 3, "PACKAGE INFO LOADED");
		}
		string text = Path.GetDirectoryName(packageInfoFilePath) + "/" + CustomMapLoader.loadedMapPackageInfo.pcFileName;
		Action<MapLoadStatus, int, string> action6 = CustomMapLoader.mapLoadProgressCallback;
		if (action6 != null)
		{
			action6(MapLoadStatus.Loading, 4, "LOADING MAP ASSET BUNDLE");
		}
		AssetBundleCreateRequest loadBundleRequest = AssetBundle.LoadFromFileAsync(text);
		yield return loadBundleRequest;
		CustomMapLoader.mapBundle = loadBundleRequest.assetBundle;
		if (CustomMapLoader.shouldAbortMapLoading || CustomMapLoader.shouldAbortSceneLoad)
		{
			yield return CustomMapLoader.AbortSceneLoad(-1);
			OnLoadComplete(false, true);
			yield break;
		}
		if (CustomMapLoader.mapBundle == null)
		{
			Action<MapLoadStatus, int, string> action7 = CustomMapLoader.mapLoadProgressCallback;
			if (action7 != null)
			{
				action7(MapLoadStatus.Error, 0, "CUSTOM MAP ASSET BUNDLE FAILED TO LOAD");
			}
			OnLoadComplete(false, false);
			yield break;
		}
		if (!CustomMapLoader.mapBundle.isStreamedSceneAssetBundle)
		{
			CustomMapLoader.mapBundle.Unload(true);
			Action<MapLoadStatus, int, string> action8 = CustomMapLoader.mapLoadProgressCallback;
			if (action8 != null)
			{
				action8(MapLoadStatus.Error, 0, "AssetBundle does not contain a Unity Scene file");
			}
			OnLoadComplete(false, false);
			yield break;
		}
		Action<MapLoadStatus, int, string> action9 = CustomMapLoader.mapLoadProgressCallback;
		if (action9 != null)
		{
			action9(MapLoadStatus.Loading, 10, "MAP ASSET BUNDLE LOADED");
		}
		CustomMapLoader.assetBundleSceneFilePaths = CustomMapLoader.mapBundle.GetAllScenePaths();
		if (CustomMapLoader.assetBundleSceneFilePaths.Length == 0)
		{
			CustomMapLoader.mapBundle.Unload(true);
			Action<MapLoadStatus, int, string> action10 = CustomMapLoader.mapLoadProgressCallback;
			if (action10 != null)
			{
				action10(MapLoadStatus.Error, 0, "AssetBundle does not contain a Unity Scene file");
			}
			OnLoadComplete(false, false);
			yield break;
		}
		foreach (string text2 in CustomMapLoader.assetBundleSceneFilePaths)
		{
			if (text2.Equals(CustomMapLoader.instance.dontDestroyOnLoadSceneName, StringComparison.OrdinalIgnoreCase))
			{
				CustomMapLoader.mapBundle.Unload(true);
				Action<MapLoadStatus, int, string> action11 = CustomMapLoader.mapLoadProgressCallback;
				if (action11 != null)
				{
					action11(MapLoadStatus.Error, 0, "Map name is " + text2 + " this is an invalid name");
				}
				OnLoadComplete(false, false);
				yield break;
			}
		}
		OnLoadComplete(true, false);
		yield break;
	}

	private static void LoadInitialSceneNames()
	{
		CustomMapLoader.initialSceneNames.Clear();
		if (CustomMapLoader.loadedMapPackageInfo != null)
		{
			if (CustomMapLoader.loadedMapPackageInfo.customMapSupportVersion <= 2)
			{
				CustomMapLoader.initialSceneNames.Add(CustomMapLoader.loadedMapPackageInfo.initialScene);
				return;
			}
			if (CustomMapLoader.loadedMapPackageInfo.customMapSupportVersion > 2)
			{
				CustomMapLoader.initialSceneNames.AddRange(CustomMapLoader.loadedMapPackageInfo.initialScenes);
			}
		}
	}

	private static void OnAssetBundleLoaded(bool loadSucceeded, bool loadAborted)
	{
		if (loadAborted)
		{
			return;
		}
		if (loadSucceeded)
		{
			CustomMapLoader.loadedMapModId = CustomMapLoader.attemptedLoadID;
			CustomMapLoader.loadedMapModFileId = 0L;
			ModIOManager.GetMod(new ModId(CustomMapLoader.loadedMapModId), false, delegate(Error error, Mod mod)
			{
				if (!error && mod != null && mod.File != null)
				{
					CustomMapLoader.loadedMapModFileId = mod.File.Id;
				}
			});
			foreach (string text in CustomMapLoader.initialSceneNames)
			{
				int num = -1;
				if (text != string.Empty)
				{
					num = CustomMapLoader.GetSceneIndex(text);
				}
				if (num == -1)
				{
					GTDev.LogError<string>("[CustomMapLoader::OnAssetBundleLoaded] Encountered invalid initial scene, could not get scene index for: \"" + text + "\"", null);
				}
				else
				{
					CustomMapLoader.initialSceneIndexes.Add(num);
				}
			}
			if (CustomMapLoader.initialSceneIndexes.Count == 0)
			{
				if (CustomMapLoader.assetBundleSceneFilePaths.Length == 1)
				{
					GTDev.LogWarning<string>("[CustomMapLoader::OnAssetBundleLoaded] Asset Bundle only contains 1 Scene, but it isn't marked as an initial scene. Treating it as an initial scene...", null);
					CustomMapLoader.initialSceneIndexes.Add(0);
				}
				else if (CustomMapLoader.mapBundle != null)
				{
					string text2 = "";
					if (CustomMapLoader.assetBundleSceneFilePaths.Length == 0)
					{
						text2 = "MAP ASSET BUNDLE CONTAINS NO VALID SCENES.";
					}
					else if (CustomMapLoader.assetBundleSceneFilePaths.Length > 1)
					{
						text2 = "MAP ASSET BUNDLE CONTAINS MULTIPLE SCENES, BUT NONE ARE SET AS INITIAL SCENE.";
					}
					Action<MapLoadStatus, int, string> action = CustomMapLoader.mapLoadProgressCallback;
					if (action != null)
					{
						action(MapLoadStatus.Error, 0, text2);
					}
					CustomMapLoader.OnInitialLoadComplete(false, true);
				}
			}
			CustomMapLoader.instance.StartCoroutine(CustomMapLoader.LoadInitialScenesCoroutine(CustomMapLoader.initialSceneIndexes.ToArray()));
		}
	}

	private static IEnumerator LoadInitialScenesCoroutine(int[] sceneIndexes)
	{
		CustomMapLoader.<>c__DisplayClass98_0 CS$<>8__locals1 = new CustomMapLoader.<>c__DisplayClass98_0();
		CS$<>8__locals1.sceneIndexes = sceneIndexes;
		if (!CustomMapLoader.loadedSceneIndexes.IsNullOrEmpty<int>())
		{
			GTDev.LogError<string>("[CustomMapLoader::LoadInitialScenesCoroutine] loadedSceneIndexes is not empty, LoadInitialScenes should not be called in this case!", null);
			yield break;
		}
		int progressAmountPerScene = 89 / CS$<>8__locals1.sceneIndexes.Length;
		GTDev.Log<string>(string.Format("[CustomMapLoader::LoadInitialScenesCoroutine] loading {0} scenes...", CS$<>8__locals1.sceneIndexes.Length), null);
		CS$<>8__locals1.i = 0;
		while (CS$<>8__locals1.i < CS$<>8__locals1.sceneIndexes.Length)
		{
			CustomMapLoader.<>c__DisplayClass98_1 CS$<>8__locals2 = new CustomMapLoader.<>c__DisplayClass98_1();
			CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
			int num = 10 + CS$<>8__locals2.CS$<>8__locals1.i * progressAmountPerScene;
			int num2 = num + progressAmountPerScene;
			CS$<>8__locals2.isLastScene = CS$<>8__locals2.CS$<>8__locals1.i == CS$<>8__locals2.CS$<>8__locals1.sceneIndexes.Length - 1;
			CS$<>8__locals2.stopLoading = false;
			CS$<>8__locals2.initialLoadAborted = false;
			yield return CustomMapLoader.LoadSceneFromAssetBundle(CS$<>8__locals2.CS$<>8__locals1.sceneIndexes[CS$<>8__locals2.CS$<>8__locals1.i], delegate(bool loadSucceeded, bool loadAborted, string loadedSceneName)
			{
				if (!loadSucceeded || loadAborted)
				{
					GTDev.Log<string>("[CustomMapLoader::LoadInitialScenesCoroutine] failed to load scene at index " + string.Format("\"{0}\", aborting initial load...", CS$<>8__locals2.CS$<>8__locals1.sceneIndexes[CS$<>8__locals2.CS$<>8__locals1.i]), null);
					CS$<>8__locals2.stopLoading = true;
					CS$<>8__locals2.initialLoadAborted = loadAborted;
					return;
				}
				if (CS$<>8__locals2.isLastScene)
				{
					CustomMapLoader.OnInitialLoadComplete(true, false);
				}
			}, true, num, num2);
			if (CS$<>8__locals2.stopLoading || CustomMapLoader.shouldAbortMapLoading)
			{
				CustomMapLoader.OnInitialLoadComplete(false, CS$<>8__locals2.initialLoadAborted);
				break;
			}
			CS$<>8__locals2 = null;
			int i = CS$<>8__locals1.i;
			CS$<>8__locals1.i = i + 1;
		}
		yield break;
	}

	private static void OnInitialLoadComplete(bool loadSucceeded, bool loadAborted)
	{
		if (loadAborted || !loadSucceeded)
		{
			if (!loadAborted)
			{
				CustomMapLoader.instance.StartCoroutine(CustomMapLoader.AbortMapLoad());
				return;
			}
			Action<bool> action = CustomMapLoader.mapLoadFinishedCallback;
			if (action == null)
			{
				return;
			}
			action(false);
			return;
		}
		else
		{
			if (CustomMapLoader.loadedMapPackageInfo != null && CustomMapLoader.loadedMapPackageInfo.customMapSupportVersion >= 3)
			{
				CustomMapLoader.maxPlayersForMap = (byte)Math.Clamp(CustomMapLoader.loadedMapPackageInfo.maxPlayers, 1, 10);
				if (CustomMapLoader.loadedMapPackageInfo.customMapSupportVersion >= 5)
				{
					CustomMapModeSelector.SetAvailableGameModes(CustomMapLoader.loadedMapPackageInfo.availableGameModes, CustomMapLoader.loadedMapPackageInfo.defaultGameMode);
					if (RoomSystem.JoinedRoom && NetworkSystem.Instance.LocalPlayer.IsMasterClient && NetworkSystem.Instance.SessionIsPrivate)
					{
						if (GameMode.ActiveGameMode.IsNull())
						{
							GameModeType gameModeType = (GameModeType)CustomMapLoader.loadedMapPackageInfo.defaultGameMode;
							GameMode.ChangeGameMode(gameModeType.ToString());
						}
						else if (GameMode.ActiveGameMode.GameType() != (GameModeType)CustomMapLoader.loadedMapPackageInfo.defaultGameMode)
						{
							GameModeType gameModeType = (GameModeType)CustomMapLoader.loadedMapPackageInfo.defaultGameMode;
							GameMode.ChangeGameMode(gameModeType.ToString());
						}
					}
				}
				else
				{
					List<int> list = new List<int>();
					foreach (GameModeType gameModeType2 in CustomMapLoader.instance.availableModesForOldMaps)
					{
						list.Add((int)gameModeType2);
					}
					GameModeType gameModeType3 = CustomMapLoader.instance.defaultGameModeForNonCustomOldMaps;
					if (!CustomMapLoader.loadedMapPackageInfo.customGamemodeScript.IsNullOrEmpty())
					{
						gameModeType3 = GameModeType.Custom;
						list.Add(7);
					}
					CustomMapModeSelector.SetAvailableGameModes(list.ToArray(), (int)gameModeType3);
					if (RoomSystem.JoinedRoom && NetworkSystem.Instance.LocalPlayer.IsMasterClient && NetworkSystem.Instance.SessionIsPrivate)
					{
						if (GameMode.ActiveGameMode.IsNull())
						{
							GameMode.ChangeGameMode(gameModeType3.ToString());
						}
						else if (GameMode.ActiveGameMode.GameType() != gameModeType3)
						{
							GameMode.ChangeGameMode(gameModeType3.ToString());
						}
					}
				}
				CustomMapLoader.cachedLuauScript = CustomMapLoader.loadedMapPackageInfo.customGamemodeScript;
				CustomMapLoader.devModeEnabled = CustomMapLoader.loadedMapPackageInfo.devMode;
				CustomMapLoader.disableHoldingHandsAllModes = CustomMapLoader.loadedMapPackageInfo.disableHoldingHandsAllModes;
				CustomMapLoader.disableHoldingHandsCustomMode = CustomMapLoader.loadedMapPackageInfo.disableHoldingHandsCustomMode;
				Color color = new Color(CustomMapLoader.loadedMapPackageInfo.uberShaderAmbientDynamicLight_R, CustomMapLoader.loadedMapPackageInfo.uberShaderAmbientDynamicLight_G, CustomMapLoader.loadedMapPackageInfo.uberShaderAmbientDynamicLight_B, CustomMapLoader.loadedMapPackageInfo.uberShaderAmbientDynamicLight_A);
				if (CustomMapLoader.loadedMapPackageInfo.useUberShaderDynamicLighting)
				{
					GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
					GameLightingManager.instance.SetAmbientLightDynamic(color);
					CustomMapLoader.usingDynamicLighting = true;
				}
				VirtualStumpReturnWatch.SetWatchProperties(CustomMapLoader.loadedMapPackageInfo.GetReturnToVStumpWatchProps());
			}
			CustomMapLoader.isLoading = false;
			CustomMapLoader.CanLoadEntities = true;
			GorillaNetworkJoinTrigger.EnableTriggerJoins();
			Action<MapLoadStatus, int, string> action2 = CustomMapLoader.mapLoadProgressCallback;
			if (action2 != null)
			{
				action2(MapLoadStatus.Loading, 100, "LOAD COMPLETE");
			}
			if (CustomMapLoader.instance.publicJoinTrigger != null)
			{
				CustomMapLoader.instance.publicJoinTrigger.SetActive(true);
			}
			foreach (string text in CustomMapLoader.loadedSceneNames)
			{
				Action<string> action3 = CustomMapLoader.sceneLoadedCallback;
				if (action3 != null)
				{
					action3(text);
				}
			}
			Action<bool> action4 = CustomMapLoader.mapLoadFinishedCallback;
			if (action4 == null)
			{
				return;
			}
			action4(true);
			return;
		}
	}

	private static IEnumerator LoadScenesCoroutine(int[] sceneIndexes, Action<bool, bool, List<string>> loadCompleteCallback = null)
	{
		CustomMapLoader.<>c__DisplayClass100_0 CS$<>8__locals1 = new CustomMapLoader.<>c__DisplayClass100_0();
		CS$<>8__locals1.loadCompleteCallback = loadCompleteCallback;
		if (sceneIndexes.IsNullOrEmpty<int>())
		{
			Action<bool, bool, List<string>> loadCompleteCallback2 = CS$<>8__locals1.loadCompleteCallback;
			if (loadCompleteCallback2 != null)
			{
				loadCompleteCallback2(false, false, null);
			}
			yield break;
		}
		CustomMapLoader.isLoading = true;
		CS$<>8__locals1.successfullyLoadedSceneNames = new List<string>();
		CS$<>8__locals1.successfullyLoadedAllScenes = true;
		int num;
		for (int i = 0; i < sceneIndexes.Length; i = num + 1)
		{
			CustomMapLoader.<>c__DisplayClass100_1 CS$<>8__locals2 = new CustomMapLoader.<>c__DisplayClass100_1();
			CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
			if (CustomMapLoader.loadedSceneIndexes.Contains(sceneIndexes[i]))
			{
				GTDev.LogWarning<string>("[CustomMapLoader::LoadScenesCoroutine] Cannot load scene " + string.Format("{0}:\"{1}\" because it's already loaded!", sceneIndexes[i], CustomMapLoader.assetBundleSceneFilePaths[sceneIndexes[i]]), null);
			}
			else
			{
				CS$<>8__locals2.shouldAbortLoad = false;
				CS$<>8__locals2.isLastScene = i == sceneIndexes.Length - 1;
				yield return CustomMapLoader.LoadSceneFromAssetBundle(sceneIndexes[i], delegate(bool loadSucceeded, bool loadAborted, string loadedSceneName)
				{
					if (!loadSucceeded || loadAborted)
					{
						CS$<>8__locals2.CS$<>8__locals1.successfullyLoadedAllScenes = false;
					}
					else
					{
						Action<string> action = CustomMapLoader.sceneLoadedCallback;
						if (action != null)
						{
							action(loadedSceneName);
						}
						CS$<>8__locals2.CS$<>8__locals1.successfullyLoadedSceneNames.Add(loadedSceneName);
					}
					if (loadAborted)
					{
						CS$<>8__locals2.shouldAbortLoad = true;
						return;
					}
					if (CS$<>8__locals2.isLastScene)
					{
						Action<bool, bool, List<string>> loadCompleteCallback4 = CS$<>8__locals2.CS$<>8__locals1.loadCompleteCallback;
						if (loadCompleteCallback4 == null)
						{
							return;
						}
						loadCompleteCallback4(CS$<>8__locals2.CS$<>8__locals1.successfullyLoadedAllScenes, false, CS$<>8__locals2.CS$<>8__locals1.successfullyLoadedSceneNames);
					}
				}, false, 10, 90);
				if (CS$<>8__locals2.shouldAbortLoad)
				{
					CustomMapLoader.isLoading = false;
					Action<bool, bool, List<string>> loadCompleteCallback3 = CS$<>8__locals2.CS$<>8__locals1.loadCompleteCallback;
					if (loadCompleteCallback3 == null)
					{
						break;
					}
					loadCompleteCallback3(false, true, CS$<>8__locals2.CS$<>8__locals1.successfullyLoadedSceneNames);
					break;
				}
				else
				{
					CS$<>8__locals2 = null;
				}
			}
			num = i;
		}
		CustomMapLoader.isLoading = false;
		yield break;
	}

	private static IEnumerator LoadSceneFromAssetBundle(int sceneIndex, Action<bool, bool, string> OnLoadComplete, bool useProgressCallback = false, int startingProgress = 10, int endingProgress = 90)
	{
		int progressAmount = endingProgress - startingProgress;
		int currentProgress = startingProgress;
		CustomMapLoader.refreshReviveStations = false;
		LoadSceneParameters loadSceneParameters = new LoadSceneParameters
		{
			loadSceneMode = LoadSceneMode.Additive,
			localPhysicsMode = LocalPhysicsMode.None
		};
		if (CustomMapLoader.shouldAbortSceneLoad)
		{
			yield return CustomMapLoader.AbortSceneLoad(sceneIndex);
			OnLoadComplete(false, true, "");
			yield break;
		}
		CustomMapLoader.runningAsyncLoad = true;
		if (useProgressCallback)
		{
			int num = startingProgress + Mathf.RoundToInt((float)progressAmount * 0.02f);
			Action<MapLoadStatus, int, string> action = CustomMapLoader.mapLoadProgressCallback;
			if (action != null)
			{
				action(MapLoadStatus.Loading, num, "LOADING MAP SCENE");
			}
		}
		CustomMapLoader.attemptedSceneToLoad = CustomMapLoader.assetBundleSceneFilePaths[sceneIndex];
		string sceneName = CustomMapLoader.GetSceneNameFromFilePath(CustomMapLoader.attemptedSceneToLoad);
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(CustomMapLoader.attemptedSceneToLoad, loadSceneParameters);
		yield return asyncOperation;
		CustomMapLoader.runningAsyncLoad = false;
		if (CustomMapLoader.shouldAbortSceneLoad)
		{
			yield return CustomMapLoader.AbortSceneLoad(sceneIndex);
			OnLoadComplete(false, true, "");
			yield break;
		}
		if (useProgressCallback)
		{
			currentProgress += Mathf.RoundToInt((float)progressAmount * 0.28f);
			Action<MapLoadStatus, int, string> action2 = CustomMapLoader.mapLoadProgressCallback;
			if (action2 != null)
			{
				action2(MapLoadStatus.Loading, currentProgress, "SANITIZING MAP");
			}
		}
		GameObject[] rootGameObjects = SceneManager.GetSceneByName(sceneName).GetRootGameObjects();
		List<MapDescriptor> list = new List<MapDescriptor>();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			MapDescriptor component = rootGameObjects[i].GetComponent<MapDescriptor>();
			if (component.IsNotNull())
			{
				list.Add(component);
			}
		}
		MapDescriptor mapDescriptor = null;
		bool flag = false;
		foreach (MapDescriptor mapDescriptor2 in list)
		{
			if (!mapDescriptor.IsNull())
			{
				flag = true;
				break;
			}
			mapDescriptor = mapDescriptor2;
		}
		if (flag)
		{
			GTDev.LogWarning<string>("[CustomMapLoader::LoadSceneFromAssetBundle] Found multiple MapDescriptor components in Scene \"" + sceneName + "\". Only the first one found will be used...", null);
		}
		if (mapDescriptor.IsNull())
		{
			yield return CustomMapLoader.AbortSceneLoad(sceneIndex);
			if (useProgressCallback)
			{
				Action<MapLoadStatus, int, string> action3 = CustomMapLoader.mapLoadProgressCallback;
				if (action3 != null)
				{
					action3(MapLoadStatus.Error, 0, "SCENE \"" + sceneName + "\" DOES NOT CONTAIN A MAP DESCRIPTOR ON ONE OF ITS ROOT GAME OBJECTS.");
				}
			}
			OnLoadComplete(false, false, "");
			yield break;
		}
		GameObject gameObject = mapDescriptor.gameObject;
		if (!CustomMapLoader.SanitizeObject(gameObject, gameObject))
		{
			yield return CustomMapLoader.AbortSceneLoad(sceneIndex);
			if (useProgressCallback)
			{
				Action<MapLoadStatus, int, string> action4 = CustomMapLoader.mapLoadProgressCallback;
				if (action4 != null)
				{
					action4(MapLoadStatus.Error, 0, "MAP DESCRIPTOR GAME OBJECT ON SCENE \"" + sceneName + "\" HAS UNAPPROVED COMPONENTS ON IT");
				}
			}
			OnLoadComplete(false, false, "");
			yield break;
		}
		if (CustomMapLoader.loadedMapPackageInfo.customMapSupportVersion < 4)
		{
			foreach (TextMeshPro textMeshPro in gameObject.transform.GetComponentsInChildren<TextMeshPro>(true))
			{
				if (textMeshPro.font == null || textMeshPro.font.material == null)
				{
					textMeshPro.font = CustomMapLoader.instance.DefaultFont;
				}
			}
			foreach (TextMeshProUGUI textMeshProUGUI in gameObject.transform.GetComponentsInChildren<TextMeshProUGUI>(true))
			{
				if (textMeshProUGUI.font == null || textMeshProUGUI.font.material == null)
				{
					textMeshProUGUI.font = CustomMapLoader.instance.DefaultFont;
				}
			}
		}
		CustomMapLoader.totalObjectsInLoadingScene = 0;
		for (int l = 0; l < rootGameObjects.Length; l++)
		{
			CustomMapLoader.SanitizeObjectRecursive(rootGameObjects[l], gameObject);
		}
		CustomMapLoader.ResolveVirtualStumpColliderOverlaps(sceneName);
		if (useProgressCallback)
		{
			currentProgress += Mathf.RoundToInt((float)progressAmount * 0.2f);
			Action<MapLoadStatus, int, string> action5 = CustomMapLoader.mapLoadProgressCallback;
			if (action5 != null)
			{
				action5(MapLoadStatus.Loading, currentProgress, "MAP SCENE LOADED");
			}
		}
		CustomMapLoader.leafGliderIndex = 0;
		yield return CustomMapLoader.FinalizeSceneLoad(mapDescriptor, useProgressCallback, currentProgress, endingProgress);
		yield return null;
		if (CustomMapLoader.shouldAbortSceneLoad)
		{
			yield return CustomMapLoader.AbortSceneLoad(sceneIndex);
			OnLoadComplete(false, true, "");
			if (CustomMapLoader.cachedExceptionMessage.Length > 0 && useProgressCallback)
			{
				Action<MapLoadStatus, int, string> action6 = CustomMapLoader.mapLoadProgressCallback;
				if (action6 != null)
				{
					action6(MapLoadStatus.Error, 0, CustomMapLoader.cachedExceptionMessage);
				}
			}
			yield break;
		}
		if (CustomMapLoader.errorEncounteredDuringLoad)
		{
			OnLoadComplete(false, false, "");
			if (CustomMapLoader.cachedExceptionMessage.Length > 0 && useProgressCallback)
			{
				Action<MapLoadStatus, int, string> action7 = CustomMapLoader.mapLoadProgressCallback;
				if (action7 != null)
				{
					action7(MapLoadStatus.Error, 0, CustomMapLoader.cachedExceptionMessage);
				}
			}
			yield break;
		}
		if (useProgressCallback)
		{
			Action<MapLoadStatus, int, string> action8 = CustomMapLoader.mapLoadProgressCallback;
			if (action8 != null)
			{
				action8(MapLoadStatus.Loading, endingProgress, "FINALIZING MAP");
			}
		}
		CustomMapLoader.loadedSceneFilePaths.AddIfNew(CustomMapLoader.attemptedSceneToLoad);
		CustomMapLoader.loadedSceneNames.AddIfNew(sceneName);
		CustomMapLoader.loadedSceneIndexes.AddIfNew(sceneIndex);
		if (CustomMapLoader.refreshReviveStations)
		{
			CustomMapLoader.instance.ghostReactorManager.reactor.RefreshReviveStations(true);
		}
		OnLoadComplete(true, false, sceneName);
		yield break;
	}

	private static void SanitizeObjectRecursive(GameObject rootObject, GameObject mapRoot)
	{
		if (!CustomMapLoader.SanitizeObject(rootObject, mapRoot))
		{
			return;
		}
		CustomMapLoader.totalObjectsInLoadingScene++;
		for (int i = 0; i < rootObject.transform.childCount; i++)
		{
			GameObject gameObject = rootObject.transform.GetChild(i).gameObject;
			if (gameObject.IsNotNull())
			{
				CustomMapLoader.SanitizeObjectRecursive(gameObject, mapRoot);
			}
		}
	}

	private static bool SanitizeObject(GameObject gameObject, GameObject mapRoot)
	{
		if (gameObject == null)
		{
			Debug.LogError("CustomMapLoader::SanitizeObject gameobject null");
			return false;
		}
		if (!CustomMapLoader.APPROVED_LAYERS.Contains(gameObject.layer))
		{
			gameObject.layer = 0;
		}
		foreach (Component component in gameObject.GetComponents<Component>())
		{
			if (component == null)
			{
				Object.DestroyImmediate(gameObject, true);
				return false;
			}
			bool flag = true;
			foreach (Type type in CustomMapLoader.componentAllowlist)
			{
				if (component.GetType() == type)
				{
					if (type == typeof(Camera))
					{
						Camera camera = (Camera)component;
						if (camera.IsNotNull() && camera.targetTexture.IsNull())
						{
							break;
						}
					}
					flag = false;
					break;
				}
			}
			if (flag)
			{
				foreach (string text in CustomMapLoader.componentTypeStringAllowList)
				{
					if (component.GetType().ToString().Contains(text))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				Object.DestroyImmediate(gameObject, true);
				return false;
			}
		}
		if (gameObject.transform.parent.IsNull() && gameObject.transform != mapRoot.transform)
		{
			gameObject.transform.SetParent(mapRoot.transform);
		}
		return true;
	}

	private static void ResolveVirtualStumpColliderOverlaps(string sceneName)
	{
		Vector3 vector = new Vector3(5.15f, 0.72f, 5.15f);
		Vector3 vector2 = new Vector3(0f, 0.73f, 0f);
		float num = vector.x * 0.5f + 2f;
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		gameObject.transform.position = CustomMapLoader.instance.virtualStumpMesh.transform.position + vector2;
		gameObject.transform.localScale = vector;
		Collider[] array = Physics.OverlapSphere(gameObject.transform.position, num);
		if (array == null || array.Length == 0)
		{
			Object.DestroyImmediate(gameObject);
			return;
		}
		MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
		meshCollider.convex = true;
		foreach (Collider collider in array)
		{
			Vector3 vector3;
			float num2;
			if (!(collider == null) && !(collider.gameObject == gameObject) && !(collider.gameObject.scene.name != sceneName) && Physics.ComputePenetration(meshCollider, gameObject.transform.position, gameObject.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out vector3, out num2) && !collider.isTrigger)
			{
				GTDev.Log<string>("[CustomMapLoader::ResolveVirtualStumpColliderOverlaps] Gameobject " + collider.name + " has a collider overlapping with the virtual stump. Collider will be removed", null);
				Object.DestroyImmediate(collider);
			}
		}
		Object.DestroyImmediate(gameObject);
	}

	private static IEnumerator FinalizeSceneLoad(MapDescriptor sceneDescriptor, bool useProgressCallback = false, int startingProgress = 50, int endingProgress = 90)
	{
		int num = endingProgress - startingProgress;
		int num2 = startingProgress;
		if (useProgressCallback)
		{
			num2 += Mathf.RoundToInt((float)num * 0.02f);
			Action<MapLoadStatus, int, string> action = CustomMapLoader.mapLoadProgressCallback;
			if (action != null)
			{
				action(MapLoadStatus.Loading, num2, "PROCESSING ROOT MAP OBJECT");
			}
		}
		CustomMapLoader.objectsProcessedForLoadingScene = 0;
		CustomMapLoader.objectsProcessedThisFrame = 0;
		if (useProgressCallback)
		{
			num2 += Mathf.RoundToInt((float)num * 0.03f);
			Action<MapLoadStatus, int, string> action2 = CustomMapLoader.mapLoadProgressCallback;
			if (action2 != null)
			{
				action2(MapLoadStatus.Loading, num2, "PROCESSING CHILD OBJECTS");
			}
		}
		int processChildrenEndingProgress = endingProgress - Mathf.RoundToInt((float)num * 0.02f);
		CustomMapLoader.initializePhaseTwoComponents.Clear();
		CustomMapLoader.entitiesToCreate.Clear();
		yield return CustomMapLoader.ProcessChildObjects(sceneDescriptor.gameObject, useProgressCallback, num2, processChildrenEndingProgress);
		if (CustomMapLoader.shouldAbortSceneLoad || CustomMapLoader.errorEncounteredDuringLoad)
		{
			yield break;
		}
		if (useProgressCallback)
		{
			Action<MapLoadStatus, int, string> action3 = CustomMapLoader.mapLoadProgressCallback;
			if (action3 != null)
			{
				action3(MapLoadStatus.Loading, processChildrenEndingProgress, "PROCESSING COMPLETE");
			}
		}
		yield return null;
		CustomMapLoader.InitializeComponentsPhaseTwo();
		CustomMapLoader.placeholderReplacements.Clear();
		if (useProgressCallback)
		{
			Action<MapLoadStatus, int, string> action4 = CustomMapLoader.mapLoadProgressCallback;
			if (action4 != null)
			{
				action4(MapLoadStatus.Loading, endingProgress, "PROCESSING COMPLETE");
			}
		}
		if (CustomMapLoader.loadedMapPackageInfo != null && CustomMapLoader.loadedMapPackageInfo.customMapSupportVersion < 3 && sceneDescriptor.IsInitialScene)
		{
			CustomMapLoader.maxPlayersForMap = (byte)Math.Clamp(sceneDescriptor.MaxPlayers, 1, 10);
			CustomMapLoader.cachedLuauScript = ((sceneDescriptor.CustomGamemode != null) ? sceneDescriptor.CustomGamemode.text : "");
			CustomMapLoader.devModeEnabled = sceneDescriptor.DevMode;
			CustomMapLoader.disableHoldingHandsAllModes = sceneDescriptor.DisableHoldingHandsAllGameModes;
			CustomMapLoader.disableHoldingHandsCustomMode = sceneDescriptor.DisableHoldingHandsCustomOnly;
			if (sceneDescriptor.UseUberShaderDynamicLighting)
			{
				GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
				GameLightingManager.instance.SetAmbientLightDynamic(sceneDescriptor.UberShaderAmbientDynamicLight);
				CustomMapLoader.usingDynamicLighting = true;
			}
			VirtualStumpReturnWatch.SetWatchProperties(sceneDescriptor.GetReturnToVStumpWatchProps());
		}
		yield break;
	}

	private static IEnumerator ProcessChildObjects(GameObject parent, bool useProgressCallback = false, int startingProgress = 75, int endingProgress = 90)
	{
		if (parent == null || CustomMapLoader.placeholderReplacements.Contains(parent))
		{
			yield break;
		}
		int progressAmount = endingProgress - startingProgress;
		int num3;
		for (int i = 0; i < parent.transform.childCount; i = num3 + 1)
		{
			Transform child = parent.transform.GetChild(i);
			if (!(child == null))
			{
				GameObject gameObject = child.gameObject;
				if (!(gameObject == null) && !CustomMapLoader.placeholderReplacements.Contains(gameObject))
				{
					try
					{
						CustomMapLoader.InitializeComponentsPhaseOne(gameObject);
					}
					catch (Exception ex)
					{
						CustomMapLoader.errorEncounteredDuringLoad = true;
						CustomMapLoader.cachedExceptionMessage = ex.ToString();
						Debug.LogError("[CML.LoadMap] Exception: " + ex.ToString());
						yield break;
					}
					if (gameObject.transform.childCount > 0)
					{
						yield return CustomMapLoader.ProcessChildObjects(gameObject, useProgressCallback, startingProgress, endingProgress);
						if (CustomMapLoader.shouldAbortSceneLoad || CustomMapLoader.errorEncounteredDuringLoad)
						{
							yield break;
						}
					}
					if (CustomMapLoader.shouldAbortSceneLoad)
					{
						yield break;
					}
					CustomMapLoader.objectsProcessedForLoadingScene++;
					CustomMapLoader.objectsProcessedThisFrame++;
					if (CustomMapLoader.objectsProcessedThisFrame >= CustomMapLoader.numObjectsToProcessPerFrame)
					{
						CustomMapLoader.objectsProcessedThisFrame = 0;
						if (useProgressCallback)
						{
							float num = (float)CustomMapLoader.objectsProcessedForLoadingScene / (float)CustomMapLoader.totalObjectsInLoadingScene;
							int num2 = startingProgress + Mathf.FloorToInt((float)progressAmount * num);
							Action<MapLoadStatus, int, string> action = CustomMapLoader.mapLoadProgressCallback;
							if (action != null)
							{
								action(MapLoadStatus.Loading, num2, "PROCESSING CHILD OBJECTS");
							}
						}
						yield return null;
					}
				}
			}
			num3 = i;
		}
		yield break;
	}

	private static void InitializeComponentsPhaseOne(GameObject childGameObject)
	{
		CustomMapLoader.SetupCollisions(childGameObject);
		CustomMapLoader.ReplaceDataOnlyScripts(childGameObject);
		CustomMapLoader.ReplacePlaceholders(childGameObject);
		CustomMapLoader.SetupDynamicLight(childGameObject);
		CustomMapLoader.StoreMapEntity(childGameObject);
		CustomMapLoader.SetupReviveStation(childGameObject);
	}

	private static void InitializeComponentsPhaseTwo()
	{
		for (int i = 0; i < CustomMapLoader.initializePhaseTwoComponents.Count; i++)
		{
		}
		CustomMapLoader.initializePhaseTwoComponents.Clear();
		if (CustomMapLoader.entitiesToCreate.Count > 0)
		{
			for (int j = 0; j < CustomMapLoader.entitiesToCreate.Count; j++)
			{
				CustomMapLoader.entitiesToCreate[j].gameObject.SetActive(false);
			}
			CustomMapsGameManager.AddAgentsToCreate(CustomMapLoader.entitiesToCreate);
		}
	}

	private static void SetupReviveStation(GameObject gameObject)
	{
		if (gameObject == null)
		{
			return;
		}
		CustomMapReviveStation component = gameObject.GetComponent<CustomMapReviveStation>();
		if (component == null)
		{
			return;
		}
		GameObject gameObject2 = Object.Instantiate<GameObject>(CustomMapLoader.instance.reviveStationPrefab, gameObject.transform.parent);
		if (gameObject2 == null)
		{
			return;
		}
		gameObject2.transform.position = gameObject.transform.position;
		gameObject2.transform.rotation = gameObject.transform.rotation;
		gameObject.transform.SetParent(gameObject2.transform);
		GRReviveStation component2 = gameObject2.GetComponent<GRReviveStation>();
		if (component2 == null)
		{
			return;
		}
		component2.audioSource = component.audioSource;
		if (!component.particleEffects.IsNullOrEmpty<ParticleSystem>())
		{
			component2.particleEffects = new ParticleSystem[component.particleEffects.Length];
			for (int i = 0; i < component.particleEffects.Length; i++)
			{
				component2.particleEffects[i] = component.particleEffects[i];
			}
		}
		component2.SetReviveCooldownSeconds(component.reviveCooldownSeconds);
		CustomMapLoader.refreshReviveStations = true;
	}

	private static void SetupCollisions(GameObject gameObject)
	{
		if (gameObject == null || CustomMapLoader.placeholderReplacements.Contains(gameObject))
		{
			return;
		}
		Collider[] components = gameObject.GetComponents<Collider>();
		if (components == null)
		{
			return;
		}
		bool flag = true;
		foreach (Collider collider in components)
		{
			if (!(collider == null))
			{
				if (collider.isTrigger)
				{
					if (gameObject.layer != UnityLayer.GorillaInteractable.ToLayerIndex())
					{
						gameObject.layer = UnityLayer.GorillaTrigger.ToLayerIndex();
						break;
					}
				}
				else
				{
					if (gameObject.layer == UnityLayer.GorillaTrigger.ToLayerIndex())
					{
						collider.isTrigger = true;
					}
					flag = false;
					if (gameObject.GetComponent<GrabbableEntity>().IsNotNull())
					{
						gameObject.layer = UnityLayer.Default.ToLayerIndex();
						return;
					}
				}
			}
		}
		if (!flag)
		{
			SurfaceOverrideSettings component = gameObject.GetComponent<SurfaceOverrideSettings>();
			GorillaSurfaceOverride gorillaSurfaceOverride = gameObject.AddComponent<GorillaSurfaceOverride>();
			if (component == null)
			{
				gorillaSurfaceOverride.overrideIndex = 0;
				return;
			}
			gorillaSurfaceOverride.overrideIndex = (int)component.soundOverride;
			gorillaSurfaceOverride.extraVelMultiplier = component.extraVelMultiplier;
			gorillaSurfaceOverride.extraVelMaxMultiplier = component.extraVelMaxMultiplier;
			gorillaSurfaceOverride.slidePercentageOverride = component.slidePercentage;
			gorillaSurfaceOverride.disablePushBackEffect = component.disablePushBackEffect;
			Object.Destroy(component);
		}
	}

	private static bool ValidateTeleporterDestination(Transform teleportTarget)
	{
		using (List<GameObject>.Enumerator enumerator = CustomMapLoader.storeCheckouts.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (Vector3.Distance(enumerator.Current.transform.position, teleportTarget.position) < Constants.minTeleportDistFromStorePlaceholder)
				{
					return false;
				}
			}
		}
		using (List<GameObject>.Enumerator enumerator = CustomMapLoader.storeDisplayStands.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (Vector3.Distance(enumerator.Current.transform.position, teleportTarget.position) < Constants.minTeleportDistFromStorePlaceholder)
				{
					return false;
				}
			}
		}
		return !CustomMapLoader.customMapATM.IsNotNull() || Vector3.Distance(CustomMapLoader.customMapATM.transform.position, teleportTarget.position) >= Constants.minTeleportDistFromStorePlaceholder;
	}

	private static bool ValidateStorePlaceholderPosition(GameObject storePlaceholder)
	{
		foreach (Component component in CustomMapLoader.teleporters)
		{
			if (!(component == null))
			{
				List<Transform> list = null;
				if (component.GetType() == typeof(CMSMapBoundary))
				{
					CMSMapBoundary cmsmapBoundary = (CMSMapBoundary)component;
					if (cmsmapBoundary != null)
					{
						list = cmsmapBoundary.TeleportPoints;
					}
				}
				else if (component.GetType() == typeof(CMSTeleporter))
				{
					CMSTeleporter cmsteleporter = (CMSTeleporter)component;
					if (cmsteleporter != null)
					{
						list = cmsteleporter.TeleportPoints;
					}
				}
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						Transform transform = list[i];
						if (Vector3.Distance(storePlaceholder.transform.position, transform.position) < Constants.minTeleportDistFromStorePlaceholder)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	private static void ReplaceDataOnlyScripts(GameObject gameObject)
	{
		MapBoundarySettings[] components = gameObject.GetComponents<MapBoundarySettings>();
		if (components != null)
		{
			foreach (MapBoundarySettings mapBoundarySettings in components)
			{
				bool flag = false;
				for (int j = 0; j < mapBoundarySettings.TeleportPoints.Count; j++)
				{
					if (!mapBoundarySettings.TeleportPoints[j].IsNull() && !CustomMapLoader.ValidateTeleporterDestination(mapBoundarySettings.TeleportPoints[j]))
					{
						flag = true;
						Object.Destroy(mapBoundarySettings);
						break;
					}
				}
				if (!flag)
				{
					CMSMapBoundary cmsmapBoundary = gameObject.AddComponent<CMSMapBoundary>();
					if (cmsmapBoundary != null)
					{
						cmsmapBoundary.CopyTriggerSettings(mapBoundarySettings);
						CustomMapLoader.teleporters.Add(cmsmapBoundary);
					}
					Object.Destroy(mapBoundarySettings);
				}
			}
		}
		TagZoneSettings[] components2 = gameObject.GetComponents<TagZoneSettings>();
		if (components2 != null)
		{
			foreach (TagZoneSettings tagZoneSettings in components2)
			{
				CMSTagZone cmstagZone = gameObject.AddComponent<CMSTagZone>();
				if (cmstagZone != null)
				{
					cmstagZone.CopyTriggerSettings(tagZoneSettings);
				}
				Object.Destroy(tagZoneSettings);
			}
		}
		TeleporterSettings[] components3 = gameObject.GetComponents<TeleporterSettings>();
		if (components3 != null)
		{
			foreach (TeleporterSettings teleporterSettings in components3)
			{
				bool flag2 = false;
				for (int k = 0; k < teleporterSettings.TeleportPoints.Count; k++)
				{
					if (!teleporterSettings.TeleportPoints[k].IsNull() && !CustomMapLoader.ValidateTeleporterDestination(teleporterSettings.TeleportPoints[k]))
					{
						flag2 = true;
						Object.Destroy(teleporterSettings);
						break;
					}
				}
				if (!flag2)
				{
					CMSTeleporter cmsteleporter = gameObject.AddComponent<CMSTeleporter>();
					if (cmsteleporter != null)
					{
						cmsteleporter.CopyTriggerSettings(teleporterSettings);
					}
					Object.Destroy(teleporterSettings);
				}
			}
		}
		ObjectActivationTriggerSettings[] components4 = gameObject.GetComponents<ObjectActivationTriggerSettings>();
		if (components4 != null)
		{
			foreach (ObjectActivationTriggerSettings objectActivationTriggerSettings in components4)
			{
				CMSObjectActivationTrigger cmsobjectActivationTrigger = gameObject.AddComponent<CMSObjectActivationTrigger>();
				if (cmsobjectActivationTrigger != null)
				{
					cmsobjectActivationTrigger.CopyTriggerSettings(objectActivationTriggerSettings);
				}
				Object.Destroy(objectActivationTriggerSettings);
			}
		}
		LuauTriggerSettings[] components5 = gameObject.GetComponents<LuauTriggerSettings>();
		if (components5 != null)
		{
			foreach (LuauTriggerSettings luauTriggerSettings in components5)
			{
				CMSLuau cmsluau = gameObject.AddComponent<CMSLuau>();
				if (cmsluau != null)
				{
					cmsluau.CopyTriggerSettings(luauTriggerSettings);
				}
				Object.Destroy(luauTriggerSettings);
			}
		}
		PlayAnimationTriggerSettings[] components6 = gameObject.GetComponents<PlayAnimationTriggerSettings>();
		if (components6 != null)
		{
			foreach (PlayAnimationTriggerSettings playAnimationTriggerSettings in components6)
			{
				CMSPlayAnimationTrigger cmsplayAnimationTrigger = gameObject.AddComponent<CMSPlayAnimationTrigger>();
				if (cmsplayAnimationTrigger != null)
				{
					cmsplayAnimationTrigger.CopyTriggerSettings(playAnimationTriggerSettings);
				}
				Object.Destroy(playAnimationTriggerSettings);
			}
		}
		LoadZoneSettings[] components7 = gameObject.GetComponents<LoadZoneSettings>();
		if (components7 != null)
		{
			foreach (LoadZoneSettings loadZoneSettings in components7)
			{
				CMSLoadingZone cmsloadingZone = gameObject.AddComponent<CMSLoadingZone>();
				if (cmsloadingZone != null)
				{
					cmsloadingZone.SetupLoadingZone(loadZoneSettings, in CustomMapLoader.assetBundleSceneFilePaths);
				}
				Object.Destroy(loadZoneSettings);
			}
		}
		ZoneShaderTriggerSettings[] components8 = gameObject.GetComponents<ZoneShaderTriggerSettings>();
		if (components8 != null)
		{
			foreach (ZoneShaderTriggerSettings zoneShaderTriggerSettings in components8)
			{
				gameObject.AddComponent<CMSZoneShaderSettingsTrigger>().CopySettings(zoneShaderTriggerSettings);
				Object.Destroy(zoneShaderTriggerSettings);
			}
		}
		CMSZoneShaderSettings component = gameObject.GetComponent<CMSZoneShaderSettings>();
		if (component.IsNotNull())
		{
			ZoneShaderSettings zoneShaderSettings = gameObject.AddComponent<ZoneShaderSettings>();
			zoneShaderSettings.CopySettings(component, false);
			if (component.isDefaultValues)
			{
				CustomMapManager.SetDefaultZoneShaderSettings(zoneShaderSettings, component.GetProperties());
			}
			CustomMapManager.AddZoneShaderSettings(zoneShaderSettings);
			Object.Destroy(component);
		}
		HandHoldSettings component2 = gameObject.GetComponent<HandHoldSettings>();
		if (component2.IsNotNull())
		{
			gameObject.AddComponent<HandHold>().CopyProperties(component2);
			Object.Destroy(component2);
		}
		CustomMapEjectButtonSettings component3 = gameObject.GetComponent<CustomMapEjectButtonSettings>();
		if (component3.IsNotNull())
		{
			CustomMapEjectButton customMapEjectButton = gameObject.AddComponent<CustomMapEjectButton>();
			customMapEjectButton.gameObject.layer = UnityLayer.GorillaInteractable.ToLayerIndex();
			customMapEjectButton.CopySettings(component3);
			Object.Destroy(component3);
		}
		MovingSurfaceSettings component4 = gameObject.GetComponent<MovingSurfaceSettings>();
		if (component4.IsNotNull())
		{
			MovingSurface movingSurface = gameObject.AddComponent<MovingSurface>();
			if (movingSurface.IsNotNull())
			{
				movingSurface.CopySettings(component4);
				Object.Destroy(component4);
			}
		}
		SurfaceMoverSettings component5 = gameObject.GetComponent<SurfaceMoverSettings>();
		if (component5.IsNotNull())
		{
			gameObject.AddComponent<SurfaceMover>().CopySettings(component5);
			Object.Destroy(component5);
		}
	}

	private static void ReplacePlaceholders(GameObject placeholderGameObject)
	{
		if (placeholderGameObject.IsNull())
		{
			return;
		}
		GTObjectPlaceholder component = placeholderGameObject.GetComponent<GTObjectPlaceholder>();
		if (component.IsNull())
		{
			return;
		}
		switch (component.PlaceholderObject)
		{
		case GTObject.LeafGlider:
			if (CustomMapLoader.leafGliderIndex < CustomMapLoader.instance.leafGliders.Length)
			{
				CustomMapLoader.instance.leafGliders[CustomMapLoader.leafGliderIndex].enabled = true;
				CustomMapLoader.instance.leafGliders[CustomMapLoader.leafGliderIndex].CustomMapLoad(component.transform, component.maxDistanceBeforeRespawn);
				CustomMapLoader.instance.leafGliders[CustomMapLoader.leafGliderIndex].transform.GetChild(0).gameObject.SetActive(true);
				CustomMapLoader.leafGliderIndex++;
				return;
			}
			break;
		case GTObject.GliderWindVolume:
		{
			List<Collider> list = new List<Collider>(component.GetComponents<Collider>());
			if (component.useDefaultPlaceholder || list.Count == 0)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(CustomMapLoader.instance.gliderWindVolume, placeholderGameObject.transform.position, placeholderGameObject.transform.rotation);
				if (gameObject != null)
				{
					CustomMapLoader.placeholderReplacements.Add(gameObject);
					gameObject.transform.localScale = placeholderGameObject.transform.localScale;
					placeholderGameObject.transform.localScale = Vector3.one;
					gameObject.transform.SetParent(placeholderGameObject.transform);
					GliderWindVolume component2 = gameObject.GetComponent<GliderWindVolume>();
					if (component2 == null)
					{
						return;
					}
					component2.SetProperties(component.maxSpeed, component.maxAccel, component.SpeedVSAccelCurve, component.localWindDirection);
					return;
				}
			}
			else
			{
				placeholderGameObject.layer = UnityLayer.GorillaTrigger.ToLayerIndex();
				GliderWindVolume gliderWindVolume = placeholderGameObject.AddComponent<GliderWindVolume>();
				if (gliderWindVolume.IsNotNull())
				{
					gliderWindVolume.SetProperties(component.maxSpeed, component.maxAccel, component.SpeedVSAccelCurve, component.localWindDirection);
					return;
				}
			}
			break;
		}
		case GTObject.WaterVolume:
		{
			List<Collider> list = new List<Collider>(component.GetComponents<Collider>());
			if (component.useDefaultPlaceholder || list.Count == 0)
			{
				GameObject gameObject2 = Object.Instantiate<GameObject>(CustomMapLoader.instance.waterVolumePrefab, placeholderGameObject.transform.position, placeholderGameObject.transform.rotation);
				if (gameObject2 != null)
				{
					CustomMapLoader.placeholderReplacements.Add(gameObject2);
					gameObject2.layer = UnityLayer.Water.ToLayerIndex();
					gameObject2.transform.localScale = placeholderGameObject.transform.localScale;
					placeholderGameObject.transform.localScale = Vector3.one;
					gameObject2.transform.SetParent(placeholderGameObject.transform);
					MeshRenderer component3 = gameObject2.GetComponent<MeshRenderer>();
					if (component3.IsNull())
					{
						return;
					}
					if (!component.useWaterMesh)
					{
						component3.enabled = false;
						return;
					}
					component3.enabled = true;
					WaterSurfaceMaterialController component4 = gameObject2.GetComponent<WaterSurfaceMaterialController>();
					if (component4.IsNull())
					{
						return;
					}
					component4.ScrollX = component.scrollTextureX;
					component4.ScrollY = component.scrollTextureY;
					component4.Scale = component.scaleTexture;
					return;
				}
			}
			else
			{
				placeholderGameObject.layer = UnityLayer.Water.ToLayerIndex();
				WaterVolume waterVolume = placeholderGameObject.AddComponent<WaterVolume>();
				if (waterVolume.IsNotNull())
				{
					WaterParameters waterParameters = null;
					CMSZoneShaderSettings.EZoneLiquidType liquidType = component.liquidType;
					if (liquidType != CMSZoneShaderSettings.EZoneLiquidType.Water)
					{
						if (liquidType == CMSZoneShaderSettings.EZoneLiquidType.Lava)
						{
							waterParameters = CustomMapLoader.instance.defaultLavaParameters;
						}
					}
					else
					{
						waterParameters = CustomMapLoader.instance.defaultWaterParameters;
					}
					waterVolume.SetPropertiesFromPlaceholder(component.GetWaterVolumeProperties(), list, waterParameters);
					waterVolume.RefreshColliders();
					return;
				}
			}
			break;
		}
		case GTObject.ForceVolume:
		{
			List<Collider> list = new List<Collider>(component.GetComponents<Collider>());
			if (component.useDefaultPlaceholder || list.Count == 0)
			{
				GameObject gameObject3 = Object.Instantiate<GameObject>(CustomMapLoader.instance.forceVolumePrefab, placeholderGameObject.transform.position, placeholderGameObject.transform.rotation);
				if (gameObject3.IsNotNull())
				{
					CustomMapLoader.placeholderReplacements.Add(gameObject3);
					gameObject3.transform.localScale = placeholderGameObject.transform.localScale;
					placeholderGameObject.transform.localScale = Vector3.one;
					gameObject3.transform.SetParent(placeholderGameObject.transform);
					ForceVolume component5 = gameObject3.GetComponent<ForceVolume>();
					if (component5.IsNull())
					{
						return;
					}
					component5.SetPropertiesFromPlaceholder(component.GetForceVolumeProperties(), null, null);
					return;
				}
			}
			else
			{
				ForceVolume forceVolume = placeholderGameObject.AddComponent<ForceVolume>();
				if (forceVolume.IsNotNull())
				{
					AudioSource audioSource = placeholderGameObject.GetComponent<AudioSource>();
					if (audioSource.IsNull())
					{
						audioSource = placeholderGameObject.AddComponent<AudioSource>();
						audioSource.spatialize = true;
						audioSource.playOnAwake = false;
						audioSource.priority = 128;
						audioSource.volume = 0.522f;
						audioSource.pitch = 1f;
						audioSource.panStereo = 0f;
						audioSource.spatialBlend = 1f;
						audioSource.reverbZoneMix = 1f;
						audioSource.dopplerLevel = 1f;
						audioSource.spread = 0f;
						audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
						audioSource.minDistance = 8.2f;
						audioSource.maxDistance = 43.94f;
						audioSource.enabled = true;
					}
					audioSource.outputAudioMixerGroup = CustomMapLoader.instance.masterAudioMixer;
					for (int i = list.Count - 1; i >= 0; i--)
					{
						if (i == 0)
						{
							list[i].isTrigger = true;
						}
						else
						{
							Object.Destroy(list[i]);
						}
					}
					placeholderGameObject.layer = UnityLayer.GorillaBoundary.ToLayerIndex();
					forceVolume.SetPropertiesFromPlaceholder(component.GetForceVolumeProperties(), audioSource, component.GetComponent<Collider>());
					return;
				}
				Debug.LogError("[CustomMapLoader::ReplacePlaceholders] Failed to add ForceVolume component to Placeholder!");
				return;
			}
			break;
		}
		case GTObject.ATM:
		{
			if (CustomMapLoader.customMapATM.IsNotNull())
			{
				Object.Destroy(component);
				return;
			}
			if (!CustomMapLoader.ValidateStorePlaceholderPosition(placeholderGameObject))
			{
				Object.Destroy(component);
				return;
			}
			GameObject gameObject4 = CustomMapLoader.instance.atmPrefab;
			if (component.useCustomMesh)
			{
				gameObject4 = CustomMapLoader.instance.atmNoShellPrefab;
			}
			if (gameObject4.IsNull())
			{
				return;
			}
			GameObject gameObject5 = Object.Instantiate<GameObject>(gameObject4, placeholderGameObject.transform.position, placeholderGameObject.transform.rotation);
			if (gameObject5.IsNotNull())
			{
				gameObject5.transform.SetParent(CustomMapLoader.instance.compositeTryOnArea.transform, true);
				gameObject5.transform.localScale = Vector3.one;
				ATM_UI componentInChildren = gameObject5.GetComponentInChildren<ATM_UI>();
				if (componentInChildren.IsNotNull() && ATM_Manager.instance.IsNotNull())
				{
					componentInChildren.SetCustomMapScene(placeholderGameObject.scene);
					CustomMapLoader.customMapATM = gameObject5;
					ATM_Manager.instance.AddATM(componentInChildren);
					if (!component.defaultCreatorCode.IsNullOrEmpty())
					{
						ATM_Manager.instance.SetTemporaryCreatorCode(component.defaultCreatorCode, true, null);
						return;
					}
				}
			}
			break;
		}
		case GTObject.HoverboardArea:
			if (component.AddComponent<HoverboardAreaTrigger>().IsNotNull())
			{
				component.gameObject.layer = UnityLayer.GorillaBoundary.ToLayerIndex();
				List<Collider> list = new List<Collider>(component.GetComponents<Collider>());
				if (list.Count != 0)
				{
					for (int j = list.Count - 1; j >= 0; j--)
					{
						if (j == 0)
						{
							list[j].isTrigger = true;
						}
						else
						{
							Object.Destroy(list[j]);
						}
					}
					return;
				}
				BoxCollider boxCollider = component.AddComponent<BoxCollider>();
				if (boxCollider.IsNotNull())
				{
					boxCollider.isTrigger = true;
					return;
				}
			}
			break;
		case GTObject.HoverboardDispenser:
		{
			if (CustomMapLoader.instance.hoverboardDispenserPrefab.IsNull())
			{
				Debug.LogError("[CustomMapLoader::ReplacePlaceholders] hoverboardDispenserPrefab is NULL!");
				return;
			}
			GameObject gameObject6 = Object.Instantiate<GameObject>(CustomMapLoader.instance.hoverboardDispenserPrefab, placeholderGameObject.transform.position, placeholderGameObject.transform.rotation);
			if (gameObject6.IsNotNull())
			{
				CustomMapLoader.placeholderReplacements.Add(gameObject6);
				gameObject6.transform.SetParent(placeholderGameObject.transform);
				return;
			}
			break;
		}
		case GTObject.RopeSwing:
		{
			GameObject gameObject7 = Object.Instantiate<GameObject>(CustomMapLoader.instance.ropeSwingPrefab, placeholderGameObject.transform.position, placeholderGameObject.transform.rotation);
			if (gameObject7.IsNull())
			{
				return;
			}
			gameObject7.transform.SetParent(placeholderGameObject.transform);
			CustomMapsGorillaRopeSwing component6 = gameObject7.GetComponent<CustomMapsGorillaRopeSwing>();
			if (component6.IsNull())
			{
				Object.DestroyImmediate(gameObject7);
				return;
			}
			component.ropeLength = Math.Clamp(component.ropeLength, 3, 31);
			if (component.useDefaultPlaceholder)
			{
				component6.SetRopeLength(component.ropeLength);
			}
			else
			{
				component6.SetRopeProperties(component);
			}
			CustomMapLoader.placeholderReplacements.Add(gameObject7);
			return;
		}
		case GTObject.ZipLine:
		{
			GameObject gameObject8 = Object.Instantiate<GameObject>(CustomMapLoader.instance.ziplinePrefab, placeholderGameObject.transform.position, placeholderGameObject.transform.rotation);
			if (gameObject8.IsNull())
			{
				return;
			}
			gameObject8.transform.SetParent(placeholderGameObject.transform);
			CustomMapsGorillaZipline component7 = gameObject8.GetComponent<CustomMapsGorillaZipline>();
			if (component7.IsNull())
			{
				Object.DestroyImmediate(gameObject8);
				return;
			}
			if (component.useDefaultPlaceholder)
			{
				if (!component7.GenerateZipline(component.spline))
				{
					Object.DestroyImmediate(gameObject8);
					return;
				}
			}
			else
			{
				component7.Init(component);
			}
			CustomMapLoader.placeholderReplacements.Add(gameObject8);
			return;
		}
		case GTObject.Store_DisplayStand:
		{
			if (CustomMapLoader.instance.storeDisplayStandPrefab.IsNull())
			{
				return;
			}
			if (CustomMapLoader.storeDisplayStands.Count >= Constants.storeDisplayStandLimit)
			{
				Object.Destroy(component);
				return;
			}
			if (placeholderGameObject.transform.lossyScale != Vector3.one)
			{
				Object.Destroy(component);
				return;
			}
			if (!CustomMapLoader.ValidateStorePlaceholderPosition(placeholderGameObject))
			{
				Object.Destroy(component);
				return;
			}
			GameObject gameObject9 = Object.Instantiate<GameObject>(CustomMapLoader.instance.storeDisplayStandPrefab, placeholderGameObject.transform);
			if (gameObject9.IsNull())
			{
				return;
			}
			gameObject9.transform.SetParent(CustomMapLoader.instance.compositeTryOnArea.transform, true);
			gameObject9.transform.localScale = Vector3.one;
			DynamicCosmeticStand component8 = gameObject9.GetComponent<DynamicCosmeticStand>();
			if (component8.IsNull())
			{
				Object.DestroyImmediate(gameObject9);
				return;
			}
			component8.InitializeForCustomMapCosmeticItem(component.CosmeticItem, placeholderGameObject.scene);
			CustomMapLoader.storeDisplayStands.Add(gameObject9);
			CustomMapLoader.placeholderReplacements.Add(gameObject9);
			return;
		}
		case GTObject.Store_TryOnArea:
		{
			if (CustomMapLoader.instance.storeTryOnAreaPrefab.IsNull() || CustomMapLoader.instance.compositeTryOnArea.IsNull())
			{
				return;
			}
			if (CustomMapLoader.storeTryOnAreas.Count >= Constants.storeTryOnAreaLimit)
			{
				Object.Destroy(component);
				return;
			}
			GameObject gameObject10 = Object.Instantiate<GameObject>(CustomMapLoader.instance.storeTryOnAreaPrefab, placeholderGameObject.transform);
			gameObject10.transform.SetParent(CustomMapLoader.instance.compositeTryOnArea.transform);
			CMSTryOnArea component9 = gameObject10.GetComponent<CMSTryOnArea>();
			if (component9.IsNull() || component9.tryOnAreaCollider.IsNull())
			{
				Object.DestroyImmediate(gameObject10);
				return;
			}
			BoxCollider tryOnAreaCollider = component9.tryOnAreaCollider;
			Vector3 zero = Vector3.zero;
			zero.x = tryOnAreaCollider.size.x * tryOnAreaCollider.transform.lossyScale.x;
			zero.y = tryOnAreaCollider.size.y * tryOnAreaCollider.transform.lossyScale.y;
			zero.z = tryOnAreaCollider.size.z * tryOnAreaCollider.transform.lossyScale.z;
			if (Math.Abs(zero.x * zero.y * zero.z) > Constants.storeTryOnAreaVolumeLimit)
			{
				Object.DestroyImmediate(gameObject10);
				return;
			}
			component9.InitializeForCustomMap(CustomMapLoader.instance.compositeTryOnArea, placeholderGameObject.scene);
			CustomMapLoader.storeTryOnAreas.Add(gameObject10);
			CustomMapLoader.placeholderReplacements.Add(gameObject10);
			break;
		}
		case GTObject.Store_Checkout:
		{
			if (CustomMapLoader.instance.storeCheckoutCounterPrefab.IsNull())
			{
				return;
			}
			if (CustomMapLoader.storeCheckouts.Count >= Constants.storeCheckoutCounterLimit)
			{
				Object.Destroy(component);
				return;
			}
			if (placeholderGameObject.transform.lossyScale != Vector3.one)
			{
				Object.Destroy(component);
				return;
			}
			if (!CustomMapLoader.ValidateStorePlaceholderPosition(placeholderGameObject))
			{
				Object.Destroy(component);
				return;
			}
			GameObject gameObject11 = Object.Instantiate<GameObject>(CustomMapLoader.instance.storeCheckoutCounterPrefab, placeholderGameObject.transform);
			if (gameObject11.IsNull())
			{
				return;
			}
			gameObject11.transform.SetParent(CustomMapLoader.instance.compositeTryOnArea.transform);
			gameObject11.transform.localScale = Vector3.one;
			ItemCheckout componentInChildren2 = gameObject11.GetComponentInChildren<ItemCheckout>();
			if (componentInChildren2.IsNull())
			{
				Object.DestroyImmediate(gameObject11);
				return;
			}
			componentInChildren2.InitializeForCustomMap(CustomMapLoader.instance.compositeTryOnArea, placeholderGameObject.scene, component.useCustomMesh);
			CustomMapLoader.storeCheckouts.Add(gameObject11);
			CustomMapLoader.placeholderReplacements.Add(gameObject11);
			return;
		}
		case GTObject.Store_TryOnConsole:
		{
			if (CustomMapLoader.instance.storeTryOnConsolePrefab.IsNull())
			{
				return;
			}
			if (CustomMapLoader.storeTryOnConsoles.Count >= Constants.storeTryOnConsoleLimit)
			{
				Object.Destroy(component);
				return;
			}
			GameObject gameObject12 = Object.Instantiate<GameObject>(CustomMapLoader.instance.storeTryOnConsolePrefab, placeholderGameObject.transform);
			if (gameObject12.IsNull())
			{
				return;
			}
			FittingRoom componentInChildren3 = gameObject12.GetComponentInChildren<FittingRoom>();
			if (componentInChildren3.IsNull())
			{
				Object.DestroyImmediate(gameObject12);
				return;
			}
			componentInChildren3.InitializeForCustomMap(component.useCustomMesh);
			CustomMapLoader.storeTryOnConsoles.Add(gameObject12);
			CustomMapLoader.placeholderReplacements.Add(gameObject12);
			return;
		}
		default:
			return;
		}
	}

	private static void SetupDynamicLight(GameObject dynamicLightGameObject)
	{
		if (dynamicLightGameObject.IsNull())
		{
			return;
		}
		UberShaderDynamicLight component = dynamicLightGameObject.GetComponent<UberShaderDynamicLight>();
		if (component.IsNull())
		{
			return;
		}
		if (component.dynamicLight.IsNull())
		{
			return;
		}
		GameObject gameObject = new GameObject(dynamicLightGameObject.name + "GameLight");
		GameLight gameLight = gameObject.AddComponent<GameLight>();
		gameLight.light = component.dynamicLight;
		GameLightingManager.instance.AddGameLight(gameLight, false);
		gameObject.transform.SetParent(dynamicLightGameObject.transform.parent);
		gameObject.transform.position = component.transform.position;
	}

	private static void StoreMapEntity(GameObject entityGameObject)
	{
		if (entityGameObject.IsNull() || CustomMapsGameManager.instance.IsNull())
		{
			return;
		}
		MapEntity component = entityGameObject.GetComponent<MapEntity>();
		if (component.IsNull())
		{
			return;
		}
		if (component is AIAgent)
		{
			AIAgent aiagent = (AIAgent)component;
			if (!aiagent.IsNull())
			{
				string.Format(" | AgentID: {0}", aiagent.enemyTypeId);
			}
		}
		if (component.isTemplate)
		{
			return;
		}
		CustomMapLoader.entitiesToCreate.Add(component);
	}

	private static void CacheLightmaps()
	{
		CustomMapLoader.lightmaps = new LightmapData[LightmapSettings.lightmaps.Length];
		if (CustomMapLoader.lightmapsToKeep.Count > 0)
		{
			CustomMapLoader.lightmapsToKeep.Clear();
		}
		CustomMapLoader.lightmapsToKeep = new List<Texture2D>(LightmapSettings.lightmaps.Length * 2);
		for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
		{
			CustomMapLoader.lightmaps[i] = LightmapSettings.lightmaps[i];
			if (LightmapSettings.lightmaps[i].lightmapColor != null)
			{
				CustomMapLoader.lightmapsToKeep.Add(LightmapSettings.lightmaps[i].lightmapColor);
			}
			if (LightmapSettings.lightmaps[i].lightmapDir != null)
			{
				CustomMapLoader.lightmapsToKeep.Add(LightmapSettings.lightmaps[i].lightmapDir);
			}
		}
	}

	private static void LoadLightmaps(Texture2D[] colorMaps, Texture2D[] dirMaps)
	{
		if (colorMaps.Length == 0)
		{
			return;
		}
		CustomMapLoader.UnloadLightmaps();
		List<LightmapData> list = new List<LightmapData>(LightmapSettings.lightmaps);
		for (int i = 0; i < colorMaps.Length; i++)
		{
			bool flag = false;
			LightmapData lightmapData = new LightmapData();
			if (colorMaps[i] != null)
			{
				lightmapData.lightmapColor = colorMaps[i];
				flag = true;
				if (i < dirMaps.Length && dirMaps[i] != null)
				{
					lightmapData.lightmapDir = dirMaps[i];
				}
			}
			if (flag)
			{
				list.Add(lightmapData);
			}
		}
		LightmapSettings.lightmaps = list.ToArray();
	}

	public static void ResetToInitialZone(Action<string> onSceneLoaded, Action<string> onSceneUnloaded)
	{
		List<int> list = new List<int>(CustomMapLoader.initialSceneIndexes);
		List<int> list2 = new List<int>(CustomMapLoader.loadedSceneIndexes);
		foreach (int num in CustomMapLoader.loadedSceneIndexes)
		{
			if (CustomMapLoader.initialSceneIndexes.Contains(num))
			{
				list2.Remove(num);
				list.Remove(num);
			}
		}
		if (CustomMapLoader.loadedMapPackageInfo.customMapSupportVersion <= 2 && CustomMapLoader.loadedSceneIndexes.Contains(CustomMapLoader.initialSceneIndexes[0]))
		{
			MapDescriptor[] array = Object.FindObjectsByType<MapDescriptor>(FindObjectsSortMode.None);
			bool flag = false;
			int i;
			for (i = 0; i < array.Length; i++)
			{
				if (array[i].IsInitialScene && array[i].UseUberShaderDynamicLighting)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
				GameLightingManager.instance.SetAmbientLightDynamic(array[i].UberShaderAmbientDynamicLight);
				CustomMapLoader.usingDynamicLighting = true;
			}
			else
			{
				GameLightingManager.instance.SetCustomDynamicLightingEnabled(false);
				GameLightingManager.instance.SetAmbientLightDynamic(Color.black);
				CustomMapLoader.usingDynamicLighting = false;
			}
		}
		else if (CustomMapLoader.loadedMapPackageInfo.customMapSupportVersion > 2)
		{
			if (CustomMapLoader.loadedMapPackageInfo.useUberShaderDynamicLighting)
			{
				Color color = new Color(CustomMapLoader.loadedMapPackageInfo.uberShaderAmbientDynamicLight_R, CustomMapLoader.loadedMapPackageInfo.uberShaderAmbientDynamicLight_G, CustomMapLoader.loadedMapPackageInfo.uberShaderAmbientDynamicLight_B, CustomMapLoader.loadedMapPackageInfo.uberShaderAmbientDynamicLight_A);
				GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
				GameLightingManager.instance.SetAmbientLightDynamic(color);
				CustomMapLoader.usingDynamicLighting = true;
			}
			else
			{
				GameLightingManager.instance.SetCustomDynamicLightingEnabled(false);
				GameLightingManager.instance.SetAmbientLightDynamic(Color.black);
				CustomMapLoader.usingDynamicLighting = false;
			}
		}
		if (list.IsNullOrEmpty<int>() && list2.IsNullOrEmpty<int>())
		{
			return;
		}
		if (CustomMapLoader.zoneLoadingCoroutine != null)
		{
			CustomMapLoader.LoadZoneRequest loadZoneRequest = new CustomMapLoader.LoadZoneRequest
			{
				sceneIndexesToLoad = list.ToArray(),
				sceneIndexesToUnload = list2.ToArray(),
				onSceneLoadedCallback = onSceneLoaded,
				onSceneUnloadedCallback = onSceneUnloaded
			};
			CustomMapLoader.queuedLoadZoneRequests.Add(loadZoneRequest);
			return;
		}
		CustomMapLoader.sceneLoadedCallback = onSceneLoaded;
		CustomMapLoader.sceneUnloadedCallback = onSceneUnloaded;
		CustomMapLoader.zoneLoadingCoroutine = CustomMapLoader.instance.StartCoroutine(CustomMapLoader.LoadZoneCoroutine(list.ToArray(), list2.ToArray()));
	}

	public static void LoadZoneTriggered(int[] loadSceneIndexes, int[] unloadSceneIndexes, Action<string> onSceneLoaded, Action<string> onSceneUnloaded)
	{
		string text = "";
		for (int i = 0; i < loadSceneIndexes.Length; i++)
		{
			text += loadSceneIndexes[i].ToString();
			if (i != loadSceneIndexes.Length - 1)
			{
				text += ", ";
			}
		}
		string text2 = "";
		for (int j = 0; j < unloadSceneIndexes.Length; j++)
		{
			text2 += unloadSceneIndexes[j].ToString();
			if (j != unloadSceneIndexes.Length - 1)
			{
				text2 += ", ";
			}
		}
		if (CustomMapLoader.zoneLoadingCoroutine != null)
		{
			CustomMapLoader.LoadZoneRequest loadZoneRequest = new CustomMapLoader.LoadZoneRequest
			{
				sceneIndexesToLoad = loadSceneIndexes,
				sceneIndexesToUnload = unloadSceneIndexes,
				onSceneLoadedCallback = onSceneLoaded,
				onSceneUnloadedCallback = onSceneUnloaded
			};
			CustomMapLoader.queuedLoadZoneRequests.Add(loadZoneRequest);
			return;
		}
		CustomMapLoader.sceneLoadedCallback = onSceneLoaded;
		CustomMapLoader.sceneUnloadedCallback = onSceneUnloaded;
		CustomMapLoader.zoneLoadingCoroutine = CustomMapLoader.instance.StartCoroutine(CustomMapLoader.LoadZoneCoroutine(loadSceneIndexes, unloadSceneIndexes));
	}

	private static IEnumerator LoadZoneCoroutine(int[] loadScenes, int[] unloadScenes)
	{
		if (!unloadScenes.IsNullOrEmpty<int>())
		{
			yield return CustomMapLoader.UnloadScenesCoroutine(unloadScenes);
		}
		if (!loadScenes.IsNullOrEmpty<int>())
		{
			yield return CustomMapLoader.LoadScenesCoroutine(loadScenes, delegate(bool successfullyLoadedAllScenes, bool loadAborted, List<string> successfullyLoadedSceneNames)
			{
				if (loadAborted)
				{
					CustomMapLoader.queuedLoadZoneRequests.Clear();
				}
			});
		}
		CustomMapLoader.zoneLoadingCoroutine = null;
		if (CustomMapLoader.queuedLoadZoneRequests.Count > 0)
		{
			CustomMapLoader.LoadZoneRequest loadZoneRequest = CustomMapLoader.queuedLoadZoneRequests[0];
			CustomMapLoader.queuedLoadZoneRequests.RemoveAt(0);
			CustomMapLoader.LoadZoneTriggered(loadZoneRequest.sceneIndexesToLoad, loadZoneRequest.sceneIndexesToUnload, loadZoneRequest.onSceneLoadedCallback, loadZoneRequest.onSceneUnloadedCallback);
		}
		yield break;
	}

	public static void CloseDoorAndUnloadMap(Action unloadCompleted = null)
	{
		if (!CustomMapLoader.IsMapLoaded() && !CustomMapLoader.isLoading)
		{
			return;
		}
		if (unloadCompleted != null)
		{
			CustomMapLoader.unloadMapCallback = unloadCompleted;
		}
		if (CustomMapLoader.isLoading)
		{
			CustomMapLoader.RequestAbortMapLoad();
			return;
		}
		CustomMapLoader.instance.StartCoroutine(CustomMapLoader.CloseDoorAndUnloadMapCoroutine());
	}

	private static IEnumerator CloseDoorAndUnloadMapCoroutine()
	{
		if (!CustomMapLoader.IsMapLoaded())
		{
			yield break;
		}
		if (CustomMapLoader.instance.accessDoor != null)
		{
			CustomMapLoader.instance.accessDoor.CloseDoor();
		}
		if (CustomMapLoader.instance.publicJoinTrigger != null)
		{
			CustomMapLoader.instance.publicJoinTrigger.SetActive(false);
		}
		CustomMapLoader.shouldAbortMapLoading = true;
		if (CustomMapLoader.IsLoading())
		{
			yield break;
		}
		yield return CustomMapLoader.UnloadMapCoroutine();
		yield break;
	}

	private static void RequestAbortMapLoad()
	{
		CustomMapLoader.shouldAbortSceneLoad = true;
		CustomMapLoader.shouldAbortMapLoading = true;
	}

	private static IEnumerator AbortMapLoad()
	{
		GTDev.Log<string>("[CML.AbortMapLoad] Aborting map load...", null);
		CustomMapLoader.shouldAbortSceneLoad = true;
		CustomMapLoader.shouldAbortMapLoading = true;
		yield return CustomMapLoader.AbortSceneLoad(-1);
		Action<bool> action = CustomMapLoader.mapLoadFinishedCallback;
		if (action != null)
		{
			action(false);
		}
		yield break;
	}

	private static IEnumerator UnloadMapCoroutine()
	{
		GTDev.Log<string>("[CML.UnloadMap_Co] Unloading Custom Map...", null);
		if (CustomMapLoader.zoneLoadingCoroutine != null)
		{
			CustomMapLoader.queuedLoadZoneRequests.Clear();
			CustomMapLoader.instance.StopCoroutine(CustomMapLoader.zoneLoadingCoroutine);
			CustomMapLoader.zoneLoadingCoroutine = null;
		}
		CustomMapLoader.isUnloading = true;
		CustomMapLoader.CanLoadEntities = false;
		CustomMapTelemetry.EndMapTracking();
		ZoneShaderSettings.ActivateDefaultSettings();
		CustomMapLoader.CleanupPlaceholders();
		CMSSerializer.ResetSyncedMapObjects();
		CustomMapLoader.instance.ghostReactorManager.reactor.RefreshReviveStations(false);
		if (!CustomMapLoader.assetBundleSceneFilePaths.IsNullOrEmpty<string>())
		{
			int num;
			for (int sceneIndex = 0; sceneIndex < CustomMapLoader.assetBundleSceneFilePaths.Length; sceneIndex = num + 1)
			{
				yield return CustomMapLoader.UnloadSceneCoroutine(sceneIndex, null);
				num = sceneIndex;
			}
		}
		GorillaNetworkJoinTrigger.EnableTriggerJoins();
		LightmapSettings.lightmaps = CustomMapLoader.lightmaps;
		CustomMapLoader.UnloadLightmaps();
		yield return CustomMapLoader.ResetLightmaps();
		CustomMapLoader.usingDynamicLighting = false;
		GameLightingManager.instance.SetCustomDynamicLightingEnabled(false);
		GameLightingManager.instance.SetAmbientLightDynamic(Color.black);
		if (CustomMapLoader.mapBundle != null)
		{
			CustomMapLoader.mapBundle.Unload(true);
		}
		CustomMapLoader.mapBundle = null;
		Resources.UnloadUnusedAssets();
		CustomMapLoader.cachedLuauScript = "";
		CustomMapLoader.devModeEnabled = false;
		CustomMapLoader.disableHoldingHandsAllModes = false;
		CustomMapLoader.disableHoldingHandsCustomMode = false;
		CustomMapLoader.queuedLoadZoneRequests.Clear();
		CustomMapLoader.assetBundleSceneFilePaths = new string[] { "" };
		CustomMapLoader.loadedMapPackageInfo = null;
		CustomMapLoader.loadedMapModId = 0L;
		CustomMapLoader.loadedSceneFilePaths.Clear();
		CustomMapLoader.loadedSceneNames.Clear();
		CustomMapLoader.loadedSceneIndexes.Clear();
		CustomMapLoader.initialSceneIndexes.Clear();
		CustomMapLoader.initialSceneNames.Clear();
		CustomMapLoader.maxPlayersForMap = 10;
		CustomMapModeSelector.ResetButtons();
		if (RoomSystem.JoinedRoom && NetworkSystem.Instance.LocalPlayer.IsMasterClient && NetworkSystem.Instance.SessionIsPrivate)
		{
			if (GameMode.ActiveGameMode.IsNull())
			{
				GameMode.ChangeGameMode(GameModeType.Casual.ToString());
			}
			else if (GameMode.ActiveGameMode.GameType() != GameModeType.Casual)
			{
				GameMode.ChangeGameMode(GameModeType.Casual.ToString());
			}
		}
		CustomMapLoader.shouldAbortMapLoading = false;
		CustomMapLoader.shouldAbortSceneLoad = false;
		CustomMapLoader.isUnloading = false;
		if (CustomMapLoader.unloadMapCallback != null)
		{
			Action action = CustomMapLoader.unloadMapCallback;
			if (action != null)
			{
				action();
			}
			CustomMapLoader.unloadMapCallback = null;
		}
		yield break;
	}

	private static IEnumerator AbortSceneLoad(int sceneIndex)
	{
		if (sceneIndex == -1)
		{
			CustomMapLoader.shouldAbortMapLoading = true;
		}
		CustomMapLoader.isLoading = false;
		if (CustomMapLoader.shouldAbortMapLoading)
		{
			yield return CustomMapLoader.UnloadMapCoroutine();
		}
		else
		{
			yield return CustomMapLoader.UnloadSceneCoroutine(sceneIndex, null);
		}
		CustomMapLoader.shouldAbortSceneLoad = false;
		yield break;
	}

	private static IEnumerator UnloadScenesCoroutine(int[] sceneIndexes)
	{
		int num;
		for (int i = 0; i < sceneIndexes.Length; i = num + 1)
		{
			yield return CustomMapLoader.UnloadSceneCoroutine(sceneIndexes[i], null);
			num = i;
		}
		yield break;
	}

	private static IEnumerator UnloadSceneCoroutine(int sceneIndex, Action OnUnloadComplete = null)
	{
		if (!CustomMapLoader.hasInstance)
		{
			yield break;
		}
		if (sceneIndex < 0 || sceneIndex >= CustomMapLoader.assetBundleSceneFilePaths.Length)
		{
			Debug.LogError(string.Format("[CustomMapLoader::UnloadSceneCoroutine] SceneIndex of {0} is invalid! ", sceneIndex) + string.Format("The currently loaded AssetBundle contains {0} scenes.", CustomMapLoader.assetBundleSceneFilePaths.Length));
			yield break;
		}
		while (CustomMapLoader.runningAsyncLoad)
		{
			yield return null;
		}
		UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.UnloadAllEmbeddedSceneObjects;
		string scenePathWithExtension = CustomMapLoader.assetBundleSceneFilePaths[sceneIndex];
		string[] array = scenePathWithExtension.Split(".", StringSplitOptions.None);
		string text = "";
		string sceneName = "";
		if (!array.IsNullOrEmpty<string>())
		{
			text = array[0];
			if (text.Length > 0)
			{
				sceneName = Path.GetFileName(text);
			}
		}
		Scene sceneByName = SceneManager.GetSceneByName(text);
		if (sceneByName.IsValid())
		{
			CustomMapLoader.RemoveUnloadingStorePrefabs(sceneByName);
			for (int i = CustomMapLoader.teleporters.Count - 1; i >= 0; i--)
			{
				if (CustomMapLoader.teleporters[i].gameObject.scene == sceneByName)
				{
					CustomMapLoader.teleporters.RemoveAt(i);
				}
			}
			AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(scenePathWithExtension, unloadSceneOptions);
			yield return asyncOperation;
			CustomMapLoader.loadedSceneFilePaths.Remove(scenePathWithExtension);
			CustomMapLoader.loadedSceneNames.Remove(sceneName);
			CustomMapLoader.loadedSceneIndexes.Remove(sceneIndex);
			Action<string> action = CustomMapLoader.sceneUnloadedCallback;
			if (action != null)
			{
				action(sceneName);
			}
			if (OnUnloadComplete != null)
			{
				OnUnloadComplete();
			}
			yield break;
		}
		yield break;
	}

	private static void RemoveUnloadingStorePrefabs(Scene unloadingScene)
	{
		if (CustomMapLoader.customMapATM.IsNotNull())
		{
			ATM_UI componentInChildren = CustomMapLoader.customMapATM.GetComponentInChildren<ATM_UI>();
			if (componentInChildren.IsNotNull() && componentInChildren.IsFromCustomMapScene(unloadingScene) && ATM_Manager.instance.IsNotNull())
			{
				ATM_Manager.instance.RemoveATM(componentInChildren);
				ATM_Manager.instance.ResetTemporaryCreatorCode();
			}
			Object.Destroy(CustomMapLoader.customMapATM);
			CustomMapLoader.customMapATM = null;
		}
		for (int i = CustomMapLoader.storeDisplayStands.Count - 1; i >= 0; i--)
		{
			if (CustomMapLoader.storeDisplayStands[i].IsNull())
			{
				CustomMapLoader.storeDisplayStands.RemoveAt(i);
			}
			else
			{
				DynamicCosmeticStand componentInChildren2 = CustomMapLoader.storeDisplayStands[i].GetComponentInChildren<DynamicCosmeticStand>();
				if (componentInChildren2.IsNotNull() && componentInChildren2.IsFromCustomMapScene(unloadingScene))
				{
					if (componentInChildren2.IsNotNull())
					{
						StoreController.instance.RemoveStandFromPlayFabIDDictionary(componentInChildren2);
					}
					Object.Destroy(CustomMapLoader.storeDisplayStands[i]);
					CustomMapLoader.storeDisplayStands.RemoveAt(i);
				}
			}
		}
		for (int i = CustomMapLoader.storeCheckouts.Count - 1; i >= 0; i--)
		{
			if (CustomMapLoader.storeCheckouts[i].IsNull())
			{
				CustomMapLoader.storeCheckouts.RemoveAt(i);
			}
			else
			{
				ItemCheckout componentInChildren3 = CustomMapLoader.storeCheckouts[i].GetComponentInChildren<ItemCheckout>();
				if (componentInChildren3.IsNotNull() && componentInChildren3.IsFromScene(unloadingScene))
				{
					componentInChildren3.RemoveFromCustomMap(CustomMapLoader.instance.compositeTryOnArea);
					CosmeticsController.instance.RemoveItemCheckout(componentInChildren3);
					Object.Destroy(CustomMapLoader.storeCheckouts[i]);
					CustomMapLoader.storeCheckouts.RemoveAt(i);
				}
			}
		}
		for (int i = CustomMapLoader.storeTryOnConsoles.Count - 1; i >= 0; i--)
		{
			if (CustomMapLoader.storeTryOnConsoles[i].IsNull())
			{
				CustomMapLoader.storeTryOnConsoles.RemoveAt(i);
			}
			else if (CustomMapLoader.storeTryOnConsoles[i].scene.Equals(unloadingScene))
			{
				FittingRoom componentInChildren4 = CustomMapLoader.storeTryOnConsoles[i].GetComponentInChildren<FittingRoom>();
				if (componentInChildren4.IsNotNull())
				{
					CosmeticsController.instance.RemoveFittingRoom(componentInChildren4);
				}
				CustomMapLoader.storeTryOnConsoles.RemoveAt(i);
			}
		}
		for (int i = CustomMapLoader.storeTryOnAreas.Count - 1; i >= 0; i--)
		{
			if (CustomMapLoader.storeTryOnAreas[i].IsNull())
			{
				CustomMapLoader.storeTryOnAreas.RemoveAt(i);
			}
			else
			{
				CMSTryOnArea component = CustomMapLoader.storeTryOnAreas[i].GetComponent<CMSTryOnArea>();
				if (component.IsNotNull() && component.IsFromScene(unloadingScene))
				{
					component.RemoveFromCustomMap(CustomMapLoader.instance.compositeTryOnArea);
					Object.Destroy(CustomMapLoader.storeTryOnAreas[i]);
					CustomMapLoader.storeTryOnAreas.RemoveAt(i);
				}
			}
		}
	}

	private static void CleanupPlaceholders()
	{
		for (int i = 0; i < CustomMapLoader.instance.leafGliders.Length; i++)
		{
			CustomMapLoader.instance.leafGliders[i].CustomMapUnload();
			CustomMapLoader.instance.leafGliders[i].enabled = false;
			CustomMapLoader.instance.leafGliders[i].transform.GetChild(0).gameObject.SetActive(false);
		}
	}

	private static IEnumerator ResetLightmaps()
	{
		CustomMapLoader.instance.dayNightManager.RequestRepopulateLightmaps();
		LoadSceneParameters loadSceneParameters = new LoadSceneParameters
		{
			loadSceneMode = LoadSceneMode.Additive,
			localPhysicsMode = LocalPhysicsMode.None
		};
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(10, loadSceneParameters);
		yield return asyncOperation;
		asyncOperation = SceneManager.UnloadSceneAsync(10);
		yield return asyncOperation;
		yield break;
	}

	private static void UnloadLightmaps()
	{
		foreach (LightmapData lightmapData in LightmapSettings.lightmaps)
		{
			if (lightmapData.lightmapColor != null && !CustomMapLoader.lightmapsToKeep.Contains(lightmapData.lightmapColor))
			{
				Resources.UnloadAsset(lightmapData.lightmapColor);
			}
			if (lightmapData.lightmapDir != null && !CustomMapLoader.lightmapsToKeep.Contains(lightmapData.lightmapDir))
			{
				Resources.UnloadAsset(lightmapData.lightmapDir);
			}
		}
	}

	private static int GetSceneIndex(string sceneName)
	{
		int num = -1;
		if (CustomMapLoader.assetBundleSceneFilePaths.Length == 1)
		{
			return 0;
		}
		for (int i = 0; i < CustomMapLoader.assetBundleSceneFilePaths.Length; i++)
		{
			string sceneNameFromFilePath = CustomMapLoader.GetSceneNameFromFilePath(CustomMapLoader.assetBundleSceneFilePaths[i]);
			if (sceneNameFromFilePath != null && sceneNameFromFilePath.Equals(sceneName))
			{
				num = i;
				break;
			}
		}
		return num;
	}

	private static string GetSceneNameFromFilePath(string filePath)
	{
		string[] array = filePath.Split("/", StringSplitOptions.None);
		return array[array.Length - 1].Split(".", StringSplitOptions.None)[0];
	}

	public static MapPackageInfo GetPackageInfo(string packageInfoFilePath)
	{
		MapPackageInfo mapPackageInfo;
		using (StreamReader streamReader = new StreamReader(File.OpenRead(packageInfoFilePath), Encoding.Default))
		{
			mapPackageInfo = JsonConvert.DeserializeObject<MapPackageInfo>(streamReader.ReadToEnd());
		}
		return mapPackageInfo;
	}

	public static ModId LoadedMapModId
	{
		get
		{
			return CustomMapLoader.loadedMapModId;
		}
	}

	public static long LoadedMapModFileId
	{
		get
		{
			return CustomMapLoader.loadedMapModFileId;
		}
	}

	public static bool CanLoadEntities { get; private set; }

	public static bool IsMapLoaded()
	{
		return CustomMapLoader.IsMapLoaded(ModId.Null);
	}

	public static bool IsMapLoaded(ModId mapModId)
	{
		if (mapModId.IsValid())
		{
			return !CustomMapLoader.IsLoading() && CustomMapLoader.LoadedMapModId == mapModId;
		}
		return !CustomMapLoader.IsLoading() && CustomMapLoader.LoadedMapModId.IsValid();
	}

	public static bool IsLoading()
	{
		return CustomMapLoader.isLoading;
	}

	public static long GetLoadingMapModId()
	{
		return CustomMapLoader.attemptedLoadID;
	}

	public static byte GetRoomSizeForCurrentlyLoadedMap()
	{
		if (!CustomMapLoader.IsMapLoaded())
		{
			return 10;
		}
		return CustomMapLoader.maxPlayersForMap;
	}

	public static bool IsCustomScene(string sceneName)
	{
		return CustomMapLoader.loadedSceneNames.Contains(sceneName);
	}

	public static string GetLuauGamemodeScript()
	{
		if (!CustomMapLoader.IsMapLoaded())
		{
			return "";
		}
		return CustomMapLoader.cachedLuauScript;
	}

	public static bool IsDevModeEnabled()
	{
		return CustomMapLoader.IsMapLoaded() && CustomMapLoader.devModeEnabled;
	}

	public static Transform GetCustomMapsDefaultSpawnLocation()
	{
		if (CustomMapLoader.hasInstance)
		{
			return CustomMapLoader.instance.CustomMapsDefaultSpawnLocation;
		}
		return null;
	}

	public static bool LoadedMapWantsHoldingHandsDisabled()
	{
		return CustomMapLoader.IsMapLoaded() && (CustomMapLoader.disableHoldingHandsAllModes || (CustomMapLoader.disableHoldingHandsCustomMode && GorillaGameManager.instance.IsNotNull() && GorillaGameManager.instance.GameType() == GameModeType.Custom));
	}

	[OnEnterPlay_SetNull]
	private static volatile CustomMapLoader instance;

	[OnEnterPlay_Set(false)]
	private static bool hasInstance;

	public Transform CustomMapsDefaultSpawnLocation;

	public CustomMapAccessDoor accessDoor;

	[FormerlySerializedAs("networkTrigger")]
	public GameObject publicJoinTrigger;

	[SerializeField]
	private BetterDayNightManager dayNightManager;

	[SerializeField]
	private GhostReactorManager ghostReactorManager;

	[SerializeField]
	private GameObject placeholderParent;

	[SerializeField]
	private GliderHoldable[] leafGliders;

	[SerializeField]
	private GameObject leafGlider;

	[SerializeField]
	private GameObject gliderWindVolume;

	[FormerlySerializedAs("waterVolume")]
	[SerializeField]
	private GameObject waterVolumePrefab;

	[SerializeField]
	private WaterParameters defaultWaterParameters;

	[SerializeField]
	private WaterParameters defaultLavaParameters;

	[FormerlySerializedAs("forceVolume")]
	[SerializeField]
	private GameObject forceVolumePrefab;

	[SerializeField]
	private GameObject atmPrefab;

	[SerializeField]
	private GameObject atmNoShellPrefab;

	[SerializeField]
	private GameObject storeDisplayStandPrefab;

	[SerializeField]
	private GameObject storeCheckoutCounterPrefab;

	[SerializeField]
	private GameObject storeTryOnConsolePrefab;

	[SerializeField]
	private GameObject storeTryOnAreaPrefab;

	[SerializeField]
	private GameObject hoverboardDispenserPrefab;

	[SerializeField]
	private GameObject ropeSwingPrefab;

	[SerializeField]
	private GameObject ziplinePrefab;

	[SerializeField]
	private GameObject reviveStationPrefab;

	[SerializeField]
	private GameObject zoneShaderSettingsTrigger;

	[SerializeField]
	private AudioMixerGroup masterAudioMixer;

	[SerializeField]
	private ZoneShaderSettings customMapZoneShaderSettings;

	[SerializeField]
	private CompositeTriggerEvents compositeTryOnArea;

	[SerializeField]
	private GameObject virtualStumpMesh;

	[SerializeField]
	private List<GameModeType> availableModesForOldMaps = new List<GameModeType>
	{
		GameModeType.Infection,
		GameModeType.FreezeTag,
		GameModeType.Paintbrawl
	};

	[SerializeField]
	private GameModeType defaultGameModeForNonCustomOldMaps = GameModeType.Infection;

	public TMP_FontAsset DefaultFont;

	private static readonly int numObjectsToProcessPerFrame = 5;

	private static readonly List<int> APPROVED_LAYERS = new List<int>
	{
		0, 1, 2, 4, 5, 9, 11, 18, 20, 22,
		27, 30
	};

	private static bool isLoading;

	private static bool isUnloading;

	private static bool runningAsyncLoad = false;

	private static long attemptedLoadID = 0L;

	private static string attemptedSceneToLoad;

	private static bool shouldAbortMapLoading = false;

	private static bool shouldAbortSceneLoad = false;

	private static bool errorEncounteredDuringLoad = false;

	private static Action unloadMapCallback;

	private static string cachedExceptionMessage = "";

	private static AssetBundle mapBundle;

	private static List<string> initialSceneNames = new List<string>();

	private static List<int> initialSceneIndexes = new List<int>();

	private static byte maxPlayersForMap = 10;

	private static ModId loadedMapModId;

	private static long loadedMapModFileId;

	private static MapPackageInfo loadedMapPackageInfo;

	private static string cachedLuauScript;

	private static bool devModeEnabled;

	private static bool disableHoldingHandsAllModes;

	private static bool disableHoldingHandsCustomMode;

	private static Action<MapLoadStatus, int, string> mapLoadProgressCallback;

	private static Action<bool> mapLoadFinishedCallback;

	private static Coroutine zoneLoadingCoroutine;

	private static Action<string> sceneLoadedCallback;

	private static Action<string> sceneUnloadedCallback;

	private static List<CustomMapLoader.LoadZoneRequest> queuedLoadZoneRequests = new List<CustomMapLoader.LoadZoneRequest>();

	private static string[] assetBundleSceneFilePaths;

	private static List<string> loadedSceneFilePaths = new List<string>();

	private static List<string> loadedSceneNames = new List<string>();

	private static List<int> loadedSceneIndexes = new List<int>();

	private Coroutine loadScenesCoroutine;

	private static int leafGliderIndex;

	private static bool usingDynamicLighting = false;

	private static bool refreshReviveStations = false;

	private static int totalObjectsInLoadingScene = 0;

	private static int objectsProcessedForLoadingScene = 0;

	private static int objectsProcessedThisFrame = 0;

	private static List<Component> initializePhaseTwoComponents = new List<Component>();

	private static List<MapEntity> entitiesToCreate = new List<MapEntity>(Constants.aiAgentLimit);

	private static LightmapData[] lightmaps;

	private static List<Texture2D> lightmapsToKeep = new List<Texture2D>();

	private static List<GameObject> placeholderReplacements = new List<GameObject>();

	private static GameObject customMapATM = null;

	private static List<GameObject> storeCheckouts = new List<GameObject>();

	private static List<GameObject> storeDisplayStands = new List<GameObject>();

	private static List<GameObject> storeTryOnConsoles = new List<GameObject>();

	private static List<GameObject> storeTryOnAreas = new List<GameObject>();

	private static List<Component> teleporters = new List<Component>();

	private string dontDestroyOnLoadSceneName = "";

	private static readonly List<Type> componentAllowlist = new List<Type>
	{
		typeof(MeshRenderer),
		typeof(Transform),
		typeof(MeshFilter),
		typeof(MeshRenderer),
		typeof(Collider),
		typeof(BoxCollider),
		typeof(SphereCollider),
		typeof(CapsuleCollider),
		typeof(MeshCollider),
		typeof(Light),
		typeof(ReflectionProbe),
		typeof(AudioSource),
		typeof(Animator),
		typeof(SkinnedMeshRenderer),
		typeof(TextMesh),
		typeof(ParticleSystem),
		typeof(ParticleSystemRenderer),
		typeof(RectTransform),
		typeof(SpriteRenderer),
		typeof(BillboardRenderer),
		typeof(Canvas),
		typeof(CanvasRenderer),
		typeof(CanvasScaler),
		typeof(GraphicRaycaster),
		typeof(Rigidbody),
		typeof(TrailRenderer),
		typeof(LineRenderer),
		typeof(LensFlareComponentSRP),
		typeof(Camera),
		typeof(UniversalAdditionalCameraData),
		typeof(NavMeshAgent),
		typeof(NavMesh),
		typeof(NavMeshObstacle),
		typeof(NavMeshLink),
		typeof(NavMeshModifierVolume),
		typeof(NavMeshModifier),
		typeof(NavMeshSurface),
		typeof(HingeJoint),
		typeof(ConstantForce),
		typeof(LODGroup),
		typeof(MapDescriptor),
		typeof(AccessDoorPlaceholder),
		typeof(MapOrientationPoint),
		typeof(SurfaceOverrideSettings),
		typeof(TeleporterSettings),
		typeof(TagZoneSettings),
		typeof(LuauTriggerSettings),
		typeof(MapBoundarySettings),
		typeof(ObjectActivationTriggerSettings),
		typeof(LoadZoneSettings),
		typeof(GTObjectPlaceholder),
		typeof(CMSZoneShaderSettings),
		typeof(ZoneShaderTriggerSettings),
		typeof(MultiPartFire),
		typeof(HandHoldSettings),
		typeof(CustomMapEjectButtonSettings),
		typeof(global::CustomMapSupport.BezierSpline),
		typeof(UberShaderDynamicLight),
		typeof(MapEntity),
		typeof(GrabbableEntity),
		typeof(AIAgent),
		typeof(AISpawnManager),
		typeof(AISpawnPoint),
		typeof(MapSpawnPoint),
		typeof(MapSpawnManager),
		typeof(RopeSwingSegment),
		typeof(ZiplineSegment),
		typeof(PlayAnimationTriggerSettings),
		typeof(SurfaceMoverSettings),
		typeof(MovingSurfaceSettings),
		typeof(CustomMapReviveStation),
		typeof(ProBuilderMesh),
		typeof(TMP_Text),
		typeof(TextMeshPro),
		typeof(TextMeshProUGUI),
		typeof(UniversalAdditionalLightData),
		typeof(BakerySkyLight),
		typeof(BakeryDirectLight),
		typeof(BakeryPointLight),
		typeof(ftLightmapsStorage),
		typeof(BakeryAlwaysRender),
		typeof(BakeryLightMesh),
		typeof(BakeryLightmapGroupSelector),
		typeof(BakeryPackAsSingleSquare),
		typeof(BakerySector),
		typeof(BakeryVolume),
		typeof(BakeryLightmapGroup)
	};

	private static readonly List<string> componentTypeStringAllowList = new List<string> { "UnityEngine.Halo" };

	private static readonly Type[] badComponents = new Type[]
	{
		typeof(EventTrigger),
		typeof(UIBehaviour),
		typeof(GorillaPressableButton),
		typeof(GorillaPressableDelayButton),
		typeof(Camera),
		typeof(AudioListener),
		typeof(VideoPlayer)
	};

	private struct LoadZoneRequest
	{
		public int[] sceneIndexesToLoad;

		public int[] sceneIndexesToUnload;

		public Action<string> onSceneLoadedCallback;

		public Action<string> onSceneUnloadedCallback;
	}
}
