function copyToClipboard(element) {
    var $tempCopy = $("<input>");
    $("body").append($tempCopy);
    $tempCopy.val($(element).text()).select();
    document.execCommand("copy");
    $tempCopy.remove();
}
