var bookLeaveType = 0;

$(function () {
    $('#gridData table tr').click(function () {
        var userID = $(this).children("td:eq(6)").attr('id');
        $.ajax({
            url: "/Manager/CalculateFlexiBalance/",
            data: { UserIDs: userID },
            type: "POST",
            success: function (response) {
                $('#' + userID).html(response);
            }
        });
    });

    $("#calculate").click(function () {
        $('#gridData table tr').each(function () {
            var userID = $(this).children("td:eq(6)").attr('id');
            var matches = new Array();
            matches.push($(this).children("td:eq(6)").attr('id'));

            var postData = { UserIDs: matches.join(',') };
            $.ajax({
                url: "/Manager/CalculateFlexiBalance/",
                data: postData,
                type: "POST",
                success: function (response) {
                    $('#' + userID).html(response);
                }
            });
        });
    });
});

//function CalculateFlexiBalance(userID) {
//    $.post('/Manager/CalculateFlexiBalance/', { userID: userID }, function (res) {
//        if (res.IsError) {
//            jqDialog.quickStatus(false, res.Description);
//        }
//        else {
//            return res + "," + " - " + " Calculate FlexiBalance ";
//        }
//    });

var FlexiDay = {
    bookFlexiDay: function () {
        jqDialog.renderView({
            url: "/Manager/BookFlexiDayView/",
            data: null,
            title: 'Select FlexiDay Details',
            buttons: {
                Save: function () {
                    var userID = $('#UserList').val();
                    var date = $('#FlexiDate').val();

                    if (date.length === 0)
                        return;
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Manager/SubmitFlexiDay/', { userID: userID, date: date }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#RecentFlexiDays').html(res);
                        }
                    });
                }
            }
        });
    },

    deleteFlexiDay: function (flexiDayID) {
        jqDialog.confirm({
            message: 'Would you like to remove this FlexiDay entry?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    $.post('/Manager/RemoveFlexiDay/', { flexiDayID: flexiDayID }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#RecentFlexiDays').html(res);
                        }
                    });
                }
            }
        });
    }
};

var FlexiBalance = {
    updateFlexiBalance: function () {
        jqDialog.renderView({
            url: '/Manager/UpdateFlexiBalanceView/',
            data: null,
            title: 'Update a user\'s FlexiBalance',
            buttons: {
                Save: function () {
                    var userID = $('#UserList').val();
                    var date = $('#AdjDate').val();
                    var movement = $('#Adjustment').val();

                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Manager/UpdateFlexiBalance/', { userID: userID, date: date, movement: movement }, function (res) {
                        if (resizeBy.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#FlexiBalances').html(res);
                        }
                    });
                }
            }
        });
    }
};

var Leave = {
    manage: function (leaveID, approve) {
        jqDialog.confirm({
            message: approve ? 'Would you like to approve this leave?' : 'Would you like to decline this leave?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Manager/ManageLeave/', { leaveID: leaveID, approved: approve }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#RecentLeave').html(res);
                        }
                    });
                }
            }
        });
    },

    getBookLeaveDayForUser: function () {
        toastr.info("Processing...");

        bookLeaveType = 1;

        $.post('/Manager/GetBookLeaveDayForUserPartial/', null, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $("div#dialog div.contents").html(res);
            }
        });
    },

    getBookLeaveDaysForUser: function () {
        toastr.info("Processing...");

        bookLeaveType = 2;

        $.post('/Manager/GetBookLeaveDaysForUserPartial/', null, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $("div#dialog div.contents").html(res);
            }
        });
    },

    bookLeaveForUser: function () {
        bookLeaveType = 2;
        jqDialog.renderView({
            url: '/Manager/GetBookLeaveDaysForUserPartial/',
            data: null,
            title: 'Book Leave for User',
            buttons: {
                OK: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Manager/BookLeaveForUser/' + bookLeaveType, $('#BookUserLeaveFrm').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, "Leave was captured successfully.");
                        }
                    });
                }
            }
        });
    }
};