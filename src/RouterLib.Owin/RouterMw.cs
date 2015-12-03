using System;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace RouterLib.Owin
{
    using System.IO;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// This class acts as an all in one Owin middleware for the router base implementation.
    /// You do not have to implement a different middleware for each router type. 
    /// check tests/bridges/OwinToHttp for more details
    /// all the functions are externalized to the options class implementation. 
    /// You can use the default supplied options class (by providing your own OnXXXX delegates), or create your own.
    /// </summary>
    public class RouterMw
    {

        protected IRouterMwOptions mOptions;
        protected AppFunc mNext; 
        public RouterMw(AppFunc next, IRouterMwOptions options) 
        {
            mNext = next;
            mOptions = options;
        }

        
        public async Task Invoke(IDictionary<string, object> environment)
        {

            await mOptions.ProcessOwinRequestAsync(mNext, environment);
            
        }
    }
}
