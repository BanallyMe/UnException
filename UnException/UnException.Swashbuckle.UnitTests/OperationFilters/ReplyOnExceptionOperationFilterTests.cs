using BanallyMe.UnException.ActionFilters.ExceptionHandling;
using BanallyMe.UnException.Swashbuckle.OperationFilters;
using FluentAssertions;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;
using Xunit;

namespace BanallyMe.UnException.Swashbuckle.UnitTests.OperationFilters
{
    public class ReplyOnExceptionOperationFilterTests
    {
        private readonly IOperationFilter testedFilter;
        private readonly Mock<ISchemaGenerator> schemaGenerator;
        private const int fakeReplyStatuscode = 500;
        private const string errorDescription1 = "ReplyMessage1";
        private const string errorDescription2 = "ReplyMessage2";

        public ReplyOnExceptionOperationFilterTests()
        {
            testedFilter = new ReplyOnExceptionOperationFilter();
            schemaGenerator = new Mock<ISchemaGenerator>();
        }

        [Fact]
        public void Apply_AddsAutomaticReplyWithReplyMessageToOperationResponses()
        {
            var testedAction = typeof(FakeController).GetMethod(nameof(FakeController.FakeActionWithReplyAttributeAndReplyText));

            AssertResponseWithDescriptionIsAddedForMethod(errorDescription1, testedAction);
        }

        [Fact]
        public void Apply_AddsAutomaticReplyWithStandardDescriptionIfNoReplyMessageProvided()
        {
            var testedAction = typeof(FakeController).GetMethod(nameof(FakeController.FakeActionWithReplyAttribute));

            AssertResponseWithDescriptionIsAddedForMethod("No description provided", testedAction);
        }

        [Fact]
        public void Apply_AddsOnlyOneResponseWithMergedDescriptionsForSameStatuscode()
        {
            var testedAction = typeof(FakeController).GetMethod(nameof(FakeController.FakeActionWithMultipleReplyAttributesForSameStatuscode));
            var expectedDescription = errorDescription1 + " / " + errorDescription2;

            AssertResponseWithDescriptionIsAddedForMethod(expectedDescription, testedAction);
        }

        private void AssertResponseWithDescriptionIsAddedForMethod(string description, MethodInfo method)
        {
            var fakeContext = new OperationFilterContext(null, schemaGenerator.Object, null, method);
            var fakeSchema = new OpenApiSchema();
            schemaGenerator.Setup(gen => gen.GenerateSchema(typeof(string), fakeContext.SchemaRepository, null, null, null)).Returns(fakeSchema);
            var fakeOperation = new OpenApiOperation { Responses = new OpenApiResponses() };

            var expectedResponseStatusCode = fakeReplyStatuscode.ToString(System.Globalization.CultureInfo.InvariantCulture);
            const string expectedContentType = "text/plain";

            testedFilter.Apply(fakeOperation, fakeContext);

            fakeOperation.Responses.Should().ContainKey(expectedResponseStatusCode);
            fakeOperation.Responses[expectedResponseStatusCode].Content.Should().HaveCount(1).And.ContainKey(expectedContentType);
            fakeOperation.Responses[expectedResponseStatusCode].Description.Should().Be(description);
            fakeOperation.Responses[expectedResponseStatusCode].Content[expectedContentType].Schema.Should().Be(fakeSchema);
        }

        [Fact]
        public void Apply_DoesntAddResponseIfActionNotDecoratedWithReplyOnExceptionAttribute()
        {
            var fakeOperation = new OpenApiOperation();
            var fakeContext = new OperationFilterContext(null, null, null, typeof(FakeController)
                .GetMethod(nameof(FakeController.FakeActionWithoutReplyAttribute)));

            testedFilter.Apply(fakeOperation, fakeContext);

            fakeOperation.Responses.Should().BeEmpty();
        }

        [Fact]
        public void Apply_DoesntThrowExceptionIfOperationIsNull()
        {
            Action applying = () => testedFilter.Apply(null, new OperationFilterContext(null, null, null, null));

            applying.Should().NotThrow();
        }

        [Fact]
        public void Apply_DoesntThrowExceptionIfContextIsNull()
        {
            Action applying = () => testedFilter.Apply(Mock.Of<OpenApiOperation>(), null);

            applying.Should().NotThrow();
        }

        private class FakeController
        {
            [ReplyOnExceptionWith(typeof(Exception), fakeReplyStatuscode)]
            public void FakeActionWithReplyAttribute() { }

            [ReplyOnExceptionWith(typeof(Exception), fakeReplyStatuscode, ErrorDescription = errorDescription1)]
            public void FakeActionWithReplyAttributeAndReplyText() { }

            public void FakeActionWithoutReplyAttribute() { }

            [ReplyOnExceptionWith(typeof(Exception), fakeReplyStatuscode, ErrorDescription = errorDescription1)]
            [ReplyOnExceptionWith(typeof(Exception), fakeReplyStatuscode, ErrorDescription = errorDescription2)]
            public void FakeActionWithMultipleReplyAttributesForSameStatuscode() { }
        }
    }
}
