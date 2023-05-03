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

function SetValidation(target, message) {
    target.addClass("input-validation-error");

    var validationMessage = target.closest(".mb-3-inner").find(".validation-message");
    validationMessage.removeClass("field-validation-valid");
    validationMessage.addClass("field-validation-error");
    validationMessage.text(message);
}

function ClearValidation(target) {
    target.removeClass("input-validation-error");

    var validationMessage = target.closest(".mb-3-inner").find(".validation-message");
    validationMessage.text("");
    validationMessage.removeClass("field-validation-error");
    validationMessage.addClass("text-danger field-validation-valid");
}

function ValidateField(target, validateUrl, params) {
    ClearValidation(target);

    var validationMessage = target.closest(".mb-3-inner").find(".validation-message");
    validationMessage.removeClass("text-danger text-success");
    validationMessage.text("Checking...");

    $.post(validateUrl,
        params,
        function (response) {
            if (!response.success) {
                SetValidation(target, response.message);
            }
            else {
                ClearValidation(target);
            }
        });
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