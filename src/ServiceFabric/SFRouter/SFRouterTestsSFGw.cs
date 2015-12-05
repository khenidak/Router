using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFRouter
{
    public partial class SFRouterTests
    {
        /*
        the below tests verify the following mapping
           
           Tenant: customer01 ->SF App fabric:/saastenantapp01
           Tenant: customer02 ->SF App fabric:/saastenantapp02
           Tenant: customer03 ->SF App fabric:/saastenantapp03
           Tenant: customer04 ->SF App fabric:/saastenantapp04
            
            
           Tenant: welcome ->SF App fabric:/saastenantappX";
        
            or anyother <subdomain>.superdoperapp.com (if you are using DNS not hosts).
           */


        [TestMethod]
        [TestCategory("SFRouter.Basic")]
        public async Task SFGwRoutingToTenant()
        {
            // test the a random tenant tenates are workoing correctly
            var randomTenant = (new Random()).Next(1, 4);
            var hrm = new HttpRequestMessage();
            // call the GW endpoint
            hrm.RequestUri = new System.Uri(string.Format("http://customer0{0}.superdopersaas.com", randomTenant)); //this goes to local host
            hrm.Method = HttpMethod.Get;
            var httpClient = new HttpClient();
            var httpResponse = await httpClient.SendAsync(hrm);
            
            var responseString = await httpResponse.Content.ReadAsStringAsync();
            httpResponse.EnsureSuccessStatusCode();

            Assert.AreEqual(string.Format("fabric:/saastenantapp0{0}", randomTenant), responseString);
        }


        [TestMethod]
        [TestCategory("SFRouter.Basic")]
        public async Task SFGwRoutingToDefaultTenant()
        {
            // test that gw routes to default tenant
            var hrm = new HttpRequestMessage();
            
            // if you have control over DNS then you route to <anything>.superdopersaas.com
            // and router will work just as well, since this test is running using
            // hosts file, we will not be able to do that.

            hrm.RequestUri = new System.Uri("http://welcome.superdopersaas.com"); //this goes to local host
            hrm.Method = HttpMethod.Get;
            var httpClient = new HttpClient();
            var httpResponse = await httpClient.SendAsync(hrm);

            var responseString = await httpResponse.Content.ReadAsStringAsync();
            httpResponse.EnsureSuccessStatusCode();

            Assert.AreEqual("fabric:/saastenantappX", responseString);
        }

    }
}
