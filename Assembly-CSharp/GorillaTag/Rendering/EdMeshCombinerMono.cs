using System;
using UnityEngine;

namespace GorillaTag.Rendering
{
	[DefaultExecutionOrder(-2147482648)]
	public class EdMeshCombinerMono : MonoBehaviour
	{
		protected void Awake()
		{
			Object.Destroy(this);
		}

		protected void OnEnable()
		{
		}
	}
}
