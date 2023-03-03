$(function () {
    $('#EmploymentTypeID').change(function () {
        View.setUpFlexiBalanceForEmploymentType($(this).val());
    });

    $('#Cancel').hide();
    $('#Save').hide();
    $('#DeactivateUser').attr('disabled', true).button('refresh');
});

var Tabs = {
    Tab0: function () {
    },
    Tab1: function () {
    },
    Tab2: function () {
    },
    Tab3: function () {
    },
    Tab4: function () {
    },
    Tab5: function () {
    }
};

var View = {
    setUpFlexiBalanceForEmploymentType: function (value) {
        //EmploymentType
        //1 = Permanent
        //2 = Monthly
        //4 = Hourly

        if (value === 4) {
            $('.flexiBalanceDiv').hide();
        }
        else {
            $('.flexiBalanceDiv').show();
        }
    }
};

var User = {
    close: function () {
        toastr.info("Processing...");
        window.location = "/User/Index/";
    },

    cancel: function () {
        toastr.info("Processing...");
        jqDialog.confirm({
            message: 'Are you sure you want to cancel this operation?',
            buttons: {
                Yes: function () {
                    window.location = "/User/Index/";
                }
            }
        });
    },

    save: function () {
        toastr.info("Processing...");

        var isNew = false;
        var action = 'Edit';

        if (ContextUserID === "0") {
            isNew = true;
            action = 'Create';
        }

        $.post('/User/' + action + '/', $('#UserDetailsFrm').serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();

                if (isNew) {
                    jqDialog.status({
                        success: true,
                        message: 'User was captured successfully.',
                        buttons: {
                            OK: function () {
                                jqDialog.close();
                                window.location = res.Url;
                            }
                        }
                    });
                }
                else {
                    $('#tab-0').html(res);
                    jqDialog.quickStatus(true, 'User was saved successfully.');
                }

                $('#Close').show();
                $('#Cancel').hide();
                $('#Save').hide();
            }
        });
    },

    refreshList: function () {
        isUserActive = $('#StatusFilter').val();
        if (isUserActive === "1") {
            isUserActive = true;
        }
        else {
            isUserActive = false;
        }

        stringSearch = $('#q').val();

        $.post('/User/RefreshList/', { q: stringSearch, isActive: isUserActive }, function (res) {
            $('#Index').html(res);
        });
    },

    activate: function (id) {
        jqDialog.confirm({
            message: 'Are you sure you want to activate this user?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post("/User/Activate/", { id: id }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#tabs-0').html(res);
                            $('#Close').show();
                            $('#Cancel').hide();
                            $('#Save').hide();
                        }
                    });
                }
            }
        });
    },

    deactivate: function (id) {
        jqDialog.confirm({
            message: 'Are you sure you want to deactivate this user?<br/><br/>* Note that the user will only be flagged as Inactive after the Employee End Date.<br/><br/>Other changes will also be saved.',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    //$.post("/User/Deactivate/", { id: id }, function (res) {
                    $.post('/User/Edit/', $('#UserDetailsFrm').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();

                            jqDialog.quickStatus(true, 'User was deactivated successfully.');

                            $('#tab-0').html(res);
                            $('#Close').show();
                            $('#Cancel').hide();
                            $('#Save').hide();
                            $('#DeactivateUser').attr('disabled', true).button('refresh');
                        }
                    });
                }
            }
        });
    },

    addActivity: function () {
        var ids = [];
        $(".addActivity:checked").each(function () {
            ids.push($(this).val());
        });
        $("#divActivities").load("/User/AssignActivities", { userID: $("#UserID").val(), activityIDs: ids.toString(), chargeRate: $("#ChargeRate").val() }, function () {
        });
    },

    removeActivity: function () {
        var ids = [];
        $(".removeActivity:checked").each(function () {
            ids.push($(this).val());
        });
        $("#divActivities").load("/User/RemoveActivities", { userID: $("#UserID").val(), activityIDs: ids.toString() }, function () {
        });
    },

    addRole: function () {
        toastr.info("Processing...");

        var ids = [];
        $(".addRole:checked").each(function () {
            ids.push($(this).val());
        });

        if (ids.length <= 0) {
            toastr.clear();
            toastr.error("Error processing.");

            jqDialog.quickStatus(false, 'Please select a role to assign to the user.');
            return;
        }

        $.post('/User/AssignRoles/', { id: $("#UserID").val(), roleIDs: ids.toString() }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $("#divRoles").html(res);
            }
        });
    },

    removeRole: function () {
        toastr.info("Processing...");

        var ids = [];
        $(".removeRole:checked").each(function () {
            ids.push($(this).val());
        });

        if (ids.length <= 0) {
            toastr.clear();
            toastr.error("Error processing.");

            jqDialog.quickStatus(false, 'Please select a role to remove from the user.');
            return;
        }

        $.post('/User/RemoveRoles', { id: $("#UserID").val(), roleIDs: ids.toString() }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $("#divRoles").html(res);
            }
        });
    }
};

var UserAllocation = {
    assignToActivities: function () {
        toastr.info("Processing...");

        var activityIDs = [];
        var startDate = $('#StartDate');

        $('.activityID:checked').each(function () {
            activityIDs.push($(this).val());
        });

        if (activityIDs.length === 0) {
            toastr.clear();
            toastr.error("Error processing.");

            jqDialog.quickStatus(false, 'Please select one or more activities to add the user to.');
            return;
        }

        if (startDate.length === 0) {
            toastr.clear();
            toastr.error("Error processing.");

            jqDialog.quickStatus(false, 'A start date is required. Please revise the start date, then try again');
            return;
        }

        $('#activityIDs').val(activityIDs.toString());

        $.post('/User/AssignActivitiesToUser/', $('#UserAllocationFrm').serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $('#tabs-3').html(res);
            }
        });
    },

    GetAllActivities: function () {
        toastr.info("Processing...");

        $.post('/User/GetAllActivities/', { id: $("#UserID").val() }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $('#tabs-3').html(res);
                $(".ui-dialog").dialog('close');
            }
        });
    },

    assignReportsToUser: function () {
        toastr.info("Processing...");

        var frequencyIDs = [];

        $('.frequencyID:checked').each(function () {
            frequencyIDs.push($(this).attr('id'));
        });

        $.post('/User/AssignReportsToUser/?selectedReports=' + frequencyIDs, null, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $('#tabs-4').html(res);
            }
        });
    },

    addEditLeaveManualAdjustmentToUser: function (id) {
        jqDialog.renderView({
            url: '/User/LeaveManualAdjustment/',
            data: { id: id },
            title: 'Leave Manual Adjustment',
            buttons: {
                Save: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/User/AddEditLeaveManualAdjustment/', $('#frmLeaveManualAdjustment').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#tabs-5').html(res);
                        }
                    });
                }
            }
        });
    },

    deleteLeaveManualAdjustment: function (id) {
        jqDialog.confirm({
            message: 'Are you sure you want to delete the leave manual adjustment?<br>',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/User/DeleteLeaveManualAdjustment/', { id: id }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#tabs-5').html(res);
                        }
                    });
                }
            }
        });
    },

    addEditUserAllocation: function (id) {
        jqDialog.renderView({
            url: '/User/UserAllocations/',
            data: { id: id },
            title: 'User Activity Allocation',
            buttons: {
                Save: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/User/AddEditUserAllocation/', $('#frmUserAllocation').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#tabs-3').html(res);
                        }
                    });
                }
            }
        });
    },

    deleteUserAllocation: function (id) {
        jqDialog.confirm({
            message: 'Are you sure you want to delete the user allocation?<br>',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/User/DeleteUserAllocation/', { id: id }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#tabs-3').html(res);
                        }
                    });
                }
            }
        });
    },

    setConfirmedEndDate: function (id) {
        jqDialog.renderView({
            url: '/User/GetSetConfirmedEndDateOnUserAllocationView/',
            data: { id: id },
            title: 'Confirm End Date',
            buttons: {
                Save: function () {
                    var endDate = $('#ConfirmedEndDate').val();
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/User/SetConfirmedEndDateOnUserAllocation/', { id: id, date: endDate }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#tabs-3').html(res);
                        }
                    });
                }
            }
        });
    }
};

$(function () {
    $("#UserDetailsFrm :input").change(function () {
        if ($('#EmployEndDate').val().length > 9) {
            $('#Close').hide();
            $('#Cancel').show();
            $('#Save').hide();
        } else {
            $('#Close').hide();
            $('#Cancel').show();
            $('#Save').show();
        }
    });
});

$(function () {
    $("#UserDetailsFrm :input").on('keyup keydown keypress', function (event) {
        if ($('#EmployEndDate').val().length > 9) {
            $('#Close').hide();
            $('#Cancel').show();
            $('#Save').hide();
            $('#DeactivateUser').attr('disabled', false).button('refresh');
        } else {
            $('#Close').hide();
            $('#Cancel').show();
            $('#Save').show();
        }
    });
});

$(function () {
    $('#EmployEndDate').change(function () {
        var value = $(this).val();

        if (value.length > 9) {
            $('#Close').hide();
            $('#Cancel').show();
            $('#Save').hide();
            $('#DeactivateUser').attr('disabled', false).button('refresh');
        }
        else {
            $('#Close').hide();
            $('#Cancel').show();
            $('#Save').show();
            $('#DeactivateUser').attr('disabled', true).button('refresh');
        }
    });
});