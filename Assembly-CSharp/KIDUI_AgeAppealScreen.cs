using System;
using System.Threading;
using UnityEngine;

public class KIDUI_AgeAppealScreen : MonoBehaviour
{
	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	public void OnDisable()
	{
		KIDAudioManager instance = KIDAudioManager.Instance;
		if (instance == null)
		{
			return;
		}
		instance.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
	}

	public void ShowRestrictedAccessScreen()
	{
		base.gameObject.SetActive(true);
	}

	public void OnChangeAgePressed()
	{
		base.gameObject.SetActive(false);
	}

	[SerializeField]
	private KIDUIButton _changeAgeButton;

	[SerializeField]
	private int _minimumDelay = 1000;

	private string _submittedEmailAddress;

	private CancellationTokenSource _cancellationTokenSource;
}
