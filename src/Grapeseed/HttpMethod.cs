using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Grapevine
{
    public partial class HttpMethod
    {
        public static HttpMethod Any { get; } = new HttpMethod("ANY");
        public static HttpMethod Get { get; } = new HttpMethod("GET");
        public static HttpMethod Put { get; } = new HttpMethod("PUT");
        public static HttpMethod Post { get; } = new HttpMethod("POST");
        public static HttpMethod Delete { get; } = new HttpMethod("DELETE");
        public static HttpMethod Head { get; } = new HttpMethod("HEAD");
        public static HttpMethod Options { get; } = new HttpMethod("OPTIONS");
        public static HttpMethod Trace { get; } = new HttpMethod("TRACE");
        public static HttpMethod Patch { get; } = new HttpMethod("PATCH");
        public static HttpMethod Connect { get; } = new HttpMethod("CONNECT");

        // for implicit conversions
        private static readonly Dictionary<string, HttpMethod> _methods;

        static HttpMethod()
        {
            _methods = new Dictionary<string, HttpMethod>();
            _methods.Add("POST", Post);
            _methods.Add("PUT", Put);
            _methods.Add("DELETE", Delete);
            _methods.Add("HEAD", Head);
            _methods.Add("GET", Get);
            _methods.Add("ANY", Any);
            _methods.Add("OPTIONS", Options);
            _methods.Add("TRACE", Trace);
            _methods.Add("PATCH", Patch);
            _methods.Add("CONNECT", Connect);
        }

        public static bool operator ==(HttpMethod left, HttpMethod right)
        {
            return left is null || right is null
                ? ReferenceEquals(left, right)
                : left.Equals(right);
        }

        public static bool operator !=(HttpMethod left, HttpMethod right)
        {
            return !(left == right);
        }

        public static implicit operator string(HttpMethod value)
        {
            return value.ToString();
        }

        public static implicit operator HttpMethod(string value)
        {
            var key = value.ToUpper();
            if (_methods.ContainsKey(key)) return _methods[key];

            var method = new HttpMethod(value);
            _methods.Add(method.ToString(), method);
            return method;
        }
    }

    public partial class HttpMethod : IEquatable<HttpMethod>
    {
        private readonly int _hashcode;

        public HttpMethod(string method)
        {
            Method = method.Trim().ToUpper();
            _hashcode = StringComparer.OrdinalIgnoreCase.GetHashCode(Method);
        }

        public string Method { get; }

        public bool Equivalent(HttpMethod other)
        {
            if (this.Equals(Any) || other.Equals(Any)) return true;
            return this.Equals(other);
        }

        public bool Equals(HttpMethod other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(Method, other.Method)) return true;
            return string.Equals(Method, other.Method, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HttpMethod);
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }

        public override string ToString()
        {
            return Method;
        }
    }
}
