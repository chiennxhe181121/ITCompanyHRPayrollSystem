// Xử lý modal quên mật khẩu + gọi API backend, không reload trang
document.addEventListener('DOMContentLoaded', function () {
    const forgotPasswordBtn = document.getElementById('forgotPasswordBtn');
    const forgotPasswordModal = document.getElementById('forgotPasswordModal');
    const closeForgotModal = document.getElementById('closeForgotModal');
    const closeSuccessBtn = document.getElementById('closeSuccessBtn');

    const forgotPasswordFormDiv = document.getElementById('forgotPasswordForm');
    const resetPasswordForm = document.getElementById('resetPasswordForm');
    const resetEmailInput = document.getElementById('resetEmail');
    const resetErrorMessage = document.getElementById('resetErrorMessage');
    const resetErrorText = document.getElementById('resetErrorText');

    const otpStep = document.getElementById('otpStep');
    const resetOtpInput = document.getElementById('resetOtp');
    const otpErrorMessage = document.getElementById('otpErrorMessage');
    const otpErrorText = document.getElementById('otpErrorText');
    const verifyOtpBtn = document.getElementById('verifyOtpBtn');

    const newPasswordStep = document.getElementById('newPasswordStep');
    const newPasswordInput = document.getElementById('newPassword');
    const confirmNewPasswordInput = document.getElementById('confirmNewPassword');
    const newPasswordErrorMessage = document.getElementById('newPasswordErrorMessage');
    const newPasswordErrorText = document.getElementById('newPasswordErrorText');
    const setNewPasswordBtn = document.getElementById('setNewPasswordBtn');

    const resetSuccessMessage = document.getElementById('resetSuccessMessage');

    if (!forgotPasswordBtn || !forgotPasswordModal) return;

    function openModal() {
        forgotPasswordModal.classList.remove('hidden');
        // reset về bước 1
        forgotPasswordFormDiv.classList.remove('hidden');
        otpStep.classList.add('hidden');
        newPasswordStep.classList.add('hidden');
        resetSuccessMessage.classList.add('hidden');

        resetEmailInput.value = '';
        resetOtpInput.value = '';
        newPasswordInput.value = '';
        confirmNewPasswordInput.value = '';

        resetErrorMessage.classList.add('hidden');
        otpErrorMessage.classList.add('hidden');
        newPasswordErrorMessage.classList.add('hidden');
    }

    function closeModal() {
        forgotPasswordModal.classList.add('hidden');
    }

    forgotPasswordBtn.addEventListener('click', openModal);
    closeForgotModal.addEventListener('click', closeModal);
    if (closeSuccessBtn) {
        closeSuccessBtn.addEventListener('click', closeModal);
    }

    // Không còn đóng khi click ra ngoài,
    // chỉ đóng khi bấm nút X hoặc nút "Đóng" sau khi thành công.

    // Gửi email để nhận OTP (POST /HumanResourcesManager/forgot-password)
    resetPasswordForm.addEventListener('submit', async function (e) {
        e.preventDefault();
        resetErrorMessage.classList.add('hidden');

        const email = resetEmailInput.value.trim();
        if (!email) {
            resetErrorText.textContent = 'Vui lòng nhập email.';
            resetErrorMessage.classList.remove('hidden');
            return;
        }

        try {
            const body = new URLSearchParams();
            body.append('Email', email);

            const res = await fetch('/HumanResourcesManager/forgot-password', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: body.toString()
            });

            const data = await res.json();
            if (!data.success) {
                resetErrorText.textContent = data.error || 'Không gửi được mã OTP. Vui lòng thử lại.';
                resetErrorMessage.classList.remove('hidden');
                return;
            }

            // Thành công: chuyển sang bước nhập OTP
            forgotPasswordFormDiv.classList.add('hidden');
            otpStep.classList.remove('hidden');
            resetOtpInput.focus();
        } catch (err) {
            resetErrorText.textContent = 'Có lỗi kết nối. Vui lòng thử lại.';
            resetErrorMessage.classList.remove('hidden');
        }
    });

    // Xác nhận OTP (POST /HumanResourcesManager/verify-otp)
    verifyOtpBtn.addEventListener('click', async function () {
        otpErrorMessage.classList.add('hidden');

        const otp = resetOtpInput.value.trim();
        if (!/^[0-9]{6}$/.test(otp)) {
            otpErrorText.textContent = 'Mã OTP phải gồm 6 chữ số.';
            otpErrorMessage.classList.remove('hidden');
            return;
        }

        try {
            const body = new URLSearchParams();
            body.append('Otp', otp);

            const res = await fetch('/HumanResourcesManager/verify-otp', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: body.toString()
            });

            const data = await res.json();
            if (!data.success) {
                otpErrorText.textContent = data.error || 'Mã OTP không chính xác hoặc đã hết hạn.';
                otpErrorMessage.classList.remove('hidden');
                return;
            }

            // Thành công: sang bước đặt mật khẩu mới
            otpStep.classList.add('hidden');
            newPasswordStep.classList.remove('hidden');
            newPasswordInput.focus();
        } catch (err) {
            otpErrorText.textContent = 'Có lỗi kết nối. Vui lòng thử lại.';
            otpErrorMessage.classList.remove('hidden');
        }
    });

    // Đặt mật khẩu mới (POST /HumanResourcesManager/reset-password)
    setNewPasswordBtn.addEventListener('click', async function () {
        newPasswordErrorMessage.classList.add('hidden');

        const newPassword = newPasswordInput.value.trim();
        const confirmPassword = confirmNewPasswordInput.value.trim();

        if (!newPassword || !confirmPassword) {
            newPasswordErrorText.textContent = 'Vui lòng nhập đầy đủ mật khẩu mới và xác nhận.';
            newPasswordErrorMessage.classList.remove('hidden');
            return;
        }

        if (newPassword.length < 4) {
            newPasswordErrorText.textContent = 'Mật khẩu mới phải có ít nhất 4 ký tự.';
            newPasswordErrorMessage.classList.remove('hidden');
            return;
        }

        if (newPassword !== confirmPassword) {
            newPasswordErrorText.textContent = 'Mật khẩu xác nhận không khớp.';
            newPasswordErrorMessage.classList.remove('hidden');
            return;
        }

        try {
            const body = new URLSearchParams();
            body.append('NewPassword', newPassword);
            body.append('ConfirmPassword', confirmPassword);

            const res = await fetch('/HumanResourcesManager/reset-password', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: body.toString()
            });

            const data = await res.json();
            if (!data.success) {
                newPasswordErrorText.textContent = data.error || 'Không thể đặt lại mật khẩu. Vui lòng thử lại.';
                newPasswordErrorMessage.classList.remove('hidden');
                return;
            }

            // Thành công
            newPasswordStep.classList.add('hidden');
            resetSuccessMessage.classList.remove('hidden');
        } catch (err) {
            newPasswordErrorText.textContent = 'Có lỗi kết nối. Vui lòng thử lại.';
            newPasswordErrorMessage.classList.remove('hidden');
        }
    });
});

