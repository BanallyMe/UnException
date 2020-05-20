using BanallyMe.UnException.ActionFilters.ExceptionHandling;
using BanallyMe.UnException.DependencyInjection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Xunit;

namespace BanallyMe.UnException.UnitTests.DependencyInjection
{
    public class UnExceptionLoaderTests
    {
        [Fact]
        public void AddUnException_AddsReplyOnActionWithFilterToMvcConfiguration()
        {
            var fakeServiceCollection = new ServiceCollection();

            fakeServiceCollection.AddUnException();

            var mvcConfig = GetMvcConfigurationFromServices(fakeServiceCollection);
            mvcConfig.Should().NotBeNull();
            var configurationAction = GetConfigurationActionFromService(mvcConfig);
            AssertConfigurationActionAddsReplyOnExceptionWithFilter(configurationAction);
        }

        private ServiceDescriptor GetMvcConfigurationFromServices(IServiceCollection services)
            => services.FirstOrDefault(service => service.ServiceType == typeof(IConfigureOptions<MvcOptions>));

        private Action<MvcOptions> GetConfigurationActionFromService(ServiceDescriptor service)
            => ((ConfigureNamedOptions<MvcOptions>)service.ImplementationInstance).Action;

        private void AssertConfigurationActionAddsReplyOnExceptionWithFilter(Action<MvcOptions> configurationAction)
        {
            var fakeOptions = new MvcOptions();

            configurationAction.Invoke(fakeOptions);

            AssertMvcOptionsContainReplyOnExceptionWithFilter(fakeOptions);
        }

        private void AssertMvcOptionsContainReplyOnExceptionWithFilter(MvcOptions mvcOptions)
        {
            var x = mvcOptions.Filters.Select(filter => filter.GetType());

            mvcOptions.Filters.OfType<TypeFilterAttribute>()
                .Where(attr => attr.ImplementationType == typeof(ReplyOnExceptionWithFilter))
                .Should().HaveCount(1);
        }
    }
}
