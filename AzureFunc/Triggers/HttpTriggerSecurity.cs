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

namespace Azure_Functions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class AccessAllowRoleAttribute : Attribute
    {
        public string Role { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class AccessAllowModuleAttribute : Attribute
    {
        public string Module { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AccessAllowAllAttribute : Attribute
    {
    }
}
