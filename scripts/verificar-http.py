"""Pruebas HTTP que no escriben datos. Ejecutar con la app iniciada."""
import sys, urllib.request, urllib.parse, urllib.error, http.cookiejar, re, html
base = sys.argv[1] if len(sys.argv) > 1 else 'http://localhost:5081'
client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
def request(path, data=None):
 try:
  with client.open(base+path, None if data is None else urllib.parse.urlencode(data).encode()) as r: return r.status, html.unescape(r.read().decode())
 except urllib.error.HTTPError as e: return e.code, html.unescape(e.read().decode())
def check(value, label):
 assert value, label
 print('OK:',label)
status, page=request('/Socios/Create')
check(status==200 and 'Nombre completo' in page, 'Formulario de socio y Razor')
token=re.search(r'name="__RequestVerificationToken" type="hidden" value="([^"]+)"',page).group(1)
status,page=request('/Socios/Create',{'__RequestVerificationToken':token,'Nombre':'','DNI':'','Email':'no-es-correo'})
check(status==200 and 'Escribe el DNI.' in page and 'Introduce un correo válido.' in page, 'Validación del servidor sin acceder a la base')
check(request('/Socios/Create',{'Nombre':'Prueba'})[0]==400, 'POST sin antiforgery rechazado')
for action in ['Create','Edit/1','Delete/1']:
 check(request('/Libros/'+action,{'Titulo':'Prueba'})[0]==400, 'Antiforgery en Libros/'+action)
status,page=request('/Prestamos/Reporte')
check(status==200 and 'Cada préstamo tiene una historia' in page, 'Reporte inicial sin consulta')
status,page=request('/Prestamos/Reporte?desde=2026-10-02&hasta=2026-10-01')
check(status==200 and 'posterior a la inicial' in page and "value='2026-10-02'" in page, 'Rango inválido y filtros conservados')
check(request('/Prestamos/Reporte?desde=incorrecto&hasta=2026-10-01')[0]==200, 'Fecha mal formada no produce excepción')
check(request('/css/site.css')[0]==200, 'CSS local disponible')
check(request('/lib/jquery-validation/dist/jquery.validate.min.js')[0]==200, 'Validación cliente local disponible')
status,page=request('/Libros?titulo='+('x'*201))
check(status==200 and 'hasta 200 caracteres' in page, 'Búsqueda demasiado larga sin consulta SQL')
print('12 comprobaciones HTTP correctas. No se escribieron datos.')
