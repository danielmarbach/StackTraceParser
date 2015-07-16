#region Copyright (c) 2004 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

// ReSharper disable once CheckNamespace

namespace Tests
{
    #region Imports

    using System;
    using System.Linq;
    using NUnit.Framework;
    using StackTraceParser = Elmah.StackTraceParser;

    #endregion

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class StackTraceTestCase : TestCaseAttribute
    {
        public StackTraceTestCase(string stackTrace, int index,
                 string frame,
                 string type, string method, string parameterList, string parameters,
                 string file, string line) :
            base(stackTrace, index, frame, type, method, parameterList, parameters, file, line) {}
    }

    [TestFixture]
    public sealed class StackTraceParserTests
    {
        const string DotNetStackTrace = @"
            Elmah.TestException: This is a test exception that can be safely ignored.
                at Elmah.ErrorLogPageFactory.FindHandler(String name) in C:\ELMAH\src\Elmah\ErrorLogPageFactory.cs:line 126
                at Elmah.ErrorLogPageFactory.GetHandler(HttpContext context, String requestType, String url, String pathTranslated) in C:\ELMAH\src\Elmah\ErrorLogPageFactory.cs:line 66
                at System.Web.HttpApplication.MapHttpHandler(HttpContext context, String requestType, VirtualPath path, String pathTranslated, Boolean useAppConfig)
                at System.Web.HttpApplication.MapHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()
                at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)";

        [StackTraceTestCase(DotNetStackTrace, 0,
            /* Frame         */ @"Elmah.ErrorLogPageFactory.FindHandler(String name) in C:\ELMAH\src\Elmah\ErrorLogPageFactory.cs:line 126",
            /* Type          */ @"Elmah.ErrorLogPageFactory",
            /* Method        */ @"FindHandler",
            /* ParameterList */ @"(String name)",
            /* Parameters    */ @"String name",
            /* File          */ @"C:\ELMAH\src\Elmah\ErrorLogPageFactory.cs",
            /* Line          */ @"126")]
        [StackTraceTestCase(DotNetStackTrace, 1,
            /* Frame         */ @"Elmah.ErrorLogPageFactory.GetHandler(HttpContext context, String requestType, String url, String pathTranslated) in C:\ELMAH\src\Elmah\ErrorLogPageFactory.cs:line 66",
            /* Type          */ @"Elmah.ErrorLogPageFactory",
            /* Method        */ @"GetHandler",
            /* ParameterList */ @"(HttpContext context, String requestType, String url, String pathTranslated)",
            /* Parameters    */ @"HttpContext context, String requestType, String url, String pathTranslated",
            /* File          */ @"C:\ELMAH\src\Elmah\ErrorLogPageFactory.cs",
            /* Line          */ @"66")]
        [StackTraceTestCase(DotNetStackTrace, 2,
            /* Frame         */ @"System.Web.HttpApplication.MapHttpHandler(HttpContext context, String requestType, VirtualPath path, String pathTranslated, Boolean useAppConfig)",
            /* Type          */ @"System.Web.HttpApplication",
            /* Method        */ @"MapHttpHandler",
            /* ParameterList */ @"(HttpContext context, String requestType, VirtualPath path, String pathTranslated, Boolean useAppConfig)",
            /* Parameters    */ @"HttpContext context, String requestType, VirtualPath path, String pathTranslated, Boolean useAppConfig",
            /* File          */ "",
            /* Line          */ "")]
        [StackTraceTestCase(DotNetStackTrace, 3,
            /* Frame         */  @"System.Web.HttpApplication.MapHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()",
            /* Type          */  @"System.Web.HttpApplication.MapHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep",
            /* Method        */  @"Execute",
            /* ParameterList */  @"()",
            /* Parameters    */  "",
            /* File          */  "",
            /* Line          */  "")]
        [StackTraceTestCase(DotNetStackTrace, 4,
            /* Frame         */ @"System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)",
            /* Type          */ @"System.Web.HttpApplication",
            /* Method        */ @"ExecuteStep",
            /* ParameterList */ @"(IExecutionStep step, Boolean& completedSynchronously)",
            /* Parameters    */ @"IExecutionStep step, Boolean& completedSynchronously",
            /* File          */ "",
            /* Line          */ "")]

        public void ParseDotNetStackTrace(string stackTrace, int index,
            string frame,
            string type, string method, string parameterList, string parameters,
            string file, string line)
        {
            Parse(stackTrace, index, frame, type, method, parameterList, parameters, file, line);
        }

        // See https://code.google.com/p/elmah/issues/detail?id=320

        const string MonoStackTrace = @"
            System.Web.HttpException: The controller for path '/helloworld' was not found or does not implement IController.
                at System.Web.Mvc.DefaultControllerFactory.GetControllerInstance (System.Web.Routing.RequestContext requestContext, System.Type controllerType) [0x00000] in <filename unknown>:0
                at System.Web.Mvc.DefaultControllerFactory.CreateController (System.Web.Routing.RequestContext requestContext, System.String controllerName) [0x00000] in <filename unknown>:0
                at System.Web.Mvc.MvcHandler.ProcessRequestInit (System.Web.HttpContextBase httpContext, IController& controller, IControllerFactory& factory) [0x00000] in <filename unknown>:0
                at System.Web.Mvc.MvcHandler.BeginProcessRequest (System.Web.HttpContextBase httpContext, System.AsyncCallback callback, System.Object state) [0x00000] in <filename unknown>:0
                at System.Web.Mvc.MvcHandler.BeginProcessRequest (System.Web.HttpContext httpContext, System.AsyncCallback callback, System.Object state) [0x00000] in <filename unknown>:0
                at System.Web.Mvc.MvcHandler.System.Web.IHttpAsyncHandler.BeginProcessRequest (System.Web.HttpContext context, System.AsyncCallback cb, System.Object extraData) [0x00000] in <filename unknown>:0
                at System.Web.HttpApplication+<Pipeline>c__Iterator3.MoveNext () [0x00000] in <filename unknown>:0";
        const string Zero = "0";
        const string FilenameUnknown = "filename unknown";

        [StackTraceTestCase(MonoStackTrace, 0,
            /* Frame          */ "System.Web.Mvc.DefaultControllerFactory.GetControllerInstance (System.Web.Routing.RequestContext requestContext, System.Type controllerType) [0x00000] in <filename unknown>:0",
            /* Type           */ "System.Web.Mvc.DefaultControllerFactory",
            /* Method         */ "GetControllerInstance",
            /* ParameterList  */ "(System.Web.Routing.RequestContext requestContext, System.Type controllerType)",
            /* Parameters     */ "System.Web.Routing.RequestContext requestContext, System.Type controllerType",
            FilenameUnknown, Zero)]
        [StackTraceTestCase(MonoStackTrace, 1,
            /* Frame          */ "System.Web.Mvc.DefaultControllerFactory.CreateController (System.Web.Routing.RequestContext requestContext, System.String controllerName) [0x00000] in <filename unknown>:0",
            /* Type           */ "System.Web.Mvc.DefaultControllerFactory",
            /* Method         */ "CreateController",
            /* ParameterList  */ "(System.Web.Routing.RequestContext requestContext, System.String controllerName)",
            /* Parameters     */ "System.Web.Routing.RequestContext requestContext, System.String controllerName",
            FilenameUnknown, Zero)]
        [StackTraceTestCase(MonoStackTrace, 2,
            /* Frame          */ "System.Web.Mvc.MvcHandler.ProcessRequestInit (System.Web.HttpContextBase httpContext, IController& controller, IControllerFactory& factory) [0x00000] in <filename unknown>:0",
            /* Type           */ "System.Web.Mvc.MvcHandler",
            /* Method         */ "ProcessRequestInit",
            /* ParameterList  */ "(System.Web.HttpContextBase httpContext, IController& controller, IControllerFactory& factory)",
            /* Parameters     */ "System.Web.HttpContextBase httpContext, IController& controller, IControllerFactory& factory",
            FilenameUnknown, Zero)]
        [StackTraceTestCase(MonoStackTrace, 3,
            /* Frame          */ "System.Web.Mvc.MvcHandler.BeginProcessRequest (System.Web.HttpContextBase httpContext, System.AsyncCallback callback, System.Object state) [0x00000] in <filename unknown>:0",
            /* Type           */ "System.Web.Mvc.MvcHandler",
            /* Method         */ "BeginProcessRequest",
            /* ParameterList  */ "(System.Web.HttpContextBase httpContext, System.AsyncCallback callback, System.Object state)",
            /* Parameters     */ "System.Web.HttpContextBase httpContext, System.AsyncCallback callback, System.Object state",
            FilenameUnknown, Zero)]
        [StackTraceTestCase(MonoStackTrace, 4,
            /* Frame          */ "System.Web.Mvc.MvcHandler.BeginProcessRequest (System.Web.HttpContext httpContext, System.AsyncCallback callback, System.Object state) [0x00000] in <filename unknown>:0",
            /* Type           */ "System.Web.Mvc.MvcHandler",
            /* Method         */ "BeginProcessRequest",
            /* ParameterList  */ "(System.Web.HttpContext httpContext, System.AsyncCallback callback, System.Object state)",
            /* Parameters     */ "System.Web.HttpContext httpContext, System.AsyncCallback callback, System.Object state",
            FilenameUnknown, Zero)]
        [StackTraceTestCase(MonoStackTrace, 5,
            /* Frame          */ "System.Web.Mvc.MvcHandler.System.Web.IHttpAsyncHandler.BeginProcessRequest (System.Web.HttpContext context, System.AsyncCallback cb, System.Object extraData) [0x00000] in <filename unknown>:0",
            /* Type           */ "System.Web.Mvc.MvcHandler.System.Web.IHttpAsyncHandler",
            /* Method         */ "BeginProcessRequest",
            /* ParameterList  */ "(System.Web.HttpContext context, System.AsyncCallback cb, System.Object extraData)",
            /* Parameters     */ "System.Web.HttpContext context, System.AsyncCallback cb, System.Object extraData",
            FilenameUnknown, Zero)]
        [StackTraceTestCase(MonoStackTrace, 6,
            /* Frame          */ "System.Web.HttpApplication+<Pipeline>c__Iterator3.MoveNext () [0x00000] in <filename unknown>:0",
            /* Type           */ "System.Web.HttpApplication+<Pipeline>c__Iterator3",
            /* Method         */ "MoveNext",
            /* ParameterList  */ "()",
            /* Parameters     */ "",
            FilenameUnknown, Zero)]

        public void ParseMonoStackTrace(string stackTrace, int index,
            string frame,
            string type, string method, string parameterList, string parameters,
            string file, string line)
        {
            Parse(stackTrace, index, frame, type, method, parameterList, parameters, file, line);
        }

        static void Parse(string stackTrace, int index,
            string frame,
            string type, string method, string parameterList, string parameters,
            string file, string line)
        {
            var actuals = StackTraceParser.Parse(stackTrace,
                 (f, t, m, pl, ps, fn, ln) => new
                 {
                     Frame         = f,
                     Type          = t,
                     Method        = m,
                     ParameterList = pl,
                     Parameters    = string.Join(", ", from e in ps select e.Key + " " + e.Value),
                     File          = fn,
                     Line          = ln,
                 });

            var actual = actuals.ElementAt(index);

            Assert.That(frame        , Is.EqualTo(actual.Frame)        , "Frame");
            Assert.That(type         , Is.EqualTo(actual.Type)         , "Type");
            Assert.That(method       , Is.EqualTo(actual.Method)       , "Method");
            Assert.That(parameterList, Is.EqualTo(actual.ParameterList), "ParameterList");
            Assert.That(parameters   , Is.EqualTo(actual.Parameters)   , "Parameters");
            Assert.That(file         , Is.EqualTo(actual.File)         , "File");
            Assert.That(line         , Is.EqualTo(actual.Line)         , "Line");
        }
    }
}