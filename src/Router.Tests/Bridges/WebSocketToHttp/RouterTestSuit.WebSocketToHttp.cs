using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;


using RouterLib;
using HttpRouterLib;
using System.Net.WebSockets;
using System.Threading;
using System;
using WebSocketServer;
namespace RouterTests
{

    
    public partial class  RouterTestSuit
    {

        private async Task<string> ReceiveFromSocket(ClientWebSocket theSocket)
        {

            // receive 

            var arraySegment = new ArraySegment<byte>(new Byte[1024 * 256]);
            var res = await theSocket.ReceiveAsync(arraySegment, CancellationToken.None);

            return Encoding.UTF8.GetString(arraySegment.Actualize(res.Count)); // helper in WebSocketServer.dll
        }


        /// <summary>
        /// Client ======>WebSocketServer(uses router)====> WebApi Backend 
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        [TestCategory("Bridges.WebSocketToHttp")]
        public async Task WebSocketsMapToHttp01()
        {
            // in order to review what are the routing rules
            // used by the router look at the Owin startup routine

            // start a web server that holds our owin pipeline (which has the router in it)
            var SocketWebServer = new TestWebServer();
            SocketWebServer.Start<OwinWebSocketStartup>(OwinSocketRouterSrvAddress);



            var InMessage = new MessageIn() { MessageType = "GetModels" };
            var InMessageJson = InMessage.GetAsJsonString();


            // create a socket client and connect

            ClientWebSocket clientWSocket = new ClientWebSocket();
            await clientWSocket.ConnectAsync(new Uri(OwinSocketRouterSrvAddressExternal), CancellationToken.None);

            // send in Message
            await clientWSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(InMessageJson)),
                                          WebSocketMessageType.Text,
                                          true,
                                          CancellationToken.None);





            var socketrcv = await ReceiveFromSocket(clientWSocket);
            var outMessage = JsonConvert.DeserializeObject<MessageOut>(socketrcv);

            // based on routing the following should be true
            Assert.AreEqual(outMessage.Method, "GET");
            Assert.IsTrue(outMessage.Models.Count() > 0);

            // stop the server
            SocketWebServer.Stop();
        }



        /// <summary>
        /// Client ======>WebSocketServer(uses router)====> WebApi Backend 
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        [TestCategory("Bridges.WebSocketToHttp")]
        public async Task WebSocketsMapToHttp02()
        {
            // in order to review what are the routing rules
            // used by the router look at the Owin startup routine

            // start a web server that holds our owin pipeline (which has the router in it)
            var SocketWebServer = new TestWebServer();
            SocketWebServer.Start<OwinWebSocketStartup>(OwinSocketRouterSrvAddress);



            var InMessage = new MessageIn() { MessageType = "AddModels" };
            InMessage.theModel = Model.getRandomModel();

            var InMessageJson = InMessage.GetAsJsonString();


            // create a socket client and connect

            ClientWebSocket clientWSocket = new ClientWebSocket();
            await clientWSocket.ConnectAsync(new Uri(OwinSocketRouterSrvAddressExternal), CancellationToken.None);

            // send in Message
            await clientWSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(InMessageJson)),
                                          WebSocketMessageType.Text,
                                          true,
                                          CancellationToken.None);





            var socketrcv = await ReceiveFromSocket(clientWSocket);
            var outMessage = JsonConvert.DeserializeObject<MessageOut>(socketrcv);

            // based on routing the following should be true
            Assert.AreEqual(outMessage.Method, "POST");
            Assert.IsTrue(outMessage.Models.Count() ==  1);
            Assert.IsTrue(outMessage.Models[0].ModelId == InMessage.theModel.ModelId);


            // stop the server
            SocketWebServer.Stop();
        }



    }
}
