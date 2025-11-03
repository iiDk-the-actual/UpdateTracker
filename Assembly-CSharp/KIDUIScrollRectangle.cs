using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("UI/KIDUI Scroll Rect", 37)]
public class KIDUIScrollRectangle : ScrollRect, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private XRUIInputModule InputModule
	{
		get
		{
			return EventSystem.current.currentInputModule as XRUIInputModule;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.thirdPersonCamera = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>();
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
		if (this._currentPointerData == null || this.InputModule == null)
		{
			return;
		}
		if (this._currentPointerData.hovered.Contains(base.viewport.gameObject) && !this._currentPointerData.hovered.Contains(base.verticalScrollbar.gameObject))
		{
			this._isPointerInside = true;
		}
		else
		{
			this._isPointerInside = false;
		}
		if (!ControllerBehaviour.Instance.TriggerDown)
		{
			this._isHolding = false;
			return;
		}
		XRRayInteractor xrrayInteractor = this.InputModule.GetInteractor(this._currentPointerData.pointerId) as XRRayInteractor;
		if (xrrayInteractor == null)
		{
			return;
		}
		XRRayInteractor xrrayInteractor2 = xrrayInteractor;
		RaycastResult raycastResult;
		if (!xrrayInteractor2.TryGetCurrentUIRaycastResult(out raycastResult))
		{
			return;
		}
		Vector2 vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.viewRect, raycastResult.screenPosition, this.thirdPersonCamera, out vector);
		if (!this._isHolding && this._isPointerInside && ControllerBehaviour.Instance.TriggerDown)
		{
			this._isHolding = true;
			this.m_PointerStartLocalCursor = vector;
			this.m_ContentStartPosition = base.content.anchoredPosition;
		}
		if (!this._isHolding)
		{
			return;
		}
		base.UpdateBounds();
		Vector2 vector2 = vector - this.m_PointerStartLocalCursor;
		Vector2 vector3 = this.m_ContentStartPosition + vector2;
		this.SetContentAnchoredPosition(vector3);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (eventData.hovered.Contains(base.viewport.gameObject))
		{
			this._isPointerInside = true;
			this._currentPointerData = eventData;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this._isPointerInside = false;
	}

	private bool _isPointerInside;

	private bool _isHolding;

	private PointerEventData _currentPointerData;

	private Vector2 m_PointerStartLocalCursor = Vector2.zero;

	private Camera thirdPersonCamera;
}
