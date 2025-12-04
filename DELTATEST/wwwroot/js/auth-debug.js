// Debug script para monitorear localStorage y autenticación
window.authDebug = {
    // Verificar estado de autenticación en localStorage
    checkAuth: function() {
        console.log('=== AUTH DEBUG ===');
        const isAuth = localStorage.getItem('isAuthenticated');
        const userName = localStorage.getItem('userName');
        const userRole = localStorage.getItem('userRole');
        const userId = localStorage.getItem('userId');
        
        console.log('isAuthenticated:', isAuth);
        console.log('userName:', userName);
        console.log('userRole:', userRole);
        console.log('userId:', userId);
        console.log('==================');
        
        return {
            isAuthenticated: isAuth === 'true',
            userName: userName,
            userRole: userRole,
            userId: userId
        };
    },
    
    // Simular login guardando datos en localStorage
    simulateLogin: function(userName, userRole, userId) {
        console.log('Simulating login for:', userName);
        localStorage.setItem('isAuthenticated', 'true');
        localStorage.setItem('userName', userName);
        localStorage.setItem('userRole', userRole);
        localStorage.setItem('userId', userId || '1');
        console.log('Login simulated. Current auth state:');
        this.checkAuth();
    },
    
    // Limpiar localStorage
    clearAuth: function() {
        console.log('Clearing auth data...');
        localStorage.removeItem('isAuthenticated');
        localStorage.removeItem('userName');
        localStorage.removeItem('userRole');
        localStorage.removeItem('userId');
        console.log('Auth data cleared. Current state:');
        this.checkAuth();
    },
    
    // Monitor cambios en localStorage
    monitorStorage: function() {
        console.log('Starting localStorage monitoring...');
        window.addEventListener('storage', (event) => {
            console.log('Storage changed:', event.key, '=', event.newValue);
            this.checkAuth();
        });
    }
};

// Auto-ejecutar monitoreo al cargar
console.log('[auth-debug.js] Loaded');
window.authDebug.checkAuth();
