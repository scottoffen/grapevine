using Shouldly;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace Grapevine.Tests
{
    public class RouteTemplateTests
    {
        public class ParseEndpoint
        {
            [Fact]
            public void returns_matches_with_generic_keys()
            {
                var pattern = @"^/api/([a-zA-Z]+)/(\d+)";
                var sample = "/api/users/1234";
                var template = new RouteTemplate(pattern);

                template.Matches(sample).ShouldBeTrue();
                var matches = template.ParseEndpoint(sample);

                matches.Count.ShouldBe(2);
                matches.ContainsKey("p0").ShouldBeTrue();
                matches.ContainsKey("p1").ShouldBeTrue();
                matches["p0"].ShouldBe("users");
                matches["p1"].ShouldBe("1234");
            }

            [Fact]
            public void returns_matches_with_parameterized_keys()
            {
                var pattern = "/api/{resource:maxlength(20)}/{id:num}";
                var sample = "/api/users/1234";
                var template = new RouteTemplate(pattern);

                template.Matches(sample).ShouldBeTrue();
                var matches = template.ParseEndpoint(sample);

                matches.Count.ShouldBe(2);
                matches.ContainsKey("resource").ShouldBeTrue();
                matches.ContainsKey("id").ShouldBeTrue();
                matches["resource"].ShouldBe("users");
                matches["id"].ShouldBe("1234");
            }

            [Fact]
            public void returns_matches_with_provided_keys()
            {
                var pattern = new Regex(@"(?i)^/api/([^/]{1,20})/(\d+)$");
                var patterKeys = new List<string>()
                {
                    "resource",
                    "id"
                };
                var sample = "/api/users/1234";
                var template = new RouteTemplate(pattern, patterKeys);

                template.Matches(sample).ShouldBeTrue();
                var matches = template.ParseEndpoint(sample);

                matches.Count.ShouldBe(2);
                matches.ContainsKey("resource").ShouldBeTrue();
                matches.ContainsKey("id").ShouldBeTrue();
                matches["resource"].ShouldBe("users");
                matches["id"].ShouldBe("1234");
            }
        }

        public class ConvertToRegex
        {
            [Fact]
            public void returns_regex_with_no_capture_groups_when_no_constraints_are_included()
            {
                var pattern = "/api/resource/id";
                var expected = $"(?i)^{pattern}$";
                var patternKeys = new List<string>();

                var regex = RouteTemplate.ConvertToRegex(pattern, out patternKeys);

                regex.ToString().ShouldBe(expected);
                patternKeys.Count.ShouldBe(0);
                regex.IsMatch(pattern).ShouldBeTrue();
            }

            [Fact]
            public void returns_regex_with_default_capture_groups_when_using_generic_contraints()
            {
                var pattern = "/api/{resource}/{id}";
                var expected = @"(?i)^/api/([^/]+)/([^/]+)$";
                var sample = "/api/thing1/thing2";
                var patternKeys = new List<string>();

                var regex = RouteTemplate.ConvertToRegex(pattern, out patternKeys);

                regex.ToString().ShouldBe(expected);
                patternKeys.Count.ShouldBe(2);
                patternKeys[0].ShouldBe("resource");
                patternKeys[1].ShouldBe("id");
                regex.IsMatch(sample).ShouldBeTrue();
            }

            [Fact]
            public void returns_regex_from_string_when_string_begins_with_caret()
            {
                var pattern = @"^/api/([a-zA-Z]+)/(\d+)";
                var sample = "/api/users/1234";
                var patternKeys = new List<string>();

                var regex = RouteTemplate.ConvertToRegex(pattern, out patternKeys);

                regex.ToString().ShouldBe(pattern);
                patternKeys.Count.ShouldBe(0);
                regex.IsMatch(sample).ShouldBeTrue();
            }

            [Fact]
            public void returns_regex_with_capture_group_based_on_constraints()
            {
                var pattern = "/api/{resource:maxlength(20)}/{id:num}";
                var expected = @"(?i)^/api/([^/]{1,20})/(\d+)$";
                var sample = "/api/users/1234";
                var patternKeys = new List<string>();

                var regex = RouteTemplate.ConvertToRegex(pattern, out patternKeys);

                regex.ToString().ShouldBe(expected);
                patternKeys.Count.ShouldBe(2);
                patternKeys[0].ShouldBe("resource");
                patternKeys[1].ShouldBe("id");
                regex.IsMatch(sample).ShouldBeTrue();
            }
        }
    }
}
