using System;
using UnityEngine;

internal abstract class RPCNetworkBase : MonoBehaviour
{
	public abstract void SetClassTarget(IWrappedSerializable target, GorillaWrappedSerializer netHandler);
}
