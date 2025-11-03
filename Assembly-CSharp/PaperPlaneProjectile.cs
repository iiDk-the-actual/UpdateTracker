using System;
using GorillaTag.Reactions;
using UnityEngine;
using UnityEngine.Events;

public class PaperPlaneProjectile : MonoBehaviour
{
	public event PaperPlaneProjectile.PaperPlaneHit OnHit;

	public new Transform transform
	{
		get
		{
			return this._tCached;
		}
	}

	public VRRig MyRig
	{
		get
		{
			return this.myRig;
		}
	}

	private void Awake()
	{
		this._tCached = base.transform;
		this.spawnWorldEffects = base.GetComponent<SpawnWorldEffects>();
	}

	private void Start()
	{
		this.ResetProjectile();
	}

	public void ResetProjectile()
	{
		this._timeElapsed = 0f;
		this.flyingObject.SetActive(true);
		this.crashingObject.SetActive(false);
	}

	internal void SetTransferrableState(TransferrableObject.SyncOptions syncType, int state)
	{
		if (!this.useTransferrableObjectState)
		{
			return;
		}
		if (syncType != TransferrableObject.SyncOptions.Bool)
		{
			if (syncType != TransferrableObject.SyncOptions.Int)
			{
				return;
			}
			UnityEvent<int> onItemStateIntChanged = this.OnItemStateIntChanged;
			if (onItemStateIntChanged == null)
			{
				return;
			}
			onItemStateIntChanged.Invoke(state);
			return;
		}
		else
		{
			bool flag = (state & 1) != 0;
			bool flag2 = (state & 2) != 0;
			bool flag3 = (state & 4) != 0;
			bool flag4 = (state & 8) != 0;
			if (flag)
			{
				UnityEvent onItemStateBoolATrue = this.OnItemStateBoolATrue;
				if (onItemStateBoolATrue != null)
				{
					onItemStateBoolATrue.Invoke();
				}
			}
			else
			{
				UnityEvent onItemStateBoolAFalse = this.OnItemStateBoolAFalse;
				if (onItemStateBoolAFalse != null)
				{
					onItemStateBoolAFalse.Invoke();
				}
			}
			if (flag2)
			{
				UnityEvent onItemStateBoolBTrue = this.OnItemStateBoolBTrue;
				if (onItemStateBoolBTrue != null)
				{
					onItemStateBoolBTrue.Invoke();
				}
			}
			else
			{
				UnityEvent onItemStateBoolBFalse = this.OnItemStateBoolBFalse;
				if (onItemStateBoolBFalse != null)
				{
					onItemStateBoolBFalse.Invoke();
				}
			}
			if (flag3)
			{
				UnityEvent onItemStateBoolCTrue = this.OnItemStateBoolCTrue;
				if (onItemStateBoolCTrue != null)
				{
					onItemStateBoolCTrue.Invoke();
				}
			}
			else
			{
				UnityEvent onItemStateBoolCFalse = this.OnItemStateBoolCFalse;
				if (onItemStateBoolCFalse != null)
				{
					onItemStateBoolCFalse.Invoke();
				}
			}
			if (flag4)
			{
				UnityEvent onItemStateBoolDTrue = this.OnItemStateBoolDTrue;
				if (onItemStateBoolDTrue == null)
				{
					return;
				}
				onItemStateBoolDTrue.Invoke();
				return;
			}
			else
			{
				UnityEvent onItemStateBoolDFalse = this.OnItemStateBoolDFalse;
				if (onItemStateBoolDFalse == null)
				{
					return;
				}
				onItemStateBoolDFalse.Invoke();
				return;
			}
		}
	}

	public void Launch(Vector3 startPos, Quaternion startRot, Vector3 vel)
	{
		base.gameObject.SetActive(true);
		this.ResetProjectile();
		this.transform.position = startPos;
		if (this.enableRotation)
		{
			this.transform.rotation = startRot;
		}
		else
		{
			this.transform.LookAt(this.transform.position + vel.normalized);
		}
		this._direction = vel.normalized;
		this._speed = Mathf.Clamp(this.speedCurve.Evaluate(vel.magnitude), this.minSpeed, this.maxSpeed);
		this._stopped = false;
		this.scaleFactor = 0.7f * (this.transform.lossyScale.x - 1f + 1.4285715f);
	}

	private void Update()
	{
		if (this._stopped)
		{
			if (!this.crashingObject.gameObject.activeSelf)
			{
				if (ObjectPools.instance)
				{
					ObjectPools.instance.Destroy(base.gameObject);
					return;
				}
				base.gameObject.SetActive(false);
			}
			return;
		}
		this._timeElapsed += Time.deltaTime;
		this.nextPos = this.transform.position + this._direction * this._speed * Time.deltaTime * this.scaleFactor;
		if (this._timeElapsed < this.maxFlightTime && (this._timeElapsed < this.minFlightTime || Physics.RaycastNonAlloc(this.transform.position, this.nextPos - this.transform.position, this.results, Vector3.Distance(this.transform.position, this.nextPos), this.layerMask.value) == 0))
		{
			this.transform.position = this.nextPos;
			this.transform.Rotate(Mathf.Sin(this._timeElapsed) * 10f * Time.deltaTime, 0f, 0f);
			return;
		}
		if (this._timeElapsed < this.maxFlightTime)
		{
			SlingshotProjectileHitNotifier slingshotProjectileHitNotifier;
			if (this.results[0].collider.TryGetComponent<SlingshotProjectileHitNotifier>(out slingshotProjectileHitNotifier))
			{
				slingshotProjectileHitNotifier.InvokeHit(this, this.results[0].collider);
			}
			if (this.spawnWorldEffects != null)
			{
				this.spawnWorldEffects.RequestSpawn(this.nextPos);
			}
		}
		this._stopped = true;
		this._timeElapsed = 0f;
		PaperPlaneProjectile.PaperPlaneHit onHit = this.OnHit;
		if (onHit != null)
		{
			onHit(this.nextPos);
		}
		this.OnHit = null;
		this.flyingObject.SetActive(false);
		this.crashingObject.SetActive(true);
	}

	internal void SetVRRig(VRRig rig)
	{
		this.myRig = rig;
	}

	private void OnDisable()
	{
		if (this.useTransferrableObjectState)
		{
			UnityEvent onResetProjectileState = this.OnResetProjectileState;
			if (onResetProjectileState == null)
			{
				return;
			}
			onResetProjectileState.Invoke();
		}
	}

	private const float speedScaleRatio = 0.7f;

	[Space]
	[NonSerialized]
	private float _timeElapsed;

	[NonSerialized]
	private float _speed;

	[NonSerialized]
	private Vector3 _direction;

	[NonSerialized]
	private bool _stopped;

	private Transform _tCached;

	private SpawnWorldEffects spawnWorldEffects;

	private Vector3 nextPos;

	private RaycastHit[] results = new RaycastHit[1];

	[Tooltip("Maximum lifetime in seconds for the projectile")]
	[SerializeField]
	private float maxFlightTime = 7.5f;

	[Tooltip("Collisions are ignored for minFlightTime seconds after launch")]
	[SerializeField]
	private float minFlightTime = 0.5f;

	[Tooltip("Hand speed to projectile launch Speed")]
	[SerializeField]
	private AnimationCurve speedCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f, 0f, 0f),
		new Keyframe(6.324555f, 20f, 6.324555f, 6.324555f)
	});

	[Tooltip("maximum speed of launched projectile (clamped after applying speed curve)")]
	[SerializeField]
	private float maxSpeed = 10f;

	[Tooltip("minimum speed of launched projectile (clamped after applying speed curve)")]
	[SerializeField]
	private float minSpeed = 1f;

	[SerializeField]
	private bool enableRotation;

	[Tooltip("Objects enabled when launched and disabled on Hit")]
	[SerializeField]
	private GameObject flyingObject;

	[Tooltip("Objects disabled when launched and enabled on Hit")]
	[SerializeField]
	private GameObject crashingObject;

	[Tooltip("Layers the projectile collides with")]
	[SerializeField]
	private LayerMask layerMask;

	[SerializeField]
	private bool useTransferrableObjectState;

	[SerializeField]
	protected UnityEvent OnResetProjectileState;

	[SerializeField]
	protected string boolADebugName;

	[SerializeField]
	protected UnityEvent OnItemStateBoolATrue;

	[SerializeField]
	protected UnityEvent OnItemStateBoolAFalse;

	[SerializeField]
	protected string boolBDebugName;

	[SerializeField]
	protected UnityEvent OnItemStateBoolBTrue;

	[SerializeField]
	protected UnityEvent OnItemStateBoolBFalse;

	[SerializeField]
	protected string boolCDebugName;

	[SerializeField]
	protected UnityEvent OnItemStateBoolCTrue;

	[SerializeField]
	protected UnityEvent OnItemStateBoolCFalse;

	[SerializeField]
	protected string boolDDebugName;

	[SerializeField]
	protected UnityEvent OnItemStateBoolDTrue;

	[SerializeField]
	protected UnityEvent OnItemStateBoolDFalse;

	[SerializeField]
	protected UnityEvent<int> OnItemStateIntChanged;

	private VRRig myRig;

	private float scaleFactor;

	public delegate void PaperPlaneHit(Vector3 endPoint);
}
