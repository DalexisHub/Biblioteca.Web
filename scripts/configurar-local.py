"""Conecta esta aplicación al SQL Server de compose, sin imprimir la contraseña."""
from pathlib import Path
import json
root=Path(__file__).resolve().parent.parent
settings=root/'appsettings.Local.json'
if settings.exists():
 raise SystemExit('appsettings.Local.json ya existe. Se conserva sin cambios.')
values=dict(line.split('=',1) for line in (root/'.env').read_text().splitlines() if line and not line.startswith('#'))
password=values['MSSQL_SA_PASSWORD'].replace('"','""')
connection=f'Server=127.0.0.1,14333;Database=BibliotecaDB;User ID=sa;Password="{password}";Encrypt=True;TrustServerCertificate=True;Connect Timeout=5;'
settings.write_text(json.dumps({'ConnectionStrings':{'BibliotecaDB':connection}},indent=2)+'\n')
settings.chmod(0o600)
print('Configuración local guardada. No se creó ni modificó ninguna base de datos.')
