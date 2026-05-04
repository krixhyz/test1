const fs = require('fs');
const path = require('path');

const srcDir = path.join(__dirname, 'src');

const exportsMap = {
    'common.jsx': ['Icon', 'Button', 'Input', 'Select', 'Textarea', 'Badge', 'StatusBadge', 'stockStatus', 'Card', 'DashboardCard', 'Loader', 'EmptyState', 'PageHeader', 'SearchBar', 'Alert', 'Modal', 'ConfirmDialog', 'Table', 'FormRow', 'StarRating', 'useToast', 'Eyebrow'],
    'layout.jsx': ['Sidebar', 'Topbar', 'ProtectedRoute', 'PublicLayout', 'AdminLayout', 'StaffLayout', 'CustomerLayout'],
    'public.jsx': ['Login', 'Register', 'Home'],
    'admin.jsx': ['AdminDashboard', 'StaffManagement', 'VendorManagement', 'PartsManagement', 'PurchaseInvoices', 'FinancialReports', 'LowStockAlerts', 'AdminNotifications'],
    'staff.jsx': ['StaffDashboard', 'CustomerRegistration', 'CustomerSearch', 'PartsSale', 'SalesInvoices', 'CustomerDetails', 'CustomerReports', 'CreditReminders'],
    'customer.jsx': ['CustomerDashboard', 'CustomerProfile', 'MyVehicles', 'BookAppointment', 'RequestUnavailablePart', 'MyHistory', 'SubmitReview', 'PartFailurePrediction']
};

const modulePaths = {
    'common.jsx': 'components/common.jsx',
    'layout.jsx': 'components/layout.jsx',
    'public.jsx': 'pages/public.jsx',
    'admin.jsx': 'pages/admin.jsx',
    'staff.jsx': 'pages/staff.jsx',
    'customer.jsx': 'pages/customer.jsx'
};

function injectImports(filePath) {
    let content = fs.readFileSync(filePath, 'utf-8');
    
    // Find what it uses
    for (const [moduleName, exports] of Object.entries(exportsMap)) {
        if (filePath.endsWith(moduleName)) continue; // Don't import from self
        
        const usedExports = exports.filter(e => {
            const regex = new RegExp(`\\b${e}\\b`);
            return regex.test(content);
        });
        
        if (usedExports.length > 0) {
            const relativeDir = path.dirname(path.relative(srcDir, filePath));
            let up = '';
            if (relativeDir !== '.') {
                const depth = relativeDir.split(path.sep).length;
                up = '../'.repeat(depth);
            } else {
                up = './';
            }
            const importPath = `${up}${modulePaths[moduleName]}`.replace(/\\/g, '/');
            content = `import { ${usedExports.join(', ')} } from '${importPath}';\n` + content;
        }
    }
    fs.writeFileSync(filePath, content, 'utf-8');
}

function walk(dir) {
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            walk(fullPath);
        } else if (fullPath.endsWith('.jsx')) {
            injectImports(fullPath);
        }
    }
}

walk(srcDir);
