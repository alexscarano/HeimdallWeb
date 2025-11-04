/**
 * Toast Alerts usando SweetAlert2 com as cores do tema Heimdall
 * Paleta de cores baseada em history-dark.scss:
 * - heimdall-accent: #2a6b6d (verde/teal)
 * - heimdall-danger: #c94b56 (vermelho)
 * - heimdall-primary: #2f6f8f (azul)
 * - heimdall-dark: #1c3033
 */

declare var Swal: any;

interface ToastOptions {
    message: string;
    duration?: number;
}

// IIFE para evitar poluir o escopo global
(function() {

const HeimdallToast = {
    /**
     * Exibe um toast de sucesso
     */
    success(options: ToastOptions): void {
        Swal.fire({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: options.duration || 3000,
            timerProgressBar: true,
            background: 'linear-gradient(135deg, rgba(42, 107, 109, 0.95), rgba(34, 87, 89, 0.95))',
            color: '#ffffff',
            icon: 'success',
            title: options.message,
            iconColor: '#ffffff',
            customClass: {
                popup: 'heimdall-toast-popup',
                title: 'heimdall-toast-title',
                icon: 'heimdall-toast-icon'
            },
            didOpen: (toast: any) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });
    },

    /**
     * Exibe um toast de erro
     */
    error(options: ToastOptions): void {
        Swal.fire({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: options.duration || 4000,
            timerProgressBar: true,
            background: 'linear-gradient(135deg, rgba(201, 75, 86, 0.95), rgba(181, 55, 66, 0.95))',
            color: '#ffffff',
            icon: 'error',
            title: options.message,
            iconColor: '#ffffff',
            customClass: {
                popup: 'heimdall-toast-popup',
                title: 'heimdall-toast-title',
                icon: 'heimdall-toast-icon'
            },
            didOpen: (toast: any) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });
    },

    /**
     * Exibe um toast de aviso/warning
     */
    warning(options: ToastOptions): void {
        Swal.fire({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: options.duration || 3500,
            timerProgressBar: true,
            background: 'linear-gradient(135deg, rgba(255, 193, 7, 0.95), rgba(235, 173, 0, 0.95))',
            color: '#1c3033',
            icon: 'warning',
            title: options.message,
            iconColor: '#1c3033',
            customClass: {
                popup: 'heimdall-toast-popup',
                title: 'heimdall-toast-title',
                icon: 'heimdall-toast-icon'
            },
            didOpen: (toast: any) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });
    },

    /**
     * Exibe um toast de informação
     */
    info(options: ToastOptions): void {
        Swal.fire({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: options.duration || 3000,
            timerProgressBar: true,
            background: 'linear-gradient(135deg, rgba(47, 111, 143, 0.95), rgba(37, 91, 123, 0.95))',
            color: '#ffffff',
            icon: 'info',
            title: options.message,
            iconColor: '#ffffff',
            customClass: {
                popup: 'heimdall-toast-popup',
                title: 'heimdall-toast-title',
                icon: 'heimdall-toast-icon'
            },
            didOpen: (toast: any) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });
    }
};

// Expõe globalmente para uso nas views
(window as any).HeimdallToast = HeimdallToast;

// Função helper para processar TempData automaticamente
function checkAndShowTempDataAlerts(): void {
    // Helper: remove todos os elementos tempdata após processar
    function removeTempDataElements() {
        const nodes = document.querySelectorAll('.tempdata-container');
        nodes.forEach(n => n.remove());
    }

    // Helper: remove um parâmetro da query string sem recarregar a página
    function removeQueryParam(param: string) {
        try {
            const url = new URL(window.location.href);
            if (url.searchParams.has(param)) {
                url.searchParams.delete(param);
                const newUrl = url.pathname + url.search + url.hash;
                window.history.replaceState(null, document.title, newUrl);
            }
        } catch (e) {
            // Se URL não for suportada (muito raro), ignoramos silenciosamente
            console.warn('Não foi possível limpar o parâmetro da URL:', e);
        }
    }

    // Verifica se existe mensagem de sucesso
    const okMsgElement = document.querySelector('[data-tempdata-ok]');
    if (okMsgElement) {
        const message = okMsgElement.getAttribute('data-tempdata-ok');
        if (message) {
            HeimdallToast.success({ message: message });
        }
    }

    // Verifica se existe mensagem de erro
    const errorMsgElement = document.querySelector('[data-tempdata-error]');
    if (errorMsgElement) {
        const message = errorMsgElement.getAttribute('data-tempdata-error');
        if (message) {
            HeimdallToast.error({ message: message });
        }
    }

    // Limpeza: remove os elementos tempdata e o parâmetro rateLimited da URL
    // (remove também outros parâmetros opcionais se necessário)
    removeTempDataElements();
    removeQueryParam('rateLimited');
}

// Auto-executa quando o DOM estiver pronto
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', checkAndShowTempDataAlerts);
} else {
    checkAndShowTempDataAlerts();
}

})(); // Fecha a IIFE
