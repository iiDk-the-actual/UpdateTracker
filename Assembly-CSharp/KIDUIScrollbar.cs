using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("UI/KIDUI Scrollbar", 37)]
public class KIDUIScrollbar : Scrollbar, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private XRUIInputModule InputModule
	{
		get
		{
			return EventSystem.current.currentInputModule as XRUIInputModule;
		}
	}

	private KIDUIScrollbar.Axis axis
	{
		get
		{
			if (base.direction != Scrollbar.Direction.LeftToRight && base.direction != Scrollbar.Direction.RightToLeft)
			{
				return KIDUIScrollbar.Axis.Vertical;
			}
			return KIDUIScrollbar.Axis.Horizontal;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.containerRect = base.handleRect.parent.GetComponent<RectTransform>();
		if (GorillaTagger.Instance)
		{
			this.thirdPersonCamera = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>();
		}
		if (ControllerBehaviour.Instance != null)
		{
			ControllerBehaviour.Instance.OnAction += this.PostUpdate;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (ControllerBehaviour.Instance != null)
		{
			ControllerBehaviour.Instance.OnAction -= this.PostUpdate;
		}
		this._isPointerInside = false;
		this._currentPointerData = null;
	}

	private void PostUpdate()
	{
		if (!this._isPointerInside && !ControllerBehaviour.Instance.TriggerDown)
		{
			this._isHolding = false;
			return;
		}
		if (!base.interactable || !ControllerBehaviour.Instance.TriggerDown || this._currentPointerData == null)
		{
			return;
		}
		if (!this._isHolding && this._isPointerInside && ControllerBehaviour.Instance.TriggerDown)
		{
			this._isHolding = true;
		}
		if (!this._isHolding || !this.IsInteractable() || this.InputModule == null)
		{
			return;
		}
		XRRayInteractor xrrayInteractor = this.InputModule.GetInteractor(this._currentPointerData.pointerId) as XRRayInteractor;
		RaycastResult raycastResult;
		if (xrrayInteractor != null && xrrayInteractor.TryGetCurrentUIRaycastResult(out raycastResult))
		{
			Vector2 vector;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(this.containerRect, raycastResult.screenPosition, this.thirdPersonCamera, out vector);
			Vector2 zero = Vector2.zero;
			Vector2 vector2 = vector - zero - this.containerRect.rect.position - (base.handleRect.rect.size - base.handleRect.sizeDelta) * 0.5f;
			float num = ((this.axis == KIDUIScrollbar.Axis.Horizontal) ? this.containerRect.rect.width : this.containerRect.rect.height) * (1f - base.size);
			if (num <= 0f)
			{
				return;
			}
			this.UpdateDrag(vector2, num);
		}
	}

	private void UpdateDrag(Vector2 handleCorner, float remainingSize)
	{
		switch (base.direction)
		{
		case Scrollbar.Direction.LeftToRight:
			base.value = Mathf.Clamp01(handleCorner.x / remainingSize);
			return;
		case Scrollbar.Direction.RightToLeft:
			base.value = Mathf.Clamp01(1f - handleCorner.x / remainingSize);
			return;
		case Scrollbar.Direction.BottomToTop:
			base.value = Mathf.Clamp01(handleCorner.y / remainingSize);
			return;
		case Scrollbar.Direction.TopToBottom:
			base.value = Mathf.Clamp01(1f - handleCorner.y / remainingSize);
			return;
		default:
			return;
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		this._isPointerInside = true;
		this._currentPointerData = eventData;
		if (this.IsInteractable() && this.InputModule != null)
		{
			XRRayInteractor xrrayInteractor = this.InputModule.GetInteractor(eventData.pointerId) as XRRayInteractor;
			if (xrrayInteractor != null)
			{
				xrrayInteractor.xrController.SendHapticImpulse(this._highlightedVibrationStrength, this._highlightedVibrationDuration);
			}
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		this._isPointerInside = false;
	}

	private float _highlightedVibrationStrength = 0.1f;

	private float _highlightedVibrationDuration = 0.1f;

	private RectTransform containerRect;

	private bool _isPointerInside;

	private bool _isHolding;

	private PointerEventData _currentPointerData;

	private Camera thirdPersonCamera;

	private enum Axis
	{
		Horizontal,
		Vertical
	}
}
