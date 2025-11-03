using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MenagerieCritter : MonoBehaviour, IHoldableObject, IEyeScannable
{
	public Menagerie.CritterData CritterData
	{
		get
		{
			return this._critterData;
		}
	}

	public MenagerieSlot Slot
	{
		get
		{
			return this._slot;
		}
		set
		{
			if (value == this._slot)
			{
				return;
			}
			if (this._slot && this._slot.critter == this)
			{
				this._slot.critter = null;
			}
			this._slot = value;
			if (this._slot)
			{
				this._slot.critter = this;
			}
		}
	}

	private void Update()
	{
		this.UpdateAnimation();
	}

	public void ApplyCritterData(Menagerie.CritterData critterData)
	{
		this._critterData = critterData;
		this._critterConfiguration = this._critterData.GetConfiguration();
		this._critterData.instance = this;
		this._critterData.GetConfiguration().ApplyVisualsTo(this.visuals, false);
		this.visuals.SetAppearance(this._critterData.appearance);
		this._animRoot = this.visuals.bodyRoot;
		this._bodyScale = this._animRoot.localScale;
		this.PlayAnimation(this.heldAnimation, global::UnityEngine.Random.value);
	}

	private void PlayAnimation(CrittersAnim anim, float time = 0f)
	{
		this._currentAnim = anim;
		this._currentAnimTime = time;
		if (this._currentAnim == null)
		{
			this._animRoot.localPosition = Vector3.zero;
			this._animRoot.localRotation = Quaternion.identity;
			this._animRoot.localScale = this._bodyScale;
		}
	}

	private void UpdateAnimation()
	{
		if (this._currentAnim != null)
		{
			this._currentAnimTime += Time.deltaTime * this._currentAnim.playSpeed;
			this._currentAnimTime %= 1f;
			float num = this._currentAnim.squashAmount.Evaluate(this._currentAnimTime);
			float num2 = this._currentAnim.forwardOffset.Evaluate(this._currentAnimTime);
			float num3 = this._currentAnim.horizontalOffset.Evaluate(this._currentAnimTime);
			float num4 = this._currentAnim.verticalOffset.Evaluate(this._currentAnimTime);
			this._animRoot.localPosition = Vector3.Scale(this._bodyScale, new Vector3(num3, num4, num2));
			float num5 = 1f - num;
			num5 *= 0.5f;
			num5 += 1f;
			this._animRoot.localScale = Vector3.Scale(this._bodyScale, new Vector3(num5, num, num5));
		}
	}

	public bool TwoHanded
	{
		get
		{
			return false;
		}
	}

	public void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
	}

	public void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		this.isHeld = true;
		this.isHeldLeftHand = grabbingHand == EquipmentInteractor.instance.leftHand;
		if (this.grabbedHaptics)
		{
			CrittersManager.PlayHaptics(this.grabbedHaptics, this.grabbedHapticsStrength, this.isHeldLeftHand);
		}
		if (this.grabbedFX)
		{
			this.grabbedFX.SetActive(true);
		}
		EquipmentInteractor.instance.UpdateHandEquipment(this, this.isHeldLeftHand);
		base.transform.parent = grabbingHand.transform;
		this.isHeld = true;
		this.heldBy = grabbingHand;
		Action onDataChange = this.OnDataChange;
		if (onDataChange == null)
		{
			return;
		}
		onDataChange();
	}

	public bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (EquipmentInteractor.instance.rightHandHeldEquipment == this && releasingHand != EquipmentInteractor.instance.rightHand)
		{
			return false;
		}
		if (EquipmentInteractor.instance.leftHandHeldEquipment == this && releasingHand != EquipmentInteractor.instance.leftHand)
		{
			return false;
		}
		if (this.grabbedHaptics)
		{
			CrittersManager.StopHaptics(this.isHeldLeftHand);
		}
		if (this.grabbedFX)
		{
			this.grabbedFX.SetActive(false);
		}
		EquipmentInteractor.instance.UpdateHandEquipment(null, this.isHeldLeftHand);
		this.isHeld = false;
		this.isHeldLeftHand = false;
		Action<MenagerieCritter> onReleased = this.OnReleased;
		if (onReleased != null)
		{
			onReleased(this);
		}
		Action onDataChange = this.OnDataChange;
		if (onDataChange != null)
		{
			onDataChange();
		}
		this.ResetToTransform();
		return true;
	}

	public void ResetToTransform()
	{
		base.transform.parent = this._slot.transform;
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = quaternion.identity;
	}

	public void DropItemCleanup()
	{
	}

	int IEyeScannable.scannableId
	{
		get
		{
			return base.gameObject.GetInstanceID();
		}
	}

	Vector3 IEyeScannable.Position
	{
		get
		{
			return this.bodyCollider.bounds.center;
		}
	}

	Bounds IEyeScannable.Bounds
	{
		get
		{
			return this.bodyCollider.bounds;
		}
	}

	IList<KeyValueStringPair> IEyeScannable.Entries
	{
		get
		{
			return this.BuildEyeScannerData();
		}
	}

	public void OnEnable()
	{
		EyeScannerMono.Register(this);
	}

	public void OnDisable()
	{
		EyeScannerMono.Unregister(this);
	}

	private IList<KeyValueStringPair> BuildEyeScannerData()
	{
		this.eyeScanData[0] = new KeyValueStringPair("Name", this._critterConfiguration.critterName);
		this.eyeScanData[1] = new KeyValueStringPair("Type", this._critterConfiguration.animalType.ToString());
		this.eyeScanData[2] = new KeyValueStringPair("Temperament", this._critterConfiguration.behaviour.temperament);
		this.eyeScanData[3] = new KeyValueStringPair("Habitat", this._critterConfiguration.biome.GetHabitatDescription());
		this.eyeScanData[4] = new KeyValueStringPair("Size", this.visuals.Appearance.size.ToString("0.00"));
		this.eyeScanData[5] = new KeyValueStringPair("State", this.GetCurrentStateName());
		return this.eyeScanData;
	}

	public event Action OnDataChange;

	private string GetCurrentStateName()
	{
		if (!this.isHeld)
		{
			return "Content";
		}
		return "Happy";
	}

	GameObject IHoldableObject.get_gameObject()
	{
		return base.gameObject;
	}

	string IHoldableObject.get_name()
	{
		return base.name;
	}

	void IHoldableObject.set_name(string value)
	{
		base.name = value;
	}

	public CritterVisuals visuals;

	public Collider bodyCollider;

	[Header("Feedback")]
	public CrittersAnim heldAnimation;

	public AudioClip grabbedHaptics;

	public float grabbedHapticsStrength = 1f;

	public GameObject grabbedFX;

	private CrittersAnim _currentAnim;

	private float _currentAnimTime;

	private Transform _animRoot;

	private Vector3 _bodyScale;

	public MenagerieCritter.MenagerieCritterState currentState = MenagerieCritter.MenagerieCritterState.Displaying;

	private CritterConfiguration _critterConfiguration;

	private Menagerie.CritterData _critterData;

	private MenagerieSlot _slot;

	private List<GorillaGrabber> activeGrabbers = new List<GorillaGrabber>();

	private GameObject heldBy;

	private bool isHeld;

	private bool isHeldLeftHand;

	public Action<MenagerieCritter> OnReleased;

	private KeyValueStringPair[] eyeScanData = new KeyValueStringPair[6];

	public enum MenagerieCritterState
	{
		Donating,
		Displaying
	}
}
