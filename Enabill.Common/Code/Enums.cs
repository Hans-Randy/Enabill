using System;

namespace Enabill
{
	[Flags]
	public enum UserRoleType
	{
		[Description("System Administrator")]
		SystemAdministrator = 1,

		[Description("Project Owner")]
		ProjectOwner = 2,

		[Description("Manager")]
		Manager = 4,

		[Description("Time Capturer")]
		TimeCapturing = 8,

		[Description("Invoice Administrator")]
		InvoiceAdministrator = 16,

		[Description("Accountant")]
		Accountant = 32,

		[Description("Story Writer")]
		StoryWriter = 64,

		[Description("Feedback Admin")]
		FeedbackAdmin = 128,

		[Description("HR")]
		HR = 256,

		[Description("Forecast Administrator")]
		ForecastAdministrator = 512,

		[Description("Ticket Manager")]
		TicketManager = 1024,

		[Description("Ticket Project Manager")]
		TicketProjectOwner = 2048,

		[Description("Encentivize")]
		Encentivize = 4096,

		[Description("Payroll Manager")]
		PayRoll = 8192,

		[Description("Contract Manager")]
		ContractManager = 16384
	}

	[Flags]
	public enum NoteFilterType
	{
		[Description("All")]
		All = 0,

		[Description("Project")]
		Project = 1,

		[Description("Activity")]
		Activity = 2
	}

	[Flags]
	public enum BillingMethodType
	{
		[Description("Time and Material")]
		TimeMaterial = 1,

		[Description("Fixed Cost")]
		FixedCost = 2,

		[Description("SLA")]
		SLA = 4,

		[Description("Travel")]
		Travel = 8,

		[Description("Ad Hoc")]
		AdHoc = 16,

		[Description("Non-Billable")]
		NonBillable = 32,

		[Description("Monthly Fixed Cost")]
		MonthlyFixedCost = 64,

		[Description("Activity Fixed Cost")]
		ActivityFixedCost = 128
	}

	[Flags]
	public enum InvoiceStatusType
	{
		[Description("Open")]
		Open = 1,

		[Description("In Progress")]
		InProgress = 2,

		[Description("Ready")]
		Ready = 4,

		[Description("Complete")]
		Complete = 8
	}

	[Flags]
	public enum EmploymentTypeEnum
	{
		[Description("Permanent")]
		Permanent = 1,

		[Description("Monthly Contractor")]
		MonthlyContractor = 2,

		[Description("Hourly Contractor")]
		HourlyContractor = 4,

		[Description("Intern")]
		Intern = 8
	}

	[Flags]
	public enum BillableIndicatorType
	{
		[Description("Yes")]
		Yes = 1,

		[Description("No")]
		No = 2,

		[Description("Partial")]
		Partial = 4
	}

	[Flags]
	public enum LeaveTypeEnum
	{
		[Description("Annual Leave")]
		Annual = 1,

		[Description("Sick Leave")]
		Sick = 2,

		[Description("Family Responsibility")]
		Compassionate = 4,

		[Description("Study Leave")]
		Study = 8,

		[Description("Maternity Leave")]
		Maternity = 16,

		[Description("Relocation Leave")]
		Relocation = 32,

		[Description("Unpaid Leave")]
		Unpaid = 64,

		[Description("No Work Day")]
		NoWorkDay = 128,

		[Description("Birthday Leave")]
		BirthDay = 256
	}

	[Flags]
	public enum ApprovalStatusType
	{
		[Description("Pending")]
		Pending = 1,

		[Description("Declined")]
		Declined = 2,

		[Description("Approved")]
		Approved = 4,

		[Description("Withdrawn")]
		Withdrawn = 8
	}

	[Flags]
	public enum WorkAllocationType
	{
		[Description("User Created")]
		UserCreated = 1,

		[Description("System Created")]
		SystemCreated = 2,

		[Description("SLA Split")]
		SLASplit = 4
	}

	[Flags]
	public enum PrintOptionType
	{
		[Description("Print by Person")]
		PrintByPerson = 1,

		[Description("Print by Activity")]
		PrintByActivity = 2,

		[Description("Description Only")]
		PrintDescriptionOnly = 5
	}

	[Flags]
	public enum PrintLayoutType
	{
		[Description("As Time Table")]
		AsTimeTable = 3,

		[Description("As Details")]
		AsDetails = 4
	}

	[Flags]
	public enum PrintTicketRemarkType
	{
		[Description("Remark Only")]
		RemarkOnly = 1,

		[Description("Ticket Reference Only")]
		TicketReferenceOnly = 2,

		[Description("Ticket And Remark")]
		TicketAndRemark = 3
	}

	[Flags]
	public enum WorkSessionStatusType
	{
		[Description("UnApproved")]
		UnApproved = 1,

		[Description("Approved")]
		Approved = 2,

		[Description("Exception")]
		Exception = 3,

		[Description("BeforeStartDate")]
		BeforeStartDate = 4
	}

	[Flags]
	public enum TrainingCategoryType
	{
		[Description("Please select")]
		PleaseSelect = 0,

		[Description("Self-study")]
		SelfStudy = 1,

		[Description("On-the-job")]
		OnTheJob = 2,

		[Description("Knowledge-sharing session")]
		KnowledgeSharingSession = 3,

		[Description("Internal training course")]
		InternalTrainingCourse = 4,

		[Description("External training course")]
		ExternalTrainingCourse = 5,

		[Description("Induction")]
		Induction = 6,

		[Description("Other")]
		Other = 7,

		[Description("Mentoring/Coaching")]
		MentoringCoaching = 8
	}

	[Flags]
	public enum FrequencyType
	{
		[Description("Daily")]
		Daily = 0,

		[Description("Weekly")]
		Weekly = 1,

		[Description("Monthly")]
		Monthly = 2
	}

	[Flags]
	public enum EmailType
	{
		[Description("Leave Requests Pending")]
		LeaveRequestsPending = 1
	}

	[Flags]
	public enum InvoiceCategoryType
	{
		[Description("Annuity")]
		Annuity = 1,

		[Description("Semi-Annuity")]
		SemiAnnuity = 2,

		[Description("Ad-Hoc")]
		AdHoc = 4
	}

	[Flags]
	public enum TicketLineSourceType
	{
		[Description("Enabill MailWatcher")]
		EnabillMailWatcher = 1,

		[Description("Enabill Post")]
		EnabillPost = 2
	}

	public enum TicketLogType
	{
		[Description("Ticket")]
		Ticket = 1,

		[Description("Status Change")]
		StatusChange = 2,

		[Description("Assignment Change")]
		AssignmentChange = 3
	}

	public enum TicketStatusEnum
	{
		Open = 1,
		WorkInProgress = 2,
		Testing = 3,
		Resolved = 4,
		Closed = 5,
		Parked = 6
	}

	public enum TicketTypeEnum
	{
		[Description("Undefined")]
		Undefined = 1,

		[Description("Bug")]
		Bug = 2,

		[Description("New Feature")]
		NewFeature = 3,

		[Description("Quote Request")]
		QuoteRequest = 4,

		[Description("Support")]
		Support = 5,

		[Description("New System")]
		NewSystem = 6
	}

	public enum TicketPriorityEnum
	{
		[Description("Low")]
		Low = 1,

		[Description("Medium")]
		Medium = 2,

		[Description("High")]
		High = 3,

		[Description("Undefined")]
		Undefined = 4
	}

	public enum TimesheetSchedule
	{
		[Description("Last Day Of The Month Minus 7 Days")]
		LastDayOfTheMonthMinusSevenDays = 1,

		[Description("Last Working Day Of The Month")]
		LastWorkingDayOfTheMonth = 2,

		[Description("First Day Of The Next Month")]
		FirstDayOfNextTheMonth = 3
	}

	[Flags]
	public enum FeedbackClientFilterEnum
	{
		From = 1,
		To = 2,
		Subject = 4
	}

	[Flags]
	public enum BalanceTypeEnum
	{
		[Description("Flexi")]
		Flexi = 1,

		[Description("Leave")]
		Leave = 2
	}

	[Flags]
	public enum BalanceChangeTypeEnum
	{
		[Description("WorkSession")]
		WorkSession = 1,

		[Description("Leave")]
		Leave = 2,

		[Description("Flexi")]
		Flexi = 4
	}

	[Flags]
	public enum ExpenseCategoryTypeEnum
	{
		[Description("Meals")]
		Meals = 1,

		[Description("Transportation")]
		Transportation = 2,

		[Description("Data")]
		Data = 3,

		[Description("Entertainment")]
		Entertainment = 4,

		[Description("Lodging")]
		Lodging = 5,

		[Description("Mileage")]
		Mileage = 6,

		[Description("Other")]
		Other = 7
	}

	[Flags]
	public enum StatusEnum
	{
		[Description("Enabled")]
		Enabled = 1,

		[Description("Deleted")]
		Deleted = 2
	}

	[Flags]
	public enum StatusTypeEnum
	{
		[Description("Inactive")]
		Inactive = 0,
		[Description("Active")]
		Active = 1,
		[Description("All")]
		All = 2
	}

}