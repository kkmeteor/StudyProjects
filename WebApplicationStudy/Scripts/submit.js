﻿function validate_required(field, alerttxt) {
    with (field) {
        if (field.value == null || field.value == "") {
            alert(alerttxt);
            return false
        }
        else {
            return true
        }
    }
}

function validate_form(thisform) {
    with (thisform) {
        if (validate_required(email, "Email must be filled out!") == false)
        { email.focus(); return false }
    }
}