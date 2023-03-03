var defaultDate = null;
var monthNames = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
];

var updateDateCalendarAndForm = function (dataObj) {

    defaultDate = new Date(selectedYear, dataObj.selectedMonth, dataObj.selectedDay);

    $("#holidayDisplayDate").val(dataObj.selectedDay + "-" + monthNames[dataObj.selectedMonth]);
    $("#holidayId").val(dataObj.hId);
    $("#holidayName").val(dataObj.Name);
    $("#holidayDate").val(defaultDate.toLocaleString());

    $("#holidayIsFixedDate").prop("checked", dataObj.IsFixed === "True");
    $("#holidayIsRepeated").prop("checked", dataObj.IsRepeat === "True");

    //$("#holiday-delete").prop("disabled", !(dataObj.IsFixed === "False" && dataObj.IsRepeat === "False"));

    $("#calendar").datepicker("refresh");
};

var onSelectFunction = function (date, inst) {
    var dteObj = new Date(date);
    
    var dte = dteObj.getMonth() + 1 + "-" + dteObj.getDate();
    var dteFormatted = monthNames[dteObj.getMonth()] + " " + dteObj.getDate();
    if (nonWorkingDays.includes(dte) && !showForm) {
        $("#workdayDateDisplay").val(dteFormatted);
        $("#workdayDateValue").val(dteObj.toLocaleString());
        $("#workdayIsWorkable").prop("checked", 0);

        $("#WorkableForm").css('display', 'block');
        $("#WorkableForm").show('fast');
    } else {
        $("#WorkableForm").css('display', 'none');
    }
    updateDateCalendarAndForm(
        {
            selectedMonth: dteObj.getMonth(),
            selectedDay: dteObj.getDate(),
            hId: 0,
            IsRepeat: 0,
            IsFixed: 0
        }
    );
};

var onBeforeShowDay = function (date) {
    var classes = [];
    var dte = date.getMonth() + 1 + "-" + date.getDate();

    // select date that is selected by user via plugin or external sources, by setting defaultDate and refreshing plugin;
    if (defaultDate !== null &&
        date.getMonth() === defaultDate.getMonth() &&
        date.getDate() === defaultDate.getDate())
        classes.push('custom-ui-state-active');

    // Sunday 0, Saturday 6
    if (date.getDay() === 0)
        classes.push('custom-ui-state-weekend custom-ui-sunday');
    else if (date.getDay() === 6)
        classes.push('custom-ui-state-weekend custom-ui-saturday');

    // non working day from Workable table
    if (nonWorkingDays.includes(dte))
        classes.push('custom-ui-state-non-workday');

    // check if day is a holiday
    if (holidays.includes(dte))
        classes.push('custom-ui-state-holiday');

    return [true, classes.join(' ')];
};

var showYear = function (year) {
    window.location = "/holiday?year=" + year;
    return;
};

$(document).ready(function () {
    if (showForm) {
        $("#HolidayForm").css('display', 'block');
        $("#HolidayForm").show('fast');

        $("#holiday-apply").css('display', 'block');
        $("#holiday-apply").show('fast');
    }

    $("#calendar").datepicker({
        defaultDate: defaultDate,
        numberOfMonths: 12,
        minDate: new Date(selectedYear, 0, 1),
        maxDate: new Date(selectedYear, 11, 31),
        hideIfNoPrevNext: true,
        onSelect: onSelectFunction,
        beforeShowDay: onBeforeShowDay
    });

    $("a.holiday-entry").on("click",
        function () {
            var thisEl = $(this);

            updateDateCalendarAndForm(
                {
                    selectedMonth: thisEl.attr("data-month") - 1,
                    selectedDay: thisEl.attr("data-day"),
                    hId: thisEl.attr("data-id"),
                    Name: thisEl.attr("data-name"),
                    IsRepeat: thisEl.attr("data-options-repeat"),
                    IsFixed: thisEl.attr("data-options-fixed")
                }
            );
        });

    $("#holiday-save").click(
        function (e) {
        
            e.preventDefault();
            // get all the inputs into an array.
            var SelectedYear = $('#selectedYear').val();
            var HolidayId = $('#holidayId').val();
            var HolidayName = $('#holidayName').val();
            var DateOfHoliday = $('#holidayDisplayDate').val();
            var isFixed = true;
            var IsFixedInput = $('[name="IsFixed"]');
            if (IsFixedInput.is(':checked')) {
                isFixed = true;
            } else {
                isFixed = false;
            }
            var isRepeated = true;
            var IsRepeatedInput = $('[name="IsRepeated"]');
            if (IsRepeatedInput.is(':checked')) {
                isRepeated = true;
            } else {
                isRepeated = false;
            }

            var values = { SelectedYear, HolidayId, HolidayName, DateOfHoliday, isFixed, isRepeated};

            if (values.HolidayName === "" || values.DateOfHoliday === "") {
                toastr.warning("Nothing to post.");
                return false;
            }

            toastr.info("Processing...");
            $.ajax({
                type: "POST",
                url: '/Holiday/Create',
                dataType: "html",
                type: "POST",
                data: { SelectedYear, HolidayId, HolidayName, DateOfHoliday, isFixed, isRepeated},
                success: function (data) {
                    toastr.success("Saved...");
                    location.reload();
                    return true;
                },
                fail: function (data) {
                    toastr.error("Error Processing");
                    return false;
                }
            });
        });

    $("#holiday-delete").click(
        function (e) {
            toastr.info("Processing...");
            e.preventDefault();

            // get all the inputs into an array.
            var values = {};
            $('#HolidayForm :input').each(function () {
                values[this.name] = $(this).val();
            });

            $.ajax({
                type: "POST",
                url: '/Holiday/Delete',
                dataType: 'json',
                async: false,
                data: values,
                success: function (data) {

                    location.reload();
                    return true;
                },
                fail: function (data) {
                    toastr.error("Error Processing");
                    return false;
                }
            });
        });

    $("#workable-save").click(
        function (e) {
            toastr.info("Processing...");
            e.preventDefault();

            // get all the inputs into an array.
            var values = {};
            $('#WorkableForm :input').each(function () {
                values[this.name] = $(this).val();
            });

            $.ajax({
                type: "POST",
                url: '/Holiday/WorkDayChange',
                dataType: 'json',
                async: false,
                data: values,
                success: function (data) {

                    location.reload();
                    return true;
                },
                fail: function (data) {
                    toastr.error("Error Processing");
                    return false;
                }
            });
        });

    $("#holiday-apply").click(
        function (e) {
            e.preventDefault();

            // get all the inputs into an array.
            $.ajax({
                type: "POST",
                url: '/Holiday/applyholidays',
                dataType: 'json',
                async: false,
                data: { year: selectedYear },
                success: function (data) {
                    toastr.success("Saved...");
                    location.reload();
                    return true;
                },
                fail: function (data) {
                    toastr.error("Error Processing");
                    return false;
                }
            });
        });
});