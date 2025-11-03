using System;
using System.Runtime.CompilerServices;

namespace GorillaTag
{
	[Serializable]
	internal class TickSystemTimer : TickSystemTimerAbstract
	{
		public TickSystemTimer()
		{
		}

		public TickSystemTimer(float cd)
			: base(cd)
		{
		}

		public TickSystemTimer(float cd, Action cb)
			: base(cd)
		{
			this.callback = cb;
		}

		public TickSystemTimer(Action cb)
		{
			this.callback = cb;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void OnTimedEvent()
		{
			Action action = this.callback;
			if (action == null)
			{
				return;
			}
			action();
		}

		public Action callback;
	}
}
