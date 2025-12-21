// Site-wide JavaScript
(function() {
    'use strict';

    $(document).ready(function() {
        // Initialize tooltips
        $('[data-bs-toggle="tooltip"]').tooltip();

        // Initialize popovers
        $('[data-bs-toggle="popover"]').popover();
    });
})();
