using System;
using System.Collections.Generic;

namespace GorillaTagScripts.AI
{
	public class StateMachine
	{
		public void Tick()
		{
			StateMachine.Transition transition = this.GetTransition();
			if (transition != null)
			{
				this.SetState(transition.To);
			}
			IState currentState = this._currentState;
			if (currentState == null)
			{
				return;
			}
			currentState.Tick();
		}

		public void SetState(IState state)
		{
			if (state == this._currentState)
			{
				return;
			}
			IState currentState = this._currentState;
			if (currentState != null)
			{
				currentState.OnExit();
			}
			this._currentState = state;
			this._transitions.TryGetValue(this._currentState.GetType(), out this._currentTransitions);
			if (this._currentTransitions == null)
			{
				this._currentTransitions = StateMachine.EmptyTransitions;
			}
			this._currentState.OnEnter();
		}

		public IState GetState()
		{
			return this._currentState;
		}

		public void AddTransition(IState from, IState to, Func<bool> predicate)
		{
			List<StateMachine.Transition> list;
			if (!this._transitions.TryGetValue(from.GetType(), out list))
			{
				list = new List<StateMachine.Transition>();
				this._transitions[from.GetType()] = list;
			}
			list.Add(new StateMachine.Transition(to, predicate));
		}

		public void AddAnyTransition(IState state, Func<bool> predicate)
		{
			this._anyTransitions.Add(new StateMachine.Transition(state, predicate));
		}

		private StateMachine.Transition GetTransition()
		{
			foreach (StateMachine.Transition transition in this._anyTransitions)
			{
				if (transition.Condition())
				{
					return transition;
				}
			}
			foreach (StateMachine.Transition transition2 in this._currentTransitions)
			{
				if (transition2.Condition())
				{
					return transition2;
				}
			}
			return null;
		}

		private IState _currentState;

		private Dictionary<Type, List<StateMachine.Transition>> _transitions = new Dictionary<Type, List<StateMachine.Transition>>();

		private List<StateMachine.Transition> _currentTransitions = new List<StateMachine.Transition>();

		private List<StateMachine.Transition> _anyTransitions = new List<StateMachine.Transition>();

		private static List<StateMachine.Transition> EmptyTransitions = new List<StateMachine.Transition>(0);

		private class Transition
		{
			public Func<bool> Condition { get; }

			public IState To { get; }

			public Transition(IState to, Func<bool> condition)
			{
				this.To = to;
				this.Condition = condition;
			}
		}
	}
}
