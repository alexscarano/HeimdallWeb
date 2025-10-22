/// <reference types="jquery" />

$(function () {
    $('.close-alert').on('click', function () {
        $('.alert').hide('slow');
    });
});