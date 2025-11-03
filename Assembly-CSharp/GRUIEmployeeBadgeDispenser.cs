using System;
using TMPro;
using UnityEngine;

public class GRUIEmployeeBadgeDispenser : MonoBehaviour
{
	public void Setup(GhostReactor reactor, int employeeIndex)
	{
		this.reactor = reactor;
	}

	public void Refresh()
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(this.actorNr);
		if (player != null && player.InRoom)
		{
			this.playerName.text = player.SanitizedNickName;
			if (this.idBadge != null)
			{
				this.idBadge.RefreshText(player);
				return;
			}
		}
		else
		{
			this.playerName.text = "";
		}
	}

	public void CreateBadge(NetPlayer player, GameEntityManager entityManager)
	{
		if (entityManager.IsAuthority())
		{
			entityManager.RequestCreateItem(this.idBadgePrefab.name.GetStaticHash(), this.spawnLocation.position, this.spawnLocation.rotation, (long)(player.ActorNumber * 100 + this.index));
		}
	}

	public Transform GetSpawnMarker()
	{
		return this.spawnLocation;
	}

	public bool IsDispenserForBadge(GRBadge badge)
	{
		return badge == this.idBadge;
	}

	public Vector3 GetSpawnPosition()
	{
		return this.spawnLocation.position;
	}

	public Quaternion GetSpawnRotation()
	{
		return this.spawnLocation.rotation;
	}

	public void ClearBadge()
	{
		this.actorNr = -1;
		this.idBadge = null;
	}

	public void AttachIDBadge(GRBadge linkedBadge, NetPlayer _player)
	{
		this.actorNr = ((_player == null) ? (-1) : _player.ActorNumber);
		this.idBadge = linkedBadge;
		this.playerName.text = ((_player == null) ? null : _player.SanitizedNickName);
		this.idBadge.Setup(_player, this.index);
	}

	[SerializeField]
	private TMP_Text msg;

	[SerializeField]
	private TMP_Text playerName;

	[SerializeField]
	private Transform spawnLocation;

	[SerializeField]
	private GameEntity idBadgePrefab;

	[SerializeField]
	private LayerMask badgeLayerMask;

	public int index;

	public int actorNr;

	public GRBadge idBadge;

	private GhostReactor reactor;

	private Coroutine getSpawnedBadgeCoroutine;

	private static Collider[] overlapColliders = new Collider[10];

	private bool isEmployee;

	private const string GR_DATA_KEY = "GRData";
}
