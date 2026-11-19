using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SmartInventory.API.Settings;
using SmartInventory.API.Utils;
using SmartInventory.Application.Common.Exceptions;
using Xunit;

namespace SmartInventory.UnitTests.Tenants
{
    public class TenantUtilsTests
    {

        [Fact]
        public void Test_GetSlug_ReturnsSlugFromHost()
        {
            HttpContext context = new DefaultHttpContext();
            context.Request.Host = new HostString("tenant1.example.com");
            var settings = new TenantResolutionSettings
            {
                BaseDomain = "example.com",
                SlugHeaderName = "X-Tenant-Slug"
            };
            var slug = TenantUtils.GetSlug(context, settings);
            slug.Should().Be("tenant1");
        }

        [Fact]
        public void Test_GetSlug_ReturnsSlugFromHeader()
        {
            HttpContext context = new DefaultHttpContext();
            context.Request.Host = new HostString("example.com");
            context.Request.Headers["X-Tenant-Slug"] = "tenant2";
            var settings = new TenantResolutionSettings
            {
                BaseDomain = "example.com",
                SlugHeaderName = "X-Tenant-Slug"
            };
            var slug = TenantUtils.GetSlug(context, settings);
            slug.Should().Be("tenant2");
        }

        [Fact]
        public void Test_GetSlug_ReturnsNullWhenNoSlug()
        {
            HttpContext context = new DefaultHttpContext();
            context.Request.Host = new HostString("example.com");
            var settings = new TenantResolutionSettings
            {
                BaseDomain = "example.com",
                SlugHeaderName = "X-Tenant-Slug"
            };
            var slug = TenantUtils.GetSlug(context, settings);
            slug.Should().BeNull();
        }

        [Fact]
        public void Test_GetSlug_ThrowsExceptionWhenHeaderAndHostMismatch()
        {
            HttpContext context = new DefaultHttpContext();
            context.Request.Host = new HostString("tenant1.example.com");
            context.Request.Headers["X-Tenant-Slug"] = "tenant2";
            var settings = new TenantResolutionSettings
            {
                BaseDomain = "example.com",
                SlugHeaderName = "X-Tenant-Slug"
            };
            Action act = () => TenantUtils.GetSlug(context, settings);
            act.Should().Throw<TenantException>().WithMessage("Tenant slug in header 'tenant2' does not match the domain slug 'tenant1'.");
        }

        [Fact]
        public void Test_GetSlug_TrimsAndLowercasesHeaderSlug()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("example.com");
            context.Request.Headers["X-Tenant-Slug"] = "  Tenant2  ";
            var settings = new TenantResolutionSettings
            {
                BaseDomain = "example.com",
                SlugHeaderName = "X-Tenant-Slug"
            };

            var slug = TenantUtils.GetSlug(context, settings);

            slug.Should().Be("tenant2");
        }

        [Fact]
        public void Test_GetSlug_MatchesHostAndHeaderIgnoringCase()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("Tenant1.EXAMPLE.com");
            context.Request.Headers["X-Tenant-Slug"] = "TENANT1";
            var settings = new TenantResolutionSettings
            {
                BaseDomain = "example.com",
                SlugHeaderName = "X-Tenant-Slug"
            };

            var slug = TenantUtils.GetSlug(context, settings);

            slug.Should().Be("tenant1");
        }

        [Fact]
        public void Test_GetSlug_UsesConfiguredHeaderName()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("example.com");
            context.Request.Headers["X-Custom-Tenant"] = "tenant3";
            var settings = new TenantResolutionSettings
            {
                BaseDomain = "example.com",
                SlugHeaderName = "X-Custom-Tenant"
            };

            var slug = TenantUtils.GetSlug(context, settings);

            slug.Should().Be("tenant3");
        }

        [Fact]
        public void Test_GetSlug_UsesHeaderWhenRequestHostIsAnIpAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("127.0.0.1");
            context.Request.Headers["X-Tenant-Slug"] = "tenant4";
            var settings = new TenantResolutionSettings
            {
                BaseDomain = "example.com",
                SlugHeaderName = "X-Tenant-Slug"
            };

            var slug = TenantUtils.GetSlug(context, settings);

            slug.Should().Be("tenant4");
        }
    }
}
