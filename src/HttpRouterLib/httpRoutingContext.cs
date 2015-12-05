using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RouterLib;

namespace HttpRouterLib
{
    /// <summary>
    /// Http specific routing context that provides the routing mechanics for http endpoints
    /// </summary>
    public class HttpRoutingContext : RoutingContextBase
    {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
      

        public Dictionary<string, IEnumerable<string>> Headers { get; set; } = new Dictionary<string, IEnumerable<string>>();


        private const string Content_Type_Header_Name = "Content-Type";
        private const string Content_Length_Header_Name = "Content-Length";

        public HttpRoutingContext(IRouteResolver Resolver, string sAddress,
                                IDictionary<string, object> Context,
                                Stream Body) :
                                base(Resolver, sAddress, Context, Body)
        {

            // no op ctor
        }

        protected virtual string GetPath()
        {
            if (OverridePath)
                return Path;

            var uri = new Uri(this.SourceAddress);
            if (0 != uri.Query.Length)
                return string.Concat(uri.AbsolutePath,uri.Query);
            else
                return uri.AbsolutePath;

        }
        protected virtual string GetScheme()
        {
            if (OverrideScheme)
                return Scheme;

            return new Uri(this.SourceAddress).Scheme;

        }


        protected virtual void processHeaders(HttpRequestMessage hrm)
        {
            
            foreach(var header in Headers)
            {
                // avoid adding content content type header
                // directly to the request, the hrm does not like it
                if (header.Key == Content_Type_Header_Name || header.Key == Content_Length_Header_Name)
                    break;


                switch (header.Value.Count())
                { 
                    case 0:
                    {
                       hrm.Headers.Add(header.Key, string.Empty);
                       break;
                    }
                    case 1:
                    {
                       hrm.Headers.Add(header.Key, header.Value.First());
                       break;
                    }

                    default:
                    {
                       hrm.Headers.Add(header.Key, header.Value);
                       break;
                    }                        
               }
            }
        }
        protected virtual void processBody(HttpRequestMessage hrm)
        {
            if (null != SourceBody)
            { 

                var bodyCopy = getBodyCopy();

                // sanity check
                if (0 == bodyCopy.Length)
                    return;


                bodyCopy.Seek(0, 0);
                hrm.Content = new StreamContent(bodyCopy);

                // process content type headers 
                if (Headers.ContainsKey(Content_Type_Header_Name))
                {

                    if (0 != Headers[Content_Type_Header_Name].Count())
                    {
                        if (1 == Headers[Content_Type_Header_Name].Count())
                            hrm.Content.Headers.Add(Content_Type_Header_Name, Headers[Content_Type_Header_Name].First());
                        else
                            hrm.Content.Headers.Add(Content_Type_Header_Name, Headers[Content_Type_Header_Name]);
                    }
                }
            }

        }
        protected virtual HttpRequestMessage getRequestMessage(string sHostAddress)
        {

            HttpRequestMessage hrm = new HttpRequestMessage();
            hrm.Method = this.Method;

            
            processBody(hrm);
            processHeaders(hrm);

            var RequestPath = GetPath();
            var RequestScheme = GetScheme();

            hrm.RequestUri = new Uri(string.Concat(RequestScheme, "://", sHostAddress, RequestPath));


            return hrm;

        }

      




        public override async Task<RoutingResultBase> ExecuteAsync(ContextExecuteModeBase executeMode)
        {
            // validate that the passed execution mode is supported by this context.
            if (!IsAllowedExecutionMode(executeMode))
                throw new NotSupportedExecuteModeException(string.Format("Context of type {0} can not support execute mode of type {1}", 
                                                                this.GetType().ToString(), 
                                                                executeMode.GetType().ToString()));



            var result = new HttpRoutingResult();
            result.ExecutionContext = this;
            var client = new HttpClient();




            switch (RouteExecuteType)
            {
                case ContextRoutingType.Single:
                    {
                        var hrm = getRequestMessage(TargetHostAddressList[0]);

                        var response = await client.SendAsync(hrm);
                        result.SetResult(response);

                        break;
                    }

                case ContextRoutingType.FastestRoute:
                    {
                        var tasks = new List<Task<HttpResponseMessage>>(TargetHostAddressList.Count());
                        foreach (var host in TargetHostAddressList)
                        {
                            var hrm = getRequestMessage(host);

                            tasks.Add(client.SendAsync(hrm));

                        }
                        // TPL hoorah!
                        var completedTask = await Task.WhenAny<HttpResponseMessage>(tasks);
                        result.SetResult(await completedTask);

                        break;
                    }

                case ContextRoutingType.ScatterGather:
                    {
                        var tasks = new List<Task<HttpResponseMessage>>(TargetHostAddressList.Count());
                        foreach (var host in TargetHostAddressList)
                        {
                            var hrm = getRequestMessage(host);
                            tasks.Add(client.SendAsync(hrm));
                        }

                        await Task.WhenAll(tasks);
                        var responses = new List<HttpResponseMessage>(TargetHostAddressList.Count());
                        foreach (var completedtask in tasks)
                            responses.Add(completedtask.Result);


                        result.SetResult(responses);

                        break;
                    }

                case ContextRoutingType.RoundRobin:
                    {
                        var hrm = getRequestMessage(this.GetNextHostAddressInBalancingSet());
                        var response = await client.SendAsync(hrm);
                        result.SetResult(response);
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException(string.Format("invalid routing type for http router", RouteExecuteType.ToString()));
                    }
            }
            
          

            return result;
        }
    }
}
