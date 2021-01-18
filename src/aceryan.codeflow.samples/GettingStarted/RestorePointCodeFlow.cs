using aceryansoft.codeflow.core;
using aceryansoft.codeflow.model.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace aceryan.codeflow.samples.GettingStarted
{
    public class RestorePointCodeFlow
    {
        public void Run(string requestId = "", bool rerunMode = false, string restorePoint = "")
        { 
            var restorecodeFlow = new CodeFlow();
            restorecodeFlow.StartNew(cfg =>
            {
                cfg.WithContext(() =>
                {
                    var context = new CodeFlowContext();
                    context.RequestId = string.IsNullOrEmpty(requestId) ? context.RequestId : requestId; // requestId is the last failed process id 
                    return context;
                })
                .RerunMode(rerunMode) // set to true to rerun the process 
                .RestorePointId(restorePoint); // set to the point where the codeflow should restart, for instance "restore.after.6hours.and.aliens.are.wake.up"
            }) 
            .Call((ctx, inputs) =>
            {
                //legacy code to run a dark code logic 
                // of course the previous developers didn't care about software craftsmanship. 
                // of course managers think that the code is working so don't change anything. 
                Thread.Sleep(TimeSpan.FromHours(4));
            })
            .RestorePoint("restore.after.4hours.of.dark.code", SaveRestorePointContextState, LoadRestorePointContextState)
            .Call((ctx, inputs) =>
            {
                if(DateTime.Now.Hour <= 6) // seriously after 4 hours of shit, i need to restart everything from the beginning ... 
                {
                    throw new Exception("Aliens are not wake up yet, try again after 6 AM"); 
                }
                Thread.Sleep(TimeSpan.FromHours(2));
            })
            .RestorePoint("restore.after.6hours.and.aliens.are.wake.up", SaveRestorePointContextState, LoadRestorePointContextState)
            .Call((ctx, inputs) =>
            {
                var random = new Random().Next(1, 10);
                if (random<=6) // we don't give a shit about your feelings, restart the program from the beginning ... 
                {
                    // seriously after 6 hours , what the F
                    throw new Exception("Another team deliver breaking changes and they don't care about your processes");
                }
                Thread.Sleep(TimeSpan.FromHours(2));
            })
            .RestorePoint("restore.after.8hours.and.another.team.fix.breaking.release.changes", SaveRestorePointContextState, LoadRestorePointContextState)
            .Call((ctx, inputs) =>
            {           
                Thread.Sleep(TimeSpan.FromSeconds(10)); // you can now save the results to database and publish the results 
                //the process took 14 hours, you relaunched it 3 times 
                //finally the business don't really care because it is 8 PM. 
                // all you have left is a prayer for tomorrow. 
            })
            .Close();

            restorecodeFlow.Execute();
        }

        private void SaveRestorePointContextState(string reqId, string pointId, object[] inputs, ICodeFlowContext ctx)
        {
            //save execution context to any persistent store
        }

        private ICodeFlowContext LoadRestorePointContextState(string reqId, string pointId)
        {
            //load execution context from any persistent store
            throw new NotImplementedException(); // left as an exercice to the reader. 
        }
    }     
}
