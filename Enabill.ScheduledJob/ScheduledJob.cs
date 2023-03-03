using System;
using System.Configuration;
using Enabill.DB;
using Enabill.Repos;

namespace Enabill.ScheduledJob
{
	internal class ScheduledJob
	{
		private static EnabillContext _context;

		private static void Main(string[] args)
		{
			try
			{
				EnabillSettings.Setup(
					cntxt => _context = cntxt as EnabillContext,
					() => _context,
					() => new EnabillContext()
				);

				string parameter = args[0].ToUpper().Replace("-", "");

				switch (parameter)
				{
					case "MONTHLY":
						Console.WriteLine("Enabill Email " + parameter.ToLower() + " reports process started.....");
						Processes.Processes.RunAndEmailReports(parameter, "%");
						Console.WriteLine("Enabill Email " + parameter.ToLower() + " reports process ended.....");
						break;

					case "WEEKLY":
						Console.WriteLine("Enabill Email " + parameter.ToLower() + " reports process started.....");
						Processes.Processes.RunAndEmailReports(parameter, "");
						Console.WriteLine("Enabill Email " + parameter.ToLower() + " reports process ended.....");
						break;

					case "RENEWLEAVECYCLE":
						Console.WriteLine("Enabill Renew Leave Cycles process started.....");
						Processes.Processes.RunRenewLeaveCycleProcess();
						Console.WriteLine("Enabill Renew Leave Cycles process ended.....");
						break;

					case "UPDATELEAVEFLEXI":
						Console.WriteLine("Enabill Update Leave and Flexi Balance process started.....");
						Processes.Processes.RunMonthEndLeaveFlexiBalanceProcess(null, 3, "");
						Console.WriteLine("Enabill Update Leave and Flexi Balance process ended.....");
						//Process below commented out until needed
						//Start the UCTC copy previous month to current month directly after leave update complete
						//Console.WriteLine("Enabill MonthEnd UserCost to Company Copy process started.....");
						//Enabill.Processes.Processes.RunMonthEndUserCostToCompanyProcess(null, 1, "RetrieveFromDB");
						//Console.WriteLine("Enabill MonthEnd UserCost to Company Copy process ended.....");
						break;

					case "UPDATELEAVEFLEXIUSERSPECIFIC":
						var user = UserRepo.GetByID(309);
						double ts = (DateTime.Now - user.EmployStartDate).TotalDays / 30;
						int iMonths = (int)ts;
						Console.WriteLine("Enabill Update Leave and Flexi Balance process started.....");
						Processes.Processes.RunMonthEndLeaveFlexiBalanceProcess(user, iMonths, "94");
						Console.WriteLine("Enabill Update Leave and Flexi Balance process ended.....");
						break;

					case "ENCENTIVIZE":
						Console.WriteLine("Generating Encentivize Report Started");
						//Enabill.Processes.Processes.RunReportAndSaveToDisk(7, "Encentivize Report", 2, "", DateTime.Today.ToFirstDayOfMonth(), DateTime.Today.ToFirstDayOfMonth().AddDays(16));
						Processes.Processes.RunReportAndSaveToDisk(7, "Encentivize Report", 1, "");
						Console.WriteLine("Generating Encentivize Report Ended");
						break;
					//case "ENCENTIVIZEENDMONTH":
					//    Console.WriteLine("Generating Encentivize Report Started");
					//    Enabill.Processes.Processes.RunReportAndSaveToDisk(7, "Encentivize Report", 1, "", DateTime.Today.AddMonths(-1).ToFirstDayOfMonth(), DateTime.Today.ToLastDayOfMonth());
					//    Console.WriteLine("Generating Encentivize Report Ended");
					//    break;
					//case "ENCENTIVIZEENDMONTHPREVIOUS":
					//    Console.WriteLine("Generating Encentivize Report Started");
					//    Enabill.Processes.Processes.RunReportAndSaveToDisk(7, "Encentivize Report", 1, "", DateTime.Today.AddMonths(-1).ToFirstDayOfMonth(), DateTime.Today.AddMonths(-1).ToLastDayOfMonth());
					//    Console.WriteLine("Generating Encentivize Report Ended");
					//    break;
					case "LEAVEREQUESTSPENDING":
						Console.WriteLine("Leave Requests Pending Email Process Started");
						int daysToSubtract = int.Parse(ConfigurationManager.AppSettings["DaysToSubtract"]);
						Processes.Processes.RunEmail(DateTime.Today.AddDays(-daysToSubtract), null, EmailType.LeaveRequestsPending);
						Console.WriteLine("Leave Requests Pending Email Process Ended");
						break;

					case "ENCENTIVIZEFULL":
						break;

					case "RECALCULATESICKLEAVEBALANCES":
						const LeaveTypeEnum leaveType = LeaveTypeEnum.Sick;
						Processes.Processes.RecalculateLeaveBalances(leaveType);
						break;

					case "DEACTIVATEEMPLOYEES":
						Console.WriteLine("Enabill Deactivate Employees process started.....");
						Processes.Processes.DeactivateEmployees();
						Console.WriteLine("Enabill Deactivate Employees process ended.....");
						break;

					case "TIMEREMINDER":
						Console.WriteLine("Enabill Update Time process started.....");
						Processes.Processes.TimeReminder();
						Console.WriteLine("Enabill Update Time process ended.....");
						break;
				}
			}
			catch (Exception e)
			{
				//logger.Debug($"A scheduled job failed called. Call stack: {Environment.StackTrace}");
				//logger.Fatal(e.Dump());
			}
		}
	}
}