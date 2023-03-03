var id = 0;

$(function () {
    id = $('#ProjectID').val();

    $("#tabs").tabs();
    $('#Cancel').hide();
    $('#Save').hide();
    $('#DeleteAttachment').hide();

    ViewProject.fixedCostSelector($('#BillingMethodID').val());

    $('#BillingMethodID').change(function () {
        ViewProject.fixedCostSelector($(this).val());
    });

    Activity.setAutoComplete();
});

var Tabs = {
    Tab0: function () {
    },
    Tab1: function () {
        Activity.setAutoComplete();
    },
    Tab2: function () {
       refreshDiv();            
     },
    Tab3: function () {
    },
    Tab4: function () {
    }
    
};

var Project = {
    close: function () {
        toastr.info("Processing...");
        window.location = "/Project/Index/";
    },

    cancel: function (id) {
        toastr.info("Processing...");
        jqDialog.confirm({
            message: 'Are you sure you want to cancel this operation?',
            buttons: {
                Yes: function () {
                    if (id > 0)
                        window.location = '/Project/Edit/' + id;
                    else
                        window.location = '/Project/Index/';
                }
            }
        });
    },

    save: function (id) {
        toastr.info("Processing...");

        var isNew = id === 0;
        var url = '/Project/Edit/' + id;

        if (isNew)
            url = '/Project/Create/';

        $.post(url, $('#ProjectDetailFrm').serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();

                $('#BillingMethodID').change(function () {
                    ViewProject.fixedCostSelector($(this).val());
                });

                if (isNew) {
                    $('#tabs-0').html(res);
                    ViewProject.fixedCostSelector($('#BillingMethodID').val());

                    jqDialog.status({
                        success: true,
                        message: 'Project was captured successfully',
                        buttons: {
                            OK: function () {
                                jqDialog.close();
                                window.location = res.Url;
                            }
                        }
                    });
                }
                else {
                    jqDialog.status({
                        success: true,
                        message: 'Project was saved successfully',
                        buttons: {
                            OK: function () {
                                jqDialog.close();
                                window.location.reload();
                            }
                        }
                    });
                }

                $('#Close').show();
                $('#Cancel').hide();
                $('#Save').hide();
            }
        });
    },

    refreshList: function () {
        isProjectActive = $('#StatusFilter').val();
        if (isProjectActive === "1") {
            isProjectActive = true;
        }
        else {
            isProjectActive = false;
        }

        stringSearch = $('#q').val();

        $.post('/Project/RefreshList/', { q: stringSearch, isActive: isProjectActive }, function (res) {
            $('#Index').html(res);
        });
    },

    refreshActivityList: function () {
        id = $('#ProjectID').val();
        isProjectActive = $('#ActivityFilter').val();

        if (isProjectActive === "1") {
            isProjectActive = true;
        }
        else {
            isProjectActive = false;
        }

        $.post('/Project/RefreshActivityList/', { id: id, isActive: isProjectActive }, function (res) {
            $('#ucActivityList').html(res);
        });
    },

    deleteActivity: function (activityID) {
        id = $('#ProjectID').val();
        jqDialog.confirm({
            message: 'Do you want to delete this activity?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Project/DeleteActivity/', { id: id, activityID: activityID }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else
                        {
                            toastr.clear();

                            jqDialog.status({
                                success: true, //false was the original value
                               // message: res.Description,
                                message: "Activity was deleted successfully.",
                                buttons: {
                                   OK: function () {
                                        Crud.loadList();
                                        jqDialog.close();
                                    }
                                }
                            });
                        }
                    });
                }
            }
        });
    },

    endProject: function (id) {
        jqDialog.confirm({
            title: 'CAUTION!!!',
            message: 'Are you sure you want to end this project?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post("/Project/EndProject/", { id: id }, function (res) {
                        $('#tabs-0').html(res);
                    });

                    toastr.clear();
                }
            }
        });
    }
};

var Activity = {
    addUser: function (id) {
        toastr.info("Processing...");

        var ids = [];
        $('.activityIDs:checked').each(function () {
            ids.push($(this).val());
        });

        if (ids.length === 0) {
            toastr.clear();
            toastr.error("Error processing.");

            jqDialog.quickStatus(false, 'Please select the activities to allocate the user to.');
            return false;
        }

        $.post("/Project/AllocateUsers/" + id, $("#frmActivityUsers").serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $("#tabs-2").html(res);
                Activity.setAutoComplete();
            }
        });
    },

    setAutoComplete: function () {
        $("#ActivityName").autocomplete({
            source: "/Project/ActivityLookup/",
            minLength: 3,
            select: function (event, ui) {
                ui.item ? $("#activityIDs").val(ui.item.id) : $("#activityIDs").val("");
            },
            change: function (event, ui) {
            }
        });

        $("#luUser").autocomplete({
            source: "/User/Lookup/",
            minLength: 1,
            select: function (event, ui) {
                ui.item ? $("#userIDs").val(ui.item.id) : $("#userIDs").val("");
            },
            change: function (event, ui) {
                if (!ui.item) {
                    $("#userIDs").val("");
                    $("#luUser").val("");
                }
            }
        });

        $('.datePicker').datepicker({ dateFormat: "yy-mm-dd", speed: "", showOn: 'both', buttonImage: "/Content/Img/calendar.gif", buttonImageOnly: true });
    },

    removeUser: function (id, activityID, userID) {
        jqDialog.confirm({
            message: 'Confirm removing this user from the activity',
            buttons: {
                OK: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post("/Project/RemoveUser/" + id, { activityID: activityID, userID: userID }, function (res) {
                        toastr.clear();
                        $("#divActivityUsersEdit").html(res);                       
                        setAutoComplete();
                    });
                }
            }
        });
    }
};

var UserAllocation = {
    addEditUserAllocation: function (userAllocationID) {
        jqDialog.renderView({
            url: '/Project/UserAllocations/',
            data: { userAllocationID: userAllocationID },
            title: 'User Activity Allocation',
            buttons: {
                Save: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Project/AddEditUserAllocation/', $('#frmUserAllocation').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#tabs-2').html(res);
                        }
                    });
                }
            }
        });
    },

    deleteUserAllocation: function (userAllocationID) {
        jqDialog.confirm({
            message: 'Are you sure you want to delete the user allocation?<br>',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Project/DeleteUserAllocation/', { userAllocationID: userAllocationID }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#tabs-2').html(res);
                            Activity.setAutoComplete();
                        }
                    });
                }
            }
        });
    },

    setConfirmedEndDate: function (userAllocationID) {
        jqDialog.renderView({
            url: '/Project/GetConfirmEndDateView/',
            data: { id: id, userAllocationID: userAllocationID },
            title: 'Set confirmed end date for allocation',
            buttons: {
                Save: function () {
                    var d = $("#ConfirmedEndDate").val();
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Project/SetConfirmedEndDate/', { id: id, userAllocationID: userAllocationID, confDate: d }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $("#tabs-2").html(res);
                            Activity.setAutoComplete();
                        }
                    });
                }
            }
        });
    }
};

var ViewProject = {
    fixedCostSelector: function (indicator) { //if indicator = 1, then fixed cost, if indicator = 2, then T&M
        if (indicator === "2") //FixedCost
        {
            $('#billingMethodSpan').show();
        }
        else /*if (indicator == 1 || indicator == 4 || indicator == 8 || indicator == 16 || indicator == 32) //T&M, SLA, Travel, AdHoc, NonBillable */ {
            $('#billingMethodSpan').hide();
        }
    }
};

var ProjectInvoiceRule = {
    create: function () {
        ClientID = parseInt($('#ClientID').val(), 10);
        ProjectID = parseInt($('#hiddenProjectID').val(), 10);

        jqDialog.confirm({
            title: 'Confirm creating invoice rule',
            message: 'Are you sure you want to create an Invoice Rule for this Project?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    window.open("/InvoiceRule/Create?clientID=" + ClientID + "&projectID=" + ProjectID, "_blank");

                    toastr.clear();
                }
            }
        });
    }
};

//#region CONTRACT ATTACHMENT

var ContractAttachment = {
    delete: function (ClientID, ProjectID) {
        toastr.info("Processing...");

        var fileList = getCheckBoxValues();
        var callingPage = "";
        var newContractDate = "01-01-1900";

        closeDocument();

        $.post('/Project/FileMoveOrDelete/', { clientID: ClientID, projectID: ProjectID, fileList: fileList, callingPage: callingPage, newContractDate: newContractDate }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();

                $('#ucProjectContractDetail').html(res);

                window.location.href = res.redirectTo; //redirect
            }
        });
    },

    displayDocument: function (ProjectID, ClientID, FileName, FilePath, CallingPage) {
        if (FilePath !== "") {
            window.location = "/Project/Edit/?id=" + ProjectID + "&clientID=" + ClientID + "&fileName=" + FileName + "&filePath=" + FilePath + "&callingPage=" + CallingPage;
        }
    }
};

//#endregion

$(document).on("focus", "#ActivityName", function () {
    Activity.setAutoComplete();
});

$(document).on("focus", "#tabs-2", function () {
    Activity.setAutoComplete();
});

function refreshDiv() {
       
 $("#tabs-2").load(location.href + " #tabs-2");
   
};

$(function () {
    $("#ProjectDetailFrm :input").on('change', function () {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#ProjectDetailFrm :input").on('keyup keydown keypress', function (event) {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $('#StartDate').on('change', function () {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $('#EndDate').on('change', function () {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

function getCheckBoxValues() {
    var chkArray = [];

    $(".chkBox:checked").each(function () {
        chkArray.push($(this).attr('id'));
    });

    return chkArray;
}

$(function () {
    $('.chkBox').click(function () {
        if ($("#Attachments input:checkbox:checked").length > 0) {
            $('#DeleteAttachment').show();
        }
        else {
            $('#DeleteAttachment').hide();
        }
    });
});


function closeDocument() {
    //DocuViewareAPI.PostCustomServerAction("docuView", true, "CloseDoc");   // To call code behind method
    if (typeof DocuViewareAPI !== "undefined")
        DocuViewareAPI.CloseDocument("DocuVieware1");
}