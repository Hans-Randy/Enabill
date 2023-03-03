var Layout = {
    init: function () {
        var pageMinWidth = windowWidth - 50; // 50 is the margin
        if (pageMinWidth < 700) {
            pageMinWidth = 700;
        }

        var maxFontSize = windowWidth / 10;
        if (maxFontSize > 100) {
            maxFontSize = 100;
        }

        var menuItems = $('#MenuCount').val();
        var menuAnchorSize = windowWidth / menuItems;

        if (menuAnchorSize > 70)
            menuAnchorSize = 70;

        $('#menuItems .menuAnchor').css({
            'width': menuAnchorSize - 2 + 'px',
            'font-size': maxFontSize + '%'
        });

        if (windowWidth < 920) {
            $('#logon-span a, #logon-span td').css({ 'font-size': "5.43pt" });
            $('.spaceMe').css({ 'width': 2.3 });
        }
        else {
            $('#logon-span a, #logon-span td').css({ 'font-size': '10pt' });
            $('.spaceMe').css({ 'width': 30 });
        }
    }
};