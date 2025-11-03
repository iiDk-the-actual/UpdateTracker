using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class ElfLauncher : MonoBehaviour
{
	private void OnEnable()
	{
		if (this._events == null)
		{
			this._events = base.gameObject.GetOrAddComponent<RubberDuckEvents>();
			NetPlayer netPlayer = ((this.parentHoldable.myOnlineRig != null) ? this.parentHoldable.myOnlineRig.creator : ((this.parentHoldable.myRig != null) ? ((this.parentHoldable.myRig.creator != null) ? this.parentHoldable.myRig.creator : NetworkSystem.Instance.LocalPlayer) : null));
			if (netPlayer != null)
			{
				this.m_player = netPlayer;
				this._events.Init(netPlayer);
			}
			else
			{
				Debug.LogError("Failed to get a reference to the Photon Player needed to hook up the cosmetic event");
			}
		}
		if (this._events != null)
		{
			this._events.Activate += this.ShootShared;
		}
	}

	private void OnDisable()
	{
		if (this._events != null)
		{
			this._events.Activate -= this.ShootShared;
			this._events.Dispose();
			this._events = null;
			this.m_player = null;
		}
	}

	private void Awake()
	{
		this._events = base.GetComponent<RubberDuckEvents>();
		this.elfProjectileHash = PoolUtils.GameObjHashCode(this.elfProjectilePrefab);
		TransferrableObjectHoldablePart_Crank[] array = this.cranks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetOnCrankedCallback(new Action<float>(this.OnCranked));
		}
	}

	private void OnCranked(float deltaAngle)
	{
		this.currentShootCrankAmount += deltaAngle;
		if (Mathf.Abs(this.currentShootCrankAmount) > this.crankShootThreshold)
		{
			this.currentShootCrankAmount = 0f;
			this.Shoot();
		}
		this.currentClickCrankAmount += deltaAngle;
		if (Mathf.Abs(this.currentClickCrankAmount) > this.crankClickThreshold)
		{
			this.currentClickCrankAmount = 0f;
			this.crankClickAudio.Play();
		}
	}

	private void Shoot()
	{
		if (this.parentHoldable.IsLocalObject())
		{
			GorillaTagger.Instance.StartVibration(true, this.shootHapticStrength, this.shootHapticDuration);
			GorillaTagger.Instance.StartVibration(false, this.shootHapticStrength, this.shootHapticDuration);
			if (PhotonNetwork.InRoom)
			{
				this._events.Activate.RaiseAll(new object[]
				{
					this.muzzle.transform.position,
					this.muzzle.transform.forward
				});
				return;
			}
			this.ShootShared(this.muzzle.transform.position, this.muzzle.transform.forward);
		}
	}

	private void ShootShared(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
	{
		if (args.Length != 2)
		{
			return;
		}
		if (sender != target)
		{
			return;
		}
		VRRig ownerRig = this.parentHoldable.ownerRig;
		if (info.senderID != ownerRig.creator.ActorNumber)
		{
			return;
		}
		if (args.Length == 2)
		{
			object obj = args[0];
			if (obj is Vector3)
			{
				Vector3 vector = (Vector3)obj;
				obj = args[1];
				if (obj is Vector3)
				{
					Vector3 vector2 = (Vector3)obj;
					float num = 10000f;
					if ((in vector).IsValid(in num))
					{
						float num2 = 10000f;
						if ((in vector2).IsValid(in num2))
						{
							if (!FXSystem.CheckCallSpam(ownerRig.fxSettings, 4, info.SentServerTime) || !ownerRig.IsPositionInRange(vector, 6f))
							{
								return;
							}
							this.ShootShared(vector, vector2);
							return;
						}
					}
				}
			}
		}
	}

	protected virtual void ShootShared(Vector3 origin, Vector3 direction)
	{
		this.shootAudio.Play();
		Vector3 lossyScale = base.transform.lossyScale;
		GameObject gameObject = ObjectPools.instance.Instantiate(this.elfProjectileHash, true);
		gameObject.transform.position = origin;
		gameObject.transform.rotation = Quaternion.LookRotation(direction);
		gameObject.transform.localScale = lossyScale;
		gameObject.GetComponent<Rigidbody>().linearVelocity = direction * this.muzzleVelocity * lossyScale.x;
	}

	[SerializeField]
	protected TransferrableObject parentHoldable;

	[SerializeField]
	private TransferrableObjectHoldablePart_Crank[] cranks;

	[SerializeField]
	private float crankShootThreshold = 360f;

	[SerializeField]
	private float crankClickThreshold = 30f;

	[SerializeField]
	private Transform muzzle;

	[SerializeField]
	private GameObject elfProjectilePrefab;

	protected int elfProjectileHash;

	[SerializeField]
	protected float muzzleVelocity = 10f;

	[SerializeField]
	private SoundBankPlayer crankClickAudio;

	[SerializeField]
	protected SoundBankPlayer shootAudio;

	[SerializeField]
	private float shootHapticStrength;

	[SerializeField]
	private float shootHapticDuration;

	private RubberDuckEvents _events;

	private float currentShootCrankAmount;

	private float currentClickCrankAmount;

	private NetPlayer m_player;
}
