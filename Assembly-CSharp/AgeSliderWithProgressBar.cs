using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AgeSliderWithProgressBar : MonoBehaviourTick
{
	public AgeSliderWithProgressBar.SliderHeldEvent onHoldComplete
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

	public bool AdjustAge
	{
		get
		{
			return this._adjustAge;
		}
	}

	public bool ControllerActive
	{
		get
		{
			return this.controllerActive;
		}
		set
		{
			if (value)
			{
				ControllerBehaviour.Instance.OnAction += this.PostUpdate;
			}
			else
			{
				ControllerBehaviour.Instance.OnAction -= this.PostUpdate;
			}
			this.controllerActive = value;
		}
	}

	public string LockMessage
	{
		get
		{
			return this._lockMessage;
		}
		set
		{
			this._lockMessage = value;
		}
	}

	public int CurrentAge
	{
		get
		{
			return this._currentAge;
		}
	}

	private void Awake()
	{
		if (this._messageText)
		{
			this._originalText = this._messageText.text;
		}
	}

	public void SetOriginalText(string text)
	{
		this._originalText = text;
	}

	private new void OnEnable()
	{
		base.OnEnable();
		if (this._progressBarContainer != null && this.progressBarFill != null)
		{
			this.progressBarFill.rectTransform.localScale = new Vector3(0f, 1f, 1f);
		}
		if (this._ageValueTxt)
		{
			this._ageValueTxt.text = ((this._currentAge > 0) ? this._currentAge.ToString() : "?");
		}
	}

	public override void Tick()
	{
		if (!this._progressBarContainer)
		{
			return;
		}
		if (!this.ControllerActive)
		{
			return;
		}
		if (!this._lockMessage.IsNullOrEmpty())
		{
			this.progress = 0f;
			if (this._messageText)
			{
				this._messageText.text = this.LockMessage;
			}
		}
		else
		{
			if (this._messageText)
			{
				this._messageText.text = this._originalText;
			}
			if ((double)this.progress == 1.0)
			{
				this.m_OnHoldComplete.Invoke(this._currentAge);
				this.progress = 0f;
			}
			if (ControllerBehaviour.Instance.ButtonDown && this._progressBarContainer != null && (this._currentAge > 0 || !this.AdjustAge))
			{
				this.progress += Time.deltaTime / this.holdTime;
				this.progress = Mathf.Clamp01(this.progress);
			}
			else
			{
				this.progress = 0f;
			}
		}
		if (this._progressBarContainer != null)
		{
			this.progressBarFill.rectTransform.localScale = new Vector3(this.progress, 1f, 1f);
		}
	}

	private void PostUpdate()
	{
		if (this.ControllerActive && this._ageValueTxt && this._ageSlidable && !this._incrementButtonsLockingSlider)
		{
			if (ControllerBehaviour.Instance.IsLeftStick)
			{
				this._currentAge = Mathf.Clamp(this._currentAge - 1, 0, this._maxAge);
				if (this._currentAge > 0 && this._currentAge < this._maxAge)
				{
					HandRayController.Instance.PulseActiveHandray(this._stickVibrationStrength, this._stickVibrationDuration);
				}
			}
			if (ControllerBehaviour.Instance.IsRightStick)
			{
				this._currentAge = Mathf.Clamp(this._currentAge + 1, 0, this._maxAge);
				if (this._currentAge > 0 && this._currentAge < this._maxAge)
				{
					HandRayController.Instance.PulseActiveHandray(this._stickVibrationStrength, this._stickVibrationDuration);
				}
			}
		}
		if (this._ageValueTxt)
		{
			this._ageValueTxt.text = this.GetAgeString();
			if (this._progressBarContainer != null)
			{
				this._progressBarContainer.SetActive(this._currentAge > 0);
			}
		}
	}

	public void EnableEditing()
	{
		this._ageSlidable = true;
	}

	public void DisableEditing()
	{
		this._ageSlidable = false;
	}

	public string GetAgeString()
	{
		if (this._confirmButton)
		{
			this._confirmButton.interactable = true;
		}
		if (this._currentAge == 0)
		{
			if (this._confirmButton)
			{
				this._confirmButton.interactable = false;
			}
			return "?";
		}
		if (this._currentAge == this._maxAge)
		{
			return this._maxAge.ToString() + "+";
		}
		return this._currentAge.ToString();
	}

	public void ForceAddAge(int number)
	{
		this._incrementButtonsLockingSlider = true;
		this._currentAge = Math.Min(this._currentAge + number, this._maxAge);
	}

	public void ForceSubtractAge(int number)
	{
		this._incrementButtonsLockingSlider = true;
		this._currentAge = Math.Max(this._currentAge - number, 1);
	}

	private const int MIN_AGE = 13;

	[SerializeField]
	private AgeSliderWithProgressBar.SliderHeldEvent m_OnHoldComplete = new AgeSliderWithProgressBar.SliderHeldEvent();

	[SerializeField]
	private bool _adjustAge;

	[SerializeField]
	private int _maxAge = 25;

	[SerializeField]
	private TMP_Text _ageValueTxt;

	[Tooltip("Optional game object that should hold the Progress Bar Fill. Disables Hold functionality if null.")]
	[SerializeField]
	private GameObject _progressBarContainer;

	[SerializeField]
	private float holdTime = 2.5f;

	[SerializeField]
	private Image progressBarFill;

	[SerializeField]
	private TMP_Text _messageText;

	[SerializeField]
	private float _stickVibrationStrength = 0.1f;

	[SerializeField]
	private float _stickVibrationDuration = 0.05f;

	[SerializeField]
	private KIDUIButton _confirmButton;

	private bool _ageSlidable = true;

	private bool _incrementButtonsLockingSlider;

	private bool controllerActive;

	[SerializeField]
	private string _lockMessage;

	private string _originalText;

	private int _currentAge;

	private float progress;

	[Serializable]
	public class SliderHeldEvent : UnityEvent<int>
	{
	}
}
