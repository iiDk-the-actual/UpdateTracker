using System;
using GorillaExtensions;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class GorillaStatusToThermalTemperatureMono : MonoBehaviour, ISpawnable
{
	public bool hasRig { get; private set; }

	public VRRig rig
	{
		get
		{
			return this.m_rig;
		}
	}

	public void SetRig(VRRig newRig)
	{
		if (newRig == this.m_rig)
		{
			return;
		}
		if (this.hasRig)
		{
			VRRig rig = this.m_rig;
			rig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Remove(rig.OnMaterialIndexChanged, new Action<int, int>(this._OnMatChanged));
		}
		this.m_rig = newRig;
		this.hasRig = newRig != null;
		if (!this.hasRig || !base.isActiveAndEnabled)
		{
			return;
		}
		VRRig rig2 = this.m_rig;
		rig2.OnMaterialIndexChanged = (Action<int, int>)Delegate.Combine(rig2.OnMaterialIndexChanged, new Action<int, int>(this._OnMatChanged));
		this._InitRuntimeArray();
		this._OnMatChanged(-1, this.m_rig.setMatIndex);
	}

	protected void Awake()
	{
		this.hasRig = this.m_rig != null;
		this._InitRuntimeArray();
	}

	private void _InitRuntimeArray()
	{
		if (!this.hasRig || this._runtimeMatIndexes_to_temperatures != null)
		{
			return;
		}
		int num = VRRig.LocalRig.materialsToChangeTo.Length;
		this._runtimeMatIndexes_to_temperatures = new float[num];
		for (int i = 0; i < this._runtimeMatIndexes_to_temperatures.Length; i++)
		{
			this._runtimeMatIndexes_to_temperatures[i] = -32768f;
		}
		foreach (GorillaStatusToThermalTemperatureMono._MaterialIndexToTemperature materialIndexToTemperature in this.m_materialIndexesToTemperatures)
		{
			foreach (int num2 in materialIndexToTemperature.matIndexes)
			{
				if (num2 >= 0 && num2 < num)
				{
					this._runtimeMatIndexes_to_temperatures[num2] = materialIndexToTemperature.temperature;
				}
			}
		}
		if (!Application.isEditor)
		{
			this.m_materialIndexesToTemperatures = null;
		}
	}

	protected void OnEnable()
	{
		if (!this.hasRig || ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (this.m_thermalSourceVolume == null)
		{
			GTDev.LogError<string>("[GorillaStatusToThermalTemperatureMono]  ERROR!!!  Disabling because thermal source is not assigned. Path=" + base.transform.GetPathQ(), this, null);
			base.enabled = false;
			return;
		}
		VRRig rig = this.m_rig;
		rig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Combine(rig.OnMaterialIndexChanged, new Action<int, int>(this._OnMatChanged));
		this._OnMatChanged(-1, this.m_rig.setMatIndex);
	}

	protected void OnDisable()
	{
		if (ApplicationQuittingState.IsQuitting || !this.hasRig)
		{
			return;
		}
		VRRig rig = this.m_rig;
		rig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Remove(rig.OnMaterialIndexChanged, new Action<int, int>(this._OnMatChanged));
	}

	private void _OnMatChanged(int oldIndex, int newIndex)
	{
		float num = this._runtimeMatIndexes_to_temperatures[newIndex];
		this.m_thermalSourceVolume.celsius = num;
		this.m_thermalSourceVolume.enabled = num > -32767.99f;
	}

	public bool IsSpawned { get; set; }

	public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

	public void OnSpawn(VRRig newRig)
	{
		this.SetRig(newRig);
	}

	public void OnDespawn()
	{
		this.SetRig(null);
	}

	private const string preLog = "[GorillaStatusToThermalTemperatureMono]  ";

	private const string preErr = "[GorillaStatusToThermalTemperatureMono]  ERROR!!!  ";

	[Tooltip("Should either be assigned here or via another script.")]
	[SerializeField]
	private VRRig m_rig;

	[SerializeField]
	private ThermalSourceVolume m_thermalSourceVolume;

	[SerializeField]
	private GorillaStatusToThermalTemperatureMono._MaterialIndexToTemperature[] m_materialIndexesToTemperatures;

	[DebugReadout]
	private float[] _runtimeMatIndexes_to_temperatures;

	private const float _k_invalidTemperature = -32768f;

	[Serializable]
	private struct _MaterialIndexToTemperature
	{
		public int[] matIndexes;

		public float temperature;
	}
}
