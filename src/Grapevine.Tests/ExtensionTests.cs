using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grapevine;
using Xunit;
using Moq;
using System.IO;

namespace Grapevine.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void ParseFormUrlEncodedDataTest()
        {
            IHttpRequest s = Mock.Of<IHttpRequest>((m) =>
                                                        m.InputStream == Stream.Null &&
                                                        m.Url == new Uri("http://api.some.test/api/test?username=moqTest&password=insecure")
                                                    );

            IDictionary<string, string> r = null;

            r = s.ParseFormUrlEncodedData().Result;

            Assert.Equal(2, r.Count);
            Assert.True(r.ContainsKey("username"));
            Assert.True(r.ContainsKey("password"));
            Assert.Equal("moqTest", r["username"]);
            Assert.Equal("insecure", r["password"]);

            r = s.ParseFormUrlEncodedData(true).Result;

            Assert.Equal(2, r.Count);
            Assert.True(r.ContainsKey("username"));
            Assert.True(r.ContainsKey("password"));
            Assert.Equal("moqTest", r["username"]);
            Assert.Equal("insecure", r["password"]);

            using (MemoryStream ms = new(Encoding.UTF8.GetBytes("username=moqTest&password=insecure")))
            {
                s = Mock.Of<IHttpRequest>((m) =>
                                                m.InputStream == ms &&
                                                m.Url == new Uri("http://api.some.test/api/test?username=moqTest&password=insecure")
                                            );

                r = s.ParseFormUrlEncodedData().Result;

                Assert.Equal(2, r.Count);
                Assert.True(r.ContainsKey("username"));
                Assert.True(r.ContainsKey("password"));
                Assert.Equal("moqTest", r["username"]);
                Assert.Equal("insecure", r["password"]);
            }
        }
    }
}
