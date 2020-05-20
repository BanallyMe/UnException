using BanallyMe.UnException.ActionFilters.ExceptionHandling;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace BanallyMe.UnException.UnitTests.ActionFilters.ExceptionHandling
{
    public class ReplyOnExceptionWithFilterTests
    {
        private readonly Mock<ILogger<ReplyOnExceptionWithAttribute>> loggerMock;
        private readonly ReplyOnExceptionWithFilter testedFilter;
        private readonly ActionResult fakeActionResult;

        public ReplyOnExceptionWithFilterTests()
        {
            loggerMock = new Mock<ILogger<ReplyOnExceptionWithAttribute>>();
            testedFilter = new ReplyOnExceptionWithFilter(loggerMock.Object);
            fakeActionResult = new StatusCodeResult(200);
        }

        [Fact]
        public void OnActionExecuted_SetsCorrectStatusCodeIfExceptionHasExplicitFilter()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            var expectedStatusCode = StatusCodes.Status500InternalServerError;
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(new ReplyOnExceptionWithAttribute(typeof(ApplicationException), expectedStatusCode), 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void OnActionExecuted_SetsCorrectStatusCodeIfExceptionHasFilterWithDerivedExceptionType()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            var expectedStatusCode = StatusCodes.Status500InternalServerError;
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(new ReplyOnExceptionWithAttribute(typeof(Exception), expectedStatusCode), 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void OnActionExecuted_SetsStatusCodeOfMoreSpecializedFilterIfSeveralFittingFiltersAreSet()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            var expectedStatusCode = StatusCodes.Status500InternalServerError;
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(new ReplyOnExceptionWithAttribute(typeof(Exception), StatusCodes.Status501NotImplemented), 0));
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(new ReplyOnExceptionWithAttribute(typeof(ApplicationException), expectedStatusCode), 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void OnActionExecuted_SetsCorrectResultMessageIfExceptionHasExplicitFilter()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            var fakeFilter = new ReplyOnExceptionWithAttribute(typeof(ApplicationException), StatusCodes.Status500InternalServerError) { ReplyMessage = "Any Result Message." };
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(fakeFilter, 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(fakeFilter.ReplyMessage);
        }

        [Fact]
        public void OnActionExecuted_SetsCorrectResultMessageIfExceptionHasFilterWithDerivedExceptionType()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            var fakeFilter = new ReplyOnExceptionWithAttribute(typeof(Exception), StatusCodes.Status500InternalServerError) { ReplyMessage = "Any Result Message." };
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(fakeFilter, 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(fakeFilter.ReplyMessage);
        }

        [Fact]
        public void OnActionExecuted_SetsResultMessageOfMoreSpecializedFilterIfSeveralFittingFiltersAreSet()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(new ReplyOnExceptionWithAttribute(typeof(Exception), StatusCodes.Status501NotImplemented), 0));
            var appropriateFilter = new ReplyOnExceptionWithAttribute(typeof(ApplicationException), StatusCodes.Status500InternalServerError) { ReplyMessage = "Any Result Message" };
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(appropriateFilter, 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(appropriateFilter.ReplyMessage);
        }

        [Fact]
        public void OnActionExecuted_SetsResultMessageFromExceptionIfExceptionHasExplicitFilterWithoutMessage()
        {
            var fakeContext = CreateFakeContext();
            var fakeException = new ApplicationException("Any Result Message.");
            fakeContext.Exception = fakeException;
            var fakeFilter = new ReplyOnExceptionWithAttribute(typeof(ApplicationException), StatusCodes.Status500InternalServerError);
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(fakeFilter, 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(fakeException.Message);
        }

        [Fact]
        public void OnActionExecuted_SetsResultMessageFromExceptionIfExceptionHasFilterWithDerivedExceptionTypeAndwithoutMessage()
        {
            var fakeContext = CreateFakeContext();
            var fakeException = new ApplicationException("Any Result Message.");
            fakeContext.Exception = fakeException;
            var fakeFilter = new ReplyOnExceptionWithAttribute(typeof(Exception), StatusCodes.Status500InternalServerError);
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(fakeFilter, 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(fakeException.Message);
        }

        [Fact]
        public void OnActionExecuted_LogsExceptionIfExplicitFilterIsDefinedWithEnabledExceptionLogging()
        {
            var fakeContext = CreateFakeContext();
            var fakeException = new ApplicationException();
            fakeContext.Exception = fakeException;
            var fakeFilter = new ReplyOnExceptionWithAttribute(typeof(ApplicationException), StatusCodes.Status500InternalServerError);
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(fakeFilter, 0));

            testedFilter.OnActionExecuted(fakeContext);

            AssertThatExceptionHasBeenLogged(fakeException);
        }

        [Fact]
        public void OnActionExecuted_LogsExceptionIfFilterWithDerivedExceptionTypeIsDefinedWithEnabledExceptionLogging()
        {
            var fakeContext = CreateFakeContext();
            var fakeException = new ApplicationException();
            fakeContext.Exception = fakeException;
            var fakeFilter = new ReplyOnExceptionWithAttribute(typeof(Exception), StatusCodes.Status500InternalServerError);
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(fakeFilter, 0));

            testedFilter.OnActionExecuted(fakeContext);

            AssertThatExceptionHasBeenLogged(fakeException);
        }

        [Fact]
        public void OnActionExecuted_DoesntLogExceptionIfExplicitFilterIsDefinedWithDisabledExceptionLogging()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            var fakeFilter = new ReplyOnExceptionWithAttribute(typeof(Exception), StatusCodes.Status500InternalServerError) { LogException = false };
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(fakeFilter, 0));

            testedFilter.OnActionExecuted(fakeContext);

            AssertThatNothingHasBeenLogged();
        }

        [Fact]
        public void OnActionExecuted_DoesntLogExceptionIfFilterWithDerivedExceptionTypeIsDefinedWithDisabledExceptionLogging()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            var fakeFilter = new ReplyOnExceptionWithAttribute(typeof(Exception), StatusCodes.Status500InternalServerError) { LogException = false };
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(fakeFilter, 0));

            testedFilter.OnActionExecuted(fakeContext);

            AssertThatNothingHasBeenLogged();
        }

        [Fact]
        public void OnActionExecuted_SetsContextExceptionToNullIfExceptionHasExplicitFilter()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(new ReplyOnExceptionWithAttribute(typeof(ApplicationException), StatusCodes.Status500InternalServerError), 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Exception.Should().BeNull();
        }

        [Fact]
        public void OnActionExecuted_SetsContextExceptionToNullIfExceptionHasFilterWithDerivedExceptionType()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new ApplicationException();
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(new ReplyOnExceptionWithAttribute(typeof(Exception), StatusCodes.Status500InternalServerError), 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Exception.Should().BeNull();
        }

        [Fact]
        public void OnActionExecuted_DoesNothingIfNoFilterFitsToException()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new Exception();
            fakeContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(new ReplyOnExceptionWithAttribute(typeof(ArgumentException), StatusCodes.Status500InternalServerError), 0));

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().Be(fakeActionResult);
        }

        [Fact]
        public void OnActionExecuted_DoesNothingIfNoFilterHasBeenSet()
        {
            var fakeContext = CreateFakeContext();
            fakeContext.Exception = new Exception();

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().Be(fakeActionResult);
        }

        [Fact]
        public void OnActionExecuted_DoesNothingIfNoExceptionHasBeenThrownByAction()
        {
            var fakeContext = CreateFakeContext();

            testedFilter.OnActionExecuted(fakeContext);

            fakeContext.Result.Should().Be(fakeActionResult);
        }

        [Fact]
        public void OnActionExecuted_DoesNotThrowIfContextIsNull()
        {
            Action onActionExecuted = () => testedFilter.OnActionExecuted(null);

            onActionExecuted.Should().NotThrow();
        }

        public ActionExecutedContext CreateFakeContext()
        {
            var fakeActionDescriptor = new ActionDescriptor() { FilterDescriptors = new List<FilterDescriptor>() };

            return new ActionExecutedContext(new ActionContext(Mock.Of<HttpContext>(), Mock.Of<RouteData>(), fakeActionDescriptor), new List<IFilterMetadata>(), null)
            {
                Result = fakeActionResult
            };
        }

        private void AssertThatExceptionHasBeenLogged(Exception exception)
        {
            loggerMock.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => true), exception, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        private void AssertThatNothingHasBeenLogged()
        {
            loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => true), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
        }
    }
}
