using System;
using TMPro;
using UnityEngine;

public class QuestDisplay : MonoBehaviour
{
	public bool IsChanged
	{
		get
		{
			return this.quest.lastChange > this._lastUpdate;
		}
	}

	public void UpdateDisplay()
	{
		this.text.text = this.quest.GetTextDescription();
		if (this.quest.isQuestComplete)
		{
			this.progressDisplay.SetVisible(false);
		}
		else if (this.quest.requiredOccurenceCount > 1)
		{
			this.progressDisplay.SetProgress(this.quest.occurenceCount, this.quest.requiredOccurenceCount);
			this.progressDisplay.SetVisible(true);
		}
		else
		{
			this.progressDisplay.SetVisible(false);
		}
		this.UpdateCompletionIndicator();
		this._lastUpdate = Time.frameCount;
	}

	private void UpdateCompletionIndicator()
	{
		bool isQuestComplete = this.quest.isQuestComplete;
		bool flag = !isQuestComplete && this.quest.requiredOccurenceCount == 1;
		this.dailyIncompleteIndicator.SetActive(this.quest.isDailyQuest && flag);
		this.dailyCompleteIndicator.SetActive(this.quest.isDailyQuest && isQuestComplete);
		this.weeklyIncompleteIndicator.SetActive(!this.quest.isDailyQuest && flag);
		this.weeklyCompleteIndicator.SetActive(!this.quest.isDailyQuest && isQuestComplete);
	}

	[SerializeField]
	private ProgressDisplay progressDisplay;

	[SerializeField]
	private TMP_Text text;

	[SerializeField]
	private TMP_Text statusText;

	[SerializeField]
	private GameObject dailyIncompleteIndicator;

	[SerializeField]
	private GameObject dailyCompleteIndicator;

	[SerializeField]
	private GameObject weeklyIncompleteIndicator;

	[SerializeField]
	private GameObject weeklyCompleteIndicator;

	[NonSerialized]
	public RotatingQuest quest;

	private int _lastUpdate = -1;
}
