﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Statecharts.NET.Language.Transition;
using Statecharts.NET.Model;
using Statecharts.NET.Utilities;

namespace Statecharts.NET.Language.Service
{
    public delegate Task ServiceLogic(CancellationToken cancellationToken);
    public delegate Task<T> ServiceLogic<T>(CancellationToken cancellationToken);

    internal class DefinitionData
    {
        public ServiceLogic Task { get; }
        public string Id { get; set; }
        public OneOf<Definition.UnguardedTransition, Definition.UnguardedContextTransition> OnErrorTransition { get; set; }
        public OneOf<Definition.UnguardedTransition, Definition.UnguardedContextTransition> OnSuccessDefinition { get; set; }

        public DefinitionData(ServiceLogic task) => Task = task ?? throw new ArgumentNullException(nameof(task));
    }

    public class WithLogic : WithId
    {
        public WithLogic(ServiceLogic task) : base(task) { }

        public WithId WithId(string id)
        {
            DefinitionData.Id = id;
            return this;
        }
    }
    public class WithId : WithOnSuccessHandler
    {
        internal WithId(ServiceLogic task) : base(task) { }

        public WithOnSuccessHandler OnSuccess(Model.Target target) // TODO: add option to specify Actions
        {
            DefinitionData.OnSuccessDefinition = WithEvent.OnServiceSuccess().TransitionTo.Multiple(target);
            return this;
        }
    }
    public class WithOnSuccessHandler : WithOnErrorHandler
    {
        internal WithOnSuccessHandler(ServiceLogic task) : base(task) { }

        public WithOnErrorHandler OnError(Model.Target target) // TODO: add option to specify Actions
        {
            DefinitionData.OnErrorTransition = WithEvent.OnServiceError().TransitionTo.Multiple(target);
            return this;
        }
    }
    public class WithOnErrorHandler : TaskService
    {
        private protected DefinitionData DefinitionData { get; }

        internal WithOnErrorHandler(ServiceLogic logic) => DefinitionData = new DefinitionData(logic);

        public override Func<CancellationToken, Task> Task => async token => await DefinitionData.Task(token);
        public override string Id => DefinitionData.Id;
        public override OneOf<Definition.UnguardedTransition, Definition.UnguardedContextTransition> OnErrorTransition => DefinitionData.OnErrorTransition;
        public override OneOf<Definition.UnguardedTransition, Definition.UnguardedContextTransition> OnSuccessDefinition => DefinitionData.OnSuccessDefinition;
    }
}