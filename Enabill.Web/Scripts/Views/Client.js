$(function () {
});

var Tabs = {
    Tab0: function () {
    },
    Tab1: function () {
    },
    Tab2: function () {
    }
};

var Department = {
    createClientDepartmentCode: function (id) {
        jqDialog.renderView({
            url: '/Client/CreateDepartmentCode/',
            data: { id: id },
            title: 'Create Department Code',
            buttons: {
                Save: function () {
                    //jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Client/SaveDepartmentCode', $('#frmEditDepartmentCode').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            if (res.Description === "Please enter a valid Department Code.") {
                                $("#DepartmentCode").val(res.Description).css("color", "red");
                                $("#DepartmentCode").keydown(function () {
                                    $("#DepartmentCode").css("color", "black");
                                });

                                return false;
                            }
                            else {
                                jqDialog.close();
                                jqDialog.quickStatus(false, res.Description);
                            }
                        }
                        else {
                            jqDialog.close();
                            toastr.clear();
                            $('#ucClientDepartmentCodes').html(res);
                        }
                    });
                }
            }
        });
    },

    editClientDepartmentCode: function (id, clientID, isClientActive) {
        jqDialog.renderView({
            url: '/Client/EditDepartmentCode/',
            data: { id: id, clientID: clientID, isClientActive: isClientActive },
            title: 'Edit Department Code',
            buttons: {
                Save: function () {
                    toastr.info("Processing...");

                    $.post('/Client/SaveDepartmentCode', $('#frmEditDepartmentCode').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            if (res.Description === "Please enter a valid Department Code.") {
                                $("#DepartmentCode").val(res.Description).css("color", "red");

                                $("#DepartmentCode").keydown(function () {
                                    $("#DepartmentCode").css("color", "black");
                                });

                                return false;
                            }
                            else {
                                jqDialog.close();
                                jqDialog.quickStatus(false, res.Description);
                            }
                        }
                        else {
                            jqDialog.close();
                            toastr.clear();
                            $('#ucClientDepartmentCodes').html(res);
                        }
                    });
                }
            }
        });
    },

    deleteClientDepartmentCode: function (ClientDepartmentCodeID) {
        jqDialog.confirm({
            message: 'Do you want to delete this department?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Client/DeleteDepartmentCode/', { id: ClientDepartmentCodeID }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            jqDialog.status({
                                success: true,
                                message: "Department successfully deleted",
                                buttons: {
                                    OK: function () {
                                        jqDialog.close();
                                        $('#ucClientDepartmentCodes').html(res);
                                    }
                                }
                            });
                        }
                    });
                }
            }
        });
    }
};

var Client = {
    close: function () {
        jqDialog.confirm({
            message: 'Are you sure you want to close?',
            buttons: {
                Yes: function () {
                    window.location = "/Client/Index/";
                }
            }
        });
    },

    save: function () {
        toastr.info("Processing...");

        var id = $('#ClientID').val();
        var isNew = id === "0";
        var url = "/Client/Edit/" + id;

        if (isNew)
            url = "/Client/Create/";

        $.post(url, $('#ClientDetailFrm').serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                if (!isNew) {
                    $('#tabs-0').html(res);
                }

                toastr.clear();

                jqDialog.status({
                    success: true,
                    message: 'Client was captured successfully.',
                    buttons: {
                        OK: function () {
                            if (isNew) {
                                window.location = res.Url;
                                jqDialog.close();
                                return;
                            }
                            else {
                                jqDialog.close();
                            }
                        }
                    }
                });
            }
        });
    },

    refreshList: function () {
        isClientActive = $('#StatusFilter').val();
        if (isClientActive === "1") {
            isClientActive = true;
        }
        else {
            isClientActive = false;
        }

        stringSearch = $('#q').val();

        $.post('/Client/RefreshList/', { q: stringSearch, isActive: isClientActive }, function (res) {
            $('#Index').html(res);
        });
    },

    activate: function (id) {
        jqDialog.confirm({
            message: 'Are you sure you want to activate this client?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post("/Client/ActivateClient/", { clientID: id }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            jqDialog.status({
                                message: res.Description,
                                buttons: {
                                    OK: function () {
                                        jqDialog.wait();
                                        Window.refresh();
                                    }
                                }
                            });
                        }
                    });
                }
            }
        });
    },

    deactivate: function (id) {
        jqDialog.confirm({
            message: 'Are you sure you want to phase out this client?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post("/Client/DeactivateClient/", { clientID: id }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            jqDialog.status({
                                width: 250,
                                message: res.Description,
                                buttons: {
                                    OK: function () {
                                        Window.refresh();
                                    }
                                }
                            });
                        }
                    });
                }
            }
        });
    }
};