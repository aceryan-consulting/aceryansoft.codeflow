using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aceryansoft.codeflow.model.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;


namespace aceryansoft.codeflow.test
{
    public class SomeUser
    {
        public long Id { get; set; }
        public string Name { get; set; }        
    }

    public class BaseTestCodeFlowContext : CodeFlowContext
    {        
        public SomeUser Manager { get; set; } = new SomeUser();
        public int SomeIndex { get; set; }
    }

    public class TestCodeFlowContext : BaseTestCodeFlowContext
    {
        public List<SomeUser> Users { get; set; } = new List<SomeUser>();
        public ConcurrentBag<int> ThreadCallStack { get; set; } = new ConcurrentBag<int>();
        public ConcurrentBag<string> ConcurrentCallStack { get; set; } = new ConcurrentBag<string>();     
    }


    [TestClass]
    public class CodeFlowContextTest
    {
        [TestMethod]
        public void should_set_and_retrieve_context_properties_and_collections()
        {
            var context = new TestCodeFlowContext();
            context.SetValue<int>("SomeIndex", 7); 
            context.SetValue<SomeUser>("Manager", new SomeUser() { Id=4,Name="it manager"});
            var sampleUsers = new List<SomeUser>()
            {
                new SomeUser() { Id=14,Name="yannick"},
                new SomeUser() { Id=15,Name="pierre"} 
            };
            context.SetCollection<SomeUser>("Users", sampleUsers);  
            
            Check.That(context.SomeIndex).Equals(7);
            Check.That(context.SomeIndex).Equals(context.GetValue<int>("SomeIndex"));

            Check.That(context.Manager.Id).Equals(4);
            Check.That(context.GetValue<SomeUser>("Manager").Id).Equals(4);
            Check.That(context.Manager.Name).Equals("it manager");
            Check.That(context.GetValue<SomeUser>("Manager").Name).Equals("it manager");

            var users = context.GetCollection<SomeUser>("Users");
            foreach(var usr in users)
            {
               var matchingUsr = sampleUsers.FirstOrDefault(x => x.Id == usr.Id && x.Name == usr.Name);
                Check.That(matchingUsr).IsNotNull();
            }
        }

        [TestMethod]
        public void should_throw_exception_if_trying_to_retrieve_missing_context_properties()
        {
            var context = new TestCodeFlowContext();
            context.SetValue<int>("SomeIndex", 7);
            Check.ThatCode(() =>
            {
                var savedValue = context.GetValue<int>("SomeIndex_missing");
            }).Throws<KeyNotFoundException>();
        }

        [TestMethod]
        public void should_throw_exception_if_trying_to_save_invalid_type_on_context_property()
        {
            var context = new TestCodeFlowContext();

            Check.ThatCode(() =>
            {
                context.SetValue<string>("SomeIndex", "t");
            }).Throws<ArgumentException>();
        }

        [TestMethod]
        public void should_throw_exception_if_trying_to_retrieve_invalid_type_on_context_property()
        {
            var context = new TestCodeFlowContext();
            context.SetValue<int>("SomeIndex", 7);
            Check.ThatCode(() =>
            {
                var savedValue = context.GetValue<string>("SomeIndex");
            }).Throws<Exception>();
        }

        [TestMethod]
        public void should_retrieve_context_properties_and_collections_in_parallel_execution()
        {
            var context = new TestCodeFlowContext();
            context.SetValue<int>("SomeIndex", 7);
            context.SetValue<SomeUser>("Manager", new SomeUser() { Id = 4, Name = "it manager" });
            var sampleUsers = new List<SomeUser>()
            {
                new SomeUser() { Id=14,Name="yannick"},
                new SomeUser() { Id=15,Name="pierre"}
            };
            context.SetCollection<SomeUser>("Users", sampleUsers);

            Parallel.ForEach(Enumerable.Range(1, 10), (index) =>
             {
                 Thread.Sleep(100);
                 var manager = context.GetValue<SomeUser>("Manager");
                 var someIndex = context.GetValue<int>("SomeIndex");
                 var users = context.GetCollection<SomeUser>("Users"); 
                 context.ThreadCallStack.Add(Thread.CurrentThread.ManagedThreadId);
             });
            Check.That(context.ThreadCallStack.Distinct().Count()).IsStrictlyGreaterThan(1);
        }

        [TestMethod]
        public void should_cast_context_to_itself_or_any_parent_type()
        {
            var context = new TestCodeFlowContext();
            context.SetValue<int>("SomeIndex", 7);
            context.SetValue<SomeUser>("Manager", new SomeUser() { Id = 4, Name = "it manager" });

            var sameContext = context.As<TestCodeFlowContext>();
            Check.That(sameContext).IsNotNull(); 
            Check.That(sameContext.GetValue<int>("SomeIndex")).IsEqualTo(7);

            var baseContext = context.As<BaseTestCodeFlowContext>();
            Check.That(baseContext).IsNotNull();
            Check.That(baseContext.GetValue<SomeUser>("Manager").Id).IsEqualTo(4);

            var topLevelContext = context.As<CodeFlowContext>();
            Check.That(topLevelContext).IsNotNull(); 
        }
    }
}
