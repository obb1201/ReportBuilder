// Filter Builder - jQuery Implementation
window.FilterBuilder = (function() {
    'use strict';

    // State
    let availableFields = [];
    let filters = [];
    let filterIdCounter = 0;
    let filterLogic = 'AND';

    // Field type to operators mapping
    const OPERATORS = {
        'String': [
            { value: 'equals', label: 'Equals', needsValue: true },
            { value: 'notEquals', label: 'Not Equals', needsValue: true },
            { value: 'contains', label: 'Contains', needsValue: true },
            { value: 'notContains', label: 'Does Not Contain', needsValue: true },
            { value: 'startsWith', label: 'Starts With', needsValue: true },
            { value: 'endsWith', label: 'Ends With', needsValue: true },
            { value: 'isEmpty', label: 'Is Empty', needsValue: false },
            { value: 'isNotEmpty', label: 'Is Not Empty', needsValue: false }
        ],
        'Int': [
            { value: 'equals', label: 'Equals', needsValue: true },
            { value: 'notEquals', label: 'Not Equals', needsValue: true },
            { value: 'greaterThan', label: 'Greater Than', needsValue: true },
            { value: 'greaterThanOrEquals', label: 'Greater Than or Equals', needsValue: true },
            { value: 'lessThan', label: 'Less Than', needsValue: true },
            { value: 'lessThanOrEquals', label: 'Less Than or Equals', needsValue: true },
            { value: 'isNull', label: 'Is Null', needsValue: false },
            { value: 'isNotNull', label: 'Is Not Null', needsValue: false }
        ],
        'Double': [
            { value: 'equals', label: 'Equals', needsValue: true },
            { value: 'notEquals', label: 'Not Equals', needsValue: true },
            { value: 'greaterThan', label: 'Greater Than', needsValue: true },
            { value: 'greaterThanOrEquals', label: 'Greater Than or Equals', needsValue: true },
            { value: 'lessThan', label: 'Less Than', needsValue: true },
            { value: 'lessThanOrEquals', label: 'Less Than or Equals', needsValue: true },
            { value: 'isNull', label: 'Is Null', needsValue: false },
            { value: 'isNotNull', label: 'Is Not Null', needsValue: false }
        ],
        'Currency': [
            { value: 'equals', label: 'Equals', needsValue: true },
            { value: 'notEquals', label: 'Not Equals', needsValue: true },
            { value: 'greaterThan', label: 'Greater Than', needsValue: true },
            { value: 'greaterThanOrEquals', label: 'Greater Than or Equals', needsValue: true },
            { value: 'lessThan', label: 'Less Than', needsValue: true },
            { value: 'lessThanOrEquals', label: 'Less Than or Equals', needsValue: true },
            { value: 'isNull', label: 'Is Null', needsValue: false },
            { value: 'isNotNull', label: 'Is Not Null', needsValue: false }
        ],
        'Boolean': [
            { value: 'equals', label: 'Equals', needsValue: true }
        ],
        'Date': [
            { value: 'equals', label: 'Equals', needsValue: true },
            { value: 'notEquals', label: 'Not Equals', needsValue: true },
            { value: 'greaterThan', label: 'After', needsValue: true },
            { value: 'greaterThanOrEquals', label: 'On or After', needsValue: true },
            { value: 'lessThan', label: 'Before', needsValue: true },
            { value: 'lessThanOrEquals', label: 'On or Before', needsValue: true },
            { value: 'isNull', label: 'Is Null', needsValue: false },
            { value: 'isNotNull', label: 'Is Not Null', needsValue: false }
        ],
        'DateTime': [
            { value: 'equals', label: 'Equals', needsValue: true },
            { value: 'notEquals', label: 'Not Equals', needsValue: true },
            { value: 'greaterThan', label: 'After', needsValue: true },
            { value: 'greaterThanOrEquals', label: 'On or After', needsValue: true },
            { value: 'lessThan', label: 'Before', needsValue: true },
            { value: 'lessThanOrEquals', label: 'On or Before', needsValue: true },
            { value: 'isNull', label: 'Is Null', needsValue: false },
            { value: 'isNotNull', label: 'Is Not Null', needsValue: false }
        ],
        'Picklist': [
            { value: 'equals', label: 'Equals', needsValue: true },
            { value: 'notEquals', label: 'Not Equals', needsValue: true },
            { value: 'isNull', label: 'Is Null', needsValue: false },
            { value: 'isNotNull', label: 'Is Not Null', needsValue: false }
        ],
        'Reference': [
            { value: 'equals', label: 'Equals', needsValue: true },
            { value: 'notEquals', label: 'Not Equals', needsValue: true },
            { value: 'isNull', label: 'Is Null', needsValue: false },
            { value: 'isNotNull', label: 'Is Not Null', needsValue: false }
        ]
    };

    // Initialize on document ready
    $(document).ready(function() {
        attachEventHandlers();
    });

    function attachEventHandlers() {
        // Add filter button
        $('#addFilterBtn').on('click', showAddFilterDialog);

        // Filter logic radio buttons
        $('input[name="filterLogic"]').on('change', function() {
            filterLogic = $(this).val();
            updateWhereClause();
        });
    }

    // Load object fields from API
    function loadObjectFields(objectName) {
        $.ajax({
            url: `/api/MetadataApi/objects/${objectName}`,
            method: 'GET',
            success: function(data) {
                availableFields = data.fields || [];
                showToast(`Loaded ${availableFields.length} fields for ${data.label}`, 'success');
                
                // Auto-open add filter dialog
                showAddFilterDialog();
            },
            error: function() {
                showToast('Failed to load object fields', 'danger');
            }
        });
    }

    // Show add filter dialog
    function showAddFilterDialog() {
        if (availableFields.length === 0) {
            showToast('Please load an object first to see available fields', 'warning');
            return;
        }

        // Create modal HTML
        const modalHtml = `
            <div class="modal fade" id="addFilterModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title"><i class="bi bi-funnel-fill"></i> Add Filter</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label for="filterFieldSelect" class="form-label">Field</label>
                                <select class="form-select" id="filterFieldSelect">
                                    <option value="">Choose a field...</option>
                                    ${availableFields.map(f => `<option value="${f.apiName}" data-type="${f.dataType}">${f.label} (${f.dataType})</option>`).join('')}
                                </select>
                            </div>
                            <div class="mb-3">
                                <label for="filterOperatorSelect" class="form-label">Operator</label>
                                <select class="form-select" id="filterOperatorSelect" disabled>
                                    <option value="">Select a field first...</option>
                                </select>
                            </div>
                            <div class="mb-3" id="filterValueContainer" style="display: none;">
                                <label for="filterValueInput" class="form-label">Value</label>
                                <input type="text" class="form-control" id="filterValueInput" placeholder="Enter value">
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" id="confirmAddFilterBtn" disabled>Add Filter</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal if any
        $('#addFilterModal').remove();
        
        // Add modal to page
        $('body').append(modalHtml);
        
        // Attach modal event handlers
        attachModalEventHandlers();
        
        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('addFilterModal'));
        modal.show();
    }

    function attachModalEventHandlers() {
        // Field selection changed
        $('#filterFieldSelect').on('change', function() {
            const selectedOption = $(this).find('option:selected');
            const fieldType = selectedOption.data('type');
            
            if (fieldType) {
                populateOperators(fieldType);
                $('#filterOperatorSelect').prop('disabled', false);
            } else {
                $('#filterOperatorSelect').html('<option value="">Select a field first...</option>').prop('disabled', true);
                $('#filterValueContainer').hide();
            }
            
            validateFilterForm();
        });

        // Operator selection changed
        $('#filterOperatorSelect').on('change', function() {
            const selectedOperator = $(this).val();
            const fieldType = $('#filterFieldSelect').find('option:selected').data('type');
            
            if (selectedOperator) {
                const operators = OPERATORS[fieldType] || OPERATORS['String'];
                const operator = operators.find(op => op.value === selectedOperator);
                
                if (operator && operator.needsValue) {
                    $('#filterValueContainer').show();
                    setValueInputType(fieldType);
                } else {
                    $('#filterValueContainer').hide();
                }
            }
            
            validateFilterForm();
        });

        // Value input changed
        $('#filterValueInput').on('input', function() {
            validateFilterForm();
        });

        // Confirm add filter
        $('#confirmAddFilterBtn').on('click', function() {
            addFilter();
            bootstrap.Modal.getInstance(document.getElementById('addFilterModal')).hide();
        });
    }

    function populateOperators(fieldType) {
        const operators = OPERATORS[fieldType] || OPERATORS['String'];
        const $select = $('#filterOperatorSelect');
        
        $select.empty();
        $select.append('<option value="">Choose an operator...</option>');
        
        operators.forEach(op => {
            $select.append(`<option value="${op.value}">${op.label}</option>`);
        });
    }

    function setValueInputType(fieldType) {
        const $input = $('#filterValueInput');
        
        switch(fieldType) {
            case 'Int':
            case 'Double':
            case 'Currency':
                $input.attr('type', 'number').attr('step', 'any');
                break;
            case 'Date':
                $input.attr('type', 'date');
                break;
            case 'DateTime':
                $input.attr('type', 'datetime-local');
                break;
            case 'Boolean':
                $input.replaceWith(`
                    <select class="form-select" id="filterValueInput">
                        <option value="">Choose...</option>
                        <option value="true">True</option>
                        <option value="false">False</option>
                    </select>
                `);
                break;
            default:
                $input.attr('type', 'text');
        }
    }

    function validateFilterForm() {
        const field = $('#filterFieldSelect').val();
        const operator = $('#filterOperatorSelect').val();
        const value = $('#filterValueInput').val();
        
        const fieldType = $('#filterFieldSelect').find('option:selected').data('type');
        const operators = OPERATORS[fieldType] || OPERATORS['String'];
        const selectedOp = operators.find(op => op.value === operator);
        
        let isValid = field && operator;
        
        if (selectedOp && selectedOp.needsValue) {
            isValid = isValid && value;
        }
        
        $('#confirmAddFilterBtn').prop('disabled', !isValid);
    }

    function addFilter() {
        const fieldApiName = $('#filterFieldSelect').val();
        const field = availableFields.find(f => f.apiName === fieldApiName);
        const operator = $('#filterOperatorSelect').val();
        const operatorLabel = $('#filterOperatorSelect option:selected').text();
        const value = $('#filterValueInput').val();
        
        const filter = {
            id: ++filterIdCounter,
            field: field,
            operator: operator,
            operatorLabel: operatorLabel,
            value: value
        };
        
        filters.push(filter);
        renderFilters();
        updateWhereClause();
        
        showToast(`Added filter: ${field.label} ${operatorLabel}`, 'success');
    }

    function renderFilters() {
        const $container = $('#filtersContainer');
        $container.empty();
        
        if (filters.length === 0) {
            $('#noFiltersMessage').show();
            $('#filterLogicContainer').addClass('d-none');
            return;
        }
        
        $('#noFiltersMessage').hide();
        
        if (filters.length > 1) {
            $('#filterLogicContainer').removeClass('d-none');
        } else {
            $('#filterLogicContainer').addClass('d-none');
        }
        
        filters.forEach((filter, index) => {
            const $filterItem = createFilterItem(filter, index);
            $container.append($filterItem);
        });
    }

    function createFilterItem(filter, index) {
        const valueDisplay = filter.value || '(no value)';
        
        const $item = $(`
            <div class="filter-item border rounded p-3 mb-2 bg-light">
                <div class="d-flex align-items-start">
                    <div class="flex-grow-1">
                        <div class="d-flex align-items-center mb-2">
                            <span class="badge bg-secondary me-2">${filter.field.dataType}</span>
                            <strong>${filter.field.label}</strong>
                        </div>
                        <div class="text-muted small">
                            ${filter.field.apiName} <strong>${filter.operatorLabel}</strong> 
                            ${filter.value ? `<code>${valueDisplay}</code>` : ''}
                        </div>
                    </div>
                    <button class="btn btn-sm btn-danger ms-2" data-filter-id="${filter.id}">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>
        `);
        
        $item.find('button').on('click', function() {
            removeFilter($(this).data('filter-id'));
        });
        
        return $item;
    }

    function removeFilter(filterId) {
        filters = filters.filter(f => f.id !== filterId);
        renderFilters();
        updateWhereClause();
        showToast('Filter removed', 'info');
    }

    function updateWhereClause() {
        const $preview = $('#whereClausePreview code');
        
        if (filters.length === 0) {
            if ($preview.length > 0) {
                $preview.text('No filters applied');
            }
        } else {
            const whereClauses = filters.map(filter => generateSOQLCondition(filter));
            const whereClause = `WHERE ${whereClauses.join(`\n  ${filterLogic} `)}`;
            
            if ($preview.length > 0) {
                $preview.text(whereClause);
            }
        }
        
        // Trigger query preview update in report builder if available
        if (window.updateQueryPreviewFromFilter) {
            window.updateQueryPreviewFromFilter();
        }
    }

    function generateSOQLCondition(filter) {
        const field = filter.field.apiName;
        const operator = filter.operator;
        const value = filter.value;
        
        // Handle operators that don't need values
        switch(operator) {
            case 'isEmpty':
                return `${field} = ''`;
            case 'isNotEmpty':
                return `${field} != ''`;
            case 'isNull':
                return `${field} = null`;
            case 'isNotNull':
                return `${field} != null`;
        }
        
        // Format value based on field type
        let formattedValue = value;
        
        if (filter.field.dataType === 'String' || 
            filter.field.dataType === 'Picklist' ||
            filter.field.dataType === 'Email' ||
            filter.field.dataType === 'Phone' ||
            filter.field.dataType === 'Url') {
            formattedValue = `'${value}'`;
        } else if (filter.field.dataType === 'Date') {
            formattedValue = value; // Already in YYYY-MM-DD format
        } else if (filter.field.dataType === 'DateTime') {
            formattedValue = value.replace('T', ' '); // Convert datetime-local format
        } else if (filter.field.dataType === 'Boolean') {
            formattedValue = value;
        }
        
        // Generate condition based on operator
        switch(operator) {
            case 'equals':
                return `${field} = ${formattedValue}`;
            case 'notEquals':
                return `${field} != ${formattedValue}`;
            case 'contains':
                return `${field} LIKE '%${value}%'`;
            case 'notContains':
                return `(NOT ${field} LIKE '%${value}%')`;
            case 'startsWith':
                return `${field} LIKE '${value}%'`;
            case 'endsWith':
                return `${field} LIKE '%${value}'`;
            case 'greaterThan':
                return `${field} > ${formattedValue}`;
            case 'greaterThanOrEquals':
                return `${field} >= ${formattedValue}`;
            case 'lessThan':
                return `${field} < ${formattedValue}`;
            case 'lessThanOrEquals':
                return `${field} <= ${formattedValue}`;
            default:
                return `${field} = ${formattedValue}`;
        }
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
        setAvailableFields: (fields) => {
            availableFields = fields;
        },
        loadObjectFields: loadObjectFields,
        getFilters: () => filters,
        getWhereClause: () => {
            if (filters.length === 0) return '';
            const whereClauses = filters.map(f => generateSOQLCondition(f));
            return `WHERE ${whereClauses.join(` ${filterLogic} `)}`;
        },
        clearFilters: () => {
            filters = [];
            renderFilters();
            updateWhereClause();
        }
    };
})();
