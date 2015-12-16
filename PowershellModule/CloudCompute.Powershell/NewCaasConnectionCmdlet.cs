﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewCaasConnectionCmdlet.cs" company="">
//   
// </copyright>
// <summary>
//   The "New-CaasConnection" Cmdlet.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Management.Automation;
using System.Net.FtpClient;
using System.Threading.Tasks;
using DD.CBU.Compute.Api.Client;

namespace DD.CBU.Compute.Powershell
{
	/// <summary>
	///     The "New-CaasConnection" Cmdlet.
	/// </summary>
	/// <remarks>
	///     Used to create a new connection to the CaaS API.
	/// </remarks>
	[Cmdlet(VerbsCommon.New, "CaasConnection")]
	[OutputType(typeof(ComputeServiceConnection))]
	public class NewCaasConnectionCmdlet : PSCmdletCaasBase
	{
		/// <summary>
		///     The credentials used to connect to the CaaS API.
		/// </summary>
		[Parameter(Mandatory = true, ValueFromPipeline = true)]
		[ValidateNotNullOrEmpty]
		public PSCredential ApiCredentials { get; set; }

		/// <summary>
		///     Name for this connection
		/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "Name to identify this connection")]
		public string Name { get; set; }

		/// <summary>
		///     The known vendor for the connection
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = "KnownApiUri", 
			HelpMessage = "A known cloud vendor for the Cloud API Uri. Not all vendor and region combinations are valid.")]
		[PSDefaultValue(Help = "Dimension Data is the default value", Value = KnownApiVendor.DimensionData)]
		public KnownApiVendor Vendor { get; set; }


		/// <summary>
		///     The known region for the connection
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = "KnownApiUri", 
			HelpMessage = "A known cloud region for the Cloud API Uri. Not all vendor and region combinations are valid.")]
		public KnownApiRegion Region { get; set; }

		/// <summary>
		///     The base uri of the REST API
		/// </summary>		
		[Parameter(Mandatory = true, ParameterSetName = "ApiDomainName", HelpMessage = "The domain name for the REST API")]
		public string ApiDomainName { get; set; }

        /// <summary>
		///     The base uri of the FTP domain
		/// </summary>		
		[Parameter(Mandatory = false, ParameterSetName = "ApiDomainName", HelpMessage = "The domain name for the FTP, default is the api domain name")]
        public string FtpDomainName { get; set; }

        /// <summary>
        ///     Process the record
        /// </summary>
        protected override void ProcessRecord()
		{
			base.ProcessRecord();

			ComputeServiceConnection newCloudComputeConnection = null;


			WriteDebug("Trying to login to the REST API");
			try
			{
				newCloudComputeConnection = LoginTask().Result;
				if (newCloudComputeConnection != null)
				{
					WriteDebug(string.Format("CaaS connection created successfully: {0}", newCloudComputeConnection));
					if (string.IsNullOrEmpty(Name))
					{
						Name = Guid.NewGuid().ToString();
						WriteWarning(
							string.Format("Connection name not specified. Therefore this connection name will be a random GUID: {0}", Name));
					}

					if (!SessionState.GetComputeServiceConnections().Any())
						WriteDebug("This is the first connection and will be the default connection.");
					SessionState.AddComputeServiceConnection(Name, newCloudComputeConnection);
					if (SessionState.GetComputeServiceConnections().Count > 1)
						WriteWarning(
							"You have created more than one connection on this session, please use the cmdlet Set-CaasActiveConnection -Name <name> to change the active/default connection");

					SessionState.AddComputeServiceConnection(Name, newCloudComputeConnection);
					WriteObject(newCloudComputeConnection);
				}
			}
			catch (AggregateException ae)
			{
				ae.Handle(
					e =>
					{
						ThrowTerminatingError(new ErrorRecord(e, "-1", ErrorCategory.AuthenticationError, newCloudComputeConnection));
						return true;
					});
			}
		}

		/// <summary>
		///     Try to login into the account using the credentials.
		///     If succeed, it will return the account details.
		/// </summary>
		/// <returns>
		///     The CaaS connection
		/// </returns>
		private async Task<ComputeServiceConnection> LoginTask()
		{
			string ftpHost = string.Empty;
			ComputeApiClient apiClient = null;

			if (ParameterSetName == "KnownApiUri")
			{
				apiClient = ComputeApiClient.GetComputeApiClient(Vendor, Region, ApiCredentials.GetNetworkCredential());
				ftpHost = ComputeApiClient.GetFtpHost(Vendor, Region);
			}

			if (ParameterSetName == "ApiDomainName")
			{
                Uri baseUri;
                // Support ApiDomainName containing https://
			    if (Uri.TryCreate(ApiDomainName, UriKind.Absolute, out baseUri))
			    {
                    ftpHost = baseUri.Host;
                }
                else
                { 
                    // Support ApiDomainName as in just the domainName
			        baseUri = new Uri(string.Format("https://{0}/", ApiDomainName));
                    ftpHost = ApiDomainName;
                }

                // Handle explicit FTP host name
			    if (!String.IsNullOrWhiteSpace(FtpDomainName))
			    {
                    ftpHost = FtpDomainName;
                    Uri ftpUri;
			        if (Uri.TryCreate(FtpDomainName, UriKind.Absolute, out ftpUri))
			        {
			            ftpHost = ftpUri.Host;
			        }
			    }

			    apiClient = ComputeApiClient.GetComputeApiClient(baseUri, ApiCredentials.GetNetworkCredential());				
			}

			var newCloudComputeConnection = new ComputeServiceConnection(apiClient);

			WriteDebug("Trying to login into the CaaS");
			newCloudComputeConnection.Account = await newCloudComputeConnection.ApiClient.Login();

			// Right now we dont need to do a connect, as ftp is used in only a few commands
			newCloudComputeConnection.FtpClient = new FtpClient
			{
				Host = ftpHost, 
				EncryptionMode = FtpEncryptionMode.Explicit, 
				DataConnectionEncryption = true, 
				Credentials = ApiCredentials.GetNetworkCredential()
					.GetCredential(new Uri(string.Format("ftp://{0}", ftpHost)), "Basic")
			};

			return newCloudComputeConnection;
		}
	}
}