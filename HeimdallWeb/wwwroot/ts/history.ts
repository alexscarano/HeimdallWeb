// Torna a função global para ser chamada pelo onclick do botão
(window as any).confirmDelete = confirmDelete;
declare var Swal: any;
declare var bootstrap: any;

function confirmDelete(historyId: number) {
    Swal.fire({
        title: 'Tem certeza?',
        text: 'Essa ação não pode ser desfeita!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sim, excluir'
    }).then((result: any) => {
        if (result.isConfirmed) {
            axios.post(`/History/DeleteHistory?id=${historyId}`, { timeout: 5000 })
            .then((response: any) => {
                if (response.status < 200 || response.status >= 300) {
                    throw new Error(`Erro HTTP! status: ${response.status}`);
                }
                return response.data;
            })
            .then((data: any) => {
                if (data.success) {
                    Swal.fire(`Item Deletado!`, data.message, 'success')
                        .then(() => {
                            const rowElement = document.getElementById('row-' + historyId);
                            if (rowElement) {
                                rowElement.remove();
                            }
                            window.location.href = '/History?page=1&pageSize=10';
                        });

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
function loadFindings(historyId: number) {
    axios.get(`/History/GetFindings?id=${historyId}`, { timeout: 5000 })
        .then((response: any) => {

            const data: Finding[] = response.data;

            const tbody = document.getElementById("findingsTableBody");

            if (!tbody)
                throw new Error("Corpo da tabela não encontrado");

            tbody.innerHTML = ""; // limpa tabela

            if (data.length === 0) {
                const table = tbody.closest('table');

                if (!table)
                    throw new Error("Tabela não encontrada");

                const thead = table.querySelector('thead');
                if (thead) thead.remove();

                tbody.innerHTML = "<tr><td colspan='4' class='text-center'>Nenhum achado encontrado.</td></tr>";
                // abre o modal se não houver nenhum dado capturado do backend
                var findingsModal = new bootstrap.Modal(document.getElementById('findingsModal'));
                findingsModal.show();
            }
            else {
                data.forEach((finding: Finding) => {
                    let row = `
                            <tr>
                                <td>${finding.type}</td>
                                <td>${finding.description}</td>
                                <td>${finding.severity_string}</td>
                                <td>${finding.recommendation}</td>
                            </tr>`;
                    tbody.innerHTML += row;
                });
            }

            // abre o modal
            var findingsModal = new bootstrap.Modal(document.getElementById('findingsModal'));
            findingsModal.show();
        })
        .catch((err: any) => {
            Swal.fire("Erro", err.message, "error");
        });
}