// #region DATA TABLE

var dataTable = {
    week: function datatableWeek
        (
            arrayOfArrays,
            startDate,
            isMondayWorkable,
            isTuesdayWorkable,
            isWednesdayWorkable,
            isThursdayWorkable,
            isFridayWorkable,
            dayLockedMonday,
            dayLockedTuesday,
            dayLockedWednesday,
            dayLockedThursday,
            dayLockedFriday,
            dayLockedSaturday,
            dayLockedSunday,
            ThreeState
        ) {
        var record;
        var rowNumber = 0;
        var editedRowNumber = 0;
        var total;
        var refreshValues;
        var columnRefresh = "";
        var activityId; // This value will be set to current activity you select before you edit.
        var selectedDay = ""; // This is the day of an activity that is selected in the "Hour" cell and is needed to match a Remark to.
        var selectedDayTime = 0; // The "hour" cell needs to be filled in before a Remark can be added for the time.
        var editedDay = ""; // This is the day of an activity that is edited in the "Hour" cell and is needed to match a Remark to.
        var editedDayTime = 0; // The "hour" cell needs to be filled in before a Remark can be edited for the time.
        var recordEdited = false;
        var remark = "";
        var remarkDay = "";
        var mustHaveRemarks = false;
        var chkThreeState = true;
        var countThreeState = 0;

        toastr.info("Loading...");

        webix.ready(function () {
            webix.ui({
                css: "style-table", // <==== this line here to use custom styling.
                container: "dataTable",
                rows: [
                    {
                        view: "treetable",
                        columns: [
                            {
                                id: "Client",
                                header: [{ content: "filterCheckBoxes", compare: threeStateCompare }, "Grouped by Client then Project", "Total Hours logged"],
                                vertical: true,
                                width: 400,
                                template: function (obj, common) {
                                    return common.space(obj, common) + common.icon(obj, common) + common.treecheckbox(obj, common) + "&nbsp" + obj.Value;
                                }
                            },
                            {
                                id: "Monday",
                                header: [shortDate(startDate, 0), "Mon", { content: "totalColumn" }],
                                editor: "text",
                                tooltip: "Double-click to edit cell",
                                css: "" + IsWorkableStyle(isMondayWorkable, dayLockedMonday) + " centered"
                            },
                            {
                                id: "Tuesday",
                                header: [shortDate(startDate, 1), "Tue", { content: "totalColumn" }],
                                editor: "text",
                                tooltip: "Double-click to edit cell",
                                css: "" + IsWorkableStyle(isTuesdayWorkable, dayLockedTuesday) + " centered"
                            },
                            {
                                id: "Wednesday",
                                header: [shortDate(startDate, 2), "Wed", { content: "totalColumn" }],
                                editor: "text",
                                tooltip: "Double-click to edit cell",
                                css: "" + IsWorkableStyle(isWednesdayWorkable, dayLockedWednesday) + " centered"
                            },
                            {
                                id: "Thursday",
                                header: [shortDate(startDate, 3), "Thu", { content: "totalColumn" }],
                                editor: "text",
                                tooltip: "Double-click to edit cell",
                                css: "" + IsWorkableStyle(isThursdayWorkable, dayLockedThursday) + " centered"
                            },
                            {
                                id: "Friday",
                                header: [shortDate(startDate, 4), "Fri", { content: "totalColumn" }],
                                editor: "text",
                                tooltip: "Double-click to edit cell",
                                css: "" + IsWorkableStyle(isFridayWorkable, dayLockedFriday) + " centered"
                            },
                            {
                                id: "Saturday",
                                header: [shortDate(startDate, 5), "Sat", { content: "totalColumn" }],
                                editor: "text",
                                tooltip: "Double-click to edit cell",
                                css: "" + IsWorkableStyle("False", dayLockedSaturday) + " centered"
                            },
                            {
                                id: "Sunday",
                                header: [shortDate(startDate, 6), "Sun", { content: "totalColumn" }],
                                editor: "text",
                                tooltip: "Double-click to edit cell",
                                css: "" + IsWorkableStyle("False", dayLockedSunday) + " centered"
                            },
                            {
                                id: "Remark",
                                width: 1040,
                                header: ["Remarks", "", { content: "totalWeek" }],
                                editor: "text",
                                tooltip: "Double-click to edit cell",
                                attributes: { maxlength: 3 }
                            }
                        ],
                        autoheight: true,
                        autowidth: true,
                        columnWidth: 55,
                        editable: true,
                        editaction: "dblclick",
                        clipboard: "custom",
                        templateCopy: function (item, row, col) { return item + "," + row + "," + col + "|"; },
                        select: "cell",
                        multiselect: true,
                        threeState: true,
                        checkboxRefresh: true,
                        filterMode: { openParents: false, level: 3 },
                        tooltip: true,
                        data: arrayOfArrays,
                        on: {
                            onItemClick: function (id) {
                                webix.storage.local.put("state", $$("$treetable1").getState());
                            },

                            onItemCheck: function (id, value) {
                                var activityIDs = [];
                                activityId = this.getItem(id).ActivityId;

                                // Multiple checkbox changes.
                                if (activityId === undefined) {
                                    var i = 0;

                                    this.data.eachSubItem(id, function (obj) {
                                        // Level 3 (Top level parent).
                                        if (obj.$level === 3) {
                                            activityIDs[i] = this.getItem(obj.id).ActivityId;
                                            i++;
                                        }
                                    });
                                }
                                else {
                                    activityIDs[0] = activityId;
                                }

                                postCheckboxData(activityIDs, value);
                            },

                            // Select
                            onAfterSelect: function (selection, preserve) {
                                record = this.getItem(selection.row);

                                // If record edited then day that was edited is previously selected day.
                                if (recordEdited) {
                                    editedDay = selectedDay;
                                    editedDayTime = selectedDayTime;
                                    editedRowNumber = rowNumber;
                                }

                                // If hour cell is selected.
                                if (selection.column.substring(selection.column.length - 3) === "day") {
                                    selectedDay = selection.column;
                                    selectedDayTime = record[selectedDay];
                                }
                                else if (selection.column === "Remark") {
                                    if (rowNumber !== selection.row) {
                                        webix.message({ type: "msginfo", text: "Please select an 'hour' cell in the same row as the remark.", expire: 2000 });
                                        editedDay = "";
                                        selectedDay = "";
                                    }
                                }

                                if (remarkDay === "" && editedDay === "") {
                                    remarkDay = 'Remark' + selectedDay;
                                }
                                else {
                                    remarkDay = 'Remark' + editedDay;
                                }

                                record.Remark = remarkDay === "" ? "" : record[remarkDay];

                                recordEdited = false;
                            },
                            onBeforeUnSelect: function (selection, preserve) {
                                record = this.getItem(selection.row);
                                record.Remark = "";
                                remarkDay = "";
                                rowNumber = selection.row;
                                if (selection.col === "Remark") {
                                    recordEdited = false;
                                }
                            },

                            // Edit
                            onBeforeEditStart: function (id) {
                                activityId = this.getItem(id).ActivityId;
                                remark = record[remarkDay];
                                mustHaveRemarks = record["MustHaveRemarks"];
                                var isDayLocked = "False";

                                switch (id.column) {
                                    case "Monday":
                                        isDayLocked = dayLockedMonday;
                                        break;
                                    case "Tuesday":
                                        isDayLocked = dayLockedTuesday;
                                        break;
                                    case "Wednesday":
                                        isDayLocked = dayLockedWednesday;
                                        break;
                                    case "Thursday":
                                        isDayLocked = dayLockedThursday;
                                        break;
                                    case "Friday":
                                        isDayLocked = dayLockedFriday;
                                        break;
                                    case "Saturday":
                                        isDayLocked = dayLockedSaturday;
                                        break;
                                    case "Sunday":
                                        isDayLocked = dayLockedSunday;
                                        break;
                                }

                                if (isDayLocked === "True") {
                                    webix.message({ type: "msginfo", text: "Day is locked!", expire: 2000 });
                                    return false;
                                }

                                if (id.column === "Remark") {
                                    if (selectedDay === "") {
                                        webix.message({ type: "msginfo", text: "Please select an 'hour' cell under a Day before adding/editing the remark.", expire: 2000 });
                                        return false;
                                    }
                                    else if (selectedDayTime === 0) {
                                        webix.message({ type: "msginfo", text: "Please fill in time under a Day before adding/editing the remark.", expire: 2000 });
                                        return false;
                                    }

                                    record.Remark = remark;
                                }

                                recordEdited = true;

                                return !isNaN(id.row); // Row id is the timestamp and thus a number. Grouped values are mixture of number and the label. Expressed as 0$ActivityName.
                            },
                            onBeforeEditStop: function (state, editor) {
                                if (state.value === state.old) {
                                    recordEdited = false;
                                    return true;
                                }

                                if (editor.column !== "Remark") {
                                    if (isNaN(state.value)) {
                                        webix.message({ type: "error", text: editor.column + " must be a number!", expire: 2000 });

                                        return false;
                                    }
                                    if (state.value <= 0 && state.value !== "") {
                                        webix.message({ type: "error", text: editor.column + " please enter a positive number!", expire: 2000 });

                                        return false;
                                    }
                                    else {
                                        editedDay = editor.column;
                                    }

                                    //Check if a decimal value and then round off to two decimal places.
                                    if (state.value % 1 !== 0) {
                                        var wholePart = Math.trunc(state.value);
                                        var fractionPart = state.value - wholePart;

                                        if (fractionPart <= 0.25) {
                                            fractionPart = 0.25;
                                        }
                                        else if (fractionPart <= 0.50) {
                                            fractionPart = 0.50;
                                        }
                                        else if (fractionPart <= 0.75) {
                                            fractionPart = 0.75;
                                        }
                                        else {
                                            fractionPart = 1;
                                        }

                                        state.value = wholePart + fractionPart;
                                    }
                                }

                                // Needed if the Enter button is hit instead of clicking on another cell. I.e. the 'select' events are not fired.
                                if (recordEdited) {
                                    editedDay = selectedDay;
                                    editedDayTime = selectedDayTime;
                                    editedRowNumber = editedRowNumber === 0 ? editor.row : editedRowNumber;
                                }

                                return true;
                            },
                            onAfterEditStop: function (state, editor) {
                                toastr.info("Saving...");

                                record = this.getItem(editor.row);

                                if (editor.column !== "Remark") {
                                    // If hour is removed, then remove corresponding remark.
                                    editedDayTime = state.value;
                                    remark = editedDayTime === "" || !editedDayTime > 0 ? "" : remark;
                                }
                                else {
                                    remark = state.value;
                                    editedDayTime = editedDayTime === null || editedDayTime === "" ? record[editedDay] : editedDayTime;
                                }

                                record["Remark" + editedDay] = remark;

                                if (recordEdited) {
                                    record.Remark = remark;
                                }
                                else {
                                    record.Remark = "";
                                }

                                // If value hasn't changed, then bypass updating (Note: cannot use strict '!==' comparison as state.value is string and state.old is numeric).
                                if ((state.value != state.old && state.value !== "") || (state.value === "" && state.old !== null && state.old !== "")) {
                                    // If an hour cell has been edited.
                                    if (editor.column.substring(editor.column.length - 3) === "day") {
                                        // Update Treetable totals.
                                        updateTreetableTotal(state.value, state.old, editor.row, editedDay, this);

                                        // Enforce remarks if mandatory.
                                        if (mustHaveRemarks && editedDayTime > 0 && (remark === null || !remark.length > 0)) {
                                            webix.message({ type: "msginfo", text: "A remark is required.", expire: 2000 });

                                            recordEdited = true;
                                            this.unselect(rowNumber);
                                            this.clearSelection();
                                            this.select(editedRowNumber, "Remark", true);

                                            return false;
                                        }
                                    }

                                    // Post back to the database.
                                    if (editedDayTime === "" || editedDayTime > 0 || state.value > 0) {
                                        postData(activityId, startDate, editor.column, state.value, editedDay, editedDayTime, remark, mustHaveRemarks);
                                        toastr.clear();
                                        webix.message({ type: "msgsuccess", text: editor.column + " submitted", expire: 2000 });
                                    }
                                    else {
                                        toastr.clear();
                                    }

                                    //Change focus to the Remark cell if hour cell was edited.
                                    if (editor.column.substring(editor.column.length - 3) === "day") {
                                        recordEdited = false;
                                        this.unselect(rowNumber);
                                        this.clearSelection();
                                        this.select(editedRowNumber, "Remark", true);

                                        if (editedDayTime === "") {
                                            selectedDay = "";
                                            editedDay = "";
                                        }
                                    }
                                    else {
                                        selectedDay = "";
                                        editedDay = "";
                                    }
                                }

                                // Reset values.
                                rowNumber = 0;
                                editedRowNumber = 0;
                                recordEdited = false;

                                return;
                            },

                            //Paste.
                            onPaste: function (text) {
                                toastr.info("Saving...");

                                text = text.split("|");
                                var j = 0;
                                var sel = this.getSelectedId(true);
                                var updatedTime = false;

                                this.clearSelection();

                                for (var i = 0; i < sel.length; i++) {
                                    // Logic ensures that there are no empty array values and that one cell can be copied to multiple cells.
                                    j = text.length - 1 > i ? i : text.length - 1;

                                    //If array does not have complete data, then use previous array value.
                                    if (text[j].length < 8) j--;

                                    textRecord = text[j].split(",");
                                    record = this.getItem(textRecord[1]);

                                    //Only if cell copied to is in the same row as the cell copied from.
                                    if (record.id === sel[i].row) {
                                        //Check if column pasted into is not the Remark column.
                                        if (sel[i].column === "Remark") continue;

                                        tempValue = textRecord[0].match(/\d+/g).map(Number);
                                        var copiedValue = "";

                                        for (j = 0; j < tempValue.length; j++) {
                                            copiedValue += tempValue[j];

                                            if (j === 0 && tempValue.length > 1)
                                                copiedValue += ".";
                                        }

                                        copiedValue = parseFloat(copiedValue);
                                        copiedRemark = "Remark" + textRecord[2];
                                        copiedRemark = record[copiedRemark];

                                        var pastedColumn = sel[i].column;
                                        var pastedColumnValue = record[pastedColumn];
                                        var pastedRemark = "Remark" + pastedColumn;

                                        record[pastedColumn] = copiedValue;
                                        record[pastedRemark] = copiedRemark;

                                        // Update Treetable totals.
                                        updateTreetableTotal(copiedValue, pastedColumnValue, record.id, pastedColumn, this);

                                        toastr.clear();

                                        updatedTime = true;
                                    }
                                    else {
                                        webix.message({ type: "msginfo", text: "Ensure that the cells copied to correspond with the same row as the cells copied from.", expire: 2000 });
                                    }

                                    this.refresh(sel[i]);
                                }

                                //Save back to the database if values changed.
                                if (updatedTime) {
                                    var arrModel = [];

                                    this.eachRow(function (row) {
                                        record = this.getItem(row);

                                        if (record.Monday ||
                                            record.Tuesday ||
                                            record.Wednesday ||
                                            record.Thursday ||
                                            record.Friday ||
                                            record.Saturday ||
                                            record.Sunday) {
                                            var model = {};

                                            model.ActivityId = record.ActivityId;
                                            model.Monday = record.Monday;
                                            model.Tuesday = record.Tuesday;
                                            model.Wednesday = record.Wednesday;
                                            model.Thursday = record.Thursday;
                                            model.Friday = record.Friday;
                                            model.Saturday = record.Saturday;
                                            model.Sunday = record.Sunday;
                                            model.RemarkMonday = record.RemarkMonday;
                                            model.RemarkTuesday = record.RemarkTuesday;
                                            model.RemarkWednesday = record.RemarkWednesday;
                                            model.RemarkThursday = record.RemarkThursday;
                                            model.RemarkFriday = record.RemarkFriday;
                                            model.RemarkSaturday = record.RemarkSaturday;
                                            model.RemarkSunday = record.RemarkSunday;

                                            arrModel.push(model);
                                        }
                                    });
                                    webix.message({ type: "msgsuccess", text: "Time submitted", expire: 2000 });

                                    return postPasteData(arrModel);
                                }
                            },
                            onBlur: function () {
                                this.clearSelection();

                                return true;
                            },
                            onAfterLoad: function () {
                                this.eachRow(function (row) {
                                    record = this.getItem(row);
                                    record.Remark = "";

                                    if (record.IsHidden) {
                                        this.blockEvent();
                                        this.checkItem(row);
                                        this.unblockEvent();
                                    }

                                    if (record.MustHaveRemarks)
                                        mustHaveRemarksCSS(this, row);
                                });
                            }
                        }
                    }
                ]
            });

            // #region GROUPING

            $$("$treetable1").data.group({
                by: function (obj) {
                    return obj.Project + "-" + obj.Client;
                },
                map: {
                    Client: ["Client"],
                    Value: ["Project"],
                    Monday: ["Monday", "sum"],
                    Tuesday: ["Tuesday", "sum"],
                    Wednesday: ["Wednesday", "sum"],
                    Thursday: ["Thursday", "sum"],
                    Friday: ["Friday", "sum"],
                    Saturday: ["Saturday", "sum"],
                    Sunday: ["Sunday", "sum"]
                }
            });

            $$("$treetable1").data.group({
                by: "Client",
                map: {
                    Value: ["Client"],
                    Project: ["Project"],
                    Monday: ["Monday", "sum"],
                    Tuesday: ["Tuesday", "sum"],
                    Wednesday: ["Wednesday", "sum"],
                    Thursday: ["Thursday", "sum"],
                    Friday: ["Friday", "sum"],
                    Saturday: ["Saturday", "sum"],
                    Sunday: ["Sunday", "sum"]
                }
            });

            // #endregion GROUPING

            // #region CHECKBOXES

            $$("$treetable1").data.each(function (obj) {
                if (obj.$level === 1) {
                    var numberLevel2Checkboxes = 0;
                    var numberLevel2Checked = 0;
                    var numberLevel3Checkboxes = 0;
                    var numberLevel3Checked = 0;

                    $$("$treetable1").data.eachChild(obj.id, function (child1obj) {
                        numberLevel2Checkboxes++;

                        $$("$treetable1").data.eachChild(child1obj.id, function (child2obj) {
                            numberLevel3Checkboxes++;

                            if (child2obj.IsHidden) {
                                $$("$treetable1").checkItem(child2obj.id);
                                numberLevel3Checked++;
                            }
                            else
                                $$("$treetable1").uncheckItem(child2obj.id);
                        });

                        // Update Level 2 checkbox.
                        if (numberLevel3Checked === numberLevel3Checkboxes) {
                            $$("$treetable1").checkItem(child1obj.id);
                            numberLevel2Checked++;
                        }
                    });

                    // Update Level 3 checkbox.
                    if (numberLevel2Checked === numberLevel2Checkboxes) {
                        $$("$treetable1").checkItem(obj.id);
                    }
                }
            });

            // #endregion CHECKBOXES

            // #region STATE

            var state = webix.storage.local.get("state");

            if (state)
                $$("$treetable1").setState(state);

            // #endregion STATE

            toastr.clear();
        });

        // #region COLUMN TOTALS

        webix.ui.datafilter.totalColumn = webix.extend({
            refresh: function (master, node, value) {
                var result = 0, _val;

                master.data.each(function (obj) {
                    if (obj.$group) return;

                    _val = Number(getHoursFromRow(obj, value.columnId));

                    if (!isNaN(_val)) result += _val;
                }, this, true);

                if (value.format)
                    result = value.format(result);

                if (value.template)
                    result = value.template({ value: result });

                // Refresh logic needed to prevent the total being repeated with each iteration of column totals.
                if (columnRefresh === "") {
                    total = 0;
                    refreshValues = true;
                    columnRefresh = value.columnId;
                }
                else
                    if (refreshValues)
                        refreshValues = columnRefresh === value.columnId ? false : true;

                if (refreshValues)
                    total += result;

                if (result === 0) result = "";

                node.firstChild.style.textAlign = "center";
                node.firstChild.innerHTML = result;
            }
        }, webix.ui.datafilter.summColumn);

        webix.ui.datafilter.totalWeek = webix.extend({
            refresh: function (master, node, value) {
                if (total % 1 !== 0) {
                    total = Math.round(total * 100) / 100;
                }

                node.firstChild.innerHTML = total;
                columnRefresh = "";
            }
        }, webix.ui.datafilter.summColumn);

        // #endregion COLUMN TOTALS

        // #region FILTER CHECKBOXES

        webix.ui.datafilter.filterCheckBoxes = webix.extend({
            getInputNode: function (node) {
                return node.firstChild ? node.firstChild.firstChild : {
                    indeterminate: true
                };
            },
            getValue: function (node) {
                /*
                    3-state

                    Checked:
                    value: true
                    three: false

                    Unchecked:
                    value: false
                    three: false

                    Three:
                    value: false
                    three: true
                */

                var value = this.getInputNode(node).checked;

                ThreeState = this.getInputNode(node).indeterminate;
                ThreeState = ThreeState ? "thirdState" : value;

                return ThreeState;
            },
            _stateSetter: function (e) {
                if (this.readOnly)
                    this.checked = this.readOnly = false;
                else if (!this.checked)
                    this.readOnly = this.indeterminate = true;

                ThreeState = this.readOnly ? "thirdState" : this.checked;

                postThreeStateData(ThreeState)
            },
            refresh: function (master, node, column) {
                master.registerFilter(node, column, this);
                node.querySelector("input").onclick = this._stateSetter;
                node.querySelector("input").indeterminate = chkThreeState === 'thirdState';
                node.querySelector("input").checked = chkThreeState === 'true';
                node.querySelector("input").onchange = function () {
                    master.filterByAll();
                };
            },
            render: function (master, column) {
                if (countThreeState === 0) {
                    chkThreeState = ThreeState;
                    countThreeState++;
                }

                var html = "<input type='checkbox' id='cb1'>" + "Hide/Show rows";

                return html;
            }
        }, webix.ui.datafilter.numberFilter);

        // #endregion FILTER CHECKBOXES

        // #region POST DATA

        function postData(activityId, startDate, column, value, editedDay, editedDayTime, remark, mustHaveRemarks) {
            var dataObj = {};
            dataObj.activityId = activityId;
            dataObj.startDate = startDate;
            dataObj.column = column;
            dataObj.value = value;
            dataObj.editedDay = editedDay;
            dataObj.editedDayTime = editedDayTime;
            dataObj.Remark = remark;
            dataObj.mustHaveRemarks = mustHaveRemarks;

            $.ajax({
                type: "POST",
                url: '/Time/Week',
                dataType: 'json',
                async: false,
                data: dataObj,
                success: function () {
                    return true;
                },
                fail: function () {
                    return false;
                }
            });
        }

        function postPasteData(obj) {
            var objToSend = {};
            objToSend.startDate = startDate;
            objToSend.Logs = obj;

            $.ajax({
                type: "POST",
                url: '/Time/WeekPaste',
                dataType: 'json',
                async: false,
                data: objToSend,
                success: function () {
                    return true;
                },
                fail: function () {
                    return false;
                }
            });
        }

        function postThreeStateData(filter) {
            var objState = {};
            objState.threeState = filter.toString();

            $.ajax({
                type: "POST",
                url: '/Configuration/WeekThreeStateUpdate',
                dataType: 'json',
                async: false,
                data: objState,
                success: function () {
                    return true;
                },
                fail: function () {
                    return false;
                }
            });
        }

        function postCheckboxData(activityIDs, value) {
            var objToSend = {};
            objToSend.activityIDs = activityIDs;
            objToSend.isHidden = value;

            $.ajax({
                type: "POST",
                url: '/Time/WeekCheckboxUpdate',
                dataType: 'json',
                async: false,
                data: objToSend,
                success: function () {
                    return true;
                },
                fail: function () {
                    return false;
                }
            });
        }

        // #endregion POST DATA
    }
};

// #endregion DATA TABLE

// #region STYLING

function IsWorkableStyle(isDayWorkable, isDayLocked) {
    var style = "";
    //style = isDayWorkable === "False" || isDayLocked === "True" ? "non_workable" : "";
    style = isDayWorkable === "False" || isDayLocked === "True" ? "non_workable" : "";
    return style;
}

function mustHaveRemarksCSS(table, row) {
    table.addCellCss(row, "Remark", "must_have_remarks");
}

// #endregion STYLING

// #region HEADER FUNCTIONS

$(function () {
    // Get the elements.
    var tablemain = document.getElementById("main");
    var underlineposition = document.getElementById("headerUnderline");
    var stickytable = document.getElementById("dataTable");

    // Get the offset position of the data table. OffsetTop is the distance from top of element to bottom of first fixed element.
    underlineDistance = underlineposition.offsetTop + underlineposition.clientHeight;
    stickyDistance = stickytable.offsetTop;

    // When the user scrolls the page, execute myFunction.
    tablemain.addEventListener("scroll", stickyHeader);
});

// Add the sticky class to the header when you reach its scroll position. Remove "sticky" when you leave the scroll position.
function stickyHeader() {
    var tableheader = document.getElementById("main");

    if (tableheader.scrollTop >= stickyDistance) {
        webix.html.addStyle(".webix_ss_header{position: fixed; top: " + underlineDistance + "px !important}", "stickyStyle");
    } else {
        webix.html.removeStyle("stickyStyle");
    }
}

function shortDate(startDate, addDays) {
    objDate = new Date(startDate);

    // Get numeric day value.
    var dayNumber = objDate.getDate();

    // Get numeric month value.
    var monthNumber = objDate.getMonth() + 1; // getMonth return integer from 0 - 11.

    // Get numeric year value.
    var yearNumber = objDate.getFullYear();

    // Get number of days in month.
    noDaysInMonth = daysInMonth(monthNumber, yearNumber);

    if (dayNumber + addDays <= noDaysInMonth) {
        dayNumber = dayNumber + addDays;
        objDate.setDate(dayNumber);
    }
    else {
        dayNumber = dayNumber + addDays - noDaysInMonth;
        objDate.setMonth(objDate.getMonth() + 1, 1);
    }

    //Short Month
    var shortMonth = objDate.toLocaleString('en-us', { month: 'short' });

    return shortMonth + " " + dayNumber;
}

function daysInMonth(month, year) {
    return new Date(year, month, 0).getDate();
}

function getHoursFromRow(obj, columnId) {
    switch (columnId) {
        case "Monday":
            return obj.Monday;
        case "Tuesday":
            return obj.Tuesday;
        case "Wednesday":
            return obj.Wednesday;
        case "Thursday":
            return obj.Thursday;
        case "Friday":
            return obj.Friday;
        case "Saturday":
            return obj.Saturday;
        case "Sunday":
            return obj.Sunday;
        default:
            return 0;
    }
}

function updateTreetableTotal(newValue, oldValue, rowId, editedDay, thisobj) {
    newValue = newValue || 0;
    oldValue = oldValue || 0;
    var difference = newValue - oldValue;
    var parentId = "";
    var parentRecord = "";
    var currentValue = 0;

    //Update parent.
    do {
        parentId = parentId === "" ? thisobj.getParentId(rowId) : thisobj.getParentId(parentId);
        parentRecord = thisobj.getItem(parentId);
        currentValue = parentRecord[editedDay];
        currentValue += difference;

        if (currentValue % 1 !== 0) {
            currentValue = Math.round(currentValue * 100) / 100;
        }

        parentRecord[editedDay] = currentValue;
    }
    while (parentRecord.$level > 1);
}

function threeStateCompare(value, filter, obj) {
    if (filter === "thirdState") return true; // Return all rows.

    if (filter && obj.checked) // 3-state checked and rows checked.
        return true;
    else if (filter === "" && !obj.checked) // 3-state unchecked and row unchecked. This should be the default setting.
        return true;
    else
        return false; // 3-state unchecked and row checked.
}

// #endregion HEADER FUNCTIONS