using System;
using UnityEngine;

namespace GorillaTag
{
	public class DestroyOnAwake : MonoBehaviour
	{
		protected void Awake()
		{
			try
			{
				Object.Destroy(base.gameObject);
			}
			catch
			{
			}
		}

		protected void OnEnable()
		{
			try
			{
				Object.Destroy(base.gameObject);
			}
			catch
			{
			}
		}

		protected void Update()
		{
			try
			{
				Object.Destroy(base.gameObject);
			}
			catch
			{
			}
		}
	}
}
