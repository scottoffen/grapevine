using Moq;

namespace Grapevine.Abstractions.Tests;

public class HttpLoggerExtensionsTests
{
    private static Mock<IHttpLogger> EnabledLogger()
    {
        var mock = new Mock<IHttpLogger>();
        mock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return mock;
    }

    private static Mock<IHttpLogger> DisabledLogger()
    {
        var mock = new Mock<IHttpLogger>();
        mock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        return mock;
    }

    public class LogTraceMethod
    {
        [Fact]
        public void CallsLog_WhenEnabled()
        {
            var message = Guid.NewGuid().ToString();
            var logger  = EnabledLogger();
            logger.Object.LogTrace(message);
            logger.Verify(l => l.Log(LogLevel.Trace, message, null), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogTrace(Guid.NewGuid().ToString());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public void CallsLog_WithException_WhenEnabled()
        {
            var message   = Guid.NewGuid().ToString();
            var exception = new Exception(Guid.NewGuid().ToString());
            var logger    = EnabledLogger();
            logger.Object.LogTrace(message, exception);
            logger.Verify(l => l.Log(LogLevel.Trace, message, exception), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WithException_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogTrace(Guid.NewGuid().ToString(), new Exception());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }
    }

    public class LogDebugMethod
    {
        [Fact]
        public void CallsLog_WhenEnabled()
        {
            var message = Guid.NewGuid().ToString();
            var logger  = EnabledLogger();
            logger.Object.LogDebug(message);
            logger.Verify(l => l.Log(LogLevel.Debug, message, null), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogDebug(Guid.NewGuid().ToString());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public void CallsLog_WithException_WhenEnabled()
        {
            var message   = Guid.NewGuid().ToString();
            var exception = new Exception(Guid.NewGuid().ToString());
            var logger    = EnabledLogger();
            logger.Object.LogDebug(message, exception);
            logger.Verify(l => l.Log(LogLevel.Debug, message, exception), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WithException_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogDebug(Guid.NewGuid().ToString(), new Exception());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }
    }

    public class LogInformationMethod
    {
        [Fact]
        public void CallsLog_WhenEnabled()
        {
            var message = Guid.NewGuid().ToString();
            var logger  = EnabledLogger();
            logger.Object.LogInformation(message);
            logger.Verify(l => l.Log(LogLevel.Information, message, null), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogInformation(Guid.NewGuid().ToString());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public void CallsLog_WithException_WhenEnabled()
        {
            var message   = Guid.NewGuid().ToString();
            var exception = new Exception(Guid.NewGuid().ToString());
            var logger    = EnabledLogger();
            logger.Object.LogInformation(message, exception);
            logger.Verify(l => l.Log(LogLevel.Information, message, exception), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WithException_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogInformation(Guid.NewGuid().ToString(), new Exception());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }
    }

    public class LogWarningMethod
    {
        [Fact]
        public void CallsLog_WhenEnabled()
        {
            var message = Guid.NewGuid().ToString();
            var logger  = EnabledLogger();
            logger.Object.LogWarning(message);
            logger.Verify(l => l.Log(LogLevel.Warning, message, null), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogWarning(Guid.NewGuid().ToString());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public void CallsLog_WithException_WhenEnabled()
        {
            var message   = Guid.NewGuid().ToString();
            var exception = new Exception(Guid.NewGuid().ToString());
            var logger    = EnabledLogger();
            logger.Object.LogWarning(message, exception);
            logger.Verify(l => l.Log(LogLevel.Warning, message, exception), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WithException_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogWarning(Guid.NewGuid().ToString(), new Exception());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }
    }

    public class LogErrorMethod
    {
        [Fact]
        public void CallsLog_WhenEnabled()
        {
            var message = Guid.NewGuid().ToString();
            var logger  = EnabledLogger();
            logger.Object.LogError(message);
            logger.Verify(l => l.Log(LogLevel.Error, message, null), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogError(Guid.NewGuid().ToString());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public void CallsLog_WithException_WhenEnabled()
        {
            var message   = Guid.NewGuid().ToString();
            var exception = new Exception(Guid.NewGuid().ToString());
            var logger    = EnabledLogger();
            logger.Object.LogError(message, exception);
            logger.Verify(l => l.Log(LogLevel.Error, message, exception), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WithException_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogError(Guid.NewGuid().ToString(), new Exception());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }
    }

    public class LogCriticalMethod
    {
        [Fact]
        public void CallsLog_WhenEnabled()
        {
            var message = Guid.NewGuid().ToString();
            var logger  = EnabledLogger();
            logger.Object.LogCritical(message);
            logger.Verify(l => l.Log(LogLevel.Critical, message, null), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogCritical(Guid.NewGuid().ToString());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public void CallsLog_WithException_WhenEnabled()
        {
            var message   = Guid.NewGuid().ToString();
            var exception = new Exception(Guid.NewGuid().ToString());
            var logger    = EnabledLogger();
            logger.Object.LogCritical(message, exception);
            logger.Verify(l => l.Log(LogLevel.Critical, message, exception), Times.Once);
        }

        [Fact]
        public void DoesNotCallLog_WithException_WhenDisabled()
        {
            var logger = DisabledLogger();
            logger.Object.LogCritical(Guid.NewGuid().ToString(), new Exception());
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }
    }
}