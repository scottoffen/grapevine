using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Grapevine.Tests;

public class ResponseStreamTests
{
    private static MemoryStream CreateInnerStream() => new();

    private static ResponseStream CreateStream(MemoryStream inner, Action? onFirstWrite = null) =>
        new(inner, onFirstWrite ?? (() => { }));

    public class Constructor
    {
        [Fact]
        public void ThrowsArgumentNullException_WhenInnerIsNull()
        {
            Should.Throw<ArgumentNullException>(() => new ResponseStream(null!, () => { }))
                .ParamName.ShouldBe("inner");
        }

        [Fact]
        public void ThrowsArgumentNullException_WhenOnFirstWriteIsNull()
        {
            Should.Throw<ArgumentNullException>(() => new ResponseStream(new MemoryStream(), null!))
                .ParamName.ShouldBe("onFirstWrite");
        }
    }

    public class Capabilities
    {
        [Fact]
        public void CanRead_ReturnsFalse()
        {
            var sut = CreateStream(CreateInnerStream());
            sut.CanRead.ShouldBeFalse();
        }

        [Fact]
        public void CanSeek_ReturnsFalse()
        {
            var sut = CreateStream(CreateInnerStream());
            sut.CanSeek.ShouldBeFalse();
        }

        [Fact]
        public void CanWrite_ReturnsTrue()
        {
            var sut = CreateStream(CreateInnerStream());
            sut.CanWrite.ShouldBeTrue();
        }
    }

    public class UnsupportedOperations
    {
        [Fact]
        public void Length_ThrowsNotSupportedException()
        {
            var sut = CreateStream(CreateInnerStream());
            Should.Throw<NotSupportedException>(() => _ = sut.Length);
        }

        [Fact]
        public void PositionGet_ThrowsNotSupportedException()
        {
            var sut = CreateStream(CreateInnerStream());
            Should.Throw<NotSupportedException>(() => _ = sut.Position);
        }

        [Fact]
        public void PositionSet_ThrowsNotSupportedException()
        {
            var sut = CreateStream(CreateInnerStream());
            Should.Throw<NotSupportedException>(() => sut.Position = 0);
        }

        [Fact]
        public void Read_ThrowsNotSupportedException()
        {
            var sut = CreateStream(CreateInnerStream());
            Should.Throw<NotSupportedException>(() => sut.Read(new byte[4], 0, 4));
        }

        [Fact]
        public void ReadAsync_ThrowsNotSupportedException()
        {
            var sut = CreateStream(CreateInnerStream());
            Should.Throw<NotSupportedException>(() => sut.ReadAsync(new byte[4], 0, 4, CancellationToken.None));
        }

        [Fact]
        public void Seek_ThrowsNotSupportedException()
        {
            var sut = CreateStream(CreateInnerStream());
            Should.Throw<NotSupportedException>(() => sut.Seek(0, SeekOrigin.Begin));
        }

        [Fact]
        public void SetLength_ThrowsNotSupportedException()
        {
            var sut = CreateStream(CreateInnerStream());
            Should.Throw<NotSupportedException>(() => sut.SetLength(0));
        }
    }

    public class WriteMethod
    {
        [Fact]
        public void InvokesCallback_OnFirstWrite()
        {
            var callCount = 0;
            var inner = CreateInnerStream();
            var sut = new ResponseStream(inner, () => callCount++);

            sut.Write(new byte[] { 1, 2, 3 }, 0, 3);

            callCount.ShouldBe(1);
        }

        [Fact]
        public void InvokesCallback_OnlyOnce_AcrossMultipleWrites()
        {
            var callCount = 0;
            var inner = CreateInnerStream();
            var sut = new ResponseStream(inner, () => callCount++);

            sut.Write(new byte[] { 1 }, 0, 1);
            sut.Write(new byte[] { 2 }, 0, 1);
            sut.Write(new byte[] { 3 }, 0, 1);

            callCount.ShouldBe(1);
        }

        [Fact]
        public void WritesBytes_ToInnerStream()
        {
            var inner = CreateInnerStream();
            var sut = CreateStream(inner);
            var data = new byte[] { 1, 2, 3 };

            sut.Write(data, 0, data.Length);

            inner.ToArray().ShouldBe(data);
        }

        [Fact]
        public void DoesNotRetryCallback_WhenCallbackThrows()
        {
            var callCount = 0;
            var inner = CreateInnerStream();
            var sut = new ResponseStream(inner, () =>
            {
                callCount++;
                throw new InvalidOperationException("commit failed");
            });

            Should.Throw<InvalidOperationException>(() => sut.Write(new byte[] { 1 }, 0, 1));

            // The second write should succeed without retrying the callback.
            Should.NotThrow(() => sut.Write(new byte[] { 2 }, 0, 1));

            callCount.ShouldBe(1);
        }
    }

    public class WriteAsyncMethod
    {
        [Fact]
        public async Task InvokesCallback_OnFirstWriteAsync()
        {
            var callCount = 0;
            var inner = CreateInnerStream();
            var sut = new ResponseStream(inner, () => callCount++);

            await sut.WriteAsync(new byte[] { 1, 2, 3 }, 0, 3, CancellationToken.None);

            callCount.ShouldBe(1);
        }

        [Fact]
        public async Task InvokesCallback_OnlyOnce_AcrossMultipleWriteAsyncs()
        {
            var callCount = 0;
            var inner = CreateInnerStream();
            var sut = new ResponseStream(inner, () => callCount++);

            await sut.WriteAsync(new byte[] { 1 }, 0, 1, CancellationToken.None);
            await sut.WriteAsync(new byte[] { 2 }, 0, 1, CancellationToken.None);
            await sut.WriteAsync(new byte[] { 3 }, 0, 1, CancellationToken.None);

            callCount.ShouldBe(1);
        }

        [Fact]
        public async Task WritesBytes_ToInnerStream()
        {
            var inner = CreateInnerStream();
            var sut = CreateStream(inner);
            var data = new byte[] { 4, 5, 6 };

            await sut.WriteAsync(data, 0, data.Length, CancellationToken.None);

            inner.ToArray().ShouldBe(data);
        }

        [Fact]
        public async Task InvokesCallback_OnlyOnce_AcrossMixedWriteAndWriteAsync()
        {
            var callCount = 0;
            var inner = CreateInnerStream();
            var sut = new ResponseStream(inner, () => callCount++);

            sut.Write(new byte[] { 1 }, 0, 1);
            await sut.WriteAsync(new byte[] { 2 }, 0, 1, CancellationToken.None);

            callCount.ShouldBe(1);
        }
    }

    public class FlushMethod
    {
        [Fact]
        public void Flush_DelegatesToInnerStream()
        {
            var inner = CreateInnerStream();
            var sut = CreateStream(inner);

            Should.NotThrow(() => sut.Flush());
        }

        [Fact]
        public async Task FlushAsync_DelegatesToInnerStream()
        {
            var inner = CreateInnerStream();
            var sut = CreateStream(inner);

            await Should.NotThrowAsync(() => sut.FlushAsync(CancellationToken.None));
        }

        [Fact]
        public void Flush_DoesNotInvokeCallback()
        {
            var callCount = 0;
            var inner = CreateInnerStream();
            var sut = new ResponseStream(inner, () => callCount++);

            sut.Flush();

            callCount.ShouldBe(0);
        }
    }
}