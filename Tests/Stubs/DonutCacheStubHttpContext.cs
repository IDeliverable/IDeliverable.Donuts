using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;

namespace IDeliverable.Donuts.Tests.Stubs
{
    public class DonutCacheStubHttpContext : HttpContextBase
    {
        private readonly string _appRelativeCurrentExecutionFilePath;
        private readonly string _hostHeader;
        private readonly IDictionary _items = new Dictionary<object, object>();

        public DonutCacheStubHttpContext()
        {
            _appRelativeCurrentExecutionFilePath = "~/yadda";
            _hostHeader = "localhost";
        }

        public DonutCacheStubHttpContext(string appRelativeCurrentExecutionFilePath)
        {
            _appRelativeCurrentExecutionFilePath = appRelativeCurrentExecutionFilePath;
            _hostHeader = "localhost";
        }

        public DonutCacheStubHttpContext(string appRelativeCurrentExecutionFilePath, string hostHeader)
        {
            _appRelativeCurrentExecutionFilePath = appRelativeCurrentExecutionFilePath;
            _hostHeader = hostHeader;
        }

        public override void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior)
        {
        }

        public override HttpRequestBase Request
        {
            get { return new StubHttpRequest(this); }
        }

        public override HttpResponseBase Response
        {
            get { return new StubHttpResponse(this); }
        }

        public override IDictionary Items
        {
            get { return _items; }
        }

        #region Nested type: StubHttpRequest

        private class StubHttpRequest : HttpRequestBase
        {
            private readonly DonutCacheStubHttpContext _httpContext;
            private NameValueCollection _headers;
            private NameValueCollection _serverVariables;

            public StubHttpRequest(DonutCacheStubHttpContext httpContext)
            {
                _httpContext = httpContext;

                ContentEncoding = Encoding.UTF8;
                RequestContext = new RequestContext
                {
                    HttpContext = httpContext
                };
            }

            public override string AppRelativeCurrentExecutionFilePath
            {
                get { return _httpContext._appRelativeCurrentExecutionFilePath; }
            }

            public override string ApplicationPath
            {
                get { return "/"; }
            }

            public override string Path
            {
                get { return _httpContext._appRelativeCurrentExecutionFilePath.TrimStart('~'); }
            }

            public override string PathInfo
            {
                get { return ""; }
            }

            public override NameValueCollection Headers
            {
                get
                {
                    return _headers = _headers
                                      ?? new NameValueCollection { { "Host", _httpContext._hostHeader } };
                }
            }

            public override NameValueCollection ServerVariables
            {
                get
                {
                    return _serverVariables = _serverVariables
                                              ?? new NameValueCollection { { "HTTP_HOST", _httpContext._hostHeader } };
                }
            }

            public override Encoding ContentEncoding { get; set; }

            public override RequestContext RequestContext { get; set; }
        }

        #endregion

        #region Nested type: StubHttpResponse

        private class StubHttpResponse : HttpResponseBase
        {
            private readonly DonutCacheStubHttpContext _httpContext;

            public StubHttpResponse(DonutCacheStubHttpContext httpContext)
            {
                _httpContext = httpContext;
            }

            public override string ApplyAppPathModifier(string virtualPath)
            {
                return "~/" + virtualPath.TrimStart('/');
            }
        }

        #endregion
    }
}