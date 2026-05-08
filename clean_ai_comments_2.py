import os
import re

patterns = [
    r'^\s*//\s*sondern unsere Request-Parameter \(10 Sekunden\) respektiert!.*$\n?',
    r'^\s*//\s*um dem ThreadPool Zeit zu geben, die fortgesetzten Tasks abzuarbeiten\..*$\n?',
    r'^\s*//\s*die Config1 für die ersten 3 Aufrufe zurückgeben\..*$\n?',
    r'^\s*//\s*1\. ZYKLUS-STEUERUNG & VERZÃ–GERUNGEN.*$\n?',
    r'^\s*//\s*2\. ALIVE CHECK \(BROWSER CRASH\).*$\n?',
    r'^\s*//\s*3\. BROWSER CLEANUP.*$\n?',
    r'^\s*//\s*4\. LAZY CONFIG RELOAD \(MID-CYCLE\).*$\n?',
    r'^\s*//\s*1\. ZYKLUS-STEUERUNG & VERZÖGERUNGEN.*$\n?'
]

def clean_file(filepath):
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
    except UnicodeDecodeError:
        with open(filepath, 'r', encoding='windows-1252') as f:
            content = f.read()
    
    new_content = content
    for pattern in patterns:
        new_content = re.sub(pattern, '', new_content, flags=re.MULTILINE)
    
    if new_content != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Cleaned {filepath}")

for root, dirs, files in os.walk('.'):
    if 'obj' in root or 'bin' in root or '.git' in root:
        continue
    for file in files:
        if file.endswith('.cs') or file.endswith('.xaml'):
            clean_file(os.path.join(root, file))
