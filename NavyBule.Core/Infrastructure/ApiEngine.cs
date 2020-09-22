using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NavyBule.Core.Infrastructure.DependencyManagement;

namespace NavyBule.Core.Infrastructure
{
    public class ApiEngine : IEngine
    {
        private ITypeFinder _typeFinder;
        public ITypeFinder TypeFinder => _typeFinder ?? (_typeFinder = new TypeFinder());

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <value>The service provider.</value>
        private IServiceProvider _serviceProvider { get; set; }
        public virtual IServiceProvider ServiceProvider => _serviceProvider;

        public virtual void RegisterDependencies(ContainerBuilder containerBuilder, IConfiguration configuration)
        {
            //register engine
            containerBuilder.RegisterInstance(this).As<IEngine>().SingleInstance();
            //register type finder
            containerBuilder.RegisterInstance(_typeFinder).As<ITypeFinder>().SingleInstance();


            //find dependency registrars provided by other assemblies
            var dependencyRegistrars = _typeFinder.FindClassesOfType<IDependencyRegistrar>();

            //create and sort instances of dependency registrars
            var instances = dependencyRegistrars
                .Select(dependencyRegistrar => (IDependencyRegistrar)Activator.CreateInstance(dependencyRegistrar))
                .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);

            //register all provided dependencies
            foreach (var dependencyRegistrar in instances)
                dependencyRegistrar.Register(containerBuilder, configuration);

        }

        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            _typeFinder = new TypeFinder();
        }

        public virtual void ConfigureRequestPipeline(IApplicationBuilder application)
        {
            _serviceProvider = application.ApplicationServices;
        }
        public virtual void ConfigureServiceProvider(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            var sp = GetServiceProvider();
            if (sp == null)
                return null;
            return sp.GetService(type);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return (IEnumerable<T>)GetServiceProvider().GetServices(typeof(T));
        }

        public object ResolveUnregistered(Type type)
        {
            throw new NotImplementedException();
        }

        protected IServiceProvider GetServiceProvider()
        {
            if (ServiceProvider == null)
                return null;
            var accessor = ServiceProvider?.GetService<IHttpContextAccessor>();
            var context = accessor?.HttpContext;
            return context?.RequestServices ?? ServiceProvider;
        }
    }
}
