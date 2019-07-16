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
}

$(".btn-spinner").on("click", function (e) {
    var parentForm = $(this).parents("form:first");
    if (parentForm.length == 0 || parentForm.valid()) {
        if ($(this).hasClass("disabled")) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
        }
        else {
            $(this).addClass("disabled");
            $(this).children(".fa-spinner").removeClass("d-none");
        }
    }
    else {
        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();
    }
});

$(".btn-spinner-no-validate").on("click", function (e) {
    if ($(this).hasClass("disabled")) {
        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();
    }
    else {
        $(this).addClass("disabled");
        $(this).children(".fa-spinner").removeClass("d-none");
    }
});

function SetValidationMessage(target, message) {
    target.addClass("input-validation-error");

    var validationSpan = target.siblings("span");
    validationSpan.removeClass("field-validation-valid").addClass("field-validation.error");
    var messageSpan = $("<span></span>").text(message);
    validationSpan.html(messageSpan);
}


$(document).on('change', ':file', function (e) {
    var fileInput = $(this),
        filePath = fileInput.val().replace(/\\/g, '/').replace(/.*\//, '');

        validateFile(e, filePath);
});

$(document).on('fileselect', ':file', function (e) {
    var fileInput = $(this),
        filePath = fileInput.val().replace(/\\/g, '/').replace(/.*\//, '');

        validateFile(e, filePath);
});

function validateFile(e, filePath) {
    var file = $(e.target)[0].files[0],
        fileButton = e.target.parentElement,
        fileDisplay = $(e.target).parents('.input-group').find(':text'),
        fileNameField = $('#File_Name');

        fileDisplay.val(filePath);

        if (fileNameField.val().length == 0) {
            fileNameField.val(file.name.split('.')[0]);
        }

        $(fileButton).removeClass('btn-outline-secondary');
        $(fileButton).addClass('btn-success');
}

function updateStub(stub, text) {
    // From https://gist.github.com/mathewbyrne/1280286
    var slug = text.toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/&/g, '-and-')
        .replace(/[^\w\-]+/g, '')
        .replace(/\-\-+/g, '-')
        .replace(/^-+/, '')
        .replace(/-+$/, '');
    stub.val(slug);
}

function validateStub(stub, id, stubCheckUrl) {
    var stubValidation = stub.parent().find('span');
    var sectionId = $("#SectionId");
    if (stub.val().trim() != "") {
        stub.val(stub.val().trim());

        stubValidation.removeClass("text-danger text-success");
        stubValidation.text("Checking stub availability...")

        $.post(stubCheckUrl, {
            item: {
                id: id.val(),
                stub: stub.val(),
                sectionId: sectionId.val()
            }
        }, function (response) {
            if (response) {
                stub.removeClass("valid");
                stub.addClass("input-validation-error");
                stubValidation.removeClass("field-validation-valid text-success");
                stubValidation.addClass("field-validation-error");
                stubValidation.text("The stub is already in use.");
            }
            else {
                stub.removeClass("input-validation-error");
                stub.addClass("valid");
                stubValidation.removeClass("field-validation-error");
                stubValidation.addClass("field-validation-valid text-success");
                stubValidation.text("The stub is available.");
            }
        });
    }
    else {
        stub.removeClass("valid");
        stub.addClass("input-validation-error");
        stubValidation.removeClass("field-validation-valid text-success");
        stubValidation.addClass("field-validation-error");
        stubValidation.text("The Stub field is required.");
    }
}

function clearStubValidation(stub) {
    if (stub.val().trim() != "") {
        var stubValidation = stub.parent().find('span');
        stubValidation.text("");
    }
}

$.validator.setDefaults({
    ignore: ".validation-ignore"
});

function removeAttachmentItem(url, id) {
    $.post(url, { id: id }, function (response) {
        if (response.success == true) {
            var attachmentItemId = "#attachmentItem_" + id;
            $(attachmentItemId).remove();

            var itemCount = $("#attachmentItemList").children().length;
            if (itemCount < 1) {
                $("#attachmentsInputRow").remove();
            }
        }
        else {
            alert(response.message);
        }
    });
}