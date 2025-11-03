using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MonkeBallResetGame : MonoBehaviourTick
{
	private void Awake()
	{
		this._resetButton.onPressButton.AddListener(new UnityAction(this.OnSelect));
		if (this._resetButton == null)
		{
			this._buttonOrigin = this._resetButton.transform.position;
		}
	}

	public override void Tick()
	{
		if (this._cooldown)
		{
			this._cooldownTimer -= Time.deltaTime;
			if (this._cooldownTimer <= 0f)
			{
				this.ToggleButton(false, -1);
				this._cooldown = false;
			}
		}
	}

	public void ToggleReset(bool toggle, int teamId, bool force = false)
	{
		if (teamId < -1 || teamId >= this.teamMaterials.Length)
		{
			return;
		}
		if (toggle)
		{
			this.ToggleButton(true, teamId);
			this._cooldown = false;
			return;
		}
		if (force)
		{
			this.ToggleButton(false, -1);
			return;
		}
		this._cooldown = true;
		this._cooldownTimer = 3f;
	}

	private void ToggleButton(bool toggle, int teamId)
	{
		this._resetButton.enabled = toggle;
		this.allowedTeamId = teamId;
		if (!toggle || teamId == -1)
		{
			this.button.sharedMaterial = this.neutralMaterial;
			return;
		}
		this.button.sharedMaterial = this.teamMaterials[teamId];
	}

	private void OnSelect()
	{
		MonkeBallGame.Instance.RequestResetGame();
	}

	[SerializeField]
	private GorillaPressableButton _resetButton;

	public Renderer button;

	public Vector3 buttonPressOffset;

	private Vector3 _buttonOrigin = Vector3.zero;

	[Space]
	public Material[] teamMaterials;

	public Material neutralMaterial;

	public int allowedTeamId = -1;

	[SerializeField]
	private TextMeshPro _resetLabel;

	private bool _cooldown;

	private float _cooldownTimer;
}
