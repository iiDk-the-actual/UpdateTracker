using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SIUIPlayerQuestDisplay : MonoBehaviour, IGorillaSliceableSimple
{
	public void RefreshDisplay()
	{
		SIPlayer siplayer = SIPlayer.Get(this.activePlayerActorNumber);
		bool flag = siplayer != null && siplayer.gamePlayer != null && siplayer.gamePlayer.rig != null && siplayer.gamePlayer.rig.Creator != null && this.activePlayerActorNumber > 0;
		if (!flag || !SIProgression.Instance.ClientReady)
		{
			if (this.activePlayer.activeSelf)
			{
				this.activePlayer.SetActive(false);
			}
			if (!this.waitingForPlayer.activeSelf)
			{
				this.waitingForPlayer.SetActive(true);
			}
			this.displayBackground.color = this.noPlayerColor;
			this.smallDisplayBackground.color = this.noPlayerColor;
			return;
		}
		if (this.activePlayer.activeSelf != flag)
		{
			this.activePlayer.SetActive(flag);
		}
		if (this.waitingForPlayer.activeSelf == flag)
		{
			this.waitingForPlayer.SetActive(!flag);
		}
		if (!flag)
		{
			this.displayBackground.color = this.noPlayerColor;
			this.smallDisplayBackground.color = this.noPlayerColor;
			return;
		}
		Color color = ((siplayer == SIPlayer.LocalPlayer) ? this.localPlayerColor : this.remotePlayerColor);
		this.displayBackground.color = color;
		this.smallDisplayBackground.color = color;
		string sanitizedNickName = siplayer.gamePlayer.rig.Creator.SanitizedNickName;
		if (this.lastNickName != sanitizedNickName)
		{
			this.playerName.text = sanitizedNickName;
		}
		this.lastNickName = sanitizedNickName;
		int num = siplayer.CurrentProgression.resourceArray[0];
		if (this.lastTechPoints != num)
		{
			this.playerTechPoints.text = string.Format("TECH POINTS: {0}", num);
		}
		this.lastTechPoints = num;
		bool flag2 = siplayer.HasLimitedResourceBeenDeposited(SIResource.LimitedDepositType.MonkeIdol);
		if (flag2 != this.monkeIdolIcon.enabled)
		{
			this.monkeIdolIcon.enabled = flag2;
		}
		int stashedQuests = siplayer.CurrentProgression.stashedQuests;
		if (this.lastStashedQuests != stashedQuests)
		{
			this.stashedQuestCount.text = string.Format("STASHED QUESTS: {0}/{1}", Mathf.Max(0, stashedQuests - 3), 6);
		}
		this.lastStashedQuests = stashedQuests;
		int stashedBonusPoints = siplayer.CurrentProgression.stashedBonusPoints;
		if (this.lastStashedBonusPoints != stashedBonusPoints)
		{
			this.stashedBonusPointCount.text = string.Format("STASHED BONUS: {0}/{1}", Mathf.Max(0, stashedBonusPoints - 1), 2);
		}
		this.lastStashedBonusPoints = stashedBonusPoints;
		int bonusProgress = siplayer.CurrentProgression.bonusProgress;
		if (this.lastBonusProgress != bonusProgress)
		{
			this.sharedProgress.UpdateFillPercent((float)bonusProgress / 10f);
			this.sharedProgress.progressText.text = string.Format("{0}%", Mathf.Min(100, bonusProgress * 10));
		}
		this.lastBonusProgress = bonusProgress;
		bool flag3 = siplayer.CurrentProgression.stashedBonusPoints > 0;
		if (this.bonusPointsInProgress.activeSelf != flag3)
		{
			this.bonusPointsInProgress.SetActive(flag3);
		}
		if (this.bonusPointsCompleted.activeSelf == flag3)
		{
			this.bonusPointsCompleted.SetActive(!flag3);
		}
		bool flag4 = siplayer.CurrentProgression.bonusProgress >= 10;
		if (this.collectBonusButton.activeSelf != flag4)
		{
			this.collectBonusButton.SetActive(flag4);
		}
		if (this.questEntries == null || siplayer.CurrentProgression.currentQuestIds == null || siplayer.CurrentProgression.currentQuestProgresses == null)
		{
			return;
		}
		for (int i = 0; i < this.questEntries.Length; i++)
		{
			this.ProcessQuestEntry(this.questEntries[i], siplayer.CurrentProgression.currentQuestIds[i], siplayer.CurrentProgression.currentQuestProgresses[i]);
		}
	}

	public void ProcessQuestEntry(SIUIPlayerQuestEntry entry, int questId, int questProgress)
	{
		if (SIProgression.Instance.questSourceList == null)
		{
			if (entry.questInfo.activeSelf)
			{
				entry.questInfo.SetActive(false);
			}
			if (!entry.noQuestAvailable.activeSelf)
			{
				entry.noQuestAvailable.SetActive(true);
			}
			if (entry.completeOverlay.activeSelf)
			{
				entry.completeOverlay.SetActive(false);
			}
			entry.lastQuestId = -1;
			entry.lastQuestProgress = -1;
			return;
		}
		RotatingQuest questById = SIProgression.Instance.questSourceList.GetQuestById(questId);
		bool flag = questId != -1 && questById != null;
		if (entry.completeOverlay.activeSelf && !flag)
		{
			entry.completeOverlay.SetActive(false);
		}
		if (entry.questInfo.activeSelf != flag)
		{
			entry.questInfo.SetActive(flag);
		}
		if (entry.noQuestAvailable.activeSelf == flag)
		{
			entry.noQuestAvailable.SetActive(!flag);
		}
		if (!flag)
		{
			entry.lastQuestId = -1;
			return;
		}
		if (questId != entry.lastQuestId)
		{
			entry.questDescription.text = questById.GetTextDescription();
		}
		if (entry.lastQuestProgress != questProgress || questId != entry.lastQuestId)
		{
			entry.progress.UpdateFillPercent((float)questProgress / (float)questById.requiredOccurenceCount);
			entry.progress.progressText.text = questProgress.ToString() + "/" + questById.requiredOccurenceCount.ToString();
		}
		if (entry.lastQuestId != -1 && entry.lastQuestId != questById.questID)
		{
			entry.newQuestTag.SetActive(true);
		}
		entry.lastQuestId = questById.questID;
		entry.lastQuestProgress = questProgress;
		bool flag2 = questProgress >= questById.requiredOccurenceCount;
		if (entry.completeOverlay.activeSelf != flag2)
		{
			entry.completeOverlay.SetActive(flag2);
		}
	}

	public void BonusPointCollectButtonPress()
	{
		if (this.activePlayerActorNumber == SIPlayer.LocalPlayer.ActorNr)
		{
			SIProgression.Instance.AttemptRedeemBonusPoint();
		}
	}

	public void QuestPointCollectButtonPress(int questIndex)
	{
		if (this.activePlayerActorNumber == SIPlayer.LocalPlayer.ActorNr && SIPlayer.LocalPlayer.QuestAvailableToClaim(questIndex))
		{
			SIProgression.Instance.AttemptRedeemCompletedQuest(questIndex);
		}
	}

	void IGorillaSliceableSimple.SliceUpdate()
	{
		this.RefreshDisplay();
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public TextMeshProUGUI playerName;

	[FormerlySerializedAs("playerTestPoints")]
	public TextMeshProUGUI playerTechPoints;

	public TextMeshProUGUI stashedQuestCount;

	public TextMeshProUGUI stashedBonusPointCount;

	public Image displayBackground;

	public Image smallDisplayBackground;

	public Image monkeIdolIcon;

	public Color localPlayerColor;

	public Color remotePlayerColor;

	public Color noPlayerColor;

	public SIUIPlayerQuestEntry[] questEntries;

	public GameObject collectBonusButton;

	public GameObject bonusPointsInProgress;

	public GameObject bonusPointsCompleted;

	public SIUIProgressBar sharedProgress;

	public GameObject activePlayer;

	public GameObject waitingForPlayer;

	public int activePlayerActorNumber;

	private string lastNickName;

	private int lastStashedQuests = -1;

	private int lastStashedBonusPoints = -1;

	private int lastTechPoints = -1;

	private int lastBonusProgress = -1;
}
