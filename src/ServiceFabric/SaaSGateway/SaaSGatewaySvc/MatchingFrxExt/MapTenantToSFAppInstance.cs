using HttpRouterLib;
using Newtonsoft.Json.Linq;
using RouterLib;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaaSGatewaySvc
{
    /// <summary>
    /// My custom SaaS matcher that maps in addres to 
    /// tenante specific address. 
    /// </summary>
    class MatchHostAddressForTenant : MatcherBase
    {
        private const string SvcName = "WebUxSvc";
        // cache key for every app per matcher tree id
        private const string Cache_Key_Format = "Resolve_Tenante_{0}_{1}";

        // This data is typically stored in the auto-scale and/or 
        //billing systems and/or provisioning component/controller.
        private static IReadOnlyDictionary<string, string> MapDomainsToTenants =
            new Dictionary<string, string>()
            {
                { "customer01", "saastenantapp01"},
                { "customer02", "saastenantapp02"},
                { "customer03", "saastenantapp03"},
                { "customer04", "saastenantapp04"},
            };

        // route all unknown customer to this tenant, maybe for users who
        // don't know thier usernames, tenantname, forgot passwords etc.
        private const string default_tenant_name = "saastenantappX";

        private string GetMapedAppNameForTenant(string sTenantDomain)
        {
            if (MapDomainsToTenants.ContainsKey(sTenantDomain))
                return MapDomainsToTenants[sTenantDomain];

            return default_tenant_name;
        }

        private async Task<Uri> GetListeningAddressForWebUxSvc(RoutingContextBase routingContext, 
                                                         Uri inTenantAsFabricAppSvcName)
        {
            ResolvedServicePartition resolvedPartition = null;

            // find if we have previously saved results
            var actualkey = string.Format(Cache_Key_Format, MatcherTreeId, inTenantAsFabricAppSvcName.ToString());
            var Resolver = routingContext.Resolver;

            if(Resolver.State.StateEntries.ContainsKey(actualkey))
                resolvedPartition  = Resolver.State.StateEntries[actualkey] as ResolvedServicePartition;


            // get Address from Service Fabric
            // we are expecting services to have one singlton partition
            FabricClient fc = new FabricClient();
            resolvedPartition = await fc.ServiceManager.ResolveServicePartitionAsync(inTenantAsFabricAppSvcName, resolvedPartition);

            // cache resolved service partition.
            routingContext.Resolver.State.StateEntries[actualkey] = resolvedPartition;

            // get the address Service Fabric returns json with all address (we are expecting just one entry named http)
            var jsonAddress = JObject.Parse(resolvedPartition.GetEndpoint().Address);
            var svcListeningAddress = (string)jsonAddress["Endpoints"]["http"];

            return new Uri(svcListeningAddress);
        }
        public MatchHostAddressForTenant()
        {

        }

        public int SaaSTenantApp { get; private set; }

        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                    string sAddress,
                                                    IDictionary<string, object> Context,
                                                    Stream Body)
        {
            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

           
            /*
             incoming requests are at <tenantname>.<some host with/without port>.com/<path>
             note: 
                   in this case tenant mapping is statically defined in this clsas
                   in a typical production you probably have this as a map in external service or a store
             note: 
                  in this case i am routing to single service with single instane, but yours might be different, same logic apply 
             Logic:
                 1- get request path
                 2- get tenante name 
                 3- use the tenante name to specific app intsance and WebUXSvc listening url from Service Fabric
                 generate a new url as the following: 
                    <service fabric service listening url including path>/<incoming path + query string>

            */
            var httpCtx = routingContext as HttpRoutingContext; 
            var inUri = new Uri(sAddress);
             
            var inPathQueryString = inUri.PathAndQuery; 
            
            // trim first /
            if (inPathQueryString.Length > 0 && inPathQueryString[0] == '/')
                inPathQueryString = inPathQueryString.Substring(1);

            // Service Fabric Application Name Uri is case senstive
            var inTenantName = inUri.DnsSafeHost.Split('.')[0].ToLowerInvariant();
            var mappedAppName = GetMapedAppNameForTenant(inTenantName);

            var inTenantAsFabricAppSvcName = new Uri(string.Concat("fabric:/", mappedAppName, "/", SvcName));

            var outUri = await GetListeningAddressForWebUxSvc(routingContext, inTenantAsFabricAppSvcName); 
            
            // get service host name from listening address (either nodename or nodename:port
            var outHostName = string.Concat(outUri.Host, !outUri.IsDefaultPort ? string.Concat(":", outUri.Port.ToString()) : string.Empty);


            // setup routing context
            routingContext.TargetHostAddressList.Clear();
            routingContext.TargetHostAddressList.Add(outHostName);
            routingContext.RouteExecuteType = ContextRoutingType.Single;

            // so if you generated long listening addresses from your replicas this should work
            // get listening address which might be something like http://node:port/path/
            // append to it orignal path of the request.
            httpCtx.OverridePath = true;
            httpCtx.Path = string.Concat(outUri.AbsolutePath, inPathQueryString);
            
            return true;
      
        }

    }
}
