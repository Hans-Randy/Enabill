using Enabill.Models;

namespace Enabill.Publishers
{
	public interface IEmailer : ISink
	{
		//void LogError(string context, string message);
		//void LogError(string context, Exception ex);
		//void LogWarning(string context, string message);
		//void LogVerbose(string context, string message, bool isErr, ProcessStepType type);
		//void LogVerbose(string context, string message, DateTime start, bool isErr, ProcessStepType type);
		//void LogDebug(string context, string message);
		//void LogCriticalError(string context, string message, bool isFeedback);

		void NotifyManagerOfLeaveBookedByUser(User manager, User staffMember, Leave leave);

		void NotifyManagerOfFlexiDayBookedByUser(User manager, User staffMember, FlexiDay flexiDay);
	}
}