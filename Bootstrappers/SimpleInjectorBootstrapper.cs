using SimpleInjector;
using Stylet;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bootstrappers
{
    public class SimpleInjectorBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private Container _container;
        private object _rootViewModel;

        protected virtual object RootViewModel => this._rootViewModel ?? (this._rootViewModel = this.GetInstance(typeof(TRootViewModel)));

        protected override void ConfigureBootstrapper()
        {
            Container container = new Container();
            DefaultConfigureIoC(container);
            ConfigureIoC(container);

            container.Verify();
            _container = container;
        }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(Container container)
        {
            var viewManagerConfig = new ViewManagerConfig()
            {
                ViewFactory = this.GetInstance,
                ViewAssemblies = new List<Assembly>() { this.GetType().Assembly }
            };
            var viewManager = new ViewManager(viewManagerConfig);
            container.RegisterInstance<IViewManager>(viewManager);

            container.RegisterInstance<IWindowManagerConfig>(this);
            container.RegisterInstance<IWindowManager>(new WindowManager(viewManager, container.GetInstance<IMessageBoxViewModel>, this));
            container.Register<IEventAggregator, EventAggregator>(Lifestyle.Singleton);
            container.Register<IMessageBoxViewModel, MessageBoxViewModel>(Lifestyle.Transient); // Not singleton
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(Container container) { }

        public override object GetInstance(Type type)
        {
            return _container.GetInstance(type);
        }

        protected override void Launch()
        {
            base.DisplayRootView(this.RootViewModel);
        }

        public override void Dispose()
        {
            ScreenExtensions.TryDispose(this._rootViewModel);
            _container?.Dispose();
            base.Dispose();
        }
    }
}