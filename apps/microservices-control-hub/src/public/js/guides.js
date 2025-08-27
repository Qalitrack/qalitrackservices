document.addEventListener('DOMContentLoaded', () => {
    const servicesList = document.getElementById('services-list');
    const searchInput = document.getElementById('searchInput');
    const serviceCount = document.getElementById('serviceCount');
    
    let allServices = [];
    let filteredServices = [];

    // Fetch services with guides
    function fetchServicesWithGuides() {
        fetch('/api/services/with-guides')
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(services => {
                allServices = services;
                filteredServices = services;
                renderServices(services);
                updateServiceCount(services.length);
            })
            .catch(error => {
                console.error('Error fetching service list:', error);
                renderError();
            });
    }

    // Render services to the DOM
    function renderServices(services) {
        if (services.length === 0) {
            renderEmptyState();
            return;
        }

        servicesList.innerHTML = '';
        services.forEach((service, index) => {
            const card = document.createElement('div');
            card.className = 'service-card';
            card.style.animationDelay = `${(index % 5 + 1) * 0.1}s`;

            const link = document.createElement('a');
            link.href = `/guides/${service.key}`;
            link.setAttribute('aria-label', `View documentation for ${service.name}`);

            const title = document.createElement('div');
            title.className = 'service-title';
            title.textContent = service.name;

            const description = document.createElement('div');
            description.className = 'service-description';
            description.textContent = `Documentation for ${service.name} service`;

            link.appendChild(title);
            link.appendChild(description);
            card.appendChild(link);
            servicesList.appendChild(card);
        });
    }

    // Render empty state when no services are found
    function renderEmptyState() {
        servicesList.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">📚</div>
                <h3>No Service Guides Available</h3>
                <p>No services with documentation guides are currently configured.</p>
            </div>
        `;
    }

    // Render error state when API call fails
    function renderError() {
        servicesList.innerHTML = `
            <div class="error-state">
                <div class="error-state-icon">⚠️</div>
                <h3>Unable to Load Service Guides</h3>
                <p>There was an error loading the service guides. Please check your connection and try again.</p>
            </div>
        `;
    }

    // Update service count display
    function updateServiceCount(count) {
        const totalText = count === 1 ? '1 Guide' : `${count} Guides`;
        serviceCount.textContent = totalText;
    }

    // Filter services based on search input
    function filterServices(searchTerm) {
        const filtered = allServices.filter(service =>
            service.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
            service.key.toLowerCase().includes(searchTerm.toLowerCase())
        );
        
        filteredServices = filtered;
        renderServices(filtered);
        updateServiceCount(filtered.length);
    }

    // Debounce function to limit search API calls
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

    // Setup search functionality
    function setupSearch() {
        const debouncedFilter = debounce((searchTerm) => {
            filterServices(searchTerm);
        }, 300);

        searchInput.addEventListener('input', (e) => {
            const searchTerm = e.target.value.trim();
            debouncedFilter(searchTerm);
        });

        // Clear search on escape key
        searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                searchInput.value = '';
                filterServices('');
            }
        });
    }

    // Enhanced keyboard navigation
    function setupKeyboardNavigation() {
        document.addEventListener('keydown', (e) => {
            const cards = document.querySelectorAll('.service-card a');
            const focusedIndex = Array.from(cards).findIndex(card => card === document.activeElement);

            switch (e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    const nextIndex = focusedIndex < cards.length - 1 ? focusedIndex + 1 : 0;
                    cards[nextIndex]?.focus();
                    break;
                case 'ArrowUp':
                    e.preventDefault();
                    const prevIndex = focusedIndex > 0 ? focusedIndex - 1 : cards.length - 1;
                    cards[prevIndex]?.focus();
                    break;
                case '/':
                    // Focus search when '/' is pressed
                    if (document.activeElement !== searchInput) {
                        e.preventDefault();
                        searchInput.focus();
                    }
                    break;
            }
        });
    }

    // Add loading animation removal
    function removeLoadingSpinner() {
        const loadingSpinner = document.querySelector('.loading-spinner');
        if (loadingSpinner) {
            loadingSpinner.style.opacity = '0';
            loadingSpinner.style.transform = 'scale(0.9)';
            setTimeout(() => {
                loadingSpinner.remove();
            }, 300);
        }
    }

    // Enhanced error handling with retry functionality
    function setupRetryFunctionality() {
        document.addEventListener('click', (e) => {
            if (e.target.matches('.retry-button')) {
                e.preventDefault();
                servicesList.innerHTML = `
                    <div class="loading-spinner">
                        <div class="spinner"></div>
                        <p>Retrying...</p>
                    </div>
                `;
                setTimeout(fetchServicesWithGuides, 1000);
            }
        });
    }

    // Initialize the application
    function init() {
        setupSearch();
        setupKeyboardNavigation();
        setupRetryFunctionality();
        fetchServicesWithGuides();
    }

    // Start the application
    init();

    // Add some visual feedback for interactions
    document.addEventListener('click', (e) => {
        if (e.target.closest('.service-card')) {
            const card = e.target.closest('.service-card');
            card.style.transform = 'translateY(-8px) scale(0.98)';
            setTimeout(() => {
                card.style.transform = '';
            }, 150);
        }
    });

    // Handle page visibility changes to refresh data when user comes back to tab
    document.addEventListener('visibilitychange', () => {
        if (!document.hidden && allServices.length === 0) {
            fetchServicesWithGuides();
        }
    });

    // Add smooth scroll behavior for better UX
    if ('scrollBehavior' in document.documentElement.style) {
        document.documentElement.style.scrollBehavior = 'smooth';
    }
});