using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace Grapevine.Tests
{
    public class RouteConstraintTests
    {
        public class AlphaResolver
        {
            [Fact]
            public void returns_alpha_pattern_with_repetition_quantifier()
            {
                var constraints = "alpha".Split(':').ToList();
                var expected = "([a-zA-Z]+)";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_alpha_pattern_with_minimum_length_quantifier()
            {
                var constraints = "alpha:minlength(2)".Split(':').ToList();
                var expected = "([a-zA-Z]{2,})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_alpha_pattern_with_maximum_length_quantifier()
            {
                var constraints = "alpha:maxlength(2)".Split(':').ToList();
                var expected = "([a-zA-Z]{1,2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_alpha_pattern_with_exact_length_quantifier()
            {
                var constraints = "alpha:length(2)".Split(':').ToList();
                var expected = "([a-zA-Z]{2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_alpha_pattern_with_range_length_quantifier()
            {
                var constraints = "alpha:length(2,5)".Split(':').ToList();
                var expected = "([a-zA-Z]{2,5})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }
        }

        public class AlphaNumericResolver
        {
            [Fact]
            public void returns_alphanum_pattern_with_repetition_quantifier()
            {
                var constraints = "alphanum".Split(':').ToList();
                var expected = @"(\w+)";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_alphanum_pattern_with_minimum_length_quantifier()
            {
                var constraints = "alphanum:minlength(2)".Split(':').ToList();
                var expected = @"(\w{2,})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_alphanum_pattern_with_maximum_length_quantifier()
            {
                var constraints = "alphanum:maxlength(2)".Split(':').ToList();
                var expected = @"(\w{1,2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_alphanum_pattern_with_exact_length_quantifier()
            {
                var constraints = "alphanum:length(2)".Split(':').ToList();
                var expected = @"(\w{2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_alphanum_pattern_with_range_length_quantifier()
            {
                var constraints = "alphanum:length(2,5)".Split(':').ToList();
                var expected = @"(\w{2,5})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }
        }

        public class CustomResolver
        {
            private static readonly string expectedResult = "RESOLVED";

            private static readonly RouteConstraintResolver customResolver = (value) => { return expectedResult; };

            [Fact]
            public void throws_exception_when_attempting_to_override_protected_resolver()
            {
                Should.Throw<ArgumentException>(() =>
                {
                    RouteConstraints.AddResolver("num", customResolver);
                });
            }

            [Fact]
            public void can_add_custom_resovler()
            {
                var customKey = "custom";
                RouteConstraints.AddResolver(customKey, customResolver);

                var actual = RouteConstraints.Resolve(new List<string>() { customKey });

                actual.ShouldBe(expectedResult);
            }
        }

        public class GuidResolver
        {
            [Fact]
            public void returns_guid_pattern_that_matches_guids()
            {
                var guidBase = Guid.NewGuid().ToString();
                var guids = new List<string>()
                {
                    guidBase.Replace("-", ""),
                    guidBase.ToLower(),
                    guidBase.ToUpper(),
                    "{" + guidBase + "}",
                    $"({guidBase})"
                };

                var notguid = guidBase.Replace("-", "=");
                var constraints = new List<string>() { "guid" };
                var pattern = new Regex($"^{RouteConstraints.Resolve(constraints)}$");

                foreach (var guid in guids)
                {
                    pattern.IsMatch(guid).ShouldBeTrue();
                }

                pattern.IsMatch(notguid).ShouldBeFalse();
            }
        }

        public class NumericResolver
        {
            [Fact]
            public void returns_numeric_pattern_with_repetition_quantifier()
            {
                var constraints = "num".Split(':').ToList();
                var expected = @"(\d+)";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_numeric_pattern_with_minimum_length_quantifier()
            {
                var constraints = "num:minlength(2)".Split(':').ToList();
                var expected = @"(\d{2,})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_numeric_pattern_with_maximum_length_quantifier()
            {
                var constraints = "num:maxlength(2)".Split(':').ToList();
                var expected = @"(\d{1,2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_numeric_pattern_with_exact_length_quantifier()
            {
                var constraints = "num:length(2)".Split(':').ToList();
                var expected = @"(\d{2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_numeric_pattern_with_range_length_quantifier()
            {
                var constraints = "num:length(2,5)".Split(':').ToList();
                var expected = @"(\d{2,5})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }
        }

        public class StringResolver
        {
            [Fact]
            public void returns_string_pattern_with_repetition_quantifier()
            {
                var constraints = "string".Split(':').ToList();
                var expected = @"([^/]+)";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_string_pattern_with_minimum_length_quantifier()
            {
                var constraints = "string:minlength(2)".Split(':').ToList();
                var expected = @"([^/]{2,})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_string_pattern_with_maximum_length_quantifier()
            {
                var constraints = "string:maxlength(2)".Split(':').ToList();
                var expected = @"([^/]{1,2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_string_pattern_with_exact_length_quantifier()
            {
                var constraints = "string:length(2)".Split(':').ToList();
                var expected = @"([^/]{2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_string_pattern_with_range_length_quantifier()
            {
                var constraints = "string:length(2,5)".Split(':').ToList();
                var expected = @"([^/]{2,5})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }
        }

        public class UnspecifiedResolver
        {
            [Fact]
            public void returns_default_pattern_when_no_constraint_is_provided()
            {
                List<string> constraints = new List<string>();
                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(RouteConstraints.DefaultPattern);
            }

            [Fact]
            public void returns_string_pattern_with_minimum_length_quantifier()
            {
                var constraints = "minlength(2)".Split(':').ToList();
                var expected = @"([^/]{2,})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_string_pattern_with_maximum_length_quantifier()
            {
                var constraints = "maxlength(2)".Split(':').ToList();
                var expected = @"([^/]{1,2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_string_pattern_with_exact_length_quantifier()
            {
                var constraints = "length(2)".Split(':').ToList();
                var expected = @"([^/]{2})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }

            [Fact]
            public void returns_string_pattern_with_range_length_quantifier()
            {
                var constraints = "length(2,5)".Split(':').ToList();
                var expected = @"([^/]{2,5})";

                var pattern = RouteConstraints.Resolve(constraints);

                pattern.ShouldBe(expected);
            }
        }

    }
}