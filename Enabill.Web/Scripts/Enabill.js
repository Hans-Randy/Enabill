/* Global variables
-------------------------------------*/
var CurrentUserID = 0;
var ContextUserID = 0;

var screenWidth = 0;
var screenHeight = 0;
var windowWidth = 0;
var windowHeight = 0;

/* Page Load
-------------------------------------*/
$(function () {
    $(".toggle_container").hide();
    $("#trigger1").addClass("active").next().slideDown();
    $("h2.trigger").click(function () {
        $("h2.trigger").removeClass("active").next().slideUp();
        if ($(this).next().is(":hidden")) {
            $(this).toggleClass("active").next().slideToggle("slow");
        } else {
            $("h2.trigger").removeClass("active").next().slideUp();
        } return false;
    });

    Window.initScreen();
    Layout.init();
    Window.ready();

    Window.init();
    Crud.loadList();
    Window.focusFirstInput();
});

/* AJAX Configurations
-------------------------------------*/
$.ajaxSetup({
    // Disable caching of AJAX responses
    cache: false
});

/* Global Functions For Enabill Views
-------------------------------------*/

var Window = {
    ready: function () {
        $('#notifier').ajaxStart(function () {
            Window.changeMouseToWaitCursor();
            setTimeout(function () { Window.showAjaxLoader(); }, 100);
        });
        $('#notifier').ajaxStop(function () {
            setTimeout(function () { Window.hideAjaxLoader(); }, 100);
            Window.changeMouseToNormalCursor();
            Window.init();
        });

        toastr.options = {
            "closeButton": false,
            "debug": false,
            "newestOnTop": false,
            "progressBar": false,
            "positionClass": "toast-bottom-full-width",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "0",
            "extendedTimeOut": "0",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };

        CurrentUserID = $('#CurrentUserID').val();
        //$('#CurrentUserID').val('');
        ContextUserID = $('#ContextUserID').val();
        //$('#ContextUserID').val('');

        //all values that require formatting to decimal : 85.5172 -> 85.52
        $('.requiresNumberFormatting').change(function () {
            var valueToConvert = $(this).val();
            $(this).val(NumberFunctions.toDoubleString(valueToConvert));
        });

        $(window).resize(function () {
            Window.initScreen();
            Layout.init();
        });

        $('form').each(function () {
            Validator.validateForm($(this));
        });
    },

    refresh: function () {
        window.location.reload();
    },

    focusFirstInput: function () {
        // set default focus of inputs
        $(':input[type=text]:visible:enabled:first:not(.datePicker)').focus();
    },

    initScreen: function () {
        screenHeight = window.screen.height;
        screenWidth = window.screen.width;
        windowWidth = $(window).width();
        windowHeight = $(window).height();
    },

    init: function () {
        $('input:submit, input:button, input:reset, a.button').not('.noButton').button();

        DatePicker.toDatePicker($('.datePicker'), null);
        DatePicker.toDatePickerForDateRange($("#StartDate"), $("#EndDate"), {}, {});
        DatePicker.toDatePickerForDateRange($("#DateFrom"), $("#DateTo"), {}, {});

        $('.requiresNumberFormatting').each(function () {
            var valueToConvert = $(this).val();
            $(this).val(NumberFunctions.toDoubleString(valueToConvert));
        });

        $('.requiresNumberFormatting').change(function () {
            var valueToConvert = $(this).val();
            $(this).val(NumberFunctions.toDoubleString(valueToConvert));
        });

        //$(".jDec").allowChars("-0123456789.");
        //$(".jInt").allowChars("-0123456789");

        $('#tabs').tabs({ cookie: { expires: 7, name: "tabCookie" } });

        CKEditorSetup.loadFCK(screenHeight);

        Validator.applyExtraValidation();
    },

    changeMouseToWaitCursor: function () {
        $('body').css({ cursor: "wait" });
    },

    changeMouseToNormalCursor: function () {
        $('body').css({ cursor: "default" });
    },

    showAjaxLoader: function () {
        $('#notifier').slideDown();
    },

    hideAjaxLoader: function () {
        $('#notifier').slideUp();
    }
};

/* Enabill View Specific
-------------------------------------*/

var EnabillViews = {
    updateDropDownList: function (elem, data) {
        var markup = '';
        for (var x = 0; x < data.length; x++) {
            markup += '<option value="' + data[x].Value + '">' + data[x].Text + '</option>';
        }
        elem.html(markup).show();
    }
};

/* Date Picker
-------------------------------------*/

var DatePicker = {
    //This function works for date ranges, like booking for leave.
    toDatePickerForDateRange: function (elemFrom, elemTo, optionsFrom, optionsTo) {
        var defaultsFrom = {
            maxDate: elemTo.val(),
            onSelect: function (selectedDate) {
                elemTo.datepicker("option", 'minDate', selectedDate);
            }
        };

        var defaultsTo = {
            minDate: elemFrom.val(),
            onSelect: function (selectedDate) {
                elemFrom.datepicker("option", 'maxDate', selectedDate);
            }
        };

        //set the FROM element to a datePicker
        var settings = $.extend(defaultsFrom, optionsFrom);
        DatePicker.toDatePicker(elemFrom, settings);

        //set the TO element to a datePicker
        settings = $.extend(defaultsTo, optionsTo);
        DatePicker.toDatePicker(elemTo, settings);
    },

    toDatePicker: function (elem, options) {
        var settings = $.extend(DatePicker.getDatePickerDefaults(), options);
        elem.datepicker(settings);

        elem.addClass('datePicker');
    },

    getDatePickerDefaults: function () {
        var defaults =
            {
                numberOfMonths: 1,
                dateFormat: 'yy-mm-dd',
                showOn: "both",
                buttonImage: "/Content/Img/date_control.png",
                buttonImageOnly: true,
                speed: "",
                showAnim: 'slideDown'
            };

        return defaults;
    }
};

/*
-------------------------------------*/

window.Crud = {
    createUrl: function () { return $('#create-url').val(); },
    editUrl: function () { return $('#edit-url').val(); },
    saveEditUrl: function () { return $('#saveEdit-url').val(); },
    listUrl: function () { return $('#list-url').val(); },
    deleteUrl: function () { return $('#delete-url').val(); },

    getExtraData: function () { var data = $('#ajax-data').serialize(); return data; },

    loadList: function () {
        if ($('#ajax-list').length > 0) {
            $('#ajax-list').load(Crud.listUrl(), Crud.getExtraData());
        }
    },

    bindCreate: function () {
        $('form', $('#ajax-create')).bind('submit', function () { Crud.createSave(); return false; });
    },

    bindEdit: function () {
        $('form', $('#ajax-create')).bind('submit', function () { Crud.editSave(); return false; });
    },

    create: function () {
        var c = $('#ajax-create');
        c.load(Crud.createUrl(), Crud.getExtraData(), function () {
            $('#ajax-create-anchor').hide();
            c.slideDown(function () { $('input[type=text]', c)[0].focus(); });
            Crud.bindCreate();
        });
    },

    edit: function (data) {
        var c = $('#ajax-create');
        c.load(Crud.editUrl(), data, function () {
            c.slideDown(function () {
                $('input[type=text]', c)[0].focus();
                $('#ajax-create-anchor').hide();
            });
            Crud.bindEdit();
        });
    },

    cancelCreate: function () {
        $('#ajax-create').slideUp(function () {
            $('#ajax-create-anchor').show();
        });
    },

    remove: function (id) {
        jqDialog.confirm({
            message: 'Do you want to delete this record?',
            buttons: {
                Yes: function () {
                    $.post(Crud.deleteUrl() + "/" + id, function (res) {
                        jqDialog.close();
                        if (res.IsError) {
                            jqDialog.quickStatus(false, res.description);
                        }
                        else {
                            Crud.loadList();
                            $('#ajax-create').slideUp();
                            $('#ajax-create-anchor').show();
                        }
                    });
                }
            }
        });
    },

    createSave: function () {
        var data = $('#ajax-create form').serialize() + "&" + Crud.getExtraData();

        $.post(Crud.createUrl(), data, function (res) {
            if (res.IsError) {
                //$('#ajax-create').html(res);
                //Crud.bindCreate();
                jqDialog.quickStatus(false, res.description);
            }
            else {
                Crud.loadList();
                $('#ajax-create').slideUp();
                $('#ajax-create-anchor').show();
            }
        });
    },

    editSave: function () {
        var data = $('#ajax-create form').serialize();

        $.post(Crud.saveEditUrl(), data, function (res) {
            if (res.IsError) {
                //$('#ajax-create').html(res);
                //Crud.bindEdit();
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                Crud.loadList();
                $('#ajax-create').slideUp(function () {
                    $('#ajax-create-anchor').show();
                });
            }
        });
    }
};

/* Collapsable Tables
-------------------------------------*/

var CollapsableTable = {
    toggle: function (elem) {
        if (elem.find('.toggleImage').attr('src').indexOf('minimize') >= 0) {
            CollapsableTable.toggleToCollapse(elem);
        }
        else {
            CollapsableTable.toggleToExpand(elem);
        }
    },

    toggleToExpand: function (elem) {
        elem.find('.expanded').show();
        elem.find('.collapsed').hide();
        elem.find('.toggleImage').attr('src', '../../Content/Img/minimize.png');
    },

    toggleToCollapse: function (elem) {
        elem.find('.expanded').hide();
        elem.find('.collapsed').show();
        elem.find('.toggleImage').attr('src', '../../Content/Img/maximize.png');
    }
};

/* Dialogs
-------------------------------------*/
var jqDialog = {
    renderView: function (options) {
        var defaults = { modal: true, resizable: false, width: 350, height: 'auto', title: '', buttons: {}, showCancel: true };
        var settings = $.extend(defaults, options);
        var dialogs = $('#dialog');

        var viewResult = jqDialog.jqGetUrlResponse(options.url, options.data);

        //If error is returned from controller, display the error message in a status dialog.
        //and cancel the view of the requested data
        if (viewResult.indexOf('"IsError":true') >= 0) {
            var desc = viewResult.indexOf('"Description":"');
            var errorMessage = viewResult.substring(desc + 15, viewResult.length - 2);
            jqDialog.quickStatus(false, errorMessage);
            return;
        }

        $('.contents', dialogs).html(viewResult);

        if (settings.showCancel) {
            settings.buttons.Cancel = function () { jqDialog.close(); };
        }

        dialogs.dialog(settings);
        Window.init();
    },

    quickStatus: function (wasSuccessful, description) {
        jqDialog.status({
            success: wasSuccessful,
            message: description,
            buttons: {
                OK: function () {
                    jqDialog.close();
                }
            }
        });
    },

    status: function (options) {
        var defaults = { modal: true, resizable: false, width: 350, height: 'auto', success: true, message: '', buttons: {} };
        var settings = $.extend(defaults, options);
        var dialogs = $('#dialog');

        $('.contents', dialogs).html(settings.message);

        settings.title = "Success";
        if (!settings.success) settings.title = "Error";

        dialogs.dialog(settings);
    },

    confirm: function (options) {
        var defaults = { modal: true, resizable: false, width: 350, height: 'auto', title: 'Are You Sure?', message: '', buttons: {}, showNo: true };
        var settings = $.extend(defaults, options);
        var dialogs = $('#dialog');

        $('.contents', dialogs).html(settings.message);

        if (settings.showNo) {
            settings.buttons.No = function () { jqDialog.close(); };
        }

        dialogs.dialog(settings);
    },

    wait: function (options) {
        jqDialog.close();

        var defaults = { modal: true, draggable: false, closeOnEscape: false, resizable: false, width: 152, height: 'auto', title: 'Loading...', message: '', open: function (event, ui) { $(".ui-dialog-titlebar-close").hide(); } };
        var settings = $.extend(defaults, options);
        var dialogs = $('#dialog');

        $('.contents', dialogs).html(settings.message + "<br /><img alt='One Moment' src='../../Content/Img/loader8.gif' style='width: 130px; margin-left: 0px; display: inline-block;' />");

        dialogs.dialog(settings);
    },

    close: function () {
        $('#dialog').dialog('destroy');

        toastr.clear();
    },

    jqGetUrlResponse: function (url, data) {
        return $.ajax({ type: 'POST', url: url, async: false, data: data }).responseText;
    }
};

/*
-------------------------------------*/

var GenericList = {
    toggle: function (id) {
        if ($('#genericState' + id).text() === 'Read more') {
            GenericList.readMore(id);
        }
        else {
            GenericList.seeLess(id);
        }
    },

    readMore: function (id) {
        $('#genericState' + id).text('See less');
        $('#genericText' + id).css({ 'height': 'auto' });
    },

    seeLess: function (id) {
        $('#genericState' + id).text('Read more');
        $('#genericText' + id).css({ 'height': '50px' });
    }
};

/* Number Functions
-------------------------------------*/

var NumberFunctions = {
    returnNumber: function (v) {
        var g = v.toString();
        var t = "";
        var s = [];

        if (g.split(",") < 0)
            if (g.split(".") > 0) {
                return v;
            }
            else {
                return v + ".00";
            }

        s = g.split(",");
        for (k = 0; k < s.length; k++) {
            t = t + s[k];
        }

        return t;
    },

    toDouble: function (amount) {
        return parseFloat(NumberFunctions.getTruncFromNumber(amount) + "." + NumberFunctions.getDecimalFromNumber(amount));
    },

    toDoubleString: function (v) {
        var g = v.toString();
        var s = [];
        var t = g;
        var result = '.00';

        if (g.indexOf(".") > 0) {
            t = NumberFunctions.getTruncFromNumber(g);
            result = '.' + NumberFunctions.getDecimalFromNumber(g);
        }

        while (t.length > 3) {
            var l = t.length;

            result = ' ' + t.substring(l - 3, l) + result;
            //result = t.substring(l - 3, l) + result;

            t = t.substring(0, l - 3);
        }

        if (t === '.00' || t === '0.00')
            return '0.00';
        else
            return t + result;
    },

    removeNonNumberChars: function (valueToChange) {
        var p = '';

        while (valueToChange.length > 0) {
            var item = valueToChange.substring(0, 1);

            if (item !== '.' && item !== ',' && item.trim().length === 1) {
                //item.trim().length == 1 makes sure that its not a white space
                p += item;
            }

            valueToChange = valueToChange.substring(1, valueToChange.length);
        }

        return p;
    },

    formatValueToNumberString: function (valueToChange) {
        var p = '';

        while (valueToChange.length > 0) {
            var item = valueToChange.substring(0, 1);

            if (item !== ',' && item.trim().length === 1)
                p += item;

            valueToChange = valueToChange.substring(1, valueToChange.length);
        }

        return p;
    },

    getTruncFromNumber: function (num) {
        num = num.toString();
        if (num.indexOf('.') < 0)
            return num;

        var s = [];
        s = num.split(".");

        var t = NumberFunctions.removeNonNumberChars(s[0]);
        return parseInt(t).toString();
    },

    getDecimalFromNumber: function (num) {
        num = num.toString();
        if (num.indexOf('.') < 0)
            return "00";

        var s = [];
        s = num.split(".");
        var t = s[1];

        if (t.length === 1)
            t = t + '0';

        return t.substring(0, 2);
    },

    isValueADouble: function (num) {
        var check = parseFloat(num);
        return check === num;
    },

    isNumber: function (num) {
        return !isNaN(parseFloat(num)) && isFinite(num);
    }
};

/* Date Functions
-------------------------------------*/
var DateFunctions = {
    getDateFormat: function (selectedDate) {
        selectedDate = new Date(selectedDate);

        var year = selectedDate.getFullYear();
        var month = selectedDate.getMonth() + 1;
        var day = selectedDate.getDate();

        var result = year + "-" + month + "-" + day;
        return result.toString();
    }
};

/* URL Functions
-------------------------------------*/

var UrlFunctions = {
    updateUrl: function (pathName, params) {
        window.location.replace(window.location.protocol + "//" + window.location.host + pathName + params);
    }
};

/* Calendar Picker
-------------------------------------*/
var CalendarPicker = {
    BasePicker: function () {
        $(div).calendarPicker({
            useWheel: false,
            callbackDelay: 500,
            years: 1,
            months: 5,
            days: 7,
            showDayArrows: true,
            callback: function (cal) {
            }
        });
    },

    DayPicker: function (div, selectedDate) {
        var datePicker;
        var count = 0;
        datePicker = $(div).calendarPicker({
            useWheel: false,
            callbackDelay: 1200,
            years: 1,
            months: 5,
            days: 7,
            showDayArrows: true,
            callback: function (cal) {
                count++;
                if (count > 1)
                    View.showDay(DateFunctions.getDateFormat(cal.currentDate));
            }
        });
        datePicker.changeDate(new Date(selectedDate));
        $(div).css({ 'margin-top': '10px', 'margin-bottom': '10px' });
    },

    MonthPicker: function (div, selectedDate) {
        var count = 0;
        var datePicker;
        datePicker = $(div).calendarPicker({
            useWheel: false,
            callbackDelay: 0,
            years: 1,
            months: 5,
            days: null,
            callback: function (cal) {
                count++;
                if (count > 1) {
                    View.showMonth(DateFunctions.getDateFormat(cal.currentDate));
                }
            }
        });
        datePicker.changeDate(new Date(selectedDate));
        $(div).css({ 'margin-top': '10px', 'margin-bottom': '10px' });
    }
};

/* Front-End Validation
-------------------------------------*/
var Validator = {
    validateForm: function (elem) {
        elem.validate(
            {
                rules: {},
                messages: {}
            });
    },

    applyExtraValidation: function () {
        $('.normal-year').each(function () {
            $(this).rules("add", {
                number: true,
                min: 1980,
                max: 2080,
                messages: {
                    max: $.validator.format("Please enter a year less than or equal to {0}."),
                    min: $.validator.format("Please enter a year greater than or equal to {0}."),
                    number: $.validator.format("Please enter a valid year.")
                }
            });
        });

        $('.normal-percentage').each(function () {
            $(this).rules("add", {
                number: true,
                min: 0,
                max: 100,
                messages: {
                    max: $.validator.format("Please enter a percenatge less than or equal to {0}."),
                    min: $.validator.format("Please enter a percentage greater than or equal to {0}."),
                    number: $.validator.format("Please enter a valid percentage.")
                }
            });
        });

        $('.extreme-percentage').each(function () {
            $(this).rules("add", {
                number: true,
                min: -1000,
                max: 1000,
                messages: {
                    max: $.validator.format("Please enter a percenatge less than or equal to {0}."),
                    min: $.validator.format("Please enter a percentage greater than or equal to {0}."),
                    number: $.validator.format("Please enter a valid percentage.")
                }
            });
        });

        $('.normal-contactNum').each(function () {
            $(this).rules("add", {
                number: true,
                minlength: 8,
                maxlength: 16,
                messages: {
                    maxlength: $.validator.format("Please enter a valid contact number."),
                    minlength: $.validator.format("Please enter a valid contact number."),
                    number: $.validator.format("Please enter a valid contact number.")
                }
            });
        });

        $('.zero-infinity').each(function () {
            $(this).rules("add", {
                number: true,
                min: 0,
                messages: {
                    min: $.validator.format("This field cannot have a negative value."),
                    number: $.validator.format("Please enter a number.")
                }
            });
        });

        $('.positive-infinity').each(function () {
            $(this).rules("add", {
                number: true,
                min: 1,
                messages: {
                    min: $.validator.format("This field cannot have a negative or zero value."),
                    number: $.validator.format("Please enter a number.")
                }
            });
        });

        $('.positive-currency').each(function () {
            $(this).rules("add", {
                number: true,
                min: 0,
                messages: {
                    min: $.validator.format("Please enter a valid amount."),
                    number: $.validator.format("Please enter a valid amount.")
                }
            });
        });
    }
};

/* Text Editor
-------------------------------------*/
window.editors = new Array();

var CKEditorSetup = {
    loadFCK: function (height) {
        $('.crudFCK:visible').each(function () {
            var h = !height ? 400 : height;
            if (h < 400)
                height = "400";
            else
                height = height - 930;

            var instance = CKEDITOR.instances[this.id];
            if (instance) {
                CKEDITOR.remove(instance);
            }
            CKEDITOR.BasePath = "/Content/ckeditor/";
            CKEDITOR.disableNativeSpellChecker = true;
            CKEDITOR.config.scayt_autoStartup = false;
            CKEDITOR.config.toolbar_Full =
                [
                    ['NewPage', 'Preview', '-', 'Templates'],
                    //['SpellChecker', 'Scayt'],
                    ['Bold', 'Italic', 'Underline'],
                    ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                    ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent'],
                    ['Source'],
                    '/',
                    ['Subscript', 'Superscript'],
                    ['Styles', 'Format', 'Font', 'FontSize'],
                    ['TextColor', 'BGColor', '-', 'SpecialChar', 'Smiley', 'PasteFromWord', 'PasteText', '-', 'Image']
                ];

            CKEDITOR.replace(this.id,
                {
                    filebrowserBrowseUrl: 'http://enabill.saratoga.co.za/Content/ckeditor/filemanager/browser/default/browser.html?Type=Image&Connector=%2FContent%2Fckeditor%2Ffilemanager%2Fconnectors%2Faspx%2Fconnector.aspx',
                    filebrowserImageBrowseUrl: 'http://enabill.saratoga.co.za/Content/ckeditor/filemanager/browser/default/browser.html?Type=Image&Connector=%2FContent%2Fckeditor%2Ffilemanager%2Fconnectors%2Faspx%2Fconnector.aspx',
                    filebrowserFlashBrowseUrl: 'http://enabill.saratoga.co.za/Content/ckeditor/filemanager/browser/default/browser.html?Type=Image&Connector=%2FContent%2Fckeditor%2Ffilemanager%2Fconnectors%2Faspx%2Fconnector.aspx',
                    height: height
                });
        });

        $('.crudFCKtall:visible').each(function () {
            var h = !height ? 400 : height;
            if (h < 500)
                height = "400";
            else
                height = height - 440;
            var instance = CKEDITOR.instances[this.id];
            if (instance) {
                CKEDITOR.remove(instance);
            }
            CKEDITOR.BasePath = "/Content/ckeditor/";
            CKEDITOR.config.scayt_autoStartup = true;
            CKEDITOR.config.toolbar_Full =
                [
                    ['NewPage', 'Preview', '-', 'Templates'],
                    ['SpellChecker', 'Scayt'],
                    ['Bold', 'Italic', 'Underline'],
                    ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                    ['NumberedList', 'BulletedList'],
                    '/',
                    ['Outdent', 'Indent'],
                    ['Source'],
                    ['Subscript', 'Superscript'],
                    ['TextColor', 'BGColor', '-', 'SpecialChar', 'Smiley', 'PasteFromWord', 'PasteText', '-', 'Image'],
                    '/',
                    ['Styles', 'Format', 'Font', 'FontSize']
                ];
        });

        $('.crudFCKmini:visible').each(function () {
            var h = !height ? 400 : height;
            if (h < 500)
                height = "400";
            else
                height = height - 475;
            var instance = CKEDITOR.instances[this.id];
            if (instance) {
                CKEDITOR.remove(instance);
            }
            CKEDITOR.BasePath = "/Content/ckeditor/";
            CKEDITOR.config.scayt_autoStartup = true;
            CKEDITOR.config.toolbar_Full =
                [
                    ['Preview', 'Templates'],
                    ['SpellChecker', 'Scayt'],
                    ['Bold', 'Italic', 'Underline'],
                    ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                    ['NumberedList', 'BulletedList'],
                    ['Outdent', 'Indent'],
                    ['Source', 'Image'],
                    '/',
                    ['Styles', 'Format', 'Font', 'FontSize'],
                    ['TextColor', 'BGColor'],
                    ['SpecialChar', 'Smiley', 'PasteFromWord', 'PasteText']
                ];

            CKEDITOR.replace(this.id,
                {
                    filebrowserBrowseUrl: 'http://enabill.saratoga.co.za/Content/ckeditor/filemanager/browser/default/browser.html?Type=Image&Connector=%2FContent%2Fckeditor%2Ffilemanager%2Fconnectors%2Faspx%2Fconnector.aspx',
                    filebrowserImageBrowseUrl: 'http://enabill.saratoga.co.za/Content/ckeditor/filemanager/browser/default/browser.html?Type=Image&Connector=%2FContent%2Fckeditor%2Ffilemanager%2Fconnectors%2Faspx%2Fconnector.aspx',
                    filebrowserFlashBrowseUrl: 'http://enabill.saratoga.co.za/Content/ckeditor/filemanager/browser/default/browser.html?Type=Image&Connector=%2FContent%2Fckeditor%2Ffilemanager%2Fconnectors%2Faspx%2Fconnector.aspx',
                    height: height
                });
        });

        $('.crudFCKtext:visible').each(function () {
            var h = !height ? 400 : height;
            if (h < 500)
                height = "400";
            else
                height = height - 410;
            var instance = CKEDITOR.instances[this.id];
            if (instance) {
                CKEDITOR.remove(instance);
            }
            CKEDITOR.BasePath = "/Content/ckeditor/";
            CKEDITOR.config.scayt_autoStartup = true;
            CKEDITOR.config.toolbar_Full =
                [
                ];

            CKEDITOR.replace(obj.id,
                {
                    height: height
                });
        });
    },

    CKupdate: function () {
        for (instance in CKEDITOR.instances)
            CKEDITOR.instances[instance].updateElement();
    }
};