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



#Add host entries for customer Xs plust the welcome page 
Import-Module .\psHosts\psHosts.psd1


Write-Host "Writing host entries in host files"

Add-HostEntry customer01.superdopersaas.com 127.0.0.1
Add-HostEntry customer02.superdopersaas.com 127.0.0.1

Add-HostEntry customer03.superdopersaas.com 127.0.0.1
Add-HostEntry customer04.superdopersaas.com 127.0.0.1
Add-HostEntry welcome.superdopersaas.com 127.0.0.1




#Copy Package & Register Apps
$CurrentDirectory = $PSScriptRoot
$BaseDirectory = (get-item $CurrentDirectory).parent.parent.FullName

Write-Host $BaseDirectory 


$GwAppTypeName = "SaaSGatewayType"
$GwAppTypeVersion = "1.0.0"
$GWPackageSource = $BaseDirectory + "\SaaSGateway\SaaSGateway\pkg\Debug\"

$SaaSTenantAppTypeName = "SaaSTenantAppType"
$SaaSTenantAppVersion = "1.0.0"
$SaaSAppPackageSource = $BaseDirectory + "\SaaSTenantApp\SaaSTenantApp\pkg\Debug\"


Write-Host "Deploying.. (The below will fail if you deploed the app using VS.NET, if so just use the portal and remove & unregister apps"

Connect-ServiceFabricCluster 
$ClusterMainfest =  Get-ServiceFabricClusterManifest
$imageStoreConnectionString = Get-ImageStoreConnectionStringFromClusterManifest -ClusterManifest $ClusterMainfest

# Tenante App
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $SaaSAppPackageSource `
									 -ImageStoreConnectionString $imageStoreConnectionString `
									 -ApplicationPackagePathInImageStore $SaaSTenantAppTypeName 

Register-ServiceFabricApplicationType -ApplicationPathInImageStore $SaaSTenantAppTypeName

# Create 4 Tenantes + default tenante


New-ServiceFabricApplication -ApplicationName "fabric:/saastenantapp01" `
							 -ApplicationTypeVersion $SaaSTenantAppVersion `
							 -ApplicationTypeName $SaaSTenantAppTypeName

New-ServiceFabricApplication -ApplicationName "fabric:/saastenantapp02" `
							 -ApplicationTypeVersion $SaaSTenantAppVersion `
							 -ApplicationTypeName $SaaSTenantAppTypeName

New-ServiceFabricApplication -ApplicationName "fabric:/saastenantapp03" `
							 -ApplicationTypeVersion $SaaSTenantAppVersion `
							 -ApplicationTypeName $SaaSTenantAppTypeName


New-ServiceFabricApplication -ApplicationName "fabric:/saastenantapp04" `
							 -ApplicationTypeVersion $SaaSTenantAppVersion `
							 -ApplicationTypeName $SaaSTenantAppTypeName

New-ServiceFabricApplication -ApplicationName "fabric:/saastenantappX" `
							 -ApplicationTypeVersion $SaaSTenantAppVersion `
							 -ApplicationTypeName $SaaSTenantAppTypeName

      

Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $GWPackageSource `
									 -ImageStoreConnectionString $imageStoreConnectionString `
									 -ApplicationPackagePathInImageStore $GwAppTypeName

#Create Gw App + Instance
Register-ServiceFabricApplicationType -ApplicationPathInImageStore $GwAppTypeName


New-ServiceFabricApplication -ApplicationName "fabric:/gw" `
							 -ApplicationTypeVersion $GwAppTypeVersion `
							 -ApplicationTypeName $GwAppTypeName 
