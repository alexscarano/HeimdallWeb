declare const axios: any;
declare var Swal: any;

function confirmDeleteUser(userId: number) {
    Swal.fire({
        title: 'Tem certeza?',
        text: 'Essa ação não pode ser desfeita!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sim, excluir',
        cancelButtonText: "Cancelar"
    }).then((result: any) => {
        if (result.isConfirmed) {
            axios.post(`/admin/deleteUser?id=${userId}`, { timeout: 5000 })
                .then((response: any) => {
                    const data = response.data;
                    if (data.success) {
                        Swal.fire(`Usuário ${userId} Deletado!`, data.message, 'success');
                        const row = document.getElementById('row-' + userId);
                        if (row) row.remove();
                    } else {
                        Swal.fire('Erro', data.message, 'error');
                    }
                })
                .catch((err: any) => {
                    Swal.fire('Erro', 'Ocorreu um erro ao processar a requisição: ' + err.message, 'error');
                });
        }
    });
}

// Torna a função global para ser chamada pelo onclick do botão
(window as any).confirmDelete = confirmDelete;