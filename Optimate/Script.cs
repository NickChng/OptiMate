using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VMS.TPS.Common.Model.Types;
using VMS.TPS.Common.Model.API;
using OptiMate;
using OptiMate.ViewModels;
using OptiMate.Models;

namespace VMS.TPS
{
    public class Script // This is the script that will be called by Eclipse.  Note that this project is referenced by CNPluginTester
    {
        public Script()
        {

        }

        private void RunOnNewStaThread(Action a)
        {
            Thread thread = new Thread(() => a());
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void InitializeAndStartMainWindow(EsapiWorker esapiWorker)
        {
            var model = new MainModel(esapiWorker);
            var viewModel = new ViewModel(model);
            var mainWindow = new ScriptWindow(viewModel);
            mainWindow.ShowDialog();
        }

        public void Execute(ScriptContext context)
        {
            // The ESAPI worker needs to be created in the main thread
            EsapiWorker ew;
            if (context.PlanSetup == null)
            {
                ew = new EsapiWorker(context.Patient, context.StructureSet, context.CurrentUser.Id);
            }
            else
                ew = new EsapiWorker(context.Patient, context.PlanSetup, context.StructureSet, context.CurrentUser.Id);

            // This new queue of tasks will prevent the script
            // for exiting until the new window is closed
            DispatcherFrame frame = new DispatcherFrame();

            RunOnNewStaThread(() =>
            {
                // This method won't return until the window is closed

                InitializeAndStartMainWindow(ew);

                // End the queue so that the script can exit
                frame.Continue = false;
            });

            // Start the new queue, waiting until the window is closed
            Dispatcher.PushFrame(frame);
        }

    }
}
