﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using DD.CBU.Compute.Api.Client;
using DD.CBU.Compute.Api.Client.Network;
using DD.CBU.Compute.Api.Contracts.Network;

namespace DD.CBU.Compute.Powershell
{
    [Cmdlet(VerbsCommon.New,"CaasNetworkPublicIpBlock")]
    [OutputType(typeof(IpBlockType[]))]
    public class NewCaasNetworkPublicIpBlockCmdlet:PsCmdletCaasBase
    {
        /// <summary>
        /// The network to add the public ip addresses
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The network to add the public ip addresses", ValueFromPipeline = true)]
        public NetworkWithLocationsNetwork Network { get; set; }



        /// <summary>
        /// The network to add the public ip addresses
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Return the IpBlockType object")]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            try
            {
              
                var status = CaaS.ApiClient.ReserveNetworkPublicIpAddressBlock(Network.id).Result;
                if (status != null)
                {
                    if (status.additionalInformation != null && PassThru.IsPresent)
                    {

                        var ipblock = new IpBlockType();


                        var ipblockid = status.additionalInformation.Single(info => info.name == "ipBlock.id");
                        if (ipblockid != null)
                            ipblock.id = ipblockid.value;

                        var baseip = status.additionalInformation.Single(info => info.name == "ipBlock.baseIp");
                        if (baseip != null)
                            ipblock.baseIp = baseip.value;

                        var blocksize = status.additionalInformation.Single(info => info.name == "ipBlock.size");
                        if (blocksize != null)
                            ipblock.subnetSize = int.Parse(blocksize.value);

                        ipblock.serverToVipConnectivity = false;
                        WriteObject(ipblock);



                    }
                    WriteDebug(
                        string.Format(
                            "{0} resulted in {1} ({2}): {3}",
                            status.operation,
                            status.result,
                            status.resultCode,
                            status.resultDetail));


                }

            }
            catch (AggregateException ae)
            {
                ae.Handle(
                    e =>
                    {
                        if (e is ComputeApiException)
                        {
                            WriteError(new ErrorRecord(e, "-2", ErrorCategory.InvalidOperation, CaaS));
                        }
                        else //if (e is HttpRequestException)
                        {
                            ThrowTerminatingError(new ErrorRecord(e, "-1", ErrorCategory.ConnectionError, CaaS));
                        }
                        return true;
                    });
            }
        }
    }
}