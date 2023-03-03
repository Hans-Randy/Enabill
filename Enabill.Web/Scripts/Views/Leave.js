var bookLeaveViewType = 0;

$(function () {
});

var ThisView = {
    setupDatePicker: function () {
        DatePicker.toDatePickerForDateRange($("#LeaveDateFrom"), $("#LeaveDateTo"), {}, {});
        DatePicker.toDatePickerForDateRange($("#StartDate"), $("#EndDate"), {}, {});
        $('#LeaveDate').datepicker({
            dateFormat: 'yy-mm-dd',
            defaultDate: $("#LeaveDate").val() === "" ? new Date() : new Date($("#LeaveDate").val()),
            showOn: 'both',
            buttonImageOnly: true,
            buttonImage: '/Content/Img/date_control.png',
            onSelect: function () { $('#LeaveRemark').focus(); }
        });
    }
};

var Leave = {
    bookLeave: function () {
        bookLeaveViewType = 2; // Book Leave Days
        jqDialog.renderView({
            url: '/Leave/GetBookLeaveDaysPartial/',
            data: null,
            title: 'Book Leave',
            buttons: {
                Save: function () {
                    var form = $('#BookLeaveFrm').serialize();
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/BookLeave/' + bookLeaveViewType, form, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                        }
                        ThisView.setupDatePicker();
                    });
                }
            }
        });

        ThisView.setupDatePicker();
    },

    changeLeaveBookingType: function (typeID) { // if id == 1 then change view to single day leave booking, if id == 2 then change view to book multiple days
        var url = '';
        bookLeaveViewType = typeID;
        if (bookLeaveViewType === 1)// change view to single day selection
        {
            url = '/Leave/GetBookLeaveDayPartial/';
        }
        else if (bookLeaveViewType === 2)// change view to multiple day selection
        {
            url = '/Leave/GetBookLeaveDaysPartial/';
        }

        $.post(url, null, function (res) {
            $('div#dialog div.contents').html(res);
            ThisView.setupDatePicker();
        });
    },

    //----------------------------
    changeLeaveType: function (leaveTypeID) { // if id == 256 (Birthday Leave) then disable other controls as it defaults to half of the permanent employee's daily work hours.
        var url = '';
        leaveType = leaveTypeID;
        if (leaveTypeID === 256)// Disable some of the other controls
        {
            var birthdayHours = $("#BirthdayHours").val();
            $("#Hours").val(birthdayHours);
            $("#Hours").attr("disabled", "disabled");
        }
        else {
            $("#Hours").removeAttr("disabled", "disabled");
        }
    },
    //----------------------------

    manage: function (leaveID, leaveApprovalID) {
        /*
        * if leave is pending, leaveApprovalID = 1
        * if leave is declined, =2
        * if leave is approved, = 4
        * if leave is withdrawn, = 8
        */

        if (CurrentUserID === ContextUserID) {
            if (leaveApprovalID === 1) {
                //Only allow user to save changes if in pending state
                LeaveManagement.showForOwnerPending(leaveID);
            }
            else {
                LeaveManagement.showForOwner(leaveID);
            }
        }
        else {
            if (leaveApprovalID === 1) // pending
            {
                LeaveManagement.showForPending(leaveID);
            }
            else if (leaveApprovalID === 2) // declined
            {
                LeaveManagement.showForDeclined(leaveID);
            }
            else if (leaveApprovalID === 4) // approved
            {
                LeaveManagement.showForApproved(leaveID);
            }
        }
    }
};

var LeaveManagement = {
    showForOwnerPending: function (leaveID) {
        jqDialog.renderView({
            url: '/Leave/GetLeaveManagementView/',
            data: { id: leaveID },
            title: 'Manage Leave',
            buttons: {
                Save: function () {
                    var form = $('#ManageLeaveFrm').serialize();
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/UpdateLeave/' + leaveID, form, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, 'Leave updated successfully.');
                        }
                    });
                },

                WithDraw: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/WithdrawLeave/', { leaveID: leaveID }, function (res) {
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

    showForOwner: function (leaveID) {
        jqDialog.renderView({
            url: '/Leave/GetLeaveManagementView/',
            data: { id: leaveID },
            title: 'Manage Leave',
            buttons: {
                WithDraw: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/WithdrawLeave/', { leaveID: leaveID }, function (res) {
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

    showForPending: function (leaveID) {
        jqDialog.renderView({
            url: '/Leave/GetLeaveManagementView/',
            data: { id: leaveID },
            title: 'Manage Leave',
            buttons: {
                Approve: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/ManageLeave/', { leaveID: leaveID, approved: true }, function (res) {
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
                },

                Decline: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/ManageLeave/', { leaveID: leaveID, approved: false }, function (res) {
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

    showForDeclined: function (leaveID) {
        jqDialog.renderView({
            url: '/Leave/GetLeaveManagementView/',
            data: { id: leaveID },
            title: 'Manage Leave',
            buttons: {
                Approve: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/ManageLeave/', { leaveID: leaveID, approved: true }, function (res) {
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

    showForApproved: function (leaveID) {
        jqDialog.renderView({
            url: '/Leave/GetLeaveManagementView/',
            data: { id: leaveID },
            title: 'Manage Leave',
            buttons: {
                Decline: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/ManageLeave/', { leaveID: leaveID, approved: false }, function (res) {
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

var UserAllocation = {
    addEditLeaveManualAdjustmentToUser: function (leaveManualAdjustmentID) {
        jqDialog.renderView({
            url: '/Leave/LeaveManualAdjustment/',
            data: { leaveManualAdjustmentID: leaveManualAdjustmentID },
            title: 'Leave Manual Adjustment',
            buttons: {
                Save: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/AddEditLeaveManualAdjustment/', $('#frmLeaveManualAdjustment').serialize(), function (res) {
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

    deleteLeaveManualAdjustment: function (leaveManualAdjustmentID) {
        jqDialog.confirm({
            message: 'Are you sure you want to delete the leave manual adjustment?<br>',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Leave/DeleteLeaveManualAdjustment/', { id: leaveManualAdjustmentID }, function (res) {
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