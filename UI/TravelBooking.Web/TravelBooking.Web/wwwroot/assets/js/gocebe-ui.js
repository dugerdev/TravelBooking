/**
 * Gocebe UI - Common UI interactions and helpers
 */

(function() {
    'use strict';

    // Loading Spinner Management
    window.GocebeUI = window.GocebeUI || {};

    GocebeUI.showLoading = function(message) {
        const spinner = document.getElementById('loadingSpinner');
        if (spinner) {
            const messageEl = spinner.querySelector('p');
            if (messageEl && message) {
                messageEl.textContent = message;
            }
            spinner.style.display = 'flex';
        }
    };

    GocebeUI.hideLoading = function() {
        const spinner = document.getElementById('loadingSpinner');
        if (spinner) {
            spinner.style.display = 'none';
        }
    };

    // Form Validation Helpers
    GocebeUI.validateForm = function(formId) {
        const form = document.getElementById(formId);
        if (!form) return false;

        const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!input.value.trim()) {
                input.classList.add('is-invalid');
                isValid = false;
            } else {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
            }
        });

        return isValid;
    };

    // AJAX Request Helper with Loading
    GocebeUI.ajaxRequest = function(options) {
        const defaults = {
            method: 'GET',
            url: '',
            data: null,
            headers: {},
            onSuccess: function() {},
            onError: function() {},
            showLoading: true,
            loadingMessage: 'Loading...'
        };

        const settings = Object.assign({}, defaults, options);

        if (settings.showLoading) {
            GocebeUI.showLoading(settings.loadingMessage);
        }

        const fetchOptions = {
            method: settings.method,
            headers: {
                'Content-Type': 'application/json',
                ...settings.headers
            }
        };

        if (settings.data && settings.method !== 'GET') {
            fetchOptions.body = JSON.stringify(settings.data);
        }

        fetch(settings.url, fetchOptions)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (settings.showLoading) {
                    GocebeUI.hideLoading();
                }
                settings.onSuccess(data);
            })
            .catch(error => {
                if (settings.showLoading) {
                    GocebeUI.hideLoading();
                }
                settings.onError(error);
                GocebeUI.showNotification('Bir hata olustu: ' + error.message, 'error');
            });
    };

    // Notification System
    GocebeUI.showNotification = function(message, type) {
        type = type || 'info';
        const alertClass = {
            'success': 'alert-success',
            'error': 'alert-danger',
            'warning': 'alert-warning',
            'info': 'alert-info'
        }[type] || 'alert-info';

        const icon = {
            'success': 'fa-check-circle',
            'error': 'fa-exclamation-circle',
            'warning': 'fa-exclamation-triangle',
            'info': 'fa-info-circle'
        }[type] || 'fa-info-circle';

        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 10000; min-width: 300px;">
                <i class="fas ${icon} me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        const alertDiv = document.createElement('div');
        alertDiv.innerHTML = alertHtml;
        document.body.appendChild(alertDiv.firstElementChild);

        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            const alert = document.querySelector('.alert-dismissible');
            if (alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    };

    // Confirm Dialog
    GocebeUI.confirm = function(message, onConfirm, onCancel) {
        if (confirm(message)) {
            if (typeof onConfirm === 'function') {
                onConfirm();
            }
        } else {
            if (typeof onCancel === 'function') {
                onCancel();
            }
        }
    };

    // Format Currency
    GocebeUI.formatCurrency = function(amount, currency) {
        currency = currency || 'TRY';
        const formatter = new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        });
        return formatter.format(amount);
    };

    // Format Date
    GocebeUI.formatDate = function(date, format) {
        format = format || 'dd/MM/yyyy';
        const d = new Date(date);
        const day = String(d.getDate()).padStart(2, '0');
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const year = d.getFullYear();

        return format
            .replace('dd', day)
            .replace('MM', month)
            .replace('yyyy', year);
    };

    // Debounce Function
    GocebeUI.debounce = function(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    };

    // Initialize on DOM Ready
    document.addEventListener('DOMContentLoaded', function() {
        // Auto-submit forms with data-auto-submit attribute
        const autoSubmitForms = document.querySelectorAll('form[data-auto-submit="true"]');
        autoSubmitForms.forEach(form => {
            const inputs = form.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                input.addEventListener('change', function() {
                    form.submit();
                });
            });
        });

        // Add loading spinner to forms with data-loading attribute
        const loadingForms = document.querySelectorAll('form[data-loading="true"]');
        loadingForms.forEach(form => {
            form.addEventListener('submit', function() {
                GocebeUI.showLoading('Processing your request...');
            });
        });

        // Real-time validation
        const validatedInputs = document.querySelectorAll('input[data-validate="true"], select[data-validate="true"], textarea[data-validate="true"]');
        validatedInputs.forEach(input => {
            input.addEventListener('blur', function() {
                if (this.hasAttribute('required') && !this.value.trim()) {
                    this.classList.add('is-invalid');
                    this.classList.remove('is-valid');
                } else {
                    this.classList.remove('is-invalid');
                    this.classList.add('is-valid');
                }
            });
        });

        // Scroll to top button
        const scrollTopBtn = document.createElement('button');
        scrollTopBtn.innerHTML = '<i class="fas fa-arrow-up"></i>';
        scrollTopBtn.className = 'scroll-to-top';
        scrollTopBtn.style.cssText = 'position: fixed; bottom: 30px; right: 30px; z-index: 1000; display: none; width: 50px; height: 50px; border-radius: 50%; background: #4D73FC; color: white; border: none; cursor: pointer; box-shadow: 0 4px 12px rgba(0,0,0,0.15);';
        document.body.appendChild(scrollTopBtn);

        window.addEventListener('scroll', function() {
            if (window.pageYOffset > 300) {
                scrollTopBtn.style.display = 'block';
            } else {
                scrollTopBtn.style.display = 'none';
            }
        });

        scrollTopBtn.addEventListener('click', function() {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    });

})();
