Function Test-CaasDockerContainer {
	# This function serves as a template for deploying your own process for Docker.

	#capture the Caas credentials and create a new CaaS connection
	$login = Get-Credential -Message "Enter CaaS API credentials"
	New-CaasConnection -ApiCredentials $login -Vendor DimensionData -Region Australia_AU

	New-CaasDockerHost -Name "Test-DockerHost" -NetworkName "ANT-WEB" -RootPassword "unicorndatapass"

	Invoke-CaasDockerContainer -Name "tomcat7" -Parameters "-p 8080:8080 -d" -NetworkName "ANT-WEB" -RootPassword "unicorndatapass"
}