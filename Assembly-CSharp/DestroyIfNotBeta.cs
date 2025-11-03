using System;
using UnityEngine;

public class DestroyIfNotBeta : MonoBehaviour
{
	private void Awake()
	{
		bool shouldKeepIfBeta = this.m_shouldKeepIfBeta;
		bool shouldKeepIfCreatorBuild = this.m_shouldKeepIfCreatorBuild;
		Object.Destroy(base.gameObject);
	}

	public bool m_shouldKeepIfBeta = true;

	public bool m_shouldKeepIfCreatorBuild;
}
