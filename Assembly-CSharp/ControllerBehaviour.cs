using System;
using UnityEngine;

[Obsolete("Use ControllerInputPoller instead", false)]
public class ControllerBehaviour : MonoBehaviour, IBuildValidation
{
	public static ControllerBehaviour Instance { get; private set; }

	private ControllerInputPoller Poller
	{
		get
		{
			if (this.poller != null)
			{
				return this.poller;
			}
			if (ControllerInputPoller.instance != null)
			{
				this.poller = ControllerInputPoller.instance;
				return this.poller;
			}
			return null;
		}
	}

	public bool ButtonDown
	{
		get
		{
			return !(this.Poller == null) && (this.Poller.leftControllerPrimaryButton || this.Poller.leftControllerSecondaryButton || this.Poller.rightControllerPrimaryButton || this.Poller.rightControllerSecondaryButton);
		}
	}

	public bool LeftButtonDown
	{
		get
		{
			return !(this.Poller == null) && (this.Poller.leftControllerPrimaryButton || this.Poller.leftControllerSecondaryButton || this.Poller.leftControllerTriggerButton);
		}
	}

	public bool RightButtonDown
	{
		get
		{
			return !(this.Poller == null) && (this.Poller.rightControllerPrimaryButton || this.Poller.rightControllerSecondaryButton || this.Poller.rightControllerTriggerButton);
		}
	}

	public bool IsLeftStick
	{
		get
		{
			return !(this.Poller == null) && Mathf.Min(this.Poller.leftControllerPrimary2DAxis.x, this.Poller.rightControllerPrimary2DAxis.x) < -this.uxSettings.StickSensitvity;
		}
	}

	public bool IsRightStick
	{
		get
		{
			return !(this.Poller == null) && Mathf.Max(this.Poller.leftControllerPrimary2DAxis.x, this.Poller.rightControllerPrimary2DAxis.x) > this.uxSettings.StickSensitvity;
		}
	}

	public bool IsUpStick
	{
		get
		{
			return !(this.Poller == null) && Mathf.Max(this.Poller.leftControllerPrimary2DAxis.y, this.Poller.rightControllerPrimary2DAxis.y) > this.uxSettings.StickSensitvity;
		}
	}

	public bool IsDownStick
	{
		get
		{
			return !(this.Poller == null) && Mathf.Min(this.Poller.leftControllerPrimary2DAxis.y, this.Poller.rightControllerPrimary2DAxis.y) < -this.uxSettings.StickSensitvity;
		}
	}

	public float StickXValue
	{
		get
		{
			if (!(this.Poller == null))
			{
				return Mathf.Max(Mathf.Abs(this.Poller.leftControllerPrimary2DAxis.x), Mathf.Abs(this.Poller.rightControllerPrimary2DAxis.x));
			}
			return 0f;
		}
	}

	public float StickYValue
	{
		get
		{
			if (!(this.Poller == null))
			{
				return Mathf.Max(Mathf.Abs(this.Poller.leftControllerPrimary2DAxis.y), Mathf.Abs(this.Poller.rightControllerPrimary2DAxis.y));
			}
			return 0f;
		}
	}

	public bool TriggerDown
	{
		get
		{
			return !(this.Poller == null) && (this.Poller.leftControllerTriggerButton || this.Poller.rightControllerTriggerButton);
		}
	}

	public event ControllerBehaviour.OnActionEvent OnAction;

	private void Awake()
	{
		if (ControllerBehaviour.Instance != null)
		{
			Debug.LogError("[CONTROLLER_BEHAVIOUR] Trying to create new singleton but one already exists", base.gameObject);
			Object.DestroyImmediate(this);
			return;
		}
		ControllerBehaviour.Instance = this;
	}

	private void Update()
	{
		bool flag = (this.IsLeftStick && this.wasLeftStick) || (this.IsRightStick && this.wasRightStick) || (this.IsUpStick && this.wasUpStick) || (this.IsDownStick && this.wasDownStick);
		if (Time.time - this.actionTime < this.actionDelay / this.repeatAction)
		{
			return;
		}
		if (this.wasHeld && flag)
		{
			this.repeatAction += this.actionRepeatDelayReduction;
		}
		else
		{
			this.repeatAction = 1f;
		}
		if (this.IsLeftStick || this.IsRightStick || this.IsUpStick || this.IsDownStick || this.ButtonDown)
		{
			this.actionTime = Time.time;
		}
		if (this.OnAction != null)
		{
			this.OnAction();
		}
		this.wasHeld = flag;
		this.wasDownStick = this.IsDownStick;
		this.wasUpStick = this.IsUpStick;
		this.wasLeftStick = this.IsLeftStick;
		this.wasRightStick = this.IsRightStick;
	}

	public bool BuildValidationCheck()
	{
		if (this.uxSettings == null)
		{
			Debug.LogError("ControllerBehaviour must set UXSettings");
			return false;
		}
		return true;
	}

	public static ControllerBehaviour CreateNewControllerBehaviour(GameObject gameObject, UXSettings settings)
	{
		ControllerBehaviour controllerBehaviour = gameObject.AddComponent<ControllerBehaviour>();
		controllerBehaviour.uxSettings = settings;
		return controllerBehaviour;
	}

	private float actionTime;

	private float repeatAction = 1f;

	[SerializeField]
	private UXSettings uxSettings;

	[SerializeField]
	private float actionDelay = 0.5f;

	[SerializeField]
	private float actionRepeatDelayReduction = 0.5f;

	[Tooltip("Should the triggers modify the x axis like the sticks do?")]
	[SerializeField]
	private bool useTriggersAsSticks;

	private ControllerInputPoller poller;

	private bool wasLeftStick;

	private bool wasRightStick;

	private bool wasUpStick;

	private bool wasDownStick;

	private bool wasHeld;

	public delegate void OnActionEvent();
}
