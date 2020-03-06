using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Collections;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.Configuration.Install;


namespace IzendaService
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
        static void Main(string[] args)
        {
            if (args != null)
            {
                if (args.Length == 1)
                {
                    if (args[0] == "/install")
                    {
                        InstallService();
                        return;
                    }

                    if (args[0] == "/uninstall")
                    {
                        RemoveService();
                        return;
                    }

                    if (args[0] == "/?")
                    {
                        Trace.WriteLine("IzendaService supported parameters: /install or /uninstall");
                        return;
                    }
                }
            }

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { new IzendaService() };
            ServiceBase.Run(ServicesToRun);
        }
        private static void InstallService()
        {
            //Install
            using (ProjectInstaller pi = new ProjectInstaller())
            {
                IDictionary savedState = new Hashtable();
                try
                {
                    pi.Context = new InstallContext();
                    pi.Context.Parameters.Add("assemblypath", Process.GetCurrentProcess().MainModule.FileName);
                    foreach (Installer i in pi.Installers)
                        i.Context = pi.Context;
                    pi.Install(savedState);
                    pi.Commit(savedState);
                    Trace.WriteLine("IzendaService successfully installed.");
                }
                catch (Exception ex)
                {
                    pi.Rollback(savedState);
                    Trace.WriteLine("IzendaService installing failed. " + ex.Message);
                }
            }
        }

        private static void RemoveService()
        {
            //UnInstall
            using (ProjectInstaller pi = new ProjectInstaller())
            {
                try
                {
                    pi.Context = new InstallContext();
                    pi.Context.Parameters.Add("assemblypath", Process.GetCurrentProcess().MainModule.FileName);
                    foreach (Installer i in pi.Installers)
                        i.Context = pi.Context;

                    pi.Uninstall(null);
                    Trace.WriteLine("IzendaService successfully uninstalled.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("IzendaService uninstalling failed. " + ex.Message);
                }
            }
        }

    }
}
