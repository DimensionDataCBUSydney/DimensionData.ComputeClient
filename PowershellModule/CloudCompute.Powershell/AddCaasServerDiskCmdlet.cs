﻿using System.Globalization;
using DD.CBU.Compute.Api.Contracts.Server;

namespace DD.CBU.Compute.Powershell
{
    using System;
    using System.Management.Automation;
    using System.Linq;
    using DD.CBU.Compute.Api.Client;
    using System.Collections.Generic;

    /// <summary>
    /// The new CaaS Virtual Machine cmdlet.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "CaasServerDisk")]
    public class AddCaasServerDiskCmdlet : PsCmdletCaasServerBase
    {
       
        
        [Parameter(Mandatory = true, HelpMessage = "The new disk size")]
        public int SizeInGB { get; set; }


        [Parameter(Mandatory = false, ParameterSetName = "SpeedId", HelpMessage = "The speedId of the new disk. The available speed Id can be retrieved using (Get-CaasDataCentre).hypervisor.diskSpeed")]
        public string SpeedId { get; set; }


        [Parameter(Mandatory = false, ParameterSetName = "DiskSpeedType", HelpMessage = "The disk speed to be created")]
        public DiskSpeedType Speed { get; set; }

     
        /// <summary>
        /// The process record method.
        /// </summary>
        protected override void ProcessRecord()
        {


            try
            {
                if (ParameterSetName.Equals("DiskSpeedType"))
                    SpeedId = Speed.ToString();
                var status = Connection.ApiClient.AddServerDisk(Server.id, SizeInGB.ToString(CultureInfo.InvariantCulture),SpeedId).Result;
                if (status != null)
                    WriteDebug(
                        string.Format(
                            "{0} resulted in {1} ({2}): {3}",
                            status.operation,
                            status.result,
                            status.resultCode,
                            status.resultDetail));


                base.ProcessRecord();
            }
            catch (AggregateException ae)
            {
                ae.Handle(
                    e =>
                    {
                        if (e is ComputeApiException)
                        {
                            WriteError(new ErrorRecord(e, "-2", ErrorCategory.InvalidOperation, Connection));
                        }
                        else //if (e is HttpRequestException)
                        {
                            ThrowTerminatingError(new ErrorRecord(e, "-1", ErrorCategory.ConnectionError, Connection));
                        }
                        return true;
                    });
            }

        }

        
    }
}