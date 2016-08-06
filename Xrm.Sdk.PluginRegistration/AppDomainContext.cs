﻿namespace Xrm.Sdk.PluginRegistration
{
    using System;

    public sealed class AppDomainContext<T> : IDisposable
        where T : class, new()
    {
        private AppDomain _domain;
        private bool _disposed;

        public AppDomainContext()
            : this("TemporaryDomain")
        {
        }

        public AppDomainContext(string domainName)
        {
            if (string.IsNullOrWhiteSpace(domainName))
            {
                throw new ArgumentNullException("domainName");
            }

            //Create the AppDomain
            var setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

            _domain = AppDomain.CreateDomain(domainName, null, setup);

            //Attach the resolver so that assemblies can be resolved correctly
            AssemblyResolver.AttachResolver(AppDomain.CurrentDomain);

            //Create the proxy in the AppDomain so that all assemblies that are loaded do not stay loaded in the AppDomain
            Proxy = (T)_domain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName);
        }

        ~AppDomainContext()
        {
            Dispose(false);
        }

        #region Properties

        public T Proxy
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Methods

        #region Private Methods

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (null != _domain)
            {
                try
                {
                    AppDomain.Unload(_domain);
                }
                catch (CannotUnloadAppDomainException ex)
                {
                    // AppDomain could not be uploaded probably because it was not loaded properly (due to any possible reason)
                }
                _domain = null;
                Proxy = null;
            }

            if (disposing)
            {
                _disposed = true;
            }
        }

        #endregion Private Methods
    }
}