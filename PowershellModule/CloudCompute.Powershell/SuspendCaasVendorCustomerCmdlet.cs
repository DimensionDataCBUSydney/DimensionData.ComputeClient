﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SuspendCaasVendorCustomerCmdlet.cs" company="">
//   
// </copyright>
// <summary>
//   Creates a new Tenant on Caas. It returns the customer ID
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System;
using System.Management.Automation;
using System.Security;
using DD.CBU.Compute.Api.Client;
using DD.CBU.Compute.Api.Client.Vendor;
using DD.CBU.Compute.Api.Contracts.General;

namespace DD.CBU.Compute.Powershell
{
	/// <summary>
	/// Creates a new Tenant on Caas. It returns the customer ID
	/// </summary>
	[Cmdlet(VerbsLifecycle.Suspend, "CaasVendorCustomer")]
	[OutputType(typeof (Guid))]
	public class SuspendCaasVendorCustomerCmdlet : PsCmdletCaasVendorBase
	{
		/// <summary>
		/// Customer Id
		/// </summary>
		[Parameter(Mandatory = true, HelpMessage = "The id of the customer id", ValueFromPipeline = true)]
		public string CustomerId { get; set; }


		/// <summary>
		/// Primary administrator new password.
		/// </summary>
		[Parameter(Mandatory = true, HelpMessage = "Primary administrator new password.")]
		public SecureString NewPassword { get; set; }

		/// <summary>
		/// When cascadePassword is set to “true”, the new password value is also assigned to all SubAdministrators.
		/// </summary>
		[Parameter(Mandatory = true, 
			HelpMessage =
				"When cascadePassword is set to “true”, the new password value is also assigned to all SubAdministrators.")]
		public bool CascadePassword { get; set; }


		/// <summary>
		/// </summary>
		[Parameter(Mandatory = true, 
			HelpMessage = @"All Cloud Networks on the account have their public interfaces disconnected, preventing
                    any traffic from entering or leaving the Cloud Network. This essentially leaves a
                    customer’s Cloud Servers running, but they can no longer communicate with anything
                    except other Cloud Servers on the same network.Recommended set to $true")]
		public bool ShutdownAce { get; set; }


		/// <summary>
		/// The process record.
		/// </summary>
		protected override void ProcessRecord()
		{
			base.ProcessRecord();
			try
			{
				Guid customerGuid = Guid.Parse(CustomerId);
				Status status =
					Connection.ApiClient.SuspendCustomer(customerGuid, NewPassword.ToPlainString(), CascadePassword, 
						ShutdownAce).Result;

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
							WriteError(new ErrorRecord(e, "-2", ErrorCategory.InvalidOperation, Connection));
						}
						else
						{
// if (e is HttpRequestException)
							ThrowTerminatingError(new ErrorRecord(e, "-1", ErrorCategory.ConnectionError, Connection));
						}

						return true;
					});
			}
		}
	}
}