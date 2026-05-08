import os
import re

patterns = [
    r'^\s*//\s*(?:---|===).*$\n?',
    r'^\s*///\s*WARUM WIRD DAS GETESTET\?.*$\n?',
    r'^\s*//\s*NEU:.*$\n?',
    r'^\s*//\s*--- NEU:.*$\n?',
    r'^\s*//\s*HIER NEU:.*$\n?',
    r'^\s*//\s*FIX\b.*$\n?',
    r'^\s*//\s*--- FIX.*$\n?',
    r'^\s*//\s*--- PERFORMANCE FIX:.*$\n?',
    r'^\s*//\s*Da \'request\' ein Record ist.*$\n?',
    r'^\s*//\s*HILFSMETHODE:.*$\n?',
    r'^\s*//\s*1\. KONFIGURATION.*$\n?',
    r'^\s*//\s*2\. HINTERGRUND-SERVICES.*$\n?',
    r'^\s*//\s*3\. JETZT ERST DAS FENSTER.*$\n?',
    r'^\s*<!--\s*===.*-->.*$\n?',
    r'^\s*//\s*Da wir LoadConfig\(\) 3x pro Zyklus aufgerufen wird.*$\n?',
    r'^\s*//\s*erstellen und dabei nur die aktualisierten Werte überschreiben!.*$\n?',
    r'^\s*//\s*LAZY CONFIG RELOAD:.*$\n?',
    r'^\s*//\s*für diesen Zyklus direkt aus der frischen Config!.*$\n?',
    r'^\s*//\s*damit wir keine anderen Nutzer-Settings versehentlich überschreiben!.*$\n?',
    r'^\s*//\s*Die Berechnung des nächsten Neustart-Datums wandert aus dem Application-Service direkt in die `Computer`-Entität.*$\n?'
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
