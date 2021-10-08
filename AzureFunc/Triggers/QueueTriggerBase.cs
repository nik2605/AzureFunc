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

using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Azure_Functions
{
    /// <summary>
    /// These generic classes are used to instantiate queue triggers in custom AppInsights context
    /// </summary>
    public abstract class QueueTriggerBase
    {
        public string TriggerName => GetType().Name;
        private ODMTraceWriter GetODMTraceWriter(TraceWriter existingWriter, string operation_ParentId, string operation_Id)
        {
            return ODMTraceWriter.Create(existingWriter, TriggerName, operation_ParentId, operation_Id);
        }
        protected void InLoggerContext(TraceWriter log, string operation_ParentId, string operation_Id, Action<ODMTraceWriter> action)
        {
            var writer = GetODMTraceWriter(log, operation_ParentId, operation_Id);
            writer.StartRequest(TriggerName);
            var success = false;
            try
            {
                action(writer);
                success = true;
            }
            catch (Exception ex)
            {
                writer.Error($"{TriggerName} Queue trigger failed.", ex);
                throw;
            }
            finally
            {
                writer.StopRequest(null, success);
            }
        }
    }

    public abstract class QueueTriggerBase<T1> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, l));
    }

    public abstract class QueueTriggerBase<T1, T2> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, l));
    }

    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, l));
    }
    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, l));
    }
    /// <summary>
    /// If you have more than 19 parameters in an queue trigger, extend this class yourself, edit and move this comment there, also don't forget to add an UT in QueueTriggerBaseTests.cs
    /// </summary>
    public abstract class QueueTriggerBase<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> : QueueTriggerBase
    {
        protected abstract void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, ODMTraceWriter log);
        protected void Invoke(TraceWriter log, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, string operation_ParentId = null, string operation_Id = null)
            => InLoggerContext(log, operation_ParentId, operation_Id, l => Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, l));
    }

}
