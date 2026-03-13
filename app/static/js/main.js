// Thealley HR System - Main JavaScript

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        document.querySelectorAll('.alert.fade.show').forEach(function (el) {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(el);
            bsAlert.close();
        });
    }, 5000);

    // Confirm delete forms
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (!confirm(form.dataset.confirm)) {
                e.preventDefault();
            }
        });
    });

    // Format number inputs on blur
    document.querySelectorAll('input[type="number"][data-format="currency"]').forEach(function (el) {
        el.addEventListener('blur', function () {
            if (this.value) {
                this.title = new Intl.NumberFormat('vi-VN').format(this.value) + ' VNĐ';
            }
        });
    });
});
