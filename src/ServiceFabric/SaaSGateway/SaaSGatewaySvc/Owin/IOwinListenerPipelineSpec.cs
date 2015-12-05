
namespace SaaSGatewaySvc
{
    using Owin;

    /// <summary>
    ///  defines an Owin Listener specification
    /// CreateOwinPipeline method is expected to create the Owin Pipeline
    /// </summary>
    public interface IOwinListenerSpec
    {
        void CreateOwinPipeline(IAppBuilder app);
    }
}