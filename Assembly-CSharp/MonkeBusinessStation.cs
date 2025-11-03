using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GameObjectScheduling;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonkeBusinessStation : MonoBehaviourPunCallbacks
{
	public override void OnEnable()
	{
		base.OnEnable();
		this.FindQuestManager();
		ProgressionController.OnQuestSelectionChanged += this.OnQuestSelectionChanged;
		ProgressionController.OnProgressEvent += this.OnProgress;
		ProgressionController.RequestProgressUpdate();
		this.UpdateCountdownTimers();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		ProgressionController.OnQuestSelectionChanged -= this.OnQuestSelectionChanged;
		ProgressionController.OnProgressEvent -= this.OnProgress;
	}

	private void FindQuestManager()
	{
		if (!this._questManager)
		{
			this._questManager = Object.FindAnyObjectByType<RotatingQuestsManager>();
		}
	}

	private void UpdateCountdownTimers()
	{
		this._dailyCountdown.SetCountdownTime(this._questManager.DailyQuestCountdown);
		this._weeklyCountdown.SetCountdownTime(this._questManager.WeeklyQuestCountdown);
	}

	private void OnQuestSelectionChanged()
	{
		this.UpdateCountdownTimers();
	}

	private void OnProgress()
	{
		this.UpdateQuestStatus();
		this.UpdateProgressDisplays();
	}

	private void UpdateProgressDisplays()
	{
		ValueTuple<int, int, int> progressionData = ProgressionController.GetProgressionData();
		int item = progressionData.Item1;
		int item2 = progressionData.Item2;
		this._weeklyProgress.SetProgress(item, ProgressionController.WeeklyCap);
		if (!this._isUpdatingPointCount)
		{
			this._unclaimedPoints.text = item2.ToString();
			this._claimButton.isOn = item2 > 0;
		}
		bool flag = item2 > 0;
		this._claimablePointsObject.SetActive(flag);
		this._noClaimablePointsObject.SetActive(!flag);
		this._badgeMount.position = (flag ? this._claimablePointsBadgePosition.position : this._noClaimablePointsBadgePosition.position);
		this._claimButton.gameObject.SetActive(flag);
	}

	private void UpdateQuestStatus()
	{
		if (this._lastQuestChange >= RotatingQuestsManager.LastQuestChange)
		{
			return;
		}
		this.FindQuestManager();
		if (this._quests.Count == 0 || this._lastQuestDailyID != RotatingQuestsManager.LastQuestDailyID)
		{
			this.BuildQuestList();
		}
		foreach (QuestDisplay questDisplay in this._quests)
		{
			if (questDisplay.IsChanged)
			{
				questDisplay.UpdateDisplay();
			}
		}
		this._lastQuestChange = Time.frameCount;
		this._lastQuestDailyID = RotatingQuestsManager.LastQuestDailyID;
	}

	public void RedeemProgress()
	{
		if (this._claimButton.isOn)
		{
			this._isUpdatingPointCount = true;
			ValueTuple<int, int, int> progressionData = ProgressionController.GetProgressionData();
			int item = progressionData.Item2;
			int item2 = progressionData.Item3;
			this._tempUnclaimedPoints = item;
			this._tempTotalPoints = item2;
			this._claimButton.isOn = false;
			ProgressionController.RedeemProgress();
			if (PhotonNetwork.InRoom)
			{
				base.photonView.RPC("BroadcastRedeemQuestPoints", RpcTarget.Others, new object[] { this._tempUnclaimedPoints });
			}
			base.StartCoroutine(this.PerformPointRedemptionSequence());
		}
	}

	private IEnumerator PerformPointRedemptionSequence()
	{
		while (this._tempUnclaimedPoints > 0)
		{
			this._tempUnclaimedPoints--;
			this._tempTotalPoints++;
			this._unclaimedPoints.text = this._tempUnclaimedPoints.ToString();
			if (this._tempUnclaimedPoints == 0)
			{
				this._audioSource.PlayOneShot(this._claimPointFinalSFX);
			}
			else
			{
				this._audioSource.PlayOneShot(this._claimPointDefaultSFX);
			}
			yield return new WaitForSeconds(this._claimDelayPerPoint);
		}
		this._isUpdatingPointCount = false;
		this.UpdateProgressDisplays();
		yield break;
	}

	[PunRPC]
	private void BroadcastRedeemQuestPoints(int redeemedPointCount, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "BroadcastRedeemQuestPoints");
		RigContainer rigContainer;
		if (new PhotonMessageInfoWrapped(info).Sender != null && VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
		{
			if (!FXSystem.CheckCallSpam(rigContainer.Rig.fxSettings, 10, (double)Time.unscaledTime))
			{
				return;
			}
			redeemedPointCount = Mathf.Min(redeemedPointCount, 50);
			Coroutine coroutine;
			if (this.perPlayerRedemptionSequence.TryGetValue(info.Sender, out coroutine))
			{
				if (coroutine != null)
				{
					base.StopCoroutine(coroutine);
				}
				this.perPlayerRedemptionSequence.Remove(info.Sender);
			}
			if (base.gameObject.activeInHierarchy)
			{
				Coroutine coroutine2 = base.StartCoroutine(this.PerformRemotePointRedemptionSequence(info.Sender, redeemedPointCount));
				this.perPlayerRedemptionSequence.Add(info.Sender, coroutine2);
			}
		}
	}

	private IEnumerator PerformRemotePointRedemptionSequence(NetPlayer player, int redeemedPointCount)
	{
		while (redeemedPointCount > 0)
		{
			int num = redeemedPointCount;
			redeemedPointCount = num - 1;
			if (redeemedPointCount == 0)
			{
				this._audioSource.PlayOneShot(this._claimPointFinalSFX);
			}
			else
			{
				this._audioSource.PlayOneShot(this._claimPointDefaultSFX);
			}
			yield return new WaitForSeconds(this._claimDelayPerPoint);
		}
		this.perPlayerRedemptionSequence.Remove(player);
		yield break;
	}

	private void BuildQuestList()
	{
		this.DestroyQuestList();
		RotatingQuestsManager.RotatingQuestList quests = this._questManager.quests;
		foreach (RotatingQuestsManager.RotatingQuestGroup rotatingQuestGroup in quests.DailyQuests)
		{
			foreach (RotatingQuest rotatingQuest in rotatingQuestGroup.quests)
			{
				if (rotatingQuest.isQuestActive)
				{
					QuestDisplay questDisplay = Object.Instantiate<QuestDisplay>(this._questDisplayPrefab, this._dailyQuestContainer);
					questDisplay.quest = rotatingQuest;
					this._quests.Add(questDisplay);
				}
			}
		}
		foreach (RotatingQuestsManager.RotatingQuestGroup rotatingQuestGroup2 in quests.WeeklyQuests)
		{
			foreach (RotatingQuest rotatingQuest2 in rotatingQuestGroup2.quests)
			{
				if (rotatingQuest2.isQuestActive)
				{
					QuestDisplay questDisplay2 = Object.Instantiate<QuestDisplay>(this._questDisplayPrefab, this._weeklyQuestContainer);
					questDisplay2.quest = rotatingQuest2;
					this._quests.Add(questDisplay2);
				}
			}
		}
		foreach (QuestDisplay questDisplay3 in this._quests)
		{
			questDisplay3.UpdateDisplay();
		}
		if (!this._hasBuiltQuestList)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(this._questContainerParent);
			this._hasBuiltQuestList = true;
			return;
		}
		LayoutRebuilder.MarkLayoutForRebuild(this._questContainerParent);
	}

	private void DestroyQuestList()
	{
		MonkeBusinessStation.<DestroyQuestList>g__DestroyChildren|40_0(this._dailyQuestContainer);
		MonkeBusinessStation.<DestroyQuestList>g__DestroyChildren|40_0(this._weeklyQuestContainer);
		this._quests.Clear();
	}

	[CompilerGenerated]
	internal static void <DestroyQuestList>g__DestroyChildren|40_0(Transform parent)
	{
		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			Object.Destroy(parent.GetChild(i).gameObject);
		}
	}

	[SerializeField]
	private RectTransform _questContainerParent;

	[SerializeField]
	private RectTransform _dailyQuestContainer;

	[SerializeField]
	private RectTransform _weeklyQuestContainer;

	[SerializeField]
	private QuestDisplay _questDisplayPrefab;

	[SerializeField]
	private List<QuestDisplay> _quests;

	[SerializeField]
	private ProgressDisplay _weeklyProgress;

	[SerializeField]
	private TMP_Text _unclaimedPoints;

	[SerializeField]
	private GorillaPressableButton _claimButton;

	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private GameObject _claimablePointsObject;

	[SerializeField]
	private GameObject _noClaimablePointsObject;

	[SerializeField]
	private Transform _claimablePointsBadgePosition;

	[SerializeField]
	private Transform _noClaimablePointsBadgePosition;

	[SerializeField]
	private Transform _badgeMount;

	[Space]
	[SerializeField]
	private float _claimDelayPerPoint = 0.12f;

	[SerializeField]
	private AudioClip _claimPointDefaultSFX;

	[SerializeField]
	private AudioClip _claimPointFinalSFX;

	[Header("Quest Timers")]
	[SerializeField]
	private CountdownText _dailyCountdown;

	[SerializeField]
	private CountdownText _weeklyCountdown;

	private RotatingQuestsManager _questManager;

	private int _lastQuestChange = -1;

	private int _lastQuestDailyID = -1;

	private bool _isUpdatingPointCount;

	private int _tempUnclaimedPoints;

	private int _tempTotalPoints;

	private bool _hasBuiltQuestList;

	private Dictionary<NetPlayer, Coroutine> perPlayerRedemptionSequence = new Dictionary<NetPlayer, Coroutine>();
}
