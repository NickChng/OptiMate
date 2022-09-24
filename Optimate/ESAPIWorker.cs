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
        private bool _CanRunSSContext = false;
        
        public EsapiWorker(PlanSetup pl, Patient p, Application app)
        {
            if (app != null)
                _app = app;
            _pl = pl;
            _ss = pl.StructureSet;
            _p = p;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public EsapiWorker(Patient p, StructureSet ss)
        {
            _CanRunSSContext = true;
            _p = p;
            _ss = ss;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
        public EsapiWorker(StructureSet ss)
        {
            _CanRunSSContext = true;
            _ss = ss;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
        public EsapiWorker(PlanSetup pl, Patient p)
        {
            _pl = pl;
            _ss = pl.StructureSet;
            _p = p;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
        public EsapiWorker(StructureSet ss, Patient p, Application app)
        {
            if (app != null)
                _app = app;
            _ss = ss;
            _p = p;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
        public void Run(Action<PlanSetup> a)
        {
            _dispatcher.BeginInvoke(a, _pl);
        }
        public void Run(Action<Patient, PlanSetup> a)
        {
            _dispatcher.BeginInvoke(a, _p, _pl);
        }

        public async Task<bool> AsyncRun(Action<Patient, PlanSetup> a)
        {
            await _dispatcher.BeginInvoke(a, _p, _pl);
            return true;
        }

        public delegate void D(Patient p, StructureSet s);
        public async Task<bool> AsyncRunStructureContext(Action<Patient, StructureSet, Dispatcher> a)
        {
            await _dispatcher.BeginInvoke(a, _p, _ss, _dispatcher);
            //var D = new SendOrPostCallback(_ => a(_p, _ss));
            //_context.Send(D, null);
            return true;
        }

        public async Task<bool> AsyncAppRun(Action<Application> a)
        {
            await _dispatcher.BeginInvoke(a, _app);
            return true;
        }

   }
}
