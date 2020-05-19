using BanallyMe.UnException.ActionFilters.ExceptionHandling;
using FluentAssertions;
using System;
using Xunit;

namespace BanallyMe.UnException.UnitTests.ActionFilters.ExceptionHandling
{
    public class ReplyOnExceptionWithAttributeTests
    {
        [Fact]
        public void Constructing_ThrowsIfExceptionTypeIsNull()
        {
            Action attributeConstruction = () => new ReplyOnExceptionWithAttribute(null, 200);

            attributeConstruction.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("exceptionType");
        }

        [Fact]
        public void Constructing_ThrowsIfExceptionTypeIsNoException()
        {
            Action attributeConstruction = () => new ReplyOnExceptionWithAttribute(typeof(ReplyOnExceptionWithAttributeTests), 200);

            attributeConstruction.Should().ThrowExactly<ArgumentException>().Which.ParamName.Should().Be("exceptionType");
        }

        [Theory]
        [InlineData(-200)]
        [InlineData(0)]
        [InlineData(99)]
        [InlineData(600)]
        public void Constructing_ThrowsIfHttpStatusCodeIsInvalid(int invalidTestStatuscode)
        {
            Action attributeConstruction = () => new ReplyOnExceptionWithAttribute(typeof(Exception), invalidTestStatuscode);

            attributeConstruction.Should().ThrowExactly<ArgumentOutOfRangeException>()
                .Which.Should().Match<ArgumentOutOfRangeException>(exc => exc.ParamName == "httpStatusCode" && (int)exc.ActualValue == invalidTestStatuscode);
        }
    }
}
