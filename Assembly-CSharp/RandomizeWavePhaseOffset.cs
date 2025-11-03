using System;
using UnityEngine;

public class RandomizeWavePhaseOffset : MonoBehaviour
{
	private void Start()
	{
		Material material = base.GetComponent<MeshRenderer>().material;
		UberShader.VertexWavePhaseOffset.SetValue<float>(material, Random.Range(this.minPhaseOffset, this.maxPhaseOffset));
	}

	[SerializeField]
	private float minPhaseOffset;

	[SerializeField]
	private float maxPhaseOffset;
}
