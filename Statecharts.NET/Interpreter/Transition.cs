﻿using System;
using System.Collections.Generic;
using System.Linq;
using Statecharts.NET.Utilities;

namespace Statecharts.NET.Interpreter
{
    public class InitialTransition
    {
        public InitialTransition(StateNode target, IEnumerable<OneOf<Model.Action, Model.ContextAction>> actions)
        {
            Target = target;
            Actions = actions;
        }

        public StateNode Target { get; }
        public IEnumerable<OneOf<Model.Action, Model.ContextAction>> Actions { get; }
    }

    public abstract class Transition : OneOfBase<ForbiddenTransition, UnguardedTransition, GuardedTransition>
    {
        public StateNode Source { get; }

        internal Transition(StateNode source) => Source = source ?? throw new ArgumentNullException(nameof(source));

        public IEnumerable<StateNode> GetTargets() =>
            this.Match(
                forbidden => Enumerable.Empty<StateNode>(),
                unguarded => unguarded.Targets,
                guarded => guarded.Targets);
    }

    public class ForbiddenTransition : Transition
    {
        public Model.CustomEvent Event { get; }
        public ForbiddenTransition(StateNode source, Model.CustomEvent @event) : base(source) =>
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
    }
    public class UnguardedTransition : Transition
    {
        public OneOf<Model.Event, Model.CustomDataEvent> Event { get; }
        public IEnumerable<StateNode> Targets { get; }
        public IEnumerable<OneOf<Model.Action, Model.ContextAction, Model.ContextDataAction>> Actions { get; }

        public UnguardedTransition(
            StateNode source,
            OneOf<Model.Event, Model.CustomDataEvent> @event,
            IEnumerable<StateNode> targets,
            IEnumerable<OneOf<Model.Action, Model.ContextAction, Model.ContextDataAction>> actions) : base(source)
        {
            if(@event.Equals(null)) throw new ArgumentNullException(nameof(@event));
            Event = @event;
            Targets = targets ?? throw new ArgumentNullException(nameof(targets));
            Actions = actions ?? Enumerable.Empty<OneOf<Model.Action, Model.ContextAction, Model.ContextDataAction>>();
        }
    }
    public class GuardedTransition : Transition
    {
        public OneOf<Model.Event, Model.CustomDataEvent> Event { get; }
        public OneOf<Model.InStateGuard, Model.ConditionContextGuard, Model.ConditionContextDataGuard> Guard { get; }
        public IEnumerable<StateNode> Targets { get; }
        public IEnumerable<OneOf<Model.Action, Model.ContextAction, Model.ContextDataAction>> Actions { get; }

        public GuardedTransition(
            StateNode source,
            OneOf<Model.Event, Model.CustomDataEvent> @event,
            OneOf<Model.InStateGuard, Model.ConditionContextGuard, Model.ConditionContextDataGuard> guard,
            IEnumerable<StateNode> targets,
            IEnumerable<OneOf<Model.Action, Model.ContextAction, Model.ContextDataAction>> actions) : base(source)
        {
            if (@event.Equals(null)) throw new ArgumentNullException(nameof(@event));
            if (guard.Equals(null)) throw new ArgumentNullException(nameof(@event));
            Event = @event;
            Guard = guard;
            Targets = targets ?? throw new ArgumentNullException(nameof(targets));
            Actions = actions ?? Enumerable.Empty<OneOf<Model.Action, Model.ContextAction, Model.ContextDataAction>>();
        }
    }
}
