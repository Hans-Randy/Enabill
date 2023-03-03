var WorkDay = '';

$(function () {
    View.initialize();
});

var View = {
    initialize: function () {
        WorkDay = $('#WorkDay').val();
        $('#calMonth').datepicker('option', 'onSelect', function (dateText, inst) { View.showMonth(dateText); });
        $('#calDay').datepicker('option', 'onSelect', function (dateText, inst) { View.showDay(dateText); });
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
        $.post('/FlexiTime/Month/', { date: date.toDateString() }, function (res) {
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
        toastr.info("Processing...");

        $.post('/FlexiTime/SelectDay/', { dateString: date }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                //change the url of the browser
                UrlFunctions.updateUrl(window.location.pathname, "?dateString=" + DateFunctions.getDateFormat(date));

                $("#ucDay").html(res);
                View.initialize();
            }
        });
    }
};

var FlexiDay = {
    bookFlexiDay: function () {
        jqDialog.renderView({
            url: "/FlexiTime/DisplayFlexiDayBooking/",
            data: null,
            title: "Book A FlexiDay",
            buttons: {
                "Book FlexiDay": function () {
                    if ($('form#BookflexiDayFrm').valid()) {
                        var flexiDate = $('#FlexiDate').val();
                        var remark = $('#Remark').val();

                        if (flexiDate.length <= 0) {
                            return;
                        }

                        jqDialog.close();
                        toastr.info("Processing...");

                        $.post("/FlexiTime/BookFlexiDate", { remark: remark, flexiDate: flexiDate }, function (res) {
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
            }
        });
    },

    manage: function (flexiDayID, approvalStatusID) {
        /*
        * pending = 1
        * declined = 2
        * approved = 4
        * withdrawn = 8
        */

        if (CurrentUserID === ContextUserID) {
            if (approvalStatusID === 1) {
                //Only allow user to save changes if in pending state
                FlexiDayManagement.showForOwnerPending(flexiDayID);
            }
            else {
                FlexiDayManagement.showForOwner(flexiDayID);
            }
        }
        else {
            if (approvalStatusID === 1) // pending
            {
                FlexiDayManagement.showForPending(flexiDayID);
            }
            else if (approvalStatusID === 2) // declined
            {
                FlexiDayManagement.showForDeclined(flexiDayID);
            }
            else if (approvalStatusID === 4) // approved
            {
                FlexiDayManagement.showForApproved(flexiDayID);
            }
        }
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

var FlexiBalance = {
    adjustFlexiBalance: function (flexiAdjustmentID) {
        jqDialog.renderView({
            url: '/FlexiTime/AdjustFlexiBalanceView/',
            data: { id: flexiAdjustmentID },
            title: 'Adjust FlexiBalance',
            buttons: {
                Save: function () {
                    var userID = ContextUserID;
                    var date = $('#AdjDate').val();
                    var movement = $('#Adjustment').val();
                    var remark = $('#Remark').val();
                    /*
                    if (typeof(movement) != "number") {
                        return;
                    }
                    */

                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/FlexiTime/AdjustFlexiBalance/', { flexiAdjustmentID: flexiAdjustmentID, date: date, movement: movement, remark: remark }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, "Adjustment captured successfully.");
                        }
                    });
                }
            }
        });
    },

    deleteFlexiBalanceAdjustment: function (flexiBalanceID) {
        jqDialog.confirm({
            message: 'Are you sure you want to delete the flexi balance adjustment?<br>',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/FlexiTime/DeleteFlexiBalanceAdjustment/', { id: flexiBalanceID }, function (res) {
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

var FlexiDayManagement = {
    showForOwnerPending: function (flexiDayID) {
        jqDialog.renderView({
            url: '/FlexiTime/GetFlexiDayManagementView/',
            data: { id: flexiDayID },
            title: 'Manage FlexiDay',
            buttons: {
                Save: function () {
                    var form = $('#ManageFlexiDayFrm').serialize();
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/FlexiTime/UpdateFlexiDay/' + flexiDayID, form, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, 'FlexiDay updated successfully.');
                        }
                    });
                },
                WithDraw: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/FlexiTime/WithdrawFlexiDay/', { flexiDayID: flexiDayID }, function (res) {
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

    showForOwner: function (flexiDayID) {
        jqDialog.renderView({
            url: '/FlexiTime/GetFlexiDayManagementView/',
            data: { id: flexiDayID },
            title: 'Manage FlexiDay',
            buttons: {
                WithDraw: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/FlexiTime/WithdrawFlexiDay/', { flexiDayID: flexiDayID }, function (res) {
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

    showForPending: function (flexiDayID) {
        jqDialog.renderView({
            url: '/FlexiTime/GetFlexiDayManagementView/',
            data: { id: flexiDayID },
            title: 'Manage FlexiDay',
            buttons: {
                Approve: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/FlexiTime/ManageFlexiDay/', { flexiDayID: flexiDayID, approved: true }, function (res) {
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

                    $.post('/FlexiTime/ManageFlexiDay/', { flexiDayID: flexiDayID, approved: false }, function (res) {
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

    showForDeclined: function (flexiDayID) {
        jqDialog.renderView({
            url: '/FlexiTime/GetFlexiDayManagementView/',
            data: { id: flexiDayID },
            title: 'Manage FlexiDay',
            buttons: {
                Approve: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/FlexiTime/ManageFlexiDay/', { flexiDayID: flexiDayID, approved: true }, function (res) {
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

    showForApproved: function (flexiDayID) {
        jqDialog.renderView({
            url: '/FlexiTime/GetFlexiDayManagementView/',
            data: { id: flexiDayID },
            title: 'Manage FlexiDay',
            buttons: {
                Decline: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/FlexiTime/ManageFlexiDay/', { flexiDayID: flexiDayID, approved: false }, function (res) {
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