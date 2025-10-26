"use strict";
$(document).on('click', '#btn-delete', function () {
    // Verificação do checkbox
    if (!$('#confirm_delete').is(':checked')) {
        Swal.fire('Ops!', 'Você precisa confirmar que deseja excluir a conta.', 'error');
        return;
    }
    // Exibe o SweetAlert
    Swal.fire({
        title: 'Tem certeza que deseja excluir?',
        text: "Essa ação não poderá ser desfeita!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#2a9df4',
        cancelButtonColor: '#ff0157',
        confirmButtonText: 'Sim, excluir!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            // Se o usuário confirmar, envia o formulário
            $('#deleteForm').submit();
        }
    });
});
//# sourceMappingURL=delete.js.map