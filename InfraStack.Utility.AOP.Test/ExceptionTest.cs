using InfraStack.Utility.AOP;
using System;
using System.Threading.Tasks;
using Xunit;

namespace InfraStack.Utility.AOP.Test
{
    public class ExceptionTest
    {
        internal static readonly Exception NotImplementedEx = new NotImplementedException();
        private readonly Exception _Ex = new Exception();

        [Fact]
        public void 是否為NotImplementedException()
        {
            try
            {
                ThrowNotImplementedException();
            }
            catch (Exception ex)
            {
                Assert.Equal(NotImplementedEx, ex);
            }
        }

        [Fact]
        public async Task 是否為NotImplementedExceptionAsync()
        {
            try
            {
                await ThrowNotImplementedExceptionAsync();
            }
            catch (Exception ex)
            {
                Assert.Equal(NotImplementedEx, ex);
            }

            try
            {
                await ThrowNotImplementedExceptionAwaitableAsync();
            }
            catch (Exception ex)
            {
                Assert.Equal(NotImplementedEx, ex);
            }
        }

        [Fact]
        public void 是否為原來的Exception()
        {
            try
            {
                ThrowException();
            }
            catch (Exception ex)
            {
                Assert.Equal(_Ex, ex);
            }
        }

        [Fact]
        public async Task 是否為原來的ExceptionAsync()
        {
            try
            {
                await ThrowExceptionAsync();
            }
            catch (Exception ex)
            {
                Assert.Equal(_Ex, ex);
            }

            try
            {
                await ThrowExceptionAwaitableAsync();
            }
            catch (Exception ex)
            {
                Assert.Equal(_Ex, ex);
            }
        }

        [NotImplementedException]
        private void ThrowNotImplementedException() => throw new Exception();

        [NotImplementedException]
        private Task ThrowNotImplementedExceptionAsync() => throw new Exception();

        [NotImplementedException]
        private async Task ThrowNotImplementedExceptionAwaitableAsync()
        {
            await Task.Delay(1);
            throw new Exception();
        }

        [Exception]
        private void ThrowException() => throw _Ex;

        [Exception]
        private Task ThrowExceptionAsync() => throw _Ex;

        [Exception]
        private async Task ThrowExceptionAwaitableAsync()
        {
            await Task.Delay(1);
            throw _Ex;
        }
    }

    class NotImplementedExceptionAttribute : MethodBoundaryAttribute
    {
        public override Task OnExceptionAsync(MethodArgs Args)
        {
            Args.Exception = ExceptionTest.NotImplementedEx;
            return base.OnExceptionAsync(Args);
        }
    }

    class ExceptionAttribute : MethodBoundaryAttribute
    {
        public override Task OnExceptionAsync(MethodArgs Args) => base.OnExceptionAsync(Args);
    }
}
