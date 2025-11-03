using System;
using System.Collections.Generic;
using GorillaTag;
using UnityEngine;

public class ScienceExperimentSceneElements : MonoBehaviour
{
	private void Awake()
	{
		ScienceExperimentManager.instance.InitElements(this);
	}

	private void OnDestroy()
	{
		ScienceExperimentManager.instance.DeInitElements();
	}

	public List<ScienceExperimentSceneElements.DisableByLiquidData> disableByLiquidList = new List<ScienceExperimentSceneElements.DisableByLiquidData>();

	public ParticleSystem sodaFizzParticles;

	public ParticleSystem sodaEruptionParticles;

	[Serializable]
	public struct DisableByLiquidData
	{
		public Transform target;

		public float heightOffset;
	}
}
