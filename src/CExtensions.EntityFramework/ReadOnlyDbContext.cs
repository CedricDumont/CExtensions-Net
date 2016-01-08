using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace CExtensions.EntityFramework
{
    public class ReadOnlyDbContextFactory
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();

        public static T Create<T>(T instance) where T : class, new()
        {
           // ProxyGenerationOptions options = new ProxyGenerationOptions(new DbContextInterceptorHook());
            
            var result = _generator.CreateClassProxyWithTarget<T>(instance, new DbContextInterceptor());

            return result;
        }

       
    }

    internal class DbContextInterceptorHook : IProxyGenerationHook
    {
        public void MethodsInspected()
        {
            
        }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
            
        }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            if (methodInfo != null && methodInfo.Name != null)
            {
                if (methodInfo.Name.Equals("SaveChanges"))
                {
                    return true;
                }
               
            }
            return false;
        }
    }

    internal class DbContextInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Method.Name;

            if(methodName == "SaveChanges")
            {
                return;
            }

            invocation.Proceed();
        }
    }
}
