function openNavColumn() {
    $("#navColumn, #closeNavColumn").addClass("navcolumn-open");
}

$("#openNavColumn").on("click", function () {
    openNavColumn();
});

$("#closeNavColumn").on("click", function () {
    $("#navColumn, #closeNavColumn").removeClass("navcolumn-open");
});

$(".navcolumn-collapsible").on("click", function () {
    $(this).siblings().collapse("toggle");
});

$(".navcolumn-collapse").on("hide.bs.collapse", function (e) {
    if ($(this).is(e.target)) {
        var item = $(this).siblings(".navcolumn-collapsible");
        item.removeClass("navitem-open");
        item.children(".fa-solid").removeClass("fa-chevron-down").addClass("fa-chevron-right");
    }
});

$(".navcolumn-collapse").on("show.bs.collapse", function (e) {
    if ($(this).is(e.target)) {
        var item = $(this).siblings(".navcolumn-collapsible");
        item.addClass("navitem-open");
        item.children(".fa-solid").removeClass("fa-chevron-right").addClass("fa-chevron-down");
    }
});