# Microservices SaaS Scenario (Based on Azure Service Fabric) #

The scenario is as the following:
1. Each tenant is an Azure Service Fabric application instance.
2. A separate Service Fabric Application (gateway) is responsible for routing the users' requests based on their tenant assignment. The gateway is running HttpRouter, HttpRouter.Owin + a custom matcher to perform the routing logic,
3. The routing logic is as the following:
  * User browse to <tenant>.superdopersaas.com (scripts adds host entry in hosts file for this).
  * The gateway uses HttpRouter + Matchers to 1) identify user's tenant [based on subdomain] 2) map the tenant to service fabric application instance [using static information] 3) Resolve the tenant listening address [using Service Fabric API] 4) execute and route the route the request.


### Running The Scenario ###
1. Open Router.ServiceFabric.sln in /src/ServiceFabric directory
3. Find SFRouter.Tests testing project.
4. Browse to Ps folder of the project.
5. Run SetupClusterForTest.ps1 [as admin] Which
  * Deploys the packages to local clusters
  * Creates 4 Service Fabric applications tenantapp01 to 04
  * Creates additional default tenant with the name tenantappX.
  * Creates the Gateway application instance.
  * Creates the needed host entries in local hosts files in /%windows directory%/syste32/drivers/etc/hosts  
6. Run the tests in the test project
7. optionally Use your browser to browse to customer01.superdopersaas.com to customer04.superdopersaas.com (each tenant is routed to a different service fabric application), the response will include the application name responded to your request.
8. Browse to welcome.superdopersaas.com which will be routed to with the default tenant.    
9. Clean up the cluster by running CleanupClusterAfterTest.ps1

> if you have control over your network's DNS, configure it with superdopersaas.com DNS A Record. You can test then with <any address>.superdopersaas.com which will be routed to the default tenant. You will not be able to do that local hosts file.
