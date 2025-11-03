using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SIUIPlayerQuestEntry : MonoBehaviour
{
	private void Awake()
	{
		this.lastQuestId = -1;
		this.lastQuestProgress = -1;
	}

	public Image background;

	public SIUIProgressBar progress;

	public TextMeshProUGUI questDescription;

	public GameObject completeOverlay;

	public GameObject questInfo;

	public GameObject noQuestAvailable;

	public GameObject newQuestTag;

	public int lastQuestId;

	public int lastQuestProgress;
}
