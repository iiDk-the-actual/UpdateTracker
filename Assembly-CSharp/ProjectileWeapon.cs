using System;
using GorillaExtensions;
using UnityEngine;

public abstract class ProjectileWeapon : TransferrableObject
{
	protected abstract Vector3 GetLaunchPosition();

	protected abstract Vector3 GetLaunchVelocity();

	internal override void OnEnable()
	{
		base.OnEnable();
		if (base.myOnlineRig != null)
		{
			base.myOnlineRig.projectileWeapon = this;
		}
		if (base.myRig != null)
		{
			base.myRig.projectileWeapon = this;
		}
	}

	protected void LaunchProjectile()
	{
		int num = PoolUtils.GameObjHashCode(this.projectilePrefab);
		int num2 = PoolUtils.GameObjHashCode(this.projectileTrail);
		GameObject gameObject = ObjectPools.instance.Instantiate(num, true);
		float num3 = Mathf.Abs(base.transform.lossyScale.x);
		gameObject.transform.localScale = Vector3.one * num3;
		Vector3 launchPosition = this.GetLaunchPosition();
		Vector3 launchVelocity = this.GetLaunchVelocity();
		bool flag;
		bool flag2;
		bool flag3;
		this.GetIsOnTeams(out flag, out flag2, out flag3);
		this.AttachTrail(num2, gameObject, launchPosition, flag, flag2, flag3 && this.targetRig, this.targetRig ? this.targetRig.playerColor : default(Color));
		SlingshotProjectile component = gameObject.GetComponent<SlingshotProjectile>();
		if (NetworkSystem.Instance.InRoom)
		{
			int num4 = ProjectileTracker.AddAndIncrementLocalProjectile(component, launchVelocity, launchPosition, num3);
			component.Launch(launchPosition, launchVelocity, NetworkSystem.Instance.LocalPlayer, flag, flag2, num4, num3, flag3, base.myRig.playerColor);
			TransferrableObject.PositionState currentState = this.currentState;
			RoomSystem.SendLaunchProjectile(launchPosition, launchVelocity, RoomSystem.ProjectileSource.ProjectileWeapon, num4, false, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.PlayLaunchSfx();
		}
		else
		{
			component.Launch(launchPosition, launchVelocity, NetworkSystem.Instance.LocalPlayer, flag, flag2, 0, num3, flag3, base.myRig.playerColor);
			this.PlayLaunchSfx();
		}
		PlayerGameEvents.LaunchedProjectile(this.projectilePrefab.name);
	}

	internal virtual SlingshotProjectile LaunchNetworkedProjectile(Vector3 location, Vector3 velocity, RoomSystem.ProjectileSource projectileSource, int projectileCounter, float scale, bool shouldOverrideColor, Color color, PhotonMessageInfoWrapped info)
	{
		GameObject gameObject = null;
		SlingshotProjectile slingshotProjectile = null;
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		try
		{
			int num = -1;
			int num2 = -1;
			if (projectileSource == RoomSystem.ProjectileSource.ProjectileWeapon)
			{
				if (this.currentState == TransferrableObject.PositionState.OnChest || this.currentState == TransferrableObject.PositionState.None)
				{
					return null;
				}
				num = PoolUtils.GameObjHashCode(this.projectilePrefab);
				num2 = PoolUtils.GameObjHashCode(this.projectileTrail);
			}
			gameObject = ObjectPools.instance.Instantiate(num, true);
			slingshotProjectile = gameObject.GetComponent<SlingshotProjectile>();
			bool flag;
			bool flag2;
			bool flag3;
			this.GetIsOnTeams(out flag, out flag2, out flag3);
			if (flag3 && !shouldOverrideColor && this.targetRig)
			{
				shouldOverrideColor = true;
				color = this.targetRig.playerColor;
			}
			if (num2 != -1)
			{
				this.AttachTrail(num2, slingshotProjectile.gameObject, location, flag, flag2, shouldOverrideColor, color);
			}
			slingshotProjectile.Launch(location, velocity, player, flag, flag2, projectileCounter, scale, shouldOverrideColor, color);
			this.PlayLaunchSfx();
		}
		catch
		{
			GorillaNot.instance.SendReport("projectile error", player.UserId, player.NickName);
			if (slingshotProjectile != null && slingshotProjectile)
			{
				slingshotProjectile.transform.position = Vector3.zero;
				slingshotProjectile.Deactivate();
				slingshotProjectile = null;
			}
			else if (gameObject.IsNotNull())
			{
				ObjectPools.instance.Destroy(gameObject);
			}
		}
		return slingshotProjectile;
	}

	protected void GetIsOnTeams(out bool blueTeam, out bool orangeTeam, out bool shouldUsePlayerColor)
	{
		NetPlayer netPlayer = base.OwningPlayer();
		blueTeam = false;
		orangeTeam = false;
		shouldUsePlayerColor = false;
		if (GorillaGameManager.instance != null)
		{
			GorillaPaintbrawlManager component = GorillaGameManager.instance.GetComponent<GorillaPaintbrawlManager>();
			if (component != null)
			{
				blueTeam = component.OnBlueTeam(netPlayer);
				orangeTeam = component.OnRedTeam(netPlayer);
				shouldUsePlayerColor = !blueTeam && !orangeTeam;
			}
		}
	}

	private void AttachTrail(int trailHash, GameObject newProjectile, Vector3 location, bool blueTeam, bool orangeTeam, bool shouldOverrideColor = false, Color overrideColor = default(Color))
	{
		GameObject gameObject = ObjectPools.instance.Instantiate(trailHash, true);
		SlingshotProjectileTrail component = gameObject.GetComponent<SlingshotProjectileTrail>();
		if (component.IsNull())
		{
			ObjectPools.instance.Destroy(gameObject);
		}
		newProjectile.transform.position = location;
		component.AttachTrail(newProjectile, blueTeam, orangeTeam, shouldOverrideColor, overrideColor);
	}

	private void PlayLaunchSfx()
	{
		if (this.shootSfx != null && this.shootSfxClips != null && this.shootSfxClips.Length != 0)
		{
			this.shootSfx.GTPlayOneShot(this.shootSfxClips[Random.Range(0, this.shootSfxClips.Length)], 1f);
		}
	}

	[SerializeField]
	protected GameObject projectilePrefab;

	[SerializeField]
	private GameObject projectileTrail;

	public AudioClip[] shootSfxClips;

	public AudioSource shootSfx;
}
