var id = 0;

$(function () {
    id = $('#RegionID').val();
    $("#tabs").tabs();
    $('#Cancel').hide();
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
    }    
};

var Region = {
    close: function () {
        toastr.info("Processing...");
        window.location = "/Region/Index/";
    },
    cancel: function (id) {
        toastr.info("Processing...");
        jqDialog.confirm({
            message: 'Are you sure you want to cancel this operation?',
            buttons: {
                Yes: function () {
                    if (id > 0)
                        window.location = '/Region/Edit/' + id;
                    else
                        window.location = '/Region/Index/';
                }
            }
        });
    },
    save: function (id) {
        toastr.info("Processing...");

        var isNew = id === 0;
        var url = '/Region/Edit/' + id;

        if (isNew)
            url = '/Region/Create/';

        $.post(url, $('#RegionDetailFrm').serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                if ($("#RegionName").val() === "") {
                    $("#RegionName").css("border", "solid 1px red");
                    $("#RegionName").keydown(function () {
                        $("#RegionName").css("border", "");
                    });
                } else if ($("#RegionShortCode").val() === "") {
                    $("#RegionShortCode").css("border", "solid 1px red");
                    $("#RegionShortCode").keydown(function () {
                        $("#RegionShortCode").css("border", "");
                    });
                }
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();            
                 if (isNew) {
                    $('#tabs-0').html(res);               
                    jqDialog.status({
                        success: true,
                        message: 'Region was captured successfully',
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
                        message: 'Region was saved successfully',
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
        showAllRegions = false;
        isRegionActive = $('#StatusFilter').val();
        if (isRegionActive === "1") {
            isRegionActive = true;
        }
        else if (isRegionActive === "2") {
            showAllRegions = true;
            isRegionActive = false;
        }
        else {
            isRegionActive = false;
        }

        stringSearch =  $('#q').val();

        $.post('/Region/RefreshList/', { q: stringSearch, isActive: isRegionActive, showAllRegions: showAllRegions }, function (res) {
            $('#Index').html(res);
        });
    }
};
