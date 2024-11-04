using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerForService;

namespace WindowsService
{
    public partial class ServerService : ServiceBase
    {
        Server server = new Server();

        public ServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread startThread = new Thread(() => server.StartServer());
            startThread.Start();
        }

        protected override void OnStop()
        {
            server.StopServer();
            Environment.Exit(0);
        }
    }
}
