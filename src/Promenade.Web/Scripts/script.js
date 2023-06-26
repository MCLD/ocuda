document.addEventListener("DOMContentLoaded", function () {
    $(".prom-carousel").slick({
        slidesToShow: 4,
        slidesToScroll: 4,
        autoplay: false,
        dots: false,
        prevArrow:
            '<span class="fas fa-arrow-alt-circle-left fa-2x prom-carousel-nav prom-carousel-prev"></span>',
        nextArrow:
            '<span class="fas fa-arrow-alt-circle-right fa-2x prom-carousel-nav prom-carousel-next"></span>',
        responsive: [
            {
                breakpoint: 1350,
                settings: {
                    slidesToShow: 3,
                    slidesToScroll: 3,
                },
            },
            {
                breakpoint: 1000,
                settings: {
                    slidesToShow: 2,
                    slidesToScroll: 2,
                },
            },
            {
                breakpoint: 750,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                },
            },
        ],
    });
});

document.addEventListener("DOMContentLoaded", function () {
    $(".prom-carousel-modal").on("click", function (event) {
        if (typeof bootstrap !== 'undefined') {
            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();

            const buttons = [];

            $.each($(this).data(), function (key, value) {
                switch (key) {
                    case "title":
                        $(".modal-content-title").text(value);
                        break;
                    case "img":
                        $(".modal-content-image").attr("src", value);
                        break;
                    case "description":
                        $(".modal-content-description").text(value);
                        break;
                    default:
                        if (key.length > 6 && key.substring(0, 6) === "button") {
                            buttons.push(value);
                        }
                        break;
                }
            });

            if (buttons.length > 0) {
                $(".modal-content-footer").text("");
                buttons.sort(function (a, b) {
                    return a.Sort - b.Sort;
                });
                buttons.forEach(function (b) {
                    $(".modal-content-footer").append('<a href="' +
                        b.Link +
                        '" target="_blank" class="btn btn-outline-primary m-1">' +
                        b.Button +
                        "</a>");
                });
            }

            bootstrap
                .Modal
                .getOrCreateInstance(document.getElementById("prom-carousel-modal-details"))
                .show();
        }
    });
});

function ResetSpinners(target) {
    if (target != null) {
        target.removeClass("disabled");
        target.children(".fa-spinner").addClass("d-none");
    }
    else {
        $(".btn-spinner, .btn-spinner-no-validate").removeClass("disabled");
        $(".btn-spinner, .btn-spinner-no-validate").children(".fa-spinner").addClass("d-none");
    }
}

window.onunload = function () {
    ResetSpinners();
};

$(".btn-spinner").on("click", function (e) {
    if ($(this).hasClass("disabled")) {
        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();
    }
    else {
        var parentForm = $(this).closest("form");

        if (!$(this).hasClass("spinner-ignore-validation")
            && (parentForm.length > 0 && !parentForm.valid())) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
        }
        else {
            parentForm.find(".btn-spinner").addClass("disabled");
            $(this).children(".fa-spinner").removeClass("d-none");
        }
    }
});