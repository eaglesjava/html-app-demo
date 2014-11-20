﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Castle.Core.Logging;
using Castle.Windsor;

namespace WebApi.Ioc {

    public class WindsorDependencyScope : IDependencyScope {

        private IWindsorContainer container;
        private ILogger logger = NullLogger.Instance;

        public ILogger Logger {
            get { return logger; }
            set { logger = value; }
        }

        protected IWindsorContainer Container {
            get {
                return container;
            }
        }

        public WindsorDependencyScope(IWindsorContainer container) {
            this.container = container;
        }

        public void Dispose() {
            if (container.Parent != null) {
                container.RemoveChildContainer(container);
            }
            container.Dispose();
        }

        public object GetService(Type serviceType) {
            Logger.DebugFormat("GetService of type {0}", serviceType);
            object service = null;
            try {
                service = container.Resolve(serviceType);
            }
            catch (Exception ex) {
                Logger.Error(string.Format("Can not resolve {0}", serviceType), ex);
            }
            return service;
        }

        public IEnumerable<object> GetServices(Type serviceType) {
            Logger.DebugFormat("Get All Service of type {0}", serviceType);
            var services = new List<Object>();
            try {
                var resolved = container.ResolveAll(serviceType).Cast<object>();
                foreach (var service in resolved) {
                    services.Add(service);
                }
            }
            catch (Exception ex) {
                Logger.Error(string.Format("Can not resolve all {0}", serviceType), ex);
            }
            return services;
        }

    }

    public class WindsorDependencyResolver : WindsorDependencyScope, IDependencyResolver {

        public WindsorDependencyResolver(IWindsorContainer container) : base(container) { }

        public IDependencyScope BeginScope() {
            var childContainer = new WindsorContainer();
            Container.AddChildContainer(childContainer);
            return new WindsorDependencyScope(childContainer);
        }

    }

}