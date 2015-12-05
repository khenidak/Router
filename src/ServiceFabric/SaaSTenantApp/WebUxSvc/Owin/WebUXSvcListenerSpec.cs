
namespace WebUxSvc
{

    using Owin;
    using System.Threading.Tasks;
    using System.IO;

    internal class WebUXSvcListenerSpec : IOwinListenerSpec
    {
        private readonly WebUXSvc service;

        public WebUXSvcListenerSpec(WebUXSvc service)
        {
            this.service = service;
        }

        public void CreateOwinPipeline(IAppBuilder app)
        {
         // just one owin mw that prints application name   
            app.Use(
                async (ctx,next) =>
                    {
                      ctx.Response.StatusCode = 200;
                      var sr = new StreamWriter(ctx.Response.Body);
                   
                      await  sr.WriteAsync(service.ServiceInitializationParameters.CodePackageActivationContext.ApplicationName);
                      await sr.FlushAsync();
                        sr = null;  
                    }
                );
               
        }
    }
}