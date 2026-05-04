const fs = require('fs');
const path = require('path');

const oldSrc = path.join(__dirname, 'src_old');
const newSrc = path.join(__dirname, 'src');

if (!fs.existsSync(newSrc)) {
    fs.mkdirSync(newSrc, { recursive: true });
}

function processFile(filePath, destPath) {
    let content = fs.readFileSync(filePath, 'utf-8');
    
    // 1. Replace React imports
    content = content.replace(/const\s+\{\s*([^}]+)\s*\}\s*=\s*React\s*;/g, "import React, { $1 } from 'react';");
    
    // 2. Replace Object.assign(window, ...) with exports
    content = content.replace(/Object\.assign\(\s*window\s*,\s*\{\s*([^}]+)\s*\}\s*\)\s*;?/g, 'export { $1 };');
    
    // 3. Replace window.* with direct variables
    content = content.replace(/window\.(auth|api|navigate|getPath|formatCurrency|formatDate|formatDateTime|DEMO)/g, '$1');
    
    // 4. In components/pages, we need to import these variables!
    // Since we don't know exactly which file uses which, we can just inject a generic import at the top of every file.
    // However, it's easier to just inject the import for utils.js if it uses them.
    const usesUtils = /(auth|api|navigate|getPath|formatCurrency|formatDate|formatDateTime|DEMO)/.test(content);
    if (usesUtils && !filePath.includes('utils.js')) {
        // Find how many levels deep we are to import correctly
        const relativeDir = path.dirname(path.relative(oldSrc, filePath));
        let up = '';
        if (relativeDir !== '.') {
            const depth = relativeDir.split(path.sep).length;
            up = '../'.repeat(depth);
        } else {
            up = './';
        }
        content = `import { auth, api, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO } from '${up}utils.js';\n` + content;
    }

    fs.mkdirSync(path.dirname(destPath), { recursive: true });
    fs.writeFileSync(destPath, content, 'utf-8');
}

// Process utils.js separately
const utilsPath = path.join(oldSrc, 'utils.js');
let utilsContent = fs.readFileSync(utilsPath, 'utf-8');
utilsContent = utilsContent.replace(/window\.auth\s*=\s*auth;/g, 'export { auth, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO, api };');
utilsContent = utilsContent.replace(/window\.(navigate|getPath|formatCurrency|formatDate|formatDateTime|DEMO|api)\s*=\s*\w+;/g, '');
fs.writeFileSync(path.join(newSrc, 'utils.js'), utilsContent, 'utf-8');

// Walk directory
function walk(dir) {
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            walk(fullPath);
        } else if (fullPath.endsWith('.jsx')) {
            const relPath = path.relative(oldSrc, fullPath);
            let destPath = path.join(newSrc, relPath);
            if (file === 'app.jsx') {
                destPath = path.join(newSrc, 'App.jsx');
            }
            processFile(fullPath, destPath);
        }
    }
}

walk(oldSrc);
