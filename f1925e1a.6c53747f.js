(window.webpackJsonp=window.webpackJsonp||[]).push([[44],{111:function(e,t,n){"use strict";n.r(t),n.d(t,"frontMatter",(function(){return i})),n.d(t,"metadata",(function(){return s})),n.d(t,"toc",(function(){return l})),n.d(t,"default",(function(){return u}));var r=n(3),a=n(7),o=(n(0),n(121)),i={slug:"beta-1",title:"Grapevine 5.0 Beta 1",author:"Scott Offen",author_title:"Grapevine Creator",author_url:"https://scottoffen.github.io",author_image_url:"https://avatars1.githubusercontent.com/u/3513626?v=4",tags:["5.0.0-beta.1"]},s={permalink:"/grapevine/blog/beta-1",editUrl:"https://github.com/scottoffen/grapevine-docs/edit/master/blog/blog/2020-12-29-beta-1.md",source:"@site/blog\\2020-12-29-beta-1.md",description:"The second beta version of Grapevine 5.0 was uploaded to NuGet.org today, December 29, 2020. Here are some updated code samples to start playing around with.",date:"2020-12-29T00:00:00.000Z",tags:[{label:"5.0.0-beta.1",permalink:"/grapevine/blog/tags/5-0-0-beta-1"}],title:"Grapevine 5.0 Beta 1",readingTime:6.635,truncated:!0,prevItem:{title:"Three Release Candidates And A Website",permalink:"/grapevine/blog/three-release-candidates-and-a-website"},nextItem:{title:"Grapevine Docs",permalink:"/grapevine/blog/docusaurus"}},l=[{value:"Using The Default Configuration",id:"using-the-default-configuration",children:[]},{value:"Using A Startup Class",id:"using-a-startup-class",children:[{value:"Specifying A Service Collection",id:"specifying-a-service-collection",children:[]},{value:"Configuring Services",id:"configuring-services",children:[]},{value:"Configuring The Server",id:"configuring-the-server",children:[]}]},{value:"Running The Server",id:"running-the-server",children:[]}],c={toc:l};function u(e){var t=e.components,n=Object(a.a)(e,["components"]);return Object(o.b)("wrapper",Object(r.a)({},c,n,{components:t,mdxType:"MDXLayout"}),Object(o.b)("p",null,"The second beta version of Grapevine 5.0 was uploaded to ",Object(o.b)("a",Object(r.a)({parentName:"p"},{href:"https://www.nuget.org/"}),"NuGet.org")," today, December 29, 2020. Here are some updated code samples to start playing around with."),Object(o.b)("p",null,"For more code samples, see ",Object(o.b)("a",Object(r.a)({parentName:"p"},{href:"https://github.com/scottoffen/grapevine/tree/main/src/Samples"}),"https://github.com/scottoffen/grapevine/tree/main/src/Samples"),"."),Object(o.b)("p",null,"As promised, this beta drops the ",Object(o.b)("inlineCode",{parentName:"p"},"RestServer.DeveloperConfiguration")," that we used in the first beta, and introduces the ",Object(o.b)("inlineCode",{parentName:"p"},"RestServerBuilder"),". Until the documentation is complete, you can dig into the details of the class in the source code if you like. Here I'm going to show you the two static methods it exposes that I expect will be most used going forward, ",Object(o.b)("inlineCode",{parentName:"p"},"RestServerBuilder.UseDefaults")," and ",Object(o.b)("inlineCode",{parentName:"p"},"RestServerBuilder.From<T>"),"."),Object(o.b)("div",{className:"admonition admonition-tip alert alert--success"},Object(o.b)("div",Object(r.a)({parentName:"div"},{className:"admonition-heading"}),Object(o.b)("h5",{parentName:"div"},Object(o.b)("span",Object(r.a)({parentName:"h5"},{className:"admonition-icon"}),Object(o.b)("svg",Object(r.a)({parentName:"span"},{xmlns:"http://www.w3.org/2000/svg",width:"12",height:"16",viewBox:"0 0 12 16"}),Object(o.b)("path",Object(r.a)({parentName:"svg"},{fillRule:"evenodd",d:"M6.5 0C3.48 0 1 2.19 1 5c0 .92.55 2.25 1 3 1.34 2.25 1.78 2.78 2 4v1h5v-1c.22-1.22.66-1.75 2-4 .45-.75 1-2.08 1-3 0-2.81-2.48-5-5.5-5zm3.64 7.48c-.25.44-.47.8-.67 1.11-.86 1.41-1.25 2.06-1.45 3.23-.02.05-.02.11-.02.17H5c0-.06 0-.13-.02-.17-.2-1.17-.59-1.83-1.45-3.23-.2-.31-.42-.67-.67-1.11C2.44 6.78 2 5.65 2 5c0-2.2 2.02-4 4.5-4 1.22 0 2.36.42 3.22 1.19C10.55 2.94 11 3.94 11 5c0 .66-.44 1.78-.86 2.48zM4 14h5c-.23 1.14-1.3 2-2.5 2s-2.27-.86-2.5-2z"})))),"tip")),Object(o.b)("div",Object(r.a)({parentName:"div"},{className:"admonition-content"}),Object(o.b)("p",{parentName:"div"},"The builder instantiates and configures the ",Object(o.b)("inlineCode",{parentName:"p"},"RestServer")," but does not start it. You will still need to call ",Object(o.b)("inlineCode",{parentName:"p"},"server.Start()")," before it will respond to requests."))),Object(o.b)("h2",{id:"using-the-default-configuration"},"Using The Default Configuration"),Object(o.b)("pre",null,Object(o.b)("code",Object(r.a)({parentName:"pre"},{className:"language-csharp"}),"var server = RestServerBuilder.UseDefaults().Build();\n")),Object(o.b)("p",null,"The default configuration is good for early development. It uses the ",Object(o.b)("inlineCode",{parentName:"p"},"ServiceCollection")," implementation from Microsoft, sets the logging level to Trace (so you can see all the output) and adds the default listener prefix of ",Object(o.b)("inlineCode",{parentName:"p"},"https://localhost:1234"),". You can now start that server, it will autoscan for routes, and you'll be up and running!"),Object(o.b)("h2",{id:"using-a-startup-class"},"Using A Startup Class"),Object(o.b)("p",null,"The startup class approach allows you to specify your prefered ",Object(o.b)("inlineCode",{parentName:"p"},"IServiceCollection")," implementation, easily register implementations with the container, and configure your ",Object(o.b)("inlineCode",{parentName:"p"},"IRestServer"),", all inside of a simple class that you specify as a generic parameter."),Object(o.b)("pre",null,Object(o.b)("code",Object(r.a)({parentName:"pre"},{className:"language-csharp"}),"var server = RestServerBuilder.From<Startup>().Build();\n")),Object(o.b)("p",null,"The class you specify must have a parameterless constructor (implict or explicit doesn't matter). The naming of the methods in the class doesn't matter, only their visibility (public) and method signature. Using reflection, only the first matching method will be used. If no matching methods are found, what you get will be similar to if you had used the default configuration, with the expection that ",Object(o.b)("strong",{parentName:"p"},"no prefixes are added to the listener"),"."),Object(o.b)("p",null,"The example below provides the same outcome as if you had used the ",Object(o.b)("inlineCode",{parentName:"p"},"UseDefaults()")," approach."),Object(o.b)("pre",null,Object(o.b)("code",Object(r.a)({parentName:"pre"},{className:"language-csharp",metastring:'title="Startup.cs"',title:'"Startup.cs"'}),'public class Startup\n{\n    /*\n    * Include a method with this signature (method name does not matter) if\n    * you want to use an IServiceCollection implementation other than the one\n    * provided by Microsoft. You can choose to configure some services here\n    * as well, if you\'d like.\n    */\n    public IServiceCollection GetServices()\n    {\n        return new ServiceCollection();\n    }\n\n    /*\n    * Include a method with this signature (method name does not matter) to\n    * configure your services. Prior to the method being called, implementations\n    * for IRestServer, IRouter and IRouteScanner have already been registered.\n    */\n    public void ConfigureServices(IServiceCollection services)\n    {\n        services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);\n    }\n\n    /*\n    * Include a method with this signature (method name does not matter) to\n    * configure your IRestServer. Add event handlers for stopping and starting\n    * the server, request recieved, and before and after routing. If you want\n    * to do manual route registration (more complex) this is this place to do it.\n    */\n    public void ConfigureServer(IRestServer server)\n    {\n        server.Prefixes.Add("http://localhost:1234/");\n    }\n}\n')),Object(o.b)("h3",{id:"specifying-a-service-collection"},"Specifying A Service Collection"),Object(o.b)("p",null,"You can use any dependency injection library you want, as long as it exposes the ",Object(o.b)("inlineCode",{parentName:"p"},"IServiceCollection")," interface. If the library you want to use doesn't expose this interface, you'd have wrap it in a class that did. See the documentation for your prefered library on how to do this."),Object(o.b)("p",null,"If you do specify a different implementation, you can configure your services here as well. However, between calling this method and calling the configure services method, concrete implementations for ",Object(o.b)("inlineCode",{parentName:"p"},"IRestServer"),", ",Object(o.b)("inlineCode",{parentName:"p"},"IRouter")," and ",Object(o.b)("inlineCode",{parentName:"p"},"IRouteScanner")," will be added. If you want to add your own implementations for that, do that in the configure services method, not here."),Object(o.b)("h3",{id:"configuring-services"},"Configuring Services"),Object(o.b)("p",null,"This method is used to add concrete implementations for dependencies you want injected into your routes. If you want to specify your own implementations for ",Object(o.b)("inlineCode",{parentName:"p"},"IRestServer"),", ",Object(o.b)("inlineCode",{parentName:"p"},"IRouter")," and ",Object(o.b)("inlineCode",{parentName:"p"},"IRouteScanner"),", this would be the place to do it. Configure logging here, too. By default, a console logger has already been added, so you might want to clear all other loggers before adding your prefered implementation (e.g. NLog or Serilog)."),Object(o.b)("h3",{id:"configuring-the-server"},"Configuring The Server"),Object(o.b)("p",null,"By the time this method is called, a concrete implementation of ",Object(o.b)("inlineCode",{parentName:"p"},"IRestServer")," has been created from the service collection. The service collection created has been added to ",Object(o.b)("inlineCode",{parentName:"p"},"IRestServer.Router.ServiceCollection")," to be used during the route scanning and routing process. Here, you want to configure your rest server."),Object(o.b)("h4",{id:"pipeline-event-handlers"},"Pipeline Event Handlers"),Object(o.b)("p",null,"You can add event handlers to be executed in the following places of the pipeline:"),Object(o.b)("ul",null,Object(o.b)("li",{parentName:"ul"},Object(o.b)("inlineCode",{parentName:"li"},"IRestServer.BeforeStarting")),Object(o.b)("li",{parentName:"ul"},Object(o.b)("inlineCode",{parentName:"li"},"IRestServer.AfterStarting")),Object(o.b)("li",{parentName:"ul"},Object(o.b)("inlineCode",{parentName:"li"},"IRestServer.BeforeStopping")),Object(o.b)("li",{parentName:"ul"},Object(o.b)("inlineCode",{parentName:"li"},"IRestServer.AfterStopping")),Object(o.b)("li",{parentName:"ul"},Object(o.b)("inlineCode",{parentName:"li"},"IRestServer.OnRequestAsync")),Object(o.b)("li",{parentName:"ul"},Object(o.b)("inlineCode",{parentName:"li"},"IRouter.BeforeRoutingAsync")),Object(o.b)("li",{parentName:"ul"},Object(o.b)("inlineCode",{parentName:"li"},"IRouter.AfterRoutingAsync"))),Object(o.b)("p",null,"The first four are good for logging information during server startup and shutdown, but can be used for any other purpose as well."),Object(o.b)("p",null,"Use ",Object(o.b)("inlineCode",{parentName:"p"},"BeforeRoutingAsync")," and ",Object(o.b)("inlineCode",{parentName:"p"},"AfterRoutingAsync")," for things you want done before and after the request is routed, respectively."),Object(o.b)("p",null,"Because any added middleware (described below) might respond to requests before the request is routed (and hence not calling ",Object(o.b)("inlineCode",{parentName:"p"},"BeforeRoutingAsync")," and ",Object(o.b)("inlineCode",{parentName:"p"},"AfterRoutingAsync"),"), use ",Object(o.b)("inlineCode",{parentName:"p"},"OnRequestAsync")," for things you want done regardless of whether the request gets routed. Because this is the same event that middleware should connect into, you might want to add your own event handlers before adding any middleware, lest the request be responded to before the handler has a chance to execute."),Object(o.b)("h4",{id:"global-response-headers"},"Global Response Headers"),Object(o.b)("p",null,"You can also configure global response headers here. These are key/value pairs that you want to send back unmodified in every request."),Object(o.b)("pre",null,Object(o.b)("code",Object(r.a)({parentName:"pre"},{className:"language-csharp"}),'server.GlobalResponseHeaders.Add("key", "value");\n')),Object(o.b)("h4",{id:"custom-ihttpcontext"},"Custom IHttpContext"),Object(o.b)("p",null,"If you want to use your own implementation of ",Object(o.b)("inlineCode",{parentName:"p"},"IHttpContext"),", but not your own implementation of ",Object(o.b)("inlineCode",{parentName:"p"},"IRestServer")," (which is where the ",Object(o.b)("inlineCode",{parentName:"p"},"IHttpContext")," is created before being passed to the router), you can replace the default factory with your own."),Object(o.b)("pre",null,Object(o.b)("code",Object(r.a)({parentName:"pre"},{className:"language-csharp"}),"server.HttpContextFactory = (context, server, token) => { return new MyHttpContext(context, server, token); };\n")),Object(o.b)("h4",{id:"configure-middleware"},"Configure Middleware"),Object(o.b)("p",null,"Finally, this is also where you want to configure middleware. There are three built-in middleware components you can turn on. I'll list them here, but for now you'll have to check out the source code for their implementation until the documentation is up."),Object(o.b)("pre",null,Object(o.b)("code",Object(r.a)({parentName:"pre"},{className:"language-csharp"}),"// Turns on using content folders to server static content\nserver.UseContentFolders();\n")),Object(o.b)("p",null,"In Grapevine 5, ",Object(o.b)("inlineCode",{parentName:"p"},"IPublicFolder")," has been replaced with ",Object(o.b)("inlineCode",{parentName:"p"},"IContentFolder"),". The implementation is still being worked on, but the functionality will be similar. You can add content folders to ",Object(o.b)("inlineCode",{parentName:"p"},"IRestServer.ContentFolders"),". The primary difference is that you will need to turn on the use of them. This can be easily done using ",Object(o.b)("inlineCode",{parentName:"p"},"server.UseContentFolders()"),". This change allowed for better seperation of concerns between the server and the router, and cleaned up a number of code smells from previous versions."),Object(o.b)("pre",null,Object(o.b)("code",Object(r.a)({parentName:"pre"},{className:"language-csharp"}),"// Turns on using correlation ids\nserver.UseCorrelationId();\n")),Object(o.b)("p",null,"Correlation ids are ids that are used to trace user requests between multiple microservices. You can turn this on to ensure that correlation ids from incoming requests get added to the outgoing response, and if no correlation id was on the request a new one is created. There are several overloads that allow you to specify the field the correlation id should be in and the method used to generate new correlation ids."),Object(o.b)("pre",null,Object(o.b)("code",Object(r.a)({parentName:"pre"},{className:"language-csharp"}),"// Turns on CORS\nserver.UseCorsPolicy();\n")),Object(o.b)("p",null,"CORS was a popular request in earlier versions of Grapevine. This middleware turns on CORS based on ",Object(o.b)("a",Object(r.a)({parentName:"p"},{href:"https://github.com/sukona/Grapevine/issues/86"}),"the technique described in this issue"),". I'd like to get some feedback on how well this works, but if the defaults provided don't work you can always create your own CORS policy and use it here via the ",Object(o.b)("inlineCode",{parentName:"p"},"Grapevine.Middleware.ICorsPolicy")," interface. (The location of that interface is subject to change.)"),Object(o.b)("h2",{id:"running-the-server"},"Running The Server"),Object(o.b)("p",null,"As always, you can start the server by calling the ",Object(o.b)("inlineCode",{parentName:"p"},"Start()")," method. This starts the server by starting the listener and the thread that listens for incoming request, and it doesn't block the execution thread. So, for those rare occasions that you ",Object(o.b)("strong",{parentName:"p"},"do want to block the thread"),", an extension method has been added that does just that."),Object(o.b)("pre",null,Object(o.b)("code",Object(r.a)({parentName:"pre"},{className:"language-csharp"}),"server.Run();\n")),Object(o.b)("p",null,"This first calls ",Object(o.b)("inlineCode",{parentName:"p"},"Start()"),", and then waits until the listener is no longer running before releasing the execution thread."))}u.isMDXComponent=!0},121:function(e,t,n){"use strict";n.d(t,"a",(function(){return d})),n.d(t,"b",(function(){return h}));var r=n(0),a=n.n(r);function o(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function i(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function s(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?i(Object(n),!0).forEach((function(t){o(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):i(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function l(e,t){if(null==e)return{};var n,r,a=function(e,t){if(null==e)return{};var n,r,a={},o=Object.keys(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||(a[n]=e[n]);return a}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(a[n]=e[n])}return a}var c=a.a.createContext({}),u=function(e){var t=a.a.useContext(c),n=t;return e&&(n="function"==typeof e?e(t):s(s({},t),e)),n},d=function(e){var t=u(e.components);return a.a.createElement(c.Provider,{value:t},e.children)},p={inlineCode:"code",wrapper:function(e){var t=e.children;return a.a.createElement(a.a.Fragment,{},t)}},b=a.a.forwardRef((function(e,t){var n=e.components,r=e.mdxType,o=e.originalType,i=e.parentName,c=l(e,["components","mdxType","originalType","parentName"]),d=u(n),b=r,h=d["".concat(i,".").concat(b)]||d[b]||p[b]||o;return n?a.a.createElement(h,s(s({ref:t},c),{},{components:n})):a.a.createElement(h,s({ref:t},c))}));function h(e,t){var n=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var o=n.length,i=new Array(o);i[0]=b;var s={};for(var l in t)hasOwnProperty.call(t,l)&&(s[l]=t[l]);s.originalType=e,s.mdxType="string"==typeof e?e:r,i[1]=s;for(var c=2;c<o;c++)i[c]=n[c];return a.a.createElement.apply(null,i)}return a.a.createElement.apply(null,n)}b.displayName="MDXCreateElement"}}]);