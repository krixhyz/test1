import os
import re

old_src = 'd:\\Projects Y3\\C# 2nd Sem\\course-work-2-son-group-2\\frontend\\src_old'
new_src = 'd:\\Projects Y3\\C# 2nd Sem\\course-work-2-son-group-2\\frontend\\src'

import shutil
if os.path.exists(new_src):
    # clear new_src first? No, we want to keep main.jsx, index.css, etc.
    pass

def process_file(filepath, destpath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 1. Replace React imports
    content = re.sub(r'const\s+\{\s*([^}]+)\s*\}\s*=\s*React\s*;', r"import React, { \1 } from 'react';", content)
    
    # 2. Replace Object.assign(window, ...) with exports
    content = re.sub(r'Object\.assign\(\s*window\s*,\s*\{\s*([^}]+)\s*\}\s*\)\s*;?', r'export { \1 };', content)
    
    # 3. Replace window.api, window.auth, window.navigate with api, auth, navigate
    content = re.sub(r'window\.(auth|api|navigate|getPath|formatCurrency|formatDate|formatDateTime|DEMO)', r'\1', content)
    
    # Write back
    os.makedirs(os.path.dirname(destpath), exist_ok=True)
    with open(destpath, 'w', encoding='utf-8') as f:
        f.write(content)

# Process utils.js separately
utils_path = os.path.join(old_src, 'utils.js')
with open(utils_path, 'r', encoding='utf-8') as f:
    utils_content = f.read()

utils_content = re.sub(r'window\.auth\s*=\s*auth;', 'export { auth, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO, api };', utils_content)
utils_content = re.sub(r'window\.(navigate|getPath|formatCurrency|formatDate|formatDateTime|DEMO|api)\s*=\s*\w+;', '', utils_content)

os.makedirs(new_src, exist_ok=True)
with open(os.path.join(new_src, 'utils.js'), 'w', encoding='utf-8') as f:
    f.write(utils_content)

# Process components and pages
for root, dirs, files in os.walk(old_src):
    for file in files:
        if file.endswith('.jsx'):
            old_filepath = os.path.join(root, file)
            rel_path = os.path.relpath(old_filepath, old_src)
            new_filepath = os.path.join(new_src, rel_path)
            
            # Special case for app.jsx -> App.jsx
            if file == 'app.jsx':
                new_filepath = os.path.join(new_src, 'App.jsx')
                
            process_file(old_filepath, new_filepath)
