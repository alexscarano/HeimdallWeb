const btn = document.querySelector("#analyze");
const form = document.querySelector("#scanForm");
const domainInput = document.querySelector("#domainInput");
var analyzing = false;

btn.addEventListener("click", (e) => {
    // se o input não existir ou estiver vazio, mostrar toast e impedir ação
    const value = domainInput ? domainInput.value : null;
    if (!value || value.trim() === "") {
        e.preventDefault();
        try {
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'warning',
                title: 'Informe a URL antes de analisar',
                showConfirmButton: false,
                timer: 2500
            });
        } catch (err) {
            // fallback simples se Swal não estiver disponível
            alert('Informe a URL antes de analisar');
        }
        return;
    }

    if (analyzing) return;
    analyzing = !analyzing;

    const stages = btn.querySelectorAll("span");

    form.classList.add("aria-disabled");
    
    stages.forEach( (s) => {
        s.classList.toggle("d-none")
    });
});