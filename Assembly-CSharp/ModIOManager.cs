using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GorillaNetworking;
using GorillaTagScripts.VirtualStumpCustomMaps;
using GorillaTagScripts.VirtualStumpCustomMaps.ModIO;
using GT_CustomMapSupportRuntime;
using Modio;
using Modio.API;
using Modio.Authentication;
using Modio.Customizations;
using Modio.Errors;
using Modio.FileIO;
using Modio.Mods;
using Modio.Unity;
using Modio.Users;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ModIOManager : MonoBehaviour, ISteamCredentialProvider, IOculusCredentialProvider
{
	private void Awake()
	{
		if (ModIOManager.instance == null)
		{
			ModIOManager.instance = this;
			ModIOManager.hasInstance = true;
			UGCPermissionManager.SubscribeToUGCEnabled(new Action(ModIOManager.OnUGCEnabled));
			UGCPermissionManager.SubscribeToUGCDisabled(new Action(ModIOManager.OnUGCDisabled));
			ModioServices.Bind<IModioAuthService>().FromInstance(ModIOManager.accountLinkingAuthService, (ModioServicePriority)41, null);
			ModioServices.Bind<IModioAuthService>().FromInstance(ModIOManager.steamAuthService, ModioServicePriority.DeveloperOverride, null);
			long gameId = ModioServices.Resolve<ModioSettings>().GameId;
			ModIOManager.ModIODirectory = Path.Combine(ModioServices.Resolve<IModioRootPathProvider>().Path, "mod.io", gameId.ToString()) + Path.DirectorySeparatorChar.ToString();
			return;
		}
		if (ModIOManager.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		NetworkSystem.Instance.OnMultiplayerStarted += this.OnJoinedRoom;
	}

	private void OnDestroy()
	{
		if (ModIOManager.instance == this)
		{
			ModIOManager.instance = null;
			ModIOManager.hasInstance = false;
			UGCPermissionManager.UnsubscribeFromUGCEnabled(new Action(ModIOManager.OnUGCEnabled));
			UGCPermissionManager.UnsubscribeFromUGCDisabled(new Action(ModIOManager.OnUGCDisabled));
		}
		NetworkSystem.Instance.OnMultiplayerStarted -= this.OnJoinedRoom;
	}

	private void Update()
	{
		bool flag = ModIOManager.hasInstance;
	}

	private static void OnUGCEnabled()
	{
	}

	private static void OnUGCDisabled()
	{
	}

	public static bool IsInitialized()
	{
		return ModIOManager.initialized;
	}

	public static async Task<Error> Initialize()
	{
		GTDev.Log<string>("[ModIOManager::Initialize] Initializing mo.io...", null);
		Error error;
		if (UGCPermissionManager.IsUGCDisabled)
		{
			GTDev.Log<string>("[ModIOManager::Initialize] Not initializing, UGC is disabled by K-ID", null);
			error = new Error(ErrorCode.UNKNOWN, "MOD.IO FUNCTIONALITY IS CURRENTLY DISABLED.");
		}
		else if (ModIOManager.initialized)
		{
			GTDev.Log<string>("[ModIOManager::Initialize] Already initialized", null);
			error = Error.None;
		}
		else
		{
			error = await ModIOManager.InitInternal();
		}
		return error;
	}

	private static async Task<Error> InitInternal()
	{
		GTDev.Log<string>("[ModIOManager::InitInternal] Initializing mod.io...", null);
		Error error;
		if (UGCPermissionManager.IsUGCDisabled)
		{
			GTDev.Log<string>("[ModIOManager::InitInternal] Not initializing, UGC is disabled by K-ID", null);
			error = new Error(ErrorCode.UNKNOWN, "MOD.IO FUNCTIONALITY IS CURRENTLY DISABLED.");
		}
		else if (ModIOManager.initialized)
		{
			GTDev.Log<string>("[ModIOManager::InitInternal] Already initialized", null);
			error = Error.None;
		}
		else
		{
			User.OnUserChanged -= ModIOManager.ModIOUserChanged;
			User.OnUserChanged += ModIOManager.ModIOUserChanged;
			User.OnUserSyncComplete -= ModIOManager.ModIOUserSyncComplete;
			User.OnUserSyncComplete += ModIOManager.ModIOUserSyncComplete;
			Error error2 = await ModioClient.Init();
			if (error2)
			{
				ModioLog error3 = ModioLog.Error;
				if (error3 != null)
				{
					error3.Log(string.Format("[ModIOManager::InitInternal] Error initializing mod.io: {0}", error2));
				}
				error = error2;
			}
			else
			{
				ModIOManager.EnableModManagement();
				ModIOManager.initialized = true;
				GTDev.Log<string>("[ModIOManager::InitInternal] ModIO plugin initialized!", null);
				await ModIOManager.GetFavoriteMods(false);
				error = Error.None;
			}
		}
		return error;
	}

	private async Task<ValueTuple<Error, bool, bool>> HasAcceptedLatestTerms()
	{
		ValueTuple<Error, bool, bool> valueTuple;
		if (!ModIOManager.initialized)
		{
			ModioLog error = ModioLog.Error;
			if (error != null)
			{
				error.Log("[ModIOManager] HasAcceptedLatestTerms called before ModIO has been initialized!");
			}
			valueTuple = new ValueTuple<Error, bool, bool>(new Error(ErrorCode.NOT_INITIALIZED, "ModIOManager has not been initialized!"), false, false);
		}
		else
		{
			ModioLog verbose = ModioLog.Verbose;
			if (verbose != null)
			{
				verbose.Log("[ModIOManager::HasAcceptedLatestTerms] Retrieving terms of use from mod.io...");
			}
			ValueTuple<Error, Agreement> valueTuple2 = await Agreement.GetAgreement(AgreementType.TermsOfUse, false);
			Error item = valueTuple2.Item1;
			Agreement fullTermsOfUse = valueTuple2.Item2;
			if (item)
			{
				ModioLog error2 = ModioLog.Error;
				if (error2 != null)
				{
					error2.Log(string.Format("[ModIOManager::HasAcceptedLatestTerms] Failed to get Mod.io Terms of Use: {0} ", item));
				}
				valueTuple = new ValueTuple<Error, bool, bool>(item, false, false);
			}
			else
			{
				ValueTuple<Error, Agreement> valueTuple3 = await Agreement.GetAgreement(AgreementType.PrivacyPolicy, false);
				Error item2 = valueTuple3.Item1;
				Agreement item3 = valueTuple3.Item2;
				if (item2)
				{
					ModioLog error3 = ModioLog.Error;
					if (error3 != null)
					{
						error3.Log("[ModIOManager::HasAcceptedLatestTerms] Failed to get Mod.io Privacy Policy: " + string.Format("{0} ", item2));
					}
					valueTuple = new ValueTuple<Error, bool, bool>(item2, false, false);
				}
				else
				{
					ModioLog verbose2 = ModioLog.Verbose;
					if (verbose2 != null)
					{
						verbose2.Log("[ModIOManager::OnReceivedTermsOfUse] retrieved terms of use from mod.io, checking if already accepted...");
					}
					long num;
					long.TryParse(PlayerPrefs.GetString("modIOAcceptedTermsOfUseId"), out num);
					bool flag = fullTermsOfUse.Id == num;
					long num2;
					long.TryParse(PlayerPrefs.GetString("modIOAcceptedPrivacyPolicyId"), out num2);
					bool flag2 = item3.Id == num2;
					ModioLog verbose3 = ModioLog.Verbose;
					if (verbose3 != null)
					{
						verbose3.Log("[ModIOManager::OnReceivedTermsOfUse] Pre-Editor Skip: " + string.Format("Terms already accepted: {0} | ", flag) + string.Format("Privacy Policy already accepted: {0}", flag2));
					}
					ModioLog verbose4 = ModioLog.Verbose;
					if (verbose4 != null)
					{
						verbose4.Log("[ModIOManager::OnReceivedTermsOfUse] Post-Editor Skip: " + string.Format("Terms already accepted: {0} | ", flag) + string.Format("Privacy Policy already accepted: {0}", flag2));
					}
					valueTuple = new ValueTuple<Error, bool, bool>(Error.None, flag, flag2);
				}
			}
		}
		return valueTuple;
	}

	private async Task<Error> ShowModIOTermsOfUse()
	{
		Error error;
		if (!ModIOManager.initialized)
		{
			error = new Error(ErrorCode.NOT_INITIALIZED, "ModIOManager has not been initialized!");
		}
		else if (this.modIOTermsOfUsePrefab != null)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.modIOTermsOfUsePrefab, base.transform);
			if (gameObject != null)
			{
				ModIOTermsOfUse_v2 component = gameObject.GetComponent<ModIOTermsOfUse_v2>();
				if (component != null)
				{
					CustomMapManager.DisableTeleportHUD();
					gameObject.SetActive(true);
					error = await component.ShowTerms();
				}
				else
				{
					ModioLog error2 = ModioLog.Error;
					if (error2 != null)
					{
						error2.Log("[ModIOManager::ShowModIOTermsOfUse] TermsOfUsePrefab doesn't contain a ModIOTermsOfUse component!");
					}
					error = new Error(ErrorCode.NOT_INITIALIZED, "ModIOManager property 'ModIOTermsOfUsePrefab' object is missing the 'ModIOTermsOfUse_v2' script component.");
				}
			}
			else
			{
				ModioLog error3 = ModioLog.Error;
				if (error3 != null)
				{
					error3.Log("[ModIOManager::ShowModIOTermsOfUse] Failed to create termsOfUseObject!");
				}
				error = new Error(ErrorCode.UNKNOWN, "ModIOManager failed to instantiate the 'ModIOTermsOfUsePrefab'.");
			}
		}
		else
		{
			ModioLog error4 = ModioLog.Error;
			if (error4 != null)
			{
				error4.Log("[ModIOManager::ShowModIOTermsOfUse] ModIOTermsOfUsePrefab is not set!");
			}
			error = new Error(ErrorCode.UNKNOWN, "ModIOManager property 'ModIOTermsOfUsePrefab' is NULL!");
		}
		return error;
	}

	private void OnModIOTermsOfUseAcknowledged(bool accepted)
	{
		if (accepted)
		{
			CustomMapManager.RequestEnableTeleportHUD(true);
			Action<ModIORequestResultAnd<bool>> action = ModIOManager.modIOTermsAcknowledgedCallback;
			if (action != null)
			{
				action(ModIORequestResultAnd<bool>.CreateSuccessResult(true));
			}
		}
		else
		{
			Action<ModIORequestResultAnd<bool>> action2 = ModIOManager.modIOTermsAcknowledgedCallback;
			if (action2 != null)
			{
				action2(ModIORequestResultAnd<bool>.CreateFailureResult("MOD.IO TERMS OF USE HAVE NOT BEEN ACCEPTED. YOU MUST ACCEPT THE MOD.IO TERMS OF USE TO LOGIN WITH YOUR PLATFORM CREDENTIALS OR YOU CAN LOGIN WITH AN EXISTING MOD.IO ACCOUNT BY PRESSING THE 'LINK MOD.IO ACCOUNT' BUTTON AND FOLLOWING THE INSTRUCTIONS."));
			}
		}
		ModIOManager.modIOTermsAcknowledgedCallback = null;
	}

	private static void EnableModManagement()
	{
		if (!ModIOManager.modManagementEnabled)
		{
			ModInstallationManagement.ManagementEvents += ModIOManager.HandleModManagementEvent;
			ModInstallationManagement.Activate();
			ModIOManager.modManagementEnabled = true;
			ModioLog verbose = ModioLog.Verbose;
			if (verbose == null)
			{
				return;
			}
			verbose.Log("[ModIOManager::EnableModManagement] Mod Management enabled.");
		}
	}

	private static void DisableModManagement()
	{
		if (ModIOManager.modManagementEnabled)
		{
			ModioLog verbose = ModioLog.Verbose;
			if (verbose != null)
			{
				verbose.Log("[ModIOManager::EnableModManagement] Mod Management disabled!");
			}
			ModInstallationManagement.ManagementEvents -= ModIOManager.HandleModManagementEvent;
			ModInstallationManagement.Deactivate(false);
			ModIOManager.modManagementEnabled = false;
		}
	}

	private static void HandleModManagementEvent(Mod mod, Modfile modfile, ModInstallationManagement.OperationType jobType, ModInstallationManagement.OperationPhase jobPhase)
	{
		ModioLog verbose = ModioLog.Verbose;
		if (verbose != null)
		{
			verbose.Log("[ModIOManager::HandleModManagementEvent] Mod " + mod.Id.ToString() + " | FileState: " + string.Format("{0} | JobType: {1} | JobPhase: {2}", modfile.State.ToString(), jobType, jobPhase));
		}
		try
		{
			if ((jobType == ModInstallationManagement.OperationType.Install || jobType == ModInstallationManagement.OperationType.Download) && jobPhase == ModInstallationManagement.OperationPhase.Completed && modfile.State == ModFileState.Installed)
			{
				ModIOManager.outdatedModCMSVersions.Remove(mod.Id);
				ModIOManager.IsModOutdated(mod);
			}
			if (jobPhase == ModInstallationManagement.OperationPhase.Started && (jobType == ModInstallationManagement.OperationType.Download || jobType == ModInstallationManagement.OperationType.Update || jobType == ModInstallationManagement.OperationType.Uninstall))
			{
				ModIOManager.outdatedModCMSVersions.Remove(mod.Id);
			}
		}
		catch (Exception ex)
		{
			ModioLog error = ModioLog.Error;
			if (error != null)
			{
				error.Log(string.Format("[ModIOManager::HandleModManagementEvent] Exception: {0}", ex));
			}
		}
		UnityEvent<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase> onModManagementEvent = ModIOManager.OnModManagementEvent;
		if (onModManagementEvent == null)
		{
			return;
		}
		onModManagementEvent.Invoke(mod, modfile, jobType, jobPhase);
	}

	public static async Task RefreshModCache()
	{
		if (ModIOManager.refreshingModCache)
		{
			ModIOManager.restartRefreshModCache = true;
		}
		else
		{
			ModIOManager.refreshingModCache = true;
			UnityEvent onModIOCacheRefreshing = ModIOManager.OnModIOCacheRefreshing;
			if (onModIOCacheRefreshing != null)
			{
				onModIOCacheRefreshing.Invoke();
			}
			ModIOManager.restartRefreshModCache = true;
			while (ModIOManager.restartRefreshModCache)
			{
				ModIOManager.restartRefreshModCache = false;
				await Mod.RefreshPotentiallyHiddenCachedMods();
			}
			ModIOManager.refreshingModCache = false;
			UnityEvent onModIOCacheRefreshed = ModIOManager.OnModIOCacheRefreshed;
			if (onModIOCacheRefreshed != null)
			{
				onModIOCacheRefreshed.Invoke();
			}
		}
	}

	public static bool IsRefreshing()
	{
		return ModIOManager.refreshingModCache;
	}

	public static async Task<ValueTuple<bool, int>> IsModOutdated(ModId modId)
	{
		ValueTuple<bool, int> valueTuple;
		int num;
		if (!ModIOManager.hasInstance)
		{
			valueTuple = new ValueTuple<bool, int>(false, -1);
		}
		else if (ModIOManager.outdatedModCMSVersions.TryGetValue(modId, out num))
		{
			valueTuple = new ValueTuple<bool, int>(true, num);
		}
		else
		{
			ValueTuple<Error, Mod> valueTuple2 = await ModIOManager.GetMod(modId, false, null);
			Error item = valueTuple2.Item1;
			Mod item2 = valueTuple2.Item2;
			if (item)
			{
				ModioLog error = ModioLog.Error;
				if (error != null)
				{
					error.Log(string.Format("[ModIOManager::IsModOutdated] Failed to retrieve mod: {0}", item));
				}
				valueTuple = new ValueTuple<bool, int>(false, -1);
			}
			else
			{
				valueTuple = ModIOManager.IsModOutdated(item2);
			}
		}
		return valueTuple;
	}

	public static ValueTuple<bool, int> IsModOutdated(Mod mod)
	{
		int num;
		if (ModIOManager.outdatedModCMSVersions.TryGetValue(mod.Id, out num))
		{
			return new ValueTuple<bool, int>(true, num);
		}
		if (mod.File != null)
		{
			if (mod.File.State == ModFileState.Installed)
			{
				ValueTuple<bool, int> valueTuple = ModIOManager.IsInstalledModOutdated(mod);
				bool item = valueTuple.Item1;
				int item2 = valueTuple.Item2;
				return new ValueTuple<bool, int>(item, item2);
			}
			ModioLog error = ModioLog.Error;
			if (error != null)
			{
				error.Log("[ModIOManager::IsModOutdated] Mod File for " + mod.Name + " is not installed. " + string.Format("State: {0}.", mod.File.State));
			}
		}
		else
		{
			ModioLog error2 = ModioLog.Error;
			if (error2 != null)
			{
				error2.Log("[ModIOManager::IsModOutdated] Mod File for " + mod.Name + " is null.");
			}
		}
		return new ValueTuple<bool, int>(false, -1);
	}

	public static void SaveFavoriteMods()
	{
		if (!ModIOManager.initialized || !ModIOManager.modManagementEnabled)
		{
			return;
		}
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(ModIOManager.ModIODirectory);
			if (!directoryInfo.Exists)
			{
				ModioLog error = ModioLog.Error;
				if (error != null)
				{
					error.Log("[ModIOManager::SaveFavoriteMods] ModIO Directory for GorillaTag does not exist!");
				}
			}
			else
			{
				long[] array = new long[ModIOManager.favoriteMods.Count];
				int num = 0;
				foreach (KeyValuePair<ModId, Mod> keyValuePair in ModIOManager.favoriteMods)
				{
					array[num++] = keyValuePair.Key;
				}
				string text = JsonConvert.SerializeObject(array);
				File.WriteAllText(Path.Join(directoryInfo.FullName, "favoriteMods.json"), text);
			}
		}
		catch (Exception)
		{
		}
	}

	[return: TupleElementNames(new string[] { "error", "favoriteMods" })]
	public static async Task<ValueTuple<Error, List<Mod>>> GetFavoriteMods(bool forceRefresh = false)
	{
		ValueTuple<Error, List<Mod>> valueTuple;
		if (!ModIOManager.initialized || !ModIOManager.modManagementEnabled)
		{
			valueTuple = new ValueTuple<Error, List<Mod>>(new Error(ErrorCode.NOT_INITIALIZED), null);
		}
		else
		{
			if (forceRefresh)
			{
				ModIOManager.favoriteModsLoaded = false;
				ModIOManager.favoriteMods.Clear();
			}
			if (ModIOManager.favoriteModsLoaded)
			{
				valueTuple = new ValueTuple<Error, List<Mod>>(Error.None, ModIOManager.favoriteMods.Values.ToList<Mod>());
			}
			else
			{
				while (ModInstallationManagement.IsRunning)
				{
					await Task.Yield();
				}
				try
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(ModIOManager.ModIODirectory);
					if (!directoryInfo.Exists)
					{
						GTDev.LogWarning<string>("ModIOManager::GetFavoriteMods Directory " + directoryInfo.ToString() + " does not exist", null);
						ModIOManager.favoriteModsLoaded = true;
						valueTuple = new ValueTuple<Error, List<Mod>>(new Error(ErrorCode.FILE_NOT_FOUND), ModIOManager.favoriteMods.Values.ToList<Mod>());
					}
					else
					{
						FileInfo[] files = directoryInfo.GetFiles("favoriteMods.json");
						if (files.Length == 0)
						{
							GTDev.LogWarning<string>("ModIOManager::GetFavoriteMods could not find file " + ModIOManager.ModIODirectory + "favoriteMods.json", null);
							ModIOManager.favoriteModsLoaded = true;
							valueTuple = new ValueTuple<Error, List<Mod>>(new Error(ErrorCode.FILE_NOT_FOUND), ModIOManager.favoriteMods.Values.ToList<Mod>());
						}
						else
						{
							ValueTuple<Error, ICollection<Mod>> valueTuple2 = await ModIOManager.GetMods(JsonConvert.DeserializeObject<long[]>(File.ReadAllText(files[0].FullName)), forceRefresh, null);
							Error item = valueTuple2.Item1;
							ICollection<Mod> item2 = valueTuple2.Item2;
							if (!item)
							{
								foreach (Mod mod in item2)
								{
									ModIOManager.favoriteMods[mod.Id] = mod;
								}
							}
							ModIOManager.favoriteModsLoaded = true;
							valueTuple = new ValueTuple<Error, List<Mod>>(item, ModIOManager.favoriteMods.Values.ToList<Mod>());
						}
					}
				}
				catch (Exception ex)
				{
					GTDev.LogError<Exception>(ex, null);
					ModIOManager.favoriteModsLoaded = true;
					valueTuple = new ValueTuple<Error, List<Mod>>(new Error(ErrorCode.READ_ERROR), ModIOManager.favoriteMods.Values.ToList<Mod>());
				}
			}
		}
		return valueTuple;
	}

	public static async Task<Error> AddFavorite(ModId modId, Action<Error> callback = null)
	{
		Error error;
		if (ModIOManager.favoriteMods.ContainsKey(modId))
		{
			error = new Error(ErrorCode.UNKNOWN, "MOD ALREADY FAVORITED");
		}
		else
		{
			ValueTuple<Error, Mod> valueTuple = await ModIOManager.GetMod(modId, false, null);
			Error item = valueTuple.Item1;
			Mod item2 = valueTuple.Item2;
			if (!item)
			{
				ModIOManager.favoriteMods.Add(modId, item2);
				ModIOManager.SaveFavoriteMods();
			}
			if (callback != null)
			{
				callback(item);
			}
			error = item;
		}
		return error;
	}

	public static Error RemoveFavorite(ModId modId)
	{
		if (!ModIOManager.favoriteMods.ContainsKey(modId))
		{
			return new Error(ErrorCode.UNKNOWN, "MOD NOT FAVORITED");
		}
		ModIOManager.favoriteMods.Remove(modId);
		ModIOManager.SaveFavoriteMods();
		return Error.None;
	}

	public static bool IsModFavorited(ModId modId)
	{
		return ModIOManager.favoriteMods.ContainsKey(modId);
	}

	[return: TupleElementNames(new string[] { "error", "installedMods" })]
	public static async Task<ValueTuple<Error, Mod[]>> GetInstalledMods(bool forceRefresh = false)
	{
		ValueTuple<Error, Mod[]> valueTuple;
		if (!ModIOManager.initialized || !ModIOManager.modManagementEnabled)
		{
			valueTuple = new ValueTuple<Error, Mod[]>(new Error(ErrorCode.NOT_INITIALIZED), null);
		}
		else
		{
			while (ModInstallationManagement.IsRunning)
			{
				await Task.Yield();
			}
			IEnumerable<Mod> enumerable = await ModInstallationManagement.GetAllInstalledMods(forceRefresh);
			List<Mod> list = new List<Mod>();
			foreach (Mod mod in enumerable)
			{
				if (mod.File.State == ModFileState.Installed)
				{
					list.AddIfNew(mod);
				}
				else if (mod.File.State == ModFileState.Queued && !mod.File.InstallLocation.IsNullOrEmpty())
				{
					list.AddIfNew(mod);
				}
			}
			valueTuple = new ValueTuple<Error, Mod[]>(Error.None, list.ToArray());
		}
		return valueTuple;
	}

	public static bool ValidateInstalledMod(Mod mod)
	{
		return ModIOManager.initialized && ModInstallationManagement.ValidateInstalledMod(mod);
	}

	private static ValueTuple<bool, int> IsInstalledModOutdated(Mod mod)
	{
		int num = -1;
		if (!ModIOManager.hasInstance)
		{
			return new ValueTuple<bool, int>(false, num);
		}
		if (mod.File == null || mod.File.State != ModFileState.Installed)
		{
			ModioLog message = ModioLog.Message;
			if (message != null)
			{
				message.Log("[ModIOManager::IsInstalledModOutdated] Mod " + mod.Id.ToString() + " is not currently installed.");
			}
			return new ValueTuple<bool, int>(false, num);
		}
		try
		{
			FileInfo[] files = new DirectoryInfo(mod.File.InstallLocation).GetFiles("package.json");
			if (files.Length == 0)
			{
				ModioLog error = ModioLog.Error;
				if (error != null)
				{
					error.Log(string.Concat(new string[]
					{
						"[ModIOManager::IsInstalledModOutdated] Directory (",
						mod.File.InstallLocation,
						") for mod ",
						mod.Name,
						" does not contain a package.json file!"
					}));
				}
			}
			if (files.Length > 1)
			{
				ModioLog warning = ModioLog.Warning;
				if (warning != null)
				{
					warning.Log(string.Concat(new string[]
					{
						"[ModIOManager::IsInstalledModOutdated] Directory (",
						mod.File.InstallLocation,
						") for mod ",
						mod.Name,
						" contains more than one package.json file! Only the first one found will be used!"
					}));
				}
			}
			MapPackageInfo packageInfo = CustomMapLoader.GetPackageInfo(files[0].FullName);
			if (packageInfo.customMapSupportVersion != global::GT_CustomMapSupportRuntime.Constants.customMapSupportVersion)
			{
				ModIOManager.outdatedModCMSVersions.Add(mod.Id, packageInfo.customMapSupportVersion);
				return new ValueTuple<bool, int>(true, packageInfo.customMapSupportVersion);
			}
		}
		catch (Exception ex)
		{
			ModioLog error2 = ModioLog.Error;
			if (error2 != null)
			{
				error2.Log(string.Format("[ModIOManager::IsInstalledModOutdated] Exception while reading package.json: {0}", ex));
			}
			ModInstallationManagement.RefreshMod(mod);
			return new ValueTuple<bool, int>(false, num);
		}
		return new ValueTuple<bool, int>(false, num);
	}

	public static async Task RefreshUserProfile(Action<bool> callback = null, bool force = false)
	{
		if (!ModIOManager.hasInstance || !ModIOManager.IsLoggedIn())
		{
			if (callback != null)
			{
				callback(false);
			}
		}
		else if (ModIOManager.refreshing && callback != null)
		{
			ModIOManager.currentRefreshCallbacks.Add(callback);
		}
		else if (force || Mathf.Approximately(0f, ModIOManager.lastRefreshTime) || Time.realtimeSinceStartup - ModIOManager.lastRefreshTime >= 5f)
		{
			ModIOManager.currentRefreshCallbacks.Add(callback);
			ModIOManager.lastRefreshTime = Time.realtimeSinceStartup;
			ModIOManager.refreshing = true;
			if (User.Current.IsUpdating)
			{
				ModioLog verbose = ModioLog.Verbose;
				if (verbose != null)
				{
					verbose.Log("[ModIOManager::Refresh] Profile already updating, waiting for Sync to finish...");
				}
				while (User.Current.IsUpdating)
				{
					await Task.Yield();
				}
			}
			else
			{
				ModioLog verbose2 = ModioLog.Verbose;
				if (verbose2 != null)
				{
					verbose2.Log("[ModIOManager::Refresh] Syncing user profile...");
				}
				await User.Current.Sync();
			}
			ModIOManager.refreshing = false;
			foreach (Action<bool> action in ModIOManager.currentRefreshCallbacks)
			{
				if (action != null)
				{
					action(true);
				}
			}
			ModIOManager.currentRefreshCallbacks.Clear();
		}
		else if (callback != null)
		{
			callback(false);
		}
	}

	[return: TupleElementNames(new string[] { "error", "mods" })]
	public static async Task<ValueTuple<Error, ICollection<Mod>>> GetMods(ICollection<long> modIds, bool forceRefresh = false, Action<Error, ICollection<Mod>> callback = null)
	{
		ValueTuple<Error, ICollection<Mod>> valueTuple;
		if (!ModIOManager.hasInstance)
		{
			Error error = new Error(ErrorCode.NOT_INITIALIZED);
			if (callback != null)
			{
				callback(error, null);
			}
			valueTuple = new ValueTuple<Error, ICollection<Mod>>(error, null);
		}
		else
		{
			ValueTuple<Error, ICollection<Mod>> valueTuple2 = await Mod.GetMods(modIds, forceRefresh, null);
			Error error = valueTuple2.Item1;
			ICollection<Mod> item = valueTuple2.Item2;
			if (error)
			{
				ModioLog error2 = ModioLog.Error;
				if (error2 != null)
				{
					error2.Log("[ModIOManager::GetMod] Failed to get requested Mods. Error: " + error.GetMessage());
				}
				if (callback != null)
				{
					callback(error, null);
				}
				valueTuple = new ValueTuple<Error, ICollection<Mod>>(error, null);
			}
			else
			{
				if (callback != null)
				{
					callback(error, item);
				}
				valueTuple = new ValueTuple<Error, ICollection<Mod>>(Error.None, item);
			}
		}
		return valueTuple;
	}

	[return: TupleElementNames(new string[] { "error", "result" })]
	public static async Task<ValueTuple<Error, Mod>> GetMod(ModId modId, bool forceUpdate = false, Action<Error, Mod> callback = null)
	{
		ValueTuple<Error, Mod> valueTuple;
		if (!ModIOManager.hasInstance)
		{
			Error error = new Error(ErrorCode.NOT_INITIALIZED);
			if (callback != null)
			{
				callback(error, null);
			}
			valueTuple = new ValueTuple<Error, Mod>(error, null);
		}
		else
		{
			Mod retrievedMod = null;
			ValueTuple<Error, Mod> valueTuple2 = await Mod.GetMod(modId, forceUpdate, null, false);
			Error error = valueTuple2.Item1;
			retrievedMod = valueTuple2.Item2;
			if (error)
			{
				ModioLog error2 = ModioLog.Error;
				if (error2 != null)
				{
					error2.Log("[ModIOManager::GetMod] Failed to get Mod " + modId.ToString() + ". Error: " + error.GetMessage());
				}
				if (callback != null)
				{
					callback(error, retrievedMod);
				}
				valueTuple = new ValueTuple<Error, Mod>(error, retrievedMod);
			}
			else
			{
				if (forceUpdate)
				{
					if (ModIOManager.IsLoggedIn())
					{
						await ModIOManager.RefreshUserProfile(null, false);
					}
					else
					{
						ModInstallationManagement.RefreshMod(retrievedMod);
					}
				}
				if (callback != null)
				{
					callback(error, retrievedMod);
				}
				valueTuple = new ValueTuple<Error, Mod>(Error.None, retrievedMod);
			}
		}
		return valueTuple;
	}

	[return: TupleElementNames(new string[] { "error", "logo" })]
	public static async Task<ValueTuple<Error, Texture2D>> GetModLogo(Mod mod, Action<Error, Texture2D> callback)
	{
		ValueTuple<Error, Texture2D> valueTuple;
		if (mod == null || !mod.Id.IsValid())
		{
			valueTuple = new ValueTuple<Error, Texture2D>(new Error(ErrorCode.BAD_PARAMETER), null);
		}
		else if (!ModIOManager.hasInstance)
		{
			Error error = new Error(ErrorCode.NOT_INITIALIZED);
			if (callback != null)
			{
				callback(error, null);
			}
			valueTuple = new ValueTuple<Error, Texture2D>(error, null);
		}
		else if (mod.Logo == null)
		{
			Error error = new Error(ErrorCode.UNKNOWN, "Mod Logo is null!");
			if (callback != null)
			{
				callback(error, null);
			}
			valueTuple = new ValueTuple<Error, Texture2D>(error, null);
		}
		else
		{
			ModioLog verbose = ModioLog.Verbose;
			if (verbose != null)
			{
				verbose.Log("[ModIOManager::GetModLogo] Getting logo for Mod " + mod.Id.ToString() + "...");
			}
			ValueTuple<Error, Texture2D> valueTuple2 = await mod.Logo.DownloadAsTexture2D(Mod.LogoResolution.X320_Y180);
			Error error = valueTuple2.Item1;
			Texture2D item = valueTuple2.Item2;
			if (error)
			{
				ModioLog error2 = ModioLog.Error;
				if (error2 != null)
				{
					error2.Log("[ModIOManager::GetModLogo] Failed to download logo for Mod " + mod.Id.ToString() + ". Error: " + error.GetMessage());
				}
			}
			if (callback != null)
			{
				callback(error, item);
			}
			valueTuple = new ValueTuple<Error, Texture2D>(error, item);
		}
		return valueTuple;
	}

	[return: TupleElementNames(new string[] { "error", "modsPage" })]
	public static async Task<ValueTuple<Error, ModioPage<Mod>>> GetMods(ModioAPI.Mods.GetModsFilter searchFilter)
	{
		ValueTuple<Error, ModioPage<Mod>> valueTuple;
		if (!ModIOManager.hasInstance)
		{
			Error error = new Error(ErrorCode.NOT_INITIALIZED);
			valueTuple = new ValueTuple<Error, ModioPage<Mod>>(error, null);
		}
		else
		{
			valueTuple = await Mod.GetMods(searchFilter, false);
		}
		return valueTuple;
	}

	private static void ModIOUserChanged(User currentUser)
	{
		ModioLog verbose = ModioLog.Verbose;
		if (verbose != null)
		{
			verbose.Log("[ModIOManager::ModIOUserChanged] CurrentUser: " + ((currentUser == null) ? "NULL" : currentUser.Profile.Username));
		}
		UnityEvent<User> onModIOUserChanged = ModIOManager.OnModIOUserChanged;
		if (onModIOUserChanged == null)
		{
			return;
		}
		onModIOUserChanged.Invoke(currentUser);
	}

	private static void ModIOUserSyncComplete()
	{
		ModioLog verbose = ModioLog.Verbose;
		if (verbose != null)
		{
			verbose.Log("[ModIOManager::ModIOUserSyncComplete] Refreshing mod cache...");
		}
		ModIOManager.RefreshModCache();
	}

	public static bool IsLoggedIn()
	{
		return User.Current != null && User.Current.IsAuthenticated;
	}

	public static bool IsLoggingIn()
	{
		return ModIOManager.loggingIn;
	}

	public static bool IsLoggingOut()
	{
		return ModIOManager.loggingOut;
	}

	public static string GetCurrentUsername()
	{
		if (!ModIOManager.IsLoggedIn())
		{
			return "";
		}
		ModioLog verbose = ModioLog.Verbose;
		if (verbose != null)
		{
			verbose.Log("[ModIOManager::GetCurrentUsername] Username: " + User.Current.Profile.Username);
		}
		return User.Current.Profile.Username;
	}

	public static string GetCurrentUserId()
	{
		if (!ModIOManager.IsLoggedIn())
		{
			return "";
		}
		ModioLog verbose = ModioLog.Verbose;
		if (verbose != null)
		{
			verbose.Log(string.Format("[ModIOManager::GetCurrentUserId] User ID: {0}", User.Current.Profile.UserId));
		}
		return User.Current.Profile.UserId.ToString();
	}

	public static string GetCurrentAuthToken()
	{
		if (!ModIOManager.IsLoggedIn())
		{
			return "";
		}
		return User.Current.Token;
	}

	public static bool IsAuthenticated(bool sendEvents = false)
	{
		if (!ModIOManager.hasInstance)
		{
			return false;
		}
		bool isAuthenticated = User.Current.IsAuthenticated;
		if (isAuthenticated)
		{
			ModIOManager.loggingIn = false;
			ModioLog verbose = ModioLog.Verbose;
			if (verbose != null)
			{
				verbose.Log("[ModIOManager::IsAuthenticated] User already authenticated...");
			}
			if (sendEvents)
			{
				UnityEvent onModIOLoggedIn = ModIOManager.OnModIOLoggedIn;
				if (onModIOLoggedIn != null)
				{
					onModIOLoggedIn.Invoke();
				}
			}
		}
		else
		{
			try
			{
				ModioLog verbose2 = ModioLog.Verbose;
				if (verbose2 != null)
				{
					verbose2.Log("[ModIOManager::IsAuthenticated] User not authenticated");
				}
				if (sendEvents)
				{
					UnityEvent onModIOLoggedOut = ModIOManager.OnModIOLoggedOut;
					if (onModIOLoggedOut != null)
					{
						onModIOLoggedOut.Invoke();
					}
				}
			}
			catch (Exception ex)
			{
				ModioLog verbose3 = ModioLog.Verbose;
				if (verbose3 != null)
				{
					verbose3.Log(string.Format("[ModIOManager::IsAuthenticated] error {0}", ex));
				}
			}
		}
		ModioLog verbose4 = ModioLog.Verbose;
		if (verbose4 != null)
		{
			verbose4.Log(string.Format("[ModIOManager::IsAuthenticated] returning {0}", isAuthenticated));
		}
		return isAuthenticated;
	}

	public static void LogoutFromModIO()
	{
		if (!ModIOManager.hasInstance || ModIOManager.loggingIn || !ModIOManager.IsLoggedIn())
		{
			return;
		}
		ModIOManager.loggingOut = true;
		ModioLog verbose = ModioLog.Verbose;
		if (verbose != null)
		{
			verbose.Log("[ModIOManager::LogoutFromModIO] Logging out of mod.io...");
		}
		ModIOManager.CancelExternalAuthentication();
		ModIOManager.loggingIn = false;
		User.DeleteUserData();
		ModioLog verbose2 = ModioLog.Verbose;
		if (verbose2 != null)
		{
			verbose2.Log("[ModIOManager::LogoutFromModIO] User data deleted...");
		}
		PlayerPrefs.SetInt("modIOLassSuccessfulAuthMethod", ModIOManager.ModIOAuthMethod.Invalid.GetIndex<ModIOManager.ModIOAuthMethod>());
		ModioLog verbose3 = ModioLog.Verbose;
		if (verbose3 != null)
		{
			verbose3.Log("[ModIOManager::LogoutFromModIO] User fully logged out.");
		}
		ModIOManager.loggingOut = false;
		UnityEvent onModIOLoggedOut = ModIOManager.OnModIOLoggedOut;
		if (onModIOLoggedOut != null)
		{
			onModIOLoggedOut.Invoke();
		}
		ModIOManager.RefreshModCache();
	}

	public static void SetAccountLinkPrompter(IWssAuthPrompter prompter)
	{
		if (ModIOManager.accountLinkingAuthService != null)
		{
			ModIOManager.accountLinkingAuthService.SetPrompter(prompter);
		}
	}

	public static async Task<Error> RequestAccountLinkCode()
	{
		Error error;
		if (!ModIOManager.hasInstance)
		{
			error = new Error(ErrorCode.NOT_INITIALIZED);
		}
		else if (ModIOManager.loggingIn)
		{
			error = new Error(ErrorCode.USER_AUTHENTICATION_IN_PROGRESS);
		}
		else if (ModIOManager.IsLoggedIn())
		{
			error = new Error(ErrorCode.ALREADY_AUTHENTICATED);
		}
		else
		{
			ModIOManager.loggingIn = true;
			ModioLog verbose = ModioLog.Verbose;
			if (verbose != null)
			{
				verbose.Log("[ModIOManager::RequestAccountLinkCode] Requesting Link Code...");
			}
			Error error2 = await ModIOManager.accountLinkingAuthService.Authenticate(false, null);
			if (!error2)
			{
				ModioLog verbose2 = ModioLog.Verbose;
				if (verbose2 != null)
				{
					verbose2.Log("[ModIOManager::RequestAccountLinkCode] Account linked successfully!");
				}
				PlayerPrefs.SetInt("modIOLassSuccessfulAuthMethod", ModIOManager.ModIOAuthMethod.LinkedAccount.GetIndex<ModIOManager.ModIOAuthMethod>());
			}
			ModIOManager.OnAuthenticationComplete(error2);
			error = error2;
		}
		return error;
	}

	public static void CancelExternalAuthentication()
	{
		if (!ModIOManager.hasInstance)
		{
			return;
		}
		if (ModIOManager.accountLinkingAuthService != null && ModIOManager.accountLinkingAuthService.InProgress())
		{
			ModioLog verbose = ModioLog.Verbose;
			if (verbose != null)
			{
				verbose.Log("[ModIOManager::CancelExternalAuthentication] Cancelling Mod.io Account Linking process...");
			}
			ModIOManager.accountLinkingAuthService.Cancel();
		}
	}

	public static async Task<Error> RequestPlatformLogin()
	{
		Error error3;
		if (!ModIOManager.hasInstance)
		{
			ModioLog error2 = ModioLog.Error;
			if (error2 != null)
			{
				error2.Log("[ModIOManager::RequestPlatformLogin] has no instance");
			}
			error3 = new Error(ErrorCode.NOT_INITIALIZED, "ModIOManager has not been initialized!");
		}
		else if (ModIOManager.loggingIn)
		{
			ModioLog message = ModioLog.Message;
			if (message != null)
			{
				message.Log("[ModIOManager::RequestPlatformLogin] is already logging in");
			}
			error3 = new Error(ErrorCode.USER_AUTHENTICATION_IN_PROGRESS);
		}
		else
		{
			ModIOManager.loggingIn = true;
			ModioLog verbose = ModioLog.Verbose;
			if (verbose != null)
			{
				verbose.Log("[ModIOManager::RequestPlatformLogin] calling IsAuthenticated");
			}
			if (ModIOManager.IsAuthenticated(true))
			{
				ModioLog verbose2 = ModioLog.Verbose;
				if (verbose2 != null)
				{
					verbose2.Log("[ModIOManager::RequestPlatformLogin] User already authenticated!");
				}
				error3 = Error.None;
			}
			else
			{
				ModioLog verbose3 = ModioLog.Verbose;
				if (verbose3 != null)
				{
					verbose3.Log("[ModIOManager::RequestPlatformLogin] calling InitializePlatformLogin");
				}
				Error error = new Error(ErrorCode.NONE);
				try
				{
					Error error4 = await ModIOManager.instance.InitiatePlatformLogin();
					error = error4;
				}
				catch (Exception ex)
				{
					ModioLog error5 = ModioLog.Error;
					if (error5 != null)
					{
						error5.Log(string.Format("[ModIOManager::RequestPlatformLogin] exception initializing platform login {0}", ex));
					}
				}
				error3 = error;
			}
		}
		return error3;
	}

	private async Task<Error> InitiatePlatformLogin()
	{
		UnityEvent onModIOLoginStarted = ModIOManager.OnModIOLoginStarted;
		if (onModIOLoginStarted != null)
		{
			onModIOLoginStarted.Invoke();
		}
		ModioLog verbose = ModioLog.Verbose;
		if (verbose != null)
		{
			verbose.Log("[ModIOManager::InitiatePlatformLogin] Attempting to login using platform credentials...");
		}
		ValueTuple<Error, bool, bool> valueTuple = await this.HasAcceptedLatestTerms();
		Error error = valueTuple.Item1;
		bool item = valueTuple.Item2;
		bool item2 = valueTuple.Item3;
		Error error2;
		if (error)
		{
			ModIOManager.loggingIn = false;
			UnityEvent<string> onModIOLoginFailed = ModIOManager.OnModIOLoginFailed;
			if (onModIOLoginFailed != null)
			{
				onModIOLoginFailed.Invoke(string.Format("FAILED TO LOGIN TO MOD.IO:\nFAILED TO CHECK TERMS OF USE ACCEPTANCE STATUS: {0}", error));
			}
			error2 = error;
		}
		else
		{
			if (!item || !item2)
			{
				error = await this.ShowModIOTermsOfUse();
				if (error)
				{
					ModIOManager.OnAuthenticationComplete(error);
					return error;
				}
				ValueTuple<Error, Agreement> valueTuple2 = await Agreement.GetAgreement(AgreementType.TermsOfUse, false);
				error = valueTuple2.Item1;
				Agreement item3 = valueTuple2.Item2;
				if (!error)
				{
					PlayerPrefs.SetString("modIOAcceptedTermsOfUseId", item3.Id.ToString());
				}
				ValueTuple<Error, Agreement> valueTuple3 = await Agreement.GetAgreement(AgreementType.PrivacyPolicy, false);
				error = valueTuple3.Item1;
				Agreement item4 = valueTuple3.Item2;
				if (!error)
				{
					PlayerPrefs.SetString("modIOAcceptedPrivacyPolicyId", item4.Id.ToString());
				}
			}
			error = await this.ContinuePlatformLogin();
			error2 = error;
		}
		return error2;
	}

	private async Task<Error> ContinuePlatformLogin()
	{
		Error error3;
		if (SteamManager.Initialized)
		{
			ModIOManager.steamAuthService.SetCredentialProvider(this);
			Error error = await ModIOManager.steamAuthService.Authenticate(true, null);
			if (error)
			{
				ModioLog error2 = ModioLog.Error;
				if (error2 != null)
				{
					error2.Log(string.Format("[ModIOManager::ContinuePlatformLogin] Failed to authenticate via Steam: {0}", error));
				}
				ModIOManager.OnAuthenticationComplete(error);
				error3 = error;
			}
			else
			{
				ModioLog verbose = ModioLog.Verbose;
				if (verbose != null)
				{
					verbose.Log("[ModIOManager::ContinuePlatformLogin] Successfully authenticated via Steam!");
				}
				PlayerPrefs.SetInt("modIOLassSuccessfulAuthMethod", ModIOManager.ModIOAuthMethod.Steam.GetIndex<ModIOManager.ModIOAuthMethod>());
				ModIOManager.OnAuthenticationComplete(Error.None);
				error3 = Error.None;
			}
		}
		else
		{
			ModioLog error4 = ModioLog.Error;
			if (error4 != null)
			{
				error4.Log("[ModIOManager::ContinuePlatformLogin] Steam enabled but not initialized...");
			}
			ModIOManager.OnAuthenticationComplete(new Error(ErrorCode.NOT_INITIALIZED, "STEAM IS ENABLED BUT NOT INITIALIZED."));
			error3 = new Error(ErrorCode.NOT_INITIALIZED, "Steam is enabled, but has not been initialized.");
		}
		return error3;
	}

	public void RequestEncryptedAppTicket(Action<bool, string> callback)
	{
		if (this.requestEncryptedAppTicketCallback != null)
		{
			ModioLog warning = ModioLog.Warning;
			if (warning != null)
			{
				warning.Log("[ModIOManager::RequestEncryptedAppTicket] Callback already set, Encrypted App Ticket request already in progress!");
			}
			if (callback != null)
			{
				callback(false, "AN ENCRYPTED APP TICKET REQUEST IS ALREADY IN PROGRESS");
			}
			return;
		}
		this.requestEncryptedAppTicketCallback = callback;
		if (ModIOManager.requestEncryptedAppTicketResponse == null)
		{
			ModIOManager.requestEncryptedAppTicketResponse = CallResult<EncryptedAppTicketResponse_t>.Create(new CallResult<EncryptedAppTicketResponse_t>.APIDispatchDelegate(this.OnRequestEncryptedAppTicketFinished));
		}
		ModioLog verbose = ModioLog.Verbose;
		if (verbose != null)
		{
			verbose.Log("[ModIOManager::RequestEncryptedAppTicket] Requesting Steam Encrypted App Ticket...");
		}
		SteamAPICall_t steamAPICall_t = SteamUser.RequestEncryptedAppTicket(null, 0);
		ModIOManager.requestEncryptedAppTicketResponse.Set(steamAPICall_t, null);
	}

	private void OnRequestEncryptedAppTicketFinished(EncryptedAppTicketResponse_t response, bool bIOFailure)
	{
		if (bIOFailure)
		{
			ModioLog error = ModioLog.Error;
			if (error != null)
			{
				error.Log("Failed to retrieve EncryptedAppTicket due to a Steam API IO failure...");
			}
			Action<bool, string> action = this.requestEncryptedAppTicketCallback;
			if (action != null)
			{
				action(false, "FAILED TO RETRIEVE 'EncryptedAppTicket' DUE TO A STEAM API IO FAILURE.");
			}
			this.requestEncryptedAppTicketCallback = null;
			return;
		}
		EResult eResult = response.m_eResult;
		if (eResult <= EResult.k_EResultNoConnection)
		{
			if (eResult != EResult.k_EResultOK)
			{
				if (eResult == EResult.k_EResultNoConnection)
				{
					ModioLog error2 = ModioLog.Error;
					if (error2 != null)
					{
						error2.Log("[ModIOManager::OnRequestEncryptedAppTicketFinished] Not connected to steam.");
					}
					Action<bool, string> action2 = this.requestEncryptedAppTicketCallback;
					if (action2 != null)
					{
						action2(false, "NOT CONNECTED TO STEAM.");
					}
					this.requestEncryptedAppTicketCallback = null;
					return;
				}
			}
			else
			{
				if (!SteamUser.GetEncryptedAppTicket(ModIOManager.ticketBlob, ModIOManager.ticketBlob.Length, out ModIOManager.ticketSize))
				{
					ModioLog error3 = ModioLog.Error;
					if (error3 != null)
					{
						error3.Log("[ModIOManager::OnRequestEncryptedAppTicketFinished] Failed to retrieve " + string.Format("EncryptedAppTicket! Needed size: {0}", ModIOManager.ticketSize));
					}
					Action<bool, string> action3 = this.requestEncryptedAppTicketCallback;
					if (action3 != null)
					{
						action3(false, "FAILED TO RETRIEVE 'EncryptedAppTicket'.");
					}
					this.requestEncryptedAppTicketCallback = null;
					return;
				}
				Array.Resize<byte>(ref ModIOManager.ticketBlob, (int)ModIOManager.ticketSize);
				string text = Convert.ToBase64String(ModIOManager.ticketBlob);
				ModioLog verbose = ModioLog.Verbose;
				if (verbose != null)
				{
					verbose.Log("[ModIOManager::OnRequestEncryptedAppTicketFinished] Successfully retrieved Steam Encrypted App Ticket: " + text);
				}
				Action<bool, string> action4 = this.requestEncryptedAppTicketCallback;
				if (action4 != null)
				{
					action4(true, text);
				}
				this.requestEncryptedAppTicketCallback = null;
				return;
			}
		}
		else
		{
			if (eResult == EResult.k_EResultLimitExceeded)
			{
				ModioLog error4 = ModioLog.Error;
				if (error4 != null)
				{
					error4.Log("[ModIOManager::OnRequestEncryptedAppTicketFinished] Rate Limit exceeded, this function should not be called more than once per minute.");
				}
				Action<bool, string> action5 = this.requestEncryptedAppTicketCallback;
				if (action5 != null)
				{
					action5(false, "RATE LIMIT EXCEEDED, CAN ONLY REQUEST ONE 'EncryptedAppTicket' PER MINUTE.");
				}
				this.requestEncryptedAppTicketCallback = null;
				return;
			}
			if (eResult == EResult.k_EResultDuplicateRequest)
			{
				ModioLog error5 = ModioLog.Error;
				if (error5 != null)
				{
					error5.Log("[ModIOManager::OnRequestEncryptedAppTicketFinished] There is already a pending EncryptedAppTicket request.");
				}
				Action<bool, string> action6 = this.requestEncryptedAppTicketCallback;
				if (action6 != null)
				{
					action6(false, "THERE IS ALREADY AN 'EncryptedAppTicket' REQUEST IN PROGRESS.");
				}
				this.requestEncryptedAppTicketCallback = null;
				return;
			}
		}
		ModioLog error6 = ModioLog.Error;
		if (error6 != null)
		{
			error6.Log(string.Format("[ModIOManager::OnRequestEncryptedAppTicketFinished] Unknown Error: {0}", response.m_eResult));
		}
		Action<bool, string> action7 = this.requestEncryptedAppTicketCallback;
		if (action7 != null)
		{
			action7(false, string.Format("{0}", response.m_eResult));
		}
		this.requestEncryptedAppTicketCallback = null;
	}

	public async Task<ValueTuple<Error, string>> GetOculusUserId()
	{
		return new ValueTuple<Error, string>(Error.Unknown, "OCULUS is not enabled for this build");
	}

	public async Task<string> GetOculusAccessToken()
	{
		return "";
	}

	public async Task<string> GetOculusUserProof()
	{
		return "";
	}

	public string GetOculusDevice()
	{
		return "";
	}

	private static void OnAuthenticationComplete(Error error)
	{
		ModIOManager.loggingIn = false;
		if (error)
		{
			UnityEvent<string> onModIOLoginFailed = ModIOManager.OnModIOLoginFailed;
			if (onModIOLoginFailed == null)
			{
				return;
			}
			onModIOLoginFailed.Invoke(string.Format("FAILED TO LOGIN TO MOD.IO: {0}", error));
			return;
		}
		else
		{
			UnityEvent onModIOLoggedIn = ModIOManager.OnModIOLoggedIn;
			if (onModIOLoggedIn == null)
			{
				return;
			}
			onModIOLoggedIn.Invoke();
			return;
		}
	}

	public static ModIOManager.ModIOAuthMethod GetLastAuthMethod()
	{
		int @int = PlayerPrefs.GetInt("modIOLassSuccessfulAuthMethod", -1);
		if (@int == -1)
		{
			return ModIOManager.ModIOAuthMethod.Invalid;
		}
		return (ModIOManager.ModIOAuthMethod)@int;
	}

	public static async Task<ValueTuple<Error, Mod[]>> GetSubscribedMods()
	{
		ValueTuple<Error, Mod[]> valueTuple;
		if (!ModIOManager.IsLoggedIn())
		{
			valueTuple = new ValueTuple<Error, Mod[]>(new Error(ErrorCode.USER_NOT_AUTHENTICATED), null);
		}
		else
		{
			if (User.Current.IsUpdating)
			{
				while (User.Current.IsUpdating)
				{
					await Task.Yield();
				}
			}
			valueTuple = new ValueTuple<Error, Mod[]>(Error.None, User.Current.ModRepository.GetSubscribed().ToArray<Mod>());
		}
		return valueTuple;
	}

	public static async Task<Error> SubscribeToMod(ModId modId, Action<Error> callback)
	{
		Error error2;
		if (!ModIOManager.IsLoggedIn())
		{
			ModioLog error = ModioLog.Error;
			if (error != null)
			{
				error.Log("[ModIOManager::SubscribeToMod] Called while not logged in!");
			}
			error2 = new Error(ErrorCode.USER_NOT_AUTHENTICATED);
		}
		else
		{
			if (User.Current.IsUpdating)
			{
				ModioLog verbose = ModioLog.Verbose;
				if (verbose != null)
				{
					verbose.Log("[ModIOManager::SubscribeToMod] User currently updating... waiting");
				}
				while (User.Current.IsUpdating)
				{
					await Task.Yield();
				}
			}
			if (User.Current.ModRepository.IsSubscribed(modId))
			{
				ModioLog message = ModioLog.Message;
				if (message != null)
				{
					message.Log(string.Format("[ModIOManager::SubscribeToMod] Already subscribed to Mod {0}", modId));
				}
				if (callback != null)
				{
					callback(Error.None);
				}
				error2 = Error.None;
			}
			else
			{
				ValueTuple<Error, Mod> valueTuple = await Mod.GetMod(modId, false, null, false);
				Error error3 = valueTuple.Item1;
				Mod item = valueTuple.Item2;
				if (error3)
				{
					ModioLog error4 = ModioLog.Error;
					if (error4 != null)
					{
						error4.Log(string.Format("[ModIOManager::SubscribeToMod] Failed to retrieve mod details for Mod {0}: ", modId) + error3.GetMessage());
					}
					if (callback != null)
					{
						callback(error3);
					}
					error2 = error3;
				}
				else
				{
					ModioLog verbose2 = ModioLog.Verbose;
					if (verbose2 != null)
					{
						verbose2.Log(string.Format("[ModIOManager::SubscribeToMod] Subscribing to mod with ID: {0}", modId));
					}
					error3 = await item.Subscribe(true);
					if (error3)
					{
						ModioLog error5 = ModioLog.Error;
						if (error5 != null)
						{
							error5.Log(string.Format("[ModIOManager::SubscribeToMod] Failed to subscribe to Mod {0}: ", modId) + error3.GetMessage());
						}
					}
					if (callback != null)
					{
						callback(error3);
					}
					error2 = error3;
				}
			}
		}
		return error2;
	}

	public static async Task<Error> UnsubscribeFromMod(ModId modId, Action<Error> callback)
	{
		Error error2;
		if (!ModIOManager.IsLoggedIn())
		{
			ModioLog error = ModioLog.Error;
			if (error != null)
			{
				error.Log("[ModIOManager::UnsubscribeFromMod] Called while not logged in!");
			}
			error2 = new Error(ErrorCode.USER_NOT_AUTHENTICATED);
		}
		else
		{
			if (User.Current.IsUpdating)
			{
				ModioLog verbose = ModioLog.Verbose;
				if (verbose != null)
				{
					verbose.Log("[ModIOManager::UnsubscribeFromMod] User currently updating... waiting");
				}
				while (User.Current.IsUpdating)
				{
					await Task.Yield();
				}
			}
			if (!User.Current.ModRepository.IsSubscribed(modId))
			{
				ModioLog message = ModioLog.Message;
				if (message != null)
				{
					message.Log(string.Format("[ModIOManager::UnsubscribeFromMod] Not currently subscribed to Mod {0}", modId));
				}
				if (callback != null)
				{
					callback(Error.None);
				}
				error2 = Error.None;
			}
			else
			{
				ValueTuple<Error, Mod> valueTuple = await Mod.GetMod(modId, false, null, false);
				Error error3 = valueTuple.Item1;
				Mod item = valueTuple.Item2;
				if (error3)
				{
					ModioLog error4 = ModioLog.Error;
					if (error4 != null)
					{
						error4.Log("[ModIOManager::UnsubscribeFromMod] Failed to retrieve mod details for Mod " + string.Format("{0}: {1}", modId, error3.GetMessage()));
					}
					if (callback != null)
					{
						callback(error3);
					}
					error2 = error3;
				}
				else
				{
					ModioLog verbose2 = ModioLog.Verbose;
					if (verbose2 != null)
					{
						verbose2.Log("[ModIOManager::UnsubscribeFromMod] Unsubscribing from Mod " + modId.ToString());
					}
					error3 = await item.Unsubscribe();
					if (error3)
					{
						ModioLog error5 = ModioLog.Error;
						if (error5 != null)
						{
							error5.Log(string.Format("[ModIOManager::UnsubscribeToMod] Failed to unsubscribe from Mod {0}: ", modId) + error3.GetMessage());
						}
					}
					if (callback != null)
					{
						callback(error3);
					}
					error2 = error3;
				}
			}
		}
		return error2;
	}

	public static async Task<ValueTuple<bool, ModFileState>> GetSubscribedModStatus(ModId modId)
	{
		ValueTuple<bool, ModFileState> valueTuple;
		if (!ModIOManager.hasInstance)
		{
			valueTuple = new ValueTuple<bool, ModFileState>(false, ModFileState.None);
		}
		else if (!ModIOManager.IsLoggedIn())
		{
			valueTuple = new ValueTuple<bool, ModFileState>(false, ModFileState.None);
		}
		else
		{
			if (User.Current.IsUpdating)
			{
				while (User.Current.IsUpdating)
				{
					await Task.Yield();
				}
			}
			if (!User.Current.ModRepository.IsSubscribed(modId))
			{
				valueTuple = new ValueTuple<bool, ModFileState>(false, ModFileState.None);
			}
			else
			{
				ValueTuple<Error, Mod> valueTuple2 = await Mod.GetMod(modId, false, null, false);
				Error item = valueTuple2.Item1;
				Mod item2 = valueTuple2.Item2;
				if (item)
				{
					ModioLog error = ModioLog.Error;
					if (error != null)
					{
						error.Log("[ModIOManager::GetSubscribedModStatus] Failed to retrieve Mod " + modId.ToString() + "'s status: " + item.GetMessage());
					}
					valueTuple = new ValueTuple<bool, ModFileState>(true, ModFileState.None);
				}
				else
				{
					valueTuple = new ValueTuple<bool, ModFileState>(true, item2.File.State);
				}
			}
		}
		return valueTuple;
	}

	public static async Task<ValueTuple<bool, Mod>> GetSubscribedModProfile(ModId modId, Action<bool, Mod> callback = null)
	{
		ValueTuple<bool, Mod> valueTuple;
		if (!ModIOManager.hasInstance)
		{
			if (callback != null)
			{
				callback(false, null);
			}
			valueTuple = new ValueTuple<bool, Mod>(false, null);
		}
		else if (!ModIOManager.IsLoggedIn())
		{
			if (callback != null)
			{
				callback(false, null);
			}
			valueTuple = new ValueTuple<bool, Mod>(false, null);
		}
		else
		{
			if (User.Current.IsUpdating)
			{
				ModioLog verbose = ModioLog.Verbose;
				if (verbose != null)
				{
					verbose.Log("[ModIOManager::GetSubscribedModProfile] Subscriptions currently updating, waiting for Sync to finish...");
				}
				while (User.Current.IsUpdating)
				{
					await Task.Yield();
				}
			}
			ModioLog verbose2 = ModioLog.Verbose;
			if (verbose2 != null)
			{
				verbose2.Log("[ModIOManager::GetSubscribedModProfile] Checking Subscribed Mod list for Mod " + modId.ToString());
			}
			foreach (Mod mod in User.Current.ModRepository.GetSubscribed())
			{
				if (mod.Id.Equals(modId))
				{
					ModioLog verbose3 = ModioLog.Verbose;
					if (verbose3 != null)
					{
						verbose3.Log("[ModIOManager::GetSubscribedModProfile] Found Mod " + modId.ToString() + " in Subscribed Mod list.");
					}
					if (callback != null)
					{
						callback(true, mod);
					}
					return new ValueTuple<bool, Mod>(true, mod);
				}
			}
			ModioLog verbose4 = ModioLog.Verbose;
			if (verbose4 != null)
			{
				verbose4.Log("[ModIOManager::GetSubscribedModProfile] Mod " + modId.ToString() + " not present in Subscribed Mod list.");
			}
			if (callback != null)
			{
				callback(false, null);
			}
			valueTuple = new ValueTuple<bool, Mod>(false, null);
		}
		return valueTuple;
	}

	public static async Task<ModFileState> GetModStatus(ModId modId)
	{
		ModFileState modFileState;
		if (!ModIOManager.hasInstance)
		{
			modFileState = ModFileState.None;
		}
		else
		{
			ValueTuple<Error, Mod> valueTuple = await Mod.GetMod(modId, false, null, false);
			Error item = valueTuple.Item1;
			Mod item2 = valueTuple.Item2;
			if (item)
			{
				ModioLog error = ModioLog.Error;
				if (error != null)
				{
					error.Log("[ModIOManager::GetModStatus] Failed to retrieve Mod " + modId.ToString() + "'s status: " + item.GetMessage());
				}
				modFileState = ModFileState.None;
			}
			else
			{
				modFileState = item2.File.State;
			}
		}
		return modFileState;
	}

	public static async Task<bool> DownloadMod(ModId modId, Action<bool> callback = null)
	{
		bool flag;
		if (!ModIOManager.hasInstance)
		{
			flag = false;
		}
		else
		{
			bool flag2 = await ModInstallationManagement.DownloadAndInstallMod(modId);
			if (callback != null)
			{
				callback(flag2);
			}
			flag = flag2;
		}
		return flag;
	}

	private void OnJoinedRoom()
	{
		if (NetworkSystem.Instance.RoomName.Contains(GorillaComputer.instance.VStumpRoomPrepend) && !GorillaComputer.instance.IsPlayerInVirtualStump() && !CustomMapManager.IsLocalPlayerInVirtualStump())
		{
			Debug.LogError("[ModIOManager::OnJoinedRoom] Player joined @ room while not in the VStump! Leaving the room...");
			NetworkSystem.Instance.ReturnToSinglePlayer();
		}
	}

	public static bool TryGetNewMapsModId(out ModId newMapsModId)
	{
		newMapsModId = ModId.Null;
		if (!ModIOManager.hasInstance)
		{
			return false;
		}
		newMapsModId = new ModId(ModIOManager.instance.newMapsModId);
		return true;
	}

	public static IEnumerator AssociateMothershipAndModIOAccounts(AssociateMotherhsipAndModIOAccountsRequest data, Action<AssociateMotherhsipAndModIOAccountsResponse> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.AuthApiBaseUrl + "/api/AssociatePlayFabAndModIO", "POST");
		string text = JsonUtility.ToJson(data);
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		bool retry = false;
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 15;
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
		{
			AssociateMotherhsipAndModIOAccountsResponse associateMotherhsipAndModIOAccountsResponse = JsonUtility.FromJson<AssociateMotherhsipAndModIOAccountsResponse>(request.downloadHandler.text);
			callback(associateMotherhsipAndModIOAccountsResponse);
		}
		else if (request.result == UnityWebRequest.Result.ProtocolError && request.responseCode != 400L)
		{
			retry = true;
			Debug.LogError(string.Format("HTTP {0} error: {1} message:{2}", request.responseCode, request.error, request.downloadHandler.text));
		}
		else if (request.result == UnityWebRequest.Result.ConnectionError)
		{
			retry = true;
			Debug.LogError("NETWORK ERROR: " + request.error + "\nMessage: " + request.downloadHandler.text);
		}
		else
		{
			Debug.LogError("HTTP ERROR: " + request.error + "\nMessage: " + request.downloadHandler.text);
			retry = true;
		}
		if (retry)
		{
			if (ModIOManager.currentAssociationRetries < ModIOManager.associationMaxRetries)
			{
				int num = (int)Mathf.Pow(2f, (float)(ModIOManager.currentAssociationRetries + 1));
				Debug.LogWarning(string.Format("Retrying Account Association... Retry attempt #{0}, waiting for {1} seconds", ModIOManager.currentAssociationRetries + 1, num));
				ModIOManager.currentAssociationRetries++;
				yield return new WaitForSeconds((float)num);
				ModIOManager.AssociateMothershipAndModIOAccounts(data, callback);
			}
			else
			{
				Debug.LogError("Maximum retries attempted. Please check your network connection.");
				callback(null);
			}
		}
		yield break;
	}

	private const string MODIO_ACCEPTED_TERMS_KEY = "modIOAcceptedTermsHash";

	private const string MODIO_ACCEPTED_TERMS_OF_USE_ID_KEY = "modIOAcceptedTermsOfUseId";

	private const string MODIO_ACCEPTED_PRIVACY_POLICY_ID_KEY = "modIOAcceptedPrivacyPolicyId";

	private const string MODIO_LAST_AUTH_METHOD_KEY = "modIOLassSuccessfulAuthMethod";

	private const string FAVORITES_FILE_NAME = "favoriteMods.json";

	private const float REFRESH_RATE_LIMIT = 5f;

	[OnEnterPlay_SetNull]
	private static volatile ModIOManager instance;

	[OnEnterPlay_Set(false)]
	private static bool hasInstance;

	private static string ModIODirectory;

	private static ModioWssAuthService accountLinkingAuthService = new ModioWssAuthService();

	private static bool initialized;

	private static bool refreshing;

	private static bool modManagementEnabled;

	private static bool loggingIn;

	private static bool loggingOut;

	private static bool refreshingModCache;

	private static bool favoriteModsLoaded;

	private static bool restartRefreshModCache;

	private static Coroutine refreshDisabledCoroutine;

	private static float lastRefreshTime;

	private static List<Action<bool>> currentRefreshCallbacks = new List<Action<bool>>();

	private static Action<ModIORequestResultAnd<bool>> modIOTermsAcknowledgedCallback;

	private static Dictionary<ModId, Mod> favoriteMods = new Dictionary<ModId, Mod>();

	private static Dictionary<ModId, int> outdatedModCMSVersions = new Dictionary<ModId, int>();

	private static byte[] ticketBlob = new byte[1024];

	private static uint ticketSize;

	protected static CallResult<EncryptedAppTicketResponse_t> requestEncryptedAppTicketResponse = null;

	private Action<bool, string> requestEncryptedAppTicketCallback;

	private static ModioSteamAuthService steamAuthService = new ModioSteamAuthService();

	[SerializeField]
	private GameObject modIOTermsOfUsePrefab;

	[SerializeField]
	private long newMapsModId;

	public static UnityEvent OnModIOLoginStarted = new UnityEvent();

	public static UnityEvent OnModIOLoggedIn = new UnityEvent();

	public static UnityEvent<string> OnModIOLoginFailed = new UnityEvent<string>();

	public static UnityEvent OnModIOLoggedOut = new UnityEvent();

	public static UnityEvent<User> OnModIOUserChanged = new UnityEvent<User>();

	public static UnityEvent<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase> OnModManagementEvent = new UnityEvent<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase>();

	public static UnityEvent OnModIOCacheRefreshing = new UnityEvent();

	public static UnityEvent OnModIOCacheRefreshed = new UnityEvent();

	private static int associationMaxRetries = 5;

	private static int currentAssociationRetries = 0;

	public enum ModIOAuthMethod
	{
		Invalid,
		LinkedAccount,
		Steam,
		Oculus
	}
}
