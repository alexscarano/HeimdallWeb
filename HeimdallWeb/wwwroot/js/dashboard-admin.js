"use strict";
function confirmDeleteUser(userId) {
    Swal.fire({
        title: 'Tem certeza?',
        text: 'Essa ação não pode ser desfeita!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sim, excluir',
        cancelButtonText: "Cancelar"
    }).then((result) => {
        if (result.isConfirmed) {
            axios.post(`/admin/deleteUser?id=${userId}`, { timeout: 5000 })
                .then((response) => {
                const data = response.data;
                if (data.success) {
                    Swal.fire(`Usuário ${userId} Deletado!`, data.message, 'success')
                        .then(() => location.reload());
                }
                else {
                    Swal.fire('Erro', data.message, 'error');
                }
            })
                .catch((err) => {
                Swal.fire('Erro', 'Ocorreu um erro ao processar a requisição: ' + err.message, 'error');
            });
        }
    });
}
function toggleUserStatus(userId, isActive) {
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
    }).then((result) => {
        if (result.isConfirmed) {
            axios.post(`/admin/toggleUserStatus?id=${userId}&isActive=${isActive}`, { timeout: 5000 })
                .then((response) => {
                const data = response.data;
                if (data.success) {
                    Swal.fire(`Usuário ${actionPast}!`, data.message, 'success')
                        .then(() => location.reload());
                }
                else {
                    Swal.fire('Erro', data.message, 'error');
                }
            })
                .catch((err) => {
                Swal.fire('Erro', 'Ocorreu um erro ao processar a requisição: ' + err.message, 'error');
            });
        }
    });
}
// Torna as funções globais para serem chamadas pelo onclick dos botões
window.confirmDeleteUser = confirmDeleteUser;
window.toggleUserStatus = toggleUserStatus;
//# sourceMappingURL=dashboard-admin.js.map