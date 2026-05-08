import os
import re

for root, dirs, files in os.walk('.'):
    if 'obj' in root or 'bin' in root or '.git' in root:
        continue
    for file in files:
        if file.endswith('.cs'):
            path = os.path.join(root, file)
            try:
                with open(path, 'r', encoding='utf-8') as f:
                    content = f.read()
            except UnicodeDecodeError:
                with open(path, 'r', encoding='windows-1252') as f:
                    content = f.read()
            
            new_content = content
            
            # Replace config.Computer.NextRestartDate = X
            new_content = re.sub(r'([a-zA-Z_0-9]+)\.Computer\.NextRestartDate\s*=\s*([^;]+);', r'\1.Computer.SetNextRestartDate(\2);', new_content)
            
            # Replace config.Computer.ComputerRestartIntervalDays = X
            # We need to use UpdateRestartSettings(interval, time, provider) but maybe we can just add setters to Computer
            
            # Replace config.Browser.DeleteBrowserCacheIntervalDays = X
            # Replace config.Browser.NextBrowserDeleteCacheDate = X
            
            # Replace with expressions
            new_content = re.sub(r'var\s+newConfig\s*=\s*currentConfig\s*with\s*\{\s*Settings\s*=\s*currentConfig\.Settings\s*with\s*\{\s*ApiUsername\s*=\s*string\.Empty,\s*ApiKey\s*=\s*string\.Empty\s*\}\s*\};', r'var newConfig = currentConfig;\nnewConfig.Settings.ApiUsername = string.Empty;\nnewConfig.Settings.ApiKey = string.Empty;', new_content)
            
            if new_content != content:
                with open(path, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f'Updated {path}')
