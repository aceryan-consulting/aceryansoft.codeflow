using System;
using System.Collections.Generic;
using System.Text;
using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.FlowApi;


namespace aceryan.codeflow.samples.GettingStarted
{ 
    public class ExtensionCodeFlow
    {
        public void SampleCode()
        {
            var hello = new CodeFlow();
            hello.StartNew()
                .Call((ctx, inputs) =>
                {
                    Console.WriteLine("Hello world");
                })
                .CustomExtension((ctx, inputs) => // add CustomExtension to the fluent api
                {
                    Console.WriteLine("How are you today ?");
                })
                .Close();
            hello.Execute();
        }
    }


    public static class MyExtensions
    {
        public static ICodeFlowApi CustomExtension(this ICodeFlowApi codeFlowApi,Action<ICodeFlowContext, object[]> customAction)
        {
            codeFlowApi.Call((ctx, inputs) =>   
            {
                // write awesome logic before  
                customAction(ctx,inputs);
                // write awesome logic after  
            });
            return codeFlowApi; 
        }
    }


}
