using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Practices.EnterpriseLibrary.Validation;

using Unity;
using Unity.Injection;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.TransparentProxyInterception;
using Unity.Interception.PolicyInjection;
using Unity.Interception.PolicyInjection.MatchingRules;
using Unity.Interception.PolicyInjection.Pipeline;
using Unity.Lifetime;
using Unity.Registration;

using vm.Aspects;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Facilities;
using vm.Aspects.Policies;

namespace DiAopTest
{
    class LoggingCallHandler : ICallHandler
    {
        public IMethodReturn Invoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            // Before invoking the method on the original target
            WriteLog($"Invoking method {input.MethodBase} at {DateTime.Now.ToLongTimeString()}");

            // Invoke the next handler in the chain
            var result = getNext().Invoke(input, getNext);

            // After invoking the method on the original target
            if (result.Exception != null)
                WriteLog($"Method {input.MethodBase} threw exception {result.Exception.Message} at {DateTime.Now.ToLongTimeString()}");
            else
                WriteLog($"Method {input.MethodBase} returned {result.ReturnValue} at {DateTime.Now.ToLongTimeString()}");

            return result;
        }

        public int Order { get; set; }

        private void WriteLog(string message)
        {
            Debug.WriteLine(message);
        }
    }

    public class TrackCallHandler : BaseCallHandler<bool>
    {
        protected override bool Prepare(
            IMethodInvocation input)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Prepare");
            return base.Prepare(input);
        }

        protected override IMethodReturn PreInvoke(
            IMethodInvocation input,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Pre-invoke");
            base.PreInvoke(input, callData);
            return null;
        }

        protected override IMethodReturn DoInvoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Do-invoke");
            return base.DoInvoke(input, getNext, callData);
        }

        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Post-invoke");
            return base.PostInvoke(input, methodReturn, callData);
        }

        protected override Task<TResult> ContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: ContinueWith");
            return base.ContinueWith<TResult>(input, methodReturn, callData);
        }

        protected override Task DoContinueWith(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Task DoContinueWith");
            return base.DoContinueWith(input, methodReturn, callData);
        }

        protected override Task<TResult> DoContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData,
            TResult result)
        {
            Facility.LogWriter.TraceInfo($"{input.MethodBase.Name}: Task<TResult> DoContinueWith");
            return base.DoContinueWith<TResult>(input, methodReturn, callData, result);
        }
    }


    class CachingCallHandler : ICallHandler
    {
        static object _sync = new object();
        static IDictionary<string, object> _cache = new Dictionary<string, object>();

        public IMethodReturn Invoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            //Before invoking the method on the original target
            if (input.MethodBase.Name == nameof(ITenantStore.GetTenant))
            {
                var tenantName = input.Arguments["tenant"].ToString();

                if (IsInCache(tenantName))
                    return input.CreateMethodReturn(FetchFromCache(tenantName));
            }

            IMethodReturn result = getNext()(input, getNext);

            //After invoking the method on the original target
            if (result.Exception is null  &&
                input.MethodBase.Name == nameof(ITenantStore.SaveTenant))
                AddToCache(input.Arguments["tenant"].ToString(), input.Arguments["tenant"]);

            return result;
        }

        public int Order { get; set; }

        bool IsInCache(string key)
        {
            lock (_sync) return _cache.ContainsKey(key);
        }

        object FetchFromCache(string key)
        {
            lock (_sync) return _cache[key];
        }

        void AddToCache(string key, object item)
        {
            lock (_sync) _cache[key] = item;
        }
    }

    public class Tenant
    {
        public string Name { get; set; }

        public byte[] Logo { get; set; }

        public override string ToString() => Name;
    }

    public interface ITenantStore
    {
        void Initialize();
        Tenant GetTenant(string tenant);
        IEnumerable<string> GetTenantNames();
        void SaveTenant(Tenant tenant);
        void UploadLogo(string tenant, byte[] logo);
    }

    abstract class TenantStoreBase
    {
        protected IList<Tenant> _store = new List<Tenant>();

    }

    [Tag("test")]
    class TenantStore : TenantStoreBase, ITenantStore
    {
        public virtual void Initialize()
        {
        }

        public virtual Tenant GetTenant(string tenant) => _store.First(t => t.Name == tenant);

        public virtual IEnumerable<string> GetTenantNames() => _store.Select(t => t.Name);

        public virtual void SaveTenant(Tenant tenant) => _store.Add(tenant);

        public virtual void UploadLogo(string tenant, byte[] logo) => GetTenant(tenant).Logo = logo;
    }

    class Program
    {
        static void Main(string[] args)
        {
            ClassMetadataRegistrar
                .RegisterMetadata()
                .Register<ArgumentValidationException, ArgumentValidationExceptionDumpMetadata>()
                .Register<ValidationResult, ValidationResultDumpMetadata>()
                .Register<ValidationResults, ValidationResultsDumpMetadata>()
                .Register<ConfigurationErrorsException, ConfigurationErrorsExceptionDumpMetadata>()
                ;

            var container = DIContainer.Initialize();

            lock (container)
            {
                var registrations = DIContainer.Root.GetRegistrationsSnapshot();
                var interception = container.Configure<Interception>();


                //container.AddNewExtension<Interception>();

                interception
                    .AddPolicy("test")
                    .AddMatchingRule<TagAttributeMatchingRule>(
                        new InjectionConstructor("test", false))
                    //.AddMatchingRule<MemberNameMatchingRule>(
                    //    new InjectionConstructor(new[] { "Get*", "Save*" }, true))
                    .AddCallHandler<CachingCallHandler>(
                        new ContainerControlledLifetimeManager())
                    .AddCallHandler<LoggingCallHandler>(
                        new ContainerControlledLifetimeManager())
                    .AddCallHandler<TrackCallHandler>(
                        new ContainerControlledLifetimeManager())
                    .AddCallHandler<CallTraceCallHandler>()
                    ;

                //container.Configure<Interception>()
                //    .AddPolicy("caching")
                //    .AddMatchingRule<TagAttributeMatchingRule>(
                //        new InjectionConstructor("test", false))
                //    .AddMatchingRule<MemberNameMatchingRule>(
                //        new InjectionConstructor(new[] { "Get*", "Save*" }, true))
                //    .AddCallHandler<CachingCallHandler>(
                //        new ContainerControlledLifetimeManager(),
                //        first);

                //container.Configure<Interception>()
                //    .AddPolicy("logging")
                //    .AddMatchingRule<TagAttributeMatchingRule>(
                //        new InjectionConstructor("test", false))
                //    .AddCallHandler<LoggingCallHandler>(
                //        new ContainerControlledLifetimeManager(),
                //        second);

                //container.Configure<Interception>()
                //    .AddPolicy("tracking")
                //    .AddMatchingRule<TagAttributeMatchingRule>(
                //        new InjectionConstructor("test", false))
                //    .AddCallHandler<TrackCallHandler>(
                //        new ContainerControlledLifetimeManager(),
                //        third);

                container
                    .Register(Facility.Registrar, true)

                    .RegisterTypeIfNot<ITenantStore, TenantStore>(
                        registrations,
                        "test",
                        new InterceptionBehavior<PolicyInjectionBehavior>(),
                        new Interceptor<TransparentProxyInterceptor>());
            }

            DIContainer.Root.DebugDump();

            var store = container.Resolve<ITenantStore>("test");

            store.SaveTenant(new Tenant { Name = "gogo" });
            store.UploadLogo("gogo", new byte[] { 1, 2, 3, });

            var t = store.GetTenant("gogo");

            t.Logo = new byte[] { 11, 12, 13, };

            Debug.WriteLine(
                string.Format("\n{0}\nThat's it Val!", Facility.LogWriter.GetTestLogText()));
        }
    }

    static class ContainerRegistrationsExtension
    {
        public static string GetMappingAsString(
          this IContainerRegistration registration)
        {
            string regName, regType, mapTo, lifetime;

            var r = registration.RegisteredType;
            regType = r.Name + GetGenericArgumentsList(r);

            var m = registration.MappedToType;
            mapTo = m.Name + GetGenericArgumentsList(m);

            regName = registration.Name ?? "[default]";

            lifetime = registration.LifetimeManager.GetType().Name;
            mapTo    = mapTo != regType ? " -> " + mapTo : mapTo = string.Empty;
            lifetime = lifetime.Substring(0, lifetime.Length - "LifetimeManager".Length);

            return $"+ {regType}{mapTo}  '{regName}'  {lifetime}";
        }

        private static string GetGenericArgumentsList(Type type)
        {
            if (type.GetGenericArguments().Length == 0)
                return string.Empty;

            var list = string.Empty;
            var first = true;

            foreach (Type t in type.GetGenericArguments())
            {
                list += first ? t.Name : ", " + t.Name;
                first = false;
                if (t.GetGenericArguments().Length > 0)
                    list += GetGenericArgumentsList(t);
            }

            return "<" + list + ">";
        }
    }
}
