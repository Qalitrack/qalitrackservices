using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TechnicianApi.Tests;

[CollectionDefinition("WebApp Collection")]
public class WebApplicationFactoryCollection : ICollectionFixture<WebApplicationFactory<Program>>
{
}
