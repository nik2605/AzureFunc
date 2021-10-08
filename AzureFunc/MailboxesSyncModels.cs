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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quest.MMD.FunctionModels.Common;
using Quest.MMD.FunctionModels.ResourceProcessing;

namespace Functions.MailboxesSyncStart
{
    public class StartMailboxesSyncRequest : ICustomerRequest
    {
        public string CustomerId { get; set; }
        public string ProjectId { get; set; }
        public AzureAdConnection Source { get; set; }
        public MailConnection SourceServer { get; set; }
        public AzureAdConnection Target { get; set; }
        public MailConnection TargetServer { get; set; }
        public SyncFilter SyncFilter { get; set; }
        public List<MailboxPair> Mailboxes { get; set; }
        public MigrationFilter MigrationFilter { get; set; }
    }

    public class SyncFilter
    {
        public string SyncToArchive { get; set; }
        public string SyncFromArchive { get; set; }
        public bool SyncEmail { get; set; }
        public bool SyncCalendar { get; set; }
        public bool SyncContacts { get; set; }
        public bool SyncTasks { get; set; }
        public bool SyncRules { get; set; }
        public bool SyncDelete { get; set; }
    }

    public class MailSyncState
    {
        public string FolderHierarchySyncState { get; set; }
        public List<FolderSyncState> FolderSyncStates { get; set; }
    }

    public class FolderSyncState
    {
        public string SourceFolderId { get; set; }
        public string MatchingFolderID { get; set; }
        public string SyncState { get; set; }
    }
}