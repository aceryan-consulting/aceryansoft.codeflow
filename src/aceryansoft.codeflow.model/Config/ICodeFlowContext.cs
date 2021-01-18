using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
        protected ConcurrentDictionary<string, ContextProperty> ContextProperties = new ConcurrentDictionary<string, ContextProperty>();

        public CodeFlowContext()
        {
            RequestId = Guid.NewGuid().ToString();
            Host = Environment.MachineName;
            Status = Status.Pending;
            StartedBy = Environment.UserName;
        }

        public virtual T GetValue<T>(string key)
        {
            return (T) ContextProperties[key].Getter(); 
        }

        public virtual void SetValue<T>(string key, T value)
        {
            ContextProperties[key].Setter(value);
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
            if (typeof(T) == typeof(CodeFlowContext))
                return this as T;
            throw new NotImplementedException();
        }

       

     

    }

}