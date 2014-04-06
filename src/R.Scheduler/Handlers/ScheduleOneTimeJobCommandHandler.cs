﻿using Quartz;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.JobRunners;

namespace R.Scheduler.Handlers
{
    public class ScheduleOneTimeJobCommandHandler : IMessageHandler<ScheduleOneTimeJobCommand>
    {
        readonly IPluginStore _pluginStore;

        public ScheduleOneTimeJobCommandHandler(IPluginStore pluginStore)
        {
            _pluginStore = pluginStore;
        }

        public void Execute(ScheduleOneTimeJobCommand command)
        {
            var registeredPlugin = _pluginStore.GetRegisteredPlugin(command.PluginName);

            IJobDetail jobDetail = JobBuilder.Create<PluginRunner>()
                .WithIdentity(command.JobName, command.GroupName)
                .StoreDurably(false)
                .Build();

            jobDetail.JobDataMap.Add("pluginPath", registeredPlugin.AssemblyPath);

            var trigger = (ISimpleTrigger) TriggerBuilder.Create()
                .WithIdentity(command.TriggerName, command.GroupName)
                .StartAt(command.RunDateTime)
                .Build();

            IScheduler sched = Scheduler.Instance();
            sched.ScheduleJob(jobDetail, trigger);
        }
    }
}
