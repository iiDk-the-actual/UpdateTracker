using System;
using System.Collections.Generic;
using System.Text;
using GorillaGameModes;
using TMPro;
using UnityEngine;

public class GorillaScoreBoard : MonoBehaviour
{
	public bool IsDirty
	{
		get
		{
			return this._isDirty || string.IsNullOrEmpty(this.initialGameMode);
		}
		set
		{
			this._isDirty = value;
		}
	}

	public void SetSleepState(bool awake)
	{
		this.boardText.enabled = awake;
		this.buttonText.enabled = awake;
		if (this.linesParent != null)
		{
			this.linesParent.SetActive(awake);
		}
	}

	private void OnDestroy()
	{
	}

	public string GetBeginningString()
	{
		return "ROOM ID: " + (NetworkSystem.Instance.SessionIsPrivate ? "-PRIVATE- GAME: " : (NetworkSystem.Instance.RoomName + "   GAME: ")) + this.RoomType() + "\n  PLAYER     COLOR  MUTE   REPORT";
	}

	public string RoomType()
	{
		this.initialGameMode = RoomSystem.RoomGameMode;
		this.gmNames = GameMode.gameModeNames;
		this.gmName = "ERROR";
		int count = this.gmNames.Count;
		for (int i = 0; i < count; i++)
		{
			this.tempGmName = this.gmNames[i];
			if (this.initialGameMode.Contains(this.tempGmName))
			{
				this.gmName = this.tempGmName;
				break;
			}
		}
		return this.gmName;
	}

	public void RedrawPlayerLines()
	{
		this.stringBuilder.Clear();
		this.stringBuilder.Append(this.GetBeginningString());
		this.buttonStringBuilder.Clear();
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags);
		for (int i = 0; i < this.lines.Count; i++)
		{
			try
			{
				if (this.lines[i].gameObject.activeInHierarchy)
				{
					this.lines[i].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0f, (float)(this.startingYValue - this.lineHeight * i), 0f);
					if (this.lines[i].linePlayer != null && this.lines[i].linePlayer.InRoom)
					{
						this.stringBuilder.Append("\n ");
						this.stringBuilder.Append(flag ? this.lines[i].playerNameVisible : this.lines[i].linePlayer.DefaultName);
						if (this.lines[i].linePlayer != NetworkSystem.Instance.LocalPlayer)
						{
							if (this.lines[i].reportButton.isActiveAndEnabled)
							{
								this.buttonStringBuilder.Append("MUTE                                REPORT\n");
							}
							else
							{
								this.buttonStringBuilder.Append("MUTE                HATE SPEECH    TOXICITY     CHEATING       CANCEL\n");
							}
						}
						else
						{
							this.buttonStringBuilder.Append("\n");
						}
					}
				}
			}
			catch
			{
			}
		}
		this.boardText.text = this.stringBuilder.ToString();
		this.buttonText.text = this.buttonStringBuilder.ToString();
		this._isDirty = false;
	}

	public string NormalizeName(bool doIt, string text)
	{
		if (doIt)
		{
			text = new string(Array.FindAll<char>(text.ToCharArray(), (char c) => Utils.IsASCIILetterOrDigit(c)));
			if (text.Length > 12)
			{
				text = text.Substring(0, 10);
			}
			text = text.ToUpper();
		}
		return text;
	}

	private void Start()
	{
		GorillaScoreboardTotalUpdater.RegisterScoreboard(this);
	}

	private void OnEnable()
	{
		GorillaScoreboardTotalUpdater.RegisterScoreboard(this);
		this._isDirty = true;
	}

	private void OnDisable()
	{
		GorillaScoreboardTotalUpdater.UnregisterScoreboard(this);
	}

	public GameObject scoreBoardLinePrefab;

	public int startingYValue;

	public int lineHeight;

	public bool includeMMR;

	public bool isActive;

	public GameObject linesParent;

	[SerializeField]
	public List<GorillaPlayerScoreboardLine> lines;

	public TextMeshPro boardText;

	public TextMeshPro buttonText;

	public bool needsUpdate;

	public TextMeshPro notInRoomText;

	public string initialGameMode;

	private string tempGmName;

	private string gmName;

	private const string error = "ERROR";

	private List<string> gmNames;

	private bool _isDirty = true;

	private StringBuilder stringBuilder = new StringBuilder(220);

	private StringBuilder buttonStringBuilder = new StringBuilder(720);
}
