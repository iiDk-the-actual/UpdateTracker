using System;
using GorillaNetworking;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Modio.Mods;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CustomMapsTerminal : MonoBehaviour
{
	public static int LocalPlayerID
	{
		get
		{
			return NetworkSystem.Instance.LocalPlayer.ActorNumber;
		}
	}

	public static long LocalModDetailsID
	{
		get
		{
			return CustomMapsTerminal.localModDetailsID;
		}
	}

	public static int CurrentScreen
	{
		get
		{
			return (int)CustomMapsTerminal.localCurrentScreen;
		}
	}

	public static bool IsDriver
	{
		get
		{
			return CustomMapsTerminal.localDriverID == CustomMapsTerminal.LocalPlayerID;
		}
	}

	private void Awake()
	{
		CustomMapsTerminal.instance = this;
		CustomMapsTerminal.hasInstance = true;
	}

	private void Start()
	{
		CustomMapsTerminal.localDriverID = -2;
		CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.TerminalControlPrompt;
		CustomMapsTerminal.previousScreen = CustomMapsTerminal.ScreenType.TerminalControlPrompt;
		this.controlAccessScreen.Show();
		this.detailsAccessScreen.Show();
		this.modListScreen.Hide();
		this.modDetailsScreen.Hide();
		ModIOManager.OnModIOLoggedIn.AddListener(new UnityAction(this.OnModIOLoggedIn));
		ModIOManager.OnModIOLoggedOut.AddListener(new UnityAction(this.OnModIOLoggedOut));
		NetworkSystem.Instance.OnMultiplayerStarted += this.OnJoinedRoom;
		NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnReturnedToSinglePlayer;
	}

	private void OnDestroy()
	{
		ModIOManager.OnModIOLoggedIn.RemoveListener(new UnityAction(this.OnModIOLoggedIn));
		ModIOManager.OnModIOLoggedOut.RemoveListener(new UnityAction(this.OnModIOLoggedOut));
		NetworkSystem.Instance.OnMultiplayerStarted -= this.OnJoinedRoom;
		NetworkSystem.Instance.OnReturnedToSinglePlayer -= this.OnReturnedToSinglePlayer;
	}

	public static void ShowDetailsScreen(Mod mod)
	{
		CustomMapsTerminal.previousScreen = CustomMapsTerminal.localCurrentScreen;
		CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.ModDetails;
		CustomMapsTerminal.localModDetailsID = mod.Id;
		CustomMapsTerminal.instance.modListScreen.Hide();
		CustomMapsTerminal.instance.controlAccessScreen.Hide();
		CustomMapsTerminal.instance.detailsAccessScreen.Hide();
		CustomMapsTerminal.instance.modDetailsScreen.Show();
		CustomMapsTerminal.instance.modDetailsScreen.SetModProfile(mod);
		CustomMapsTerminal.instance.modDisplayScreen.Show();
		CustomMapsTerminal.instance.modDisplayScreen.SetModProfile(mod);
		CustomMapsTerminal.instance.modSearchScreen.Hide();
		CustomMapsTerminal.SendTerminalStatus();
	}

	public static void ReturnFromDetailsScreen()
	{
		CustomMapsTerminal.ScreenType screenType = CustomMapsTerminal.previousScreen;
		if (screenType == CustomMapsTerminal.ScreenType.ModDetails || screenType == CustomMapsTerminal.ScreenType.Invalid || screenType == CustomMapsTerminal.ScreenType.TerminalControlPrompt)
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.AvailableMods;
			CustomMapsTerminal.previousScreen = CustomMapsTerminal.ScreenType.AvailableMods;
		}
		else
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.previousScreen;
		}
		switch (CustomMapsTerminal.localCurrentScreen)
		{
		case CustomMapsTerminal.ScreenType.TerminalControlPrompt:
			CustomMapsTerminal.instance.modListScreen.Hide();
			CustomMapsTerminal.instance.modDetailsScreen.Hide();
			CustomMapsTerminal.instance.modDisplayScreen.Hide();
			CustomMapsTerminal.instance.modSearchScreen.Hide();
			CustomMapsTerminal.instance.controlAccessScreen.Show();
			CustomMapsTerminal.instance.detailsAccessScreen.Show();
			break;
		case CustomMapsTerminal.ScreenType.AvailableMods:
		case CustomMapsTerminal.ScreenType.InstalledMods:
		case CustomMapsTerminal.ScreenType.FavoriteMods:
		case CustomMapsTerminal.ScreenType.SubscribedMods:
			CustomMapsTerminal.instance.modListScreen.Show();
			CustomMapsTerminal.instance.modSearchScreen.Hide();
			CustomMapsTerminal.instance.modDetailsScreen.Hide();
			CustomMapsTerminal.instance.modDisplayScreen.Hide();
			CustomMapsTerminal.instance.controlAccessScreen.Hide();
			CustomMapsTerminal.instance.detailsAccessScreen.Show();
			break;
		case CustomMapsTerminal.ScreenType.SearchMods:
			CustomMapsTerminal.instance.modListScreen.Hide();
			CustomMapsTerminal.instance.modSearchScreen.ReturnFromDetailsScreen();
			CustomMapsTerminal.instance.modDetailsScreen.Hide();
			CustomMapsTerminal.instance.modDisplayScreen.Hide();
			CustomMapsTerminal.instance.controlAccessScreen.Hide();
			CustomMapsTerminal.instance.detailsAccessScreen.Show();
			break;
		}
		CustomMapsTerminal.SendTerminalStatus();
	}

	public static void ShowSearchScreen()
	{
		CustomMapsTerminal.previousScreen = CustomMapsTerminal.localCurrentScreen;
		CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.SearchMods;
		CustomMapsTerminal.instance.modListScreen.Hide();
		CustomMapsTerminal.instance.controlAccessScreen.Hide();
		CustomMapsTerminal.instance.detailsAccessScreen.SetDetailsScreenForDriver();
		CustomMapsTerminal.instance.detailsAccessScreen.Show();
		CustomMapsTerminal.instance.modDetailsScreen.Hide();
		CustomMapsTerminal.instance.modDisplayScreen.Hide();
		CustomMapsTerminal.instance.modSearchScreen.Show();
		CustomMapsTerminal.SendTerminalStatus();
	}

	public static void ReturnFromSearchScreen()
	{
		CustomMapsTerminal.ScreenType screenType = CustomMapsTerminal.previousScreen;
		if (screenType == CustomMapsTerminal.ScreenType.ModDetails || screenType == CustomMapsTerminal.ScreenType.Invalid || screenType == CustomMapsTerminal.ScreenType.TerminalControlPrompt || screenType == CustomMapsTerminal.ScreenType.SearchMods)
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.AvailableMods;
			CustomMapsTerminal.previousScreen = CustomMapsTerminal.ScreenType.AvailableMods;
		}
		else
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.previousScreen;
		}
		switch (CustomMapsTerminal.localCurrentScreen)
		{
		case CustomMapsTerminal.ScreenType.TerminalControlPrompt:
			CustomMapsTerminal.instance.modListScreen.Hide();
			CustomMapsTerminal.instance.modSearchScreen.Hide();
			CustomMapsTerminal.instance.modDetailsScreen.Hide();
			CustomMapsTerminal.instance.modDisplayScreen.Hide();
			CustomMapsTerminal.instance.controlAccessScreen.Show();
			CustomMapsTerminal.instance.detailsAccessScreen.Show();
			break;
		case CustomMapsTerminal.ScreenType.AvailableMods:
		case CustomMapsTerminal.ScreenType.InstalledMods:
		case CustomMapsTerminal.ScreenType.FavoriteMods:
		case CustomMapsTerminal.ScreenType.SubscribedMods:
			CustomMapsTerminal.instance.modListScreen.Show();
			CustomMapsTerminal.instance.modSearchScreen.Hide();
			CustomMapsTerminal.instance.modDetailsScreen.Hide();
			CustomMapsTerminal.instance.modDisplayScreen.Hide();
			CustomMapsTerminal.instance.controlAccessScreen.Hide();
			CustomMapsTerminal.instance.detailsAccessScreen.Show();
			break;
		}
		CustomMapsTerminal.SendTerminalStatus();
	}

	public static void SendTerminalStatus()
	{
		if (!CustomMapsTerminal.hasInstance)
		{
			return;
		}
		CustomMapsTerminal.instance.mapTerminalNetworkObject.SendTerminalStatus();
	}

	public static void ResetTerminalControl()
	{
		CustomMapsTerminal.localDriverID = -2;
		CustomMapsTerminal.instance.terminalControlButton.UnlockTerminalControl();
		CustomMapsTerminal.ShowTerminalControlScreen();
	}

	public static void HandleTerminalControlStatusChangeRequest(bool lockedStatus, int playerID)
	{
		if (lockedStatus && playerID == -2)
		{
			return;
		}
		if (CustomMapsTerminal.localDriverID == -2)
		{
			if (!lockedStatus)
			{
				return;
			}
		}
		else if (CustomMapsTerminal.localDriverID != playerID)
		{
			return;
		}
		CustomMapsTerminal.SetTerminalControlStatus(lockedStatus, playerID, true);
	}

	public static void SetTerminalControlStatus(bool isLocked, int driverID = -2, bool sendRPC = false)
	{
		GTDev.Log<string>(string.Format("[CustomMapsTerminal::SetTerminalControlStatus] isLocked: {0} | driverID: {1} | playerId {2} | sendRPC: {3}", new object[]
		{
			isLocked,
			driverID,
			CustomMapsTerminal.LocalPlayerID,
			sendRPC
		}), null);
		if (isLocked)
		{
			CustomMapsTerminal.localDriverID = driverID;
			CustomMapsTerminal.instance.terminalControlButton.LockTerminalControl();
			if (CustomMapsTerminal.IsDriver)
			{
				CustomMapsTerminal.HideTerminalControlScreens();
			}
			else
			{
				CustomMapsTerminal.ShowTerminalControlScreen();
			}
		}
		else
		{
			CustomMapsTerminal.localDriverID = -2;
			CustomMapsTerminal.instance.terminalControlButton.UnlockTerminalControl();
			CustomMapsTerminal.ShowTerminalControlScreen();
		}
		if (sendRPC && NetworkSystem.Instance.IsMasterClient)
		{
			CustomMapsTerminal.instance.mapTerminalNetworkObject.SetTerminalControlStatus(isLocked, CustomMapsTerminal.localDriverID);
		}
	}

	public static void UpdateFromDriver(int currentScreen, long modDetailsID, int driverID)
	{
		if (!CustomMapsTerminal.hasInstance)
		{
			return;
		}
		CustomMapsTerminal.localDriverID = driverID;
		CustomMapsTerminal.cachedModDetailsID = modDetailsID;
		CustomMapsTerminal.localModDetailsID = modDetailsID;
		CustomMapsTerminal.cachedCurrentScreen = (CustomMapsTerminal.ScreenType)currentScreen;
		CustomMapsTerminal.localCurrentScreen = (CustomMapsTerminal.ScreenType)currentScreen;
		Debug.Log(string.Format("[CustomMapsTerminal::UpdateFromDriver] currentScreen {0} modDetailsID {1}", CustomMapsTerminal.localCurrentScreen, CustomMapsTerminal.localModDetailsID));
		if (CustomMapsTerminal.localDriverID != -2)
		{
			CustomMapsTerminal.RefreshDriverNickName();
		}
		CustomMapsTerminal.ScreenType screenType = CustomMapsTerminal.localCurrentScreen;
		if (screenType <= CustomMapsTerminal.ScreenType.SearchMods)
		{
			CustomMapsTerminal.ShowTerminalControlScreen();
			return;
		}
		if (screenType != CustomMapsTerminal.ScreenType.ModDetails)
		{
			return;
		}
		CustomMapsTerminal.ShowTerminalControlScreen();
		if (CustomMapsTerminal.localModDetailsID <= 0L)
		{
			return;
		}
		CustomMapsTerminal.instance.detailsAccessScreen.Hide();
		CustomMapsTerminal.instance.modDisplayScreen.Show();
		CustomMapsTerminal.instance.modDisplayScreen.RetrieveModFromModIO(CustomMapsTerminal.localModDetailsID, false, null);
	}

	private void UpdateControlScreenForDriver()
	{
		GTDev.Log<string>(string.Format("[CustomMapsTerminal::UpdateScreenToMatchStatus] driverID: {0} ", CustomMapsTerminal.localDriverID) + string.Format("| currentScreen: {0} ", CustomMapsTerminal.localCurrentScreen) + string.Format("| previousScreen: {0} ", CustomMapsTerminal.previousScreen), null);
		switch (CustomMapsTerminal.localCurrentScreen)
		{
		case CustomMapsTerminal.ScreenType.TerminalControlPrompt:
			return;
		case CustomMapsTerminal.ScreenType.AvailableMods:
		case CustomMapsTerminal.ScreenType.InstalledMods:
		case CustomMapsTerminal.ScreenType.FavoriteMods:
		case CustomMapsTerminal.ScreenType.SubscribedMods:
			this.controlAccessScreen.Hide();
			this.modSearchScreen.Hide();
			this.detailsAccessScreen.SetDetailsScreenForDriver();
			this.detailsAccessScreen.Show();
			this.modListScreen.Show();
			this.modDetailsScreen.Hide();
			this.modDisplayScreen.Hide();
			return;
		case CustomMapsTerminal.ScreenType.SearchMods:
			this.controlAccessScreen.Hide();
			this.modSearchScreen.Show();
			this.detailsAccessScreen.SetDetailsScreenForDriver();
			this.detailsAccessScreen.Show();
			this.modListScreen.Hide();
			this.modDetailsScreen.Hide();
			this.modDisplayScreen.Hide();
			return;
		case CustomMapsTerminal.ScreenType.ModDetails:
			this.controlAccessScreen.Hide();
			this.modSearchScreen.Hide();
			this.detailsAccessScreen.Hide();
			this.modListScreen.Hide();
			this.modDetailsScreen.Show();
			this.modDetailsScreen.RetrieveModFromModIO(CustomMapsTerminal.localModDetailsID, false, null);
			this.modDisplayScreen.Show();
			this.modDisplayScreen.RetrieveModFromModIO(CustomMapsTerminal.localModDetailsID, false, null);
			return;
		default:
			return;
		}
	}

	private void ValidateLocalStatus()
	{
		if (CustomMapsTerminal.localDriverID == -2)
		{
			return;
		}
		if (CustomMapLoader.IsMapLoaded())
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.ModDetails;
			CustomMapsTerminal.localModDetailsID = CustomMapLoader.LoadedMapModId;
			CustomMapsTerminal.SendTerminalStatus();
			return;
		}
		if (CustomMapManager.IsLoading())
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.ModDetails;
			CustomMapsTerminal.localModDetailsID = CustomMapManager.LoadingMapId;
			CustomMapsTerminal.SendTerminalStatus();
			return;
		}
		if (CustomMapManager.GetRoomMapId() != ModId.Null)
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.ModDetails;
			CustomMapsTerminal.localModDetailsID = CustomMapManager.GetRoomMapId()._id;
			CustomMapsTerminal.SendTerminalStatus();
		}
	}

	private void OnModIOLoggedIn()
	{
	}

	private void OnModIOLoggedOut()
	{
		if (CustomMapsTerminal.localCurrentScreen == CustomMapsTerminal.ScreenType.SubscribedMods)
		{
			if (this.modListScreen.isActiveAndEnabled)
			{
				this.modListScreen.SwapListDisplay(CustomMapsListScreen.ListScreenState.AvailableMods, false);
			}
			else
			{
				CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.AvailableMods;
			}
		}
		if (CustomMapsTerminal.previousScreen == CustomMapsTerminal.ScreenType.SubscribedMods)
		{
			CustomMapsTerminal.previousScreen = CustomMapsTerminal.ScreenType.AvailableMods;
		}
	}

	public void HandleTerminalControlButtonPressed()
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			CustomMapsTerminal.SetTerminalControlStatus(!this.terminalControlButton.IsLocked, CustomMapsTerminal.LocalPlayerID, false);
			return;
		}
		if (CustomMapsTerminal.localDriverID != -2 && !CustomMapsTerminal.IsDriver)
		{
			return;
		}
		if (this.mapTerminalNetworkObject.HasAuthority)
		{
			CustomMapsTerminal.HandleTerminalControlStatusChangeRequest(!this.terminalControlButton.IsLocked, CustomMapsTerminal.LocalPlayerID);
			return;
		}
		this.mapTerminalNetworkObject.RequestTerminalControlStatusChange(!this.terminalControlButton.IsLocked);
	}

	private static void ShowTerminalControlScreen()
	{
		if (!CustomMapsTerminal.hasInstance)
		{
			return;
		}
		if (CustomMapsTerminal.localDriverID == -2)
		{
			CustomMapsTerminal.instance.controlAccessScreen.Reset();
			CustomMapsTerminal.instance.detailsAccessScreen.Reset();
		}
		else
		{
			CustomMapsTerminal.instance.controlAccessScreen.SetDriverName();
			CustomMapsTerminal.instance.detailsAccessScreen.SetDriverName();
		}
		CustomMapsTerminal.instance.modListScreen.Hide();
		CustomMapsTerminal.instance.modDetailsScreen.Hide();
		CustomMapsTerminal.instance.modDisplayScreen.Hide();
		CustomMapsTerminal.instance.controlAccessScreen.Show();
		CustomMapsTerminal.instance.detailsAccessScreen.Show();
		CustomMapsTerminal.instance.modSearchScreen.Hide();
		CustomMapsTerminal.previousScreen = CustomMapsTerminal.localCurrentScreen;
		CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.TerminalControlPrompt;
	}

	private static void HideTerminalControlScreens()
	{
		if (!CustomMapsTerminal.hasInstance)
		{
			return;
		}
		if (CustomMapsTerminal.localCurrentScreen != CustomMapsTerminal.ScreenType.TerminalControlPrompt)
		{
			return;
		}
		if (CustomMapsTerminal.previousScreen > CustomMapsTerminal.ScreenType.TerminalControlPrompt)
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.previousScreen;
			if ((CustomMapsTerminal.localCurrentScreen == CustomMapsTerminal.ScreenType.SubscribedMods || CustomMapsTerminal.localCurrentScreen == CustomMapsTerminal.ScreenType.FavoriteMods) && !ModIOManager.IsLoggedIn())
			{
				CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.AvailableMods;
			}
		}
		else if (CustomMapLoader.IsMapLoaded() || CustomMapManager.IsLoading() || CustomMapManager.GetRoomMapId() != ModId.Null)
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.ModDetails;
		}
		else
		{
			CustomMapsTerminal.localCurrentScreen = CustomMapsTerminal.ScreenType.AvailableMods;
		}
		CustomMapsTerminal.instance.UpdateControlScreenForDriver();
	}

	public static void RequestDriverNickNameRefresh()
	{
		if (!CustomMapsTerminal.hasInstance)
		{
			return;
		}
		if (!CustomMapsTerminal.IsDriver)
		{
			return;
		}
		CustomMapsTerminal.RefreshDriverNickName();
		CustomMapsTerminal.instance.mapTerminalNetworkObject.RefreshDriverNickName();
	}

	public static void RefreshDriverNickName()
	{
		if (!CustomMapsTerminal.hasInstance)
		{
			return;
		}
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags);
		CustomMapsTerminal.instance.terminalControllerLabelText.gameObject.SetActive(true);
		if (NetworkSystem.Instance.InRoom)
		{
			NetPlayer netPlayerByID = NetworkSystem.Instance.GetNetPlayerByID(CustomMapsTerminal.localDriverID);
			CustomMapsTerminal.instance.terminalControllerText.text = netPlayerByID.DefaultName;
			if (GorillaComputer.instance.NametagsEnabled && flag)
			{
				RigContainer rigContainer;
				if (netPlayerByID.IsLocal)
				{
					CustomMapsTerminal.instance.terminalControllerText.text = netPlayerByID.NickName;
				}
				else if (VRRigCache.Instance.TryGetVrrig(netPlayerByID, out rigContainer))
				{
					CustomMapsTerminal.instance.terminalControllerText.text = rigContainer.Rig.playerNameVisible;
				}
			}
		}
		else
		{
			CustomMapsTerminal.instance.terminalControllerText.text = ((GorillaComputer.instance.NametagsEnabled && flag) ? NetworkSystem.Instance.LocalPlayer.NickName : NetworkSystem.Instance.LocalPlayer.DefaultName);
		}
		CustomMapsTerminal.instance.terminalControllerText.gameObject.SetActive(true);
		CustomMapsTerminal.instance.modListScreen.RefreshDriverNickname(CustomMapsTerminal.instance.terminalControllerText.text);
	}

	private void OnReturnedToSinglePlayer()
	{
		if (CustomMapsTerminal.localDriverID != CustomMapsTerminal.cachedLocalPlayerID)
		{
			CustomMapsTerminal.ResetTerminalControl();
		}
		else
		{
			CustomMapsTerminal.localDriverID = CustomMapsTerminal.LocalPlayerID;
		}
		CustomMapsTerminal.cachedLocalPlayerID = -1;
	}

	private void OnJoinedRoom()
	{
		CustomMapsTerminal.cachedLocalPlayerID = CustomMapsTerminal.LocalPlayerID;
		CustomMapsTerminal.ResetTerminalControl();
	}

	public static bool IsLocked()
	{
		return CustomMapsTerminal.localDriverID != -2;
	}

	public static int GetDriverID()
	{
		return CustomMapsTerminal.localDriverID;
	}

	public static string GetDriverNickname()
	{
		if (!CustomMapsTerminal.hasInstance)
		{
			return "";
		}
		return CustomMapsTerminal.instance.terminalControllerText.text;
	}

	[SerializeField]
	private CustomMapsAccessScreen controlAccessScreen;

	[SerializeField]
	private CustomMapsAccessScreen detailsAccessScreen;

	[SerializeField]
	private CustomMapsListScreen modListScreen;

	[SerializeField]
	private CustomMapsDetailsScreen modDetailsScreen;

	[SerializeField]
	private CustomMapsDisplayScreen modDisplayScreen;

	[SerializeField]
	private CustomMapsSearchScreen modSearchScreen;

	[SerializeField]
	private VirtualStumpSerializer mapTerminalNetworkObject;

	[SerializeField]
	private CustomMapsTerminalControlButton terminalControlButton;

	[SerializeField]
	private TMP_Text terminalControllerLabelText;

	[SerializeField]
	private TMP_Text terminalControllerText;

	public const int NO_DRIVER_ID = -2;

	private static CustomMapsTerminal instance;

	private static bool hasInstance;

	private static long localModDetailsID = -1L;

	private static long cachedModDetailsID = -1L;

	private static int localDriverID = -1;

	private static int cachedLocalPlayerID = -1;

	private static CustomMapsTerminal.ScreenType localCurrentScreen = CustomMapsTerminal.ScreenType.Invalid;

	private static CustomMapsTerminal.ScreenType cachedCurrentScreen = CustomMapsTerminal.ScreenType.Invalid;

	private static CustomMapsTerminal.ScreenType previousScreen = CustomMapsTerminal.ScreenType.Invalid;

	public enum ScreenType
	{
		Invalid = -1,
		TerminalControlPrompt,
		AvailableMods,
		InstalledMods,
		FavoriteMods,
		SubscribedMods,
		SearchMods,
		ModDetails
	}
}
