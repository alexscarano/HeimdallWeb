let cropper;

document.querySelector("#profileImageInput").addEventListener("change", (e) => {
    let file = e.target.files[0];
    if (!file) return;
    if (file.type !== "image/jpeg" && file.type !== "image/png" && file.type !== "image/webp")
        return;

    let reader = new FileReader();
    reader.onload = (event) => {
        document.querySelector('#imageToCrop').src = event.target.result;

        let bModal = new bootstrap.Modal(document.querySelector('#cropperModal'))
        bModal.show();

        setTimeout(() => {
            if (cropper) cropper.destroy();

            cropper = new Cropper(document.querySelector('#imageToCrop'), {
                aspectRatio: 1,
                viewMode: 2,
                cropBoxResizable: false,
                minCropBoxWidth: 300,
                minCropBoxHeight: 300,
                background: false,
            });
        }, 200);
    };
    reader.readAsDataURL(file);
});

document.querySelector("#cropBtn").addEventListener("click", () => {
    let canvas = cropper.getCroppedCanvas({
        width: 500,
        height: 500,
    });

    canvas.toBlob(function (blob) {
        let fileInput = document.querySelector('#profileImageInput');

        let croppedFile = new File([blob], "profile.jpg", { type: "image/jpeg" });

        let dataTransfer = new DataTransfer();
        dataTransfer.items.add(croppedFile);
        fileInput.files = dataTransfer.files;

        bootstrap.Modal.getInstance(document.querySelector('#cropperModal')).hide();
    });
});