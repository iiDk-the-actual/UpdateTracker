using System;
using UnityEngine;

public class MetaAuthenticator : MonoBehaviour
{
	public int MaxAppEntitlementCheckAttempts = 5;

	public int MaxGetUserNonceAttempts = 5;

	public int MaxGetDeviceAttestationTokenAttempts = 5;
}
