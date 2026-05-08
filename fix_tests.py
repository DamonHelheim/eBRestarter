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
            
            # Fix EVisitorApiAdapterTests
            new_content = re.sub(r'var configToSave = new AppConfig\(\);\s*configToSave = configToSave with\s*\{\s*Settings = configToSave\.Settings with \{ ApiUsername = "TestUser", ApiKey = "TestKey" \}\s*\};', r'var configToSave = new AppConfig();\n            configToSave.Settings.ApiUsername = "TestUser";\n            configToSave.Settings.ApiKey = "TestKey";', new_content)
            
            new_content = re.sub(r'var configToSave = new AppConfig\(\);\s*configToSave = configToSave with\s*\{\s*Settings = configToSave\.Settings with \{ ApiUsername = "", ApiKey = "" \}\s*\};', r'var configToSave = new AppConfig();\n            configToSave.Settings.ApiUsername = "";\n            configToSave.Settings.ApiKey = "";', new_content)
            
            # Fix ManageRestarterCycleServiceTests
            new_content = re.sub(r'var dummyConfig = new AppConfig \{ Browser = new BrowserConfig \{ DeleteBrowserCacheIntervalDays = 7, NextBrowserDeleteCacheDate = DateTime\.MinValue \} \};', r'var dummyConfig = new AppConfig { Browser = new BrowserConfig() };\n            dummyConfig.Browser.UpdateCleanupSettings(7, _fakeTimeProvider);\n            dummyConfig.Browser.SetNextCleanupDate(DateTime.MinValue);', new_content)
            
            # Fix ApplicationLaunchConfigServiceTests
            new_content = re.sub(r'var browser = new BrowserConfig\s*\{\s*DeleteBrowserCacheIntervalDays = 0,\s*NextBrowserDeleteCacheDate = DateTime\.MinValue\s*\};', r'var browser = new BrowserConfig();\n        browser.UpdateCleanupSettings(0, TimeProvider.System);\n        browser.SetNextCleanupDate(DateTime.MinValue);', new_content)
            
            if new_content != content:
                with open(path, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f'Updated {path}')
