using System;
using UnityEngine;

namespace com.AnotherAxiom.SpaceFight
{
	public class SpaceFight : ArcadeGame
	{
		private void Update()
		{
			for (int i = 0; i < 2; i++)
			{
				if (base.getButtonState(i, ArcadeButtons.UP))
				{
					this.move(this.player[i], 0.15f);
					this.clamp(this.player[i]);
				}
				if (base.getButtonState(i, ArcadeButtons.RIGHT))
				{
					this.turn(this.player[i], true);
				}
				if (base.getButtonState(i, ArcadeButtons.LEFT))
				{
					this.turn(this.player[i], false);
				}
				if (this.projectilesFired[i])
				{
					this.move(this.projectile[i], 0.5f);
					if (Vector2.Distance(this.player[1 - i].localPosition, this.projectile[i].localPosition) < 0.25f)
					{
						base.PlaySound(1, 2);
						this.player[1 - i].Rotate(0f, 0f, 180f);
						this.projectilesFired[i] = false;
					}
					if (Mathf.Abs(this.projectile[i].localPosition.x) > this.tableSize.x || Mathf.Abs(this.projectile[i].localPosition.y) > this.tableSize.y)
					{
						this.projectilesFired[i] = false;
					}
				}
				if (!this.projectilesFired[i])
				{
					this.projectile[i].position = this.player[i].position;
					this.projectile[i].rotation = this.player[i].rotation;
				}
			}
		}

		private void clamp(Transform tr)
		{
			tr.localPosition = new Vector2(Mathf.Clamp(tr.localPosition.x, -this.tableSize.x, this.tableSize.x), Mathf.Clamp(tr.localPosition.y, -this.tableSize.y, this.tableSize.y));
		}

		protected override void ButtonDown(int player, ArcadeButtons button)
		{
			if (button == ArcadeButtons.TRIGGER)
			{
				if (!this.projectilesFired[player])
				{
					base.PlaySound(0, 3);
				}
				this.projectilesFired[player] = true;
			}
		}

		private void move(Transform p, float speed)
		{
			p.Translate(p.up * Time.deltaTime * speed, Space.World);
		}

		private void turn(Transform p, bool cw)
		{
			p.Rotate(0f, 0f, (float)(cw ? 180 : (-180)) * Time.deltaTime);
		}

		public override byte[] GetNetworkState()
		{
			this.netStateCur.P1LocX = this.player[0].localPosition.x;
			this.netStateCur.P1LocY = this.player[0].localPosition.y;
			this.netStateCur.P1Rot = this.player[0].localRotation.eulerAngles.z;
			this.netStateCur.P2LocX = this.player[1].localPosition.x;
			this.netStateCur.P2LocY = this.player[1].localPosition.y;
			this.netStateCur.P2Rot = this.player[1].localRotation.eulerAngles.z;
			this.netStateCur.P1PrLocX = this.projectile[0].localPosition.x;
			this.netStateCur.P1PrLocY = this.projectile[0].localPosition.y;
			this.netStateCur.P2PrLocX = this.projectile[1].localPosition.x;
			this.netStateCur.P2PrLocY = this.projectile[1].localPosition.y;
			if (!this.netStateCur.Equals(this.netStateLast))
			{
				this.netStateLast = this.netStateCur;
				base.SwapNetStateBuffersAndStreams();
				ArcadeGame.WrapNetState(this.netStateLast, this.netStateMemStream);
			}
			return this.netStateBuffer;
		}

		public override void SetNetworkState(byte[] b)
		{
			SpaceFight.SpaceFlightNetState spaceFlightNetState = (SpaceFight.SpaceFlightNetState)ArcadeGame.UnwrapNetState(b);
			this.player[0].localPosition = new Vector2(spaceFlightNetState.P1LocX, spaceFlightNetState.P1LocY);
			this.player[0].localRotation = Quaternion.Euler(0f, 0f, spaceFlightNetState.P1Rot);
			this.player[1].localPosition = new Vector2(spaceFlightNetState.P2LocX, spaceFlightNetState.P2LocY);
			this.player[1].localRotation = Quaternion.Euler(0f, 0f, spaceFlightNetState.P2Rot);
			this.projectile[0].localPosition = new Vector2(spaceFlightNetState.P1PrLocX, spaceFlightNetState.P1PrLocY);
			this.projectile[1].localPosition = new Vector2(spaceFlightNetState.P2PrLocX, spaceFlightNetState.P2PrLocY);
		}

		protected override void ButtonUp(int player, ArcadeButtons button)
		{
		}

		public override void OnTimeout()
		{
		}

		[SerializeField]
		private Transform[] player;

		[SerializeField]
		private Transform[] projectile;

		[SerializeField]
		private Vector2 tableSize;

		private bool[] projectilesFired = new bool[2];

		private SpaceFight.SpaceFlightNetState netStateLast;

		private SpaceFight.SpaceFlightNetState netStateCur;

		[Serializable]
		private struct SpaceFlightNetState : IEquatable<SpaceFight.SpaceFlightNetState>
		{
			public bool Equals(SpaceFight.SpaceFlightNetState other)
			{
				return this.P1LocX.Approx(other.P1LocX, 1E-06f) && this.P1LocY.Approx(other.P1LocY, 1E-06f) && this.P1Rot.Approx(other.P1Rot, 1E-06f) && this.P2LocX.Approx(other.P2LocX, 1E-06f) && this.P2LocY.Approx(other.P2LocY, 1E-06f) && this.P1Rot.Approx(other.P1Rot, 1E-06f) && this.P1PrLocX.Approx(other.P1PrLocX, 1E-06f) && this.P1PrLocY.Approx(other.P1PrLocY, 1E-06f) && this.P2PrLocX.Approx(other.P2PrLocX, 1E-06f) && this.P2PrLocY.Approx(other.P2PrLocY, 1E-06f);
			}

			public float P1LocX;

			public float P1LocY;

			public float P1Rot;

			public float P2LocX;

			public float P2LocY;

			public float P2Rot;

			public float P1PrLocX;

			public float P1PrLocY;

			public float P2PrLocX;

			public float P2PrLocY;
		}
	}
}
