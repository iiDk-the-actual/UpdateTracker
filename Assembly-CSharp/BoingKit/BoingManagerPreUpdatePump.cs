using System;
using UnityEngine;

namespace BoingKit
{
	public class BoingManagerPreUpdatePump : MonoBehaviour
	{
		private void FixedUpdate()
		{
			this.TryPump();
		}

		private void Update()
		{
			this.TryPump();
		}

		private void TryPump()
		{
			if (this.m_lastPumpedFrame >= Time.frameCount)
			{
				return;
			}
			if (this.m_lastPumpedFrame >= 0)
			{
				this.DoPump();
			}
			this.m_lastPumpedFrame = Time.frameCount;
		}

		private void DoPump()
		{
			BoingManager.RestoreBehaviors();
			BoingManager.RestoreReactors();
			BoingManager.RestoreBones();
			BoingManager.DispatchReactorFieldCompute();
		}

		private int m_lastPumpedFrame = -1;
	}
}
