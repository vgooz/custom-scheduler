using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Diagnostics;
using System.Configuration;

namespace IzendaService
{
	public partial class IzendaService : ServiceBase
	{
		string rsPath = "";
		string ssl = "";
		string timePeriod = "1";
		string tenants = "";
		string user = "";
		string pass = "";
		string izUser = "";
		string izPassword = "";

		public IzendaService()
		{
			InitializeComponent();
			timer = new Timer();
			timer.Elapsed += RunScheduledReports;
		}

		private Timer timer;
		private bool reportsInProcess = false;
		private void RunScheduledReports(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (reportsInProcess) return;

			reportsInProcess = true;

			try
			{
				string schedulingLogs = "";
				using (CustomWebClient client = new CustomWebClient())
				{
					if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
					{
						NetworkCredential credentials = new NetworkCredential(user, pass);
						client.UseDefaultCredentials = false;
						client.Credentials = credentials;
					}
					else
						client.UseDefaultCredentials = true;
					string url = string.Format("{0}?run_scheduled_reports={1}{2}{3}{4}", 
						rsPath, 
						timePeriod,
						string.IsNullOrEmpty(ssl) ? "" : ("&ssl=" + ssl),
						string.IsNullOrEmpty(tenants) ? "" : ("&tenants=" + tenants), 
						string.IsNullOrEmpty(izUser) ? "" : "&izUser=" + izUser, 
						string.IsNullOrEmpty(izPassword) ? "" : "&izPassword=" + izPassword);
					Stream networkStream = client.OpenRead(url);
					using (StreamReader reader = new StreamReader(networkStream))
						schedulingLogs = reader.ReadToEnd().Replace("<br>", Environment.NewLine).Replace("<br/>", Environment.NewLine);
					networkStream.Close();
				}
				EventLog.WriteEntry(ServiceName,"Scheduling operation succeeded. Log which can be parsed: " + schedulingLogs, EventLogEntryType.Information);
			}
			catch (Exception e)
			{
				EventLog.WriteEntry(ServiceName, "Scheduling operation failed: " + e.Message, EventLogEntryType.Error);
			}

			reportsInProcess = false;
		}

		protected override void OnStart(string[] args)
		{
			rsPath = (ConfigurationManager.AppSettings["responseServerPath"] ?? "").ToString();
			ssl = (ConfigurationManager.AppSettings["ssl"] ?? "").ToString();
			user = (ConfigurationManager.AppSettings["user"] ?? "").ToString();
			pass = (ConfigurationManager.AppSettings["password"] ?? "").ToString();
			tenants = (ConfigurationManager.AppSettings["tenants"] ?? "").ToString();
			timePeriod = (ConfigurationManager.AppSettings["timePeriod"] ?? "1").ToString();
			izUser = (ConfigurationManager.AppSettings["izu"] ?? "").ToString();
			izPassword = (ConfigurationManager.AppSettings["izp"] ?? "").ToString();
            int interval = Convert.ToInt32((ConfigurationManager.AppSettings["interval"] ?? "-1").ToString());
			if (string.IsNullOrEmpty(rsPath))
			{
				EventLog.WriteEntry(ServiceName, "Response server URL is not specified. Attribute name is 'responseServerPath'", EventLogEntryType.Warning);
				return;
			}
			if (interval <= 0)
			{
				EventLog.WriteEntry(ServiceName, "Time interval between scheduler runs is not specified. Attribute name is 'interval'", EventLogEntryType.Warning);
				return;
			}
			timer.Interval = interval;
			timer.Start();
		}

		protected override void OnStop()
		{
			timer.Stop();
			timer.Close();
		}
	}
}
