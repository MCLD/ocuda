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
            buttons.sort((a, b) => (a.Sort > b.Sort ? 1 : b.Sort > a.Sort ? -1 : 0));
            buttons.forEach(function (b) {
                let buttonText =
                    '<a href="' +
                    b.Link +
                    '" class="btn btn-outline-primary mx-1">' +
                    b.Button +
                    "</a>";
                $(".modal-content-footer").append(buttonText);
            });
        }
        $(".prom-modal-details").modal().show();
    });
});
