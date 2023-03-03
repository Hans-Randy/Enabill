$(function () {
});

var Configuration = {
    setPreferences: function () {
        jqDialog.renderView({
            url: '/Configuration/GetPreferenceView/',
            data: {},
            title: 'Your Preferences',
            width: 400,
            buttons: {
                Save: function () {
                    var startTime = $('#StartTime').val();
                    var endTime = $('#EndTime').val();
                    var lunch = $('#Lunch').val();
                    var dayView = $('#DayView').prop('checked');

                    jqDialog.close();

                    toastr.info("Processing...");

                    $.post('/Configuration/SetupPreferences/', { startTime: startTime, endTime: endTime, lunch: lunch, dayView: dayView }, function (res) {
                        toastr.clear();
                        jqDialog.quickStatus(!res.IsError, res.description);
                    });
                }
            }
        });
    },

    setPassPhrase: function () {
        jqDialog.renderView({
            url: '/Configuration/GetPassPhraseView/',
            data: {},
            title: 'Manage PassPhrase',
            buttons: {
                Save: function () {
                    var oldPassPhrase = $('#OldPassPhrase').val();
                    var newPassPhrase = $('#NewPassPhrase').val();
                    var confirmPassPhrase = $('#ConfirmNewPassPhrase').val();

                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Configuration/SetupPassPhrase/', { oldPassPhrase: oldPassPhrase, newPassPhrase: newPassPhrase, confirmPassPhrase: confirmPassPhrase }, function (res) {
                        jqDialog.quickStatus(!res.IsError, res.description);
                    });

                    toastr.clear();
                }
            }
        });
    },

    manageFinPeriods: function (getAll) {
        jqDialog.renderView({
            url: '/Configuration/ManageFinPeriods/',
            data: { getAll: getAll },
            title: 'Manage Financial Periods',
            width: 700,
            buttons: {
                OK: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Configuration/Index/', null, function (res) {
                        $('#CurrentFinPeriod').val($("ActivePeriod").val());
                    });

                    toastr.clear();
                }
            }
        });
    },

    updateFinPeriods: function (finPeriod) {
        $.post('/Configuration/UpdateFinPeriod/', { finPeriod: finPeriod }, function (res) {
            $('#ManagePeriods').html(res);
        });
    },

    addFinPeriodPopup: function () {
        jqDialog.renderView({
            url: '/Configuration/NewFinPeriod/',
            data: {},
            title: 'Add Financial Period',
            width: 400,
            buttons: {
                Save: function () {
                    var periodFrom = $('#PeriodFrom').val();
                    var periodTo = $('#PeriodTo').val();
                    var isCurrent = $('#IsCurrent').val();

                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Configuration/AddFinPeriod/', { dateFrom: periodFrom, dateTo: periodTo, isCurrent: isCurrent }, function (res) {
                        Configuration.manageFinPeriods();
                    });

                    toastr.clear();
                }
            }
        });
    }
};

var Processes = {
    runMonthEndLeaveFlexiBalanceProcess: function () {
        toastr.info("Processing...");

        jqDialog.renderView({
            url: '/Configuration/GetRunMonthEndLeaveFlexiBalanceProcessPartial/',
            data: null,
            title: 'Run Leave & FlexiBalance Update Process',
            buttons: {
                Run: function () {
                    var numOfMonths = parseInt($('#Months').val());
                    var userIDs = [];
                    userIDs.push($('#UserList').val());

                    if (typeof numOfMonths === "number" && numOfMonths > 0) {
                        jqDialog.wait({
                            title: 'Executing...'
                        });

                        $.post('/Configuration/RunMonthEndLeaveFlexiBalanceProcess/', { numberOfMonths: numOfMonths, userIDs: userIDs.toString() }, function (res) {
                            jqDialog.close();
                            toastr.clear();
                            toastr.success("Success processing.");
                            jqDialog.quickStatus(!res.IsError, res.description);
                        });
                    }
                    else {
                        jqDialog.close();
                        toastr.clear();

                        jqDialog.status({
                            success: false,
                            message: 'Please select a value greater than 0',
                            buttons: {
                                OK: function () {
                                    jqDialog.close();

                                    Processes.runMonthEndLeaveFlexiBalanceProcess();
                                }
                            }
                        });

                        return;
                    }
                }
            }
        });
    },

    runUserCostToCompanyProcess: function () {
        toastr.info("Processing...");

        $.post('/Configuration/IsPassphraseCorrect/', null, function (res) {
            if (res === "False") {
                window.location = '/Home/Passphrase?cont=Configuration';

                toastr.clear();

                return;
            }
        });

        jqDialog.renderView({
            url: '/Configuration/GetRunUserCostToCompanyProcessPartial/',
            data: null,
            title: 'Run User Cost To Company Update Process',
            buttons: {
                Run: function () {
                    toastr.info("Processing...");
                    var numOfMonths = parseInt($('#Months').val());

                    if (typeof numOfMonths === "number" && numOfMonths > 0) {
                        jqDialog.wait({
                            title: 'Executing...'
                        });

                        $.post('/Configuration/RunUserCostToCompanyProcess/', { numberOfMonths: numOfMonths }, function (res) {
                            jqDialog.close();
                            toastr.clear();
                            jqDialog.quickStatus(!res.IsError, res.description);
                        });
                    }
                    else {
                        jqDialog.close();
                        toastr.clear();

                        jqDialog.status({
                            success: false,
                            message: 'Please select a value greater than 0',
                            buttons: {
                                OK: function () {
                                    jqDialog.close();
                                    Processes.runMonthEndLeaveFlexiBalanceProcess();
                                }
                            }
                        });

                        return;
                    }
                }
            }
        });
    },

    runDailyInvoiceProcess: function () {
        var initialPeriod = $('#CurrentFinPeriod').val();
        var period = initialPeriod;

        if ($('#ActivePeriod').val() !== undefined) {
            period = $('#ActivePeriod').val();
        }

        var confirmationMessage = 'Would you like to generate invoices for <b>' + period + ' </b>?';

        jqDialog.confirm({
            title: 'Confirm Financial Period',
            message: confirmationMessage,
            buttons: {
                Yes: function () {
                    toastr.info("Processing...");
                    jqDialog.wait();

                    $.post('/Configuration/RunDailyInvoiceProcess/', null, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.description);
                        }
                        else {
                            toastr.clear();
                            $('#ConfigSection').html(res);
                        }

                        jqDialog.close();
                    });
                }
            }
        });
    },

    runTimeSheetApproval: function () {
        jqDialog.confirm({
            message: 'Would you like to run the timesheet approval process?',
            buttons: {
                Yes: function () {
                    toastr.info("Processing...");
                    jqDialog.wait();

                    $.post('/Configuration/RunTimesheetApprovalProcess/', null, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ConfigSection').html(res);
                        }

                        jqDialog.close();
                    });
                }
            }
        });
    },

    runLeaveCycleBalanceProcess: function () {
        jqDialog.confirm({
            message: 'Would you like to run the Leave Cycle Balance process?',
            buttons: {
                Yes: function () {
                    toastr.info("Processing...");
                    jqDialog.wait();

                    $.post('/Configuration/RunLeaveCycleBalanceProcess/', null, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ConfigSection').html(res);
                        }
                        jqDialog.close();
                    });
                }
            }
        });
    },

    runRenewLeaveCycleProcess: function () {
        jqDialog.confirm({
            message: 'Would you like to run the Renew Leave Cycle process?',
            buttons: {
                Yes: function () {
                    toastr.info("Processing...");
                    jqDialog.wait();

                    $.post('/Configuration/RunRenewLeaveCycleProcess/', null, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ConfigSection').html(res);
                        }

                        jqDialog.close();
                    });
                }
            }
        });
    },

    RunAndEmailReports: function (frequency) {
        url = '/Configuration/RunAndEmailReports/?frequency=' + frequency;

        jqDialog.confirm({
            message: 'Would you like to email the ' + frequency + ' reports?',
            buttons: {
                Yes: function () {
                    toastr.info("Processing...");
                    jqDialog.wait();

                    $.post(url, null, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ConfigSection').html(res);
                        }

                        jqDialog.close();
                    });
                }
            }
        });
    }
};