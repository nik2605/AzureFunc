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

//using Functions.DomainCoexistenceDeploy;
using System.Collections.Generic;

namespace Functions.ResourceManagerHelper
{
    public interface IAzureDeployer
    {
        void CreateResourceGroup(string resourceGroupName, string region, Dictionary<string, string> tags = null);
        string Deploy(string deployingName, string resourceGroupName, string template, string parameters);

        DeploymentInfo GetDeploymentInfo(string deploymentId);

        //AzureVirtualNetwork GetVirtualNetwork(string resourceGroupName, string vnetName);
        void CreateSubnet(string resourceGroupName, string vnetName, string subnetName, string subnetPrefix, string networkSecurityGroup);
    }
}
