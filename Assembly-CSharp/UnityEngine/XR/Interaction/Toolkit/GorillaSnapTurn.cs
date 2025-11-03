using System;
using System.Collections.Generic;
using GorillaLocomotion;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

namespace UnityEngine.XR.Interaction.Toolkit
{
	public class GorillaSnapTurn : LocomotionProvider, ITickSystemTick
	{
		public bool TickRunning { get; set; }

		public GorillaSnapTurn.InputAxes turnUsage
		{
			get
			{
				return this.m_TurnUsage;
			}
			set
			{
				this.m_TurnUsage = value;
			}
		}

		public List<XRController> controllers
		{
			get
			{
				return this.m_Controllers;
			}
			set
			{
				this.m_Controllers = value;
			}
		}

		public float turnAmount
		{
			get
			{
				return this.m_TurnAmount;
			}
			set
			{
				this.m_TurnAmount = value;
			}
		}

		public float debounceTime
		{
			get
			{
				return this.m_DebounceTime;
			}
			set
			{
				this.m_DebounceTime = value;
			}
		}

		public float deadZone
		{
			get
			{
				return this.m_DeadZone;
			}
			set
			{
				this.m_DeadZone = value;
			}
		}

		public string turnType
		{
			get
			{
				return this.m_TurnType;
			}
			private set
			{
				this.m_TurnType = value;
			}
		}

		public int turnFactor
		{
			get
			{
				return this.m_TurnFactor;
			}
			private set
			{
				this.m_TurnFactor = value;
			}
		}

		public static GorillaSnapTurn CachedSnapTurnRef
		{
			get
			{
				if (GorillaSnapTurn._cachedReference == null)
				{
					Debug.LogError("[SNAP_TURN] Tried accessing static cached reference, but was still null. Trying to find component in scene");
					GorillaSnapTurn._cachedReference = Object.FindAnyObjectByType<GorillaSnapTurn>();
				}
				return GorillaSnapTurn._cachedReference;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (GorillaSnapTurn._cachedReference != null)
			{
				Debug.LogError("[SNAP_TURN] A [GorillaSnapTurn] component already exists in the scene");
				return;
			}
			GorillaSnapTurn._cachedReference = this;
			TickSystem<object>.AddTickCallback(this);
		}

		public void Tick()
		{
			this.ValidateTurningOverriders();
			if (this.m_Controllers.Count > 0)
			{
				this.EnsureControllerDataListSize();
				for (int i = 0; i < this.m_Controllers.Count; i++)
				{
					XRController xrcontroller = this.m_Controllers[i];
					if (!(xrcontroller == null) && xrcontroller.enableInputActions)
					{
						float num = 0f;
						if (xrcontroller.controllerNode == XRNode.RightHand)
						{
							num = ControllerInputPoller.instance.rightControllerPrimary2DAxis.x;
						}
						else if (xrcontroller.controllerNode == XRNode.LeftHand)
						{
							num = ControllerInputPoller.instance.leftControllerPrimary2DAxis.x;
						}
						if (num > this.deadZone)
						{
							this.StartTurn(this.m_TurnAmount);
						}
						else if (num < -this.deadZone)
						{
							this.StartTurn(-this.m_TurnAmount);
						}
						else
						{
							this.m_AxisReset = true;
						}
					}
				}
			}
			if (Mathf.Abs(this.m_CurrentTurnAmount) > 0f && base.TryPrepareLocomotion())
			{
				if (this.xrOrigin != null)
				{
					GTPlayer.Instance.Turn(this.m_CurrentTurnAmount);
				}
				this.m_CurrentTurnAmount = 0f;
				base.TryEndLocomotion();
			}
		}

		private void EnsureControllerDataListSize()
		{
			if (this.m_Controllers.Count != this.m_ControllersWereActive.Count)
			{
				while (this.m_ControllersWereActive.Count < this.m_Controllers.Count)
				{
					this.m_ControllersWereActive.Add(false);
				}
				while (this.m_ControllersWereActive.Count < this.m_Controllers.Count)
				{
					this.m_ControllersWereActive.RemoveAt(this.m_ControllersWereActive.Count - 1);
				}
			}
		}

		internal void FakeStartTurn(bool isLeft)
		{
			this.StartTurn(isLeft ? (-this.m_TurnAmount) : this.m_TurnAmount);
		}

		private void StartTurn(float amount)
		{
			if (this.m_TimeStarted + this.m_DebounceTime > Time.time && !this.m_AxisReset)
			{
				return;
			}
			if (base.isLocomotionActive)
			{
				return;
			}
			if (this.turningOverriders.Count > 0)
			{
				return;
			}
			this.m_TimeStarted = Time.time;
			this.m_CurrentTurnAmount = amount;
			this.m_AxisReset = false;
		}

		public void ChangeTurnMode(string turnMode, int turnSpeedFactor)
		{
			this.turnType = turnMode;
			this.turnFactor = turnSpeedFactor;
			if (turnMode == "SNAP")
			{
				this.m_DebounceTime = 0.5f;
				this.m_TurnAmount = 60f * this.ConvertedTurnFactor((float)turnSpeedFactor);
				return;
			}
			if (!(turnMode == "SMOOTH"))
			{
				this.m_DebounceTime = 0f;
				this.m_TurnAmount = 0f;
				return;
			}
			this.m_DebounceTime = 0f;
			this.m_TurnAmount = 360f * Time.fixedDeltaTime * this.ConvertedTurnFactor((float)turnSpeedFactor);
		}

		public float ConvertedTurnFactor(float newTurnSpeed)
		{
			return Mathf.Max(0.75f, 0.5f + newTurnSpeed / 10f * 1.5f);
		}

		public void SetTurningOverride(ISnapTurnOverride caller)
		{
			if (!this.turningOverriders.Contains(caller))
			{
				this.turningOverriders.Add(caller);
			}
		}

		public void UnsetTurningOverride(ISnapTurnOverride caller)
		{
			if (this.turningOverriders.Contains(caller))
			{
				this.turningOverriders.Remove(caller);
			}
		}

		public void ValidateTurningOverriders()
		{
			foreach (ISnapTurnOverride snapTurnOverride in this.turningOverriders)
			{
				if (snapTurnOverride == null || !snapTurnOverride.TurnOverrideActive())
				{
					this.turningOverriders.Remove(snapTurnOverride);
				}
			}
		}

		public static void DisableSnapTurn()
		{
			Debug.Log("[SNAP_TURN] Disabling Snap Turn");
			if (GorillaSnapTurn.CachedSnapTurnRef == null)
			{
				return;
			}
			GorillaSnapTurn._cachedTurnFactor = PlayerPrefs.GetInt("turnFactor");
			GorillaSnapTurn._cachedTurnType = PlayerPrefs.GetString("stickTurning");
			GorillaSnapTurn.CachedSnapTurnRef.ChangeTurnMode("NONE", 0);
		}

		public static void UpdateAndSaveTurnType(string mode)
		{
			if (GorillaSnapTurn.CachedSnapTurnRef == null)
			{
				Debug.LogError("[SNAP_TURN] Failed to Update, [CachedSnapTurnRef] is NULL");
				return;
			}
			PlayerPrefs.SetString("stickTurning", mode);
			PlayerPrefs.Save();
			GorillaSnapTurn.CachedSnapTurnRef.ChangeTurnMode(mode, GorillaSnapTurn.CachedSnapTurnRef.turnFactor);
		}

		public static void UpdateAndSaveTurnFactor(int factor)
		{
			if (GorillaSnapTurn.CachedSnapTurnRef == null)
			{
				Debug.LogError("[SNAP_TURN] Failed to Update, [CachedSnapTurnRef] is NULL");
				return;
			}
			PlayerPrefs.SetInt("turnFactor", factor);
			PlayerPrefs.Save();
			GorillaSnapTurn.CachedSnapTurnRef.ChangeTurnMode(GorillaSnapTurn.CachedSnapTurnRef.turnType, factor);
		}

		public static void LoadSettingsFromPlayerPrefs()
		{
			if (GorillaSnapTurn.CachedSnapTurnRef == null)
			{
				return;
			}
			string text = ((Application.platform == RuntimePlatform.Android) ? "NONE" : "SNAP");
			string @string = PlayerPrefs.GetString("stickTurning", text);
			int @int = PlayerPrefs.GetInt("turnFactor", 4);
			GorillaSnapTurn.CachedSnapTurnRef.ChangeTurnMode(@string, @int);
		}

		public static void LoadSettingsFromCache()
		{
			if (GorillaSnapTurn.CachedSnapTurnRef == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(GorillaSnapTurn._cachedTurnType))
			{
				GorillaSnapTurn._cachedTurnType = ((Application.platform == RuntimePlatform.Android) ? "NONE" : "SNAP");
			}
			string cachedTurnType = GorillaSnapTurn._cachedTurnType;
			int cachedTurnFactor = GorillaSnapTurn._cachedTurnFactor;
			GorillaSnapTurn.CachedSnapTurnRef.ChangeTurnMode(cachedTurnType, cachedTurnFactor);
		}

		[Header("References")]
		[SerializeField]
		private XROrigin xrOrigin;

		private static readonly InputFeatureUsage<Vector2>[] m_Vec2UsageList = new InputFeatureUsage<Vector2>[]
		{
			CommonUsages.primary2DAxis,
			CommonUsages.secondary2DAxis
		};

		[SerializeField]
		[Tooltip("The 2D Input Axis on the primary devices that will be used to trigger a snap turn.")]
		private GorillaSnapTurn.InputAxes m_TurnUsage;

		[SerializeField]
		[Tooltip("A list of controllers that allow Snap Turn.  If an XRController is not enabled, or does not have input actions enabled.  Snap Turn will not work.")]
		private List<XRController> m_Controllers = new List<XRController>();

		[SerializeField]
		[Tooltip("The number of degrees clockwise to rotate when snap turning clockwise.")]
		private float m_TurnAmount = 45f;

		[SerializeField]
		[Tooltip("The amount of time that the system will wait before starting another snap turn.")]
		private float m_DebounceTime = 0.5f;

		[SerializeField]
		[Tooltip("The deadzone that the controller movement will have to be above to trigger a snap turn.")]
		private float m_DeadZone = 0.75f;

		private float m_CurrentTurnAmount;

		private float m_TimeStarted;

		private bool m_AxisReset;

		public float turnSpeed = 1f;

		private HashSet<ISnapTurnOverride> turningOverriders = new HashSet<ISnapTurnOverride>();

		private List<bool> m_ControllersWereActive = new List<bool>();

		private static int _cachedTurnFactor;

		private static string _cachedTurnType;

		private string m_TurnType = "";

		private int m_TurnFactor = 1;

		[OnEnterPlay_SetNull]
		private static GorillaSnapTurn _cachedReference;

		public enum InputAxes
		{
			Primary2DAxis,
			Secondary2DAxis
		}
	}
}
