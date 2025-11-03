using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildSafe
{
	public abstract class SceneBakeTask : MonoBehaviour
	{
		public SceneBakeMode bakeMode
		{
			get
			{
				return this.m_bakeMode;
			}
			set
			{
				this.m_bakeMode = value;
			}
		}

		public virtual int callbackOrder
		{
			get
			{
				return this.m_callbackOrder;
			}
			set
			{
				this.m_callbackOrder = value;
			}
		}

		public bool runIfInactive
		{
			get
			{
				return this.m_runIfInactive;
			}
			set
			{
				this.m_runIfInactive = value;
			}
		}

		[Conditional("UNITY_EDITOR")]
		public abstract void OnSceneBake(Scene scene, SceneBakeMode mode);

		[Conditional("UNITY_EDITOR")]
		private void ForceRun()
		{
		}

		[SerializeField]
		private SceneBakeMode m_bakeMode;

		[SerializeField]
		private int m_callbackOrder;

		[Space]
		[SerializeField]
		private bool m_runIfInactive = true;
	}
}
