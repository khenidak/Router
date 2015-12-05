#From Service Fabric SDK..
function Get-ImageStoreConnectionStringFromClusterManifest
{
    <#
    .SYNOPSIS 
    Returns the value of the image store connection string from the cluster manifest.

    .PARAMETER ClusterManifest
    Contents of cluster manifest file.
    #>

    [CmdletBinding()]
    Param
    (
        [xml]
        $ClusterManifest
    )

    $managementSection = $ClusterManifest.ClusterManifest.FabricSettings.Section | ? { $_.Name -eq "Management" }
    return $managementSection.ChildNodes | ? { $_.Name -eq "ImageStoreConnectionString" } | Select-Object -Expand Value
}

#remove host entries for customer Xs plust the welcome page 
Import-Module .\psHosts\psHosts.psd1


Write-Host "removing host entries in host files"



Remove-HostEntry customer01.superdopersaas.com 
Remove-HostEntry customer02.superdopersaas.com 

Remove-HostEntry customer03.superdopersaas.com 
Remove-HostEntry customer04.superdopersaas.com 
Remove-HostEntry welcome.superdopersaas.com 


Write-Host "Removing Service Fabric App Instances, Types & Packages"


Connect-ServiceFabricCluster 
$ClusterMainfest =  Get-ServiceFabricClusterManifest
$imageStoreConnectionString = Get-ImageStoreConnectionStringFromClusterManifest -ClusterManifest $ClusterMainfest


#Remove Service Fabrics App Instances
$GwAppTypeName = "SaaSGatewayType"
$GwAppTypeVersion = "1.0.0"

$SaaSTenantAppTypeName = "SaaSTenantAppType"
$SaaSTenantAppVersion = "1.0.0"

Remove-ServiceFabricApplication -ApplicationName "fabric:/saastenantapp01" -Force
Remove-ServiceFabricApplication -ApplicationName "fabric:/saastenantapp02" -Force
Remove-ServiceFabricApplication -ApplicationName "fabric:/saastenantapp03" -Force
Remove-ServiceFabricApplication -ApplicationName "fabric:/saastenantapp04" -Force
Remove-ServiceFabricApplication -ApplicationName "fabric:/saastenantappX"-Force
Remove-ServiceFabricApplication -ApplicationName "fabric:/gw" -Force


Unregister-ServiceFabricApplicationType -ApplicationTypeName $GwAppTypeName  `
									    -ApplicationTypeVersion $GwAppTypeVersion `
										-Force



Unregister-ServiceFabricApplicationType  -ApplicationTypeName $SaaSTenantAppTypeName  `
									    -ApplicationTypeVersion $SaaSTenantAppVersion `
										-Force

Remove-ServiceFabricApplicationPackage -ApplicationPackagePathInImageStore $GwAppTypeName `
									   -ImageStoreConnectionString $imageStoreConnectionString

									   
Remove-ServiceFabricApplicationPackage -ApplicationPackagePathInImageStore $SaaSTenantAppTypeName `
									   -ImageStoreConnectionString $imageStoreConnectionString