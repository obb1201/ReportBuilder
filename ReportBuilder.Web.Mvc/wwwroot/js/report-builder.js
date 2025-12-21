// Report Builder - jQuery Implementation
// All API calls go through MVC controllers
(function() {
    'use strict';

    // State
    let allObjects = [];
    let selectedObject = null;
    let selectedFields = [];

    // Initialize on document ready
    $(document).ready(function() {
        initializeReportBuilder();
    });

    function initializeReportBuilder() {
        loadObjects();
        attachEventHandlers();
    }

    function attachEventHandlers() {
        // Object search
        $('#objectSearch').on('input', function() {
            filterObjects($(this).val());
        });

        // Object selection
        $('#objectSelect').on('change', function() {
            const objectName = $(this).val();
            if (objectName) {
                loadObjectDetails(objectName);
            }
        });

        // Field search
        $('#fieldSearch').on('input', function() {
            filterFields($(this).val());
        });

        // Clear fields button
        $('#clearFieldsBtn').on('click', clearAllFields);

        // Copy query button
        $('#copyQueryBtn').on('click', copyQueryToClipboard);

        // Reset button
        $('#resetBtn').on('click', resetReportBuilder);

        // Run query button (placeholder)
        $('#runQueryBtn').on('click', function() {
            showToast('Run Query feature coming soon!', 'info');
        });

        // Save report button (placeholder)
        $('#saveReportBtn').on('click', function() {
            showToast('Save Report feature coming soon!', 'info');
        });
    }

    // Load all objects from MVC API controller
    function loadObjects() {
        $.ajax({
            url: '/api/MetadataApi/objects',
            method: 'GET',
            success: function(data) {
                allObjects = data;
                displayObjects(data);
                showToast(`Loaded ${data.length} objects`, 'success');
            },
            error: function(xhr, status, error) {
                console.error('Error loading objects:', error);
                $('#objectSelect').html('<option>Error loading objects. Check API connection.</option>');
                showToast('Failed to load objects', 'danger');
            }
        });
    }

    // Display objects in the select list
    function displayObjects(objects) {
        const $select = $('#objectSelect');
        $select.empty();

        if (objects.length === 0) {
            $select.append('<option>No objects found</option>');
            return;
        }

        objects.forEach(obj => {
            const icon = obj.isCustom ? '🔧' : '📦';
            $select.append(`<option value="${obj.apiName}">${icon} ${obj.label}</option>`);
        });
    }

    // Filter objects based on search
    function filterObjects(searchTerm) {
        if (!searchTerm) {
            displayObjects(allObjects);
            return;
        }

        const filtered = allObjects.filter(obj => 
            obj.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
            obj.apiName.toLowerCase().includes(searchTerm.toLowerCase())
        );

        displayObjects(filtered);
    }

    // Load object details with fields from MVC API controller
    function loadObjectDetails(objectName) {
        $.ajax({
            url: `/api/MetadataApi/objects/${objectName}`,
            method: 'GET',
            success: function(data) {
                selectedObject = data;
                displayObjectInfo(data);
                displayFields(data.fields);
                showToast(`Loaded ${data.fields.length} fields for ${data.label}`, 'success');
            },
            error: function(xhr, status, error) {
                console.error('Error loading object details:', error);
                showToast('Failed to load object details', 'danger');
            }
        });
    }

    // Display object info
    function displayObjectInfo(obj) {
        $('#selectedObjectName').text(obj.label);
        $('#selectedObjectFieldCount').text(`${obj.fields.length} fields`);
        $('#objectInfo').removeClass('d-none');
    }

    // Display fields list
    function displayFields(fields) {
        const $fieldsList = $('#fieldsList');
        $fieldsList.empty();

        if (fields.length === 0) {
            $fieldsList.append('<p class="text-muted">No fields available</p>');
            return;
        }

        fields.forEach(field => {
            const $fieldItem = createFieldItem(field);
            $fieldsList.append($fieldItem);
        });
    }

    // Create field item HTML
    function createFieldItem(field) {
        const typeIcon = getFieldTypeIcon(field.dataType);
        const badges = [];

        if (field.isRequired) {
            badges.push('<span class="badge bg-danger ms-1">Required</span>');
        }
        if (field.isCustom) {
            badges.push('<span class="badge bg-info ms-1">Custom</span>');
        }

        const $item = $(`
            <div class="field-item p-2 border-bottom cursor-pointer hover-bg-light" data-field='${JSON.stringify(field)}'>
                <div class="d-flex align-items-center">
                    <span class="me-2">${typeIcon}</span>
                    <div class="flex-grow-1">
                        <strong>${field.label}</strong>
                        <br>
                        <small class="text-muted">${field.apiName}</small>
                    </div>
                    <span class="badge bg-secondary">${field.dataType}</span>
                    ${badges.join('')}
                </div>
            </div>
        `);

        $item.on('click', function() {
            const fieldData = JSON.parse($(this).attr('data-field'));
            addFieldToReport(fieldData);
        });

        return $item;
    }

    // Get icon for field type
    function getFieldTypeIcon(dataType) {
        const icons = {
            'String': '📝',
            'Int': '🔢',
            'Double': '🔢',
            'Currency': '💰',
            'Boolean': '✓',
            'Date': '📅',
            'DateTime': '🕐',
            'Email': '📧',
            'Phone': '📞',
            'Url': '🔗',
            'Picklist': '📋',
            'Reference': '🔗',
            'Id': '🔑'
        };
        return icons[dataType] || '📄';
    }

    // Filter fields based on search
    function filterFields(searchTerm) {
        if (!selectedObject) return;

        const filtered = selectedObject.fields.filter(field =>
            field.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
            field.apiName.toLowerCase().includes(searchTerm.toLowerCase())
        );

        displayFields(filtered);
    }

    // Add field to report
    function addFieldToReport(field) {
        // Check if already added
        if (selectedFields.some(f => f.apiName === field.apiName)) {
            showToast(`Field "${field.label}" is already added`, 'warning');
            return;
        }

        selectedFields.push(field);
        updateSelectedFieldsList();
        updateQueryPreview();
        showToast(`Added field: ${field.label}`, 'success');
    }

    // Update selected fields display
    function updateSelectedFieldsList() {
        const $list = $('#selectedFieldsList');
        $list.empty();

        if (selectedFields.length === 0) {
            $list.append('<p class="text-muted">Click on fields from the Available Fields panel to add them to your report</p>');
            $('#clearFieldsBtn').prop('disabled', true);
            $('#copyQueryBtn').prop('disabled', true);
            $('#runQueryBtn').prop('disabled', true);
            $('#saveReportBtn').prop('disabled', true);
            return;
        }

        selectedFields.forEach((field, index) => {
            const typeIcon = getFieldTypeIcon(field.dataType);
            const $item = $(`
                <div class="selected-field-item d-flex align-items-center p-2 border-bottom">
                    <span class="me-2">${typeIcon}</span>
                    <div class="flex-grow-1">
                        <strong>${field.label}</strong>
                        <br>
                        <small class="text-muted">${field.apiName}</small>
                    </div>
                    <span class="badge bg-secondary me-2">${field.dataType}</span>
                    <button class="btn btn-sm btn-danger" data-index="${index}">
                        <i class="bi bi-x"></i>
                    </button>
                </div>
            `);

            $item.find('button').on('click', function() {
                removeField($(this).data('index'));
            });

            $list.append($item);
        });

        $('#clearFieldsBtn').prop('disabled', false);
        $('#copyQueryBtn').prop('disabled', false);
        $('#runQueryBtn').prop('disabled', false);
        $('#saveReportBtn').prop('disabled', false);
    }

    // Remove field from report
    function removeField(index) {
        const field = selectedFields[index];
        selectedFields.splice(index, 1);
        updateSelectedFieldsList();
        updateQueryPreview();
        showToast(`Removed field: ${field.label}`, 'info');
    }

    // Clear all fields
    function clearAllFields() {
        if (selectedFields.length === 0) return;

        if (confirm('Are you sure you want to clear all selected fields?')) {
            selectedFields = [];
            updateSelectedFieldsList();
            updateQueryPreview();
            showToast('All fields cleared', 'info');
        }
    }

    // Update SOQL query preview
    function updateQueryPreview() {
        const $preview = $('#queryPreview code');

        if (!selectedObject || selectedFields.length === 0) {
            $preview.text('Select an object and add fields to generate a SOQL query');
            return;
        }

        const fieldNames = selectedFields.map(f => f.apiName).join(',\n  ');
        const query = `SELECT\n  ${fieldNames}\nFROM ${selectedObject.apiName}`;

        $preview.text(query);
    }

    // Copy query to clipboard
    function copyQueryToClipboard() {
        const query = $('#queryPreview code').text();
        
        if (navigator.clipboard) {
            navigator.clipboard.writeText(query).then(() => {
                showToast('Query copied to clipboard!', 'success');
            }).catch(err => {
                console.error('Failed to copy:', err);
                showToast('Failed to copy query', 'danger');
            });
        } else {
            // Fallback for older browsers
            const $temp = $('<textarea>');
            $('body').append($temp);
            $temp.val(query).select();
            document.execCommand('copy');
            $temp.remove();
            showToast('Query copied to clipboard!', 'success');
        }
    }

    // Reset report builder
    function resetReportBuilder() {
        if (selectedFields.length === 0 && !selectedObject) return;

        if (confirm('Are you sure you want to reset the report builder?')) {
            selectedObject = null;
            selectedFields = [];
            $('#objectSelect').val('');
            $('#objectSearch').val('');
            $('#fieldSearch').val('');
            $('#objectInfo').addClass('d-none');
            $('#fieldsList').html('<p class="text-muted">Select an object to view its fields</p>');
            updateSelectedFieldsList();
            updateQueryPreview();
            showToast('Report builder reset', 'info');
        }
    }

    // Show toast notification
    function showToast(message, type = 'info') {
        const toastId = 'toast-' + Date.now();
        const toast = $(`
            <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `);

        // Create toast container if it doesn't exist
        if ($('#toastContainer').length === 0) {
            $('body').append('<div id="toastContainer" class="toast-container position-fixed top-0 end-0 p-3"></div>');
        }

        $('#toastContainer').append(toast);
        const bsToast = new bootstrap.Toast(toast[0]);
        bsToast.show();

        // Remove after hidden
        toast.on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }
})();
