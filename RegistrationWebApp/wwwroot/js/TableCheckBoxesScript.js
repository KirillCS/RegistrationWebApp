function mainCheckBoxClicked() {
    var main = document.getElementById("mainCheckBox");
    var singleCheckBoxes = document.getElementsByClassName("singleCheckBox");
    for (var i = 0; i < singleCheckBoxes.length; i++) {
        singleCheckBoxes[i].checked = main.checked;
    }
}
function singleCheckBoxClicked(current) {
    var main = document.getElementById("mainCheckBox");
    if (!current.checked) main.checked = false;
    else {
        var singleCheckBoxes = document.getElementsByClassName("singleCheckBox");
        var checked = true;
        for (var i = 0; i < singleCheckBoxes.length; i++) {
            if (!singleCheckBoxes[i].checked) {
                checked = false;
                break;
            }
        }
        main.checked = checked;
    }
}