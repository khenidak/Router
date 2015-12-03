using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Owin;

namespace RouterLib.Owin
{

    public static class  OwinRouterExt
    {
        public static void UseOwinRouter(this IAppBuilder app, IRouterMwOptions options)
        {
            options.Validate();
            app.Use(typeof(RouterMw), options);
        }
    }

}
