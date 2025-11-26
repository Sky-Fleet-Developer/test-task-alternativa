using System;
using System.Collections.Generic;

namespace MyTestTask.Abstraction.Injection
{
    public class DiContainer
    {
        private Dictionary<Type, object> _instances = new ();
        
        public void BindInstance<TInterface, TClass>(TClass instance) where TClass : class, TInterface
        {
            if (_instances.ContainsKey(typeof(TInterface)))
            {
                throw new Exception("Instance already exists");
            }
            _instances[typeof(TInterface)] = instance;
        }

        public T Resolve<T>() where T : class
        {
            var t = typeof(T);
            if(_instances.TryGetValue(t, out var instance))
            {
                return (T)instance;
            }

            return null;
        }
    }
}