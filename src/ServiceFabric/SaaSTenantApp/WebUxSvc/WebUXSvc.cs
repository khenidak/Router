using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebUxSvc
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class WebUXSvc : StatelessService
    {

        public WebUXSvc(StatelessServiceContext serviceContext) : base ( serviceContext)
        {

        }
        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            
            var instanceListener = new ServiceInstanceListener (
                   parameteres => 
                   {
                       var owinlistener = new OwinCommunicationListener(new WebUXSvcListenerSpec(this), parameteres);
                       return owinlistener;
                   }, "http");
            return new ServiceInstanceListener[1] { instanceListener };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancelServiceInstance">Canceled when Service Fabric terminates this instance.</param>
        protected override async Task RunAsync(CancellationToken cancelServiceInstance)
        {
            // TODO: Replace the following sample code with your own logic.

            int iterations = 0;
            // This service instance continues processing until the instance is terminated.
            while (!cancelServiceInstance.IsCancellationRequested)
            {

                // Log what the service is doing
                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", iterations++);

                // Pause for 1 second before continue processing.
                await Task.Delay(TimeSpan.FromSeconds(1), cancelServiceInstance);
            }
        }
    }
}
