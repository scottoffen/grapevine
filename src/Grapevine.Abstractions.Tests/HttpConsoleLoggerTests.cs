using Grapevine.Abstractions;

namespace Grapevine.Abstractions.Tests;

public class HttpConsoleLoggerTests
{
    // A subclass that captures the output of FormatEntry via WriteEntry,
    // allowing us to test formatting without touching Console.Out.
    private sealed class CapturingLogger : HttpConsoleLogger
    {
        public string? LastEntry { get; private set; }
        public int WriteCount { get; private set; }

        public CapturingLogger(string? category = null, LogLevel minimumLevel = LogLevel.Trace)
            : base(category, minimumLevel)
        {
        }

        protected override void WriteEntry(string formattedEntry)
        {
            LastEntry = formattedEntry;
            WriteCount++;
        }
    }

    // A subclass that exposes FormatEntry as public for direct testing.
    private sealed class ExposingLogger : HttpConsoleLogger
    {
        public ExposingLogger(string? category = null, LogLevel minimumLevel = LogLevel.Trace)
            : base(category, minimumLevel)
        {
        }

        public string ExposeFormatEntry(LogLevel level, string message, Exception? exception = null)
            => FormatEntry(level, message, exception);
    }

    public class IsEnabledMethod
    {
        [Fact]
        public void ReturnsFalse_WhenLevelIsNone()
        {
            var sut = new HttpConsoleLogger();
            sut.IsEnabled(LogLevel.None).ShouldBeFalse();
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public void ReturnsFalse_WhenLevelIsBelowMinimumLevel(LogLevel level)
        {
            // Set minimum above every real level so all return false.
            var sut = new HttpConsoleLogger(minimumLevel: LogLevel.None);
            sut.IsEnabled(level).ShouldBeFalse();
        }

        [Theory]
        [InlineData(LogLevel.Information, LogLevel.Information)]
        [InlineData(LogLevel.Warning,     LogLevel.Information)]
        [InlineData(LogLevel.Error,       LogLevel.Information)]
        [InlineData(LogLevel.Critical,    LogLevel.Information)]
        [InlineData(LogLevel.Trace,       LogLevel.Trace)]
        public void ReturnsTrue_WhenLevelIsAtOrAboveMinimumLevel(LogLevel level, LogLevel minimum)
        {
            var sut = new HttpConsoleLogger(minimumLevel: minimum);
            sut.IsEnabled(level).ShouldBeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Trace,   LogLevel.Debug)]
        [InlineData(LogLevel.Debug,   LogLevel.Information)]
        [InlineData(LogLevel.Information, LogLevel.Warning)]
        public void ReturnsFalse_WhenLevelIsBelowConfiguredMinimum(LogLevel level, LogLevel minimum)
        {
            var sut = new HttpConsoleLogger(minimumLevel: minimum);
            sut.IsEnabled(level).ShouldBeFalse();
        }
    }

    public class LogMethod
    {
        [Fact]
        public void DoesNotCallWriteEntry_WhenLevelIsDisabled()
        {
            var sut = new CapturingLogger(minimumLevel: LogLevel.Warning);
            sut.Log(LogLevel.Debug, Guid.NewGuid().ToString());
            sut.WriteCount.ShouldBe(0);
        }

        [Fact]
        public void CallsWriteEntry_WhenLevelIsEnabled()
        {
            var sut = new CapturingLogger(minimumLevel: LogLevel.Information);
            sut.Log(LogLevel.Information, Guid.NewGuid().ToString());
            sut.WriteCount.ShouldBe(1);
        }

        [Fact]
        public void PassesFormattedEntryToWriteEntry()
        {
            var message = Guid.NewGuid().ToString();
            var sut = new CapturingLogger();
            sut.Log(LogLevel.Information, message);
            sut.LastEntry.ShouldNotBeNullOrWhiteSpace();
            sut.LastEntry.ShouldContain(message);
        }
    }

    public class FormatEntryMethod
    {
        [Fact]
        public void ContainsTimestamp_InExpectedFormat()
        {
            var sut = new ExposingLogger();
            var result = sut.ExposeFormatEntry(LogLevel.Information, Guid.NewGuid().ToString());

            // Timestamp appears between the first pair of brackets.
            var timestampStart = result.IndexOf('[') + 1;
            var timestampEnd   = result.IndexOf(']');
            var timestamp      = result.Substring(timestampStart, timestampEnd - timestampStart);

            DateTime.TryParseExact(
                timestamp,
                HttpConsoleLogger.TimestampFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out _).ShouldBeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Trace,       "Trace   ")]
        [InlineData(LogLevel.Debug,       "Debug   ")]
        [InlineData(LogLevel.Information, "Info    ")]
        [InlineData(LogLevel.Warning,     "Warning ")]
        [InlineData(LogLevel.Error,       "Error   ")]
        [InlineData(LogLevel.Critical,    "Critical")]
        public void ContainsCorrectLevelLabel(LogLevel level, string expectedLabel)
        {
            var sut    = new ExposingLogger();
            var result = sut.ExposeFormatEntry(level, Guid.NewGuid().ToString());
            result.ShouldContain($"[{expectedLabel}]");
        }

        [Fact]
        public void ContainsMessage()
        {
            var message = Guid.NewGuid().ToString();
            var sut     = new ExposingLogger();
            var result  = sut.ExposeFormatEntry(LogLevel.Information, message);
            result.ShouldContain(message);
        }

        [Fact]
        public void ContainsCategoryPrefix_WhenCategoryProvided()
        {
            var category = Guid.NewGuid().ToString();
            var message  = Guid.NewGuid().ToString();
            var sut      = new ExposingLogger(category: category);
            var result   = sut.ExposeFormatEntry(LogLevel.Information, message);
            result.ShouldContain($"{category}: {message}");
        }

        [Fact]
        public void DoesNotContainCategoryPrefix_WhenCategoryIsNull()
        {
            var message = Guid.NewGuid().ToString();
            var sut     = new ExposingLogger(category: null);
            var result  = sut.ExposeFormatEntry(LogLevel.Information, message);

            // The message should appear but without a "something: " prefix.
            result.ShouldContain(message);
            result.ShouldNotContain(": " + message);
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        public void AppendsExceptionInline_ForLevelsBelowError(LogLevel level)
        {
            var message   = Guid.NewGuid().ToString();
            var exception = new InvalidOperationException(Guid.NewGuid().ToString());
            var sut       = new ExposingLogger();
            var result    = sut.ExposeFormatEntry(level, message, exception);

            // Inline format: message -- ExceptionType: ExceptionMessage (single line)
            result.ShouldContain($"{message} -- {exception.GetType().Name}: {exception.Message}");
            result.ShouldNotContain(Environment.NewLine);
        }

        [Theory]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public void AppendsExceptionOnSeparateLine_ForErrorAndCritical(LogLevel level)
        {
            var message   = Guid.NewGuid().ToString();
            var exception = new InvalidOperationException(Guid.NewGuid().ToString());
            var sut       = new ExposingLogger();
            var result    = sut.ExposeFormatEntry(level, message, exception);

            // Full exception detail on a separate line.
            result.ShouldContain(Environment.NewLine);
            result.ShouldContain(exception.ToString());
        }

        [Fact]
        public void DoesNotContainExceptionDetail_WhenExceptionIsNull()
        {
            var message = Guid.NewGuid().ToString();
            var sut     = new ExposingLogger();
            var result  = sut.ExposeFormatEntry(LogLevel.Error, message, null);

            result.ShouldContain(message);
            result.ShouldNotContain(Environment.NewLine);
            result.ShouldNotContain("Exception");
        }
    }

    public class WriteEntryMethod
    {
        [Fact]
        public void WritesToConsoleOut()
        {
            var message        = Guid.NewGuid().ToString();
            var sut            = new HttpConsoleLogger(minimumLevel: LogLevel.Trace);
            var originalOut    = Console.Out;
            var writer         = new System.IO.StringWriter();

            try
            {
                Console.SetOut(writer);
                sut.Log(LogLevel.Information, message);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            writer.ToString().ShouldContain(message);
        }
    }

    public class Subclassing
    {
        [Fact]
        public void FormatEntry_IsCalledByLog()
        {
            var message = Guid.NewGuid().ToString();
            var sut     = new CapturingLogger();
            sut.Log(LogLevel.Information, message);

            // CapturingLogger captures whatever FormatEntry returns via WriteEntry.
            sut.LastEntry.ShouldNotBeNull();
            sut.LastEntry.ShouldContain(message);
        }

        [Fact]
        public void WriteEntry_IsCalledByLog()
        {
            var sut = new CapturingLogger();
            sut.Log(LogLevel.Information, Guid.NewGuid().ToString());
            sut.WriteCount.ShouldBe(1);
        }

        [Fact]
        public void WriteEntry_CanRedirectOutput()
        {
            // Demonstrates a consumer subclass that overrides WriteEntry to
            // split output: Info and below go to one writer, Warning and above
            // to another. This pattern works because FormatEntry embeds the
            // level label, allowing the override to inspect it.
            var infoMessage    = Guid.NewGuid().ToString();
            var warningMessage = Guid.NewGuid().ToString();
            var sut            = new SplittingLogger();

            sut.Log(LogLevel.Information, infoMessage);
            sut.Log(LogLevel.Warning, warningMessage);

            sut.NormalOutput.ShouldContain(infoMessage);
            sut.NormalOutput.ShouldNotContain(warningMessage);
            sut.ElevatedOutput.ShouldContain(warningMessage);
            sut.ElevatedOutput.ShouldNotContain(infoMessage);
        }

        // Simulates a consumer subclass that splits output by inspecting the
        // formatted entry for the level label rather than re-deriving level.
        private sealed class SplittingLogger : HttpConsoleLogger
        {
            private readonly System.IO.StringWriter _normalWriter   = new();
            private readonly System.IO.StringWriter _elevatedWriter = new();

            public string NormalOutput   => _normalWriter.ToString();
            public string ElevatedOutput => _elevatedWriter.ToString();

            public SplittingLogger() : base(minimumLevel: LogLevel.Trace) { }

            protected override void WriteEntry(string formattedEntry)
            {
                // A real stderr-redirect subclass would override Log() to track
                // the level alongside the formatted entry. Here we demonstrate
                // the simpler case: inspect the formatted string for the label.
                if (formattedEntry.Contains("[Warning ") ||
                    formattedEntry.Contains("[Error   ") ||
                    formattedEntry.Contains("[Critical"))
                    _elevatedWriter.WriteLine(formattedEntry);
                else
                    _normalWriter.WriteLine(formattedEntry);
            }
        }
    }
}