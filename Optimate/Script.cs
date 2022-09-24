using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VMS.TPS.Common.Model.Types;
using VMS.TPS.Common.Model.API;

namespace Optimate
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
            var viewModel = new ViewModel(esapiWorker);
            var mainWindow = new ScriptWindow(viewModel);
            mainWindow.ShowDialog();
        }

        public void Execute(ScriptContext context)
        {
            // Rather than take the standard ESAPI input window, this code instantiates a WPF window subclass (ScriptWindow) which can be edited using the VS Designer 

            // If you don't want to use a GUI you can just pass the relevant ScriptContext fields to any method you want and call this method in the PluginTesterForm.cs instead of creating the ScriptWindow

            // I recommend you design your script so that you don't do any work in the Execute method, or any method which uses the "ScriptContext" class directly, as this class isn't available in a standalone application.  
            // Instead, have Execute call a method that takes elements you can get from either the standalone or scriptcontext (like Patient and PlanSetup) so you can call (and debug) it from the standalone PluginTester without having to make any changes
            // to the Script class itself.
            Helpers.Logger.user = context.CurrentUser.Id;
            // The ESAPI worker needs to be created in the main thread
            var esapiWorker = new EsapiWorker(context.Patient, context.StructureSet);

            // This new queue of tasks will prevent the script
            // for exiting until the new window is closed
            DispatcherFrame frame = new DispatcherFrame();

            RunOnNewStaThread(() =>
            {
                // This method won't return until the window is closed

                InitializeAndStartMainWindow(esapiWorker);

                // End the queue so that the script can exit
                frame.Continue = false;
            });

            // Start the new queue, waiting until the window is closed
            Dispatcher.PushFrame(frame);

            // Note that Eclipse will pass you a handle to a window if you want to configure it yourself at runtime by adding and formating the layout of the controls.
            // I find this to be very tedious and prefer to design the GUI using the Visual Studio designer.  However, this limits you (well, without adding significant complexity)
            // to running your script as a DLL (i.e. you have to compile the TestScript project to get TestScript.esapi.dll and run this in Eclipse.
            // If you don't want to do this, see https://github.com/VarianAPIs/Varian-Code-Samples/blob/master/Eclipse%20Scripting%20API/plugins/DvhLookups.cs for some examples of configuring the window layout

            // Show the script GUI

        }

    }
}
