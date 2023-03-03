using System;

namespace Enabill.ProcessExec
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			object db = null;

			EnabillSettings.Setup(
				cntxt => db = cntxt,
				() => db,
				() => new DB.EnabillContext()
				);

			Console.WriteLine("Starting Process...");
			Processes.Processes.OnProgress += Processes_OnProgress;

			Processes.Processes.DailyProcess(EnabillSettings.GetSystemUser());

			if (DateTime.Now.IsFirstDayOfMonth())
				Processes.Processes.RunMonthEndLeaveFlexiBalanceProcess(EnabillSettings.GetSystemUser(), 2, null);
		}

		private static void Processes_OnProgress(object sender, Processes.ProcessEventArgs e) => Console.WriteLine("Progress : " + e.Progress.ToString());
	}
}