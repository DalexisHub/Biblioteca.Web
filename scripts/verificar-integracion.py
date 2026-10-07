"""Prueba real contra Docker local. Crea registros identificados QA y los da de baja.
No modifica registros previos. Sin DELETE físico. Uso: python3 scripts/verificar-integracion.py --docker-local
"""
import sys, urllib.request, urllib.parse, urllib.error, http.cookiejar, html, re, uuid, subprocess, json
from pathlib import Path
if sys.argv[1:] != ['--docker-local']:
 raise SystemExit('Usa --docker-local para autorizar registros de prueba en la instancia local.')
root=Path(__file__).resolve().parent.parent
base='http://localhost:5081'
class SinRedirect(urllib.request.HTTPRedirectHandler):
 def redirect_request(self,*args,**kwargs): return None
client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()),SinRedirect())
def request(path,data=None):
 try:
  with client.open(base+path,None if data is None else urllib.parse.urlencode(data).encode()) as r: return r.status,html.unescape(r.read().decode()),r.headers
 except urllib.error.HTTPError as e: return e.code,html.unescape(e.read().decode()),e.headers
checks=0
def check(value,label):
 global checks
 assert value,label
 checks+=1;print('OK:',label,flush=True)
def token(path):
 status,page,_=request(path);assert status==200,(path,status)
 return re.search(r'name="__RequestVerificationToken" type="hidden" value="([^"]+)"',page).group(1)
def post(path,fields): return request(path,{'__RequestVerificationToken':token(path),**fields})
def sql(query):
 cmd=['docker','compose','exec','-T','sqlserver','sh','-c','SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -d BibliotecaDB -b -h -1 -W']
 result=subprocess.run(cmd,input='SET NOCOUNT ON;\n'+query+'\nGO\n',text=True,capture_output=True,cwd=root,check=True)
 return result.stdout.strip()
# Huella exacta del contenido previo, en orden de clave primaria; excluye solo nuestros futuros registros.
limits=[('Autores','AutorId',sql('SELECT MAX(AutorId) FROM dbo.Autores')),('Libros','LibroId',sql('SELECT MAX(LibroId) FROM dbo.Libros')),('Socios','SocioId',sql('SELECT MAX(SocioId) FROM dbo.Socios')),('Prestamos','PrestamoId',sql('SELECT MAX(PrestamoId) FROM dbo.Prestamos')),('DetallePrestamo','PrestamoId',sql('SELECT MAX(PrestamoId) FROM dbo.DetallePrestamo'))]
def huellas():
 return {t:sql(f"SELECT CONVERT(varchar(64), HASHBYTES('SHA2_256', (SELECT * FROM dbo.{t} WHERE {pk} <= {int(n)} ORDER BY {pk}"+(', LibroId' if t=='DetallePrestamo' else '')+" FOR JSON PATH, INCLUDE_NULL_VALUES)), 2)") for t,pk,n in limits}
before=huellas()
uid=uuid.uuid4().hex[:10];isbn='QA8-'+uid;dni='QA8'+uid
book_id=None
try:
 status,page,_=request('/Libros')
 check(status==200 and 'Cien años de soledad' in page and 'Gabriel García Márquez' in page,'Catálogo real con INNER JOIN de autores')
 status,page,_=request('/Libros?titulo=Rayuela')
 check(status==200 and 'Julio Cortázar' in page and 'Cien años de soledad' not in page,'Búsqueda real por título')
 status,page,_=request('/Libros?titulo=%25')
 check(status==200 and 'No encontramos ese título' in page,'Búsqueda literal de %')
 status,page,_=request('/Libros/Create')
 check(status==200 and page.count('<option value=')==9,'Desplegable con ocho autores activos')
 status,page,_=post('/Libros/Create',{'Titulo':'','ISBN':'Q','AutorId':1,'Ejemplares':-1})
 check(status==200 and 'Escribe el título.' in page and 'cero o más' in page,'Create inválido conserva formulario y autores')
 fields={'Titulo':'Prueba integración '+uid,'ISBN':isbn,'AutorId':1,'Ejemplares':2}
 status,page,headers=post('/Libros/Create',fields)
 check(status==302 and headers.get('Location')=='/','Inserción Dapper y redirección PRG')
 status,page,_=request(headers['Location'])
 check(status==200 and 'El libro ya forma parte del catálogo.' in page,'TempData después de guardar')
 book_id=int(sql(f"SELECT LibroId FROM dbo.Libros WHERE ISBN=N'{isbn}'"))
 status,page,_=request(f'/Libros/Details/{book_id}')
 check(status==200 and fields['Titulo'] in page and isbn in page,'Details del libro insertado')
 status,page,_=post('/Libros/Create',fields)
 check(status==200 and 'Ya existe un registro con ese ISBN' in page,'ISBN duplicado es validación, no excepción')
 fields.update({'LibroId':book_id,'Titulo':'Prueba editada '+uid,'AutorId':2,'Ejemplares':5})
 status,_,_=post(f'/Libros/Edit/{book_id}',fields)
 check(status==302,'Edit guarda y redirige')
 status,page,_=request(f'/Libros/Details/{book_id}')
 check('Prueba editada '+uid in page and 'Mario Vargas Llosa' in page and sql(f'SELECT Ejemplares FROM dbo.Libros WHERE LibroId={book_id}')=='5','Edición persistida en SQL Server')
 status,_,_=post(f'/Libros/Delete/{book_id}',{'id':book_id})
 check(status==302 and sql(f'SELECT Activo FROM dbo.Libros WHERE LibroId={book_id}')=='0','Baja lógica conserva fila con Activo=0')
 check(request(f'/Libros/Details/{book_id}')[0]==404,'Libro inactivo no se ofrece en Details')
 status,page,_=post('/Libros/Delete/1',{'id':1})
 check(status==200 and 'préstamos pendientes' in page and sql('SELECT Activo FROM dbo.Libros WHERE LibroId=1')=='1','Baja con préstamo pendiente rechazada sin cambios')
 status,page,_=request('/Socios')
 check(status==200 and 'Ana Torres' in page,'Listado real de socios')
 member={'Nombre':'Socio de prueba '+uid,'DNI':dni,'Email':'qa@example.invalid'}
 status,_,_=post('/Socios/Create',member)
 check(status==302 and sql(f"SELECT COUNT(*) FROM dbo.Socios WHERE DNI=N'{dni}' AND Activo=1")=='1','Alta de socio persistida')
 status,page,_=post('/Socios/Create',member)
 check(status==200 and 'Ya existe un registro con ese DNI' in page,'DNI duplicado real llega a ModelState')
 status,page,_=request('/Prestamos/Reporte?desde=2026-09-10&hasta=2026-09-10')
 check(status==200 and page.count('Préstamo #1')==3 and 'Ana Torres' in page and '24/09/2026' in page and 'Pendiente' in page,'Reporte del mismo día: tres libros, socio, límite y estado')
 status,page,_=request('/Prestamos/Reporte?desde=2026-08-01&hasta=2026-10-01')
 check(status==200 and page.count('Préstamo #')==8 and 'Devuelto' in page,'Reporte completo de los cinco préstamos y ocho detalles')
 status,page,_=request('/Prestamos/Reporte?desde=2000-01-01&hasta=2000-01-02')
 check(status==200 and 'Sin préstamos en estas fechas' in page,'Reporte sin resultados')
finally:
 # Solo filas nuevas de esta ejecución. Mantiene evidencia, sin borrado físico.
 sql(f"UPDATE dbo.Libros SET Activo=0 WHERE ISBN=N'{isbn}'; UPDATE dbo.Socios SET Activo=0 WHERE DNI=N'{dni}';")
 check(huellas()==before,'Todos los datos previos conservan la misma huella SHA-256')
print(f'{checks} comprobaciones de integración correctas. Registros QA nuevos conservados como inactivos.')
