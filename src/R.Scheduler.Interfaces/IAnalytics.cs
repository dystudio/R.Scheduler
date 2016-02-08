﻿using System;
using System.Collections.Generic;
using Quartz;

namespace R.Scheduler.Interfaces
{
    public interface IAnalytics
    {
        int GetJobCount();
        int GetTriggerCount();
        IEnumerable<KeyValuePair<ITrigger, Guid>> GetFiredTriggers();
        IEnumerable<AuditLog> GetErroredJobs(int count);
        IEnumerable<AuditLog> GetExecutedJobs(int count);
    }
}