using System;
using UnityEngine;
using UnityEngine.Events;

public class TransferrableObjectSyncedBool : TransferrableObject
{
	internal override void OnEnable()
	{
		base.OnEnable();
		this.OnItemStateBoolFalse.AddListener(new UnityAction(this.OnItemStateChanged));
		this.OnItemStateBoolTrue.AddListener(new UnityAction(this.OnItemStateChanged));
	}

	internal override void OnDisable()
	{
		base.OnDisable();
		this.OnItemStateBoolFalse.RemoveListener(new UnityAction(this.OnItemStateChanged));
		this.OnItemStateBoolTrue.RemoveListener(new UnityAction(this.OnItemStateChanged));
	}

	public void SetItemState(bool state)
	{
		base.SetItemStateBool(state);
	}

	public void ToggleItemState()
	{
		base.ToggleNetworkedItemStateBool();
	}

	private void OnItemStateChanged()
	{
		if (this.itemState == TransferrableObject.ItemStates.State0)
		{
			UnityEvent onItemStateSetFalse = this.OnItemStateSetFalse;
			if (onItemStateSetFalse == null)
			{
				return;
			}
			onItemStateSetFalse.Invoke();
			return;
		}
		else
		{
			UnityEvent onItemStateSetTrue = this.OnItemStateSetTrue;
			if (onItemStateSetTrue == null)
			{
				return;
			}
			onItemStateSetTrue.Invoke();
			return;
		}
	}

	[SerializeField]
	private bool deprecatedWarning = true;

	[SerializeField]
	private UnityEvent OnItemStateSetTrue;

	[SerializeField]
	private UnityEvent OnItemStateSetFalse;
}
