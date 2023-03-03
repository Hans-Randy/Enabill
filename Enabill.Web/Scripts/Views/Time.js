var WorkDay = '';
var dayWorkSessions = 0;
var dayWorkAllocations = 0;

$(function () {
    View.initialize();
    View.CalculateTotalHours();
    WorkAllocation.setAutoComplete();
});

var View = {
    initialize: function () {
        WorkDay = $('#WorkDay').val();
        $('#calMonth').datepicker('option', 'onSelect', function (dateText, inst) { View.showMonth(dateText); });
        $('#calDay').datepicker('option', 'onSelect', function (dateText, inst) { View.showDay(dateText); });

        if ($("#DayWorkSessions").length > 0) {
            dayWorkSessions = $("#DayWorkSessions").val();
            $("#DayWorkSessions").val('0');
        }

        if ($("#DayWorkAllocations").length > 0) {
            dayWorkAllocations = $("#DayWorkAllocations").val();
            $("#DayWorkAllocations").val('0');
        }
    },

    CalculateTotalHours: function () {
        if ($('#hoursTotal').length) {
            var component = $('#hoursTotal');
            var amount = 0;

            $('.actHours').each(function () {
                var value = $(this).val();

                // Nico - Function below was no longer working
                if (!NumberFunctions.isNumber(value)) {
                    $(this).val('0');
                }

                amount += parseFloat($(this).val());
            });

            component.text(amount + " hours");
        }
    },

    skipToMonth: function (numOfMonthToAdd) {
        var beginDate = new Date(WorkDay);
        var month = (parseInt(beginDate.getMonth()) + parseInt(numOfMonthToAdd)) % 12;
        var year = (parseInt(beginDate.getMonth()) + parseInt(numOfMonthToAdd)) / 12;

        beginDate.setMonth(month);

        // code is used to prevent errors when scrolling backward from january of one year to december of the previous year.
        if (numOfMonthToAdd >= 0) {
            beginDate.setFullYear(parseInt(beginDate.getFullYear()) + year);
        }

        beginDate.setDate(1);
        View.showMonth(beginDate);
    },

    showMonth: function (selectedMonth) {
        toastr.info("Processing...");
        var date = new Date(selectedMonth);

        $.post('/Time/Month/', { date: date.toDateString() }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $("#ucIndex").html(res);
                View.initialize();
            }
        });
    },

    showDay: function (date) {
        UrlFunctions.updateUrl(window.location.pathname, "?dateString=" + DateFunctions.getDateFormat(date));
        /*
        $.post('/Time/SelectDay/', { dateString: date }, function (res) {
        if (res.IsError) {
        jqDialog.quickStatus(false, res.Description);
        }
        else {
        //change the url of the browser
        UrlFunctions.updateUrl(window.location.pathname, "?dateString=" + DateFunctions.getDateFormat(date));

        $("#ucDay").html(res);
        View.initialize();
        }
        });
        */
    }
};

var WorkSession = {
    addWS: function () {
        toastr.info("Processing...");

        $.post("/Time/AddWS/", $("#frmAddWS").serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $("#ucDay").html(res);
                View.initialize();
            }
        });
    },

    approveWS: function (hasExceptions, isUserDaysUnapproved) {
        toastr.info("Processing...");

        if (hasExceptions === 'true') {
            toastr.clear();
            toastr.error("Error processing.");
            jqDialog.quickStatus(false, 'One or more exceptions found. Approval cancelled.');
        }
        else {
            if (isUserDaysUnapproved === 'false') {
                toastr.clear();
                toastr.success("Success processing.");
                jqDialog.quickStatus(false, 'There are no unapproved records to update.');
            }
            else {
                $.post("/Time/ApproveWS/", $("#frmApproveTime").serialize(), function (res) {
                    if (res.IsError) {
                        toastr.clear();
                        toastr.error("Error processing.");
                        jqDialog.quickStatus(false, res.Description);
                    }
                    else {
                        toastr.clear();
                        toastr.success("Success processing.");
                        $('#ucApproveIndex').html(res);
                        jqDialog.quickStatus(true, 'Time successfully approved.');
                    }
                });
            }
        }
    },

    unapproveWS: function (isUserDaysApproved) {
        toastr.info("Processing...");

        if (isUserDaysApproved === 'false') {
            toastr.clear();
            toastr.success("Success processing.");
            jqDialog.quickStatus(false, 'There are no approved records to update.');
        }
        else {
            $.post("/Time/UnApproveWS/", $("#frmApproveTime").serialize(), function (res) {
                if (res.IsError) {
                    toastr.clear();
                    toastr.error("Error processing.");
                    jqDialog.quickStatus(false, res.Description);
                }
                else {
                    toastr.clear();
                    toastr.success("Success processing.");
                    $('#ucApproveIndex').html(res);
                    jqDialog.quickStatus(true, 'Time successfully unapproved.');
                }
            });
        }
    },

    delWS: function (wsID) {
        if (dayWorkSessions === 1 && dayWorkAllocations > 0) {
            jqDialog.confirm({
                message: 'Are you sure you want to delete this work session?<br>All existing work allocations and notes for the day will be deleted too.',
                buttons: {
                    Yes: function () {
                        toastr.info("Processing...");

                        $.post('/Time/DelWS/', { wsID: wsID }, function (res) {
                            if (res.IsError) {
                                toastr.clear();
                                toastr.error("Error processing.");
                                jqDialog.quickStatus(false, res.Description);
                            }
                            else {
                                jqDialog.close();
                                toastr.clear();
                                $('#ucDay').html(res);
                                View.initialize();
                            }
                        });
                    }
                }
            });
        }
        else {
            toastr.info("Processing...");

            $.post('/Time/DelWS/', { wsID: wsID }, function (res) {
                if (res.IsError) {
                    toastr.clear();
                    toastr.error("Error processing.");
                    jqDialog.quickStatus(false, res.Description);
                }
                else {
                    //jqDialog.close();
                    toastr.clear();
                    $('#ucDay').html(res);
                    View.initialize();
                }
            });
        }
    },

    edit: function (workSessionID) {
        toastr.info("Processing...");
        var origRow = $('#WorkSession' + workSessionID);

        $.post('/Time/GetEditWorkSession/', { workSessionID: workSessionID }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                origRow.toggle();
                $(res).insertAfter(origRow);
                toastr.clear();
            }
        });
    },

    update: function (workSessionID) {
        toastr.info("Processing...");
        var origRow = $('#WorkSession' + workSessionID);
        var newRow = $('#WorkSessionEdit' + workSessionID);

        var startDate = $('#StartTime' + workSessionID).val();
        var endDate = $('#EndTime' + workSessionID).val();
        var lunch = $('#LunchTime' + workSessionID).val();

        /*        jqDialog.confirm({
        message: "Would you like to update this work session?",
        buttons: {
        Yes: function () {
        $.post('/Time/UpdateWorkSession/', { workSessionID: workSessionID, startDate: startDate, endDate: endDate, lunch: lunch }, function (res) {
        jqDialog.close();
        if (res.IsError) {
        jqDialog.quickStatus(false, res.Description);
        }
        else {
        $('#ucDay').html(res);
        View.initialize();
        }
        });
        }
        }
        });
        */
        $.post('/Time/UpdateWorkSession/', { workSessionID: workSessionID, startDate: startDate, endDate: endDate, lunch: lunch }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $('#ucDay').html(res);
                View.initialize();
            }
        });
    },

    cancelEdit: function (workSessionID) {
        toastr.info("Processing...");
        var origRow = $('#WorkSession' + workSessionID);
        var newRow = $('#WorkSessionEdit' + workSessionID);

        origRow.toggle();
        newRow.remove();
        toastr.clear();
    }
};

var AdditionalAllocationDetailsPlugins = {
    'Alacrity-Training': function (workAllocationID, activityID, hoursWorked, remark) {
        if (remark.length <= 0) {
            return;
        }
        else {
            WorkAllocation.addTrainingDetails(workAllocationID, activityID, hoursWorked, remark);
        }
    }
};

var WorkAllocation = {
    copyDay: function (dayToCopy, previous) {
        toastr.info("Saving...");

        if (previous === "false")
            WorkAllocation.save();

        $.post('/Time/SaveNextDay/', { dayToCopy: dayToCopy, previous: previous }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                $('#ucDay').html(res);

                if ($('#HoursWorked').length > 0) {
                    $(this).val("");
                    $('#Remark').val("");
                }

                toastr.clear();
                View.initialize();
            }
        });
    },

    copyWeek: function (currentDay, previous) {
        toastr.info("Saving...");

        $.post('/Time/SaveNextWeek/', { currentDay: currentDay, previous: previous }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $('#ucWeek').html(res);
                window.location.href = res.redirectTo; //redirect
            }
        });
    },

    save: function () {
        toastr.info("Saving...");
        // get the extra details
        var oneOrMoreRequiredRemarksMissing = 'false';

        $('.actHours').each(function (item) {
            item = $(this);

            if (item.val() > 0) {
                var hoursWorked = item.val();
                var remark = $('.Remark', item.parents('tr')[0]).val();
                var mustHaveRemarks = $('.MustHaveRemarks', item.parents('tr')[0]).val();
                var workAllocationID = $('.WorkAllocationID', item.parents('tr')[0]).val();
                var trainingCategoryID = $('.TrainingCategoryID', item.parents('tr')[0]).val();
                var activityID = $('.ActivityID', item.parents('tr')[0]).val();
                var tag = $('.Activity-Info', item.parents('tr')[0]).val();

                if (mustHaveRemarks === 'true' && $.trim(remark).length <= 0) {
                    oneOrMoreRequiredRemarksMissing = 'true';
                }

                if (tag in AdditionalAllocationDetailsPlugins) {
                    if (trainingCategoryID === 0) {
                        AdditionalAllocationDetailsPlugins[tag](workAllocationID, activityID, hoursWorked, remark);
                    }
                }
            }
        });

        if (oneOrMoreRequiredRemarksMissing === 'true') {
            toastr.clear();
            toastr.error("Error processing.");
            jqDialog.quickStatus(false, 'One or more remarks are required. Action cancelled.');
        }
        else {
            toastr.clear();
            WorkAllocation.saveDayAllocation();
        }
    },

    saveDayAllocation: function () {
        toastr.info("Processing...");

        $.post('/Time/SaveDayAllocation/', $('#frmUserAllocations').serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                $('#ucDay').html(res);

                if ($('#HoursWorked').length > 0) {
                    $(this).val("");
                    $('#Remark').val("");
                }

                toastr.clear();
                View.initialize();
            }
        });
    },

    addExtraAllocation: function (activityID) {
        jqDialog.renderView({
            url: '/Time/AllocateExtraTime/',
            data: { activityID: activityID },
            title: 'Add Extra Allocation',
            buttons: {
                Save: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Time/AddExtraAllocation/', $('#frmExtraAllocation').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucDay').html(res);
                            View.initialize();
                        }
                    });
                }
            }
        });

        WorkAllocation.setAutoComplete();
    },

    setAutoComplete: function () {
        $('.AutoComplete').each(function (item) {
            var itemID = $(this).attr("id");
            var clientID = itemID;
            var projectID = itemID;
            clientID = clientID.replace('Ticket', '#ClientID');
            projectID = projectID.replace('Ticket', '#ProjectID');

            $(this).autocomplete({
                source: function (request, response) {
                    $.get("/Time/LookupTicket", { term: request.term, clientID: $(clientID).val(), projectID: $(projectID).val() }, response);
                },
                minLength: 1,
                select: function (event, ui) {
                    ui.item ? $("#TicketIDs").val(ui.item.id) : $("#TicketIDs").val("");
                }
            });
        });
    },

    addTrainingDetails: function (workAllocationID, activityID, hoursWorked, remark) {
        jqDialog.renderView({
            url: '/Time/TrainingDetails/',
            data: { workAllocationID: workAllocationID, activityID: activityID, hoursWorked: hoursWorked, remark: remark },
            title: 'Training Details',
            buttons: {
                Save: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Time/AddTrainingDetails/', $('#frmTrainingDetails').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucDay').html(res);
                            View.initialize();
                        }
                    });
                }
            }
        });
    },

    deleteWA: function (workAllocationID) {
        jqDialog.confirm({
            message: 'Are you sure you want to delete this work allocation?<br>Any linked items, eg. Notes, will be deleted too.',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Time/DeleteAllocation/', { id: workAllocationID }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucDay').html(res);
                            View.initialize();
                        }
                    });
                }
            }
        });
    }
};

var Note = {
    viewNote: function (workAllocationID) {
        var viewTitle = 'View Note';
        jqDialog.renderView({
            url: '/Note/QuickEditView/',
            data: { workAllocationID: workAllocationID },
            width: 800,
            title: viewTitle,
            buttons: {
                Save: function () {
                    CKEditorSetup.CKupdate();
                    var noteText = $('#NoteText').val();
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Time/SaveNote/', { workAllocationID: workAllocationID, noteText: noteText }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            $('#ucDay').html(res);
                            toastr.clear();
                            View.initialize();
                            jqDialog.quickStatus(true, 'Note saved successfully.');
                        }
                    });
                }
            }
        });
    }
};

var Timesheet = {
    changeLockStatus: function (currentStatus) {
        if (currentStatus === 'True') {
            //i am locked, going to be unlocked
            Timesheet.unlock();
        }
        else {
            //i am unlocked, going to be locked
            Timesheet.lock();
        }
    },

    unlock: function () {
        jqDialog.confirm({
            message: 'Would you like to unlock the selected month\'s timesheet?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Time/UnlockTimesheet/', null, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                        }
                    });
                }
            }
        });
    },

    lock: function () {
        jqDialog.confirm({
            message: 'Would you like to lock the selected month\'s timesheet?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Time/LockTimesheet/', null, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                        }
                    });
                }
            }
        });
    }
};

var FlexiDay = {
    bookFlexiDay: function () {
        jqDialog.renderView({
            url: '/FlexiTime/DisplayFlexiDayBooking/',
            data: null,
            title: "Book A FlexiDay",
            buttons: {
                Save: function () {
                    if ($('form#BookflexiDayFrm').valid()) {
                        var remark = $('#Remark').val();
                        jqDialog.close();
                        toastr.info("Processing...");

                        $.post('/Time/BookFlexiDate/', { remark: remark }, function (res) {
                            if (res.IsError) {
                                toastr.clear();
                                toastr.error("Error processing.");
                                jqDialog.quickStatus(false, res.Description);
                            }
                            else {
                                toastr.clear();
                                $('#ucDay').html(res);
                                jqDialog.quickStatus(true, 'Flexi day booked successfully.');
                            }
                        });
                    }
                }
            }
        });
    },

    deleteFlexiDay: function (flexiDayID) {
        jqDialog.confirm({
            message: 'Would you like to remove this FlexiDay?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/FlexiTime/RemoveFlexiDay/', { flexiDayID: flexiDayID }, function (res) {
                        jqDialog.close();
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                        }
                    });
                }
            }
        });
    }
};

$(document).ready(function () {
    $('#DateFrom').datepicker({
        dateFormat: 'yy-mm-dd',
        maxDate: new Date($("#DateTo").val()),
        defaultDate: new Date($("#DateFrom").val()),
        onSelect: function (dateText, inst) {
            filterObj.dateFrom = dateText;
        },
        buttonImage: '/Content/Img/date_control.png',
        buttonImageOnly: true,
        showOn: "both"
    });

    $('#DateTo').datepicker({
        dateFormat: 'yy-mm-dd',
        defaultDate: new Date($("#DateTo").val()),
        onSelect: function (dateText, inst) {
            filterObj.dateTo = dateText;
        },
        buttonImage: '/Content/Img/date_control.png',
        buttonImageOnly: true,
        showOn: "both"
    });
});

getApproveUserIndex = function () {
    filterObj.dateFrom = $("#DateFrom").val();
    filterObj.dateTo = $("#DateTo").val();
    filterObj.managerID = $("#ManagerList").val();
    $.get('/Time/GetApproveUserIndex?dateFrom=' + filterObj.dateFrom + '&dateTo=' + filterObj.dateTo + "&managerID=" + filterObj.managerID + "&status=" + filterObj.status, null, function (res) {
        $("#ucApproveUserIndex").empty();
        $("#ucApproveUserIndex").html(res);
        $('#DateFrom').datepicker({
            dateFormat: 'yy-mm-dd',
            maxDate: new Date($("#DateTo").val()),
            defaultDate: new Date($("#DateFrom").val()),
            onSelect: function (dateText) {
                filterObj.dateFrom = dateText;
            },
            buttonImage: '/Content/Img/date_control.png',
            buttonImageOnly: true,
            showOn: "both"
        });
        $('#DateTo').datepicker({
            dateFormat: 'yy-mm-dd',
            minDate: new Date($("#DateFrom").val()),
            defaultDate: new Date($("#DateTo").val()),
            onSelect: function (dateText) {
                filterObj.dateTo = dateText;
            },
            buttonImage: '/Content/Img/date_control.png',
            buttonImageOnly: true,
            showOn: "both"
        });
    });
};

var filterObj = {
    'dateFrom': null,
    'dateTo': null,
    'managerID': 0,
    'status': 1
};

$(document).on("change", "#ManagerList", function (e) {
    filterObj.managerID = parseInt($(this).val());
});

$(document).on("change", "#UserWorkDayStatus", function (e) {
    filterObj.status = parseInt($(this).val());
});

$(document).on("click", "#btnSearch", function () {
    getApproveUserIndex();
});