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
const dialogEl = document.getElementById('modal-alert-dialog');

function clearModal() {
    titleEl.innerHTML = '';
    bodyEl.innerHTML = '';
    dialogEl?.classList.remove('modal-lg');
    dialogEl?.classList.add('modal-md');
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

function showModalErrorDetails(
    title,
    message,
    iconClass,
    diagnosticMessage,
    serviceStackTrace,
    errorDetails) {
    clearModal();
    dialogEl?.classList.remove('modal-md');
    dialogEl?.classList.add('modal-lg');

    const titleIcon = document.createElement('i');
    titleIcon.className = iconClass;

    const titleText = document.createElement('span');
    titleText.textContent = title;

    titleEl.append(titleIcon, titleText);

    const messageElement = document.createElement('p');
    messageElement.className = 'mb-0 fw-semibold';
    messageElement.textContent = message;
    bodyEl.appendChild(messageElement);

    const hasErrorDetails = errorDetails && Object.keys(errorDetails).length > 0;
    if (!diagnosticMessage && !serviceStackTrace && !hasErrorDetails) {
        modal.show();
        return;
    }

    const detailsElement = document.createElement('details');
    detailsElement.className = 'error-details mt-3';

    const summaryElement = document.createElement('summary');
    summaryElement.className = 'error-details-summary';

    const summaryContent = document.createElement('span');
    summaryContent.className = 'd-flex align-items-center gap-2';

    const summaryIcon = document.createElement('i');
    summaryIcon.className = 'bi bi-code-square';

    const summaryText = document.createElement('span');
    summaryText.textContent = 'Detalles del error';

    summaryContent.append(summaryIcon, summaryText);

    const chevron = document.createElement('i');
    chevron.className = 'bi bi-chevron-down error-details-chevron';

    summaryElement.append(summaryContent, chevron);
    detailsElement.appendChild(summaryElement);

    const contentElement = document.createElement('div');
    contentElement.className = 'error-details-content';

    appendErrorDetail(contentElement, 'Diagnóstico', diagnosticMessage);

    if (hasErrorDetails) {
        Object.entries(errorDetails).forEach(([field, detail]) => {
            appendErrorDetail(
                contentElement,
                field,
                detail?.clientMessage ?? detail?.diagnosticMessage);
        });
    }

    appendErrorDetail(contentElement, 'Pila del error', serviceStackTrace, true);
    detailsElement.appendChild(contentElement);
    bodyEl.appendChild(detailsElement);

    modal.show();
}

function appendErrorDetail(container, label, value, preserveWhitespace = false) {
    if (!value) return;

    const item = document.createElement('div');
    item.className = 'error-details-item';

    const labelElement = document.createElement('span');
    labelElement.className = 'error-details-label';
    labelElement.textContent = label;

    const valueElement = document.createElement(preserveWhitespace ? 'pre' : 'p');
    valueElement.className = preserveWhitespace
        ? 'error-details-stack'
        : 'error-details-value';
    valueElement.textContent = value;

    item.append(labelElement, valueElement);
    container.appendChild(item);
}


//FIN MODAL


// INICIO ARBOL
function toggleNode(element) {
    const parentLi = element.parentElement;
    const children = parentLi.querySelector(":scope > .tree-children");
    const toggleIcon = element.querySelector(".toggle");

    if (!children) return;

    children.classList.toggle("open");
    toggleIcon.classList.toggle("open");
}
// FIN ARBOL

