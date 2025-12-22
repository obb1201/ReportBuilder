// Report Builder V2 - Redesigned with Drag & Drop
(function() {
    'use strict';

    // State
    let allObjects = [];
    let selectedObject = null;
    let allColumns = [];
    let reportColumns = []; // Columns in the report template
    let sortableInstance = null;
    let pageSize = 25;
    let selectedObjectHasData = false;

    // Initialize
    $(document).ready(function() {
        init();
    });

    function init() {
        loadObjects();
        attachEventHandlers();
        initializeDragDrop();
    }

    function attachEventHandlers() {
        // Object search
        $('#objectSearch').on('input', function() {
            filterObjects($(this).val());
        });

        // Column search
        $('#columnSearch').on('input', function() {
            filterColumns($(this).val());
        });

        // Page size selector
        $('#pageSizeSelect').on('change', function() {
            pageSize = parseInt($(this).val());
            updateQueryPreview();
        });

        // Copy query button
        $('#copyQueryBtn').on('click', copyQueryToClipboard);

        // Reset button
        $('#resetBtn').on('click', resetReportBuilder);

        // Run Query button
        $('#runQueryBtn').on('click', executeQuery);

        // Save Report button (placeholder)
        $('#saveReportBtn').on('click', function() {
            showToast('Save Report feature coming soon!', 'info');
        });
    }

    // Initialize Sortable.js for drag & drop
    function initializeDragDrop() {
        const el = document.getElementById('reportColumns');
        sortableInstance = Sortable.create(el, {
            animation: 150,
            handle: '.drag-handle',
            ghostClass: 'sortable-ghost',
            dragClass: 'sortable-drag',
            onEnd: function() {
                // Update report columns array based on new order
                updateReportColumnsFromDOM();
                updateQueryPreview();
            }
        });
    }

    // Load all objects from API
    function loadObjects() {
        $.ajax({
            url: '/api/MetadataApi/objects',
            method: 'GET',
            success: function(data) {
                allObjects = data;
                displayObjects(data);
                showToast(`Loaded ${data.length} objects`, 'success');
            },
            error: function() {
                $('#objectList').html('<div class="text-danger text-center">Failed to load objects</div>');
                showToast('Failed to load objects', 'danger');
            }
        });
    }

    // Display objects
    function displayObjects(objects) {
        const $list = $('#objectList');
        $list.empty();

        if (objects.length === 0) {
            $list.html('<div class="text-muted text-center">No objects found</div>');
            return;
        }

        objects.forEach(obj => {
            const $item = createObjectItem(obj);
            $list.append($item);
        });
    }

    // Create object item HTML
    function createObjectItem(obj) {
        const icon = obj.isCustom ? '🔧' : '📦';
        const type = obj.isCustom ? 'Custom' : 'Standard';
        
        const $item = $(`
            <div class="object-item" data-object='${JSON.stringify(obj)}'>
                <div class="object-item-name">${icon} ${obj.label}</div>
                <div class="object-item-type">${type}</div>
            </div>
        `);

        $item.on('click', function() {
            const objectData = JSON.parse($(this).attr('data-object'));
            selectObject(objectData);
        });

        return $item;
    }

    // Filter objects
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

    // Select an object
    function selectObject(obj) {
        selectedObject = obj;
        
        // Update UI
        $('.object-item').removeClass('active');
        $(`.object-item`).each(function() {
            const itemData = JSON.parse($(this).attr('data-object'));
            if (itemData.apiName === obj.apiName) {
                $(this).addClass('active');
            }
        });

        // Load object fields
        loadObjectFields(obj.apiName);
    }

    // Load object fields
    function loadObjectFields(objectName) {
        $.ajax({
            url: `/api/MetadataApi/objects/${objectName}`,
            method: 'GET',
            success: function(data) {
                allColumns = data.fields || [];
                displayColumns(allColumns);
                
                // Initialize FilterBuilder with fields
                if (window.FilterBuilder) {
                    window.FilterBuilder.setAvailableFields(allColumns);
                    $('#addFilterBtn').prop('disabled', false);
                }
                
                // Check if object has sample data
                checkObjectHasData(objectName);
                
                showToast(`Loaded ${allColumns.length} columns`, 'success');
            },
            error: function() {
                showToast('Failed to load columns', 'danger');
            }
        });
    }

    // Display columns
    function displayColumns(columns) {
        const $list = $('#columnList');
        $list.empty();

        if (columns.length === 0) {
            $list.html('<div class="text-muted text-center py-4">No columns available</div>');
            return;
        }

        columns.forEach(col => {
            const $item = createColumnItem(col);
            $list.append($item);
        });
    }

    // Create column item HTML
    function createColumnItem(col) {
        const typeIcon = getTypeIcon(col.dataType);
        const isSelected = reportColumns.some(rc => rc.apiName === col.apiName);
        
        const $item = $(`
            <div class="column-item ${isSelected ? 'selected' : ''}" data-column='${JSON.stringify(col)}'>
                <div class="column-type-icon" style="background: ${getTypeColor(col.dataType)}">
                    ${typeIcon}
                </div>
                <div class="column-info">
                    <div class="column-name">${col.label}</div>
                    <div class="column-api-name">${col.apiName}</div>
                </div>
                <div class="column-badges">
                    ${col.isRequired ? '<span class="badge bg-danger badge-custom">Req</span>' : ''}
                    ${col.isCustom ? '<span class="badge bg-info badge-custom">Custom</span>' : ''}
                </div>
            </div>
        `);

        $item.on('click', function() {
            const columnData = JSON.parse($(this).attr('data-column'));
            toggleColumn(columnData, $item);
        });

        return $item;
    }

    // Toggle column selection
    function toggleColumn(column, $item) {
        const index = reportColumns.findIndex(rc => rc.apiName === column.apiName);
        
        if (index > -1) {
            // Remove from report
            reportColumns.splice(index, 1);
            $item.removeClass('selected');
        } else {
            // Add to report
            reportColumns.push({
                ...column,
                sortDirection: null // null, 'ASC', or 'DESC'
            });
            $item.addClass('selected');
        }

        renderReportColumns();
        updateQueryPreview();
    }

    // Render report columns (drag & drop area)
    function renderReportColumns() {
        const $container = $('#reportColumns');
        $container.empty();

        if (reportColumns.length === 0) {
            $('#noColumnsMessage').show();
            $('#copyQueryBtn').prop('disabled', true);
            $('#saveReportBtn').prop('disabled', true);
            updateRunQueryButton();
            return;
        }

        $('#noColumnsMessage').hide();
        $('#copyQueryBtn').prop('disabled', false);
        $('#saveReportBtn').prop('disabled', false);
        updateRunQueryButton();

        reportColumns.forEach((col, index) => {
            const $item = createReportColumnItem(col, index);
            $container.append($item);
        });
    }

    // Create report column item (draggable)
    function createReportColumnItem(col, index) {
        const typeIcon = getTypeIcon(col.dataType);
        
        const $item = $(`
            <div class="report-column-item" data-index="${index}">
                <div class="drag-handle">
                    <i class="bi bi-grip-vertical"></i>
                </div>
                <div class="column-type-icon" style="background: ${getTypeColor(col.dataType)}">
                    ${typeIcon}
                </div>
                <div class="report-column-info">
                    <div class="report-column-name">${col.label}</div>
                    <small class="text-muted">${col.apiName}</small>
                </div>
                <div class="report-column-sort">
                    <button class="sort-btn ${col.sortDirection === 'ASC' ? 'active' : ''}" data-direction="ASC" data-index="${index}">
                        <i class="bi bi-sort-alpha-down"></i> ASC
                    </button>
                    <button class="sort-btn ${col.sortDirection === 'DESC' ? 'active' : ''}" data-direction="DESC" data-index="${index}">
                        <i class="bi bi-sort-alpha-down-alt"></i> DESC
                    </button>
                </div>
                <button class="btn btn-sm btn-danger remove-column-btn" data-index="${index}">
                    <i class="bi bi-x"></i>
                </button>
            </div>
        `);

        // Sort button handlers
        $item.find('.sort-btn').on('click', function() {
            const idx = $(this).data('index');
            const direction = $(this).data('direction');
            toggleSort(idx, direction);
        });

        // Remove button handler
        $item.find('.remove-column-btn').on('click', function() {
            const idx = $(this).data('index');
            removeColumn(idx);
        });

        return $item;
    }

    // Toggle sort direction
    function toggleSort(index, direction) {
        const col = reportColumns[index];
        
        // If clicking the same direction, turn it off
        if (col.sortDirection === direction) {
            col.sortDirection = null;
        } else {
            col.sortDirection = direction;
        }

        renderReportColumns();
        updateQueryPreview();
    }

    // Remove column from report
    function removeColumn(index) {
        const col = reportColumns[index];
        reportColumns.splice(index, 1);
        
        // Update column list to show unselected
        $(`.column-item`).each(function() {
            const itemData = JSON.parse($(this).attr('data-column'));
            if (itemData.apiName === col.apiName) {
                $(this).removeClass('selected');
            }
        });

        renderReportColumns();
        updateQueryPreview();
    }

    // Update report columns array from DOM (after drag & drop)
    function updateReportColumnsFromDOM() {
        const newOrder = [];
        $('#reportColumns .report-column-item').each(function() {
            const index = $(this).data('index');
            newOrder.push(reportColumns[index]);
        });
        reportColumns = newOrder;
    }

    // Filter columns
    function filterColumns(searchTerm) {
        if (!searchTerm) {
            displayColumns(allColumns);
            return;
        }

        const filtered = allColumns.filter(col =>
            col.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
            col.apiName.toLowerCase().includes(searchTerm.toLowerCase())
        );

        displayColumns(filtered);
    }

    // Update SOQL query preview
    function updateQueryPreview() {
        const $preview = $('#queryPreview code');

        if (!selectedObject || reportColumns.length === 0) {
            $preview.text('Select an object and add columns to generate a query');
            return;
        }

        // Build SELECT clause
        const fieldNames = reportColumns.map(c => c.apiName).join(',\n  ');
        let query = `SELECT\n  ${fieldNames}\nFROM ${selectedObject.apiName}`;

        // Add WHERE clause from FilterBuilder
        if (window.FilterBuilder) {
            const whereClause = window.FilterBuilder.getWhereClause();
            if (whereClause) {
                query += `\n${whereClause}`;
            }
        }

        // Add ORDER BY clause
        const sortedColumns = reportColumns.filter(c => c.sortDirection);
        if (sortedColumns.length > 0) {
            const orderClauses = sortedColumns.map(c => `${c.apiName} ${c.sortDirection}`);
            query += `\nORDER BY ${orderClauses.join(', ')}`;
        }

        // Add LIMIT clause (page size)
        query += `\nLIMIT ${pageSize}`;

        $preview.text(query);
    }

    // Expose function for FilterBuilder to call
    window.updateQueryPreviewFromFilter = updateQueryPreview;

    // Copy query to clipboard
    function copyQueryToClipboard() {
        const query = $('#queryPreview code').text();
        
        if (navigator.clipboard) {
            navigator.clipboard.writeText(query).then(() => {
                showToast('Query copied to clipboard!', 'success');
            }).catch(() => {
                fallbackCopy(query);
            });
        } else {
            fallbackCopy(query);
        }
    }

    function fallbackCopy(text) {
        const $temp = $('<textarea>');
        $('body').append($temp);
        $temp.val(text).select();
        document.execCommand('copy');
        $temp.remove();
        showToast('Query copied to clipboard!', 'success');
    }

    // Reset report builder
    function resetReportBuilder() {
        if (!selectedObject && reportColumns.length === 0) return;

        if (confirm('Are you sure you want to reset the report builder?')) {
            selectedObject = null;
            allColumns = [];
            reportColumns = [];
            selectedObjectHasData = false;
            
            $('.object-item').removeClass('active');
            $('#columnList').html('<div class="text-center text-muted py-4"><i class="bi bi-columns" style="font-size: 2rem;"></i><p class="mt-2 mb-0">Select an object to view columns</p></div>');
            $('#reportColumns').empty();
            $('#noColumnsMessage').show();
            $('#objectSearch').val('');
            $('#columnSearch').val('');
            $('#addFilterBtn').prop('disabled', true);
            
            if (window.FilterBuilder) {
                window.FilterBuilder.clearFilters();
            }
            
            updateRunQueryButton();
            updateQueryPreview();
            showToast('Report builder reset', 'info');
        }
    }

    // Get type icon
    function getTypeIcon(dataType) {
        const icons = {
            'String': 'Aa',
            'Int': '#',
            'Double': '#',
            'Currency': '$',
            'Boolean': '✓',
            'Date': '📅',
            'DateTime': '🕐',
            'Email': '@',
            'Phone': '📞',
            'Url': '🔗',
            'Picklist': '▼',
            'Reference': '→',
            'Id': '🔑'
        };
        return icons[dataType] || '•';
    }

    // Get type color
    function getTypeColor(dataType) {
        const colors = {
            'String': '#e3f2fd',
            'Int': '#fff3e0',
            'Double': '#fff3e0',
            'Currency': '#e8f5e9',
            'Boolean': '#f3e5f5',
            'Date': '#fce4ec',
            'DateTime': '#fce4ec',
            'Email': '#e0f2f1',
            'Phone': '#e0f2f1',
            'Url': '#e0f7fa',
            'Picklist': '#fff9c4',
            'Reference': '#e1bee7',
            'Id': '#ffebee'
        };
        return colors[dataType] || '#f5f5f5';
    }

    // Show toast notification
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

    // Check if object has sample data
    async function checkObjectHasData(objectName) {
        try {
            const response = await $.ajax({
                url: `/api/DataApi/check/${objectName}`,
                method: 'GET'
            });
            
            selectedObjectHasData = response.hasData;
            updateRunQueryButton();
            
            if (response.hasData) {
                showToast(`${objectName} has sample data ready`, 'success');
            } else {
                showToast(`${objectName} needs sample data - click Setup Data`, 'warning');
            }
        } catch (error) {
            console.error('Error checking object data:', error);
            selectedObjectHasData = false;
            updateRunQueryButton();
        }
    }

    // Setup sample data for selected object
    async function setupObjectData() {
        if (!selectedObject) {
            showToast('Please select an object first', 'warning');
            return;
        }
        
        if (confirm(`Generate 500 sample records for ${selectedObject.label}?\n\nThis may take 10-30 seconds.`)) {
            const originalHtml = '<i class="bi bi-gear-fill"></i> Setup';
            const loadingHtml = '<span class="spinner-border spinner-border-sm me-1"></span>Generating...';
            
            // Note: Setup button will be added to HTML
            showToast('Generating sample data... please wait', 'info');
            
            try {
                const response = await $.ajax({
                    url: `/api/DataApi/setup/${selectedObject.apiName}?recordCount=500`,
                    method: 'POST',
                    timeout: 300000 // 5 minutes
                });
                
                if (response.success) {
                    showToast(response.message, 'success');
                    selectedObjectHasData = true;
                    updateRunQueryButton();
                } else {
                    showToast('Failed to generate data', 'danger');
                }
            } catch (error) {
                console.error('Error setting up data:', error);
                const errorMsg = error.responseJSON?.error || error.statusText || 'Unknown error';
                showToast('Error generating data: ' + errorMsg, 'danger');
            }
        }
    }

    // Execute SOQL query
    async function executeQuery() {
        if (!selectedObject || reportColumns.length === 0) {
            showToast('Please select an object and add columns', 'warning');
            return;
        }
        
        if (!selectedObjectHasData) {
            if (confirm('This object has no sample data yet.\n\nWould you like to generate 500 sample records now?')) {
                await setupObjectData();
                // After setup completes, try executing again
                if (selectedObjectHasData) {
                    await executeQuery();
                }
            }
            return;
        }
        
        // Generate SOQL query
        const soqlQuery = generateSoqlQuery();
        
        const $btn = $('#runQueryBtn');
        const originalHtml = $btn.html();
        $btn.html('<span class="spinner-border spinner-border-sm me-1"></span>Running...').prop('disabled', true);
        
        try {
            const response = await $.ajax({
                url: '/api/DataApi/execute',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ soqlQuery: soqlQuery }),
                timeout: 60000 // 1 minute
            });
            
            if (response.success) {
                displayQueryResults(response);
                showToast(`Query executed in ${response.executionTimeMs}ms - ${response.recordCount} records`, 'success');
            } else {
                showToast('Query failed: ' + response.error, 'danger');
            }
        } catch (error) {
            console.error('Error executing query:', error);
            const errorMsg = error.responseJSON?.error || error.statusText || 'Unknown error';
            showToast('Error executing query: ' + errorMsg, 'danger');
        } finally {
            $btn.html(originalHtml).prop('disabled', !selectedObjectHasData || reportColumns.length === 0);
        }
    }

    // Generate SOQL query from current state
    function generateSoqlQuery() {
        const fields = reportColumns.map(c => c.apiName).join(', ');
        let query = `SELECT ${fields} FROM ${selectedObject.apiName}`;
        
        // Add WHERE clause from FilterBuilder if available
        if (window.FilterBuilder) {
            const whereClause = window.FilterBuilder.getWhereClause();
            if (whereClause) {
                query += `\n${whereClause}`;
            }
        }
        
        // Add ORDER BY from sorting
        const sortedColumns = reportColumns.filter(c => c.sortDirection);
        if (sortedColumns.length > 0) {
            const orderBy = sortedColumns.map(c => `${c.apiName} ${c.sortDirection}`).join(', ');
            query += `\nORDER BY ${orderBy}`;
        }
        
        // Add LIMIT from page size
        query += `\nLIMIT ${pageSize}`;
        
        return query;
    }

    // Display query results in modal
    function displayQueryResults(response) {
        // Remove existing results modal if any
        $('#queryResultsModal').remove();
        
        // Create results modal
        const modal = `
            <div class="modal fade" id="queryResultsModal" tabindex="-1">
                <div class="modal-dialog modal-xl modal-fullscreen-lg-down">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">
                                <i class="bi bi-table"></i> Query Results 
                                <span class="badge bg-primary ms-2">${response.recordCount} records</span>
                                <span class="badge bg-secondary ms-1">${response.executionTimeMs}ms</span>
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3 d-flex justify-content-between align-items-center">
                                <div>
                                    <button class="btn btn-sm btn-success" id="exportCsvBtn">
                                        <i class="bi bi-download"></i> Export CSV
                                    </button>
                                </div>
                                <div>
                                    <small class="text-muted">
                                        <i class="bi bi-info-circle"></i> 
                                        Showing up to ${pageSize} records
                                    </small>
                                </div>
                            </div>
                            <div class="table-responsive" style="max-height: 60vh; overflow-y: auto;">
                                <table class="table table-striped table-hover table-sm" id="resultsTable">
                                    <thead class="sticky-top bg-light">
                                        <tr>
                                            ${response.columns.map(c => `<th>${escapeHtml(c.name)}</th>`).join('')}
                                        </tr>
                                    </thead>
                                    <tbody>
                                        ${response.data.map(row => `
                                            <tr>
                                                ${response.columns.map(c => `
                                                    <td>${formatValue(row[c.name])}</td>
                                                `).join('')}
                                            </tr>
                                        `).join('')}
                                    </tbody>
                                </table>
                            </div>
                            <div class="mt-3">
                                <strong>Generated SQL Query:</strong>
                                <pre class="bg-light p-2 rounded mt-2" style="font-size: 0.85rem;"><code>${escapeHtml(response.sqlQuery)}</code></pre>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        $('body').append(modal);
        const modalInstance = new bootstrap.Modal(document.getElementById('queryResultsModal'));
        modalInstance.show();
        
        // CSV Export handler
        $('#exportCsvBtn').on('click', function() {
            exportToCsv(response.columns, response.data);
        });
    }

    // Format value for display
    function formatValue(value) {
        if (value === null || value === undefined) {
            return '<span class="text-muted">null</span>';
        }
        if (typeof value === 'boolean') {
            return value ? '<span class="badge bg-success">true</span>' : '<span class="badge bg-secondary">false</span>';
        }
        if (typeof value === 'number') {
            return value.toLocaleString();
        }
        return escapeHtml(String(value));
    }

    // Escape HTML to prevent XSS
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Export to CSV
    function exportToCsv(columns, data) {
        let csv = columns.map(c => c.name).join(',') + '\n';
        
        data.forEach(row => {
            const values = columns.map(c => {
                let value = row[c.name];
                if (value === null || value === undefined) {
                    return '';
                }
                value = String(value);
                if (value.includes(',') || value.includes('"') || value.includes('\n')) {
                    value = '"' + value.replace(/"/g, '""') + '"';
                }
                return value;
            });
            csv += values.join(',') + '\n';
        });
        
        const blob = new Blob([csv], { type: 'text/csv' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${selectedObject.apiName}_${new Date().toISOString().split('T')[0]}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
        
        showToast('CSV file downloaded', 'success');
    }

    // Update Run Query button state
    function updateRunQueryButton() {
        const canRun = selectedObject && reportColumns.length > 0 && selectedObjectHasData;
        $('#runQueryBtn').prop('disabled', !canRun);
    }

    // Expose setupObjectData globally so it can be called from a button
    window.setupObjectData = setupObjectData;
})();
