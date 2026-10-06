document.addEventListener("DOMContentLoaded", () => {
    const btnMenu = document.getElementById("btnMenuMovil");
    const menuPrincipal = document.getElementById("menuPrincipal");

    // 1. FUNCIONALIDAD DEL MENÚ MÓVIL (Hamburguesa)
    if (btnMenu && menuPrincipal) {
        btnMenu.addEventListener("click", () => {
            menuPrincipal.classList.toggle("mostrar");
            
            // Cambia el ícono entre ☰ (hamburguesa) y ✖ (cerrar)
            if (menuPrincipal.classList.contains("mostrar")) {
                btnMenu.textContent = "✖";
            } else {
                btnMenu.textContent = "☰";
            }
        });
    }

    // 2. FUNCIONALIDAD DE DESPLAZAMIENTO SUAVE (Smooth Scroll)
    // Selecciona todos los enlaces que empiezan con "#"
    const enlacesInternos = document.querySelectorAll('a[href^="#"]');
    
    enlacesInternos.forEach(enlace => {
        enlace.addEventListener('click', function (e) {
            // Evitamos el salto brusco por defecto de HTML
            e.preventDefault(); 
            
            const destinoId = this.getAttribute('href');
            
            // Si el enlace es solo un "#", lo ignoramos
            if (destinoId === '#') return;

            const elementoDestino = document.querySelector(destinoId);
            
            if (elementoDestino) {
                // Si el menú móvil está abierto, lo cerramos automáticamente al hacer clic en una opción
                if (menuPrincipal && menuPrincipal.classList.contains("mostrar")) {
                    menuPrincipal.classList.remove("mostrar");
                    if (btnMenu) btnMenu.textContent = "☰";
                }

                // Hacemos el deslizamiento suave hacia la sección elegida
                elementoDestino.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // 3. ANIMACIÓN DE ENTRADA AL HACER SCROLL (Fade-in)
    const elementosAnimados = document.querySelectorAll('.animacion-entrada');
    if (elementosAnimados.length > 0 && 'IntersectionObserver' in window) {
        const observador = new IntersectionObserver((entradas) => {
            entradas.forEach((entrada) => {
                if (entrada.isIntersecting) {
                    entrada.target.classList.add('visible');
                    observador.unobserve(entrada.target);
                }
            });
        }, { threshold: 0.15 });

        elementosAnimados.forEach((el) => observador.observe(el));
    } else {
        // Navegadores antiguos: se muestran directamente
        elementosAnimados.forEach((el) => el.classList.add('visible'));
    }
});