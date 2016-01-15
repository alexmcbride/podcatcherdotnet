using Caliburn.Micro;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace PodcatcherDotNet {
    public class AppBootstrapper : BootstrapperBase, IDisposable {
        private const string AppGuid = "9a8d7427-23a9-4938-9d1c-fb1ce2825915";

        private SimpleContainer _container;
        private Mutex _singleInstanceMutex;

        public AppBootstrapper() {
#if !DEBUG
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

            Initialize();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            UnhandledException((Exception)e.ExceptionObject);
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            UnhandledException(e.Exception);
        }

        private void UnhandledException(Exception exception) {
            try {
                LogHelper.WriteLine(exception.ToString());

                MessageBox.Show(
                    String.Format("An unhandled exception occured, the file 'Log.txt' has been written"),
                    "Unhandled Error");
            }
            finally {
                Process.GetCurrentProcess().Kill();
            }
        }

        protected override void Configure() {
            _container = new SimpleContainer();
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key) {
            var instance = _container.GetInstance(service, key);
            if (instance != null) {
                return instance;
            }
            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance) {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e) {
            if (IsSingleInstance(e.Args)) {
                DisplayRootViewFor<ShellViewModel>();
            }
        }

        private bool IsSingleInstance(string[] args) {
            bool createdNew = false;

            // We add username to distinqush from versions of the program running under another user's profile.
            var appId = Environment.UserName + AppGuid;

            _singleInstanceMutex = new Mutex(true, appId, out createdNew);

            if (!createdNew) {                
                Application.Shutdown();
            }

            return createdNew;
        }

        public void Dispose() {
            if (_singleInstanceMutex != null) {
                _singleInstanceMutex.Dispose();
            }
        }
    }
}