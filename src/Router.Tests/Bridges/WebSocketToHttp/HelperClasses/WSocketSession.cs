using Microsoft.Owin;
using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using WebSocketServer;
using System.IO;
using System.Net.Http;

namespace RouterTests
{
    public class WSocketSession : WebSocketSessionBase
    {
        public const string Message_Type_Key = "Message.Type";
        public WSocketSession(IOwinContext context,
                             WSocketSessionFactory factory,
                             CancellationToken cancelToken) : base(context,  factory, cancelToken)
        {
            //no op just init base 
        }

        public override async Task OnReceiveAsync(ArraySegment<byte> buffer, Tuple<int, bool, int> received)
        {
            

            var wSocketFactory = m_factory as WSocketSessionFactory; // injected by the factory
            var ctx = new Dictionary<string, object>(); // empty routing context


            // There are better ways to do that, in a typical high performance production
            // systems you really need to do that without unpacking the stream



            var inMessage = await GetFromBufferAsJson<MessageIn>(buffer, received);
            var sMessageType = inMessage.MessageType;
            Stream inStream = null;
            if (null != inMessage.theModel)
                inStream = inMessage.theModel.asJsonStream();


            // prepare the routing data 
            ctx.Add(Message_Type_Key, sMessageType);



            // you need to route and when done, you will put the response to the socket down stream
            // you don't need to hold OnRecieveAsync much longer
            var t = Task.Run( async () => 
                    {
                        var socketinstance = this;

                        var httpRoutingResults = await wSocketFactory.Router.RouteAsync("", ctx, inStream);
                        // get the httpresponseMessage (we are always routing 1:1 in this test)

                        var hrm = await httpRoutingResults.ResultAsAsync<HttpResponseMessage>();

                        // in a typical system you will extend the sockets to use bytes directly
                        var outstring = await hrm.Content.ReadAsStringAsync();

                        await socketinstance.Post(outstring);
                    });
        }
    }
}
