﻿using System;
using System.Collections.Generic;
using System.Linq;
using Statecharts.NET.Definition;
using Statecharts.NET.Language.Service;
using Statecharts.NET.Utilities;
using static Statecharts.NET.Language.Keywords;

namespace Statecharts.NET.Language.StateNode
{
    internal class DefinitionData
    {
        public string Name { get; }
        public IEnumerable<Action> EntryActions { get; set; }
        public IEnumerable<Action> ExitActions { get; set; }
        public IEnumerable<TransitionDefinition> Transitions { get; set; }
        public IEnumerable<IActivity> Activities { get; set; }
        public IEnumerable<IBaseServiceDefinition> Services { get; set; }
        public InitialTransitionDefinition InitialTransition { get; set; }
        public IEnumerable<IBaseStateNodeDefinition> States { get; set; }

        public DefinitionData(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public class WithName : WithEntryActions
    {
        public WithName(string name) : base(name) { }

        public WithEntryActions WithEntryActions(Action action, params Action[] actions)
        {
            DefinitionData.EntryActions = action.Append(actions);
            return this;
        }
    }
    public class WithEntryActions : WithExitActions
    {
        internal WithEntryActions(string name) : base(name) { }

        public WithExitActions WithExitActions(Action action, params Action[] actions)
        {
            DefinitionData.ExitActions = action.Append(actions);
            return this;
        }
    }
    public class WithExitActions : WithTransitions
    {
        internal WithExitActions(string name) : base(name) { }

        public WithTransitions WithTransitions(TransitionDefinition transitionDefinition, params TransitionDefinition[] transitionDefinitions)
        {
            DefinitionData.Transitions = transitionDefinition.Append(transitionDefinitions);
            return this;
        }
    }
    public class WithTransitions : WithActivities
    {
        internal WithTransitions(string name) : base(name) { }

        public WithActivities WithActivities(IActivity activity, params IActivity[] activities)
        {
            DefinitionData.Activities = activity.Append(activities);
            return this;
        }
    }

    public class WithActivities : WithServices
    {
        internal WithActivities(string name) : base(name) { }

        public WithServices WithServices(
            OneOf<ServiceLogic, IBaseServiceDefinition> service,
            params OneOf<ServiceLogic, IBaseServiceDefinition>[] services)
        {
            DefinitionData.Services = service.Append(services).Select(
                definition => definition.Match(
                    logic => new WithLogic(logic),
                    valid => valid));
            return this;
        }
    }
    public class WithServices : IAtomicStateNodeDefinition
    {
        private protected DefinitionData DefinitionData { get; }

        internal WithServices(string name) => DefinitionData = new DefinitionData(name);

        public string Name => DefinitionData.Name;
        public IEnumerable<TransitionDefinition> Transitions => DefinitionData.Transitions;
        public IEnumerable<Action> EntryActions => DefinitionData.EntryActions;
        public IEnumerable<Action> ExitActions => DefinitionData.ExitActions;
        public IEnumerable<IActivity> Activities => DefinitionData.Activities;
        public IEnumerable<IBaseServiceDefinition> Services => DefinitionData.Services;
        public InitialTransitionDefinition InitialTransition => DefinitionData.InitialTransition;
        public IEnumerable<IBaseStateNodeDefinition> States => DefinitionData.States;

        public Final AsFinal() => new Final(DefinitionData);
        public Compound AsCompound() => new Compound(DefinitionData);
        public Orthogonal AsOrthogonal() => new Orthogonal(DefinitionData);
    }

    public class Final : IFinalStateNodeDefinition
    {
        private DefinitionData DefinitionData { get; }

        internal Final(DefinitionData data)
            => DefinitionData = data;

        public string Name => DefinitionData.Name;
        public IEnumerable<TransitionDefinition> Transitions => DefinitionData.Transitions;
        public IEnumerable<Action> EntryActions => DefinitionData.EntryActions;
        public IEnumerable<Action> ExitActions => DefinitionData.ExitActions;
        public IEnumerable<IActivity> Activities => DefinitionData.Activities;
    }

    public class Compound
    {
        internal DefinitionData DefinitionData { get; }

        internal Compound(DefinitionData data)
            => DefinitionData = data;

        public CompoundWithInitialState WithInitialState(string stateName)
        {
            DefinitionData.InitialTransition = new InitialTransitionDefinition { Target = Child(stateName) }; // TODO: change to builder pattern or ctor
            return new CompoundWithInitialState(this);
        }
    }
    public class CompoundWithInitialState
    {
        internal DefinitionData DefinitionData { get; }

        internal CompoundWithInitialState(Compound compound)
            => DefinitionData = compound.DefinitionData;

        public CompoundWithInitialActions WithInitialActions(Action action, params Action[] actions)
        {
            DefinitionData.InitialTransition.Actions = action.Append(actions);
            return new CompoundWithInitialActions(this);
        }

        public CompoundWithStates WithStates(
            OneOf<string, IBaseStateNodeDefinition> state,
            params OneOf<string, IBaseStateNodeDefinition>[] states)
        {
            DefinitionData.States = state.Append(states).Select(
                definition => definition.Match(name => new WithName(name), valid => valid));
            return new CompoundWithStates(this);
        }
    }
    public class CompoundWithInitialActions
    {
        internal DefinitionData DefinitionData { get; }

        internal CompoundWithInitialActions(CompoundWithInitialState compound)
            => DefinitionData = compound.DefinitionData;

        public CompoundWithStates WithStates(
            OneOf<string, IBaseStateNodeDefinition> state,
            params OneOf<string, IBaseStateNodeDefinition>[] states)
        {
            DefinitionData.States = state.Append(states).Select(
                definition => definition.Match(name => new WithName(name), valid => valid));
            return new CompoundWithStates(this);
        }
    }
    public class CompoundWithStates : ICompoundStateNodeDefinition
    {
        private DefinitionData DefinitionData { get; }

        internal CompoundWithStates(CompoundWithInitialState compoundWithInitialState)
            => DefinitionData = compoundWithInitialState.DefinitionData;
        internal CompoundWithStates(CompoundWithInitialActions compoundWithInitialActions)
            => DefinitionData = compoundWithInitialActions.DefinitionData;

        public string Name => DefinitionData.Name;
        public IEnumerable<TransitionDefinition> Transitions => DefinitionData.Transitions;
        public IEnumerable<Action> EntryActions => DefinitionData.EntryActions;
        public IEnumerable<Action> ExitActions => DefinitionData.ExitActions;
        public IEnumerable<IActivity> Activities => DefinitionData.Activities;
        public IEnumerable<IBaseServiceDefinition> Services => DefinitionData.Services;
        public InitialTransitionDefinition InitialTransition => DefinitionData.InitialTransition;
        public IEnumerable<IBaseStateNodeDefinition> States => DefinitionData.States;
    }

    public class Orthogonal
    {
        internal DefinitionData DefinitionData { get; }

        internal Orthogonal(DefinitionData data)
            => DefinitionData = data;

        public OrthogonalWithStates WithStates(
            OneOf<string, IBaseStateNodeDefinition> state,
            params OneOf<string, IBaseStateNodeDefinition>[] states)
        {
            DefinitionData.States = state.Append(states).Select(
                definition => definition.Match(name => new WithName(name), valid => valid));
            return new OrthogonalWithStates(this);
        }
    }
    public class OrthogonalWithStates : IOrthogonalStateNodeDefinition
    {
        private DefinitionData DefinitionData { get; }

        internal OrthogonalWithStates(Orthogonal orthogonal)
            => DefinitionData = orthogonal.DefinitionData;

        public string Name => DefinitionData.Name;
        public IEnumerable<TransitionDefinition> Transitions => DefinitionData.Transitions;
        public IEnumerable<Action> EntryActions => DefinitionData.EntryActions;
        public IEnumerable<Action> ExitActions => DefinitionData.ExitActions;
        public IEnumerable<IActivity> Activities => DefinitionData.Activities;
        public IEnumerable<IBaseServiceDefinition> Services => DefinitionData.Services;
        public IEnumerable<IBaseStateNodeDefinition> States => DefinitionData.States;
    }
}
