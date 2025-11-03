using System;
using System.Collections.Generic;
using Photon.Pun;

[Serializable]
public class ViewsAndAllocator
{
	public List<PhotonView> views;

	public string path;

	public int order;

	public bool isStatic;
}
