const components = {
    renderTable(data, columns, containerId) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error(`Container with id '${containerId}' not found`);
            return;
        }

        if (!data || data.length === 0) {
            container.innerHTML = `
                <div class="text-center py-12 text-gray-500">
                    <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"></path>
                    </svg>
                    <p class="mt-2">No data available</p>
                </div>
            `;
            return;
        }

        let tableHTML = `
            <div class="overflow-x-auto">
                <table class="min-w-full divide-y divide-gray-200">
                    <thead class="bg-gray-50">
                        <tr>
        `;

        columns.forEach(col => {
            tableHTML += `<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">${col.label}</th>`;
        });

        tableHTML += `
                        </tr>
                    </thead>
                    <tbody class="bg-white divide-y divide-gray-200">
        `;

        data.forEach(row => {
            tableHTML += '<tr class="hover:bg-gray-50">';
            columns.forEach(col => {
                let cellValue = row[col.field];
                
                if (col.render) {
                    cellValue = col.render(row);
                } else if (col.type === 'date') {
                    cellValue = utils.formatDate(cellValue);
                } else if (col.type === 'datetime') {
                    cellValue = utils.formatDateTime(cellValue);
                } else if (col.type === 'currency') {
                    cellValue = utils.formatCurrency(cellValue);
                }

                tableHTML += `<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">${cellValue || '-'}</td>`;
            });
            tableHTML += '</tr>';
        });

        tableHTML += `
                    </tbody>
                </table>
            </div>
        `;

        container.innerHTML = tableHTML;
    },

    showModal(title, content, onClose = null) {
        const existingModal = document.getElementById('dynamic-modal');
        if (existingModal) {
            existingModal.remove();
        }

        const modal = document.createElement('div');
        modal.id = 'dynamic-modal';
        modal.className = 'fixed inset-0 z-50 overflow-y-auto';
        modal.innerHTML = `
            <div class="flex items-center justify-center min-h-screen px-4">
                <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" onclick="components.hideModal()"></div>
                <div class="relative bg-white rounded-lg shadow-xl max-w-2xl w-full p-6">
                    <div class="flex justify-between items-center mb-4">
                        <h3 class="text-xl font-semibold text-gray-900">${title}</h3>
                        <button onclick="components.hideModal()" class="text-gray-400 hover:text-gray-600">
                            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                            </svg>
                        </button>
                    </div>
                    <div class="modal-content">
                        ${content}
                    </div>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        if (onClose) {
            modal.dataset.onClose = onClose.toString();
        }
    },

    hideModal() {
        const modal = document.getElementById('dynamic-modal');
        if (modal) {
            const onClose = modal.dataset.onClose;
            if (onClose) {
                eval(onClose)();
            }
            modal.remove();
        }
    },

    renderPagination(currentPage, totalPages, onPageChange) {
        const container = document.getElementById('pagination-container');
        if (!container) return;

        if (totalPages <= 1) {
            container.innerHTML = '';
            return;
        }

        let paginationHTML = `
            <div class="flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3 sm:px-6">
                <div class="flex flex-1 justify-between sm:hidden">
                    <button onclick="${onPageChange}(${currentPage - 1})" 
                            ${currentPage === 1 ? 'disabled' : ''}
                            class="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                        Previous
                    </button>
                    <button onclick="${onPageChange}(${currentPage + 1})" 
                            ${currentPage === totalPages ? 'disabled' : ''}
                            class="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                        Next
                    </button>
                </div>
                <div class="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
                    <div>
                        <p class="text-sm text-gray-700">
                            Page <span class="font-medium">${currentPage}</span> of <span class="font-medium">${totalPages}</span>
                        </p>
                    </div>
                    <div>
                        <nav class="isolate inline-flex -space-x-px rounded-md shadow-sm" aria-label="Pagination">
                            <button onclick="${onPageChange}(${currentPage - 1})" 
                                    ${currentPage === 1 ? 'disabled' : ''}
                                    class="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                <span class="sr-only">Previous</span>
                                <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z" clip-rule="evenodd" />
                                </svg>
                            </button>
        `;

        const maxVisible = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
        let endPage = Math.min(totalPages, startPage + maxVisible - 1);

        if (endPage - startPage < maxVisible - 1) {
            startPage = Math.max(1, endPage - maxVisible + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            const isActive = i === currentPage;
            paginationHTML += `
                <button onclick="${onPageChange}(${i})" 
                        class="relative inline-flex items-center px-4 py-2 text-sm font-semibold ${
                            isActive 
                            ? 'z-10 bg-blue-600 text-white focus:z-20 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600' 
                            : 'text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50'
                        }">
                    ${i}
                </button>
            `;
        }

        paginationHTML += `
                            <button onclick="${onPageChange}(${currentPage + 1})" 
                                    ${currentPage === totalPages ? 'disabled' : ''}
                                    class="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                <span class="sr-only">Next</span>
                                <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clip-rule="evenodd" />
                                </svg>
                            </button>
                        </nav>
                    </div>
                </div>
            </div>
        `;

        container.innerHTML = paginationHTML;
    },

    confirmDialog(title, message, onConfirm) {
        const content = `
            <div class="text-center">
                <p class="text-gray-700 mb-6">${message}</p>
                <div class="flex justify-center space-x-4">
                    <button onclick="components.hideModal()" 
                            class="px-4 py-2 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300">
                        Cancel
                    </button>
                    <button onclick="${onConfirm}; components.hideModal()" 
                            class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700">
                        Confirm
                    </button>
                </div>
            </div>
        `;
        this.showModal(title, content);
    }
};
