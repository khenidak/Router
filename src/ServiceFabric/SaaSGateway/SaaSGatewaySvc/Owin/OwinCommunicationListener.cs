
namespace SaaSGatewaySvc
{
    using System;
    using System.Fabric;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Owin.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using HttpRouterLib;
    using RouterLib;

    // generic Owin Listener
    public class OwinCommunicationListener : ICommunicationListener
    {
        private IDisposable webServer = null;
        private string listeningAddress = string.Empty;
        private string publishingAddress = string.Empty;

        private readonly IOwinListenerSpec pipelineSpec;
        private readonly ServiceContext ServiceContext;

       
        public OwinCommunicationListener(IOwinListenerSpec pipelineSpec, ServiceContext serviceContext)
        {
            this.pipelineSpec = pipelineSpec;
            this.ServiceContext = serviceContext;
        }
        
        public void Abort()
        {
            if (null != this.webServer)
            {
                this.webServer.Dispose();
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.Abort();
            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            StatefulServiceContext statefulInitParam = this.ServiceContext as StatefulServiceContext;
            int port = this.ServiceContext.CodePackageActivationContext.GetEndpoint("ServiceEndPoint").Port;


            if (statefulInitParam != null)
            {
                this.listeningAddress = String.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/{1}/{2}/{3}/",
                    port,
                    statefulInitParam.PartitionId,
                    statefulInitParam.ReplicaId,
                    Guid.NewGuid());
            }
            else
            {
                this.listeningAddress = String.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/",
                    port);
            }

            //netsh http add urlacl url = http://+:80/ user="NT AUTHORITY\NETWORK SERVICE"
            this.publishingAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            this.webServer = WebApp.Start(this.listeningAddress, app => this.pipelineSpec.CreateOwinPipeline(app));

            return Task.FromResult(this.publishingAddress);   
        }
    }
}