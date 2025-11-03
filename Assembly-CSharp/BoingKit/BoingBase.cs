using System;
using UnityEngine;

namespace BoingKit
{
	public class BoingBase : MonoBehaviour
	{
		public Version CurrentVersion
		{
			get
			{
				return this.m_currentVersion;
			}
		}

		public Version PreviousVersion
		{
			get
			{
				return this.m_previousVersion;
			}
		}

		public Version InitialVersion
		{
			get
			{
				return this.m_initialVersion;
			}
		}

		protected virtual void OnUpgrade(Version oldVersion, Version newVersion)
		{
			this.m_previousVersion = this.m_currentVersion;
			if (this.m_currentVersion.Revision < 33)
			{
				this.m_initialVersion = Version.Invalid;
				this.m_previousVersion = Version.Invalid;
			}
			this.m_currentVersion = newVersion;
		}

		[SerializeField]
		private Version m_currentVersion;

		[SerializeField]
		private Version m_previousVersion;

		[SerializeField]
		private Version m_initialVersion = BoingKit.Version;
	}
}
