"use strict";
// Torna a função global para ser chamada pelo onclick do botão
window.confirmDelete = confirmDelete;
window.loadFindings = loadFindings;
window.loadTechnologies = loadTechnologies;
window.showSummary = showSummary;
window.exportPdf = exportPdf;
function getQueryParamNumber(name, defaultValue) {
    try {
        const url = new URL(window.location.href);
        const val = url.searchParams.get(name);
        if (!val)
            return defaultValue;
        const n = parseInt(val, 10);
        return isNaN(n) ? defaultValue : n;
    }
    catch (_a) {
        return defaultValue;
    }
}
function confirmDelete(historyId) {
    const currentPage = getQueryParamNumber('page', 1);
    Swal.fire({
        title: 'Tem certeza?',
        text: 'Essa ação não pode ser desfeita!',
        theme: 'dark',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sim, excluir',
        cancelButtonText: "Cancelar"
    }).then((result) => {
        if (result.isConfirmed) {
            axios.post(`/history/deletehistory?id=${historyId}`, { timeout: 5000 })
                .then((response) => {
                if (response.status < 200 || response.status >= 300) {
                    throw new Error(`Erro HTTP! status: ${response.status}`);
                }
                return response.data;
            })
                .then((data) => {
                if (data.success) {
                    Swal.fire(`Item Deletado!`, data.message, 'success')
                        .then(() => {
                        const rowElement = document.getElementById('row-' + historyId);
                        if (rowElement) {
                            rowElement.remove();
                        }
                        window.location.href = `/history?page=${currentPage}&pageSize=10`;
                    });
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
function loadFindings(historyId) {
    axios.get(`/history/getfindings?id=${historyId}`, { timeout: 5000 })
        .then((response) => {
        const data = response.data;
        const tbody = document.getElementById("findingsTableBody");
        if (!tbody)
            throw new Error("Corpo da tabela não encontrado");
        tbody.innerHTML = ""; // limpa tabela
        if (data.length === 0) {
            const table = tbody.closest('table');
            if (!table)
                throw new Error("Tabela não encontrada");
            const thead = table.querySelector('thead');
            if (thead)
                thead.remove();
            tbody.innerHTML = "<tr><td colspan='4' class='text-center'>Nenhum achado encontrado.</td></tr>";
            // abre o modal se não houver nenhum dado capturado do backend
            var findingsModal = new bootstrap.Modal(document.getElementById('findingsModal'));
            findingsModal.show();
        }
        else {
            data.forEach((finding) => {
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
        .catch((err) => {
        Swal.fire("Erro", err.message, "error");
    });
}
function loadTechnologies(historyId) {
    axios.get(`/history/gettechnologies?id=${historyId}`, { timeout: 5000 })
        .then((response) => {
        const data = response.data;
        const tbody = document.getElementById("technologiesTableBody");
        if (!tbody)
            throw new Error("Corpo da tabela de tecnologias não encontrado");
        tbody.innerHTML = "";
        if (!data || data.length === 0) {
            tbody.innerHTML = "<tr><td colspan='2' class='text-center'>Nenhuma tecnologia encontrada.</td></tr>";
            const table = tbody.closest('table');
            if (!table)
                throw new Error("Tabela não encontrada");
            const thead = table.querySelector('thead');
            if (thead)
                thead.remove();
        }
        else {
            data.forEach((tech) => {
                var _a;
                let row = `<tr>
                                <td>${tech.technology_name}</td>
                                <td>${(_a = tech.version) !== null && _a !== void 0 ? _a : "Não detectada"}</td>
                                <td>${tech.technology_category}</td>
                                <td>${tech.technology_description}</td>
                               </tr>`;
                tbody.innerHTML += row;
            });
        }
        var techModal = new bootstrap.Modal(document.getElementById('technologiesModal'));
        techModal.show();
    })
        .catch((err) => {
        Swal.fire("Erro", err.message, "error");
    });
}
function showSummary(text) {
    Swal.fire({
        title: 'Resumo',
        theme: 'dark',
        html: `<div style="text-align:left">${text}</div>`,
        confirmButtonText: 'Fechar'
    });
}

function exportPdf(userId) {
    const currentPage = getQueryParamNumber('page', 1);
    const pageSize = getQueryParamNumber('pageSize', 10);
    
    if (!userId) {
        Swal.fire('Erro', 'ID do usuário não encontrado.', 'error');
        return;
    }

    // Mostrar mensagem de carregamento
    Swal.fire({
        title: 'Gerando PDF...',
        text: 'Por favor aguarde',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    // Criar payload para enviar ao backend
    const payload = {
        userId: parseInt(userId),
        page: currentPage,
        pageSize: pageSize
    };

    // Fazer requisição POST para exportar PDF
    axios.post('/history/exportpdf', payload, {
        responseType: 'blob',
        timeout: 30000,
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then((response) => {
        // Fechar o loading
        Swal.close();

        // Criar um link temporário para download
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        
        // Extrair nome do arquivo do header ou usar padrão
        const contentDisposition = response.headers['content-disposition'];
        let fileName = `Historico_${new Date().toISOString().slice(0,10)}.pdf`;
        if (contentDisposition) {
            const fileNameMatch = contentDisposition.match(/filename="?(.+)"?/i);
            if (fileNameMatch && fileNameMatch.length === 2) {
                fileName = fileNameMatch[1];
            }
        }
        
        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);

        // Mostrar mensagem de sucesso
        Swal.fire({
            title: 'Sucesso!',
            text: 'PDF exportado com sucesso!',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false
        });
    })
    .catch((err) => {
        console.error('Erro ao exportar PDF:', err);
        let errorMessage = 'Ocorreu um erro ao gerar o PDF.';
        
        if (err.response && err.response.data) {
            // Tentar ler a mensagem de erro do blob
            const reader = new FileReader();
            reader.onload = function() {
                try {
                    const errorData = JSON.parse(reader.result);
                    errorMessage = errorData.message || errorMessage;
                } catch (e) {
                    // Se não for JSON, manter mensagem padrão
                }
                Swal.fire('Erro', errorMessage, 'error');
            };
            reader.readAsText(err.response.data);
        } else {
            Swal.fire('Erro', errorMessage, 'error');
        }
    });
}

//# sourceMappingURL=history.js.map