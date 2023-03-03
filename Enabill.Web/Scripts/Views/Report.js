var SelectedMonth = '';
var CurrentDate = '';

$(function () {
    Report.initialize();
});

var Report = {
    setViewData: function () {
        toastr.info("Processing...");

        var clientID = $("#ClientList").val();
        var projectName = $("#ProjectList").val();
        $.post("/Report/ShowActivityFilterResults/", $("#frmActivityReport").serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                //$('#ucActivityReportIndex').html(res);
                $("#dialog").html(res);
            }
        });
    },

    initialize: function () {
        SelectedMonth = $('#SelectedMonth').val();
        CurrentDate = $('#CurrentDate').val();
        $('#calMonth').datepicker('option', 'onSelect', function (dateText, inst) { Report.showMonth(dateText); });
        $('#calDay').datepicker('option', 'onSelect', function (dateText, inst) { Report.showDay(dateText); });
    },

    skipToMonth: function (numOfMonthToAdd) {
        var beginDate = new Date(SelectedMonth);
        var month = (parseInt(beginDate.getMonth()) + parseInt(numOfMonthToAdd)) % 12;
        var year = (parseInt(beginDate.getMonth()) + parseInt(numOfMonthToAdd)) / 12;

        beginDate.setMonth(month);

        // code is used to prevent errors when scrolling backward from january of one year to december of the previous year.
        if (numOfMonthToAdd >= 0) {
            beginDate.setFullYear(parseInt(beginDate.getFullYear()) + year);
        }

        beginDate.setDate(1);
        Report.showMonth(beginDate);
    },

    showMonth: function (selectedMonth) {
        toastr.info("Processing...");

        var date = new Date(selectedMonth);

        $.post('/Report/Month/', { date: date.toDateString() }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $("#ucFlexiTimeBalanceReportIndex").html(res);
                Report.initialize();
            }
        });
    },

    linkInvoices: function (forecastHeaderID, period) {
        jqDialog.renderView({
            url: '/Report/ShowLinkInvoicesPartialView/',
            data: { forecastHeaderID: forecastHeaderID, period: period },
            title: 'Link Invoices',
            width: 800,
            buttons: {
                Save: function () {
                    var form = $('#LinkInvoicesFrm').serialize();
                    jqDialog.close();
                    toastr.info("Processing...");

                    var forecastInvoiceIDs = [];

                    $('.forecastInvoiceIDs:checked').each(function () {
                        forecastInvoiceIDs.push($(this).attr('id'));
                    });

                    $.post('/Report/LinkInvoices/?selectedInvoices=' + forecastInvoiceIDs, form, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucForecastReportIndex').html(res);
                            jqDialog.quickStatus(true, 'Invoices successfully linked.');
                        }
                    });
                }
            }
        });
    },

    showActivityReportParameters: function (isAnalysis = false) {
        $("#reportConsole").html("");
        jqDialog.renderView({
            url: '/Report/ShowActivityReportParameters?isAnalysis=' + isAnalysis,
            title: 'Activity' + (isAnalysis ? ' Analysis' : '') + ' Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    window.location = '/Report/PrintActivityReport?dateFrom=' + filterObj.dateFrom
                        + '&dateTo=' + filterObj.dateTo
                        + '&managerId=' + filterObj.managerId
                        + '&userId=' + filterObj.userId
                        + '&divisionId=' + filterObj.divisionId
                        + '&clientId=' + filterObj.clientId
                        + '&projectId=' + filterObj.projectId
                        + '&activityId=' + filterObj.activityId
                        + '&employmentTypeId=' + filterObj.employmentTypeId
                        + '&pageSize=' + filterObj.pageSize
                        + '&pagenumber=' + filterObj.pageNumber
                        + '&includeLeave=' + filterObj.includeLeave
                        + '&isAnalysis=' + isAnalysis;
                }
            },
            open: function (event, ui) {
                $('#DTFrom').datepicker({
                    dateFormat: 'yy-mm-dd',
                    maxDate: new Date($("#DTTo").val()),
                    defaultDate: new Date($("#DTFrom").val()),
                    onSelect: function (dateText) {
                        filterObj.dateFrom = dateText;
                        //getFilterActivityReport();
                    },
                    showOn: 'both',
                    buttonImageOnly: true,
                    buttonImage: '/Content/Img/date_control.png'
                });

                $('#DTTo').datepicker({
                    dateFormat: 'yy-mm-dd',
                    defaultDate: new Date($("#DTTo").val()),
                    onSelect: function (dateText) {
                        filterObj.dateTo = dateText;
                        //getFilterActivityReport();
                    },
                    showOn: 'both',
                    buttonImageOnly: true,
                    buttonImage: '/Content/Img/date_control.png'
                });

                $("#IncludeLeave").click(function () {
                    filterObj.includeLeave = $('#IncludeLeave').prop('checked');
                });
                $("#UserList").val(0);
                $("#DivisionList").val(0);
                $("#ClientList").val(0);
                $("#ActivityList").val(0);
                $("#ProjectList").val(0);
                $("#EmploymentTypeList").val(0);
                filterObj.dateFrom = null;
                filterObj.dateTo = null;
                filterObj.userId = 0;
                filterObj.divisionId = 0;
                filterObj.clientId = 0;
                filterObj.projectId = 0;
                filterObj.activityId = 0;
                filterObj.employmentTypeId = 0;
                filterObj.pageNumber = 1;
                filterObj.pageSize = 300;
                filterObj.isAnalysis = isAnalysis;
            }
        });
    },

    showTrainingReportParameters: function () {
        jqDialog.renderView({
            url: '/Report/ShowTrainingReportParameters/',
            title: 'Training Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    window.location = '/Report/PrintTrainingReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo;
                }
            }
        });
    },

    PercentageAllocationReportParameters: function () {
        jqDialog.renderView({
            url: '/Report/PercentageAllocationReportParameters/',
            title: 'Percentage Allocation Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var year = $('#Year').val();
                    var month = $('#Month').val();
                    var finPeriod = $('#FinPeriod').val();
                    var managerID = $('#Manager').val();
                    window.location = '/Report/PrintPercentageAllocationReport?finperiod=' + finPeriod;
                }
            }
        });
    },

    showTimesheetReportParameters: function () {
        toastr.info("Processing...");
        jqDialog.renderView({
            url: '/Report/ShowTimesheetReportParameters/',
            title: 'Timesheet Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    window.location = '/Report/PrintTimesheetReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo;
                }
            }
        });
        toastr.clear();
    },

    showDSActivityReportParameters: function () {
        jqDialog.renderView({
            url: '/Report/ShowDSActivityReportParameters/',
            title: 'Development Services Activity Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    window.location = '/Report/PrintDSActivityReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo;
                }
            }
        });
    },

    showExpenseReportParameters: function () {
        jqDialog.renderView({
            url: '/Report/ShowExpenseReportParameters/',
            title: 'Expense Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    var activeStatus = $('#ExpenseActiveUserList').val();
                    var employmentTypeID = $('#ExpenseEmploymentTypeList').val();
                    var userID = $('#ExpenseEmployeeList').val();
                    var clientID = $('#ExpenseClientList').val();
                    var projectID = $('#ExpenseProjectList').val();
                    var expenseCategoryTypeID = $('#ExpenseCategoryTypeList').val();
                    var approvalStatus = $('#ExpenseApprovalList').val();
                    var billableStatus = $('#ExpenseBillableList').val();
                    window.location = '/Report/PrintExpenseReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo + '&activeStatus=' + activeStatus + '&employmentTypeID=' + employmentTypeID + '&userID=' + userID + '&clientID=' + clientID + '&projectID=' + projectID + '&expenseCategoryTypeID=' + expenseCategoryTypeID + '&approvalStatus=' + approvalStatus + '&billableStatus=' + billableStatus;
                }
            }
        });
    },

    showTicketTimeAllocationReportParameters: function () {
        jqDialog.renderView({
            url: '/Report/ShowTicketTimeAllocationReportParameters/',
            title: 'Ticket Time Allocation Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    window.location = '/Report/PrintTicketTimeAllocationReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo;
                }
            }
        });
    },

    showProjectActivityReportParameters: function () {
        jqDialog.renderView({
            url: '/Report/ShowProjectManagerActivityReportParameters/',
            title: 'Project Activity Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    var projectManagerID = $('#ProjectManagerList').val();
                    window.location = '/Report/PrintProjectManagerActivityReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo + '&projectManagerID=' + projectManagerID;
                }
            }
        });
    },

    showWAExceptionReportParameters: function () {
        jqDialog.renderView({
            url: '/Report/ShowWAExceptionReportParameters/',
            title: 'WorkAllocation Exception Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    var division = $('#Division').val();
                    window.location = '/Report/PrintWAExceptionReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo + '&division=' + division;
                }
            }
        });
    },

    showEncentivizeReportParameters: function () {
        $("#reportConsole").html("");
        jqDialog.renderView({
            url: '/Report/ShowEncentivizeReportParameters/',
            title: 'Encentivize Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    window.location = '/Report/PrintEncentivizeReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo;
                },
                OnScreen: function () {
                    jqDialog.close();
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    $.post("/Report/EncentivizeReport?dateFrom=" + dateFrom + "&dateTo=" + dateTo, null, function (res) {
                        $("#reportConsole").html(res);
                        jqDialog.close();
                    });
                }
            }
        });
    },

    printEncentivizeReport: function () {
        var dateFrom = $('#DateFrom').val();
        var dateTo = $('#DateTo').val();
        window.location = '/Report/PrintEncentivizeReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo;
    },

    refreshInvoiceList: function () {
        var period = $('#MonthList').val();
        var forecastHeaderID = $('#ForecastHeaderID').val();
        Report.linkInvoices(forecastHeaderID, period);
    },

    showActivityReportOnScreen: function () {
        toastr.info("Processing...");

        $.post('/Report/ShowOnScreenActivityReport', null, function (res) {
            $("#reportConsole").empty();
            $("#reportConsole").html(res);
            $('#DTFrom').datepicker({
                dateFormat: 'yy-mm-dd',
                maxDate: new Date($("#DTTo").val()),
                defaultDate: new Date($("#DTFrom").val()),
                onSelect: function (dateText) {
                    filterObj.dateFrom = dateText;
                }
            });
            $('#DTTo').datepicker({
                dateFormat: 'yy-mm-dd',
                defaultDate: new Date($("#DTTo").val()),
                onSelect: function (dateText) {
                    filterObj.dateTo = dateText;
                }
            });
            $("#UserList").val('');
            $("#DivisionList").val(0);
            $("#ClientList").val(0);
            $("#ActivityList").val('');
            $("#ProjectList").val('');
            $("#EmploymentTypeList").val(0);
            filterObj.dateFrom = null;
            filterObj.dateTo = null;
            filterObj.userName = '';
            filterObj.divisionId = 0;
            filterObj.clientId = 0;
            filterObj.projectName = '';
            filterObj.activityName = '';
            filterObj.employmentTypeId = 0;
            filterObj.pageNumber = 1;
            filterObj.pageSize = 300;
        });

        toastr.clear();
    },

    showLeaveRequestsPendingReportParameters: function () {
        $("#reportConsole").html("");
        jqDialog.renderView({
            url: '/Report/ShowLeaveRequestsPendingReportParameters/',
            title: 'Leave Request Pending Report Parameters',
            width: 400,
            buttons: {
                OnScreen: function () {
                    jqDialog.close();
                    Report.leaveRequestsPendingReport();
                }
            }
        });
    },

    leaveRequestsPendingReport: function () {
        $("#reportConsole").html("");
        var dateFrom = $('#DateFrom').val();
        $.post("/Report/LeaveRequestsPendingReport?dateFrom=" + dateFrom, null, function (res) {
            $("#reportConsole").html(res);
            jqDialog.close();
        });
    },

    updateActivityReportDate: function (inputItem) {
        var dateTo = $(inputItem).val();
        var hiddenDateTo = $("#hiddenDateTo").val();
        var hiddenDateTemp = $("#hiddenDateToTmp").val();

        console.log("1..." + dateTo);
        console.log("2..." + hiddenDateTo);
        console.log("3..." + hiddenDateTemp);

        $("#hiddenDateTo").val(dateTo);
        $("#hiddenDateToTmp").val(hiddenDateTo);
    },

    leaveRequestPendingRunEmail: function (managerID) {
        var dateFrom = $('#DateFrom').val();
        $.post("/Report/LeaveRequestPendingRunEmail?managerID=" + managerID + "&dateFrom=" + dateFrom, null, function (res) {
            alert(res);
        });
    },

    showOnScreenTimeSheetUserList: function () {
        toastr.info("Processing...");

        $.post("/Report/ShowOnScreenTimeSheetApproval", null, function (res) {
            $("#reportConsole").empty();
            $("#reportConsole").html(res);
            $('#DTFrom').datepicker({
                dateFormat: 'yy-mm-dd',
                maxDate: new Date($("#DTTo").val()),
                defaultDate: new Date($("#DTFrom").val()),
                onSelect: function (dateText) {
                    filterObj.dateFrom = dateText;
                },
                showOn: 'both',
                buttonImageOnly: true,
                buttonImage: '/Content/Img/date_control.png'
            });
            $('#DTTo').datepicker({
                dateFormat: 'yy-mm-dd',
                defaultDate: new Date($("#DTTo").val()),
                onSelect: function (dateText) {
                    filterObj.dateTo = dateText;
                },
                showOn: 'both',
                buttonImageOnly: true,
                buttonImage: '/Content/Img/date_control.png'
            });
        });

        toastr.clear();
    },

    showOnScreenTimeSheetApproval: function () {
        toastr.info("Processing...");

        var dateFrom = $("#DTFrom").val();
        var dateTo = $("#DTTo").val();
        var userID = $("#userList").val();
        $.post("/Report/ShowOnScreenTimeSheetApproval?dateFrom=" + dateFrom + "&dateTo=" + dateTo + "&userID=" + userID, null, function (res) {
            $("#reportConsole").empty();
            $("#reportConsole").html(res);
            $('#DTFrom').datepicker({
                dateFormat: 'yy-mm-dd',
                maxDate: new Date($("#DTTo").val()),
                defaultDate: new Date($("#DTFrom").val()),
                onSelect: function (dateText) {
                    filterObj.dateFrom = dateText;
                },
                showOn: 'both',
                buttonImageOnly: true,
                buttonImage: '/Content/Img/date_control.png'
            });
            $('#DTTo').datepicker({
                dateFormat: 'yy-mm-dd',
                defaultDate: new Date($("#DTTo").val()),
                onSelect: function (dateText) {
                    filterObj.dateTo = dateText;
                },
                showOn: 'both',
                buttonImageOnly: true,
                buttonImage: '/Content/Img/date_control.png'
            });
        });

        toastr.clear();
    },

    showRatesReportParameters: function () {
        jqDialog.renderView({
            url: '/Report/ShowRatesReportParameters/',
            title: 'Rates Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var userID = $('#EmployeeRatesList').val();
                    var clientID = $('#ClientRatesList').val();
                    window.location = '/Report/PrintRatesReport?userID=' + userID + '&clientID=' + clientID;
                }
            }
        });
    },

    showLeaveGeneralReportParameters: function () {
        jqDialog.renderView({
            url: '/Report/ShowLeaveGeneralReportParameters/',
            title: 'General Leave Report Parameters',
            width: 400,
            buttons: {
                Print: function () {
                    var dateFrom = $('#DateFrom').val();
                    var dateTo = $('#DateTo').val();
                    var userID = $('#EmployeeGeneralLeaveList').val();
                    var employmentTypeID = $('#EmploymentTypeGeneralLeaveList').val();
                    var leaveTypeID = $('#LeaveTypeGeneralLeaveList').val();
                    var approvalStatusID = $('#ApprovalStatusGeneralLeaveList').val();
                    window.location = '/Report/PrintLeaveGeneralReport?dateFrom=' + dateFrom + '&dateTo=' + dateTo + '&userID=' + userID + '&employmentTypeID=' + employmentTypeID + '&leaveTypeID=' + leaveTypeID + '&approvalStatusID=' + approvalStatusID;
                }
            }
        });
    },

    getLatestLogs: function () {
        $.post('/Report/GetLogEntries', null, function (res) {
            $("#reportConsole").empty();
            $("#reportConsole").html(res);
        });
    },

    validateTimesheets: function () {
        var schedule = $("#timesheetSchedule").val();
        var month = $("#months").val();
        $.post('/Report/ValidateTimesheets?month=' + month + '&schedule=' + schedule, null, function (res) {
            console.log(res);
        });
    },

    testJob: function () {
        var month = $("#months").val();
        $.post('/Report/TestJob', null, function (res) {
            console.log(res);
        });
    }
};

var filterObj = {
    'dateFrom': null,
    'dateTo': null,
    'managerId': null,
    'userId': null,
    'divisionId': null,
    'clientId': null,
    'projectId': null,
    'activityId': null,
    'employmentTypeId': null,
    'pageNumber': 1,
    'pageSize': 300,
    'includeLeave': false,
    'isAnalysis': false
};

getFilterActivityReport = function () {
    toastr.info("Processing...");

    $.post('/Report/FilterActivityReport?dateFrom=' + filterObj.dateFrom
        + '&dateTo=' + filterObj.dateTo
        + '&managerId=' + filterObj.managerId
        + '&userId=' + filterObj.userId
        + '&divisionId=' + filterObj.divisionId
        + '&clientId=' + filterObj.clientId
        + '&projectId=' + filterObj.projectId
        + '&activityId=' + filterObj.activityId
        + '&employmentTypeId=' + filterObj.employmentTypeId
        + '&pageSize=' + filterObj.pageSize
        + '&pageNumber=' + filterObj.pageNumber
        + '&includeLeave=' + filterObj.includeLeave
        + '&isAnalysis=' + filterObj.isAnalysis
        , function (res) {
        });

    toastr.clear();
};

$(document).on("change", "#ManagerList", function (e) {
    filterObj.managerId = $(this).val();
    filterObj.pageNumber = 1;
    //getFilterActivityReport();
});

$(document).on("change", "#UserList", function (e) {
    filterObj.userId = $(this).val();
    filterObj.pageNumber = 1;
    //getFilterActivityReport();
});

$(document).on("change", "#DivisionList", function (e) {
    filterObj.divisionId = $(this).val();
    filterObj.pageNumber = 1;
    //getFilterActivityReport();
});

$(document).on("change", "#ClientList", function (e) {
    filterObj.clientId = $(this).val();
    filterObj.pageNumber = 1;
    //getFilterActivityReport();
});

$(document).on("change", "#ActivityList", function (e) {
    filterObj.activityId = $(this).val();
    filterObj.pageNumber = 1;
    //getFilterActivityReport();
});

$(document).on("change", "#ProjectList", function (e) {
    filterObj.projectId = $(this).val();
    filterObj.pageNumber = 1;
    //getFilterActivityReport();
});

$(document).on("change", "#EmploymentTypeList", function (e) {
    filterObj.employmentTypeId = $(this).val();
    filterObj.pageNumber = 1;
    //getFilterActivityReport();
});

$(document).on("change", "#users", Report.showOnScreenTimeSheetApproval);