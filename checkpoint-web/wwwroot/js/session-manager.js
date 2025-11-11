// ============================================
// GESTIÓN DE SESIÓN INTELIGENTE
// - Sin "Recuérdame": Cierra sesión al cerrar pestaña/ventana
// - Con "Recuérdame": Mantiene sesión activa (cookie persistente)
// ============================================

(function() {
 'use strict';
  
    // Solo ejecutar si el usuario está autenticado
    const authAttr = document.body.getAttribute('data-authenticated');
    const isAuthenticated = authAttr === 'True' || authAttr === 'true';
    
    if (!isAuthenticated) {
        console.log('[SESSION-MANAGER] Usuario no autenticado - skip');
        return;
    }

    // Verificar si hay cookie persistente (IsPersistent)
    const isPersistent = document.body.getAttribute('data-persistent') === 'True';
    
    if (isPersistent) {
        console.log('[SESSION-MANAGER] Sesión persistente ("Recuérdame" activado) - NO se cerrará automáticamente');
        return; // No hacer nada, la sesión debe persistir
    }

    console.log('[SESSION-MANAGER] Sesión temporal - Se cerrará al cerrar pestaña/navegador');

    // Endpoint específico para sendBeacon (POST sin CSRF)
    const logoutEndpoint = '/api/session/end';

    // ============================================
    // CRÍTICO: beforeunload se dispara en CUALQUIER navegación
    // Esto incluye clicks en enlaces, submit de forms, etc.
    // Por eso lo deshabilitamos completamente
    // ============================================
    
    // Solo usar pagehide que es mucho más confiable
 window.addEventListener('pagehide', function(event) {
        // Si la página va al bfcache (back-forward cache), NO hacer logout
        // Esto pasa cuando usas el botón "atrás" del navegador
        if (event.persisted) {
            console.log('[SESSION-MANAGER] Página a bfcache - NO logout');
            return;
      }
  
        // Si llegamos aquí, es un cierre REAL de pestaña/ventana
        console.log('[SESSION-MANAGER] pagehide - Cerrando sesión (cierre real de pestaña)');
     
        if (navigator.sendBeacon) {
            const sent = navigator.sendBeacon(logoutEndpoint, new Blob(['{}'], { type: 'application/json' }));
            console.log('[SESSION-MANAGER] sendBeacon resultado:', sent);
        } else {
            // Fallback para navegadores muy antiguos
        try {
        const xhr = new XMLHttpRequest();
                xhr.open('POST', logoutEndpoint, false); // síncrono
 xhr.setRequestHeader('Content-Type', 'application/json');
                xhr.send('{}');
         console.log('[SESSION-MANAGER] XHR síncrono completado');
            } catch (err) {
      console.warn('[SESSION-MANAGER] Error en fallback XHR:', err);
     }
      }
    });
    
    // También detectar cuando la página se descarga completamente
    // (cierre de ventana, navegación externa)
    window.addEventListener('unload', function() {
        // unload se ejecuta cuando la página se está DESCARGANDO completamente
        // Esto solo pasa en cierre de ventana o navegación a otro dominio
      console.log('[SESSION-MANAGER] unload - Página descargándose');
        
      // sendBeacon aquí también por seguridad
        if (navigator.sendBeacon) {
         navigator.sendBeacon(logoutEndpoint, new Blob(['{}'], { type: 'application/json' }));
        }
    });

    console.log('[SESSION-MANAGER] Listeners registrados - Sesión se cerrará al cerrar pestaña/navegador');
    console.log('[SESSION-MANAGER] beforeunload DESHABILITADO - No interferirá con navegación interna');
})();
