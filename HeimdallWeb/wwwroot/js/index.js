document.addEventListener('MostrarSenha', function() {
  const passwordInput = document.getElementById('passwordInput');
  const togglePassword = document.getElementById('togglePassword');
  const showEye = document.getElementById('show_eye');
  const hideEye = document.getElementById('hide_eye');

  togglePassword.addEventListener('click', function() {
    if (passwordInput.type === 'password') {
      passwordInput.type = 'text';
      showEye.classList.add('d-none');
      hideEye.classList.remove('d-none');
    } else {
      passwordInput.type = 'password';
      showEye.classList.remove('d-none');
      hideEye.classList.add('d-none');
    }
  });
});

