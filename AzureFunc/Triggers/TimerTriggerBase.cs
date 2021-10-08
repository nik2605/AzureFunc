// QUEST SOFTWARE PROPRIETARY INFORMATION
//
// This software is confidential. Quest Software Inc., or one of its
// subsidiaries, has supplied this software to you under terms of a
// license agreement, nondisclosure agreement or both.
//
// You may not copy, disclose, or use this software except in
// accordance with those terms.
//
// COPYRIGHT 2021 Quest Software Inc.
// ALL RIGHTS RESERVED.
//
// QUEST SOFTWARE MAKES NO REPRESENTATIONS OR
// WARRANTIES ABOUT THE SUITABILITY OF THE SOFTWARE,
// EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.
// QUEST SOFTWARE SHALL NOT BE LIABLE FOR ANY DAMAGES
// SUFFERED BY LICENSEE AS A RESULT OF USING, MODIFYING
// OR DISTRIBUTING THIS SOFTWARE OR ITS DERIVATIVES.

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Azure_Functions
{
    /// <summary>
    /// These generic classes are used to instantiate timer triggers in custom AppInsights context
    /// </summary>
    public abstract class TimerTriggerBase
    {
        public string TriggerName => GetType().Name;
        private ODMTraceWriter GetODMTraceWriter(TraceWriter existingWriter)
        {
            return ODMTraceWriter.Create(existingWriter, TriggerName, null, null);
        }
        protected void InLoggerContext(TraceWriter log, Action<ODMTraceWriter> action)
        {
            var writer = GetODMTraceWriter(log);
            writer.StartRequest(TriggerName);
            var success = false;
            try
            {
                action(writer);
                success = true;
            }
            catch (Exception ex)
            {
                writer.Error($"{TriggerName} Timer trigger failed.", ex);
                throw;
            }
            finally
            {
                writer.StopRequest(null, success);
            }
        }
    }

    public abstract class TimerTriggerBase<T1> : TimerTriggerBase
    {
        protected abstract void Invoke(TimerInfo timerInfo, T1 p1, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, TimerInfo timerInfo, T1 p1)
            => InLoggerContext(log, l => Invoke(timerInfo, p1, l));
    }

    /// <summary>
    /// If you have more than 2 parameters in a timer trigger, extend this class yourself, edit and move this comment there, also don't forget to add an UT in TimerTriggerBaseTests.cs
    /// </summary>
    public abstract class TimerTriggerBase<T1, T2> : TimerTriggerBase
    {
        protected abstract void Invoke(TimerInfo timerInfo, T1 p1, T2 p2, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, TimerInfo timerInfo, T1 p1, T2 p2)
            => InLoggerContext(log, l => Invoke(timerInfo, p1, p2, l));
    }
}
