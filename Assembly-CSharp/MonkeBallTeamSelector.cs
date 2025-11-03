using System;
using UnityEngine;
using UnityEngine.Events;

public class MonkeBallTeamSelector : MonoBehaviour
{
	public void Awake()
	{
		this._setTeamButton.onPressButton.AddListener(new UnityAction(this.OnSelect));
	}

	public void OnDestroy()
	{
		this._setTeamButton.onPressButton.RemoveListener(new UnityAction(this.OnSelect));
	}

	private void OnSelect()
	{
		MonkeBallGame.Instance.RequestSetTeam(this.teamId);
	}

	public int teamId;

	[SerializeField]
	private GorillaPressableButton _setTeamButton;
}
