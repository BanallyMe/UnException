# UnException
The UnException libraries provide a fast and easy way to handle exceptions that are thrown by an ASP.NET Core action. It uses attributes to define handled exceptions and the response to these exceptions. Being an ASP.NET Core library it is based on .NET Standard 2.0+ and coded in C# language.

## Project status
![Build and Test](https://github.com/BanallyMe/UnException/workflows/Build%20and%20Test/badge.svg)

## Why use it
It is simply one of many ways to keep actions in your controllers clean of code that is only there to handle exceptions - and makes the code harder to read. So you will get a standard way of handling exceptions which makes your code easier to read.

## How it works
The library defines an [action filter](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-3.1#action-filters) which is executed after your action and checks for thrown exceptions. If the filter recognizes an exception it will handle the exception, log it if applicable, and change the response to the desired one.
There is also a package for [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) integration which adds exception handling automatically to generated swagger documents. This package makes use of Swashbuckle's Operation filters.

## Dependencies
The library requires .NET Standard in version 2.0 at least. Using .Net Standard >= 2.1 you will benefit from nullbale reference types. The library also depends on the MVC part of the ASP.NET Core framework.
If you intend using Swashbuckle integration you will also need the Swashbuckle.AspNetCore package which is pretty obvious as the integration wouldn't make sense without using Swashbuckle in your project.

## Installation
For basic UnException features you can simply add the [NuGet package](https://www.nuget.org/packages/BanallyMe.UnException/) to your project. Swashbuckle integration has been packed to a [seperate NuGet package](https://www.nuget.org/packages/BanallyMe.UnException.Swashbuckle/) to keep the basic package clean of Swashbuckle dependencies. You can use a package manager or simply use the dotnet command to add these packages to your project.
``` shell
# Adds the basic UnException package to your project
dotnet add package BanallyMe.UnException

# Adds the package for Swashbuckle integration
dotnet add package BanallyMe.UnException.Swashbuckle
```

## Activating UnException
Both packages provide extension methods to the IServiceCollection to add UnException features to MVC and Swashbuckle. These extension methods should be called from within ConfigureServices method of Startup.cs.
``` csharp
using BanallyMe.UnException.DependencyInjection;
// If using Swashbuckle integration
using BanallyMe.UnException.Swashbuckle.DependencyInjection;

public class Startup
{
    // [...]

    public void ConfigureServices(IServiceCollection services)
    {
        // [...]
        
        // Adds basic UnException functionality to the MVC application
        services.AddUnException();
        
        // Adds Swashbuckle integration
        services.AddUnExceptionToSwashbuckle();
    }

    // [...]
}
```

## Usage
After adding UnException to the dependency injection container you can use the *ReplyOnExceptionWith* attribute to decorate all actions that should get automatic exception handling.
The attribute has five parameters to customize exception handling:

Name | Type | Mandatory | Purpose | Default
---- | ---- | --------- | ------- | -------
ExceptionType | Type | **Mandatory** | This declares which exception type should be handled automatically. You can also specify a basic exception type here (even *Exception* itself) which will lead to all derived exception types being handled by the filter. The more special exception get preferred in handling though. Unfortunately generic attributes have been delayed to C# of version 10 at least, so there is no simple way to avoid this nasty typeof declaration. | -
HttpStatusCode | int | **Mandatory** | This sets the HTTP statuscode of the automatically generated response. You can use any valid HTTP statuscode. | -
LogException | bool | *Optional* | This specifies whether the handled exception should be logged or not. If this is set to true, the handled exception will be sent to the [ASP.NET Core Logging Api](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1) before handling the exception. | true
ReplyMessage | string? | *Optional* | You can set a custom message that will effectively be sent as response body to the client, if this exception occurs. If this parameter is set to *null* the exception message of the thrown exception will be sent as response body instead. Therefore use this parameter to hide possibly sensitive exception messages - for example when handling the basic exception type as a fallback. | null
ErrorDescription | string? | *Optional* | This parameter provides a description of when and why this exception occurs. This description is used from within Swashbuckle integration to describe the response. If there are multiple handlings which lead to the same statuscode their descriptions will be merged. The swagger documentation will contain "No description provided" if this is set to null. | null

``` csharp
using Microsoft.AspNetCore.Http;
using BanallyMe.UnException.ActionFilters.ExceptionHandling;

public class ExampleController : Controller
{
    // This sends a 409 as response in case of SpecialException. Exception is not logged.
    // Response body will be the exception mesage. Swagger description is set.
    [ReplyOnExceptionWith(typeof(SpecialException), StatusCodes.Status409Conflict, LogException = false, ErrorDescription = "This resource has been added already")]
    // This sends a 500 response for all remaining exceptions (=> SpecialExceptions is handled by filter above).
    // This hides the exception message from the client by using ReplyMessage. Exception is logged.
    [ReplyOnExceptionWith(typeof(Exception), StatusCodes.Status500InternalServerError, LogException = true, ReplyMessage = "An internal error occured.", ErrorDescription = "Fallback for unhandled exceptions"]
    public IActionResult ExampleAction() {}
}
```

## Contributing
Feel free to provide pull requests to improve UnException. Please also make sure to update any tests affected by changed code.

## License
UnException is published under the [MIT license](https://choosealicense.com/licenses/mit/).
