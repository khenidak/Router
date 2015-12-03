using HttpRouterLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketServer;

namespace RouterTests
{
    public class WSocketSessionFactory : WebSocketSessionManager<WSocketSession>
    {

        public HttpRouter Router { get; }

        public WSocketSessionFactory(HttpRouter router)
        {
            Router = router;
        }
    }
}

