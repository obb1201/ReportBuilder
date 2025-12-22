// Sort & Limit Builder - jQuery Implementation
window.SortLimit = (function() {
    'use strict';

    // State
    let sortFields = [];
    let sortIdCounter = 0;
    let limitValue = null;
    let availableFields = [];

    // Initialize on document ready
    $(document).ready(function() {
        attachEventHandlers();
    });

    function attachEventHandlers() {
        // Add sort field button
        $('#addSortBtn').on('click', showAddSortDialog);

        // Limit input
        $('#limitInput').on('input', function() {
            const value = parseInt($(this).val());
            limitValue = value > 0 ? value : null;
            updateQueryPreview();
        });

        // Clear limit button
        $('#clearLimitBtn').on('click', function() {
            $('#limitInput').val('');
            limitValue = null;
            updateQueryPreview();
        });
    }

    // Set available fields from report builder
    function setAvailableFields(fields) {
        availableFields = fields;
    }

    // Show add sort dialog
    function showAddSortDialog() {
        if (availableFields.length === 0) {
            showToast('Please select an object first', 'warning');
            return;
        }

        // Create modal HTML
        const modalHtml = `
            <div class="modal fade" id="addSortModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title"><i class="bi bi-sort-down"></i> Add Sort Field</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label for="sortFieldSelect" class="form-label">Field to Sort By</label>
                                <select class="form-select" id="sortFieldSelect">
                                    <option value="">Choose a field...</option>
                                    ${availableFields.map(f => `<option value="${f.apiName}">${f.label}</option>`).join('')}
                                </select>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Sort Direction</label>
                                <div class="btn-group w-100" role="group">
                                    <input type="radio" class="btn-check" name="sortDirection" id="sortAsc" value="ASC" checked>
                                    <label class="btn btn-outline-primary" for="sortAsc">
                                        <i class="bi bi-sort-alpha-down"></i> Ascending (A-Z, 0-9)
                                    </label>
                                    
                                    <input type="radio" class="btn-check" name="sortDirection" id="sortDesc" value="DESC">
                                    <label class="btn btn-outline-primary" for="sortDesc">
                                        <i class="bi bi-sort-alpha-down-alt"></i> Descending (Z-A, 9-0)
                                    </label>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" id="confirmAddSortBtn" disabled>Add Sort</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal if any
        $('#addSortModal').remove();
        
        // Add modal to page
        $('body').append(modalHtml);
        
        // Attach modal event handlers
        attachSortModalEventHandlers();
        
        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('addSortModal'));
        modal.show();
    }

    function attachSortModalEventHandlers() {
        // Field selection changed
        $('#sortFieldSelect').on('change', function() {
            validateSortForm();
        });

        // Confirm add sort
        $('#confirmAddSortBtn').on('click', function() {
            addSortField();
            bootstrap.Modal.getInstance(document.getElementById('addSortModal')).hide();
        });
    }

    function validateSortForm() {
        const field = $('#sortFieldSelect').val();
        $('#confirmAddSortBtn').prop('disabled', !field);
    }

    function addSortField() {
        const fieldApiName = $('#sortFieldSelect').val();
        const field = availableFields.find(f => f.apiName === fieldApiName);
        const direction = $('input[name="sortDirection"]:checked').val();

        // Check if field already added for sorting
        if (sortFields.some(sf => sf.field.apiName === fieldApiName)) {
            showToast(`Field "${field.label}" is already in sort list`, 'warning');
            return;
        }

        const sortField = {
            id: ++sortIdCounter,
            field: field,
            direction: direction
        };

        sortFields.push(sortField);
        renderSortFields();
        updateQueryPreview();
        
        showToast(`Added sort: ${field.label} ${direction}`, 'success');
    }

    function renderSortFields() {
        const $container = $('#sortFieldsContainer');
        $container.empty();

        if (sortFields.length === 0) {
            $('#noSortMessage').show();
            return;
        }

        $('#noSortMessage').hide();

        sortFields.forEach((sortField, index) => {
            const $item = createSortFieldItem(sortField, index);
            $container.append($item);
        });
    }

    function createSortFieldItem(sortField, index) {
        const directionIcon = sortField.direction === 'ASC' 
            ? '<i class="bi bi-sort-alpha-down"></i>' 
            : '<i class="bi bi-sort-alpha-down-alt"></i>';
        
        const directionLabel = sortField.direction === 'ASC' ? 'Ascending' : 'Descending';

        const $item = $(`
            <div class="sort-item border rounded p-2 mb-2 bg-light d-flex align-items-center">
                <div class="me-2 text-muted" style="cursor: move;">
                    <i class="bi bi-grip-vertical"></i>
                </div>
                <div class="flex-grow-1">
                    <strong>${sortField.field.label}</strong>
                    <span class="badge bg-secondary ms-2">${directionIcon} ${directionLabel}</span>
                </div>
                <div class="btn-group btn-group-sm me-2">
                    <button class="btn btn-outline-secondary move-up-btn" data-index="${index}" ${index === 0 ? 'disabled' : ''}>
                        <i class="bi bi-arrow-up"></i>
                    </button>
                    <button class="btn btn-outline-secondary move-down-btn" data-index="${index}" ${index === sortFields.length - 1 ? 'disabled' : ''}>
                        <i class="bi bi-arrow-down"></i>
                    </button>
                </div>
                <button class="btn btn-sm btn-danger remove-sort-btn" data-id="${sortField.id}">
                    <i class="bi bi-trash"></i>
                </button>
            </div>
        `);

        // Attach event handlers
        $item.find('.remove-sort-btn').on('click', function() {
            removeSortField($(this).data('id'));
        });

        $item.find('.move-up-btn').on('click', function() {
            moveSortField($(this).data('index'), 'up');
        });

        $item.find('.move-down-btn').on('click', function() {
            moveSortField($(this).data('index'), 'down');
        });

        return $item;
    }

    function removeSortField(sortId) {
        sortFields = sortFields.filter(sf => sf.id !== sortId);
        renderSortFields();
        updateQueryPreview();
        showToast('Sort field removed', 'info');
    }

    function moveSortField(index, direction) {
        if (direction === 'up' && index > 0) {
            [sortFields[index], sortFields[index - 1]] = [sortFields[index - 1], sortFields[index]];
        } else if (direction === 'down' && index < sortFields.length - 1) {
            [sortFields[index], sortFields[index + 1]] = [sortFields[index + 1], sortFields[index]];
        }
        
        renderSortFields();
        updateQueryPreview();
    }

    function getOrderByClause() {
        if (sortFields.length === 0) return '';

        const sortClauses = sortFields.map(sf => `${sf.field.apiName} ${sf.direction}`);
        return `ORDER BY ${sortClauses.join(', ')}`;
    }

    function getLimitClause() {
        if (!limitValue || limitValue <= 0) return '';
        return `LIMIT ${limitValue}`;
    }

    function updateQueryPreview() {
        // Trigger query preview update in report builder if available
        if (window.updateQueryPreviewFromSortLimit) {
            window.updateQueryPreviewFromSortLimit();
        }
    }

    function clearAll() {
        sortFields = [];
        limitValue = null;
        $('#limitInput').val('');
        renderSortFields();
        updateQueryPreview();
    }

    function showToast(message, type = 'info') {
        const toastId = 'toast-' + Date.now();
        const toast = $(`
            <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `);

        if ($('#toastContainer').length === 0) {
            $('body').append('<div id="toastContainer" class="toast-container position-fixed top-0 end-0 p-3"></div>');
        }

        $('#toastContainer').append(toast);
        new bootstrap.Toast(toast[0]).show();
        
        toast.on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }

    // Public API
    return {
        setAvailableFields: setAvailableFields,
        getOrderByClause: getOrderByClause,
        getLimitClause: getLimitClause,
        clearAll: clearAll
    };
})();
