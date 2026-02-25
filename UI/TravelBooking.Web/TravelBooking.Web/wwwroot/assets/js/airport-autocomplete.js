// Airport Autocomplete for Flight Search Forms
(function() {
    'use strict';

    // Debounce function to limit API calls
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Create autocomplete dropdown
    function createDropdown(input) {
        let dropdown = input.parentElement.querySelector('.airport-autocomplete-dropdown');
        if (!dropdown) {
            dropdown = document.createElement('div');
            dropdown.className = 'airport-autocomplete-dropdown';
            dropdown.style.cssText = `
                position: absolute;
                top: 100%;
                left: 0;
                right: 0;
                background: white;
                border: 1px solid #ddd;
                border-top: none;
                max-height: 300px;
                overflow-y: auto;
                z-index: 1000;
                box-shadow: 0 4px 6px rgba(0,0,0,0.1);
                display: none;
            `;
            input.parentElement.style.position = 'relative';
            input.parentElement.appendChild(dropdown);
        }
        return dropdown;
    }

    // Search airports via API
    async function searchAirports(query) {
        try {
            const response = await fetch(`/Flight/SearchAirports?query=${encodeURIComponent(query)}`);
            const data = await response.json();
            return data.success ? data.data : [];
        } catch (error) {
            console.error('Airport search error:', error);
            return [];
        }
    }

    // Show results in dropdown
    function showResults(input, results) {
        const dropdown = createDropdown(input);
        dropdown.innerHTML = '';

        if (results.length === 0) {
            dropdown.innerHTML = '<div style="padding: 12px; color: #666;">Airport not found</div>';
            dropdown.style.display = 'block';
            return;
        }

        results.forEach(airport => {
            const item = document.createElement('div');
            item.className = 'airport-autocomplete-item';
            item.style.cssText = `
                padding: 12px 16px;
                cursor: pointer;
                border-bottom: 1px solid #f0f0f0;
                transition: background-color 0.2s;
            `;
            
            item.innerHTML = `
                <div style="font-weight: 600; color: #333; margin-bottom: 4px;">
                    ${airport.city}, ${airport.country} (${airport.iata})
                </div>
                <div style="font-size: 13px; color: #666;">
                    ${airport.name}
                </div>
            `;

            item.addEventListener('mouseenter', () => {
                item.style.backgroundColor = '#f8f9fa';
            });

            item.addEventListener('mouseleave', () => {
                item.style.backgroundColor = 'white';
            });

            item.addEventListener('click', () => {
                // Backend'e sadece sehir adini gonder (GetIataCodesByNameOrIataAsync ile parse edilecek)
                input.value = airport.city;
                input.setAttribute('data-iata', airport.iata);
                input.setAttribute('data-city', airport.city);
                input.setAttribute('data-selected', 'true');
                dropdown.style.display = 'none';
            });

            dropdown.appendChild(item);
        });

        dropdown.style.display = 'block';
    }

    // Hide dropdown
    function hideDropdown(input) {
        const dropdown = input.parentElement.querySelector('.airport-autocomplete-dropdown');
        if (dropdown) {
            setTimeout(() => {
                dropdown.style.display = 'none';
            }, 200);
        }
    }

    // Initialize autocomplete on input
    function initAirportAutocomplete(input) {
        const debouncedSearch = debounce(async (query) => {
            if (query.length < 2) {
                hideDropdown(input);
                return;
            }

            const results = await searchAirports(query);
            showResults(input, results);
        }, 300);

        input.addEventListener('input', (e) => {
            debouncedSearch(e.target.value);
        });

        input.addEventListener('focus', (e) => {
            if (e.target.value.length >= 2) {
                debouncedSearch(e.target.value);
            }
        });

        input.addEventListener('blur', () => {
            hideDropdown(input);
        });

        // Clear data attributes when user manually types
        input.addEventListener('keydown', () => {
            input.removeAttribute('data-iata');
            input.removeAttribute('data-city');
        });
    }

    // Initialize all airport inputs on page load
    function initAll() {
        // Flight Listing page
        const flightFrom = document.getElementById('flightFrom');
        const flightTo = document.getElementById('flightTo');
        
        if (flightFrom) initAirportAutocomplete(flightFrom);
        if (flightTo) initAirportAutocomplete(flightTo);

        // Home page / BookingSearches component
        const fromCity = document.getElementById('fromCity');
        const toCity = document.getElementById('toCity');
        
        if (fromCity) initAirportAutocomplete(fromCity);
        if (toCity) initAirportAutocomplete(toCity);

        // Any other inputs with class 'airport-search'
        document.querySelectorAll('.airport-search').forEach(input => {
            initAirportAutocomplete(input);
        });
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAll);
    } else {
        initAll();
    }
})();
