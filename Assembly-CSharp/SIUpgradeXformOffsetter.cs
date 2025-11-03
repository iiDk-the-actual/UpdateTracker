using System;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Serialization;

public class SIUpgradeXformOffsetter : MonoBehaviour
{
	protected void Awake()
	{
		if (this.m_superInfectionGadget == null)
		{
			Debug.LogError("[SIUpgradeXformOffsetter]  ERROR!!!  Awake: Disabling component because `m_superInfectionGadget` is null. Path=" + base.transform.GetPathQ(), this);
			base.enabled = false;
			return;
		}
		foreach (SIUpgradeXformOffsetter.SIUpgradeXformOffsetOp siupgradeXformOffsetOp in this.m_upgradeXformOffsetOps)
		{
			if (!(siupgradeXformOffsetOp.xform != null) && !(siupgradeXformOffsetOp.targetXform != null))
			{
				Debug.LogError("[SIUpgradeXformOffsetter]  ERROR!!!  Awake: Disabling component because null reference in `m_upgradeXformOffsetOps` array. Path=" + base.transform.GetPathQ(), this);
				base.enabled = false;
				return;
			}
		}
	}

	protected void OnEnable()
	{
		SIGadget superInfectionGadget = this.m_superInfectionGadget;
		superInfectionGadget.OnPostRefreshVisuals = (Action<SIUpgradeSet>)Delegate.Combine(superInfectionGadget.OnPostRefreshVisuals, new Action<SIUpgradeSet>(this._HandleGadgetOnPostRefreshVisuals));
	}

	protected void OnDisable()
	{
		SIGadget superInfectionGadget = this.m_superInfectionGadget;
		superInfectionGadget.OnPostRefreshVisuals = (Action<SIUpgradeSet>)Delegate.Remove(superInfectionGadget.OnPostRefreshVisuals, new Action<SIUpgradeSet>(this._HandleGadgetOnPostRefreshVisuals));
	}

	private void _HandleGadgetOnPostRefreshVisuals(SIUpgradeSet upgradeSet)
	{
		for (int i = 0; i < this.m_upgradeXformOffsetOps.Length; i++)
		{
			SIUpgradeXformOffsetter.SIUpgradeXformOffsetOp siupgradeXformOffsetOp = this.m_upgradeXformOffsetOps[i];
			if (upgradeSet.Contains(siupgradeXformOffsetOp.upgradeType))
			{
				siupgradeXformOffsetOp.xform.SetLocalPositionAndRotation(siupgradeXformOffsetOp.targetXform.localPosition, siupgradeXformOffsetOp.targetXform.localRotation);
				siupgradeXformOffsetOp.xform.localScale = siupgradeXformOffsetOp.targetXform.localScale;
			}
		}
	}

	private const string preLog = "[SIUpgradeXformOffsetter]  ";

	private const string preErr = "[SIUpgradeXformOffsetter]  ERROR!!!  ";

	[SerializeField]
	private SIGadget m_superInfectionGadget;

	[SerializeField]
	private SIUpgradeXformOffsetter.SIUpgradeXformOffsetOp[] m_upgradeXformOffsetOps;

	[Serializable]
	public struct SIUpgradeXformOffsetOp
	{
		public SIUpgradeType upgradeType;

		public Transform xform;

		[FormerlySerializedAs("newParent")]
		public Transform targetXform;
	}
}
