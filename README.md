# Router #
Router is a building block that allows you to route requests between two applications endpoints. The router is designed to work in-proc or behind a communication listener/broker. It is also designed to work in microservices based applications such as those hosted by mesos or Azure Service Fabric. It is also designed to in SaaS scenarios hosted on microservices applications. Along with  your process/communication broker it represents an application gateway.

 # Usage Context #
1. Context based routing such as based on Http headers, host address, path, time of day, combination of conditions or custom.
2. Complex routing based on external factors such as match address based on user identity. For example matching user/tenants (SaaS Scenarios). Addresses can be statically defined or dynamically resolved from a backend system.
3. Apply complex inflight call modification logic such as replacing payload body, modifying headers (in case of http), changing path or others   
4. Apply call routing logic such as
  * Scatter/Gather call is routed to multiple backend hosts and response is aggregated
  * Fastest route, call is routed to multiple backend hosts, fastest responding host wins.
  * Round/Robin (R/R) load balancing between backend hosts.
5. Apply call execution logic such as retry, retry with back-off, fail to a different host, log. Execution logic can be chained and applied in sequence.
6. Protocol Transition (aka protocol bridges). For example, routing from Web Sockets to Http API *test project contains 2 samples on this*
7. Version Management: for example, route some clients to older versions of the API deployed on the backend, while other clients are routed to newer versions. Also used for A/B testing.

>The testing project (in the source code) contains samples on how to use the above.

 Because application gateway are meant to process all request, You can use it to do central user AuthN/AuthZ at the entry point then perform trusted subsystem AuthN/AuthZ between the gateway and the backend.

 

 # Overview #

![Overview](./docs/overview.png)

> Everything in the framework is either replaceable or customizable (except for the router which supports only customization).

**From top to bottom, the idea is:**
* The hosting process (which might include a communication listener) will host one or many router instances.
* **The Router** uses a resolver to perform a resolve on calls. In addition to maintaining telemetry.
* The resolver itself uses the **Matching Framework** (discussed in the in-depth section). Currently the resolve operation takes < 0.01 ms (and most of times falls under 0.001 ms) on intel i7 quad core machine.
* The **Matching Framework** is a stateless in memory tree of *matchers* that are executed in sequence allowing request validation, request modification, in addition to building the **Routing Context**.
* **Routing Context** represents a repeatable execution context. The router then executes the context returning to caller **Routing Results**. Router uses the Execution Strategy to handle errors, perform retries or other.
* The resolver instance is presented to the matching framework and routing contexts to allow them to perform in-between-routing-calls caching such as cache results, cache long calculation results (for example it is used by the base **Routing Context** to maintain a reference to last host used in a load balancing set).  
* **Execution Strategy Framework** is a framework that is used by router to handle errors. The framework consists of a set of execution Strategies, they are represented as an in memory stateless linked list. Router executes strategy and then move to the next in case of errors (more on this in the in-depth section).         

# How to Use #
```
// The test project contain additional examples with more complex routing logic.  
// at minimum you need router, resolver, and one or more matchers
var resolver = new HttpRouteResolver(); // included in the repo

/*
 if host address matches xxx://xxx.bing.xxx then route to  www.microsoft.com
 as Http Get
 */
var headMatcher = new HttpSetMethodMatcher(HttpMethod.Get);
headMatcher.Chain(new SetAddressListMatcher("www.microsoft.com"),
                  new HostAddressMatcher("www.bing.com", StringMatchType.UriHostandPortMatch));

// resolver sets on top of multiple matching trees (ordered)
await resolver.AddMatcherAsync(headMatcher, 0);

var router = new HttpRouter(resolver); //included in the package
// by not having any execution strategies assigned to the router, router will not retry
// check the test project for samples on execution strategy  

// **** in other places of your application                              
var results = await router.RouteAsync("http://www.bing.com");
//or this will also will work.
var results = await router.RouteAsync("bing");
//or you can additionally pass context Dictionary<string,object> in addition to stream (body)
```
# Repo Src Content #
1. **RouterLib**: Base routing, matching, resolve and strategy handling logic. The library has no reference to any external dependency (and no .NET assemblies except core ones) and is designed to work with CoreClr on Linux.
2. **HttpRouterLib**: Extends RouterLib to support Http routing.
3. **RouterLib.Owin**:  Broker as Owin Owin pipeline stage.
4. **Router.Tests**: testing and sample project for all of the above.

# Next Steps #
* [In-Depth](./docs/in-depth.md) Further discussion on how matching & execution strategy frameworks are working.
