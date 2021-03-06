﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


namespace RouterLib.Owin
{
    
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // original owin context, routing results, returns a task
    using ProcessResponseFunc = Func<IDictionary<string, object>, RoutingResultBase, Task>;
    // original owin context, Error, Exception generated by router, Next in OwinPipeline returns a task
    using ExceptionFunc = Func<IDictionary<string, object>, Exception, Func<IDictionary<string, object>, Task>, Task>;
    // Owin original context, Next in Owin pipeline, returns a task
    using NoRouteFoundFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>;
    using System.IO;

    public class DefaultRouterMwOptions<R,RER, Resolver>  : IRouterMwOptions
        where R : Router<RER,Resolver>
        where RER : RoutingResultBase
        where Resolver : IRouteResolver
    {
        public R Router { get; set; }
        public bool CallNextOnSuccess { get; set; } = false;

        public ProcessResponseFunc OnProcessResponse { get; set; } = null; // used to process responses (i.e. copy to the down stream).
        public ExceptionFunc OnException { get; set; } = null; // if router throw an exception this will be called
        public NoRouteFoundFunc OnNoRouteFound { get; set; } = null;


        public DefaultRouterMwOptions(R router) : base()
        {
            Router = router;
        }

        public void Validate()
        {
            if(null == Router)
                throw new InvalidOperationException("Router is null");


            if (null == OnProcessResponse)
                throw new InvalidOperationException("OnProcessResponse is null");
        }

        public string GetOwinRequestAddress(IDictionary<string, object> environment)
        {

            // there is an oops below
            // your routing should depends on the host passed by the client
            // if you have a reverse proxy terminating the connection
            // then make sure that ll your routing rules are based on this addres


            // the below is based on http://owin.org/spec/spec/owin-1.0.0.html#Paths

            var queryString = environment["owin.RequestQueryString"] as string;
            var scheme = environment["owin.RequestScheme"] as string;

            var headers = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
            var host = headers["Host"].First();


            var basePath = environment["owin.RequestPathBase"] as string;
            var path = environment["owin.RequestPath"] as string;

            var uri = string.Concat(scheme, "://", host, basePath, path);



            if (!string.IsNullOrEmpty(queryString))
                uri = string.Concat(uri, "?", queryString);

            return uri;
        }


         
        public async Task ProcessOwinRequestAsync(AppFunc Next, IDictionary<string, object> environment)
        {

            var body = environment["owin.RequestBody"] as Stream;
            var requestAddress = GetOwinRequestAddress(environment);

            RER results = null;

            try
            {
                results = await Router.RouteAsync(requestAddress, environment, body);
                if (null != results)
                {
                    // call was sucessful
                    await OnProcessResponse(environment, results);
                }
                else // no route found
                {
                    // do we have a handler
                    if (null != OnNoRouteFound)
                        await OnNoRouteFound(environment, Next);
                    else // let exception buble up the chain
                        throw new Exception("Router couldn't find a route for this request");

                }
            }

            catch (Exception e)
            {

                if (null != OnException) // do we have an exception handler
                    await OnException(environment, e,Next);
                else
                    throw; // throw the exception as is
            }



            if (CallNextOnSuccess)
                await Next.Invoke(environment);
        }
    }
}
