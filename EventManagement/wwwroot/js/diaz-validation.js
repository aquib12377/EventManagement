function validateInput(inputField) {
    inputField = $(inputField);
    const inputVal = inputField.val();
    const validationType = inputField.attr("validation");
    const errorMsg = inputField.attr("errorMsg") || "This field is required";

    // Check if input is empty and validation is not optional
    if ((!inputVal || inputVal.trim() === "") && inputField.attr("optional") !== "true") {
        inputField.addClass("is-invalid");
        addErrorMessage(inputField, errorMsg);
        return false;
    }

    // Check if input is not empty and the input value does not match the validation type
    if (inputVal && validationType) {
        const validationRegex = getValidationRegex(validationType);
        if (!validationRegex.test(inputVal)) {
            inputField.addClass("is-invalid");
            inputField.removeClass("is-valid");
            addErrorMessage(inputField, inputField.attr("errorMsg") || getDefaultErrorMsg(validationType));
            return false;
        }
    }
    removeErrorMessage(inputField);
    return true;
}

function addErrorMessage(inputField, errorMsg) {
    inputField.siblings(".error-msg").remove();
    inputField.after(`<label class="error-msg mb-0">${errorMsg}</label>`);
}

function removeErrorMessage(inputField) {
    inputField.siblings(".error-msg").remove();
    inputField.removeClass("is-invalid");
    inputField.addClass("is-valid");
}

function getValidationRegex(validationType) {
    switch (validationType) {
        case "alpha":
            return /^[a-zA-Z ]+$/;
        case "alphanum":
            return /^[a-zA-Z0-9 ,\-!@#$%^&*]+$/;
        case "date":
            return /^\d{4}-\d{2}-\d{2}$/;
        case "datetime":
            return /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/;
        case "email":
            return /^([a-zA-Z0-9._%+-]+)@([a-zA-Z0-9.-]+\.[a-zA-Z]{2,63})$/;
        case "password":
            return /^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#$%^&*])[a-zA-Z\d!@#$%^&*]{8,}$/;
        case "mobile":
            return /^[0-9]{10}$/;
        case "url":
            return /^((http|https):\/\/)?([a-zA-Z0-9]+\.)+[a-zA-Z]{2,}(\/[a-zA-Z0-9#?=&\/-]+)*$/
        default:
            return /^.*$/;
    }
}
function getDefaultErrorMsg(validationType) {
    switch (validationType) {
        case "alpha":
            return "Only alphabets allowed.";
        case "alphanum":
            return "Only alphabets and numbers allowed.";
        case "email":
            return "Please enter valid email id.";
        case "password":
            return "Password must be at least 8 characters and contain at least one letter, one number, and one special character";
        case "mobile":
            return "Please enter valid mobile no.";
        case "url":
            return "Please enter valid URL"
        default:
            return "Incorrect format";
    }
}

function validateForm(form) {
    let isValid = true;
    $(form)
        .find("[validation]")
        .each(function () {
            isValid = validateInput($(this)) && isValid;
        });
    return isValid;
}


window.addEventListener('load', function () {

    document.querySelectorAll('[validation]').forEach(el => {
        let hasChanged = false;
        el.addEventListener('change', function () {
            validateInput(this);
            hasChanged = true;
        });
        el.addEventListener('keyup', function () {
            if (hasChanged) validateInput(this);
        });
    });

});