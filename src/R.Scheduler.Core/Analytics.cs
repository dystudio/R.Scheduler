﻿using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Core
{
    /// <summary>
    /// Provides analytical data about scheduled jobs
    /// </summary>
    public class Analytics : IAnalytics
    {
        private readonly IScheduler _scheduler;
        private readonly IPersistanceStore _persistanceStore;

        public Analytics(IScheduler scheduler, IPersistanceStore persistanceStore)
        {
            _scheduler = scheduler;
            _persistanceStore = persistanceStore;
        }

        /// <summary>
        /// Get number of job setup in scheduler
        /// </summary>
        /// <returns></returns>
        public int GetJobCount()
        {
            return _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).Count;
        }

        /// <summary>
        /// Get number of triggers setup in scheduler
        /// </summary>
        /// <returns></returns>
        public int GetTriggerCount()
        {
            return _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()).Count;
        }

        /// <summary>
        /// Get currently executing triggers mapped to trigger ids
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<ITrigger, Guid>> GetFiredTriggers()
        {
            IDictionary<ITrigger, Guid> retval = new Dictionary<ITrigger, Guid>();
            IEnumerable<TriggerKey> firedTriggers = _persistanceStore.GetFiredTriggers();

            foreach (var firedTrigger in firedTriggers)
            {
                var triggerId = _persistanceStore.GetTriggerId(firedTrigger);
                retval.Add(_scheduler.GetTrigger(firedTrigger), triggerId);
            }

            return retval;
        }

        /// <summary>
        /// Get a specified number of most recently failed jobs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetErroredJobs(int count)
        {
            return _persistanceStore.GetErroredJobs(count);
        }

        /// <summary>
        /// Get a specified number of most recently executed jobs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetExecutedJobs(int count)
        {
            return _persistanceStore.GetExecutedJobs(count);
        }

        /// <summary>
        /// Get a specified number of upcoming jobs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<FireInstance> GetUpcomingJobs(int count)
        {
            IList<FireInstance> temp = new List<FireInstance>();

            var allTriggerKeys = _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            foreach (var triggerKey in allTriggerKeys)
            {
                ITrigger trigger = _scheduler.GetTrigger(triggerKey);

                ICalendar cal = null;
                if (!string.IsNullOrEmpty(trigger.CalendarName))
                {
                    cal = _scheduler.GetCalendar(trigger.CalendarName);
                }
                var fireTimes = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, cal, count);

                foreach (var dateTimeOffset in fireTimes)
                {
                    temp.Add(new FireInstance
                    {
                        FireTimeUtc = dateTimeOffset,
                        JobName = trigger.JobKey.Name,
                        JobGroup = trigger.JobKey.Group,
                        TriggerName = trigger.Key.Name,
                        TriggerGroup = trigger.Key.Group,
                        JobId = _persistanceStore.GetJobId(trigger.JobKey)
                    });
                }
            }

            IList<FireInstance> retval = temp.OrderBy(i => i.FireTimeUtc).Take(count).ToList();

            return retval;
        }
    }
}
