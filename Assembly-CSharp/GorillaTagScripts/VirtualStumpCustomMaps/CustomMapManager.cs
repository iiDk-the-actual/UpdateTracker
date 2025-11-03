using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using GorillaNetworking;
using GorillaTag.Rendering;
using GorillaTagScripts.CustomMapSupport;
using GorillaTagScripts.UI.ModIO;
using GT_CustomMapSupportRuntime;
using Modio;
using Modio.Mods;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.VirtualStumpCustomMaps
{
	public class CustomMapManager : MonoBehaviour, IBuildValidation
	{
		public static bool WaitingForRoomJoin
		{
			get
			{
				return CustomMapManager.waitingForRoomJoin;
			}
		}

		public static bool WaitingForDisconnect
		{
			get
			{
				return CustomMapManager.waitingForDisconnect;
			}
		}

		public static long LoadingMapId
		{
			get
			{
				return CustomMapManager.loadingMapId;
			}
		}

		public static long UnloadingMapId
		{
			get
			{
				return CustomMapManager.unloadingMapId;
			}
		}

		public bool BuildValidationCheck()
		{
			if (this.defaultTeleporter.IsNull())
			{
				Debug.LogError("CustomMapManager does not have its \"Default Teleporter\" property.");
				return false;
			}
			return true;
		}

		private void Awake()
		{
			if (CustomMapManager.instance == null)
			{
				CustomMapManager.instance = this;
				CustomMapManager.hasInstance = true;
				return;
			}
			if (CustomMapManager.instance != this)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void OnEnable()
		{
			UGCPermissionManager.UnsubscribeFromUGCEnabled(new Action(this.OnUGCEnabled));
			UGCPermissionManager.SubscribeToUGCEnabled(new Action(this.OnUGCEnabled));
			UGCPermissionManager.UnsubscribeFromUGCDisabled(new Action(this.OnUGCDisabled));
			UGCPermissionManager.SubscribeToUGCDisabled(new Action(this.OnUGCDisabled));
			CMSSerializer.OnTriggerHistoryProcessedForScene.RemoveListener(new UnityAction<string>(CustomMapManager.OnSceneTriggerHistoryProcessed));
			CMSSerializer.OnTriggerHistoryProcessedForScene.AddListener(new UnityAction<string>(CustomMapManager.OnSceneTriggerHistoryProcessed));
			ModIOManager.OnModManagementEvent.RemoveListener(new UnityAction<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase>(this.HandleModManagementEvent));
			ModIOManager.OnModManagementEvent.AddListener(new UnityAction<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase>(this.HandleModManagementEvent));
			RoomSystem.JoinedRoomEvent -= new Action(this.OnJoinedRoom);
			RoomSystem.JoinedRoomEvent += new Action(this.OnJoinedRoom);
			NetworkSystem.Instance.OnReturnedToSinglePlayer -= this.OnDisconnected;
			NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnDisconnected;
		}

		public void OnDisable()
		{
			UGCPermissionManager.UnsubscribeFromUGCEnabled(new Action(this.OnUGCEnabled));
			UGCPermissionManager.UnsubscribeFromUGCDisabled(new Action(this.OnUGCDisabled));
			CMSSerializer.OnTriggerHistoryProcessedForScene.RemoveListener(new UnityAction<string>(CustomMapManager.OnSceneTriggerHistoryProcessed));
			ModIOManager.OnModManagementEvent.RemoveListener(new UnityAction<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase>(this.HandleModManagementEvent));
			RoomSystem.JoinedRoomEvent -= new Action(this.OnJoinedRoom);
			NetworkSystem.Instance.OnReturnedToSinglePlayer -= this.OnDisconnected;
		}

		private void OnUGCEnabled()
		{
		}

		private void OnUGCDisabled()
		{
		}

		private void Start()
		{
			CustomMapLoader.Initialize(new Action<MapLoadStatus, int, string>(CustomMapManager.OnMapLoadProgress), new Action<bool>(CustomMapManager.OnMapLoadFinished), new Action<string>(CustomMapManager.OnSceneLoaded), new Action<string>(CustomMapManager.OnSceneUnloaded));
			for (int i = this.virtualStumpTeleportLocations.Count - 1; i >= 0; i--)
			{
				if (this.virtualStumpTeleportLocations[i] == null)
				{
					this.virtualStumpTeleportLocations.RemoveAt(i);
				}
			}
			if (this.defaultTeleporter.IsNull())
			{
				GTDev.LogError<string>("[CustomMapManager::Start] \"Default Teleporter\" property is invalid.", null);
			}
			this.virtualStumpToggleableRoot.SetActive(false);
			base.gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			if (CustomMapManager.instance == this)
			{
				CustomMapManager.instance = null;
				CustomMapManager.hasInstance = false;
			}
			UGCPermissionManager.UnsubscribeFromUGCEnabled(new Action(this.OnUGCEnabled));
			UGCPermissionManager.UnsubscribeFromUGCDisabled(new Action(this.OnUGCDisabled));
			CMSSerializer.OnTriggerHistoryProcessedForScene.RemoveListener(new UnityAction<string>(CustomMapManager.OnSceneTriggerHistoryProcessed));
			ModIOManager.OnModManagementEvent.RemoveListener(new UnityAction<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase>(this.HandleModManagementEvent));
			RoomSystem.JoinedRoomEvent -= new Action(this.OnJoinedRoom);
			NetworkSystem.Instance.OnReturnedToSinglePlayer -= this.OnDisconnected;
		}

		private void HandleModManagementEvent(Mod mod, Modfile modfile, ModInstallationManagement.OperationType jobType, ModInstallationManagement.OperationPhase jobPhase)
		{
			if (CustomMapManager.waitingForModInstall && CustomMapManager.waitingForModInstallId == mod.Id)
			{
				if (CustomMapManager.abortModLoadIds.Contains(mod.Id))
				{
					CustomMapManager.abortModLoadIds.Remove(mod.Id);
					if (CustomMapManager.waitingForModInstallId.Equals(mod.Id))
					{
						CustomMapManager.waitingForModInstall = false;
						CustomMapManager.waitingForModDownload = false;
						CustomMapManager.waitingForModInstallId = ModId.Null;
					}
					return;
				}
				switch (modfile.State)
				{
				case ModFileState.Downloading:
				case ModFileState.Updating:
					CustomMapManager.waitingForModDownload = true;
					return;
				case ModFileState.Downloaded:
					CustomMapManager.waitingForModDownload = false;
					return;
				case ModFileState.Installing:
				case ModFileState.Uninstalling:
					break;
				case ModFileState.Installed:
					CustomMapManager.waitingForModDownload = false;
					this.LoadInstalledMap(mod);
					break;
				case ModFileState.FileOperationFailed:
					switch (jobType)
					{
					case ModInstallationManagement.OperationType.Download:
						Debug.LogError("[CustomMapManager::HandleModManagementEvent] Failed to download map with modID " + mod.Id.ToString() + ", error: " + modfile.FileStateErrorCause.GetMessage());
						CustomMapManager.HandleMapLoadFailed("FAILED TO DOWNLOAD MAP: " + modfile.FileStateErrorCause.GetMessage());
						CustomMapManager.waitingForModDownload = false;
						return;
					case ModInstallationManagement.OperationType.Install:
						Debug.LogError("[CustomMapManager::HandleModManagementEvent] Failed to install map with modID " + mod.Id.ToString() + ", error: " + modfile.FileStateErrorCause.GetMessage());
						CustomMapManager.HandleMapLoadFailed("FAILED TO INSTALL MAP: " + modfile.FileStateErrorCause.GetMessage());
						return;
					case ModInstallationManagement.OperationType.Update:
						Debug.LogError("[CustomMapManager::HandleModManagementEvent] Failed to update map with modID " + mod.Id.ToString() + ", error: " + modfile.FileStateErrorCause.GetMessage());
						CustomMapManager.HandleMapLoadFailed("FAILED TO UPDATE MAP: " + modfile.FileStateErrorCause.GetMessage());
						return;
					default:
						return;
					}
					break;
				default:
					return;
				}
			}
		}

		internal static void TeleportToVirtualStump(VirtualStumpTeleporter fromTeleporter, Action<bool> callback)
		{
			if (UGCPermissionManager.IsUGCDisabled)
			{
				return;
			}
			if (!CustomMapManager.hasInstance || fromTeleporter == null)
			{
				if (callback != null)
				{
					callback(false);
				}
				return;
			}
			CustomMapManager.instance.gameObject.SetActive(true);
			CustomMapManager.instance.StartCoroutine(CustomMapManager.Internal_TeleportToVirtualStump(fromTeleporter, callback));
		}

		private static IEnumerator Internal_TeleportToVirtualStump(VirtualStumpTeleporter fromTeleporter, Action<bool> callback)
		{
			CustomMapManager.lastUsedTeleporter = fromTeleporter;
			CustomMapManager.preVStumpGamemode = GorillaComputer.instance.currentGameMode.Value;
			if (CustomMapManager.lastUsedTeleporter.GetAutoLoadGamemode() != GameModeType.None && CustomMapManager.lastUsedTeleporter.GetAutoLoadGamemode() != GameModeType.Count)
			{
				GorillaComputer.instance.SetGameModeWithoutButton(CustomMapManager.lastUsedTeleporter.GetAutoLoadGamemode().ToString());
			}
			GTDev.Log<string>("[CustomMapManager::TeleportToVirtualStump] Teleporting to Virtual Stump...", null);
			PrivateUIRoom.ForceStartOverlay();
			GorillaTagger.Instance.overrideNotInFocus = true;
			GreyZoneManager greyZoneManager = GreyZoneManager.Instance;
			if (greyZoneManager != null)
			{
				greyZoneManager.ForceStopGreyZone();
			}
			if (CustomMapManager.instance.virtualStumpTeleportLocations.Count > 0)
			{
				int num = Random.Range(0, CustomMapManager.instance.virtualStumpTeleportLocations.Count);
				Transform randTeleportTarget = CustomMapManager.instance.virtualStumpTeleportLocations[num];
				CustomMapManager.instance.EnableTeleportHUD(true);
				CustomMapManager.lastUsedTeleporter.PlayTeleportEffects(true, true, CustomMapManager.instance.localTeleportSFXSource, true);
				yield return new WaitForSeconds(0.75f);
				CosmeticsController.instance.ClearCheckoutAndCart(false);
				CustomMapManager.instance.virtualStumpToggleableRoot.SetActive(true);
				GTPlayer.Instance.TeleportTo(randTeleportTarget, true, false);
				GorillaComputer.instance.SetInVirtualStump(true);
				yield return null;
				if (VRRig.LocalRig.IsNotNull() && VRRig.LocalRig.zoneEntity.IsNotNull())
				{
					VRRig.LocalRig.zoneEntity.DisableZoneChanges();
				}
				ZoneManagement.SetActiveZone(GTZone.customMaps);
				foreach (GameObject gameObject in CustomMapManager.instance.rootObjectsToDeactivateAfterTeleport)
				{
					if (gameObject != null)
					{
						gameObject.gameObject.SetActive(false);
					}
				}
				if (CustomMapManager.hasInstance && CustomMapManager.instance.virtualStumpZoneShaderSettings.IsNotNull())
				{
					CustomMapManager.instance.virtualStumpZoneShaderSettings.BecomeActiveInstance(false);
				}
				else
				{
					ZoneShaderSettings.ActivateDefaultSettings();
				}
				CustomMapManager.instance.ghostReactorManager.reactor.EnableGhostReactorForVirtualStump();
				CustomMapManager.currentTeleportCallback = callback;
				CustomMapManager.pendingNewPrivateRoomName = "";
				CustomMapManager.preTeleportInPrivateRoom = false;
				if (NetworkSystem.Instance.InRoom)
				{
					if (NetworkSystem.Instance.SessionIsPrivate)
					{
						CustomMapManager.preTeleportInPrivateRoom = true;
						CustomMapManager.waitingForRoomJoin = true;
						CustomMapManager.pendingNewPrivateRoomName = GorillaComputer.instance.VStumpRoomPrepend + NetworkSystem.Instance.RoomName;
					}
					GTDev.Log<string>("[CustomMapManager::TeleportToVirtualStump] Returning to singleplayer...", null);
					CustomMapManager.waitingForLoginDisconnect = true;
					NetworkSystem.Instance.ReturnToSinglePlayer();
				}
				else
				{
					GTDev.Log<string>("[CustomMapManager::TeleportToVirtualStump] Attempting auto-login to mod.io...", null);
					CustomMapManager.AttemptAutoLogin();
				}
				randTeleportTarget = null;
			}
			else
			{
				GTDev.Log<string>("[CustomMapManager::TeleportToVirtualStump] Not Teleporting, virtualStumpTeleportLocations is empty!", null);
				CustomMapManager.EndTeleport(false);
			}
			yield break;
		}

		private static void OnAutoLoginComplete(Error error)
		{
			GTDev.Log<string>(string.Format("[CustomMapManager::OnAutoLoginComplete] Error: {0}", error), null);
			if (!CustomMapManager.hasInstance)
			{
				Debug.LogError("[CustomMapManager::OnAutoLoginComplete] CustomMapManager not initialized!");
				return;
			}
			GTDev.Log<string>(string.Format("[CustomMapManager::OnAutoLoginComplete] Needs to rejoin private room: {0}", CustomMapManager.preTeleportInPrivateRoom), null);
			if (CustomMapManager.preTeleportInPrivateRoom)
			{
				if (NetworkSystem.Instance.netState != NetSystemState.Idle)
				{
					GTDev.Log<string>(string.Format("[CustomMapManager::OnAutoLoginComplete] Netstate not Idle, delaying join attempt. CurrentStatus: {0}", NetworkSystem.Instance.netState), null);
					CustomMapManager.delayedJoinCoroutine = CustomMapManager.instance.StartCoroutine(CustomMapManager.DelayedJoinVStumpPrivateRoom());
				}
				else
				{
					GTDev.Log<string>("[CustomMapManager::OnAutoLoginComplete] joining @ version of private room: " + CustomMapManager.pendingNewPrivateRoomName, null);
					PhotonNetworkController.Instance.AttemptToJoinSpecificRoomWithCallback(CustomMapManager.pendingNewPrivateRoomName, JoinType.Solo, new Action<NetJoinResult>(CustomMapManager.OnJoinSpecificRoomResult));
				}
			}
			GTDev.Log<string>(string.Format("[CustomMapManager::OnAutoLoginComplete] Waiting For D/C? {0}", CustomMapManager.waitingForDisconnect), null);
			if (!CustomMapManager.preTeleportInPrivateRoom && !CustomMapManager.waitingForDisconnect)
			{
				GTDev.Log<string>("[CustomMapManager::OnAutoLoginComplete] Ending teleport...", null);
				CustomMapManager.EndTeleport(true);
			}
			CustomMapManager.preTeleportInPrivateRoom = false;
		}

		private static IEnumerator DelayedJoinVStumpPrivateRoom()
		{
			GTDev.Log<string>("[CustomMapManager::DelayedJoinVStumpPrivateRoom] waiting for netstate to be Idle", null);
			while (NetworkSystem.Instance.netState != NetSystemState.Idle)
			{
				yield return null;
			}
			GTDev.Log<string>("[CustomMapManager::DelayedJoinVStumpPrivateRoom] joining @ version of private room: " + CustomMapManager.pendingNewPrivateRoomName, null);
			PhotonNetworkController.Instance.AttemptToJoinSpecificRoomWithCallback(CustomMapManager.pendingNewPrivateRoomName, JoinType.Solo, new Action<NetJoinResult>(CustomMapManager.OnJoinSpecificRoomResult));
			yield break;
		}

		public static void ExitVirtualStump(Action<bool> callback)
		{
			if (!CustomMapManager.hasInstance)
			{
				return;
			}
			if (CustomMapManager.lastUsedTeleporter.IsNull())
			{
				if (CustomMapManager.instance.defaultTeleporter.IsNull())
				{
					if (callback != null)
					{
						callback(false);
					}
				}
				else
				{
					CustomMapManager.lastUsedTeleporter = CustomMapManager.instance.defaultTeleporter;
				}
			}
			if (CustomMapManager.delayedJoinCoroutine != null)
			{
				CustomMapManager.instance.StopCoroutine(CustomMapManager.delayedJoinCoroutine);
				CustomMapManager.delayedJoinCoroutine = null;
			}
			if (CustomMapManager.delayedTryAutoLoadCoroutine != null)
			{
				CustomMapManager.instance.StopCoroutine(CustomMapManager.delayedTryAutoLoadCoroutine);
				CustomMapManager.delayedTryAutoLoadCoroutine = null;
			}
			CustomMapManager.instance.dayNightManager.RequestRepopulateLightmaps();
			PrivateUIRoom.ForceStartOverlay();
			GorillaTagger.Instance.overrideNotInFocus = true;
			CustomMapManager.instance.EnableTeleportHUD(false);
			CustomMapManager.currentTeleportCallback = callback;
			CustomMapManager.exitVirtualStumpPending = true;
			if (!CustomMapManager.UnloadMap(false))
			{
				CustomMapManager.FinalizeExitVirtualStump();
			}
		}

		private static void FinalizeExitVirtualStump()
		{
			if (!CustomMapManager.hasInstance)
			{
				return;
			}
			GTPlayer.Instance.SetHoverActive(false);
			VRRig.LocalRig.hoverboardVisual.SetNotHeld();
			RoomSystem.ClearOverridenRoomSize();
			CosmeticsController.instance.ClearCheckoutAndCart(false);
			foreach (GameObject gameObject in CustomMapManager.instance.rootObjectsToDeactivateAfterTeleport)
			{
				if (gameObject != null)
				{
					gameObject.gameObject.SetActive(true);
				}
			}
			if (CustomMapManager.lastUsedTeleporter.GetReturnGamemode() != GameModeType.None && CustomMapManager.lastUsedTeleporter.GetReturnGamemode() != GameModeType.Count)
			{
				GorillaComputer.instance.SetGameModeWithoutButton(CustomMapManager.lastUsedTeleporter.GetReturnGamemode().ToString());
			}
			else if (CustomMapManager.preVStumpGamemode != "")
			{
				GorillaComputer.instance.SetGameModeWithoutButton(CustomMapManager.preVStumpGamemode);
				CustomMapManager.preVStumpGamemode = "";
			}
			if (VRRig.LocalRig.IsNotNull())
			{
				GRPlayer component = VRRig.LocalRig.GetComponent<GRPlayer>();
				if (component != null && component.State == GRPlayer.GRPlayerState.Ghost)
				{
					CustomMapManager.instance.defaultReviveStation.RevivePlayer(component);
				}
			}
			ZoneManagement.SetActiveZone(CustomMapManager.lastUsedTeleporter.GetZone());
			if (VRRig.LocalRig.IsNotNull() && VRRig.LocalRig.zoneEntity.IsNotNull())
			{
				VRRig.LocalRig.zoneEntity.EnableZoneChanges();
			}
			GorillaComputer.instance.SetInVirtualStump(false);
			GTPlayer.Instance.TeleportTo(CustomMapManager.lastUsedTeleporter.GetReturnTransform(), true, false);
			CustomMapManager.instance.virtualStumpToggleableRoot.SetActive(false);
			ZoneShaderSettings.ActivateDefaultSettings();
			VRRig.LocalRig.EnableVStumpReturnWatch(false);
			GTPlayer.Instance.SetHoverAllowed(false, true);
			CustomMapManager.exitVirtualStumpPending = false;
			if (CustomMapManager.delayedEndTeleportCoroutine != null)
			{
				CustomMapManager.instance.StopCoroutine(CustomMapManager.delayedEndTeleportCoroutine);
			}
			CustomMapManager.delayedEndTeleportCoroutine = CustomMapManager.instance.StartCoroutine(CustomMapManager.DelayedEndTeleport());
			if (CustomMapManager.preTeleportInPrivateRoom)
			{
				CustomMapManager.waitingForRoomJoin = true;
				CustomMapManager.pendingNewPrivateRoomName = CustomMapManager.pendingNewPrivateRoomName.RemoveAll(GorillaComputer.instance.VStumpRoomPrepend, StringComparison.OrdinalIgnoreCase);
				PhotonNetworkController.Instance.AttemptToJoinSpecificRoomWithCallback(CustomMapManager.pendingNewPrivateRoomName, JoinType.Solo, new Action<NetJoinResult>(CustomMapManager.OnJoinSpecificRoomResult));
				return;
			}
			if (NetworkSystem.Instance.InRoom)
			{
				if (NetworkSystem.Instance.SessionIsPrivate)
				{
					CustomMapManager.waitingForRoomJoin = true;
					CustomMapManager.pendingNewPrivateRoomName = NetworkSystem.Instance.RoomName.RemoveAll(GorillaComputer.instance.VStumpRoomPrepend, StringComparison.OrdinalIgnoreCase);
					PhotonNetworkController.Instance.AttemptToJoinSpecificRoomWithCallback(CustomMapManager.pendingNewPrivateRoomName, JoinType.Solo, new Action<NetJoinResult>(CustomMapManager.OnJoinSpecificRoomResult));
					return;
				}
				if (CustomMapManager.lastUsedTeleporter.GetExitVStumpJoinTrigger() != null)
				{
					CustomMapManager.waitingForRoomJoin = true;
					GorillaComputer.instance.allowedMapsToJoin = CustomMapManager.lastUsedTeleporter.GetExitVStumpJoinTrigger().myCollider.myAllowedMapsToJoin;
					Debug.Log(string.Format("[CustomMapManager::FinalizeExit] allowedMaps: {0}", GorillaComputer.instance.allowedMapsToJoin));
					PhotonNetworkController.Instance.AttemptToJoinPublicRoom(CustomMapManager.lastUsedTeleporter.GetExitVStumpJoinTrigger(), JoinType.Solo, null);
					return;
				}
			}
			else
			{
				if (CustomMapManager.lastUsedTeleporter.GetExitVStumpJoinTrigger() != null)
				{
					GorillaComputer.instance.allowedMapsToJoin = CustomMapManager.lastUsedTeleporter.GetExitVStumpJoinTrigger().myCollider.myAllowedMapsToJoin;
					Debug.Log(string.Format("[CustomMapManager::FinalizeExit] allowedMaps: {0}", GorillaComputer.instance.allowedMapsToJoin));
					CustomMapManager.waitingForRoomJoin = true;
					PhotonNetworkController.Instance.AttemptToJoinPublicRoom(CustomMapManager.lastUsedTeleporter.GetExitVStumpJoinTrigger(), JoinType.Solo, null);
					return;
				}
				CustomMapManager.EndTeleport(true);
			}
		}

		private static void OnJoinSpecificRoomResult(NetJoinResult result)
		{
			GTDev.Log<string>("[CustomMapManager::OnJoinSpecificRoomResult] Result: " + result.ToString(), null);
			switch (result)
			{
			case NetJoinResult.Failed_Full:
				CustomMapManager.instance.OnJoinRoomFailed();
				return;
			case NetJoinResult.AlreadyInRoom:
				CustomMapManager.instance.OnJoinedRoom();
				return;
			case NetJoinResult.Failed_Other:
				GTDev.Log<string>("[CustomMapManager::OnJoinSpecificRoomResult] Joining " + CustomMapManager.pendingNewPrivateRoomName + " failed, marking for retry... ", null);
				CustomMapManager.waitingForDisconnect = true;
				CustomMapManager.shouldRetryJoin = true;
				return;
			default:
				return;
			}
		}

		private static void OnJoinSpecificRoomResultFailureAllowed(NetJoinResult result)
		{
			if (!CustomMapManager.hasInstance)
			{
				return;
			}
			GTDev.Log<string>("[CustomMapManager::OnJoinSpecificRoomResultFailureAllowed] Result: " + result.ToString(), null);
			switch (result)
			{
			case NetJoinResult.Success:
			case NetJoinResult.FallbackCreated:
				return;
			case NetJoinResult.Failed_Full:
			case NetJoinResult.Failed_Other:
				CustomMapManager.instance.OnJoinRoomFailed();
				return;
			case NetJoinResult.AlreadyInRoom:
				CustomMapManager.instance.OnJoinedRoom();
				return;
			default:
				return;
			}
		}

		public static bool AreAllPlayersInVirtualStump()
		{
			if (!CustomMapManager.hasInstance)
			{
				return false;
			}
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				if (!CustomMapManager.instance.virtualStumpPlayerDetector.playerIDsCurrentlyTouching.Contains(vrrig.creator.UserId))
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsRemotePlayerInVirtualStump(string playerID)
		{
			return CustomMapManager.hasInstance && !CustomMapManager.instance.virtualStumpPlayerDetector.IsNull() && CustomMapManager.instance.virtualStumpPlayerDetector.playerIDsCurrentlyTouching.Contains(playerID);
		}

		public static bool IsLocalPlayerInVirtualStump()
		{
			return CustomMapManager.hasInstance && !CustomMapManager.instance.virtualStumpPlayerDetector.IsNull() && !VRRig.LocalRig.IsNull() && CustomMapManager.instance.virtualStumpPlayerDetector.playerIDsCurrentlyTouching.Contains(VRRig.LocalRig.creator.UserId);
		}

		private void OnDisconnected()
		{
			if (!CustomMapManager.hasInstance)
			{
				return;
			}
			CustomMapManager.ClearRoomMap();
			if (CustomMapManager.waitingForLoginDisconnect)
			{
				CustomMapManager.waitingForLoginDisconnect = false;
				GTDev.Log<string>("[CustomMapManager::OnDisconnected] Attempting auto-login to mod.io...", null);
				CustomMapManager.AttemptAutoLogin();
				return;
			}
			if (CustomMapManager.waitingForDisconnect)
			{
				CustomMapManager.waitingForDisconnect = false;
				if (CustomMapManager.shouldRetryJoin)
				{
					CustomMapManager.shouldRetryJoin = false;
					GTDev.Log<string>("[CustomMapManager::OnDisconnected] Joining " + CustomMapManager.pendingNewPrivateRoomName + " failed previously, retrying once... ", null);
					PhotonNetworkController.Instance.AttemptToJoinSpecificRoomWithCallback(CustomMapManager.pendingNewPrivateRoomName, JoinType.Solo, new Action<NetJoinResult>(CustomMapManager.OnJoinSpecificRoomResultFailureAllowed));
					return;
				}
				GTDev.Log<string>("[CustomMapManager::OnDisconnected] Ending teleport...", null);
				CustomMapManager.EndTeleport(true);
			}
		}

		private static async Task AttemptAutoLogin()
		{
			GTDev.Log<string>(string.Format("[CustomMapManager::AttemptAutoLogin] delayed end teleport coroutine == null : {0}", CustomMapManager.delayedJoinCoroutine == null), null);
			if (CustomMapManager.delayedEndTeleportCoroutine != null)
			{
				CustomMapManager.instance.StopCoroutine(CustomMapManager.delayedEndTeleportCoroutine);
			}
			CustomMapManager.delayedEndTeleportCoroutine = CustomMapManager.instance.StartCoroutine(CustomMapManager.DelayedEndTeleport());
			Error error = await ModIOManager.Initialize();
			if (error)
			{
				CustomMapManager.OnAutoLoginComplete(error);
			}
			else
			{
				ModIOManager.IsAuthenticated(true);
				CustomMapManager.OnAutoLoginComplete(Error.None);
			}
		}

		private void OnJoinRoomFailed()
		{
			if (!CustomMapManager.hasInstance)
			{
				return;
			}
			if (CustomMapManager.waitingForRoomJoin)
			{
				GTDev.Log<string>("[CustomMapManager::OnJoinRoomFailed] Currently waiting for room join, resetting state, ending teleport...", null);
				CustomMapManager.waitingForRoomJoin = false;
				CustomMapManager.EndTeleport(false);
			}
		}

		private static void EndTeleport(bool teleportSuccessful)
		{
			if (CustomMapManager.hasInstance)
			{
				if (CustomMapManager.delayedEndTeleportCoroutine != null)
				{
					CustomMapManager.instance.StopCoroutine(CustomMapManager.delayedEndTeleportCoroutine);
					CustomMapManager.delayedEndTeleportCoroutine = null;
				}
				if (CustomMapManager.delayedJoinCoroutine != null)
				{
					CustomMapManager.instance.StopCoroutine(CustomMapManager.delayedJoinCoroutine);
					CustomMapManager.delayedJoinCoroutine = null;
				}
			}
			CustomMapManager.DisableTeleportHUD();
			GorillaTagger.Instance.overrideNotInFocus = false;
			PrivateUIRoom.StopForcedOverlay();
			Action<bool> action = CustomMapManager.currentTeleportCallback;
			if (action != null)
			{
				action(teleportSuccessful);
			}
			CustomMapManager.currentTeleportCallback = null;
			if (CustomMapManager.hasInstance && !GorillaComputer.instance.IsPlayerInVirtualStump())
			{
				GTDev.Log<string>("[CustomMapManager::EndTeleport] Player is not in VStump, disabling VStump_Lobby GameObject", null);
				CustomMapManager.instance.gameObject.SetActive(false);
			}
			if (teleportSuccessful && GorillaComputer.instance.IsPlayerInVirtualStump() && CustomMapManager.lastUsedTeleporter.GetAutoLoadMapModId() != ModId.Null)
			{
				bool flag = false;
				if (CustomMapManager.waitingForRoomJoin)
				{
					GTDev.Log<string>("[CustomMapManager::EndTeleport] Still waiting for room join, delaying auto-load...", null);
					flag = true;
				}
				else if (NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.IsMasterClient && VirtualStumpSerializer.IsWaitingForRoomInit())
				{
					GTDev.Log<string>("[CustomMapManager::EndTeleport] Still waiting for room init, delaying auto-load...", null);
					flag = true;
				}
				if (flag)
				{
					CustomMapManager.delayedTryAutoLoadCoroutine = CustomMapManager.instance.StartCoroutine(CustomMapManager.DelayedTryAutoLoad());
					return;
				}
				GTDev.Log<string>("[CustomMapManager::EndTeleport] Attempting auto-load...", null);
				if (!NetworkSystem.Instance.InRoom || (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.IsMasterClient))
				{
					CustomMapManager.SetRoomMap(CustomMapManager.lastUsedTeleporter.GetAutoLoadMapModId());
					CustomMapManager.LoadMap(CustomMapManager.lastUsedTeleporter.GetAutoLoadMapModId());
					return;
				}
				if (CustomMapManager.GetRoomMapId() == CustomMapManager.lastUsedTeleporter.GetAutoLoadMapModId())
				{
					CustomMapManager.LoadMap(CustomMapManager.lastUsedTeleporter.GetAutoLoadMapModId());
				}
			}
		}

		private static IEnumerator DelayedEndTeleport()
		{
			yield return new WaitForSecondsRealtime(CustomMapManager.instance.maxPostTeleportRoomProcessingTime);
			GTDev.Log<string>("[CustomMapManager::DelayedEndTeleport] Timer expired, force ending teleport...", null);
			CustomMapManager.EndTeleport(false);
			yield break;
		}

		private static IEnumerator DelayedTryAutoLoad()
		{
			while (CustomMapManager.waitingForRoomJoin || VirtualStumpSerializer.IsWaitingForRoomInit())
			{
				yield return new WaitForSeconds(0.1f);
			}
			GTDev.Log<string>("[CustomMapManager::DelayedTryAutoLoad] Room Init finished, attempting auto-load...", null);
			if (!NetworkSystem.Instance.InRoom || (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.IsMasterClient))
			{
				CustomMapManager.SetRoomMap(CustomMapManager.lastUsedTeleporter.GetAutoLoadMapModId());
				CustomMapManager.LoadMap(CustomMapManager.lastUsedTeleporter.GetAutoLoadMapModId());
			}
			else if (CustomMapManager.GetRoomMapId() == CustomMapManager.lastUsedTeleporter.GetAutoLoadMapModId())
			{
				CustomMapManager.LoadMap(CustomMapManager.lastUsedTeleporter.GetAutoLoadMapModId());
			}
			yield break;
		}

		private void OnJoinedRoom()
		{
			if (!CustomMapManager.hasInstance)
			{
				return;
			}
			if (CustomMapManager.waitingForRoomJoin)
			{
				CustomMapManager.waitingForRoomJoin = false;
				GTDev.Log<string>("[CustomMapManager::OnJoinedRoom] Ending teleport...", null);
				CustomMapManager.EndTeleport(true);
				if (CustomMapManager.lastUsedTeleporter.IsNotNull())
				{
					CustomMapManager.lastUsedTeleporter.PlayTeleportEffects(true, false, null, true);
				}
			}
		}

		public static bool UnloadMap(bool returnToSinglePlayerIfInPublic = true)
		{
			if (CustomMapManager.unloadInProgress)
			{
				return false;
			}
			if (!CustomMapLoader.IsMapLoaded() && !CustomMapLoader.IsLoading())
			{
				if (CustomMapManager.loadInProgress)
				{
					GTDev.Log<string>("[CustomMapManager::UnloadMap] Map load is currently in progress... aborting...", null);
					CustomMapManager.abortModLoadIds.AddIfNew(CustomMapManager.loadingMapId);
					bool flag = CustomMapManager.waitingForModDownload;
					CustomMapManager.loadInProgress = false;
					CustomMapManager.loadingMapId = ModId.Null;
					CustomMapManager.waitingForModDownload = false;
					CustomMapManager.waitingForModInstall = false;
					CustomMapManager.waitingForModInstallId = ModId.Null;
					CustomMapManager.ClearRoomMap();
				}
				else
				{
					CustomMapManager.ClearRoomMap();
				}
				return false;
			}
			CustomMapManager.unloadInProgress = true;
			CustomMapManager.unloadingMapId = new ModId(CustomMapLoader.IsMapLoaded() ? CustomMapLoader.LoadedMapModId : CustomMapLoader.GetLoadingMapModId());
			CustomMapManager.OnMapLoadProgress(MapLoadStatus.Unloading, 0, "");
			CustomMapManager.loadInProgress = false;
			CustomMapManager.loadingMapId = ModId.Null;
			CustomMapManager.waitingForModDownload = false;
			CustomMapManager.waitingForModInstall = false;
			CustomMapManager.waitingForModInstallId = ModId.Null;
			CustomMapManager.ClearRoomMap();
			CustomGameMode.LuaScript = "";
			if (CustomGameMode.gameScriptRunner != null)
			{
				CustomGameMode.StopScript();
			}
			CustomMapManager.customMapDefaultZoneShaderSettingsInitialized = false;
			CustomMapManager.customMapDefaultZoneShaderProperties = default(CMSZoneShaderSettings.CMSZoneShaderProperties);
			CustomMapManager.loadedCustomMapDefaultZoneShaderSettings = null;
			if (CustomMapManager.hasInstance)
			{
				CustomMapManager.instance.customMapDefaultZoneShaderSettings.CopySettings(CustomMapManager.instance.virtualStumpZoneShaderSettings, false);
				CustomMapManager.instance.virtualStumpZoneShaderSettings.BecomeActiveInstance(false);
				CustomMapManager.allCustomMapZoneShaderSettings.Clear();
			}
			CustomMapLoader.CloseDoorAndUnloadMap(new Action(CustomMapManager.OnMapUnloadCompleted));
			if (returnToSinglePlayerIfInPublic && NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.SessionIsPrivate)
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
			}
			return true;
		}

		private static void OnMapUnloadCompleted()
		{
			CustomMapManager.unloadInProgress = false;
			CustomMapManager.OnMapUnloadComplete.Invoke();
			CustomMapManager.currentRoomMapModId = ModId.Null;
			CustomMapManager.currentRoomMapApproved = false;
			CustomMapManager.OnRoomMapChanged.Invoke(ModId.Null);
			if (CustomMapManager.exitVirtualStumpPending)
			{
				CustomMapManager.FinalizeExitVirtualStump();
			}
		}

		public static async Task LoadMap(ModId modId)
		{
			if (CustomMapManager.hasInstance && !CustomMapManager.loadInProgress)
			{
				if (CustomMapManager.abortModLoadIds.Contains(modId))
				{
					CustomMapManager.abortModLoadIds.Remove(modId);
				}
				if (!CustomMapLoader.IsMapLoaded(modId))
				{
					CustomMapManager.loadInProgress = true;
					CustomMapManager.loadingMapId = modId;
					CustomMapManager.waitingForModDownload = false;
					CustomMapManager.waitingForModInstall = false;
					CustomMapManager.waitingForModInstallId = ModId.Null;
					Error error = Error.None;
					ValueTuple<Error, Mod> valueTuple = await ModIOManager.GetMod(modId, false, null);
					error = valueTuple.Item1;
					Mod item = valueTuple.Item2;
					if (error)
					{
						Debug.LogError("[CustomMapManager::LoadMap] Failed to get details for Mod with modID " + modId.ToString() + ", error: " + error.GetMessage());
						CustomMapManager.HandleMapLoadFailed("FAILED TO GET MAP DETAILS: " + error.GetMessage());
					}
					else if (item.Creator == null)
					{
						CustomMapManager.loadInProgress = false;
						CustomMapManager.loadingMapId = ModId.Null;
					}
					else if (CustomMapManager.abortModLoadIds.Contains(modId))
					{
						GTDev.Log<string>("[CustomMapManager::LoadMap] Aborting load...", null);
						CustomMapManager.abortModLoadIds.Remove(modId);
					}
					else if (item.File != null)
					{
						switch (item.File.State)
						{
						case ModFileState.None:
						case ModFileState.Queued:
						{
							GTDev.Log<string>(string.Format("[CustomMapManager::LoadMap] Downloading mod {0}...", modId), null);
							CustomMapManager.waitingForModDownload = true;
							CustomMapManager.waitingForModInstall = true;
							CustomMapManager.waitingForModInstallId = item.Id;
							bool flag = await ModIOManager.DownloadMod(modId, null);
							if (CustomMapManager.abortModLoadIds.Contains(modId))
							{
								GTDev.Log<string>("[CustomMapManager::LoadMap] Aborting load...", null);
								CustomMapManager.abortModLoadIds.Remove(modId);
							}
							else if (!flag)
							{
								CustomMapManager.HandleMapLoadFailed("FAILED TO START MAP DOWNLOAD");
							}
							break;
						}
						case ModFileState.Downloading:
						case ModFileState.Updating:
							CustomMapManager.waitingForModDownload = true;
							CustomMapManager.waitingForModInstallId = modId;
							break;
						case ModFileState.Downloaded:
						case ModFileState.Installing:
							CustomMapManager.waitingForModInstall = true;
							CustomMapManager.waitingForModInstallId = modId;
							break;
						case ModFileState.Installed:
							CustomMapManager.instance.LoadInstalledMap(item);
							break;
						case ModFileState.Uninstalling:
						case ModFileState.FileOperationFailed:
							Debug.LogError("[CustomMapManager::LoadMap] Failed to load map with modID " + modId.ToString() + ", error: " + item.File.State.ToString());
							CustomMapManager.HandleMapLoadFailed("FAILED TO LOAD MAP: " + item.File.State.ToString());
							break;
						}
					}
				}
			}
		}

		private async Task LoadInstalledMap(Mod installedMod)
		{
			CustomMapManager.waitingForModInstall = false;
			CustomMapManager.waitingForModInstallId = ModId.Null;
			if (installedMod.File.State != ModFileState.Installed)
			{
				Debug.LogError("[CustomMapManager::LoadInstalledMap] Requested map is not installed!");
				CustomMapManager.HandleMapLoadFailed("MAP IS NOT INSTALLED");
			}
			else
			{
				if (ModIOManager.ValidateInstalledMod(installedMod))
				{
					try
					{
						FileInfo[] files = new DirectoryInfo(installedMod.File.InstallLocation).GetFiles("package.json");
						if (files.Length == 0)
						{
							Debug.LogError(string.Concat(new string[]
							{
								"[CustomMapManager::LoadInstalledMap] Directory (",
								installedMod.File.InstallLocation,
								") for mod ",
								installedMod.Name,
								" does not contain a package.json file!"
							}));
							CustomMapManager.HandleMapLoadFailed("COULD NOT FIND PACKAGE.JSON IN MAP FILES");
							return;
						}
						GTDev.Log<string>("[CustomMapManager::LoadInstalledMap] Loading map file: " + files[0].FullName, null);
						CustomMapLoader.LoadMap(installedMod.Id, files[0].FullName);
						goto IL_0202;
					}
					catch (Exception ex)
					{
						Debug.LogError(string.Format("[CustomMapManager::LoadInstalledMap] Failed to load installed map: {0}", ex));
						CustomMapManager.HandleMapLoadFailed(string.Format("FAILED TO LOAD: {0}", ex));
						goto IL_0202;
					}
				}
				CustomMapManager.waitingForModDownload = true;
				CustomMapManager.waitingForModInstall = true;
				CustomMapManager.waitingForModInstallId = installedMod.Id;
				bool flag = await ModIOManager.DownloadMod(installedMod.Id, null);
				if (CustomMapManager.abortModLoadIds.Contains(installedMod.Id))
				{
					GTDev.Log<string>("[CustomMapManager::LoadInstalledMap] Aborting load...", null);
					CustomMapManager.abortModLoadIds.Remove(installedMod.Id);
				}
				else if (!flag)
				{
					CustomMapManager.HandleMapLoadFailed("FAILED TO START MAP DOWNLOAD");
				}
				IL_0202:;
			}
		}

		private static void OnMapLoadProgress(MapLoadStatus loadStatus, int progress, string message)
		{
			CustomMapManager.OnMapLoadStatusChanged.Invoke(loadStatus, progress, message);
		}

		private static void OnMapLoadFinished(bool success)
		{
			CustomMapManager.loadInProgress = false;
			CustomMapManager.loadingMapId = ModId.Null;
			CustomMapManager.waitingForModDownload = false;
			CustomMapManager.waitingForModInstall = false;
			CustomMapManager.waitingForModInstallId = ModId.Null;
			if (success)
			{
				CustomMapLoader.OpenDoorToMap();
				if (!CustomMapLoader.GetLuauGamemodeScript().IsNullOrEmpty())
				{
					CustomGameMode.LuaScript = CustomMapLoader.GetLuauGamemodeScript();
					if (CustomGameMode.LuaScript != "" && CustomGameMode.GameModeInitialized && CustomGameMode.gameScriptRunner == null)
					{
						CustomGameMode.LuaStart();
					}
				}
			}
			CustomMapManager.OnMapLoadComplete.Invoke(success);
		}

		private static void HandleMapLoadFailed(string message = null)
		{
			CustomMapManager.loadInProgress = false;
			CustomMapManager.loadingMapId = ModId.Null;
			CustomMapManager.waitingForModInstall = false;
			CustomMapManager.waitingForModInstallId = ModId.Null;
			CustomMapManager.OnMapLoadStatusChanged.Invoke(MapLoadStatus.Error, 0, message ?? "UNKNOWN ERROR");
			CustomMapManager.OnMapLoadComplete.Invoke(false);
		}

		public static bool IsUnloading()
		{
			return CustomMapManager.unloadInProgress;
		}

		public static bool IsLoading()
		{
			return CustomMapManager.IsLoading(ModId.Null);
		}

		public static bool IsLoading(ModId modId)
		{
			if (!modId.IsValid())
			{
				return CustomMapManager.loadInProgress || CustomMapLoader.IsLoading();
			}
			return CustomMapManager.loadInProgress && CustomMapManager.loadingMapId == modId;
		}

		public static ModId GetRoomMapId()
		{
			if (NetworkSystem.Instance.InRoom)
			{
				if (CustomMapManager.currentRoomMapModId == ModId.Null && NetworkSystem.Instance.IsMasterClient && CustomMapLoader.IsMapLoaded())
				{
					CustomMapManager.currentRoomMapModId = new ModId(CustomMapLoader.LoadedMapModId);
				}
				return CustomMapManager.currentRoomMapModId;
			}
			if (CustomMapManager.IsLoading())
			{
				return CustomMapManager.loadingMapId;
			}
			if (CustomMapLoader.IsMapLoaded())
			{
				return new ModId(CustomMapLoader.LoadedMapModId);
			}
			return ModId.Null;
		}

		public static void SetRoomMap(long modId)
		{
			if (!CustomMapManager.hasInstance || modId == CustomMapManager.currentRoomMapModId._id)
			{
				return;
			}
			CustomMapManager.currentRoomMapModId = new ModId(modId);
			CustomMapManager.currentRoomMapApproved = false;
			CustomMapManager.OnRoomMapChanged.Invoke(CustomMapManager.currentRoomMapModId);
		}

		public static void ClearRoomMap()
		{
			if (!CustomMapManager.hasInstance || CustomMapManager.currentRoomMapModId.Equals(ModId.Null))
			{
				return;
			}
			CustomMapManager.currentRoomMapModId = ModId.Null;
			CustomMapManager.currentRoomMapApproved = false;
			CustomMapManager.OnRoomMapChanged.Invoke(ModId.Null);
		}

		public static bool CanLoadRoomMap()
		{
			return CustomMapManager.currentRoomMapModId != ModId.Null;
		}

		public static void ApproveAndLoadRoomMap()
		{
			CustomMapManager.currentRoomMapApproved = true;
			CMSSerializer.ResetSyncedMapObjects();
			CustomMapManager.LoadMap(CustomMapManager.currentRoomMapModId);
		}

		public static void RequestEnableTeleportHUD(bool enteringVirtualStump)
		{
			if (CustomMapManager.hasInstance)
			{
				CustomMapManager.instance.EnableTeleportHUD(enteringVirtualStump);
			}
		}

		private void EnableTeleportHUD(bool enteringVirtualStump)
		{
			if (CustomMapManager.teleportingHUD != null)
			{
				CustomMapManager.teleportingHUD.gameObject.SetActive(true);
				CustomMapManager.teleportingHUD.Initialize(enteringVirtualStump);
				return;
			}
			if (this.teleportingHUDPrefab != null)
			{
				Camera main = Camera.main;
				if (main != null)
				{
					GameObject gameObject = Object.Instantiate<GameObject>(this.teleportingHUDPrefab, main.transform);
					if (gameObject != null)
					{
						CustomMapManager.teleportingHUD = gameObject.GetComponent<VirtualStumpTeleportingHUD>();
						if (CustomMapManager.teleportingHUD != null)
						{
							CustomMapManager.teleportingHUD.Initialize(enteringVirtualStump);
						}
					}
				}
			}
		}

		public static void DisableTeleportHUD()
		{
			if (CustomMapManager.teleportingHUD != null)
			{
				CustomMapManager.teleportingHUD.gameObject.SetActive(false);
			}
		}

		public static void LoadZoneTriggered(int[] scenesToLoad, int[] scenesToUnload)
		{
			CustomMapLoader.LoadZoneTriggered(scenesToLoad, scenesToUnload, new Action<string>(CustomMapManager.OnSceneLoaded), new Action<string>(CustomMapManager.OnSceneUnloaded));
		}

		private static void OnSceneLoaded(string sceneName)
		{
			CMSSerializer.ProcessSceneLoad(sceneName);
			CustomMapManager.ProcessZoneShaderSettings(sceneName);
		}

		private static void OnSceneUnloaded(string sceneName)
		{
			CMSSerializer.UnregisterTriggers(sceneName);
			for (int i = CustomMapManager.allCustomMapZoneShaderSettings.Count - 1; i >= 0; i--)
			{
				if (CustomMapManager.allCustomMapZoneShaderSettings[i].IsNull())
				{
					CustomMapManager.allCustomMapZoneShaderSettings.RemoveAt(i);
				}
			}
		}

		private static void OnSceneTriggerHistoryProcessed(string sceneName)
		{
			CapsuleCollider bodyCollider = GTPlayer.Instance.bodyCollider;
			SphereCollider headCollider = GTPlayer.Instance.headCollider;
			Vector3 vector = bodyCollider.transform.TransformPoint(bodyCollider.center);
			float num = Mathf.Max(bodyCollider.height, bodyCollider.radius) * GTPlayer.Instance.scale;
			Collider[] array = new Collider[100];
			Physics.OverlapSphereNonAlloc(vector, num, array);
			foreach (Collider collider in array)
			{
				if (collider != null && collider.gameObject.scene.name.Equals(sceneName))
				{
					CMSTrigger[] components = collider.gameObject.GetComponents<CMSTrigger>();
					for (int j = 0; j < components.Length; j++)
					{
						if (components[j] != null)
						{
							components[j].OnTriggerEnter(bodyCollider);
							components[j].OnTriggerEnter(headCollider);
						}
					}
					CMSLoadingZone[] components2 = collider.gameObject.GetComponents<CMSLoadingZone>();
					for (int k = 0; k < components2.Length; k++)
					{
						if (components2[k] != null)
						{
							components2[k].OnTriggerEnter(bodyCollider);
						}
					}
					CMSZoneShaderSettingsTrigger[] components3 = collider.gameObject.GetComponents<CMSZoneShaderSettingsTrigger>();
					for (int l = 0; l < components3.Length; l++)
					{
						if (components3[l] != null)
						{
							components3[l].OnTriggerEnter(bodyCollider);
						}
					}
					HoverboardAreaTrigger[] components4 = collider.gameObject.GetComponents<HoverboardAreaTrigger>();
					for (int m = 0; m < components4.Length; m++)
					{
						if (components4[m] != null)
						{
							components4[m].OnTriggerEnter(headCollider);
						}
					}
					WaterVolume[] components5 = collider.gameObject.GetComponents<WaterVolume>();
					for (int n = 0; n < components5.Length; n++)
					{
						if (components5[n] != null)
						{
							components5[n].OnTriggerEnter(bodyCollider);
							components5[n].OnTriggerEnter(headCollider);
						}
					}
				}
			}
		}

		public static void SetDefaultZoneShaderSettings(ZoneShaderSettings defaultCustomMapShaderSettings, CMSZoneShaderSettings.CMSZoneShaderProperties defaultZoneShaderProperties)
		{
			if (CustomMapManager.hasInstance)
			{
				CustomMapManager.instance.customMapDefaultZoneShaderSettings.CopySettings(defaultCustomMapShaderSettings, true);
				CustomMapManager.loadedCustomMapDefaultZoneShaderSettings = defaultCustomMapShaderSettings;
				CustomMapManager.customMapDefaultZoneShaderProperties = defaultZoneShaderProperties;
				CustomMapManager.customMapDefaultZoneShaderSettingsInitialized = true;
			}
		}

		private static void ProcessZoneShaderSettings(string loadedSceneName)
		{
			if (CustomMapManager.hasInstance && CustomMapManager.customMapDefaultZoneShaderSettingsInitialized && CustomMapManager.customMapDefaultZoneShaderProperties.isInitialized)
			{
				for (int i = 0; i < CustomMapManager.allCustomMapZoneShaderSettings.Count; i++)
				{
					if (CustomMapManager.allCustomMapZoneShaderSettings[i].IsNotNull() && CustomMapManager.allCustomMapZoneShaderSettings[i] != CustomMapManager.loadedCustomMapDefaultZoneShaderSettings && CustomMapManager.allCustomMapZoneShaderSettings[i].gameObject.scene.name.Equals(loadedSceneName))
					{
						CustomMapManager.allCustomMapZoneShaderSettings[i].ReplaceDefaultValues(CustomMapManager.customMapDefaultZoneShaderProperties, true);
					}
				}
				return;
			}
			if (CustomMapManager.hasInstance && CustomMapManager.instance.virtualStumpZoneShaderSettings.IsNotNull())
			{
				for (int j = 0; j < CustomMapManager.allCustomMapZoneShaderSettings.Count; j++)
				{
					if (CustomMapManager.allCustomMapZoneShaderSettings[j].IsNotNull() && CustomMapManager.allCustomMapZoneShaderSettings[j].gameObject.scene.name.Equals(loadedSceneName))
					{
						CustomMapManager.allCustomMapZoneShaderSettings[j].ReplaceDefaultValues(CustomMapManager.instance.virtualStumpZoneShaderSettings, true);
					}
				}
			}
		}

		public static void AddZoneShaderSettings(ZoneShaderSettings zoneShaderSettings)
		{
			CustomMapManager.allCustomMapZoneShaderSettings.AddIfNew(zoneShaderSettings);
		}

		public static void ActivateDefaultZoneShaderSettings()
		{
			if (CustomMapManager.hasInstance && CustomMapManager.customMapDefaultZoneShaderSettingsInitialized)
			{
				CustomMapManager.instance.customMapDefaultZoneShaderSettings.BecomeActiveInstance(true);
				return;
			}
			if (CustomMapManager.hasInstance)
			{
				CustomMapManager.instance.virtualStumpZoneShaderSettings.BecomeActiveInstance(true);
			}
		}

		public static void ReturnToVirtualStump()
		{
			if (!CustomMapManager.hasInstance)
			{
				return;
			}
			if (!GorillaComputer.instance.IsPlayerInVirtualStump())
			{
				return;
			}
			if (CustomMapManager.instance.returnToVirtualStumpTeleportLocation.IsNotNull())
			{
				GTPlayer gtplayer = GTPlayer.Instance;
				if (gtplayer != null)
				{
					CustomMapLoader.ResetToInitialZone(new Action<string>(CustomMapManager.OnSceneLoaded), new Action<string>(CustomMapManager.OnSceneUnloaded));
					gtplayer.TeleportTo(CustomMapManager.instance.returnToVirtualStumpTeleportLocation, true, false);
				}
			}
		}

		public static bool WantsHoldingHandsDisabled()
		{
			if (GorillaComputer.instance.IsPlayerInVirtualStump())
			{
				if (!CustomMapLoader.IsMapLoaded())
				{
					return true;
				}
				if (CustomMapLoader.LoadedMapWantsHoldingHandsDisabled())
				{
					return true;
				}
			}
			return false;
		}

		[OnEnterPlay_SetNull]
		private static volatile CustomMapManager instance;

		[OnEnterPlay_Set(false)]
		private static bool hasInstance = false;

		[SerializeField]
		private GameObject virtualStumpToggleableRoot;

		[SerializeField]
		private Transform returnToVirtualStumpTeleportLocation;

		[SerializeField]
		private List<Transform> virtualStumpTeleportLocations;

		[SerializeField]
		private GameObject[] rootObjectsToDeactivateAfterTeleport;

		[SerializeField]
		private GorillaFriendCollider virtualStumpPlayerDetector;

		[SerializeField]
		private ZoneShaderSettings virtualStumpZoneShaderSettings;

		[SerializeField]
		private BetterDayNightManager dayNightManager;

		[SerializeField]
		private GhostReactorManager ghostReactorManager;

		[SerializeField]
		private GRReviveStation defaultReviveStation;

		[SerializeField]
		private ZoneShaderSettings customMapDefaultZoneShaderSettings;

		[SerializeField]
		private GameObject teleportingHUDPrefab;

		[SerializeField]
		private AudioSource localTeleportSFXSource;

		[SerializeField]
		private VirtualStumpTeleporter defaultTeleporter;

		[SerializeField]
		private float maxPostTeleportRoomProcessingTime = 15f;

		private static VirtualStumpTeleporter lastUsedTeleporter;

		private static string preVStumpGamemode = "";

		private static bool customMapDefaultZoneShaderSettingsInitialized;

		private static ZoneShaderSettings loadedCustomMapDefaultZoneShaderSettings;

		private static CMSZoneShaderSettings.CMSZoneShaderProperties customMapDefaultZoneShaderProperties;

		private static readonly List<ZoneShaderSettings> allCustomMapZoneShaderSettings = new List<ZoneShaderSettings>();

		private static bool loadInProgress = false;

		private static ModId loadingMapId = ModId.Null;

		private static bool unloadInProgress = false;

		private static ModId unloadingMapId = ModId.Null;

		private static List<ModId> abortModLoadIds = new List<ModId>();

		private static bool waitingForModDownload = false;

		private static bool waitingForModInstall = false;

		private static ModId waitingForModInstallId = ModId.Null;

		private static bool preTeleportInPrivateRoom = false;

		private static string pendingNewPrivateRoomName = "";

		private static Action<bool> currentTeleportCallback;

		private static bool waitingForLoginDisconnect = false;

		private static bool waitingForDisconnect = false;

		private static bool waitingForRoomJoin = false;

		private static bool shouldRetryJoin = false;

		private static short pendingTeleportVFXIdx = -1;

		private static bool exitVirtualStumpPending = false;

		private static ModId currentRoomMapModId = ModId.Null;

		private static bool currentRoomMapApproved = false;

		private static VirtualStumpTeleportingHUD teleportingHUD;

		private static Coroutine delayedEndTeleportCoroutine;

		private static Coroutine delayedJoinCoroutine;

		private static Coroutine delayedTryAutoLoadCoroutine;

		public static UnityEvent<ModId> OnRoomMapChanged = new UnityEvent<ModId>();

		public static UnityEvent<MapLoadStatus, int, string> OnMapLoadStatusChanged = new UnityEvent<MapLoadStatus, int, string>();

		public static UnityEvent<bool> OnMapLoadComplete = new UnityEvent<bool>();

		public static UnityEvent OnMapUnloadComplete = new UnityEvent();
	}
}
