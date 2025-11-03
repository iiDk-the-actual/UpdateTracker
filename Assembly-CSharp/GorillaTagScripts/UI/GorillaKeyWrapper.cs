using System;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.UI
{
	public class GorillaKeyWrapper<TBinding> : MonoBehaviour where TBinding : Enum
	{
		public void Start()
		{
			if (!this.defineButtonsManually)
			{
				this.FindMatchingButtons(base.gameObject);
				return;
			}
			if (this.buttons.Count > 0)
			{
				for (int i = this.buttons.Count - 1; i >= 0; i--)
				{
					if (this.buttons[i].IsNull())
					{
						this.buttons.RemoveAt(i);
					}
					else
					{
						this.buttons[i].OnKeyButtonPressed.AddListener(new UnityAction<TBinding>(this.OnKeyButtonPressed));
					}
				}
			}
		}

		public void OnDestroy()
		{
			for (int i = 0; i < this.buttons.Count; i++)
			{
				if (this.buttons[i].IsNotNull())
				{
					this.buttons[i].OnKeyButtonPressed.RemoveListener(new UnityAction<TBinding>(this.OnKeyButtonPressed));
				}
			}
		}

		public void FindMatchingButtons(GameObject obj)
		{
			if (obj.IsNull())
			{
				return;
			}
			for (int i = 0; i < obj.transform.childCount; i++)
			{
				Transform child = obj.transform.GetChild(i);
				if (child.IsNotNull())
				{
					this.FindMatchingButtons(child.gameObject);
				}
			}
			GorillaKeyButton<TBinding> component = obj.GetComponent<GorillaKeyButton<TBinding>>();
			if (component.IsNotNull() && !this.buttons.Contains(component))
			{
				this.buttons.Add(component);
				component.OnKeyButtonPressed.AddListener(new UnityAction<TBinding>(this.OnKeyButtonPressed));
			}
		}

		private void OnKeyButtonPressed(TBinding binding)
		{
			UnityEvent<TBinding> onKeyPressed = this.OnKeyPressed;
			if (onKeyPressed == null)
			{
				return;
			}
			onKeyPressed.Invoke(binding);
		}

		public UnityEvent<TBinding> OnKeyPressed = new UnityEvent<TBinding>();

		public bool defineButtonsManually;

		public List<GorillaKeyButton<TBinding>> buttons = new List<GorillaKeyButton<TBinding>>();
	}
}
