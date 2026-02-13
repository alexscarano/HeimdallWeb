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
                        Swal.fire(`Usuário ${userId} Deletado!`, data.message, 'success')
                            .then(() => location.reload());
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

function toggleUserStatus(userId: number, isActive: boolean) {
    const action = isActive ? 'desbloquear' : 'bloquear';
    const actionPast = isActive ? 'desbloqueado' : 'bloqueado';

    Swal.fire({
        title: `Tem certeza?`,
        text: `Deseja ${action} este usuário?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: `Sim, ${action}`,
        cancelButtonText: "Cancelar",
        confirmButtonColor: isActive ? '#28a745' : '#ffc107'
    }).then((result: any) => {
        if (result.isConfirmed) {
            axios.post(`/admin/toggleUserStatus?id=${userId}&isActive=${isActive}`, { timeout: 5000 })
                .then((response: any) => {
                    const data = response.data;
                    if (data.success) {
                        Swal.fire(`Usuário ${actionPast}!`, data.message, 'success')
                            .then(() => location.reload());
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

// Torna as funções globais para serem chamadas pelo onclick dos botões
(window as any).confirmDeleteUser = confirmDeleteUser;
(window as any).toggleUserStatus = toggleUserStatus;