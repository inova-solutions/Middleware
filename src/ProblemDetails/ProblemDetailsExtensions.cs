﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Hellang.Middleware.ProblemDetails
{
    public static class ProblemDetailsExtensions
    {
        public static IServiceCollection AddProblemDetails(this IServiceCollection services)
        {
            return services.AddProblemDetails(configure: null);
        }

        public static IServiceCollection AddProblemDetails(this IServiceCollection services, Action<ProblemDetailsOptions> configure)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }

            services.TryAddSingleton<ProblemDetailsMarkerService, ProblemDetailsMarkerService>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<ProblemDetailsOptions>, ProblemDetailsOptionsSetup>());

            return services;
        }

        public static IApplicationBuilder UseProblemDetails(this IApplicationBuilder app)
        {
            var markerService = app.ApplicationServices.GetService<ProblemDetailsMarkerService>();

            if (markerService is null)
            {
                throw new InvalidOperationException(
                    $"Please call {nameof(IServiceCollection)}.{nameof(AddProblemDetails)} in ConfigureServices before adding the middleware.");
            }

            return app.UseMiddleware<ProblemDetailsMiddleware>();
        }

        [Obsolete("This overload is deprecated. Please call " + nameof(IServiceCollection) + "." + nameof(AddProblemDetails) + " and use the parameterless overload instead.")]
        public static IApplicationBuilder UseProblemDetails(this IApplicationBuilder app, Action<ProblemDetailsOptions> configure)
        {
            var options = new ProblemDetailsOptions();

            configure?.Invoke(options);

            // Try to pull the IConfigureOptions<ProblemDetailsOptions> service from the container
            // in case the user has called AddProblemDetails and still use this overload.
            // If the setup hasn't been configured. Create an instance explicitly and use that.
            var setup = app.ApplicationServices.GetService<IConfigureOptions<ProblemDetailsOptions>>() ?? new ProblemDetailsOptionsSetup();

            setup.Configure(options);

            return app.UseMiddleware<ProblemDetailsMiddleware>(Options.Create(options));
        }

        /// <summary>
        /// A marker class used to determine if the required services were added
        /// to the <see cref="IServiceCollection"/> before the middleware is configured.
        /// </summary>
        private class ProblemDetailsMarkerService
        {
        }
    }
}
