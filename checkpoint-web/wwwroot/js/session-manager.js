// ============================================
// GESTIÓN DE SESIÓN: TEMPORALMENTE DESHABILITADO
// Para diagnosticar el problema de cierre de sesión prematura
// ============================================

(function() {
    'use strict';
    
    console.log('[SESSION-MANAGER] DESHABILITADO TEMPORALMENTE - No se cerrará la sesión automáticamente');
    console.log('[SESSION-MANAGER] Usa el botón Logout para cerrar sesión manualmente');
    
    // TODO: Re-habilitar solo beforeunload después de confirmar que funciona sin problemas
    
    /*
    // Solo ejecutar si el usuario está autenticado
    const authAttr = document.body.getAttribute('data-authenticated');
    const isAuthenticated = authAttr === 'True' || authAttr === 'true';
    
    if (!isAuthenticated) {
        console.log('[SESSION-MANAGER] Usuario no autenticado - skip');
        return;
    }

    console.log('[SESSION-MANAGER] Iniciado - detectará cierre de pestaña');

    // Endpoint específico para sendBeacon (POST sin CSRF)
    const logoutEndpoint = '/api/session/end';

    // Detectar cuando el usuario cierra la pestaña/ventana
    window.addEventListener('beforeunload', function(event) {
      console.log('[SESSION-MANAGER] beforeunload - enviando logout');
        
      // sendBeacon es la única forma confiable de enviar datos en beforeunload
if (navigator.sendBeacon) {
         // sendBeacon solo acepta POST y envía los datos incluso si la página se cierra
            const sent = navigator.sendBeacon(logoutEndpoint, new Blob(['{}'], { type: 'application/json' }));
        console.log('[SESSION-MANAGER] sendBeacon resultado:', sent);
        } else {
        // Fallback muy limitado para navegadores antiguos
      try {
        const xhr = new XMLHttpRequest();
       xhr.open('POST', logoutEndpoint, false); // false = síncrono (bloqueante pero funciona)
    xhr.setRequestHeader('Content-Type', 'application/json');
 xhr.send('{}');
        console.log('[SESSION-MANAGER] XHR síncrono completado');
      } catch (err) {
 console.warn('[SESSION-MANAGER] Error en fallback XHR:', err);
            }
        }
    });

    // También detectar pagehide (más confiable en móviles)
window.addEventListener('pagehide', function(event) {
      // Si la página va al bfcache, no hacer logout
        if (event.persisted) {
         console.log('[SESSION-MANAGER] Página a bfcache - NO logout');
         return;
        }
  
        console.log('[SESSION-MANAGER] pagehide - enviando logout');
        if (navigator.sendBeacon) {
      navigator.sendBeacon(logoutEndpoint, new Blob(['{}'], { type: 'application/json' }));
        }
    });
    */
})();
