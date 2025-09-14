// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
$('.close-alert').click(function () {
    $('.alert').hide('hide');
    
});

//$(function () {
//    $("#scanForm").on("submit", function (e) {
//        e.preventDefault();

//        let domain = $("#domainInput").val();
//        if (!domain) return;

//        $("#loading").removeClass("d-none");
//        $("#result").addClass("d-none");

//        // colocar endpoint 
//        $.post("", { domain: domain })
//            .done(function (data) {
//                $("#resultContent").text(JSON.stringify(data, null, 2));
//                $("#loading").addClass("d-none");
//                $("#result").removeClass("d-none");
//            })
//            .fail(function () {
//                $("#resultContent").text("❌ Erro ao processar a análise");
//                $("#loading").addClass("d-none");
//                $("#result").removeClass("d-none");
//            });
//    });
//});