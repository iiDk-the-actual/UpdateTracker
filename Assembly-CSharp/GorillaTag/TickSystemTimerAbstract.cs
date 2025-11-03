using System;
using System.Runtime.CompilerServices;

namespace GorillaTag
{
	[Serializable]
	internal abstract class TickSystemTimerAbstract : CoolDownHelper, ITickSystemPre
	{
		bool ITickSystemPre.PreTickRunning
		{
			get
			{
				return this.registered;
			}
			set
			{
				this.registered = value;
			}
		}

		public bool Running
		{
			get
			{
				return this.registered;
			}
		}

		protected TickSystemTimerAbstract()
		{
		}

		protected TickSystemTimerAbstract(float cd)
			: base(cd)
		{
		}

		public override void Start()
		{
			base.Start();
			TickSystem<object>.AddPreTickCallback(this);
		}

		public override void Stop()
		{
			base.Stop();
			TickSystem<object>.RemovePreTickCallback(this);
		}

		public override void OnCheckPass()
		{
			this.OnTimedEvent();
		}

		public abstract void OnTimedEvent();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ITickSystemPre.PreTick()
		{
			base.CheckCooldown();
		}

		[NonSerialized]
		internal bool registered;
	}
}
