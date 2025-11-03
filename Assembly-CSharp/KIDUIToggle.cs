using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KIDUIToggle : Slider
{
	public bool CurrentValue { get; private set; }

	public bool IsOn
	{
		get
		{
			return this.CurrentValue;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this.SetupToggleComponent();
	}

	protected override void Start()
	{
		base.Start();
		base.interactable = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.interactable = false;
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction += this.PostUpdate;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		this.Toggle();
	}

	public override void OnPointerEnter(PointerEventData pointerEventData)
	{
		this.SetHighlighted();
		this.inside = true;
	}

	public override void OnPointerExit(PointerEventData pointerEventData)
	{
		this.SetNormal();
		this.inside = false;
	}

	protected virtual void SetupToggleComponent()
	{
		this.SetupSliderComponent();
		base.handleRect.anchorMin = new Vector2(0f, 0.5f);
		base.handleRect.anchorMax = new Vector3(0f, 0.5f);
		base.handleRect.pivot = new Vector2(0f, 0.5f);
		base.handleRect.sizeDelta = new Vector2(base.handleRect.sizeDelta.x, base.handleRect.sizeDelta.x);
	}

	protected virtual void SetupSliderComponent()
	{
		base.interactable = false;
		base.colors.disabledColor = Color.white;
		this.SetColors();
		base.transition = Selectable.Transition.None;
	}

	public void RegisterOnChangeEvent(Action onChange)
	{
		this._onToggleChanged.AddListener(delegate
		{
			Action onChange2 = onChange;
			if (onChange2 == null)
			{
				return;
			}
			onChange2();
		});
	}

	public void UnregisterOnChangeEvent(Action onChange)
	{
		this._onToggleChanged.RemoveListener(delegate
		{
			Action onChange2 = onChange;
			if (onChange2 == null)
			{
				return;
			}
			onChange2();
		});
	}

	public void RegisterToggleOnEvent(Action onToggle)
	{
		this._onToggleOn.AddListener(delegate
		{
			Action onToggle2 = onToggle;
			if (onToggle2 == null)
			{
				return;
			}
			onToggle2();
		});
	}

	public void UnregisterToggleOnEvent(Action onToggle)
	{
		this._onToggleOn.RemoveListener(delegate
		{
			Action onToggle2 = onToggle;
			if (onToggle2 == null)
			{
				return;
			}
			onToggle2();
		});
	}

	public void RegisterToggleOffEvent(Action onToggle)
	{
		this._onToggleOff.AddListener(delegate
		{
			Action onToggle2 = onToggle;
			if (onToggle2 == null)
			{
				return;
			}
			onToggle2();
		});
	}

	public void UnregisterToggleOffEvent(Action onToggle)
	{
		this._onToggleOff.RemoveListener(delegate
		{
			Action onToggle2 = onToggle;
			if (onToggle2 == null)
			{
				return;
			}
			onToggle2();
		});
	}

	private void SetColors()
	{
		base.colors = this._fillColors;
	}

	private void Toggle()
	{
		if (this._isDisabled)
		{
			return;
		}
		this.SetStateAndStartAnimation(!this.CurrentValue, false);
	}

	public void SetValue(bool newValue)
	{
		if (newValue == this.CurrentValue)
		{
			return;
		}
		this.SetStateAndStartAnimation(newValue, false);
	}

	private void SetStateAndStartAnimation(bool state, bool skipAnim = false)
	{
		if (this.CurrentValue == state)
		{
			Debug.Log("IS SAME STATE, WILL NOT CHANGE");
			return;
		}
		this.CurrentValue = state;
		UnityEvent onToggleChanged = this._onToggleChanged;
		if (onToggleChanged != null)
		{
			onToggleChanged.Invoke();
		}
		if (this.CurrentValue)
		{
			UnityEvent onToggleOn = this._onToggleOn;
			if (onToggleOn != null)
			{
				onToggleOn.Invoke();
			}
			KIDAudioManager.Instance.PlaySound(KIDAudioManager.KIDSoundType.Success);
		}
		else
		{
			UnityEvent onToggleOff = this._onToggleOff;
			if (onToggleOff != null)
			{
				onToggleOff.Invoke();
			}
			KIDAudioManager.Instance.PlaySound(KIDAudioManager.KIDSoundType.TurnOffPermission);
		}
		if (this._animationCoroutine != null)
		{
			base.StopCoroutine(this._animationCoroutine);
		}
		this._handleUnlockIcon.gameObject.SetActive(this.CurrentValue);
		this._handleLockIcon.gameObject.SetActive(!this.CurrentValue);
		if (this._animationDuration == 0f || skipAnim)
		{
			Debug.Log("[KID::UI::SetStateAndStartAnimation] Skipping animation. Setting value to " + (this.CurrentValue ? "1f" : "0f"));
			this.value = (this.CurrentValue ? 1f : 0f);
			return;
		}
		this._animationCoroutine = base.StartCoroutine(this.AnimateSlider());
	}

	private IEnumerator AnimateSlider()
	{
		Debug.Log(string.Format("[KID::UI::TOGGLE] Toggle: [{0}] is {1}", base.name, this.CurrentValue));
		float startValue = (this.CurrentValue ? 0f : 1f);
		float endValue = (this.CurrentValue ? 1f : 0f);
		Debug.Log(string.Format("[KID::UI::TOGGLE] Toggle: [{0}] Start: {1}, End: {2}, Value: {3}", new object[] { base.name, startValue, endValue, this.value }));
		float time = 0f;
		while (time < this._animationDuration)
		{
			time += Time.deltaTime;
			float num = this._toggleEase.Evaluate(time / this._animationDuration);
			this.value = Mathf.Lerp(startValue, endValue, num);
			yield return null;
		}
		this.value = endValue;
		yield break;
	}

	private void PostUpdate()
	{
		if (!this.inside)
		{
			return;
		}
		if (ControllerBehaviour.Instance)
		{
			if (ControllerBehaviour.Instance.TriggerDown && KIDUIToggle._canTrigger)
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
				this.Toggle();
				KIDUIToggle._triggeredThisFrame = true;
				KIDUIToggle._canTrigger = false;
				return;
			}
			if (!ControllerBehaviour.Instance.TriggerDown)
			{
				KIDUIToggle._canTrigger = true;
			}
		}
	}

	private void LateUpdate()
	{
		if (KIDUIToggle._triggeredThisFrame)
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
		KIDUIToggle._triggeredThisFrame = false;
	}

	protected new void OnDisable()
	{
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction -= this.PostUpdate;
		}
		this.inside = false;
	}

	private void SetDisabled(bool isLockedButEnabled)
	{
		this.SetSwitchColors(this._borderColors.disabledColor, this._handleColors.disabledColor, this._fillColors.disabledColor);
		this.SetBorderSize(this._disabledBorderSize);
		this.SetBackgroundActive(false);
	}

	private void SetNormal()
	{
		if (this._isDisabled)
		{
			return;
		}
		this.SetSwitchColors(this._borderColors.normalColor, this._handleColors.normalColor, this._fillColors.normalColor);
		this.SetBorderSize(this._normalBorderSize);
		this.SetBackgroundActive(false);
	}

	private void SetSelected()
	{
		if (this._isDisabled)
		{
			return;
		}
		this.SetSwitchColors(this._borderColors.selectedColor, this._handleColors.selectedColor, this._fillColors.selectedColor);
		this.SetBorderSize(this._selectedBorderSize);
		this.SetBackgroundActive(true);
	}

	private void SetHighlighted()
	{
		if (this._isDisabled)
		{
			return;
		}
		this.SetSwitchColors(this._borderColors.highlightedColor, this._handleColors.highlightedColor, this._fillColors.highlightedColor);
		this.SetBorderSize(this._highlightedBorderSize);
		this.SetBackgroundActive(true);
	}

	private void SetPressed()
	{
		if (this._isDisabled)
		{
			return;
		}
		this.SetSwitchColors(this._borderColors.pressedColor, this._handleColors.pressedColor, this._fillColors.pressedColor);
		this.SetBorderSize(this._pressedBorderSize);
		this.SetBackgroundActive(true);
	}

	private void SetSwitchColors(Color borderColor, Color handleColor, Color fillColor)
	{
		this._borderImg.color = borderColor;
		this._handleImg.color = handleColor;
	}

	private void SetBorderSize(float borderScale)
	{
		this._borderImgRef.offsetMin = new Vector2(-borderScale, -borderScale * this._borderHeightRatio);
		this._borderImgRef.offsetMax = new Vector2(borderScale, borderScale * this._borderHeightRatio);
	}

	private void SetBackgroundActive(bool isActive)
	{
		this._fillImg.gameObject.SetActive(isActive);
		this._fillInactiveImg.gameObject.SetActive(!isActive);
		this.SetBackgroundLocksActive(isActive);
	}

	private void SetBackgroundLocksActive(bool isActive)
	{
		Color color = (isActive ? this._lockActiveColor : this._lockInactiveColor);
		this._lockIcon.color = color;
		this._unlockIcon.color = color;
	}

	[Header("Toggle Setup")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _initValue;

	[SerializeField]
	private Image _borderImg;

	[SerializeField]
	private float _borderHeightRatio = 2f;

	[SerializeField]
	private Image _fillImg;

	[SerializeField]
	private Image _fillInactiveImg;

	[SerializeField]
	private Image _handleImg;

	[SerializeField]
	private Image _lockIcon;

	[SerializeField]
	private Image _unlockIcon;

	[SerializeField]
	private Image _handleLockIcon;

	[SerializeField]
	private Image _handleUnlockIcon;

	[SerializeField]
	private Color _lockActiveColor;

	[SerializeField]
	private Color _lockInactiveColor;

	[SerializeField]
	private RectTransform _borderImgRef;

	[Header("Steam Settings")]
	[SerializeField]
	private UXSettings _cbUXSettings;

	[Header("Animation")]
	[SerializeField]
	private float _animationDuration = 0.15f;

	[SerializeField]
	private AnimationCurve _toggleEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Header("Fill Colors")]
	[SerializeField]
	private ColorBlock _fillColors;

	[Header("Border Colors")]
	[SerializeField]
	private ColorBlock _borderColors;

	[Header("Borders")]
	[SerializeField]
	private float _normalBorderSize = 1f;

	[SerializeField]
	private float _disabledBorderSize = 1f;

	[SerializeField]
	private float _highlightedBorderSize = 1f;

	[SerializeField]
	private float _pressedBorderSize = 1f;

	[SerializeField]
	private float _selectedBorderSize = 1f;

	[Header("Handle Colors")]
	[SerializeField]
	private ColorBlock _handleColors;

	[Header("Events")]
	[SerializeField]
	private UnityEvent _onToggleOn;

	[SerializeField]
	private UnityEvent _onToggleOff;

	[SerializeField]
	private UnityEvent _onToggleChanged;

	private bool _previousValue;

	private bool _isDisabled;

	private Coroutine _animationCoroutine;

	private bool inside;

	private static bool _triggeredThisFrame = false;

	private static bool _canTrigger = true;
}
