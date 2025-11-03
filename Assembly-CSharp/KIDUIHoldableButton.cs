using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KIDUIHoldableButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	public KIDUIHoldableButton.ButtonHoldCompleteEvent onHoldComplete
	{
		get
		{
			return this.m_OnHoldComplete;
		}
		set
		{
			this.m_OnHoldComplete = value;
		}
	}

	public float HoldPercentage
	{
		get
		{
			return this._elapsedTime / this._holdDuration;
		}
	}

	private void OnEnable()
	{
		this._holdProgressFill.rectTransform.localScale = new Vector3(0f, 1f, 1f);
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction += this.PostUpdate;
		}
	}

	private void Update()
	{
		this.ManageButtonInteraction(false);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		this._isHoldingMouse = true;
		this.ToggleHoldingButton(true);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		this._isHoldingMouse = false;
		this.ManageButtonInteraction(true);
		this.ToggleHoldingButton(false);
	}

	private void ToggleHoldingButton(bool isPointerDown)
	{
		this._isHoldingButton = isPointerDown && this._button.interactable;
		this._holdProgressFill.rectTransform.localScale = new Vector3(0f, 1f, 1f);
		if (isPointerDown)
		{
			this._elapsedTime = 0f;
			KIDUIHoldableButton.ButtonHoldStartEvent onHoldStart = this.m_OnHoldStart;
			if (onHoldStart != null)
			{
				onHoldStart.Invoke();
			}
			KIDAudioManager.Instance.StartButtonHeldSound();
			return;
		}
		KIDUIHoldableButton.ButtonHoldReleaseEvent onHoldRelease = this.m_OnHoldRelease;
		if (onHoldRelease != null)
		{
			onHoldRelease.Invoke();
		}
		KIDAudioManager.Instance.StopButtonHeldSound();
	}

	private void ManageButtonInteraction(bool isPointerUp = false)
	{
		if (!this._isHoldingButton)
		{
			return;
		}
		if (isPointerUp)
		{
			return;
		}
		if (this._holdDuration <= 0f)
		{
			this.HoldComplete();
			return;
		}
		this._elapsedTime += Time.deltaTime;
		bool flag = this._elapsedTime > this._holdDuration;
		float num = this._elapsedTime / this._holdDuration;
		this._holdProgressFill.rectTransform.localScale = new Vector3(num, 1f, 1f);
		HandRayController.Instance.PulseActiveHandray(num, 0.1f);
		if (flag)
		{
			this.HoldComplete();
		}
	}

	private void HoldComplete()
	{
		this.ToggleHoldingButton(false);
		KIDUIHoldableButton.ButtonHoldCompleteEvent onHoldComplete = this.m_OnHoldComplete;
		if (onHoldComplete != null)
		{
			onHoldComplete.Invoke();
		}
		Debug.Log("[HOLD_BUTTON " + base.name + " ]: Hold Complete");
		this.ResetButton();
	}

	private void ResetButton()
	{
		this._elapsedTime = 0f;
		this.inside = false;
		KIDUIHoldableButton._triggeredThisFrame = false;
		this._button.ResetButton();
	}

	protected void Awake()
	{
		if (this._button != null)
		{
			return;
		}
		this._button = base.GetComponentInChildren<KIDUIButton>();
		if (this._button == null)
		{
			Debug.LogError("[KID::UI_BUTTON] Could not find [KIDUIButton] in children, trying to create a new one.");
			return;
		}
	}

	private void PostUpdate()
	{
		if (!KIDUIHoldableButton._canTrigger)
		{
			KIDUIHoldableButton._canTrigger = !ControllerBehaviour.Instance.TriggerDown;
		}
		if (!this._button.interactable || !KIDUIHoldableButton._canTrigger)
		{
			return;
		}
		if (ControllerBehaviour.Instance)
		{
			if (ControllerBehaviour.Instance.TriggerDown && this.inside)
			{
				if (!this._isHoldingButton)
				{
					string text = string.Concat(new string[]
					{
						"[",
						base.transform.parent.parent.parent.name,
						".",
						base.transform.parent.parent.name,
						".",
						base.transform.parent.name,
						".",
						base.transform.name,
						"]"
					});
					Debug.Log(string.Concat(new string[]
					{
						"[KID::UIBUTTON::DEBUG] ",
						text,
						" - STEAM - OnClick is pressed. Time: [",
						Time.time.ToString(),
						"]"
					}), this);
					this.ToggleHoldingButton(true);
					KIDUIHoldableButton._triggeredThisFrame = true;
					KIDUIHoldableButton._canTrigger = false;
					return;
				}
			}
			else if (this._isHoldingButton && !this._isHoldingMouse)
			{
				this.ToggleHoldingButton(false);
			}
		}
	}

	private void LateUpdate()
	{
		if (KIDUIHoldableButton._triggeredThisFrame)
		{
			string text = string.Concat(new string[]
			{
				"[",
				base.transform.parent.parent.parent.name,
				".",
				base.transform.parent.parent.name,
				".",
				base.transform.parent.name,
				".",
				base.transform.name,
				"]"
			});
			Debug.Log(string.Concat(new string[]
			{
				"[KID::UIBUTTON::DEBUG] ",
				text,
				" - STEAM - OnLateUpdate triggered and Triggered Frame Reset. Time: [",
				Time.time.ToString(),
				"]"
			}), this);
		}
		KIDUIHoldableButton._triggeredThisFrame = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this.inside = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.inside = false;
	}

	protected void OnDisable()
	{
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction -= this.PostUpdate;
		}
		this.inside = false;
	}

	public KIDUIButton _button;

	[SerializeField]
	private float _holdDuration;

	[SerializeField]
	private Image _holdProgressFill;

	[Header("Steam Settings")]
	[SerializeField]
	private UXSettings _cbUXSettings;

	[SerializeField]
	private KIDUIHoldableButton.ButtonHoldCompleteEvent m_OnHoldComplete = new KIDUIHoldableButton.ButtonHoldCompleteEvent();

	[SerializeField]
	private KIDUIHoldableButton.ButtonHoldStartEvent m_OnHoldStart = new KIDUIHoldableButton.ButtonHoldStartEvent();

	[SerializeField]
	private KIDUIHoldableButton.ButtonHoldReleaseEvent m_OnHoldRelease = new KIDUIHoldableButton.ButtonHoldReleaseEvent();

	private bool _isHoldingButton;

	private float _elapsedTime;

	private bool inside;

	private bool _isHoldingMouse;

	private static bool _triggeredThisFrame = false;

	private static bool _canTrigger = true;

	[Serializable]
	public class ButtonHoldCompleteEvent : UnityEvent
	{
	}

	[Serializable]
	public class ButtonHoldStartEvent : UnityEvent
	{
	}

	[Serializable]
	public class ButtonHoldReleaseEvent : UnityEvent
	{
	}
}
