using System;
using GorillaTagScripts.VirtualStumpCustomMaps.UI;
using UnityEngine;
using UnityEngine.Events;

public abstract class CustomMapsTerminalScreen : MonoBehaviour
{
	public abstract void Initialize();

	public virtual void Show()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
			CustomMapsKeyboard customMapsKeyboard = this.terminalKeyboard;
			if (customMapsKeyboard != null)
			{
				customMapsKeyboard.OnKeyPressed.AddListener(new UnityAction<CustomMapKeyboardBinding>(this.PressButton));
			}
		}
		this.showTime = Time.time;
	}

	public virtual void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(false);
			CustomMapsKeyboard customMapsKeyboard = this.terminalKeyboard;
			if (customMapsKeyboard != null)
			{
				customMapsKeyboard.OnKeyPressed.RemoveListener(new UnityAction<CustomMapKeyboardBinding>(this.PressButton));
			}
		}
		this.showTime = 0f;
	}

	public virtual void PressButton(CustomMapKeyboardBinding pressedButton)
	{
	}

	public CustomMapsKeyboard terminalKeyboard;

	[SerializeField]
	protected float activationTime = 0.25f;

	protected float showTime;
}
