using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace aceryansoft.codeflow.model.Config
{
    public interface ICodeFlowContext : IExecutionLog
    {
        string RequestId { get; set; }
        string Host { get; set; }

        T GetValue<T>(string key);
        void SetValue<T>(string key, T value);


        List<T> GetCollection<T>(string key);
        void SetCollection<T>(string key, List<T> value);
        
        T As<T>() where T : class, ICodeFlowContext;
    } 

    public class ContextProperty 
    {
        public Func<object> Getter { get; set; }
        public Action<object> Setter { get; set; }

        public ContextProperty(Func<object> getter, Action<object> setter)
        {
            Getter = getter;
            Setter = setter;
        }
    }

    public class CodeFlowContext : ExecutionLog, ICodeFlowContext
    {
        public string RequestId { get; set; }
        public string Host { get; set; }

        public CodeFlowContext()
        {
            RequestId = Guid.NewGuid().ToString();
            Host = Environment.MachineName;
            Status = Status.Pending;
            StartedBy = Environment.UserName;
        }

        public virtual T GetValue<T>(string key)
        { 
            return (T)GetTargetProperty(key).GetValue(this); 
        }

        public virtual void SetValue<T>(string key, T value)
        {
            GetTargetProperty(key).SetValue(this, value); 
        }

        public virtual List<T> GetCollection<T>(string key)
        {
            return GetValue<List<T>>(key);
        }

        public void SetCollection<T>(string key, List<T> value)
        {
            SetValue<List<T>>(key, value);
        }

        public virtual T As<T>() where T : class,ICodeFlowContext
        {
            if (typeof(T) == GetType() || GetType().IsSubclassOf(typeof(T)))
                return this as T;
            throw new NotImplementedException();
        } 

        private PropertyInfo GetTargetProperty(string key)
        {
            var targetProp = GetType().GetProperty(key);
            if (targetProp == null)
            {
                throw new KeyNotFoundException($"Can't find property {key} in context");
            }
            return targetProp;
        }

     

    }

}