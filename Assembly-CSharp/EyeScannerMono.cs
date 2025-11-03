using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Text;
using GorillaLocomotion;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class EyeScannerMono : MonoBehaviour, ISpawnable, IGorillaSliceableSimple
{
	private Color32 KeyTextColor
	{
		get
		{
			return this.m_keyTextColor;
		}
		set
		{
			this.m_keyTextColor = value;
			this._keyRichTextColorTagString = string.Format(CultureInfo.InvariantCulture.NumberFormat, "<color=#{0:X2}{1:X2}{2:X2}>", value.r, value.g, value.b);
		}
	}

	private List<IEyeScannable> registeredScannables
	{
		get
		{
			return EyeScannerMono._registeredScannables;
		}
	}

	public static void Register(IEyeScannable scannable)
	{
		if (EyeScannerMono._registeredScannableIds.Add(scannable.scannableId))
		{
			EyeScannerMono._registeredScannables.Add(scannable);
		}
	}

	public static void Unregister(IEyeScannable scannable)
	{
		if (EyeScannerMono._registeredScannableIds.Remove(scannable.scannableId))
		{
			EyeScannerMono._registeredScannables.Remove(scannable);
		}
	}

	protected void Awake()
	{
		this._sb = ZString.CreateStringBuilder();
		this.KeyTextColor = this.KeyTextColor;
		math.sign(this.m_textTyper.transform.parent.localScale);
		this.m_textTyper.SetText(string.Empty);
		this.m_reticle.gameObject.SetActive(false);
		this.m_textTyper.gameObject.SetActive(false);
		this.m_overlayBg.SetActive(false);
		this._line = base.GetComponent<LineRenderer>();
		this._line.enabled = false;
	}

	bool ISpawnable.IsSpawned { get; set; }

	ECosmeticSelectSide ISpawnable.CosmeticSelectedSide { get; set; }

	public string DebugData { get; private set; }

	public void OnSpawn(VRRig rig)
	{
		if (rig != null && !rig.isOfflineVRRig)
		{
			Object.Destroy(base.gameObject);
		}
		if (GTPlayer.hasInstance)
		{
			GTPlayer instance = GTPlayer.Instance;
			this._firstPersonCamera = instance.GetComponentInChildren<Camera>();
			this._has_firstPersonCamera = this._firstPersonCamera != null;
		}
	}

	public void OnDespawn()
	{
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	void IGorillaSliceableSimple.SliceUpdate()
	{
		if (GorillaTagger.Instance.offlineVRRig.zoneEntity.currentZone == GTZone.bayou)
		{
			if (this._oldClosestScannable != null)
			{
				this._OnScannableChanged(null, false);
				this._oldClosestScannable = null;
			}
			return;
		}
		IEyeScannable eyeScannable = null;
		Transform transform = base.transform;
		Vector3 position = transform.position;
		Vector3 forward = transform.forward;
		float num = this.m_LookPrecision;
		for (int i = 0; i < EyeScannerMono._registeredScannables.Count; i++)
		{
			IEyeScannable eyeScannable2 = EyeScannerMono._registeredScannables[i];
			Vector3 normalized = (eyeScannable2.Position - position).normalized;
			float num2 = Vector3.Distance(position, eyeScannable2.Position);
			float num3 = Vector3.Dot(forward, normalized);
			if (num2 >= this.m_scanDistanceMin && num2 <= this.m_scanDistanceMax && num3 > num)
			{
				RaycastHit raycastHit;
				if (!this.m_xrayVision && Physics.Raycast(position, normalized, out raycastHit, this.m_scanDistanceMax, this._layerMask.value))
				{
					IEyeScannable componentInParent = raycastHit.collider.GetComponentInParent<IEyeScannable>();
					if (componentInParent == null || componentInParent != eyeScannable2)
					{
						goto IL_00EF;
					}
				}
				num = num3;
				eyeScannable = eyeScannable2;
			}
			IL_00EF:;
		}
		if (eyeScannable != this._oldClosestScannable)
		{
			if (this._oldClosestScannable != null)
			{
				this._oldClosestScannable.OnDataChange -= this.Scannable_OnDataChange;
			}
			this._OnScannableChanged(eyeScannable, true);
			this._oldClosestScannable = eyeScannable;
			if (this._oldClosestScannable != null)
			{
				this._oldClosestScannable.OnDataChange += this.Scannable_OnDataChange;
			}
		}
	}

	private void Scannable_OnDataChange()
	{
		this._OnScannableChanged(this._oldClosestScannable, false);
	}

	private void LateUpdate()
	{
		if (this._oldClosestScannable != null)
		{
			this.m_reticle.position = this._oldClosestScannable.Position;
			float num = math.distance(base.transform.position, this.m_reticle.position);
			Mathf.Clamp(num * 0.33333f, 0f, 1f);
			float num2 = num * this.m_reticleScale;
			float num3 = num * this.m_textScale;
			float num4 = num * this.m_overlayScale;
			this.m_reticle.localScale = new Vector3(num2, num2, num2);
			this.m_overlay.localPosition = new Vector3(this.m_position.x * num, this.m_position.y * num, num);
			this.m_overlay.localScale = new Vector3(num4, num4, 1f);
			this._line.SetPosition(0, this.m_reticle.position);
			this._line.SetPosition(1, this.m_textTyper.transform.position + this.m_pointerOffset * num3);
			this._line.widthMultiplier = num2;
		}
	}

	private void _OnScannableChanged(IEyeScannable scannable, bool typeingShow)
	{
		this._sb.Clear();
		if (scannable == null)
		{
			this.m_textTyper.SetText(this._sb);
			this.m_textTyper.gameObject.SetActive(false);
			this.m_reticle.gameObject.SetActive(false);
			this.m_overlayBg.SetActive(false);
			this.m_reticle.parent = base.transform;
			this._line.enabled = false;
			return;
		}
		this.m_reticle.gameObject.SetActive(true);
		this.m_textTyper.gameObject.SetActive(true);
		this.m_overlayBg.SetActive(true);
		this.m_reticle.position = scannable.Position;
		this._line.enabled = true;
		this._sb.AppendLine(this.DebugData);
		this._entryIndexes[0] = 0;
		int i = 1;
		int num = 0;
		for (int j = 0; j < scannable.Entries.Count; j++)
		{
			KeyValueStringPair keyValueStringPair = scannable.Entries[j];
			if (!string.IsNullOrEmpty(keyValueStringPair.Key))
			{
				this._sb.Append(this._keyRichTextColorTagString);
				this._sb.Append(keyValueStringPair.Key);
				this._sb.Append("</color>: ");
				num += keyValueStringPair.Key.Length + 2;
			}
			if (!string.IsNullOrEmpty(keyValueStringPair.Value))
			{
				this._sb.Append(keyValueStringPair.Value);
				num += keyValueStringPair.Value.Length;
			}
			this._sb.AppendLine();
			num += Environment.NewLine.Length;
			if (i < this._entryIndexes.Length)
			{
				this._entryIndexes[i++] = num - 1;
			}
		}
		while (i < this._entryIndexes.Length)
		{
			this._entryIndexes[i] = -1;
			i++;
		}
		if (typeingShow)
		{
			this.m_textTyper.SetText(this._sb, this._entryIndexes, num);
			return;
		}
		this.m_textTyper.UpdateText(this._sb, num);
	}

	[FormerlySerializedAs("_scanDistance")]
	[Tooltip("Any scannables with transforms beyond this distance will be automatically ignored.")]
	[SerializeField]
	private float m_scanDistanceMax = 10f;

	[SerializeField]
	private float m_scanDistanceMin = 0.5f;

	[FormerlySerializedAs("_textTyper")]
	[Tooltip("The component that handles setting text in the TextMeshPro and animates the text typing.")]
	[SerializeField]
	private TextTyperAnimatorMono m_textTyper;

	[SerializeField]
	private Transform m_reticle;

	[SerializeField]
	private Transform m_overlay;

	[SerializeField]
	private GameObject m_overlayBg;

	[SerializeField]
	private float m_reticleScale = 1f;

	[SerializeField]
	private float m_textScale = 1f;

	[SerializeField]
	private float m_overlayScale = 1f;

	[SerializeField]
	private Vector3 m_pointerOffset;

	[SerializeField]
	private Vector2 m_position;

	[HideInInspector]
	[SerializeField]
	private Color32 m_keyTextColor = new Color32(byte.MaxValue, 34, 0, byte.MaxValue);

	private string _keyRichTextColorTagString = "";

	private static readonly List<IEyeScannable> _registeredScannables = new List<IEyeScannable>(128);

	private static readonly HashSet<int> _registeredScannableIds = new HashSet<int>(128);

	private IEyeScannable _oldClosestScannable;

	private Utf16ValueStringBuilder _sb;

	private readonly int[] _entryIndexes = new int[16];

	[SerializeField]
	private LayerMask _layerMask;

	private Camera _firstPersonCamera;

	private bool _has_firstPersonCamera;

	[SerializeField]
	private float m_LookPrecision = 0.65f;

	[SerializeField]
	private bool m_xrayVision;

	private LineRenderer _line;
}
