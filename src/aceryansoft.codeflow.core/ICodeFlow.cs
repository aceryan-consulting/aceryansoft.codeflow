using aceryansoft.codeflow.core.Config;
using aceryansoft.codeflow.core.FlowApi;
using aceryansoft.codeflow.model.Config;
using aceryansoft.codeflow.model.FlowApi;
using System;
using aceryansoft.codeflow.core.Containers;

namespace aceryansoft.codeflow.core
{
    /// <summary>
    /// Code Flow Orchestrator interface
    /// </summary>
    public interface ICodeFlow  
    {
        /// <summary>
        /// build and execute code flow
        /// </summary>
        /// <returns>code flow context after all activities execution</returns>
        ICodeFlowContext Execute();

        /// <summary>
        /// set up new code flow configuration, instance and block path
        /// </summary>
        /// <param name="configAction">set up action on code flow configuration</param>
        /// <returns>fluent block path for code flow definition</returns>
        ICodeFlowApi StartNew(Action<ICodeFlowExecutionConfig> configAction = null);
    }

    // todo create nuget package command
    // msbuild -t:pack -p:Configuration=Release -restore 

    /// <summary>
    /// main code flow execution class
    /// </summary>
    public sealed class CodeFlow  
    { 
        //private CodeFlowBlockPath _codeFlowBlockPath;
        private CodeFlowApi _codeFlowApi;
         

        /// <summary>
        /// execute code flow middlewares and activities
        /// </summary>
        /// <returns>code flow context after all activities</returns>
        public ICodeFlowContext Execute()
        {
            return _codeFlowApi.Execute(); 
        }
                
        /// <summary>
        /// set up new code flow configuration, instance and block path
        /// </summary>
        /// <param name="configAction">set up action on code flow configuration</param>
        /// <returns>fluent block path for code flow definition</returns>
        public ICodeFlowApi StartNew(Action<ICodeFlowExecutionConfig> configAction = null)
        {
            var config = new CodeFlowExecutionConfig();
            configAction?.Invoke(config);
            _codeFlowApi = new CodeFlowApi( new SequenceContainerFlow(), config);
            return _codeFlowApi;
        }
    }
}
