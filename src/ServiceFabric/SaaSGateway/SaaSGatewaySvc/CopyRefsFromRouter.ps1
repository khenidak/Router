<#
    Pre Build Event
    Copy updated ref assemblies from Router solution
#>

$CurrentDirectory = $PSScriptRoot

$SrcDirectory = (get-item $CurrentDirectory).parent.parent.parent.FullName
$RefSourceFiles01 = $SrcDirectory + "\RouterLib.Owin\bin\Debug\*"
$RefSourceFiles02 = $SrcDirectory + "\HttpRouterLib\bin\Debug\*"

$RefTargetDirectory =  $SrcDirectory + "\ServiceFabric\SaaSGateway\SaaSGatewaySvc\bin\x64\Debug"

Write-Host "*******************************************************"
Write-Host "COPY Source Refs for SaaSGatewaySvc: Start"
Write-Host "From: $RefSourceFiles01"
Write-Host "To  : $RefTargetDirectory" 
Copy-Item  $RefSourceFiles01  $RefTargetDirectory -force -Verbose


Write-Host "From: $RefSourceFiles02"
Write-Host "To  : $RefTargetDirectory" 
Copy-Item  $RefSourceFiles02  $RefTargetDirectory -force -Verbose

Write-Host "COPY Source Refs for SaaSGatewaySvc: End"
Write-Host "*******************************************************"


