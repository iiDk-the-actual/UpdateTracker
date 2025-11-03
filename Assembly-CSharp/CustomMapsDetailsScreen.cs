using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GorillaTagScripts.VirtualStumpCustomMaps;
using GorillaTagScripts.VirtualStumpCustomMaps.UI;
using Modio;
using Modio.Mods;
using Modio.Users;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CustomMapsDetailsScreen : CustomMapsTerminalScreen
{
	public Mod currentMapMod { get; private set; }

	public override void Initialize()
	{
	}

	public override void Show()
	{
		base.Show();
		ModIOManager.OnModIOLoggedIn.RemoveListener(new UnityAction(this.OnModIOLoggedIn));
		ModIOManager.OnModIOLoggedIn.AddListener(new UnityAction(this.OnModIOLoggedIn));
		ModIOManager.OnModIOLoggedOut.RemoveListener(new UnityAction(this.OnModIOLoggedOut));
		ModIOManager.OnModIOLoggedOut.AddListener(new UnityAction(this.OnModIOLoggedOut));
		ModIOManager.OnModIOUserChanged.RemoveListener(new UnityAction<User>(this.OnModIOUserChanged));
		ModIOManager.OnModIOUserChanged.AddListener(new UnityAction<User>(this.OnModIOUserChanged));
		ModIOManager.OnModManagementEvent.RemoveListener(new UnityAction<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase>(this.HandleModManagementEvent));
		ModIOManager.OnModManagementEvent.AddListener(new UnityAction<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase>(this.HandleModManagementEvent));
		CustomMapManager.OnMapLoadStatusChanged.RemoveListener(new UnityAction<MapLoadStatus, int, string>(this.OnMapLoadProgress));
		CustomMapManager.OnMapLoadStatusChanged.AddListener(new UnityAction<MapLoadStatus, int, string>(this.OnMapLoadProgress));
		CustomMapManager.OnMapLoadComplete.RemoveListener(new UnityAction<bool>(this.OnMapLoadComplete));
		CustomMapManager.OnMapLoadComplete.AddListener(new UnityAction<bool>(this.OnMapLoadComplete));
		CustomMapManager.OnRoomMapChanged.RemoveListener(new UnityAction<ModId>(this.OnRoomMapChanged));
		CustomMapManager.OnRoomMapChanged.AddListener(new UnityAction<ModId>(this.OnRoomMapChanged));
		CustomMapManager.OnMapUnloadComplete.RemoveListener(new UnityAction(this.OnMapUnloaded));
		CustomMapManager.OnMapUnloadComplete.AddListener(new UnityAction(this.OnMapUnloaded));
		if (!ModIOManager.IsLoggedIn())
		{
			this.subscriptionToggleButton.gameObject.SetActive(false);
		}
		this.deleteButton.gameObject.SetActive(false);
		this.ResetToDefaultView();
	}

	public override void Hide()
	{
		base.Hide();
		ModIOManager.OnModIOLoggedIn.RemoveListener(new UnityAction(this.OnModIOLoggedIn));
		ModIOManager.OnModIOLoggedOut.RemoveListener(new UnityAction(this.OnModIOLoggedOut));
		ModIOManager.OnModIOUserChanged.RemoveListener(new UnityAction<User>(this.OnModIOUserChanged));
		ModIOManager.OnModManagementEvent.RemoveListener(new UnityAction<Mod, Modfile, ModInstallationManagement.OperationType, ModInstallationManagement.OperationPhase>(this.HandleModManagementEvent));
		CustomMapManager.OnMapLoadStatusChanged.RemoveListener(new UnityAction<MapLoadStatus, int, string>(this.OnMapLoadProgress));
		CustomMapManager.OnMapLoadComplete.RemoveListener(new UnityAction<bool>(this.OnMapLoadComplete));
		CustomMapManager.OnRoomMapChanged.RemoveListener(new UnityAction<ModId>(this.OnRoomMapChanged));
		CustomMapManager.OnMapUnloadComplete.RemoveListener(new UnityAction(this.OnMapUnloaded));
	}

	private void OnModUpdated()
	{
		ModRating currentUserRating = this.currentMapMod.CurrentUserRating;
		this.rateUpButton.SetButtonActive(currentUserRating == ModRating.Positive);
		this.rateDownButton.SetButtonActive(currentUserRating == ModRating.Negative);
	}

	private void OnModIOLoggedIn()
	{
		if (this.currentMapMod.Creator == null)
		{
			this.RefreshCurrentMapMod();
			return;
		}
		if (this.currentMapMod.IsHidden())
		{
			this.UpdateMapDetails(true);
			return;
		}
		this.UpdateStatus(false);
	}

	private void OnModIOLoggedOut()
	{
		if (this.currentMapMod.IsHidden())
		{
			this.UpdateMapDetails(true);
			return;
		}
		this.UpdateStatus(false);
	}

	private void OnModIOUserChanged(User user)
	{
		this.UpdateStatus(false);
	}

	private void HandleModManagementEvent(Mod mod, Modfile modfile, ModInstallationManagement.OperationType jobType, ModInstallationManagement.OperationPhase jobPhase)
	{
		if (base.isActiveAndEnabled && this.hasModProfile && this.GetModId() == mod.Id)
		{
			this.UpdateStatus(jobPhase == ModInstallationManagement.OperationPhase.Cancelled || jobPhase == ModInstallationManagement.OperationPhase.Failed);
			if (jobPhase == ModInstallationManagement.OperationPhase.Failed)
			{
				this.modDescriptionText.gameObject.SetActive(false);
				this.loadingMapLabelText.text = this.mapLoadingErrorString;
				this.loadingMapLabelText.gameObject.SetActive(true);
				this.loadingMapMessageText.text = this.mapLoadingErrorInvalidModFile;
				this.loadingMapMessageText.gameObject.SetActive(true);
			}
		}
	}

	private void Update()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		string text;
		if (this.GetModId().IsValid() && ModInstallationManagement.CurrentOperationOnMod != null && ModInstallationManagement.CurrentOperationOnMod.Id == this.GetModId() && ModInstallationManagement.CurrentOperationOnMod.File.State != ModFileState.Installed && CustomMapsDetailsScreen.modStatusStrings.TryGetValue(ModInstallationManagement.CurrentOperationOnMod.File.State, out text))
		{
			float num = this.currentMapMod.File.FileStateProgress * 100f;
			this.modStatusText.text = text + string.Format(" {0}%", Mathf.RoundToInt(num));
		}
	}

	public void RetrieveModFromModIO(long id, bool forceUpdate = false, Action<Error, Mod> callback = null)
	{
		if (this.hasModProfile && this.GetModId()._id == id)
		{
			this.UpdateMapDetails(true);
			return;
		}
		this.pendingModId = id;
		ModIOManager.GetMod(new ModId(id), forceUpdate, (callback != null) ? callback : new Action<Error, Mod>(this.OnProfileReceived));
	}

	public void SetModProfile(Mod mod)
	{
		if (mod.Id != ModId.Null)
		{
			this.pendingModId = 0L;
			this.currentMapMod = mod;
			this.hasModProfile = true;
			this.currentMapMod.OnModUpdated += this.OnModUpdated;
			this.isFavorite = ModIOManager.IsModFavorited(mod.Id);
			this.favoriteToggleButton.SetButtonActive(this.isFavorite);
			this.UpdateMapDetails(true);
		}
	}

	public override void PressButton(CustomMapKeyboardBinding buttonPressed)
	{
		if (Time.time < this.showTime + this.activationTime)
		{
			return;
		}
		GTDev.Log<string>("[CustomMapsDetailsScreen::PressButton] Is Driver: " + CustomMapsTerminal.IsDriver.ToString() + ", Button Pressed: " + buttonPressed.ToString(), null);
		if (!base.isActiveAndEnabled || !CustomMapsTerminal.IsDriver)
		{
			return;
		}
		if (buttonPressed == CustomMapKeyboardBinding.goback)
		{
			if (CustomMapManager.IsLoading())
			{
				return;
			}
			if (CustomMapManager.IsUnloading())
			{
				return;
			}
			if (this.mapLoadError)
			{
				this.mapLoadError = false;
				this.loadingMapMessageText.fontSize = 40f;
				CustomMapManager.ClearRoomMap();
				this.ResetToDefaultView();
				return;
			}
			if (CustomMapLoader.IsMapLoaded() || CustomMapManager.GetRoomMapId() != ModId.Null)
			{
				string text;
				if (!this.CanChangeMapState(false, out text))
				{
					this.modDescriptionText.gameObject.SetActive(false);
					this.errorText.text = text;
					this.errorText.gameObject.SetActive(true);
					return;
				}
				this.UnloadMap();
				return;
			}
			else
			{
				if (ModInstallationManagement.CurrentOperationOnMod != null && ModInstallationManagement.CurrentOperationOnMod.Id == this.GetModId())
				{
					GTDev.Log<string>("[CustomMapsDetailsScreen::PressButton] Attempted to go back while this mod is " + ModInstallationManagement.CurrentOperationOnMod.File.State.ToString() + ", ignoring...", null);
					return;
				}
				CustomMapsTerminal.ReturnFromDetailsScreen();
				this.hasModProfile = false;
				this.currentMapMod.OnModUpdated -= this.OnModUpdated;
				this.currentMapMod = null;
				return;
			}
		}
		else
		{
			if (!this.hasModProfile || this.mapLoadError)
			{
				bool flag = this.mapLoadError;
				return;
			}
			if (buttonPressed == CustomMapKeyboardBinding.option3)
			{
				this.RefreshCurrentMapMod();
				return;
			}
			if (buttonPressed == CustomMapKeyboardBinding.map)
			{
				if (this.currentMapMod == null || CustomMapLoader.IsMapLoaded() || CustomMapManager.IsLoading() || CustomMapManager.IsUnloading())
				{
					return;
				}
				this.errorText.gameObject.SetActive(false);
				this.errorText.text = "";
				this.loadingMapLabelText.gameObject.SetActive(false);
				this.loadingMapMessageText.gameObject.SetActive(false);
				this.modDescriptionText.gameObject.SetActive(true);
				ModIOManager.RefreshUserProfile(delegate(bool result)
				{
					if (this.currentMapMod.IsSubscribed)
					{
						ModIOManager.UnsubscribeFromMod(this.GetModId(), delegate(Error error)
						{
							if (!error)
							{
								this.UpdateMapDetails(false);
							}
						});
						return;
					}
					ModIOManager.SubscribeToMod(this.GetModId(), delegate(Error error)
					{
						if (!error)
						{
							this.UpdateMapDetails(false);
						}
					});
				}, false);
			}
			if (buttonPressed == CustomMapKeyboardBinding.enter && !CustomMapManager.IsLoading() && !CustomMapManager.IsUnloading() && !CustomMapLoader.IsMapLoaded() && this.currentMapMod != null && !this.IsCurrentModHidden())
			{
				if (this.currentMapMod.File.State == ModFileState.Installed)
				{
					string text2;
					if (!this.CanChangeMapState(true, out text2))
					{
						this.modDescriptionText.gameObject.SetActive(false);
						this.errorText.text = text2;
						this.errorText.gameObject.SetActive(true);
					}
					else
					{
						this.LoadMap();
					}
				}
				else
				{
					ModFileState modFileState = this.currentMapMod.File.State;
					if (modFileState == ModFileState.Queued || modFileState == ModFileState.None)
					{
						ModIOManager.DownloadMod(this.GetModId(), delegate(bool modDownloadStarted)
						{
							if (modDownloadStarted)
							{
								this.UpdateStatus(false);
							}
						});
					}
					else
					{
						Debug.Log(string.Format("[CustomMapsDetailsScreen::PressButton] mod has status: {0}, ", this.currentMapMod.File.State) + "cannot start download or attempt to load map...");
					}
				}
			}
			if (buttonPressed == CustomMapKeyboardBinding.fav && this.currentMapMod != null)
			{
				if (this.isFavorite)
				{
					ModIOManager.RemoveFavorite(this.currentMapMod.Id);
					this.isFavorite = ModIOManager.IsModFavorited(this.currentMapMod.Id);
					this.favoriteToggleButton.SetButtonActive(this.isFavorite);
					if (this.IsCurrentModHidden())
					{
						this.favoriteToggleButton.gameObject.SetActive(false);
					}
				}
				else if (!this.IsCurrentModHidden())
				{
					ModIOManager.AddFavorite(this.currentMapMod.Id, delegate(Error error)
					{
						this.isFavorite = ModIOManager.IsModFavorited(this.currentMapMod.Id);
						this.favoriteToggleButton.SetButtonActive(this.isFavorite);
					});
				}
			}
			if (buttonPressed == CustomMapKeyboardBinding.delete)
			{
				if (CustomMapManager.IsLoading() || CustomMapManager.IsUnloading() || CustomMapLoader.IsMapLoaded())
				{
					return;
				}
				Mod currentMapMod = this.currentMapMod;
				bool flag2;
				if (currentMapMod != null)
				{
					Modfile file = currentMapMod.File;
					if (file != null)
					{
						ModFileState modFileState = file.State;
						if (modFileState == ModFileState.Queued || modFileState == ModFileState.Installed)
						{
							flag2 = true;
							goto IL_03EC;
						}
					}
				}
				flag2 = false;
				IL_03EC:
				if (flag2)
				{
					this.currentMapMod.UninstallOtherUserMod(true);
					this.UpdateStatus(false);
				}
			}
			if (buttonPressed == CustomMapKeyboardBinding.rateUp)
			{
				this.currentMapMod.RateMod((this.currentMapMod.CurrentUserRating == ModRating.Positive) ? ModRating.None : ModRating.Positive);
			}
			if (buttonPressed == CustomMapKeyboardBinding.rateDown)
			{
				this.currentMapMod.RateMod((this.currentMapMod.CurrentUserRating == ModRating.Negative) ? ModRating.None : ModRating.Negative);
			}
			return;
		}
	}

	private void RefreshCurrentMapMod()
	{
		if (CustomMapLoader.IsMapLoaded() || CustomMapManager.IsLoading() || CustomMapManager.IsUnloading())
		{
			return;
		}
		if (this.hasModProfile)
		{
			long id = this.GetModId()._id;
			this.hasModProfile = false;
			this.currentMapMod.OnModUpdated -= this.OnModUpdated;
			this.currentMapMod = null;
			this.ResetToDefaultView();
			this.RetrieveModFromModIO(id, true, null);
		}
	}

	private void OnProfileReceived(Error error, Mod mod)
	{
		if (error)
		{
			this.modDescriptionText.gameObject.SetActive(false);
			this.errorText.text = string.Format("FAILED TO RETRIEVE MOD DETAILS FOR MOD: {0}", this.GetModId());
			this.errorText.gameObject.SetActive(true);
			return;
		}
		this.SetModProfile(mod);
	}

	private void ResetToDefaultView()
	{
		this.loadingMapLabelText.gameObject.SetActive(false);
		this.loadingMapMessageText.gameObject.SetActive(false);
		this.mapReadyText.gameObject.SetActive(false);
		this.errorText.gameObject.SetActive(false);
		this.modNameText.gameObject.SetActive(false);
		this.modCreatorLabelText.gameObject.SetActive(false);
		this.modCreatorText.gameObject.SetActive(false);
		this.modDescriptionText.gameObject.SetActive(false);
		this.modStatusText.gameObject.SetActive(false);
		this.modSubscriptionStatusText.gameObject.SetActive(false);
		this.mapScreenshotImage.gameObject.SetActive(false);
		this.hiddenRoomMapText.gameObject.SetActive(false);
		this.outdatedText.gameObject.SetActive(false);
		this.unloadPromptText.gameObject.SetActive(false);
		this.loadingText.gameObject.SetActive(true);
		if (CustomMapLoader.IsMapLoaded() || CustomMapManager.IsLoading() || CustomMapManager.IsUnloading())
		{
			ModId modId = new ModId(CustomMapLoader.IsMapLoaded() ? CustomMapLoader.LoadedMapModId : (CustomMapManager.IsLoading() ? CustomMapManager.LoadingMapId : CustomMapManager.UnloadingMapId));
			if (this.hasModProfile && this.GetModId() == modId)
			{
				this.UpdateMapDetails(true);
				return;
			}
			this.RetrieveModFromModIO(modId, false, delegate(Error error, Mod mod)
			{
				this.OnProfileReceived(error, mod);
			});
			return;
		}
		else
		{
			if (CustomMapManager.GetRoomMapId() != ModId.Null)
			{
				this.OnRoomMapChanged(CustomMapManager.GetRoomMapId());
				return;
			}
			if (this.hasModProfile)
			{
				this.UpdateMapDetails(true);
			}
			return;
		}
	}

	private void UpdateMapDetails(bool refreshScreenState = true)
	{
		if (!this.hasModProfile)
		{
			return;
		}
		if (this.IsCurrentModHidden())
		{
			this.modNameText.text = this.hiddenMapTitle;
			this.modDescriptionText.text = this.hiddenMapDesc;
			this.modCreatorLabelText.gameObject.SetActive(false);
			this.modCreatorText.text = "";
			this.mapScreenshotImage.sprite = this.hiddenMapLogo;
			this.mapScreenshotImage.gameObject.SetActive(true);
		}
		else
		{
			this.modNameText.text = this.currentMapMod.Name;
			this.modDescriptionText.text = this.currentMapMod.Description;
			this.modCreatorLabelText.gameObject.SetActive(true);
			this.modCreatorText.text = this.currentMapMod.Creator.Username;
			ModIOManager.GetModLogo(this.currentMapMod, new Action<Error, Texture2D>(this.OnGetModLogo));
		}
		this.UpdateStatus(false);
		if (refreshScreenState)
		{
			this.loadingText.gameObject.SetActive(false);
			this.loadingMapLabelText.gameObject.SetActive(false);
			this.loadingMapMessageText.gameObject.SetActive(false);
			this.hiddenRoomMapText.gameObject.SetActive(false);
			this.mapReadyText.gameObject.SetActive(false);
			this.unloadPromptText.gameObject.SetActive(false);
			this.errorText.gameObject.SetActive(false);
			this.modNameText.gameObject.SetActive(true);
			this.modDescriptionText.gameObject.SetActive(true);
			if (!this.IsCurrentModHidden())
			{
				this.modCreatorLabelText.gameObject.SetActive(true);
				this.modCreatorText.gameObject.SetActive(true);
			}
			if (CustomMapLoader.IsMapLoaded())
			{
				ModId modId = new ModId(CustomMapLoader.LoadedMapModId);
				if (this.GetModId() == modId)
				{
					this.OnMapLoadComplete_UIUpdate();
					return;
				}
				this.RetrieveModFromModIO(modId, false, delegate(Error error, Mod mod)
				{
					this.OnProfileReceived(error, mod);
				});
				return;
			}
			else
			{
				if (CustomMapManager.IsLoading() && !this.mapLoadError)
				{
					this.modDescriptionText.gameObject.SetActive(false);
					if (!CustomMapManager.IsUnloading())
					{
						this.loadingMapLabelText.text = this.mapLoadingString + " 0%";
					}
					else
					{
						this.loadingMapLabelText.text = this.mapUnloadingString;
					}
					this.loadingMapLabelText.gameObject.SetActive(true);
					return;
				}
				if (CustomMapManager.IsUnloading())
				{
					this.modDescriptionText.gameObject.SetActive(false);
					this.loadingMapLabelText.text = this.mapUnloadingString;
					this.loadingMapLabelText.gameObject.SetActive(true);
					return;
				}
				if (CustomMapManager.GetRoomMapId() != ModId.Null)
				{
					this.ShowLoadRoomMapPrompt();
					return;
				}
				if (this.mapLoadError)
				{
					this.modDescriptionText.gameObject.SetActive(false);
					this.loadingMapLabelText.gameObject.SetActive(true);
					this.loadingMapMessageText.gameObject.SetActive(true);
				}
			}
		}
	}

	private void OnGetModLogo(Error error, Texture2D modLogo)
	{
		if (error)
		{
			Debug.LogError(string.Format("[CustomMapsDetailsScreen::OnGetModLogo] Failed to retrieve logo for Mod {0}", this.GetModId()));
			return;
		}
		this.mapScreenshotImage.sprite = Sprite.Create(modLogo, new Rect(0f, 0f, 320f, 180f), new Vector2(0.5f, 0.5f));
		this.mapScreenshotImage.gameObject.SetActive(true);
	}

	private async Task UpdateStatus(bool errorEncountered = false)
	{
		if (base.isActiveAndEnabled && this.currentMapMod != null)
		{
			this.outdatedText.gameObject.SetActive(false);
			this.deleteButton.gameObject.SetActive(false);
			this.subscriptionToggleButton.gameObject.SetActive(false);
			this.favoriteToggleButton.gameObject.SetActive(false);
			this.rateUpButton.gameObject.SetActive(false);
			this.rateDownButton.gameObject.SetActive(false);
			this.modSubscriptionStatusText.gameObject.SetActive(false);
			TMP_Text tmp_Text = this.modStatusLabelText;
			if (tmp_Text != null)
			{
				tmp_Text.gameObject.SetActive(false);
			}
			this.modStatusText.gameObject.SetActive(false);
			if (this.mapLoadError || CustomMapManager.IsUnloading() || CustomMapManager.IsLoading() || CustomMapLoader.IsMapLoaded() || CustomMapManager.GetRoomMapId() != ModId.Null || this.IsCurrentModHidden())
			{
				CustomMapsScreenButton customMapsScreenButton = this.loadButton;
				if (customMapsScreenButton != null)
				{
					customMapsScreenButton.gameObject.SetActive(false);
				}
				if (ModIOManager.IsModFavorited(this.currentMapMod.Id))
				{
					this.favoriteToggleButton.SetButtonActive(true);
					this.favoriteToggleButton.gameObject.SetActive(CustomMapsTerminal.IsDriver);
				}
				if (this.currentMapMod.File.State == ModFileState.Installed)
				{
					this.deleteButton.gameObject.SetActive(CustomMapsTerminal.IsDriver);
				}
			}
			else
			{
				CustomMapsScreenButton customMapsScreenButton2 = this.loadButton;
				if (customMapsScreenButton2 != null)
				{
					customMapsScreenButton2.gameObject.SetActive(true);
				}
				this.isFavorite = ModIOManager.IsModFavorited(this.currentMapMod.Id);
				this.favoriteToggleButton.SetButtonActive(this.isFavorite);
				this.favoriteToggleButton.gameObject.SetActive(CustomMapsTerminal.IsDriver);
				this.rateUpButton.gameObject.SetActive(CustomMapsTerminal.IsDriver);
				this.rateDownButton.gameObject.SetActive(CustomMapsTerminal.IsDriver);
				ModFileState modFileState = (errorEncountered ? ModFileState.FileOperationFailed : this.currentMapMod.File.State);
				this.modStatusText.text = CustomMapsDetailsScreen.modStatusStrings.GetValueOrDefault(modFileState, "STATUS STRING MISSING!");
				if (ModIOManager.IsLoggedIn())
				{
					this.modSubscriptionStatusText.text = (this.currentMapMod.IsSubscribed ? this.subscribedStatusString : this.unsubscribedStatusString);
					this.modSubscriptionStatusText.gameObject.SetActive(true);
					this.subscriptionToggleButton.SetButtonActive(this.currentMapMod.IsSubscribed);
					this.subscriptionToggleButton.SetButtonText(this.currentMapMod.IsSubscribed ? this.unsubscribeString : this.subscribeString);
					this.subscriptionToggleButton.gameObject.SetActive(CustomMapsTerminal.IsDriver);
				}
				if (modFileState != ModFileState.None)
				{
					if (modFileState != ModFileState.Queued)
					{
						if (modFileState == ModFileState.Installed)
						{
							CustomMapsScreenButton customMapsScreenButton3 = this.loadButton;
							if (customMapsScreenButton3 != null)
							{
								customMapsScreenButton3.SetButtonText(this.loadMapString);
							}
							this.deleteButton.gameObject.SetActive(CustomMapsTerminal.IsDriver);
							TaskAwaiter<ValueTuple<bool, int>> taskAwaiter = ModIOManager.IsModOutdated(this.GetModId()).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								TaskAwaiter<ValueTuple<bool, int>> taskAwaiter2;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<ValueTuple<bool, int>>);
							}
							bool item = taskAwaiter.GetResult().Item1;
							this.outdatedText.gameObject.SetActive(item);
						}
					}
					else
					{
						bool flag = ModInstallationManagement.DoesModNeedUpdate(this.currentMapMod);
						CustomMapsScreenButton customMapsScreenButton4 = this.loadButton;
						if (customMapsScreenButton4 != null)
						{
							customMapsScreenButton4.SetButtonText(flag ? this.updateMapString : this.downloadMapString);
						}
						this.modStatusText.text = (flag ? this.mapNeedsUpdateString : this.mapNotDownloadedString);
					}
				}
				else
				{
					CustomMapsScreenButton customMapsScreenButton5 = this.loadButton;
					if (customMapsScreenButton5 != null)
					{
						customMapsScreenButton5.SetButtonText(this.downloadMapString);
					}
				}
				TMP_Text tmp_Text2 = this.modStatusLabelText;
				if (tmp_Text2 != null)
				{
					tmp_Text2.gameObject.SetActive(true);
				}
				this.modStatusText.gameObject.SetActive(true);
			}
		}
	}

	private bool CanChangeMapState(bool load, out string disallowedReason)
	{
		disallowedReason = "";
		if (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.SessionIsPrivate)
		{
			if (!CustomMapManager.AreAllPlayersInVirtualStump())
			{
				disallowedReason = "ALL PLAYERS IN THE ROOM MUST BE INSIDE THE VIRTUAL STUMP BEFORE " + (load ? "" : "UN") + "LOADING A MAP.";
				return false;
			}
			return true;
		}
		else
		{
			if (!CustomMapManager.IsLocalPlayerInVirtualStump())
			{
				disallowedReason = "YOU MUST BE INSIDE THE VIRTUAL STUMP TO " + (load ? "" : "UN") + "LOAD A MAP.";
				return false;
			}
			return true;
		}
	}

	private void LoadMap()
	{
		this.modDescriptionText.gameObject.SetActive(false);
		this.modStatusText.gameObject.SetActive(false);
		this.modSubscriptionStatusText.gameObject.SetActive(false);
		this.outdatedText.gameObject.SetActive(false);
		this.loadingMapLabelText.gameObject.SetActive(true);
		if (NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.SessionIsPrivate)
		{
			NetworkSystem.Instance.ReturnToSinglePlayer();
		}
		this.deleteButton.gameObject.SetActive(false);
		this.subscriptionToggleButton.gameObject.SetActive(false);
		this.networkObject.LoadMapSynced(this.GetModId());
	}

	private void UnloadMap()
	{
		this.networkObject.UnloadMapSynced();
	}

	public void OnMapLoadComplete(bool success)
	{
		if (success)
		{
			this.OnMapLoadComplete_UIUpdate();
		}
	}

	private void OnMapLoadComplete_UIUpdate()
	{
		this.modDescriptionText.gameObject.SetActive(false);
		this.loadingMapLabelText.gameObject.SetActive(false);
		this.loadingMapMessageText.gameObject.SetActive(false);
		this.hiddenRoomMapText.gameObject.SetActive(false);
		this.errorText.gameObject.SetActive(false);
		this.mapReadyText.gameObject.SetActive(true);
		this.unloadPromptText.gameObject.SetActive(true);
	}

	private void OnMapUnloaded()
	{
		this.mapLoadError = false;
		this.loadingMapMessageText.fontSize = 40f;
		this.UpdateMapDetails(true);
	}

	private void OnRoomMapChanged(ModId roomMapID)
	{
		if (roomMapID == ModId.Null)
		{
			this.UpdateMapDetails(true);
			return;
		}
		if (this.GetModId() != roomMapID)
		{
			this.RetrieveModFromModIO(roomMapID, false, new Action<Error, Mod>(this.OnRoomMapRetrieved));
			return;
		}
		this.ShowLoadRoomMapPrompt();
	}

	private void OnRoomMapRetrieved(Error error, Mod mod)
	{
		this.OnProfileReceived(error, mod);
		if (!error)
		{
			this.ShowLoadRoomMapPrompt();
		}
	}

	private void ShowLoadRoomMapPrompt()
	{
		if (CustomMapManager.IsUnloading() || CustomMapManager.IsLoading() || CustomMapLoader.IsMapLoaded(this.GetModId()))
		{
			return;
		}
		this.modDescriptionText.gameObject.SetActive(false);
		this.loadingText.gameObject.SetActive(false);
		this.loadingMapLabelText.gameObject.SetActive(false);
		this.mapReadyText.gameObject.SetActive(false);
		this.unloadPromptText.gameObject.SetActive(false);
		this.hiddenRoomMapText.gameObject.SetActive(false);
		if (this.IsCurrentModHidden())
		{
			this.hiddenRoomMapText.gameObject.SetActive(true);
		}
	}

	public void OnMapLoadProgress(MapLoadStatus loadStatus, int progress, string message)
	{
		if (loadStatus != MapLoadStatus.None)
		{
			this.mapLoadError = false;
			this.loadingMapMessageText.fontSize = 40f;
			this.hiddenRoomMapText.gameObject.SetActive(false);
			this.modDescriptionText.gameObject.SetActive(false);
		}
		switch (loadStatus)
		{
		case MapLoadStatus.Downloading:
			this.loadingMapLabelText.text = this.mapAutoDownloadingString;
			this.loadingMapLabelText.gameObject.SetActive(true);
			this.loadingMapMessageText.gameObject.SetActive(false);
			this.loadingMapMessageText.text = "";
			return;
		case MapLoadStatus.Loading:
			this.loadingMapLabelText.text = this.mapLoadingString + " " + progress.ToString() + "%";
			this.loadingMapLabelText.gameObject.SetActive(true);
			this.loadingMapMessageText.text = message;
			this.loadingMapMessageText.gameObject.SetActive(true);
			return;
		case MapLoadStatus.Unloading:
			this.mapReadyText.gameObject.SetActive(false);
			this.unloadPromptText.gameObject.SetActive(false);
			this.loadingMapLabelText.text = this.mapUnloadingString;
			this.loadingMapLabelText.gameObject.SetActive(true);
			this.loadingMapMessageText.gameObject.SetActive(false);
			this.loadingMapMessageText.text = "";
			return;
		case MapLoadStatus.Error:
			this.mapLoadError = true;
			this.loadingMapLabelText.text = this.mapLoadingErrorString;
			this.loadingMapLabelText.gameObject.SetActive(true);
			if (CustomMapsTerminal.IsDriver)
			{
				this.loadingMapMessageText.text = message + "\n" + this.mapLoadingErrorDriverString;
			}
			else
			{
				this.loadingMapMessageText.text = message + "\n" + this.mapLoadingErrorNonDriverString;
			}
			if (this.loadingMapMessageText.text.Length > 150)
			{
				this.loadingMapMessageText.fontSize = 30f;
			}
			else
			{
				this.loadingMapMessageText.fontSize = 40f;
			}
			this.loadingMapMessageText.gameObject.SetActive(true);
			return;
		default:
			return;
		}
	}

	public ModId GetModId()
	{
		Mod currentMapMod = this.currentMapMod;
		if (currentMapMod == null)
		{
			return ModId.Null;
		}
		return currentMapMod.Id;
	}

	public bool IsCurrentModHidden()
	{
		return this.hasModProfile && (this.currentMapMod.Creator == null || (!ModIOManager.IsLoggedIn() && this.currentMapMod.IsHidden()));
	}

	[SerializeField]
	private SpriteRenderer mapScreenshotImage;

	[SerializeField]
	private Sprite hiddenMapLogo;

	[SerializeField]
	private TMP_Text loadingText;

	[SerializeField]
	private TMP_Text modNameText;

	[SerializeField]
	private TMP_Text modCreatorLabelText;

	[SerializeField]
	private TMP_Text modCreatorText;

	[SerializeField]
	private TMP_Text modDescriptionText;

	[SerializeField]
	private TMP_Text modStatusText;

	[SerializeField]
	private TMP_Text modStatusLabelText;

	[SerializeField]
	private TMP_Text modSubscriptionStatusText;

	[SerializeField]
	private TMP_Text loadingMapLabelText;

	[SerializeField]
	private TMP_Text loadingMapMessageText;

	[SerializeField]
	private TMP_Text hiddenRoomMapText;

	[SerializeField]
	private TMP_Text mapReadyText;

	[SerializeField]
	private TMP_Text unloadPromptText;

	[SerializeField]
	private TMP_Text errorText;

	[SerializeField]
	private TMP_Text outdatedText;

	[SerializeField]
	private CustomMapsScreenButton subscriptionToggleButton;

	[SerializeField]
	private CustomMapsScreenButton favoriteToggleButton;

	[SerializeField]
	private CustomMapsScreenButton rateUpButton;

	[SerializeField]
	private CustomMapsScreenButton rateDownButton;

	[SerializeField]
	private CustomMapsScreenButton loadButton;

	[SerializeField]
	private CustomMapsScreenButton deleteButton;

	[SerializeField]
	private string modAvailableString = "AVAILABLE";

	[SerializeField]
	private string mapAutoDownloadingString = "DOWNLOADING...";

	[SerializeField]
	private string mapDownloadQueuedString = "DOWNLOAD QUEUED";

	[SerializeField]
	private string mapLoadingString = "LOADING:";

	[SerializeField]
	private string mapUnloadingString = "UNLOADING...";

	[SerializeField]
	private string mapLoadingErrorString = "ERROR:";

	[SerializeField]
	private string mapLoadingErrorDriverString = "PRESS THE 'BACK' BUTTON TO TRY AGAIN";

	[SerializeField]
	private string mapLoadingErrorNonDriverString = "LEAVE AND REJOIN THE VIRTUAL STUMP TO TRY AGAIN";

	[SerializeField]
	private string mapLoadingErrorInvalidModFile = "INSTALL FAILED DUE TO INVALID MAP FILE";

	[SerializeField]
	private VirtualStumpSerializer networkObject;

	public static Dictionary<ModFileState, string> modStatusStrings = new Dictionary<ModFileState, string>
	{
		{
			ModFileState.Installed,
			"READY"
		},
		{
			ModFileState.Queued,
			"QUEUED"
		},
		{
			ModFileState.Downloading,
			"DOWNLOADING"
		},
		{
			ModFileState.Installing,
			"INSTALLING"
		},
		{
			ModFileState.Uninstalling,
			"UNINSTALLING"
		},
		{
			ModFileState.Updating,
			"UPDATING"
		},
		{
			ModFileState.FileOperationFailed,
			"ERROR"
		},
		{
			ModFileState.None,
			"AVAILABLE"
		}
	};

	[SerializeField]
	private string mapNotDownloadedString = "NOT DOWNLOADED";

	[SerializeField]
	private string mapNeedsUpdateString = "NEEDS UPDATE";

	[SerializeField]
	private string subscribeString = "SUBSCRIBE";

	[SerializeField]
	private string unsubscribeString = "UNSUBSCRIBE";

	[SerializeField]
	private string subscribedStatusString = "SUBSCRIBED";

	[SerializeField]
	private string unsubscribedStatusString = "NOT SUBSCRIBED";

	[SerializeField]
	private string loadMapString = "LOAD";

	[SerializeField]
	private string downloadMapString = "DOWNLOAD";

	[SerializeField]
	private string updateMapString = "UPDATE";

	[SerializeField]
	private string hiddenMapTitle = "HIDDEN MAP";

	[SerializeField]
	private string hiddenMapDesc = "YOU DON'T CURRENTLY HAVE ACCESS TO THIS HIDDEN MAP.\nCHECK THAT YOU'RE LOGGED IN TO THE CORRECT MOD.IO ACCOUNT.";

	private const float LOGO_WIDTH = 320f;

	private const float LOGO_HEIGHT = 180f;

	public long pendingModId;

	private bool hasModProfile;

	private bool mapLoadError;

	private bool isFavorite;
}
