// ============================================
// GESTIÓN DE SESIÓN: Logout al cerrar pestaña
// ============================================

(function() {
    'use strict';
    
    // Solo ejecutar si el usuario está autenticado
    const isAuthenticated = document.body.classList.contains('authenticated') || 
  document.querySelector('[data-authenticated="true"]');
    
    if (!isAuthenticated) {
     return;
    }

    console.log('[SESSION-MANAGER] Iniciado - detectará cierre de pestaña');

    // Detectar cuando el usuario cierra la pestaña/ventana
    window.addEventListener('beforeunload', function(event) {
        // Hacer logout asíncrono (best-effort)
        // Usar sendBeacon para asegurar que la petición se envíe aunque la página se cierre
        const logoutUrl = '/Account/Logout';
        
        // sendBeacon es más confiable que fetch en beforeunload
        if (navigator.sendBeacon) {
    // Enviar petición de logout
     navigator.sendBeacon(logoutUrl);
        console.log('[SESSION-MANAGER] Logout enviado (sendBeacon)');
   } else {
            // Fallback para navegadores viejos
    fetch(logoutUrl, {
    method: 'GET',
keepalive: true,
        credentials: 'same-origin'
    }).catch(err => {
       console.warn('[SESSION-MANAGER] Error en logout:', err);
            });
        }
 });

    // También detectar cuando el usuario navega fuera del sitio
    window.addEventListener('pagehide', function(event) {
   if (event.persisted) {
       // Página va a bfcache, no hacer logout
            return;
  }
        
        // Página se descarga completamente, hacer logout
        if (navigator.sendBeacon) {
            navigator.sendBeacon('/Account/Logout');
            console.log('[SESSION-MANAGER] Logout enviado (pagehide)');
  }
    });

    console.log('[SESSION-MANAGER] Listeners registrados correctamente');
})();
