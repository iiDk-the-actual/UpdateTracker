using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GorillaTagScripts;
using GorillaTagScripts.Builder;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BuilderScanKiosk : MonoBehaviourTick
{
	public static bool IsSaveSlotValid(int slot)
	{
		return slot >= 0 && slot < BuilderScanKiosk.NUM_SAVE_SLOTS;
	}

	private void Start()
	{
		if (this.saveButton != null)
		{
			this.saveButton.onPressButton.AddListener(new UnityAction(this.OnSavePressed));
		}
		if (this.targetTable != null)
		{
			this.targetTable.OnSaveDirtyChanged.AddListener(new UnityAction<bool>(this.OnSaveDirtyChanged));
			this.targetTable.OnSaveSuccess.AddListener(new UnityAction(this.OnSaveSuccess));
			this.targetTable.OnSaveFailure.AddListener(new UnityAction<string>(this.OnSaveFail));
			SharedBlocksManager.OnSaveTimeUpdated += this.OnSaveTimeUpdated;
		}
		if (this.noneButton != null)
		{
			this.noneButton.onPressButton.AddListener(new UnityAction(this.OnNoneButtonPressed));
		}
		foreach (GorillaPressableButton gorillaPressableButton in this.scanButtons)
		{
			gorillaPressableButton.onPressed += this.OnScanButtonPressed;
		}
		this.scanTriangle = this.scanAnimation.GetComponent<MeshRenderer>();
		this.scanTriangle.enabled = false;
		this.scannerState = BuilderScanKiosk.ScannerState.IDLE;
		this.LoadPlayerPrefs();
		this.UpdateUI();
	}

	private new void OnEnable()
	{
		LocalisationManager.RegisterOnLanguageChanged(new Action(this.UpdateUI));
	}

	private new void OnDisable()
	{
		LocalisationManager.UnregisterOnLanguageChanged(new Action(this.UpdateUI));
	}

	private void OnDestroy()
	{
		if (this.saveButton != null)
		{
			this.saveButton.onPressButton.RemoveListener(new UnityAction(this.OnSavePressed));
		}
		SharedBlocksManager.OnSaveTimeUpdated -= this.OnSaveTimeUpdated;
		if (this.targetTable != null)
		{
			this.targetTable.OnSaveDirtyChanged.RemoveListener(new UnityAction<bool>(this.OnSaveDirtyChanged));
			this.targetTable.OnSaveFailure.RemoveListener(new UnityAction<string>(this.OnSaveFail));
		}
		if (this.noneButton != null)
		{
			this.noneButton.onPressButton.RemoveListener(new UnityAction(this.OnNoneButtonPressed));
		}
		foreach (GorillaPressableButton gorillaPressableButton in this.scanButtons)
		{
			if (!(gorillaPressableButton == null))
			{
				gorillaPressableButton.onPressed -= this.OnScanButtonPressed;
			}
		}
	}

	private void OnNoneButtonPressed()
	{
		if (this.targetTable == null)
		{
			return;
		}
		if (this.scannerState == BuilderScanKiosk.ScannerState.CONFIRMATION)
		{
			this.scannerState = BuilderScanKiosk.ScannerState.IDLE;
		}
		if (this.targetTable.CurrentSaveSlot != -1)
		{
			this.targetTable.CurrentSaveSlot = -1;
			this.SavePlayerPrefs();
			this.UpdateUI();
		}
	}

	private void OnScanButtonPressed(GorillaPressableButton button, bool isLeft)
	{
		if (this.targetTable == null)
		{
			return;
		}
		if (this.scannerState == BuilderScanKiosk.ScannerState.CONFIRMATION)
		{
			this.scannerState = BuilderScanKiosk.ScannerState.IDLE;
		}
		int i = 0;
		while (i < this.scanButtons.Count)
		{
			if (button.Equals(this.scanButtons[i]))
			{
				if (i != this.targetTable.CurrentSaveSlot)
				{
					this.targetTable.CurrentSaveSlot = i;
					this.SavePlayerPrefs();
					this.UpdateUI();
					return;
				}
				break;
			}
			else
			{
				i++;
			}
		}
	}

	public void OnDevScanPressed()
	{
	}

	private void LoadPlayerPrefs()
	{
		int @int = PlayerPrefs.GetInt(BuilderScanKiosk.playerPrefKey, -1);
		this.targetTable.CurrentSaveSlot = @int;
		this.UpdateUI();
	}

	private void SavePlayerPrefs()
	{
		PlayerPrefs.SetInt(BuilderScanKiosk.playerPrefKey, this.targetTable.CurrentSaveSlot);
		PlayerPrefs.Save();
	}

	private void ToggleSaveButton(bool enabled)
	{
		if (enabled)
		{
			this.saveButton.enabled = true;
			this.saveButton.buttonRenderer.material = this.saveButton.unpressedMaterial;
			return;
		}
		this.saveButton.enabled = false;
		this.saveButton.buttonRenderer.material = this.saveButton.pressedMaterial;
	}

	public override void Tick()
	{
		if (this.isAnimating)
		{
			if (this.scanAnimation == null)
			{
				this.isAnimating = false;
			}
			else if ((double)Time.time > this.scanCompleteTime)
			{
				this.scanTriangle.enabled = false;
				this.isAnimating = false;
			}
		}
		if (this.coolingDown && (double)Time.time > this.coolDownCompleteTime)
		{
			this.coolingDown = false;
			this.UpdateUI();
		}
	}

	private void OnSavePressed()
	{
		if (this.targetTable == null || !this.isDirty || this.coolingDown)
		{
			return;
		}
		BuilderScanKiosk.ScannerState scannerState = this.scannerState;
		if (scannerState == BuilderScanKiosk.ScannerState.IDLE)
		{
			this.scannerState = BuilderScanKiosk.ScannerState.CONFIRMATION;
			this.UpdateUI();
			return;
		}
		if (scannerState != BuilderScanKiosk.ScannerState.CONFIRMATION)
		{
			return;
		}
		this.scannerState = BuilderScanKiosk.ScannerState.SAVING;
		if (this.scanAnimation != null)
		{
			this.scanCompleteTime = (double)(Time.time + this.scanAnimation.clip.length);
			this.scanTriangle.enabled = true;
			this.scanAnimation.Rewind();
			this.scanAnimation.Play();
		}
		if (this.soundBank != null)
		{
			this.soundBank.Play();
		}
		this.isAnimating = true;
		this.saveError = false;
		this.errorMsg = string.Empty;
		this.coolDownCompleteTime = (double)(Time.time + this.saveCooldownSeconds);
		this.coolingDown = true;
		this.UpdateUI();
		string text;
		LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SAVE_ERROR_BUSY", out text, "BUSY");
		string text2;
		LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SAVE_ERROR_BLOCKS", out text2, "PLEASE REMOVE BLOCKS CONNECTED OUTSIDE OF TABLE PLATFORM");
		this.targetTable.SaveTableForPlayer(text, text2);
	}

	private string GetSavePath()
	{
		return string.Concat(new string[]
		{
			this.GetSaveFolder(),
			Path.DirectorySeparatorChar.ToString(),
			BuilderScanKiosk.SAVE_FILE,
			"_",
			this.targetTable.CurrentSaveSlot.ToString(),
			".png"
		});
	}

	private string GetSaveFolder()
	{
		return Application.persistentDataPath + Path.DirectorySeparatorChar.ToString() + BuilderScanKiosk.SAVE_FOLDER;
	}

	private void OnSaveDirtyChanged(bool dirty)
	{
		this.isDirty = dirty;
		this.UpdateUI();
	}

	private void OnSaveTimeUpdated()
	{
		this.scannerState = BuilderScanKiosk.ScannerState.IDLE;
		this.saveError = false;
		this.UpdateUI();
	}

	private void OnSaveSuccess()
	{
		this.scannerState = BuilderScanKiosk.ScannerState.IDLE;
		this.saveError = false;
		this.UpdateUI();
	}

	private void OnSaveFail(string errorMsg)
	{
		this.scannerState = BuilderScanKiosk.ScannerState.IDLE;
		this.saveError = true;
		this.errorMsg = errorMsg;
		this.UpdateUI();
	}

	private void UpdateUI()
	{
		this.screenText.text = this.GetTextForScreen();
		this.ToggleSaveButton(BuilderScanKiosk.IsSaveSlotValid(this.targetTable.CurrentSaveSlot) && !this.coolingDown);
		this.noneButton.buttonRenderer.material = ((!BuilderScanKiosk.IsSaveSlotValid(this.targetTable.CurrentSaveSlot)) ? this.noneButton.pressedMaterial : this.noneButton.unpressedMaterial);
		for (int i = 0; i < this.scanButtons.Count; i++)
		{
			this.scanButtons[i].buttonRenderer.material = ((this.targetTable.CurrentSaveSlot == i) ? this.scanButtons[i].pressedMaterial : this.scanButtons[i].unpressedMaterial);
		}
		if (this.scannerState == BuilderScanKiosk.ScannerState.CONFIRMATION)
		{
			string text;
			LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_UPDATE_CONFIRM_BUTTON", out text, "YES UPDATE SCAN");
			this.saveButton.myTmpText.text = text;
			return;
		}
		string text2;
		LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_UPDATED_BUTTON", out text2, "UPDATE SCAN");
		this.saveButton.myTmpText.text = text2;
	}

	private string GetTextForScreen()
	{
		if (this.targetTable == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = "";
		int currentSaveSlot = this.targetTable.CurrentSaveSlot;
		if (!BuilderScanKiosk.IsSaveSlotValid(currentSaveSlot))
		{
			LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_NO_SAVE_SLOT", out text, "<b><color=red>NONE</color></b>");
			stringBuilder.Append(text);
		}
		else if (currentSaveSlot == BuilderScanKiosk.DEV_SAVE_SLOT)
		{
			stringBuilder.Append("<b><color=red>DEV SCAN</color></b>");
		}
		else
		{
			stringBuilder.Append("<b><color=red>");
			LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SCAN_LABEL", out text, "SCAN ");
			stringBuilder.Append(text);
			stringBuilder.Append(currentSaveSlot + 1);
			stringBuilder.Append("</color></b>");
			SharedBlocksManager.LocalPublishInfo publishInfoForSlot = SharedBlocksManager.GetPublishInfoForSlot(currentSaveSlot);
			DateTime dateTime = DateTime.FromBinary(publishInfoForSlot.publishTime);
			if (dateTime > DateTime.MinValue)
			{
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_UPDATE_LABEL", out text, "UPDATED ");
				stringBuilder.Append(": ");
				stringBuilder.Append(text);
				stringBuilder.Append(dateTime.ToString());
				stringBuilder.Append("\n");
			}
			if (SharedBlocksManager.IsMapIDValid(publishInfoForSlot.mapID))
			{
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_MAP_ID_LABEL", out text, "MAP ID: ");
				stringBuilder.Append(text);
				stringBuilder.Append(publishInfoForSlot.mapID.Substring(0, 4));
				stringBuilder.Append("-");
				stringBuilder.Append(publishInfoForSlot.mapID.Substring(4));
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_CODE_INSTRUCTIONS", out text, "\nUSE THIS CODE IN THE SHARE MY BLOCKS ROOM");
				stringBuilder.Append(text);
			}
		}
		stringBuilder.Append("\n");
		switch (this.scannerState)
		{
		case BuilderScanKiosk.ScannerState.IDLE:
			if (this.saveError)
			{
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SAVE_ERROR", out text, "ERROR WHILE SCANNING: ");
				stringBuilder.Append(text);
				stringBuilder.Append(this.errorMsg);
			}
			else if (this.coolingDown)
			{
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SAVE_COOLDOWN", out text, "COOLING DOWN...");
				stringBuilder.Append(text);
			}
			else if (!this.isDirty)
			{
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SAVE_NO_CHANGES", out text, "NO UNSAVED CHANGES");
				stringBuilder.Append(text);
			}
			break;
		case BuilderScanKiosk.ScannerState.CONFIRMATION:
			LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SAVE_WARNING_REPLACE", out text, "YOU ARE ABOUT TO REPLACE ");
			if (currentSaveSlot == BuilderScanKiosk.DEV_SAVE_SLOT)
			{
				stringBuilder.Append(text);
				stringBuilder.Append("<b><color=red>DEV SCAN</color></b>");
			}
			else
			{
				stringBuilder.Append(text);
				stringBuilder.Append("<b><color=red>");
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SCAN_LABEL", out text, "SCAN ");
				stringBuilder.Append(text);
				stringBuilder.Append(currentSaveSlot + 1);
				stringBuilder.Append("</color></b>");
			}
			LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SAVE_WARNING_CONFIRMATION", out text, " ARE YOU SURE YOU WANT TO SCAN?");
			stringBuilder.Append(text);
			break;
		case BuilderScanKiosk.ScannerState.SAVING:
			LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SAVE_SAVING", out text, "SCANNING BUILD...");
			stringBuilder.Append(text);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		stringBuilder.Append("\n\n\n");
		LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_LOAD_INSTRUCTIONS", out text, "CREATE A <b><color=red>NEW</color></b> PRIVATE ROOM TO LOAD ");
		stringBuilder.Append(text);
		if (!BuilderScanKiosk.IsSaveSlotValid(currentSaveSlot))
		{
			LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_EMPTY_TABLE", out text, "<b><color=red>AN EMPTY TABLE</color></b>");
			stringBuilder.Append(text);
		}
		else if (currentSaveSlot == BuilderScanKiosk.DEV_SAVE_SLOT)
		{
			stringBuilder.Append("<b><color=red>DEV SCAN</color></b>");
		}
		else
		{
			LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_SAVE_KIOSK_SCAN_LABEL", out text, "SCAN ");
			stringBuilder.Append("<b><color=red>");
			stringBuilder.Append(text);
			stringBuilder.Append(currentSaveSlot + 1);
			stringBuilder.Append("</color></b>");
		}
		return stringBuilder.ToString();
	}

	private const string MONKE_BLOCKS_SAVE_KIOSK_NO_SAVE_SLOT_KEY = "MONKE_BLOCKS_SAVE_KIOSK_NO_SAVE_SLOT";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SCAN_LABEL_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SCAN_LABEL";

	private const string MONKE_BLOCKS_SAVE_KIOSK_UPDATE_LABEL_KEY = "MONKE_BLOCKS_SAVE_KIOSK_UPDATE_LABEL";

	private const string MONKE_BLOCKS_SAVE_KIOSK_MAP_ID_LABEL_KEY = "MONKE_BLOCKS_SAVE_KIOSK_MAP_ID_LABEL";

	private const string MONKE_BLOCKS_SAVE_KIOSK_CODE_INSTRUCTIONS_KEY = "MONKE_BLOCKS_SAVE_KIOSK_CODE_INSTRUCTIONS";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SAVE_ERROR_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SAVE_ERROR";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SAVE_ERROR_BUSY_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SAVE_ERROR_BUSY";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SAVE_ERROR_BLOCKS_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SAVE_ERROR_BLOCKS";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SAVE_COOLDOWN_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SAVE_COOLDOWN";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SAVE_NO_CHANGES_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SAVE_NO_CHANGES";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SAVE_WARNING_REPLACE_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SAVE_WARNING_REPLACE";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SAVE_WARNING_CONFIRMATION_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SAVE_WARNING_CONFIRMATION";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SAVE_SAVING_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SAVE_SAVING";

	private const string MONKE_BLOCKS_SAVE_KIOSK_LOAD_INSTRUCTIONS_KEY = "MONKE_BLOCKS_SAVE_KIOSK_LOAD_INSTRUCTIONS";

	private const string MONKE_BLOCKS_SAVE_KIOSK_EMPTY_TABLE_KEY = "MONKE_BLOCKS_SAVE_KIOSK_EMPTY_TABLE";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SLOT_NONE_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SLOT_NONE";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SLOT_ONE_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SLOT_ONE";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SLOT_TWO_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SLOT_TWO";

	private const string MONKE_BLOCKS_SAVE_KIOSK_SLOT_THREE_KEY = "MONKE_BLOCKS_SAVE_KIOSK_SLOT_THREE";

	private const string MONKE_BLOCKS_SAVE_KIOSK_UPDATED_BUTTON_KEY = "MONKE_BLOCKS_SAVE_KIOSK_UPDATED_BUTTON";

	private const string MONKE_BLOCKS_SAVE_KIOSK_UPDATE_CONFIRM_BUTTON_KEY = "MONKE_BLOCKS_SAVE_KIOSK_UPDATE_CONFIRM_BUTTON";

	[SerializeField]
	private GorillaPressableButton saveButton;

	[SerializeField]
	private GorillaPressableButton noneButton;

	[SerializeField]
	private List<GorillaPressableButton> scanButtons;

	[SerializeField]
	private BuilderTable targetTable;

	[SerializeField]
	private float saveCooldownSeconds = 5f;

	[SerializeField]
	private TMP_Text screenText;

	[SerializeField]
	private SoundBankPlayer soundBank;

	[SerializeField]
	private Animation scanAnimation;

	private MeshRenderer scanTriangle;

	private bool isAnimating;

	private static string playerPrefKey = "BuilderSaveSlot";

	private static string SAVE_FOLDER = "MonkeBlocks";

	private static string SAVE_FILE = "MyBuild";

	public static int NUM_SAVE_SLOTS = 3;

	public static int DEV_SAVE_SLOT = -2;

	private Texture2D buildCaptureTexture;

	private bool isDirty;

	private bool saveError;

	private string errorMsg = string.Empty;

	private bool coolingDown;

	private double coolDownCompleteTime;

	private double scanCompleteTime;

	private BuilderScanKiosk.ScannerState scannerState;

	private enum ScannerState
	{
		IDLE,
		CONFIRMATION,
		SAVING
	}
}
