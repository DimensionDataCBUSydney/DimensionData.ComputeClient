#
# Powershell module manifest for the Compute Client for Dimension Data CaaS.
#

New-Alias New-CaasVM New-CaasServer
New-Alias Remove-CaasVM Remove-CaasServer
New-Alias Add-CaasNatRule New-CaasNatRule
New-Alias Add-CaasAclRule New-CaasAclRule
New-Alias Out-CaasWaitForOperation Wait-CaasServerOperation

#following singular name commands, backward compatibility
New-Alias Get-CaasNetworks Get-CaasNetwork
New-Alias Get-CaasServers Get-CaasServer
New-Alias Get-CaasCustomerImages Get-CaasCustomerImage
New-Alias Get-CaasOsImages Get-CaasOsImage

