using System;
using UnityEngine;

public class ArcadeMachineButton : GorillaPressableButton
{
	public event ArcadeMachineButton.ArcadeMachineButtonEvent OnStateChange;

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		if (!this.state)
		{
			this.state = true;
			if (this.OnStateChange != null)
			{
				this.OnStateChange(this.ButtonID, this.state);
			}
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (!base.enabled || !this.state)
		{
			return;
		}
		if (collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
		{
			return;
		}
		this.state = false;
		if (this.OnStateChange != null)
		{
			this.OnStateChange(this.ButtonID, this.state);
		}
	}

	private bool state;

	[SerializeField]
	private int ButtonID;

	public delegate void ArcadeMachineButtonEvent(int id, bool state);
}
