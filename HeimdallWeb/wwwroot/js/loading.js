const btn = document.querySelector("#analyze");
const form = document.querySelector("#scanForm");
let analyzing = false;

btn.addEventListener("click", () => {
    if (analyzing) return;
    analyzing = !analyzing;
    
    const stages = btn.querySelectorAll("span");

    form.classList.add("aria-disabled");
    
    stages.forEach( (s) => {
        s.classList.toggle("d-none")
    });
})