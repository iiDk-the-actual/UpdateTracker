using System;
using UnityEngine.Events;

public class GorillaToggleActionButton : GorillaPressableButton
{
	public override void Start()
	{
		this.BindToggleAction();
	}

	private void BindToggleAction()
	{
		if (this.ToggleAction == null || !this.ToggleAction.IsValid)
		{
			return;
		}
		this.ToggleAction.Cache();
		this.onPressButton = new UnityEvent();
		this.onPressButton.AddListener(new UnityAction(this.ExecuteToggleAction));
	}

	private void ExecuteToggleAction()
	{
		ComponentFunctionReference<bool> toggleAction = this.ToggleAction;
		this.isOn = toggleAction != null && toggleAction.Invoke();
		this.UpdateColor();
	}

	public ComponentFunctionReference<bool> ToggleAction;

	private Func<bool> toggleFunc;
}
