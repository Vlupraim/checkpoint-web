// ============================================
// GESTIÓN DE SESIÓN INTELIGENTE
// - Sin "Recuérdame": Cierra sesión al cerrar pestaña/navegador
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

    // Variable para detectar si es un refresh o navegación interna
    let isRefreshing = false;
    
    // Detectar refresh (F5, Ctrl+R)
    window.addEventListener('beforeunload', function(event) {
        // Si el usuario está navegando a otra página dentro de la app, NO cerrar sesión
        // Esto se detecta porque el performance.navigation.type será reload o navigate
   if (performance.navigation.type === performance.navigation.TYPE_RELOAD) {
        console.log('[SESSION-MANAGER] Refresh detectado - NO cerrar sesión');
      isRefreshing = true;
  return;
    }
        
        // Solo cerrar sesión si es un cierre real de pestaña/ventana
  if (!isRefreshing) {
   console.log('[SESSION-MANAGER] beforeunload - Cerrando sesión');
       
      // sendBeacon es la única forma confiable de enviar datos en beforeunload
            if (navigator.sendBeacon) {
         const sent = navigator.sendBeacon(logoutEndpoint, new Blob(['{}'], { type: 'application/json' }));
console.log('[SESSION-MANAGER] sendBeacon resultado:', sent);
            } else {
    // Fallback para navegadores antiguos
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
   }
    });

    // También detectar pagehide (más confiable en móviles)
    window.addEventListener('pagehide', function(event) {
  // Si la página va al bfcache (back-forward cache), NO hacer logout
        if (event.persisted) {
     console.log('[SESSION-MANAGER] Página a bfcache - NO logout');
     return;
        }
  
      // Solo cerrar sesión si NO es un refresh
        if (!isRefreshing) {
        console.log('[SESSION-MANAGER] pagehide - Cerrando sesión');
            if (navigator.sendBeacon) {
      navigator.sendBeacon(logoutEndpoint, new Blob(['{}'], { type: 'application/json' }));
            }
     }
    });

    console.log('[SESSION-MANAGER] Listeners registrados - Sesión se cerrará al cerrar pestaña/navegador');
})();
