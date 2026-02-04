function showPassword() {
    var inputPassword = document.getElementById("password_hide");
    var hide_eye = document.getElementById("hide_eye");
    var show_eye = document.getElementById("show_eye");

    if (inputPassword.type === "password") {
        hide_eye.classList.add("d-none");
        show_eye.classList.remove("d-none");
        inputPassword.type = 'text';
    } else {
        inputPassword.type = 'password'
        hide_eye.classList.remove("d-none");
        show_eye.classList.add("d-none");
    }
}

function showConfirmPassword() {
    var inputConfirmPassword = document.getElementById("confirm_password");
    var confirmHideEye = document.getElementById("confirm_hide_eye");
    var confirmShowEye = document.getElementById("confirm_show_eye");

    if (inputConfirmPassword.type === "password") {
        confirmHideEye.classList.add("d-none");
        confirmShowEye.classList.remove("d-none");
        inputConfirmPassword.type = 'text';
    } else {
        inputConfirmPassword.type = 'password'
        confirmHideEye.classList.remove("d-none");
        confirmShowEye.classList.add("d-none");
    }
}