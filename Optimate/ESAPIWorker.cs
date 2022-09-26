using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Optimate
{
    public class EsapiWorker 
    {
        private readonly PlanSetup _pl;
        private readonly StructureSet _ss;
        private readonly Patient _p;
        private readonly Application _app;
        private readonly Dispatcher _dispatcher;
        
        public EsapiWorker(Patient p, StructureSet ss)
        {
            _p = p;
            _ss = ss;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
    
        public delegate void D(Patient p, StructureSet s);
        public async Task<bool> AsyncRunStructureContext(Action<Patient, StructureSet, Dispatcher> a)
        {
            await _dispatcher.BeginInvoke(a, _p, _ss, _dispatcher);
            //var D = new SendOrPostCallback(_ => a(_p, _ss));
            //_context.Send(D, null);
            return true;
        }
   }
}
