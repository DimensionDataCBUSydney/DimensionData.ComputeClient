﻿namespace DD.CBU.Compute.Powershell
{
    using System;
    using System.Management.Automation;

    using DD.CBU.Compute.Api.Client;

    /// <summary>
    /// The set server state cmdlet.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "CaasServerCloneToCustomerImage")]
    public class NewCaasServerCloneToCustomerImageCmdlet : PsCmdletCaasServerBase
    {
        /// <summary>
        /// Customer Image name
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Set the customer image name. Note that the Customer Image name is required to be unique in a given data center.")]
        public string Name { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Set the customer image description")]
        public string Description { get; set; }   
        /// <summary>
        /// The process record method.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                var status = CaaS.ApiClient.ServerCloneToCustomerImage(Server.id,Name,Description).Result;
                if (status != null)
                    WriteDebug(
                        string.Format(
                            "{0} resulted in {1} ({2}): {3}",
                            status.operation,
                            status.result,
                            status.resultCode,
                            status.resultDetail));
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
     
            base.ProcessRecord();

        }


    }
}