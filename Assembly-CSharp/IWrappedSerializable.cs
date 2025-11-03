using System;
using Fusion;
using Photon.Pun;

internal interface IWrappedSerializable : INetworkStruct
{
	void OnSerializeRead(object newData);

	void OnSerializeRead(PhotonStream stream, PhotonMessageInfo info);

	object OnSerializeWrite();

	void OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info);
}
