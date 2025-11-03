using System;
using System.Collections.Generic;
using System.Text;
using KID.Model;
using TMPro;
using UnityEngine;

namespace GorillaTagScripts
{
	public class PlayerTimerBoard : MonoBehaviour
	{
		public bool IsDirty { get; set; } = true;

		private void Start()
		{
			this.TryInit();
		}

		private void OnEnable()
		{
			this.TryInit();
			LocalisationManager.RegisterOnLanguageChanged(new Action(this.RedrawPlayerLines));
		}

		private void TryInit()
		{
			if (this.isInitialized)
			{
				return;
			}
			if (PlayerTimerManager.instance == null)
			{
				return;
			}
			PlayerTimerManager.instance.RegisterTimerBoard(this);
			this.isInitialized = true;
		}

		private void OnDisable()
		{
			if (PlayerTimerManager.instance != null)
			{
				PlayerTimerManager.instance.UnregisterTimerBoard(this);
			}
			this.isInitialized = false;
			LocalisationManager.UnregisterOnLanguageChanged(new Action(this.RedrawPlayerLines));
		}

		public void SetSleepState(bool awake)
		{
			this.playerColumn.enabled = awake;
			this.timeColumn.enabled = awake;
			if (this.linesParent != null)
			{
				this.linesParent.SetActive(awake);
			}
		}

		public void SortLines()
		{
			this.lines.Sort(new Comparison<PlayerTimerBoardLine>(PlayerTimerBoardLine.CompareByTotalTime));
		}

		public void RedrawPlayerLines()
		{
			this.stringBuilder.Clear();
			this.stringBuilderTime.Clear();
			string text;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_TIMER_BOARD_COLUMN_PLAYER", out text, "<b><color=yellow>PLAYER</color></b>"))
			{
				Debug.LogError("[LOCALIZATION::MONKE_BLOCKS::TIMER] Failed to get key for Game Mode [MONKE_BLOCKS_TIMER_BOARD_COLUMN_PLAYER]");
			}
			this.stringBuilder.Append("<b><color=yellow>");
			this.stringBuilder.Append(text);
			this.stringBuilder.Append("</color></b>");
			if (!LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_TIMER_BOARD_COLUMN_TIMES", out text, "<b><color=yellow>LATEST TIME</color></b>"))
			{
				Debug.LogError("[LOCALIZATION::MONKE_BLOCKS::TIMER] Failed to get key for Game Mode [MONKE_BLOCKS_TIMER_BOARD_COLUMN_TIMES]");
			}
			this.stringBuilderTime.Append("<b><color=yellow>");
			this.stringBuilderTime.Append(text);
			this.stringBuilderTime.Append("</color></b>");
			this.SortLines();
			Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Custom_Nametags);
			bool flag = (permissionDataByFeature.Enabled || permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PLAYER) && permissionDataByFeature.ManagedBy != Permission.ManagedByEnum.PROHIBITED;
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
							this.stringBuilderTime.Append("\n ");
							this.stringBuilderTime.Append(this.lines[i].playerTimeStr);
						}
					}
				}
				catch
				{
				}
			}
			this.playerColumn.text = this.stringBuilder.ToString();
			this.timeColumn.text = this.stringBuilderTime.ToString();
			this.IsDirty = false;
		}

		[SerializeField]
		private GameObject linesParent;

		public List<PlayerTimerBoardLine> lines;

		public TextMeshPro notInRoomText;

		public TextMeshPro playerColumn;

		public TextMeshPro timeColumn;

		[SerializeField]
		private int startingYValue;

		[SerializeField]
		private int lineHeight;

		private StringBuilder stringBuilder = new StringBuilder(220);

		private StringBuilder stringBuilderTime = new StringBuilder(220);

		private const string MONKE_BLOCKS_TIMER_BOARD_COLUMN_PLAYER_KEY = "MONKE_BLOCKS_TIMER_BOARD_COLUMN_PLAYER";

		private const string MONKE_BLOCKS_TIMER_BOARD_COLUMN_TIMES_KEY = "MONKE_BLOCKS_TIMER_BOARD_COLUMN_TIMES";

		private bool isInitialized;
	}
}
