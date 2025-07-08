//using Khronus.Framework.Core.Util;
//using Khronus.Framework.DataAccess;
//using Khronus.Framework.DataAccess.Components;
//using Khronus.Framework.DataAccess.ORM;
//using Sync.Custom;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Reflection;
//using System.Threading;
//using System.Windows.Forms;

//namespace Sync
//{
//    static class Program
//    {
//        private static System.Threading.Timer timer1;
//        private static Worker worker;
//        private static string appName;

//        /// <summary>
//        /// The main entry point for the application.
//        /// </summary>
//        static void Main()
//        {
//            appName = Assembly.GetExecutingAssembly().GetName().Name;

//            System.Console.WriteLine(appName);

//            Process currentProcess = Process.GetCurrentProcess();

//            int currentProcessID = currentProcess.Id;

//            Process[] procs = Process.GetProcessesByName(appName);

//            foreach (Process p in procs)
//            {
//                if (p.Id != currentProcessID)
//                {
//                    return;
//                }
//            }

//            timer1 = new System.Threading.Timer(new TimerCallback(timer1_Tick), null, 0, 1000 * 60);

//            System.Console.ReadKey();
//        }

//        private static void timer1_Tick(object sender)
//        {
//            if (worker == null)
//            {
//                String server = "mysql.vsisolucoes.com.br";
//                String port = "3306";
//                String database = "vsisolucoe153";
//                String user = "vsisolucoe153";
//                String password = "V123456";
//                String options = "charset=utf8";

//                String connection = "server=" + server + ";port=" + port + ";database=" + database + ";Uid=" + user + ";Pwd=" + password + ";" + options;

//                DataAccessManager dataAccessManager = new DataAccessManager();
//                dataAccessManager.Provider = ConnectionProviders.MYSQL;
//                dataAccessManager.ConnectionString = connection;

//                BusinessObjectManager businessObjectManager = new BusinessObjectManager();
//                businessObjectManager.DataAccessManager = dataAccessManager;

//                Logger logger = new Logger();
//                logger.AppName = Assembly.GetExecutingAssembly().GetName().Name;
//                logger.FileName = Path.Combine(Application.StartupPath, "vsintegra.log");
//                logger.Limit = 1024 * 1024 * 4;

//                worker = new Worker();
//                worker.IsRunning = true;
//                worker.DataAccessManager = dataAccessManager;
//                worker.BusinessObjectManager = businessObjectManager;
//                //worker.LogFileName = Path.Combine(Application.StartupPath, "vsintegra.log");
//                //worker.AppName = appName;
//                worker.Logger = logger;
//                worker.Store = 0;

//                //
//                // Injetar os parametros
//                //
//                Dictionary<string, dynamic> parameters = new Dictionary<string, dynamic>();

//                IEnumerable<ParametroBO> parametroBOList = businessObjectManager.GetListByFilter<ParametroBO>($"SYSAH_FILIAL_COD = {worker.Store}");
//                foreach (ParametroBO parametroBO in parametroBOList)
//                {
//                    parameters.Add(parametroBO.SYSAH_NOME, parametroBO.SYSAH_VALOR);
//                }

//                worker.Parameters = parameters;
//            }

//            worker.Run();
//        }
//    }
//}

using Khronus.Framework.Core.Util;
using Khronus.Framework.DataAccess.Components;
using Khronus.Framework.DataAccess.ORM;
using Sync.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Sync
{
    static class Program
    {
        private static string appName;
        private static Timer timer1;
        private static Worker worker;
        private static BusinessObjectManager businessObjectManager;
        private static WorkerService workerService;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            appName = Assembly.GetExecutingAssembly().GetName().Name;

            //
            // Verifico se ja não existe uma versão rodando
            //
            bool createdNew;

            Mutex mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Environment.Exit(0);
            }

            System.Console.WriteLine(appName);

            String server = "mysql.vsisolucoes.com.br";
            String port = "3306";
            String database = "vsisolucoe152";
            String user = "vsisolucoe152";
            String password = "V123456";
            String options = "charset=utf8";

            String connection = "server=" + server + ";port=" + port + ";database=" + database + ";Uid=" + user + ";Pwd=" + password + ";" + options;

            DataAccessManager dataAccessManager = new DataAccessManager();
            dataAccessManager.Provider = ConnectionProviders.MYSQL;
            dataAccessManager.ConnectionString = connection;

            businessObjectManager = new BusinessObjectManager();
            businessObjectManager.DataAccessManager = dataAccessManager;

            Logger logger = new Logger();
            logger.AppName = appName;
            logger.FileName = Path.Combine(Thread.GetDomain().BaseDirectory, "vsintegra.log");
            logger.Limit = 1024 * 1024 * 4;

            worker = new Worker();
            worker.Name = appName;
            worker.IsRunning = true;
            worker.DataAccessManager = dataAccessManager;
            worker.BusinessObjectManager = businessObjectManager;
            worker.Logger = logger;
            worker.Store = 0;

            workerService = new WorkerService(worker);

            timer1 = new System.Threading.Timer(new TimerCallback(timer1_Tick), null, 0, 1000 * 60);

            System.Console.ReadKey();
        }

        private static void timer1_Tick(object sender)
        {
            //
            // Injetar os parametros
            //
            Dictionary<string, dynamic> parameters = new Dictionary<string, dynamic>();

            IEnumerable<ParametroBO> parametroBOList = businessObjectManager.GetListByFilter<ParametroBO>($"SYSAH_FILIAL_COD={worker.Store}");
            foreach (ParametroBO parametroBO in parametroBOList)
            {
                parameters.Add(parametroBO.SYSAH_NOME, parametroBO.SYSAH_VALOR);
            }

            worker.Parameters = parameters;

            //worker.Run();
            workerService.Execute();
        }
    }
}