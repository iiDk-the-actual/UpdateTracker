using System;
using UnityEngine;

namespace GorillaNetworking
{
	public class SafeAccountObjectSwapper : MonoBehaviour
	{
		public void Start()
		{
			if (PlayFabAuthenticator.instance.GetSafety())
			{
				this.SwitchToSafeMode();
			}
			PlayFabAuthenticator instance = PlayFabAuthenticator.instance;
			instance.OnSafetyUpdate = (Action<bool>)Delegate.Combine(instance.OnSafetyUpdate, new Action<bool>(this.SafeAccountUpdated));
		}

		public void SafeAccountUpdated(bool isSafety)
		{
			if (isSafety)
			{
				this.SwitchToSafeMode();
			}
		}

		public void SwitchToSafeMode()
		{
			foreach (GameObject gameObject in this.UnSafeGameObjects)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(false);
				}
			}
			foreach (GameObject gameObject2 in this.UnSafeTexts)
			{
				if (gameObject2 != null)
				{
					gameObject2.SetActive(false);
				}
			}
			foreach (GameObject gameObject3 in this.SafeTexts)
			{
				if (gameObject3 != null)
				{
					gameObject3.SetActive(true);
				}
			}
			foreach (GameObject gameObject4 in this.SafeModeObjects)
			{
				if (gameObject4 != null)
				{
					gameObject4.SetActive(true);
				}
			}
		}

		public GameObject[] UnSafeGameObjects;

		public GameObject[] UnSafeTexts;

		public GameObject[] SafeTexts;

		public GameObject[] SafeModeObjects;
	}
}
