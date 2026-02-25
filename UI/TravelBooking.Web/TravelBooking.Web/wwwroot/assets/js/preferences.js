// Currency and Language Selector Handlers
(function() {
    'use strict';

    $(document).ready(function() {
        // Currency selector
        $('.currency-option').on('click', function(e) {
            e.preventDefault();
            const currency = $(this).data('currency');
            
            $.ajax({
                url: '/Preferences/SetCurrency',
                method: 'POST',
                data: { currency: currency },
                success: function(response) {
                    if (response.success) {
                        location.reload();
                    } else {
                        console.error('Currency change failed:', response.message);
                    }
                },
                error: function() {
                    console.error('Error changing currency');
                }
            });
        });

        // Language selector
        $('.language-option').on('click', function(e) {
            e.preventDefault();
            const language = $(this).data('lang');
            
            $.ajax({
                url: '/Preferences/SetLanguage',
                method: 'POST',
                data: { language: language },
                success: function(response) {
                    if (response.success) {
                        location.reload();
                    } else {
                        console.error('Language change failed:', response.message);
                    }
                },
                error: function() {
                    console.error('Error changing language');
                }
            });
        });
    });
})();
