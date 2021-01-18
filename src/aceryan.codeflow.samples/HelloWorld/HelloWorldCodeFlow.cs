using System;
using aceryansoft.codeflow.core;

namespace aceryan.codeflow.samples.HelloWorld
{
    public class HelloWorldCodeFlow
    {
        public void SampleCode()
        {
            var hello = new CodeFlow();
            hello.StartNew()
                 .Call((ctx, inputs) =>
                 {
                    Console.WriteLine("Hello world"); 
                 })
                .Close();
            hello.Execute();
        }
    }
}
