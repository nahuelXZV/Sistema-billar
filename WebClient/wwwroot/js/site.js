//INICIO PRELOADER
(function () {
    const preloaderId = 'preloader';

    function getPreloader() {
        return document.getElementById(preloaderId);
    }

    // Disponible para uso manual
    window.showPreloader = function () {
        const el = getPreloader();
        if (!el) return;

        el.classList.remove('d-none');
        el.style.opacity = '1';
    };

    // Disponible para uso manual
    window.hidePreloader = function (delay = 300) {
        return new Promise(resolve => {
            const el = getPreloader();
            if (!el) return resolve();

            setTimeout(() => {
                el.style.opacity = '0';
                setTimeout(() => {
                    el.classList.add('d-none');
                    resolve();
                }, 300);
            }, delay);
        });
    };

    // 🔥 CLAVE: ocultar automáticamente cuando el DOM está listo
    document.addEventListener('DOMContentLoaded', () => {
        hidePreloader();
    });
})();
//FIN PRELOADER

//INICIO MODAL
window.showModal = async function ({
    title = 'Mensaje',
    content = '',
    buttonText = 'Cerrar',
    type = 'danger' // danger | warning | success | info
}) {
    // Espera a que el preloader termine
    if (typeof hidePreloader === 'function') {
        await hidePreloader(0);
    }

    const modalEl = document.getElementById('modal-error');
    if (!modalEl) return;

    // Elementos
    const titleEl = document.getElementById('modalTitle');
    const bodyEl = document.getElementById('modalBody');
    const buttonEl = document.getElementById('modalButton');

    // Iconos por tipo
    const icons = {
        danger: 'bi-exclamation-triangle-fill text-danger',
        warning: 'bi-exclamation-circle-fill text-warning',
        success: 'bi-check-circle-fill text-success',
        info: 'bi-info-circle-fill text-primary'
    };

    // Título (solo texto)
    titleEl.innerHTML = `
		<i class="bi ${icons[type] ?? icons.danger} me-2"></i>
		${title}
	`;

    // Contenido (HTML permitido)
    bodyEl.innerHTML = content;

    // Botón
    buttonEl.textContent = buttonText;
    buttonEl.className = `btn btn-${type === 'info' ? 'primary' : type}`;

    // Mostrar modal
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    modal.show();
};

const modalEl = document.getElementById('modal-alert');
const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

const titleEl = document.getElementById('modal-title');
const bodyEl = document.getElementById('modal-content');

function clearModal() {
    titleEl.innerHTML = '';
    bodyEl.innerHTML = '';
}

function showModalAlert(title, message, iconClass) {
    clearModal();

    titleEl.innerHTML = `
			<i class="${iconClass}"></i>
			<span>${title}</span>
		`;

    bodyEl.innerHTML = `
			<div class="d-flex align-items-start gap-2">
				<div>${message}</div>
			</div>
		`;

    modal.show();
}


//FIN MODAL