using System;
using System.Collections;
using System.Runtime.CompilerServices;
using GorillaTag;
using GorillaTagScripts.VirtualStumpCustomMaps.UI;
using UnityEngine;

public abstract class CustomMapsScreenTouchPoint : MonoBehaviour
{
	protected virtual void Awake()
	{
	}

	protected virtual void OnDisable()
	{
		if (this.colorUpdateCoroutine != null)
		{
			base.StopCoroutine(this.colorUpdateCoroutine);
		}
		if (this.buttonColorSettings != null)
		{
			this.touchPointRenderer.color = this.buttonColorSettings.UnpressedColor;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		GTDev.Log<string>(string.Format("trigger {0} pressTime={1} time={2}", base.gameObject.name, CustomMapsScreenTouchPoint.pressTime, Time.time), null);
		if (Time.time < CustomMapsScreenTouchPoint.pressTime + CustomMapsScreenTouchPoint.pressedTime)
		{
			return;
		}
		if (collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null)
		{
			Vector3 vector = this.GetForwardDirection();
			if (Vector3.Dot((collider.transform.position - base.transform.position).normalized, vector) < 0f)
			{
				return;
			}
			GTDev.Log<string>(string.Format("trigger {0} collider {1} postion {2}", base.gameObject.name, collider.gameObject.name, collider.transform.position), null);
			GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
			CustomMapsScreenTouchPoint.pressTime = Time.time;
			this.OnButtonPressedEvent();
			this.PressButtonColourUpdate();
			if (this.screen != null)
			{
				this.screen.PressButton(this.keyBinding);
			}
			if (component != null)
			{
				GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
			}
		}
	}

	public virtual void PressButtonColourUpdate()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		this.touchPointRenderer.color = this.buttonColorSettings.PressedColor;
		this.colorUpdateCoroutine = base.StartCoroutine(this.<PressButtonColourUpdate>g__ButtonColorUpdate_Local|12_0());
	}

	private Vector3 GetForwardDirection()
	{
		switch (this.forwardDirection)
		{
		case CustomMapsScreenTouchPoint.TouchPointDirections.Forward:
			return base.transform.forward;
		case CustomMapsScreenTouchPoint.TouchPointDirections.Backward:
			return -base.transform.forward;
		case CustomMapsScreenTouchPoint.TouchPointDirections.Left:
			return -base.transform.right;
		case CustomMapsScreenTouchPoint.TouchPointDirections.Right:
			return base.transform.right;
		case CustomMapsScreenTouchPoint.TouchPointDirections.Up:
			return base.transform.up;
		case CustomMapsScreenTouchPoint.TouchPointDirections.Down:
			return -base.transform.up;
		default:
			return base.transform.forward;
		}
	}

	protected abstract void OnButtonPressedEvent();

	[CompilerGenerated]
	private IEnumerator <PressButtonColourUpdate>g__ButtonColorUpdate_Local|12_0()
	{
		yield return new WaitForSeconds(CustomMapsScreenTouchPoint.pressedTime);
		if (CustomMapsScreenTouchPoint.pressTime != 0f && Time.time > CustomMapsScreenTouchPoint.pressedTime + CustomMapsScreenTouchPoint.pressTime)
		{
			this.touchPointRenderer.color = this.buttonColorSettings.UnpressedColor;
			CustomMapsScreenTouchPoint.pressTime = 0f;
		}
		yield break;
	}

	[SerializeField]
	private CustomMapsTerminalScreen screen;

	[SerializeField]
	private CustomMapKeyboardBinding keyBinding;

	[SerializeField]
	private CustomMapsScreenTouchPoint.TouchPointDirections forwardDirection;

	[SerializeField]
	protected SpriteRenderer touchPointRenderer;

	[SerializeField]
	protected ButtonColorSettings buttonColorSettings;

	private static float pressedTime = 0.25f;

	protected static float pressTime;

	private Coroutine colorUpdateCoroutine;

	public enum TouchPointDirections
	{
		Forward,
		Backward,
		Left,
		Right,
		Up,
		Down
	}
}
