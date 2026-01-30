function showConfirmModal(message = '¿Está seguro de continuar?') {
    return new Promise(resolve => {
        const modalEl = document.getElementById('modal-confirm');
        const bodyEl = modalEl.querySelector('#modalBody');
        const confirmBtn = document.getElementById('btn-modal-confirmar');
        const cancelBtn = document.getElementById('btn-modal-cancelar');

        bodyEl.innerHTML = message;

        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

        const cleanup = () => {
            confirmBtn.removeEventListener('click', onConfirm);
            cancelBtn.removeEventListener('click', onCancel);
            modal.hide();
        };

        const onConfirm = () => {
            cleanup();
            resolve(true);
        };

        const onCancel = () => {
            cleanup();
            resolve(false);
        };

        confirmBtn.addEventListener('click', onConfirm);
        cancelBtn.addEventListener('click', onCancel);
        modal.show();
        modalEl.click();
    });
}
function handleMenuButtonClick(button) {
    const formId = button.dataset.formId;
    const requireConfirm = button.dataset.confirm === 'true';
    const message = button.dataset.message || '¿Está seguro de continuar?';

    if (!requireConfirm) {
        document.getElementById(formId)?.submit();
        return;
    }

    showConfirmModal(message).then(result => {
        if (result) {
            document.getElementById(formId)?.submit();
        }
    });
}

function generateMenuHtml(items, id) {
    return `
        <ul class="list-group list-group-flush">
            ${items.map((item, index) => {
        const formId = `menuForm_${index}_${id}`;

        if (item.actionType === 1) {
            return `
                        <li class="list-group-item p-0">
                            <form id="${formId}" action="${item.url}" method="post" class="d-none">
                                <input type="hidden" name="id" value="${id}">
                            </form>

                            <button type="button"
                                    class="list-group-item list-group-item-action"
                                    data-form-id="${formId}"
                                    data-confirm="${item.confirm ? 'true' : 'false'}"
                                    data-message="${item.confirmMessage ?? ''}"
                                    onclick="handleMenuButtonClick(this)">
                                ${item.iconCss ? `<i class="${item.iconCss} me-2"></i>` : ''}
                                ${item.text}
                            </button>
                        </li>
                    `;
        }

        return `
                    <li class="list-group-item p-0">
                        <a href="${item.url}?id=${id}"
                           class="list-group-item list-group-item-action">
                            ${item.iconCss ? `<i class="${item.iconCss} me-2"></i>` : ''}
                            ${item.text}
                        </a>
                    </li>
                `;
    }).join('')}
        </ul>
    `;
}
function togglePortalDropdown(event, id) {
    event.stopPropagation();

    const button = event.currentTarget;
    const rect = button.getBoundingClientRect();

    let dropdown = document.getElementById('portal-dropdown');

    if (!dropdown) {
        dropdown = document.createElement('div');
        dropdown.id = 'portal-dropdown';
        dropdown.className = 'dropdown-menu show';
        dropdown.style.position = 'absolute';
        document.body.appendChild(dropdown);
    }

    dropdown.innerHTML = generateMenuHtml(menuItems, id);

    const isVisible = dropdown.style.display === 'block';

    dropdown.style.display = isVisible ? 'none' : 'block';
    dropdown.style.top = `${rect.bottom + window.scrollY}px`;
    dropdown.style.left = `${rect.left + window.scrollX}px`;
}

document.addEventListener('click', (e) => {
    const dropdown = document.getElementById('portal-dropdown');
    if (dropdown && !dropdown.contains(e.target)) {
        dropdown.style.display = 'none';
    }
});

function menuTableHandler(id) {
    return `
        <button type="button"
                class="btn btn-light btn-sm"
                onclick="togglePortalDropdown(event, '${id}')">
            <i class="bi bi-three-dots-vertical"></i>
        </button>
    `;
}

var optionsTable = {
    search: {
        debounceTimeout: 1000,
        server: {
            url: (prev, keyword) => `${prev}?search=${keyword}`
        }
    },
    pagination: {
        limit: 10,
        resetPageOnUpdate: true,
        server: {
            url: (prev, page, limit) => {
                const sep = prev.includes('?') ? '&' : '?';
                return `${prev}${sep}limit=${limit}&offset=${page * limit}`;
            }
        }
    },
    language: {
        search: { placeholder: 'Buscar...' },
        pagination: {
            showing: 'Viendo',
            to: 'a',
            of: 'de',
            results: () => 'Resultados',
            previous: 'Anterior',
            next: 'Siguiente'
        }
    },
    sort: true,
    resizable: true,
    autoWidth: true,
    className: {
        table: 'table table-sm table-hover',
        th: 'text-start',
        td: 'align-middle'
    }
};
